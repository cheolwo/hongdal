using Ssalddel.Domain.TraditionalMarkets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssalddel.Infrastructure.Persistence.TraditionalMarkets;

public sealed class TraditionalMarketSyncRunConfiguration : IEntityTypeConfiguration<TraditionalMarketSyncRun>
{
    public void Configure(EntityTypeBuilder<TraditionalMarketSyncRun> builder)
    {
        builder.ToTable("public_data_traditional_market_sync_runs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasMaxLength(30).IsRequired();
        builder.Property(x => x.SourceDatasetKey).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.HasIndex(x => new { x.SourceDatasetKey, x.StartedAtUtc });
        builder.HasIndex(x => x.Status);
    }
}
