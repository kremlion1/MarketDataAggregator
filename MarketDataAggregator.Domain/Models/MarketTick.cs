namespace MarketDataAggregator.Domain.Models
{
    public class MarketTick
    {
        public string Ticker { get; init; }
        public decimal Price { get; init; }
        public decimal Volume { get; init; }
        public DateTime Timestamp { get; init; }
        public string Source { get; init; } 
    }
}
