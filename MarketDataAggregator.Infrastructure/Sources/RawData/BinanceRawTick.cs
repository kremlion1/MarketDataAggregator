using System.Text.Json.Serialization;
using MarketDataAggregator.Infrastructure.Sources.RawData.Converters;

namespace MarketDataAggregator.Infrastructure.Sources.RawData
{
    public class BinanceRawTick : IRawTick
    {
        [JsonPropertyName("e")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("E")]
        public long EventTime { get; set; }

        [JsonPropertyName("s")]
        public string Symbol { get; set; } = string.Empty;

        [JsonPropertyName("t")]
        public long TradeId { get; set; }

        [JsonPropertyName("p")]
        [JsonConverter(typeof(DecimalConverter))]
        public decimal Price { get; set; }

        [JsonPropertyName("q")]
        [JsonConverter(typeof(DecimalConverter))]
        public decimal Quantity { get; set; }

        [JsonPropertyName("T")]
        public long TradeTime { get; set; }

        [JsonPropertyName("m")]
        public bool IsBuyerMaker { get; set; }

        public string GetMessageType() => EventType;
    }
}

