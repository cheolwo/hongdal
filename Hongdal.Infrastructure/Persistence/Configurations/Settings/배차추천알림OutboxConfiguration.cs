using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.설정;

namespace 홍달.Infrastructure.Persistence.Configurations.Settings;

public sealed class 배차추천알림OutboxConfiguration : IEntityTypeConfiguration<배차추천알림Outbox>
{
    public void Configure(EntityTypeBuilder<배차추천알림Outbox> builder)
    {
        builder.ToTable("배차추천_알림_Outbox");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.배차대기Id).HasColumnName("dispatch_waiting_id");
        builder.Property(x => x.의뢰Id).HasColumnName("request_id").HasMaxLength(64).IsRequired();
        builder.Property(x => x.기사Id).HasColumnName("driver_id").HasMaxLength(64).IsRequired();
        builder.Property(x => x.추천라운드).HasColumnName("recommendation_round");
        builder.Property(x => x.제목).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(x => x.본문).HasColumnName("body").HasMaxLength(500).IsRequired();
        builder.Property(x => x.DataJson).HasColumnName("data_json").IsRequired();
        builder.Property(x => x.발송상태).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(x => x.시도횟수).HasColumnName("retry_count");
        builder.Property(x => x.마지막시도시각).HasColumnName("last_attempted_at");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => new { x.발송상태, x.CreatedAt });
        builder.HasIndex(x => new { x.배차대기Id, x.기사Id, x.추천라운드 }).IsUnique();
    }
}
