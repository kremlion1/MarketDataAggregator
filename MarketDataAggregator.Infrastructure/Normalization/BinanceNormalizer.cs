using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using MarketDataAggregator.Infrastructure.Sources.RawData;

namespace MarketDataAggregator.Infrastructure.Normalization
{
    public class BinanceNormalizer : INormalizer
    {
        public MarketTick Normalize(object rawData, string source)
        {
            if (rawData is not BinanceRawTick binanceTick)
                throw new ArgumentException($"Expected BinanceRawTick, got {rawData.GetType().Name}");

            var timestamp = UnixTimeStampToDateTime(binanceTick.TradeTime);

            return new MarketTick
            {
                Ticker = binanceTick.Symbol.ToUpper(),
                Price = binanceTick.Price,
                Volume = binanceTick.Quantity,
                Timestamp = timestamp,
                Source = source
            };
        }

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToUniversalTime();
            return dateTime;
        }
    }
}




