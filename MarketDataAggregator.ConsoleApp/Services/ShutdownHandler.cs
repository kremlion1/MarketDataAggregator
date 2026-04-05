namespace MarketDataAggregator.ConsoleApp.Services
{
    public class ShutdownHandler
    {
        private readonly CancellationTokenSource _cts;

        public ShutdownHandler(CancellationTokenSource cts)
        {
            _cts = cts;
        }

        public void RegisterShutdownHandler()
        {
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                Console.WriteLine("\nShutting down gracefully...");
                _cts.Cancel();
            };
        }
    }
}

