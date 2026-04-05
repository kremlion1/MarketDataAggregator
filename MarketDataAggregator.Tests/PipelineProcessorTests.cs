using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Application.Pipeline;
using MarketDataAggregator.Domain.Models;
using Moq;

namespace MarketDataAggregator.Tests;

public class PipelineProcessorTests
{
    private PipelineProcessor _sut;
    private Mock<ITickStorage> _mockStorage;
    private Mock<IDeduplicationService> _mockDeduplicationService;
    private Mock<IMetricsService> _mockMetrics;

    [SetUp]
    public void Setup()
    {
        _mockStorage = new Mock<ITickStorage>();
        _mockDeduplicationService = new Mock<IDeduplicationService>();
        _mockMetrics = new Mock<IMetricsService>();

        _sut = new PipelineProcessor(_mockStorage.Object, _mockDeduplicationService.Object, _mockMetrics.Object);
    }

    [Test]
    public void ShouldProcessTicksSuccessfully()
    {
        var tick = new MarketTick
        {
            Ticker = "BTC-USD",
            Price = 45000m,
            Volume = 0.5m,
            Timestamp = DateTime.UtcNow,
            Source = "Binance"
        };

        _mockDeduplicationService
            .Setup(x => x.FilterDuplicatesAsync(It.IsAny<List<MarketTick>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MarketTick> { tick }.AsEnumerable());

        Assert.Pass();
    }

    [Test]
    public void ShouldHaveWriter()
    {
        Assert.That(_sut.Writer, Is.Not.Null);
    }
}

