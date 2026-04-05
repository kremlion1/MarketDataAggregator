using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using MarketDataAggregator.Infrastructure.Context;

namespace MarketDataAggregator.Infrastructure.Storage
{
    public class TickStorage : ITickStorage
    {
        private readonly MarketDataDbContext _db;

        public TickStorage(MarketDataDbContext db)
        {
            _db = db;
        }

        public async Task SaveAsync(MarketTick tick, CancellationToken ct)
        {
            await _db.Ticks.AddAsync(tick, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task SaveBatchAsync(IEnumerable<MarketTick> ticks, CancellationToken ct)
        {
            await _db.Ticks.AddRangeAsync(ticks, ct);
            await _db.SaveChangesAsync(ct);
        }
    }
}
