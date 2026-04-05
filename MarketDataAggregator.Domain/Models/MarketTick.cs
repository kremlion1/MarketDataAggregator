namespace MarketDataAggregator.Domain.Models
{
    public class MarketTick
    {
        public long Id { get; set; }
        public string Ticker { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public decimal Volume { get; init; }
        public DateTime Timestamp { get; init; }
        public string Source { get; init; } = string.Empty;
    }
}
