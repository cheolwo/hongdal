using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.정산;

namespace 살뜰.Infrastructure.Persistence.Configurations.Settlement;

public sealed class PlatformProfitReturnScheduleRecordConfiguration : IEntityTypeConfiguration<PlatformProfitReturnScheduleRecord>
{
    public void Configure(EntityTypeBuilder<PlatformProfitReturnScheduleRecord> builder)
    {
        builder.HasIndex(x => new { x.ParticipantUserId, x.ScheduledPaymentDate, x.Status });

        builder.HasOne(x => x.Policy)
            .WithMany()
            .HasForeignKey(x => x.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
