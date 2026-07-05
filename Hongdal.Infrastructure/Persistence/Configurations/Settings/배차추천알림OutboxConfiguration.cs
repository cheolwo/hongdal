using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.설정;

namespace 홍달.Infrastructure.Persistence.Configurations.Settings;

public sealed class 배차추천알림OutboxConfiguration : IEntityTypeConfiguration<배차추천알림Outbox>
{
    public void Configure(EntityTypeBuilder<배차추천알림Outbox> builder)
    {
        builder.HasIndex(x => new { x.발송상태, x.CreatedAt });
        builder.HasIndex(x => new { x.배차대기Id, x.기사Id, x.추천라운드 }).IsUnique();
    }
}
