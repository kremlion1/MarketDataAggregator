using System.Net.WebSockets;
using System.Threading.Channels;
using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using MarketDataAggregator.Infrastructure.Sources.RawData;

namespace MarketDataAggregator.Infrastructure.Sources
{
    public abstract class WebSocketDataSourceBase : IMarketDataSource
    {
        protected readonly INormalizer Normalizer;

        protected WebSocketDataSourceBase(INormalizer normalizer)
        {
            Normalizer = normalizer;
        }

        protected abstract string WebSocketUrl { get; }
        protected abstract Task OnConnectedAsync(ClientWebSocket ws, CancellationToken ct);
        protected abstract string SourceName { get; }
        protected abstract bool ShouldProcessMessage(IRawTick rawTick);
        protected abstract object DeserializeMessage(string json);

        public async Task StartAsync(ChannelWriter<MarketTick> writer, CancellationToken ct)
        {
            try
            {
                Console.WriteLine($"Connecting to {SourceName}: {WebSocketUrl}");

                using (var ws = new ClientWebSocket())
                {
                    await ws.ConnectAsync(new Uri(WebSocketUrl), ct);
                    Console.WriteLine($"{SourceName} WebSocket connected");

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
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{SourceName} receive error: {ex.Message}");
                        }
                    }

                    if (ws.State == WebSocketState.Open)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"{SourceName} source stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{SourceName} source error: {ex.Message}");
            }
        }

        private void ProcessMessage(string json, ChannelWriter<MarketTick> writer, CancellationToken ct)
        {
            try
            {
                var rawData = DeserializeMessage(json);
                
                if (rawData is IRawTick rawTick && ShouldProcessMessage(rawTick))
                {
                    var tick = Normalizer.Normalize(rawData, SourceName);
                    writer.WriteAsync(tick, ct).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{SourceName} deserialization error: {ex.Message}");
            }
        }
    }
}

