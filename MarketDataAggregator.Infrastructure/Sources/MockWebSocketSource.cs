using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using System.Threading.Channels;

namespace MarketDataAggregator.Infrastructure.Sources
{
    public class MockWebSocketSource: IMarketDataSource
    {
        private readonly string _sourceName;
        private readonly Random _random = new();

        public MockWebSocketSource(string sourceName)
        {
            _sourceName = sourceName;
        }

        public async Task StartAsync(ChannelWriter<MarketTick> writer, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var tick = new MarketTick
                {
                    Ticker = "BTCUSDT",
                    Price = _random.Next(20000, 30000),
                    Volume = _random.Next(1, 10),
                    Timestamp = DateTime.UtcNow,
                    Source = _sourceName
                };

                await writer.WriteAsync(tick, ct);
                await Task.Delay(50, ct); // ~20 тиков/сек
            }
        }
    }
}
