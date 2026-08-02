using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Domain.Community;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Infrastructure.Persistence.Configurations.Community;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityActivityPaidDetail,
    SsalddelCodeLayer.Infrastructure,
    "유료 상세, 구매 상태 이력과 열람권의 관계형 제약과 멱등 인덱스를 구성합니다.",
    FlowOrder = 70,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "구매자별 중복 구매와 결제 또는 열람권의 중복 연결을 고유 제약으로 차단합니다.")]
public sealed class 커뮤니티활동유료상세Configuration : IEntityTypeConfiguration<커뮤니티활동유료상세>
{
    public void Configure(EntityTypeBuilder<커뮤니티활동유료상세> builder)
    {
        builder.ToTable("community_activity_paid_details");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.상세Id).HasColumnName("detail_id").HasMaxLength(80).IsRequired();
        builder.Property(x => x.게시글Id).HasColumnName("post_id");
        builder.Property(x => x.판매자UserId).HasColumnName("seller_user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.공개미리보기).HasColumnName("public_preview").HasMaxLength(500).IsRequired();
        builder.Property(x => x.상세내용).HasColumnName("detail_content").HasColumnType("longtext").IsRequired();
        builder.Property(x => x.가격금액).HasColumnName("price_amount");
        builder.Property(x => x.통화Code).HasColumnName("currency_code").HasMaxLength(3).IsRequired();
        builder.Property(x => x.판매상태).HasColumnName("sale_status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc");

        builder.HasIndex(x => x.상세Id).IsUnique();
        builder.HasIndex(x => x.게시글Id).IsUnique();
        builder.HasIndex(x => new { x.판매자UserId, x.판매상태, x.CreatedAtUtc });
        builder.HasOne(x => x.게시글)
            .WithMany()
            .HasForeignKey(x => x.게시글Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class 커뮤니티활동상세열람권Configuration : IEntityTypeConfiguration<커뮤니티활동상세열람권>
{
    public void Configure(EntityTypeBuilder<커뮤니티활동상세열람권> builder)
    {
        builder.ToTable("community_activity_detail_entitlements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.열람권Id).HasColumnName("entitlement_id").HasMaxLength(80).IsRequired();
        builder.Property(x => x.상세Id).HasColumnName("detail_id").HasMaxLength(80).IsRequired();
        builder.Property(x => x.구매자UserId).HasColumnName("buyer_user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.결제Id).HasColumnName("payment_id").HasMaxLength(120).IsRequired();
        builder.Property(x => x.상태).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.발급일시Utc).HasColumnName("granted_at_utc");
        builder.Property(x => x.철회일시Utc).HasColumnName("revoked_at_utc");

        builder.HasIndex(x => x.열람권Id).IsUnique();
        builder.HasIndex(x => new { x.상세Id, x.구매자UserId }).IsUnique();
        builder.HasIndex(x => x.결제Id).IsUnique();
        builder.HasOne(x => x.상세)
            .WithMany(x => x.열람권목록)
            .HasPrincipalKey(x => x.상세Id)
            .HasForeignKey(x => x.상세Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class 커뮤니티활동상세구매Configuration : IEntityTypeConfiguration<커뮤니티활동상세구매>
{
    public void Configure(EntityTypeBuilder<커뮤니티활동상세구매> builder)
    {
        builder.ToTable("community_activity_detail_purchases");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.구매Id).HasColumnName("purchase_id").HasMaxLength(80).IsRequired();
        builder.Property(x => x.상세Id).HasColumnName("detail_id").HasMaxLength(80).IsRequired();
        builder.Property(x => x.구매자UserId).HasColumnName("buyer_user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.판매자UserId).HasColumnName("seller_user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.멱등성Key).HasColumnName("idempotency_key").HasMaxLength(160);
        builder.Property(x => x.요청금액).HasColumnName("requested_amount");
        builder.Property(x => x.통화Code).HasColumnName("currency_code").HasMaxLength(3).IsRequired();
        builder.Property(x => x.현재상태).HasColumnName("current_status").HasMaxLength(40).IsRequired();
        builder.Property(x => x.결제Id).HasColumnName("payment_id").HasMaxLength(120);
        builder.Property(x => x.열람권Id).HasColumnName("entitlement_id").HasMaxLength(80);
        builder.Property(x => x.요청일시Utc).HasColumnName("requested_at_utc");
        builder.Property(x => x.완료일시Utc).HasColumnName("completed_at_utc");

        builder.HasIndex(x => x.구매Id).IsUnique();
        builder.HasIndex(x => new { x.상세Id, x.구매자UserId }).IsUnique();
        builder.HasIndex(x => x.멱등성Key).IsUnique();
        builder.HasIndex(x => x.결제Id).IsUnique();
        builder.HasIndex(x => x.열람권Id).IsUnique();
        builder.HasOne(x => x.상세)
            .WithMany(x => x.구매목록)
            .HasPrincipalKey(x => x.상세Id)
            .HasForeignKey(x => x.상세Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class 커뮤니티활동상세구매상태이력Configuration : IEntityTypeConfiguration<커뮤니티활동상세구매상태이력>
{
    public void Configure(EntityTypeBuilder<커뮤니티활동상세구매상태이력> builder)
    {
        builder.ToTable("community_activity_detail_purchase_status_history");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.구매Id).HasColumnName("purchase_id").HasMaxLength(80).IsRequired();
        builder.Property(x => x.순서).HasColumnName("sequence");
        builder.Property(x => x.상태).HasColumnName("status").HasMaxLength(40).IsRequired();
        builder.Property(x => x.사유Code).HasColumnName("reason_code").HasMaxLength(80).IsRequired();
        builder.Property(x => x.기록일시Utc).HasColumnName("recorded_at_utc");

        builder.HasIndex(x => new { x.구매Id, x.순서 }).IsUnique();
        builder.HasOne(x => x.구매)
            .WithMany(x => x.상태이력)
            .HasPrincipalKey(x => x.구매Id)
            .HasForeignKey(x => x.구매Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
