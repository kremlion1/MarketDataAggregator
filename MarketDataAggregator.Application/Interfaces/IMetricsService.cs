namespace MarketDataAggregator.Application.Interfaces
{
    public interface IMetricsService
    {
        void IncrementTicksProcessed(int count);
        void IncrementDuplicatesRemoved(int count);
        void IncrementSaveErrors();
        void PrintMetrics();
    }
}

