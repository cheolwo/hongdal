using Ssalddel.Domain.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ssalddel.Infrastructure.Persistence.Configurations.Community;

public sealed class 커뮤니티원장상태이벤트Configuration : IEntityTypeConfiguration<커뮤니티원장상태이벤트>
{
    public void Configure(EntityTypeBuilder<커뮤니티원장상태이벤트> builder)
    {
        builder.ToTable("community_ledger_state_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventId).HasMaxLength(80).IsRequired();
        builder.Property(x => x.커뮤니티원장Id).HasMaxLength(120).IsRequired();
        builder.Property(x => x.커뮤니티Id).HasMaxLength(120).IsRequired();
        builder.Property(x => x.원장템플릿Key).HasMaxLength(120).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.이전상태).HasMaxLength(80);
        builder.Property(x => x.상태).HasMaxLength(80).IsRequired();
        builder.Property(x => x.현재단계Key).HasMaxLength(120);
        builder.Property(x => x.변경사유).HasMaxLength(500);
        builder.Property(x => x.UpdatedBy).HasMaxLength(450).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(120);
        builder.Property(x => x.SnapshotJson).HasColumnType("longtext").IsRequired();

        builder.HasIndex(x => x.EventId).IsUnique();
        builder.HasIndex(x => new { x.커뮤니티원장Id, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.커뮤니티Id, x.원장템플릿Key, x.상태 });
        builder.HasIndex(x => x.CorrelationId);
    }
}
