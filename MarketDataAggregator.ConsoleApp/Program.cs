using MarketDataAggregator.ConsoleApp.Configuration;
using MarketDataAggregator.ConsoleApp.Infrastructure;
using MarketDataAggregator.ConsoleApp.Logging;
using MarketDataAggregator.ConsoleApp.Services;
using MarketDataAggregator.Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Serilog;

namespace MarketDataAggregator.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            var config = ConfigurationBuilder.Build();
            var logger = LoggerSetup.CreateLogger(config);

            try
            {
                Log.Logger = logger;
                logger.Information("Application starting");

                var services = ServiceConfiguration.ConfigureServices(config);
                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));
                var provider = services.BuildServiceProvider();

                using (var scope = provider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<MarketDataDbContext>();
                    var initializer = new DatabaseInitializer(dbContext);
                    await initializer.InitializeAsync();
                }

                var cts = new CancellationTokenSource();
                var shutdownHandler = new ShutdownHandler(cts);
                shutdownHandler.RegisterShutdownHandler();

                logger.Information("Starting {SourceCount} data sources", 
                    config.GetSection("MarketDataSources").GetChildren().Count());

                var runner = new ApplicationRunner(provider);
                await runner.RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                logger.Information("Application stopped");
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Application error");
                Environment.Exit(1);
            }
            finally
            {
                logger.Information("Application shutdown");
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
