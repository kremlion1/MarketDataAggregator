using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MarketDataAggregator.ConsoleApp.Configuration
{
    public class ServiceConfiguration
    {
        public static ServiceCollection ConfigureServices(IConfiguration config)
        {
            var services = new ServiceCollection();
            services.AddSingleton(config);
            
            MarketDataAggregator.Infrastructure.RegisterDependency
                .RegisterInfrastructureDependencies(services, config);

            return services;
        }
    }
}

