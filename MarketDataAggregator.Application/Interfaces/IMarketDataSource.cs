using MarketDataAggregator.Domain.Models;
using System.Threading.Channels;

namespace MarketDataAggregator.Application.Interfaces
{
    public interface IMarketDataSource
    {
        Task StartAsync(ChannelWriter<MarketTick> writer, CancellationToken ct);
    }
}
