using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using System.Threading.Channels;
using Serilog;

namespace MarketDataAggregator.Application.Pipeline
{
    public class PipelineProcessor
    {
        private readonly ITickStorage _storage;
        private readonly IDeduplicationService _deduplicationService;
        private readonly IMetricsService _metrics;

        private readonly Channel<MarketTick> _channel = Channel.CreateBounded<MarketTick>(
            new BoundedChannelOptions(1000) 
            { 
                FullMode = BoundedChannelFullMode.DropNewest 
            });

        private const int BatchSize = 10;

        public ChannelWriter<MarketTick> Writer => _channel.Writer;

        public PipelineProcessor(
            ITickStorage storage, 
            IDeduplicationService deduplicationService,
            IMetricsService metrics)
        {
            _storage = storage;
            _deduplicationService = deduplicationService;
            _metrics = metrics;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            var batch = new List<MarketTick>(BatchSize);
            var statsTimer = new System.Timers.Timer(5000);
            statsTimer.Elapsed += (s, e) => _metrics.PrintMetrics();
            statsTimer.Start();

            Log.Information("Pipeline started");

            try
            {
                await foreach (var tick in _channel.Reader.ReadAllAsync(ct))
                {
                    batch.Add(tick);
                    Log.Debug("Received tick: {Ticker} from {Source} at {Timestamp}",
                        tick.Ticker, tick.Source, tick.Timestamp.ToString("HH:mm:ss.fff"));

                    if (batch.Count >= BatchSize)
                    {
                        Log.Information("Processing batch of {BatchSize} ticks", batch.Count);
                        await ProcessBatchAsync(batch, ct);
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    Log.Information("Processing final batch of {BatchSize} ticks", batch.Count);
                    await ProcessBatchAsync(batch, ct);
                }
            }
            finally
            {
                statsTimer.Stop();
                statsTimer.Dispose();
                _metrics.PrintMetrics();
            }
        }

        private async Task ProcessBatchAsync(List<MarketTick> batch, CancellationToken ct)
        {
            try
            {
                var dedupedInMemory = batch
                    .GroupBy(x => new { x.Ticker, x.Timestamp, x.Source })
                    .Select(g => g.First())
                    .ToList();
                
                if (batch.Count > dedupedInMemory.Count)
                {
                    var duplicateCount = batch.Count - dedupedInMemory.Count;
                    Log.Information("In-memory dedup: {Original} -> {Unique} ticks",
                        batch.Count, dedupedInMemory.Count);
                    _metrics.IncrementDuplicatesRemoved(duplicateCount);
                }

                IEnumerable<MarketTick> uniqueTicks;
                try
                {
                    uniqueTicks = await _deduplicationService.FilterDuplicatesAsync(dedupedInMemory, ct);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Deduplication service error, skipping batch");
                    _metrics.IncrementSaveErrors();
                    return;
                }

                var uniqueList = uniqueTicks.ToList();
                if (uniqueList.Count > 0)
                {
                    try
                    {
                        await _storage.SaveBatchAsync(uniqueList, ct);
                        Log.Information("Successfully saved {TickCount} ticks", uniqueList.Count);
                        _metrics.IncrementTicksProcessed(uniqueList.Count);
                    }
                    catch (OperationCanceledException)
                    {
                        Log.Warning("Batch save operation cancelled for {TickCount} ticks", uniqueList.Count);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to save batch of {TickCount} ticks", uniqueList.Count);
                        _metrics.IncrementSaveErrors();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected error in ProcessBatchAsync");
                _metrics.IncrementSaveErrors();
            }
        }
    }
}

