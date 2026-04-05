using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using System.Threading.Channels;

namespace MarketDataAggregator.Infrastructure.Sources
{
    public class MockWebSocketSource: IMarketDataSource
    {
        private readonly string _sourceName;
        private readonly int _tickDelayMs;
        private readonly Random _random = new();

        public MockWebSocketSource(string sourceName, int tickDelayMs = 50)
        {
            _sourceName = sourceName;
            _tickDelayMs = tickDelayMs;
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
                await Task.Delay(_tickDelayMs, ct);
            }
        }
    }
}
