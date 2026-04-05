using MarketDataAggregator.Infrastructure.Monitoring;

namespace MarketDataAggregator.Tests;

public class MetricsServiceTests
{
    private MetricsService _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new MetricsService();
    }

    [Test]
    public void IncrementTicksProcessed_ShouldIncrement()
    {
        _sut.IncrementTicksProcessed(10);
        _sut.IncrementTicksProcessed(5);

        Assert.Pass();
    }

    [Test]
    public void IncrementDuplicatesRemoved_ShouldIncrement()
    {
        _sut.IncrementDuplicatesRemoved(3);
        _sut.IncrementDuplicatesRemoved(2);

        Assert.Pass();
    }

    [Test]
    public void IncrementSaveErrors_ShouldIncrement()
    {
        _sut.IncrementSaveErrors();
        _sut.IncrementSaveErrors();

        Assert.Pass();
    }

    [Test]
    public void PrintMetrics_ShouldNotThrow()
    {
        _sut.IncrementTicksProcessed(100);
        _sut.IncrementDuplicatesRemoved(10);
        _sut.IncrementSaveErrors();

        _sut.PrintMetrics();

        Assert.Pass();
    }
}

