using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;

namespace MarketDataAggregator.Infrastructure.Normalization
{
    public class SimpleNormalizer: INormalizer
    {
        public MarketTick Normalize(object rawData, string source)
        {
            //TODO  implement
            return (MarketTick)rawData;
        }
    }
}
