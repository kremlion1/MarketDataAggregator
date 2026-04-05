using MarketDataAggregator.Infrastructure.Context;
using Serilog;

namespace MarketDataAggregator.ConsoleApp.Infrastructure
{
    public class DatabaseInitializer
    {
        private readonly MarketDataDbContext _dbContext;

        public DatabaseInitializer(MarketDataDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task InitializeAsync()
        {
            try
            {
                await _dbContext.Database.EnsureCreatedAsync();
                Log.Information("Database initialized successfully");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Error initializing database");
                throw;
            }
        }
    }
}

