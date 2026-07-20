using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.HumanResources;

namespace 살뜰.Infrastructure.Persistence.Configurations.HumanResources;

public sealed class HrRoleApplicationRecordConfiguration : IEntityTypeConfiguration<HrRoleApplicationRecord>
{
    public void Configure(EntityTypeBuilder<HrRoleApplicationRecord> builder)
    {
        builder.HasIndex(x => new { x.ApplicantUserId, x.SubmissionRequestId }).IsUnique();
        builder.HasIndex(x => x.ActiveApplicationKey).IsUnique();
        builder.HasIndex(x => new { x.ApplicantUserId, x.SubmittedAtUtc });
        builder.HasIndex(x => new { x.StatusCode, x.SubmittedAtUtc });
    }
}
