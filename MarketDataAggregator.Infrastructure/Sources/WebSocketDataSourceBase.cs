using System.Net.WebSockets;
using System.Threading.Channels;
using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using MarketDataAggregator.Infrastructure.Sources.RawData;
using Serilog;

namespace MarketDataAggregator.Infrastructure.Sources
{
    public abstract class WebSocketDataSourceBase : IMarketDataSource
    {
        private readonly INormalizer _normalizer;
        private readonly string _webSocketUrl;
        private const int MaxRetries = 5;
        private const int InitialRetryDelayMs = 1000;

        protected WebSocketDataSourceBase(INormalizer normalizer, string webSocketUrl)
        {
            _normalizer = normalizer;
            _webSocketUrl = webSocketUrl;
        }

        protected abstract Task OnConnectedAsync(ClientWebSocket ws, CancellationToken ct);
        protected abstract string SourceName { get; }
        protected abstract bool ShouldProcessMessage(IRawTick rawTick);
        protected abstract object DeserializeMessage(string json);

        public async Task StartAsync(ChannelWriter<MarketTick> writer, CancellationToken ct)
        {
            int retryCount = 0;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    Log.Information("Connecting to {SourceName}: {WebSocketUrl} (attempt {RetryCount})", 
                        SourceName, _webSocketUrl, retryCount + 1);

                    using (var ws = new ClientWebSocket())
                    {
                        await ws.ConnectAsync(new Uri(_webSocketUrl), ct);
                        Log.Information("{SourceName} WebSocket connected", SourceName);

                        await OnConnectedAsync(ws, ct);

                        var buffer = new byte[1024 * 4];

                        while (ws.State == WebSocketState.Open && !ct.IsCancellationRequested)
                        {
                            try
                            {
                                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                                if (result.MessageType == WebSocketMessageType.Text)
                                {
                                    var json = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                                    ProcessMessage(json, writer, ct);
                                }
                            }
                            catch (WebSocketException ex)
                            {
                                Log.Warning(ex, "{SourceName} WebSocket connection lost", SourceName);
                                break;
                            }
                            catch (Exception ex)
                            {
                                Log.Warning(ex, "{SourceName} receive error", SourceName);
                            }
                        }

                        if (ws.State == WebSocketState.Open)
                        {
                            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
                        }
                    }

                    retryCount++;
                    if (retryCount >= MaxRetries)
                    {
                        Log.Fatal("{SourceName} exhausted all retry attempts after connection loss", SourceName);
                        break;
                    }

                    var delayMs = InitialRetryDelayMs * (int)Math.Pow(2, retryCount - 1);
                    Log.Information("{SourceName} connection lost, retrying in {DelayMs}ms", SourceName, delayMs);
                    
                    try
                    {
                        await Task.Delay(delayMs, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        Log.Information("{SourceName} source stopped during retry delay", SourceName);
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    Log.Information("{SourceName} source stopped", SourceName);
                    break;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    if (retryCount >= MaxRetries)
                    {
                        Log.Fatal(ex, "{SourceName} failed after {MaxRetries} attempts", SourceName, MaxRetries);
                        break;
                    }

                    var delayMs = InitialRetryDelayMs * (int)Math.Pow(2, retryCount - 1);
                    Log.Warning(ex, "{SourceName} connection error, retrying in {DelayMs}ms", SourceName, delayMs);
                    
                    try
                    {
                        await Task.Delay(delayMs, ct);
                    }
                    catch (OperationCanceledException)
                    {
                        Log.Information("{SourceName} source stopped during retry delay", SourceName);
                        break;
                    }
                }
            }
        }

        private void ProcessMessage(string json, ChannelWriter<MarketTick> writer, CancellationToken ct)
        {
            try
            {
                var rawData = DeserializeMessage(json);
                
                if (rawData is IRawTick rawTick && ShouldProcessMessage(rawTick))
                {
                    var tick = _normalizer.Normalize(rawData, SourceName);
                    writer.WriteAsync(tick, ct).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "{SourceName} deserialization error", SourceName);
            }
        }
    }
}

