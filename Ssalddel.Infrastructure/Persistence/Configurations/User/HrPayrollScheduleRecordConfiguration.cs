using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.사용자;

namespace 살뜰.Infrastructure.Persistence.Configurations.User;

public sealed class HrPayrollScheduleRecordConfiguration : IEntityTypeConfiguration<HrPayrollScheduleRecord>
{
    public void Configure(EntityTypeBuilder<HrPayrollScheduleRecord> builder)
    {
        builder.HasIndex(x => new { x.WorkerUserId, x.ScheduledPaymentDate, x.Status });

        builder.HasOne(x => x.Contract)
            .WithMany(x => x.PayrollSchedules)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
