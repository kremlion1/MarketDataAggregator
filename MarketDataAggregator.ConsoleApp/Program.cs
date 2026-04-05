using MarketDataAggregator.ConsoleApp.Configuration;
using MarketDataAggregator.ConsoleApp.Infrastructure;
using MarketDataAggregator.ConsoleApp.Services;
using MarketDataAggregator.Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace MarketDataAggregator.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

            try
            {
                var config = ConfigurationBuilder.Build();

                var services = ServiceConfiguration.ConfigureServices(config);
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

                var runner = new ApplicationRunner(provider);
                await runner.RunAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Application stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Application error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}
