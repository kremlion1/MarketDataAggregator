using MarketDataAggregator.Application.Pipeline;
using MarketDataAggregator.Infrastructure.Normalization;
using MarketDataAggregator.Infrastructure.Sources;
using MarketDataAggregator.Infrastructure.Storage;

namespace MarketDataAggregator.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            var normalizer = new SimpleNormalizer();
            var storage = new ConsoleTickStorage();
            var pipeline = new PipelineProcessor(normalizer, storage);

            var sources = new List<MockWebSocketSource>
            {
                new("Binance"),
                new("Coinbase")
            };

            var tasks = new List<Task>
            {
                Task.Run(() => pipeline.StartAsync(cts.Token))
            };

            foreach (var source in sources)
            {
                tasks.Add(Task.Run(() => source.StartAsync(pipeline.Writer, cts.Token)));
            }

            Console.WriteLine("Started...");

            await Task.WhenAll(tasks);
        }
    }
}
