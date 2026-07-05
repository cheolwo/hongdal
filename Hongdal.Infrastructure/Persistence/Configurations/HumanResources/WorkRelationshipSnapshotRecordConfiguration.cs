using Hongdal.Domain.HumanResources;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hongdal.Infrastructure.Persistence.Configurations.HumanResources;

public sealed class WorkRelationshipSnapshotRecordConfiguration : IEntityTypeConfiguration<WorkRelationshipSnapshotRecord>
{
    public void Configure(EntityTypeBuilder<WorkRelationshipSnapshotRecord> builder)
    {
        builder.ToTable("work_relationship_snapshots");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ActorUserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.ActorAnonymousLabel).HasMaxLength(80).IsRequired();
        builder.Property(x => x.ActorRoleCode).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ActorRoleName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.WorkDomain).HasMaxLength(80).IsRequired();
        builder.Property(x => x.WorkProcess).HasMaxLength(80).IsRequired();
        builder.Property(x => x.ActionCode).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ActionLabel).HasMaxLength(160).IsRequired();
        builder.Property(x => x.RelatedEntityType).HasMaxLength(120).IsRequired();
        builder.Property(x => x.RelatedEntityId).HasMaxLength(160).IsRequired();
        builder.Property(x => x.RelatedDisplayLabel).HasMaxLength(240).IsRequired();
        builder.Property(x => x.CounterpartyUserId).HasMaxLength(450);
        builder.Property(x => x.CounterpartyAnonymousLabel).HasMaxLength(80);
        builder.Property(x => x.CounterpartyRoleCode).HasMaxLength(120);
        builder.Property(x => x.PrivacyLevel).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Memo).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.AppKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.TraceId).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ClientIpSnapshot).HasMaxLength(80).IsRequired();

        builder.HasIndex(x => new { x.ActorUserId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.WorkDomain, x.WorkProcess, x.ActionCode, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.RelatedEntityType, x.RelatedEntityId, x.OccurredAtUtc });
    }
}
