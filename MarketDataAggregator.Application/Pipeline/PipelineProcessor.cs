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

        private const int BatchSize = 100;

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

            try
            {
                await foreach (var tick in _channel.Reader.ReadAllAsync(ct))
                {
                    batch.Add(tick);

                    if (batch.Count >= BatchSize)
                    {
                        await ProcessBatchAsync(batch, ct);
                        batch.Clear();
                    }
                }

                if (batch.Count > 0)
                {
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
                var uniqueTicks = await _deduplicationService.FilterDuplicatesAsync(batch, ct);
                var uniqueList = uniqueTicks.ToList();

                var duplicateCount = batch.Count - uniqueList.Count;
                if (duplicateCount > 0)
                {
                    _metrics.IncrementDuplicatesRemoved(duplicateCount);
                }

                if (uniqueList.Any())
                {
                    await _storage.SaveBatchAsync(uniqueList, ct);
                    _metrics.IncrementTicksProcessed(uniqueList.Count);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error processing batch: {ex.Message}");
                _metrics.IncrementSaveErrors();
            }
        }
    }
}

