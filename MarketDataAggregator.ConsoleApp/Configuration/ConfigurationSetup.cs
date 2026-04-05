using Microsoft.Extensions.Configuration;

namespace MarketDataAggregator.ConsoleApp.Configuration
{
    public class ConfigurationBuilder
    {
        public static IConfiguration Build()
        {
            return new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}

