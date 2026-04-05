using MarketDataAggregator.Domain.Models;

namespace MarketDataAggregator.Application.Interfaces
{
    public interface INormalizer
    {
        MarketTick Normalize(object rawData, string source);
    }
}
