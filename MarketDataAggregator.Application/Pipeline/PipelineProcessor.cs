using MarketDataAggregator.Application.Interfaces;
using MarketDataAggregator.Domain.Models;
using System.Threading.Channels;

namespace MarketDataAggregator.Application.Pipeline
{
    public class PipelineProcessor
    {
        private readonly INormalizer _normalizer;
        private readonly ITickStorage _storage;

        private readonly Channel<MarketTick> _channel = Channel.CreateUnbounded<MarketTick>();

        public ChannelWriter<MarketTick> Writer => _channel.Writer;

        public PipelineProcessor(INormalizer normalizer, ITickStorage storage)
        {
            _normalizer = normalizer;
            _storage = storage;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            await foreach (var tick in _channel.Reader.ReadAllAsync(ct))
            {
                // TODO add normalization
                await _storage.SaveAsync(tick, ct);
            }
        }
    }
}
