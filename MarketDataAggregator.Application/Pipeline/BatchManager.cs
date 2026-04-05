using MarketDataAggregator.Domain.Models;

namespace MarketDataAggregator.Application.Pipeline
{
    public class BatchManager
    {
        private readonly List<MarketTick> _batch;
        private readonly int _batchSize;
        private readonly Func<List<MarketTick>, CancellationToken, Task> _onBatchReady;

        public BatchManager(int batchSize, Func<List<MarketTick>, CancellationToken, Task> onBatchReady)
        {
            _batchSize = batchSize;
            _onBatchReady = onBatchReady;
            _batch = new List<MarketTick>(batchSize);
        }

        public void Add(MarketTick tick)
        {
            _batch.Add(tick);
        }

        public async Task ProcessIfReadyAsync(CancellationToken ct)
        {
            if (_batch.Count >= _batchSize)
            {
                await ProcessBatchAsync(ct);
            }
        }

        public async Task ProcessRemainingAsync(CancellationToken ct)
        {
            if (_batch.Count > 0)
            {
                await ProcessBatchAsync(ct);
            }
        }

        private async Task ProcessBatchAsync(CancellationToken ct)
        {
            await _onBatchReady(_batch, ct);
            _batch.Clear();
        }

        public int Count => _batch.Count;
    }
}

