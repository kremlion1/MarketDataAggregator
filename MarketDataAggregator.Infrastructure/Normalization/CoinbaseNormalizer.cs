using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using MarketDataAggregator.Infrastructure.Sources.RawData;

namespace MarketDataAggregator.Infrastructure.Normalization
{
    public class CoinbaseNormalizer : INormalizer
    {
        public MarketTick Normalize(object rawData, string source)
        {
            if (rawData is not CoinbaseRawTick coinbaseTick)
                throw new ArgumentException($"Expected CoinbaseRawTick, got {rawData.GetType().Name}");

            var timestamp = DateTime.Parse(coinbaseTick.time, null, System.Globalization.DateTimeStyles.RoundtripKind);
            var ticker = ConvertProductIdToTicker(coinbaseTick.product_id);

            return new MarketTick
            {
                Ticker = ticker,
                Price = coinbaseTick.price,
                Volume = coinbaseTick.size,
                Timestamp = timestamp,
                Source = source
            };
        }


        private string ConvertProductIdToTicker(string productId)
        {
            return productId switch
            {
                "BTC-USD" => "BTCUSDT",
                "ETH-USD" => "ETHUSDT",
                "BNB-USD" => "BNBUSDT",
                _ => productId.Replace("-", "")
            };
        }
    }
}



