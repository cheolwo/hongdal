using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.설정;

namespace 살뜰.Infrastructure.Persistence.Configurations.Settings;

public sealed class Command알림OutboxConfiguration : IEntityTypeConfiguration<Command알림Outbox>
{
    public void Configure(EntityTypeBuilder<Command알림Outbox> builder)
    {
        builder.HasIndex(x => new { x.Status, x.CreatedAt });
    }
}
