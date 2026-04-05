using Serilog;
using Serilog.Core;
using Microsoft.Extensions.Configuration;

namespace MarketDataAggregator.ConsoleApp.Logging
{
    public static class LoggerSetup
    {
        public static Logger CreateLogger(IConfiguration config)
        {
            var logPath = config["Logging:LogPath"] ?? "./logs";
            Directory.CreateDirectory(logPath);
            
            var logFilePath = Path.Combine(logPath, "market-data-.txt");

            return new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 7)
                .CreateLogger();
        }
    }
}

