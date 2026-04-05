using MarketDataAggregator.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketDataAggregator.Infrastructure.Tables
{
    public class MarketTickConfiguration: IEntityTypeConfiguration<MarketTick>
    {
        public void Configure(EntityTypeBuilder<MarketTick> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Ticker)
                .HasMaxLength(20)
                .IsRequired();
            
            builder.Property(x => x.Source)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(x => x.Price).HasPrecision(18, 6);
            builder.Property(x => x.Volume).HasPrecision(18, 6);
            
            builder.HasIndex(x => new { x.Ticker, x.Timestamp, x.Source })
                .IsUnique()
                .HasDatabaseName("IX_UniqueTickerTimestampSource");
        }
    }
}
