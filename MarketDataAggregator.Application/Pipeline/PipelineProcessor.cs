using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using System.Threading.Channels;

namespace MarketDataAggregator.Application.Pipeline
{
    public class PipelineProcessor
    {
        private readonly INormalizer _normalizer;
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
            INormalizer normalizer, 
            ITickStorage storage, 
            IDeduplicationService deduplicationService,
            IMetricsService metrics)
        {
            _normalizer = normalizer;
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

            Console.WriteLine("Pipeline started");

            try
            {
                await foreach (var tick in _channel.Reader.ReadAllAsync(ct))
                {
                    batch.Add(tick);
                    Console.WriteLine($"Received tick: {tick.Ticker} from {tick.Source} at {tick.Timestamp:HH:mm:ss.fff}");

                    if (batch.Count >= BatchSize)
                    {
                        Console.WriteLine($"Processing batch of {batch.Count} ticks");
                        await ProcessBatchAsync(batch, ct);
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
                    Console.WriteLine($"Processing final batch of {batch.Count} ticks");
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
                Console.WriteLine($"Starting deduplication for batch of {batch.Count} ticks");
                
                var dedupedInMemory = batch
                    .GroupBy(x => new { x.Ticker, x.Timestamp, x.Source })
                    .Select(g => g.First())
                    .ToList();
                
                Console.WriteLine($"In-memory dedup: {batch.Count} -> {dedupedInMemory.Count} ticks");
                
                var uniqueTicks = await _deduplicationService.FilterDuplicatesAsync(dedupedInMemory, ct);
                var uniqueList = uniqueTicks.ToList();
                Console.WriteLine($"Deduplication completed, {uniqueList.Count} unique ticks remain");

                var duplicateCount = batch.Count - uniqueList.Count;
                if (duplicateCount > 0)
                {
                    Console.WriteLine($"Removed {duplicateCount} duplicates, saving {uniqueList.Count} ticks");
                    _metrics.IncrementDuplicatesRemoved(duplicateCount);
                }

                if (uniqueList.Any())
                {
                    Console.WriteLine($"Saving {uniqueList.Count} ticks to database");
                    await _storage.SaveBatchAsync(uniqueList, ct);
                    Console.WriteLine($"Successfully saved {uniqueList.Count} ticks");
                    _metrics.IncrementTicksProcessed(uniqueList.Count);
                }
                else
                {
                    Console.WriteLine("No unique ticks to save");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing batch: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                _metrics.IncrementSaveErrors();
            }
        }
    }
}

