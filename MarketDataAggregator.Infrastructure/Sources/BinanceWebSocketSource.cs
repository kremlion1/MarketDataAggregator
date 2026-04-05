using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Infrastructure.Sources.RawData;
using System.Net.WebSockets;
using System.Text.Json;

namespace MarketDataAggregator.Infrastructure.Sources
{
    public class BinanceWebSocketSource : WebSocketDataSourceBase
    {
        private readonly string[] _symbols;

        public BinanceWebSocketSource(INormalizer normalizer, string[] symbols)
            : base(normalizer)
        {
            _symbols = symbols.Length > 0 ? symbols : new[] { "btcusdt" };
        }

        protected override string SourceName => "Binance";

        protected override string WebSocketUrl
        {
            get
            {
                var streams = string.Join("/", _symbols.Select(s => $"{s.ToLower()}@trade"));
                return $"wss://stream.binance.com:9443/ws/{streams}";
            }
        }

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






