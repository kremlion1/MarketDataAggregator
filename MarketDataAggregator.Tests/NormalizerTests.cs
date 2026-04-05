using MarketDataAggregator.Infrastructure.Normalization;
using MarketDataAggregator.Infrastructure.Sources.RawData;

namespace MarketDataAggregator.Tests;

public class BinanceNormalizerTests
{
    private BinanceNormalizer _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new BinanceNormalizer();
    }

    [Test]
    public void ShouldNormalizeRawTick()
    {
        var rawTick = new BinanceRawTick
        {
            EventType = "trade",
            Symbol = "BTCUSDT",
            Price = 45000m,
            Quantity = 0.5m,
            TradeTime = 1712386839000
        };

        var result = _sut.Normalize(rawTick, "Binance");

        Assert.That(result.Ticker, Is.EqualTo("BTCUSDT"));
        Assert.That(result.Price, Is.EqualTo(45000m));
        Assert.That(result.Volume, Is.EqualTo(0.5m));
        Assert.That(result.Source, Is.EqualTo("Binance"));
    }

    [Test]
    public void ShouldHaveCorrectTimestamp()
    {
        var rawTick = new BinanceRawTick
        {
            EventType = "trade",
            Symbol = "BTCUSDT",
            Price = 45000m,
            Quantity = 0.5m,
            TradeTime = 1712386839000
        };

        var result = _sut.Normalize(rawTick, "Binance");

        Assert.That(result.Timestamp, Is.Not.EqualTo(default(DateTime)));
    }
}

public class CoinbaseNormalizerTests
{
    private CoinbaseNormalizer _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new CoinbaseNormalizer();
    }

    [Test]
    public void ShouldNormalizeRawTick()
    {
        var rawTick = new CoinbaseRawTick
        {
            type = "match",
            product_id = "BTC-USD",
            price = 45000m,
            size = 0.5m,
            time = "2026-04-06T12:00:00.000000Z"
        };

        var result = _sut.Normalize(rawTick, "Coinbase");

        Assert.That(result.Ticker, Is.EqualTo("BTCUSDT"));
        Assert.That(result.Price, Is.EqualTo(45000m));
        Assert.That(result.Volume, Is.EqualTo(0.5m));
        Assert.That(result.Source, Is.EqualTo("Coinbase"));
    }

    [Test]
    public void ShouldConvertProductIdToTicker()
    {
        var rawTick = new CoinbaseRawTick
        {
            type = "match",
            product_id = "ETH-USD",
            price = 2500m,
            size = 1m,
            time = "2026-04-06T12:00:00.000000Z"
        };

        var result = _sut.Normalize(rawTick, "Coinbase");

        Assert.That(result.Ticker, Is.EqualTo("ETHUSDT"));
    }
}

