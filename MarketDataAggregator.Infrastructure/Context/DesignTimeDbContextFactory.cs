using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MarketDataAggregator.Infrastructure.Context
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MarketDataDbContext>
    {
        public MarketDataDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MarketDataDbContext>();
            
            var connectionString = "Server=localhost;Database=MarketData;Trusted_Connection=True;TrustServerCertificate=True";
            
            optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });

            return new MarketDataDbContext(optionsBuilder.Options);
        }
    }
}

