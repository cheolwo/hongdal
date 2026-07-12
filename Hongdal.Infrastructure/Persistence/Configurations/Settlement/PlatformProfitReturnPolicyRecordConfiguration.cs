using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.정산;

namespace 홍달.Infrastructure.Persistence.Configurations.Settlement;

public sealed class PlatformProfitReturnPolicyRecordConfiguration : IEntityTypeConfiguration<PlatformProfitReturnPolicyRecord>
{
    public void Configure(EntityTypeBuilder<PlatformProfitReturnPolicyRecord> builder)
    {
        builder.HasIndex(x => new { x.TargetParticipantCategory, x.IsActive, x.EffectiveStartDate });
    }
}
