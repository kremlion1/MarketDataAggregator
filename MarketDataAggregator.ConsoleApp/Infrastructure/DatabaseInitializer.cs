using MarketDataAggregator.Infrastructure.Context;

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
                Console.WriteLine("Database initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw;
            }
        }
    }
}

