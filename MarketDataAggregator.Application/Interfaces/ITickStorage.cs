using MarketDataAggregator.Domain.Models;

namespace MarketDataAggregator.Application.Interfaces
{
    public interface ITickStorage
    {
        Task SaveAsync(MarketTick tick, CancellationToken ct);
        Task SaveBatchAsync(IEnumerable<MarketTick> ticks, CancellationToken ct);
    }
}
