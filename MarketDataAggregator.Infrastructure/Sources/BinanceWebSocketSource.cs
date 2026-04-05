using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Infrastructure.Sources.RawData;
using System.Net.WebSockets;
using System.Text.Json;

namespace MarketDataAggregator.Infrastructure.Sources
{
    public class BinanceWebSocketSource : WebSocketDataSourceBase
    {
        private readonly string[] _symbols;

        public BinanceWebSocketSource(INormalizer normalizer, string webSocketUrl, string[] symbols)
            : base(normalizer, BuildWebSocketUrl(webSocketUrl, symbols))
        {
            _symbols = symbols.Length > 0 ? symbols : new[] { "btcusdt" };
        }

        private static string BuildWebSocketUrl(string baseUrl, string[] symbols)
        {
            var streams = string.Join("/", symbols.Select(s => $"{s.ToLower()}@trade"));
            return $"{baseUrl}{streams}";
        }

        protected override string SourceName => "Binance";


        protected override Task OnConnectedAsync(ClientWebSocket ws, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        protected override bool ShouldProcessMessage(IRawTick rawTick)
        {
            return rawTick is BinanceRawTick binance && binance.EventType == "trade";
        }

        protected override object DeserializeMessage(string json)
        {
            return JsonSerializer.Deserialize<BinanceRawTick>(json) ?? new BinanceRawTick();
        }
    }
}






