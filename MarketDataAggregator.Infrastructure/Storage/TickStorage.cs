using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using MarketDataAggregator.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Serilog;

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
            try
            {
                await _db.Ticks.AddAsync(tick, ct);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving tick {Ticker} to database", tick.Ticker);
                throw;
            }
        }

        public async Task SaveBatchAsync(IEnumerable<MarketTick> ticks, CancellationToken ct)
        {

            var tickList = ticks.ToList();
            if (tickList.Count == 0)
                return;

            try
            {
                await _db.Ticks.AddRangeAsync(tickList, ct);
                await _db.SaveChangesAsync(ct);
                Log.Information("Successfully saved batch of {TickCount} ticks", tickList.Count);
            }
            catch (DbUpdateException ex)
            {
                Log.Error(ex, "Database update error while saving batch of {TickCount} ticks", tickList.Count);
                throw;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving batch of {TickCount} ticks to database", tickList.Count);
                throw;
            }
        }
    }
}
