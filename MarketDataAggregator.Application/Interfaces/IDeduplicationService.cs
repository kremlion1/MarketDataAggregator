using MarketDataAggregator.Domain.Models;

namespace MarketDataAggregator.Application.Interfaces
{
    public interface IDeduplicationService
    {
        Task<bool> IsTickExistAsync(MarketTick tick, CancellationToken ct);
        Task<IEnumerable<MarketTick>> FilterDuplicatesAsync(IEnumerable<MarketTick> ticks, CancellationToken ct);
    }
}

