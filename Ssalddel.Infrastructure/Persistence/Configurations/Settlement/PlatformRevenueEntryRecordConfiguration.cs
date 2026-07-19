using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.정산;

namespace 살뜰.Infrastructure.Persistence.Configurations.Settlement;

public sealed class PlatformRevenueEntryRecordConfiguration : IEntityTypeConfiguration<PlatformRevenueEntryRecord>
{
    public void Configure(EntityTypeBuilder<PlatformRevenueEntryRecord> builder)
    {
        builder.HasIndex(x => new { x.RevenueSource, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.SourceReferenceType, x.SourceReferenceId });
    }
}
