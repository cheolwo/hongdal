using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.설정;

namespace 살뜰.Infrastructure.Persistence.Configurations.Settings;

public sealed class 기사지급OutboxConfiguration : IEntityTypeConfiguration<기사지급Outbox>
{
    public void Configure(EntityTypeBuilder<기사지급Outbox> builder)
    {
        builder.ToTable("기사지급_Outbox");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.기사지급요청Id).HasColumnName("driver_payout_request_id");
        builder.Property(x => x.멱등키).HasColumnName("idempotency_key").HasMaxLength(128).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnName("payload_json").IsRequired();
        builder.Property(x => x.처리상태).HasColumnName("status_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.시도횟수).HasColumnName("attempt_count");
        builder.Property(x => x.다음시도시각Utc).HasColumnName("next_attempt_at_utc");
        builder.Property(x => x.마지막시도시각Utc).HasColumnName("last_attempted_at_utc");
        builder.Property(x => x.마지막결과코드).HasColumnName("last_result_code").HasMaxLength(80);
        builder.Property(x => x.마지막오류메시지).HasColumnName("last_error_message").HasMaxLength(500);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => x.기사지급요청Id).IsUnique();
        builder.HasIndex(x => x.멱등키).IsUnique();
        builder.HasIndex(x => new { x.처리상태, x.다음시도시각Utc, x.CreatedAtUtc });
        builder.HasOne(x => x.기사지급요청)
            .WithOne()
            .HasForeignKey<기사지급Outbox>(x => x.기사지급요청Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
