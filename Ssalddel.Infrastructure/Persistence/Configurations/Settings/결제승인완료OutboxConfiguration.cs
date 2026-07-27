using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.설정;

namespace 살뜰.Infrastructure.Persistence.Configurations.Settings;

public sealed class 결제승인완료OutboxConfiguration : IEntityTypeConfiguration<결제승인완료Outbox>
{
    public void Configure(EntityTypeBuilder<결제승인완료Outbox> builder)
    {
        builder.Property(x => x.처리상태).IsConcurrencyToken();
        builder.HasIndex(x => new { x.처리상태, x.CreatedAt });
        builder.HasIndex(x => x.결제레코드Id).IsUnique();
    }
}
