using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Application.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace MarketDataAggregator.ConsoleApp.Services
{
    public class ApplicationRunner
    {
        private readonly PipelineProcessor _pipeline;
        private readonly IEnumerable<IMarketDataSource> _sources;

        public ApplicationRunner(IServiceProvider provider)
        {
            _pipeline = provider.GetRequiredService<PipelineProcessor>();
            _sources = provider.GetServices<IMarketDataSource>();
        }

        public async Task RunAsync(CancellationToken ct)
        {
            Console.WriteLine($"\nStarted {_sources.Count()} data sources\n");

            var tasks = new List<Task>
            {
                Task.Run(() => _pipeline.StartAsync(ct))
            };

            foreach (var source in _sources)
            {
                tasks.Add(Task.Run(() => source.StartAsync(_pipeline.Writer, ct)));
            }

            await Task.WhenAll(tasks);
        }
    }
}

