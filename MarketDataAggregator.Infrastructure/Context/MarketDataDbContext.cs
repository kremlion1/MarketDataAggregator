using MarketDataAggregator.Domain.Models;
using MarketDataAggregator.Infrastructure.Tables;
using Microsoft.EntityFrameworkCore;

namespace MarketDataAggregator.Infrastructure.Context
{
    public class MarketDataDbContext: DbContext
    {
        public DbSet<MarketTick> Ticks => Set<MarketTick>();

        public MarketDataDbContext(DbContextOptions<MarketDataDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketTickConfiguration).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
