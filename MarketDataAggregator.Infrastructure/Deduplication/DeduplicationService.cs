using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using MarketDataAggregator.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace MarketDataAggregator.Infrastructure.Deduplication
{
    public class DeduplicationService : IDeduplicationService
    {
        private readonly MarketDataDbContext _db;

        public DeduplicationService(MarketDataDbContext db)
        {
            _db = db;
        }

        public async Task<bool> IsTickExistAsync(MarketTick tick, CancellationToken ct)
        {
            return await _db.Ticks
                .AsNoTracking()
                .AnyAsync(x => x.Ticker == tick.Ticker && 
                              x.Timestamp == tick.Timestamp && 
                              x.Source == tick.Source, ct);
        }

        public async Task<IEnumerable<MarketTick>> FilterDuplicatesAsync(IEnumerable<MarketTick> ticks, CancellationToken ct)
        {
            var tickList = ticks.ToList();
            if (!tickList.Any())
                return tickList;

            var existingKeys = await _db.Ticks
                .AsNoTracking()
                .Where(x => tickList.Select(t => t.Ticker).Contains(x.Ticker))
                .Select(x => new TickKey { Ticker = x.Ticker, Timestamp = x.Timestamp, Source = x.Source })
                .ToListAsync(ct);

            var existingSet = existingKeys.ToHashSet(new TickKeyComparer());

            return tickList.Where(t => !existingSet.Contains(new TickKey 
                { 
                    Ticker = t.Ticker, 
                    Timestamp = t.Timestamp, 
                    Source = t.Source 
                }))
                          .ToList();
        }
    }

    internal class TickKey
    {
        public string Ticker { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    internal class TickKeyComparer : IEqualityComparer<TickKey>
    {
        public bool Equals(TickKey? x, TickKey? y)
        {
            if (x == null || y == null) return x == y;
            return x.Ticker == y.Ticker && x.Timestamp == y.Timestamp && x.Source == y.Source;
        }

        public int GetHashCode(TickKey obj)
        {
            return HashCode.Combine(obj.Ticker, obj.Timestamp, obj.Source);
        }
    }
}


