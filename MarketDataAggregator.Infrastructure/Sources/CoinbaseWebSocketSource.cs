using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Infrastructure.Sources.RawData;
using System.Net.WebSockets;
using System.Text.Json;

namespace MarketDataAggregator.Infrastructure.Sources
{
    public class CoinbaseWebSocketSource : WebSocketDataSourceBase
    {
        private readonly string[] _products;

        public CoinbaseWebSocketSource(INormalizer normalizer, string webSocketUrl, string[] products)
            : base(normalizer, webSocketUrl)
        {
            _products = products.Length > 0 ? products : new[] { "BTC-USD" };
        }

        protected override string SourceName => "Coinbase";

        protected override async Task OnConnectedAsync(ClientWebSocket ws, CancellationToken ct)
        {
            var subscribeMessage = new
            {
                type = "subscribe",
                product_ids = _products,
                channels = new[] { "matches" }
            };

            var json = JsonSerializer.Serialize(subscribeMessage);
            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, ct);
        }

        protected override bool ShouldProcessMessage(IRawTick rawTick)
        {
            return rawTick is CoinbaseRawTick coinbase && coinbase.type == "match";
        }

        protected override object DeserializeMessage(string json)
        {
            return JsonSerializer.Deserialize<CoinbaseRawTick>(json) ?? new CoinbaseRawTick();
        }
    }
}









