using System.Text.Json.Serialization;
using MarketDataAggregator.Infrastructure.Sources.RawData.Converters;

namespace MarketDataAggregator.Infrastructure.Sources.RawData
{
    public class CoinbaseRawTick : IRawTick
    {
        public string type { get; set; } = string.Empty;
        public string product_id { get; set; } = string.Empty;
        [JsonConverter(typeof(DecimalConverter))]
        public decimal price { get; set; }
        [JsonConverter(typeof(DecimalConverter))]
        public decimal size { get; set; }
        public string time { get; set; } = string.Empty;
        public string side { get; set; } = string.Empty;

        public string GetMessageType() => type;
    }
}

