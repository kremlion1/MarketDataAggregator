using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;

namespace MarketDataAggregator.Infrastructure.Storage
{
    public class ConsoleTickStorage: ITickStorage
    {
        private int _count;

        public Task SaveAsync(MarketTick tick, CancellationToken ct)
        {
            _count++;

            Console.WriteLine(
                $"{_count}: {tick.Source} {tick.Ticker} {tick.Price} {tick.Timestamp:HH:mm:ss.fff}");

            return Task.CompletedTask;
        }
    }
}
