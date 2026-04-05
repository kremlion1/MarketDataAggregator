using MarketDataAggregator.Application.Interfaces;

namespace MarketDataAggregator.Application.Pipeline
{
    public class MetricsReporter : IDisposable
    {
        private readonly IMetricsService _metrics;
        private readonly System.Timers.Timer _timer;

        public MetricsReporter(IMetricsService metrics, int intervalMs = 5000)
        {
            _metrics = metrics;
            _timer = new System.Timers.Timer(intervalMs);
            _timer.Elapsed += (s, e) => _metrics.PrintMetrics();
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}

