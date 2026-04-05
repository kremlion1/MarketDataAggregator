using MarketDataAggregator.Application.Interfaces;

namespace MarketDataAggregator.Infrastructure.Monitoring
{
    public class MetricsService : IMetricsService
    {
        private long _ticksProcessed = 0;
        private long _duplicatesRemoved = 0;
        private long _saveErrors = 0;
        private DateTime _startTime = DateTime.UtcNow;
        private readonly object _lockObj = new();

        public void IncrementTicksProcessed(int count)
        {
            lock (_lockObj)
            {
                _ticksProcessed += count;
            }
        }

        public void IncrementDuplicatesRemoved(int count)
        {
            lock (_lockObj)
            {
                _duplicatesRemoved += count;
            }
        }

        public void IncrementSaveErrors()
        {
            lock (_lockObj)
            {
                _saveErrors++;
            }
        }

        public void PrintMetrics()
        {
            lock (_lockObj)
            {
                var elapsed = DateTime.UtcNow - _startTime;
                var ticksPerSecond = elapsed.TotalSeconds > 0 ? _ticksProcessed / elapsed.TotalSeconds : 0;

                Console.WriteLine("\n" +
                    $"╔════════════════════════════════════════╗\n" +
                    $"║ METRICS                                ║\n" +
                    $"║ ────────────────────────────────────── ║\n" +
                    $"║ Ticks Processed: {_ticksProcessed,-24}║\n" +
                    $"║ Duplicates Removed: {_duplicatesRemoved,-20}║\n" +
                    $"║ Save Errors: {_saveErrors,-27}║\n" +
                    $"║ Ticks/sec: {ticksPerSecond:F2,-24}║\n" +
                    $"║ Uptime: {FormatTimeSpan(elapsed),-27}║\n" +
                    $"╚════════════════════════════════════════╝\n");
            }
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }
    }
}


