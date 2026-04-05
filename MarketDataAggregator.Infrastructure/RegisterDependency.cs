﻿using MarketDataAggregator.Application.Interfaces;
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
            services.AddSingleton<ITickStorage, TickStorage>();
            services.AddSingleton<IDeduplicationService, DeduplicationService>();
            services.AddSingleton<IMetricsService, MetricsService>();
            services.AddSingleton<PipelineProcessor>();

            var binanceNormalizer = new BinanceNormalizer();
            var coinbaseNormalizer = new CoinbaseNormalizer();

            var sources = config.GetSection("MarketDataSources")
                .Get<MarketDataSourceConfig[]>() ?? Array.Empty<MarketDataSourceConfig>();
            
            foreach (var source in sources)
            {
                IMarketDataSource dataSource = source.Type?.ToLower() switch
                {
                    "binance" => new BinanceWebSocketSource(
                        binanceNormalizer,
                        source.WebSocketUrl ?? "wss://stream.binance.com:9443/ws/",
                        source.Symbols ?? new[] { "btcusdt" }),
                    
                    "coinbase" => new CoinbaseWebSocketSource(
                        coinbaseNormalizer,
                        source.WebSocketUrl ?? "wss://ws-feed.exchange.coinbase.com",
                        source.Products ?? new[] { "BTC-USD" }),
                    
                    _ => throw new InvalidOperationException($"Unknown source type: {source.Type}")
                };

                services.AddSingleton(dataSource);
                services.AddSingleton<IMarketDataSource>(dataSource);
            }
            
            return services;
        }
    }

    public class MarketDataSourceConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string WebSocketUrl { get; set; } = string.Empty;
        public string[]? Symbols { get; set; }
        public string[]? Products { get; set; }
    }
}

