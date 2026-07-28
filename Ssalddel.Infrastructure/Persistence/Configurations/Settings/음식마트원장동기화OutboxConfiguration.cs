using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.설정;

namespace 살뜰.Infrastructure.Persistence.Configurations.Settings;

public sealed class 음식마트원장동기화OutboxConfiguration
    : IEntityTypeConfiguration<음식마트원장동기화Outbox>
{
    public void Configure(EntityTypeBuilder<음식마트원장동기화Outbox> builder)
    {
        builder.Property(x => x.처리상태).IsConcurrencyToken();
        builder.HasIndex(x => x.멱등키).IsUnique();
        builder.HasIndex(x => new { x.처리상태, x.UpdatedAtUtc });
    }
}
