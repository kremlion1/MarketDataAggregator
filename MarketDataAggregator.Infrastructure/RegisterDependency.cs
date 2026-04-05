using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Application.Pipeline;
using MarketDataAggregator.Infrastructure.Context;
using MarketDataAggregator.Infrastructure.Deduplication;
using MarketDataAggregator.Infrastructure.Monitoring;
using MarketDataAggregator.Infrastructure.Normalization;
using MarketDataAggregator.Infrastructure.Sources;
using MarketDataAggregator.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarketDataAggregator.Infrastructure
{
    public static class RegisterDependency
    {
        public static IServiceCollection RegisterInfrastructureDependencies(this IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? config["Database:ConnectionString"];

            services.AddDbContext<MarketDataDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null);
                }));

            services.AddSingleton<INormalizer, SimpleNormalizer>();
            services.AddScoped<ITickStorage, TickStorage>();
            services.AddScoped<IDeduplicationService, DeduplicationService>();
            services.AddSingleton<IMetricsService, MetricsService>();
            services.AddSingleton<PipelineProcessor>();

            var sources = config.GetSection("MarketDataSources")
                .Get<MarketDataSourceConfig[]>() ?? Array.Empty<MarketDataSourceConfig>();
            foreach (var source in sources)
            {
                services.AddSingleton<IMarketDataSource>(new MockWebSocketSource(source.Name, source.TickDelayMs));
            }
            
            return services;
        }
    }

    public class MarketDataSourceConfig
    {
        public string Name { get; set; } = string.Empty;
        public int TickDelayMs { get; set; } = 50;
    }
}

