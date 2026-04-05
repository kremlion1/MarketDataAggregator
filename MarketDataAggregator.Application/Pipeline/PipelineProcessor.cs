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
            using (var metricsReporter = new MetricsReporter(_metrics))
            {
                metricsReporter.Start();
                Log.Information("Pipeline started");

                var batchManager = new BatchManager(BatchSize, ProcessBatchAsync);

                try
                {
                    await foreach (var tick in _channel.Reader.ReadAllAsync(ct))
                    {
                        batchManager.Add(tick);
                        await batchManager.ProcessIfReadyAsync(ct);
                    }

                    await batchManager.ProcessRemainingAsync(ct);
                }
                finally
                {
                    metricsReporter.Stop();
                    _metrics.PrintMetrics();
                }
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
                        Log.Warning("Batch save operation cancelled");
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

