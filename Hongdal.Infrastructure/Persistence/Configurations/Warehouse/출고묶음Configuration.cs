using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.창고;

namespace 홍달.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 출고묶음Configuration : IEntityTypeConfiguration<출고묶음>
{
    public void Configure(EntityTypeBuilder<출고묶음> builder)
    {
        builder.ToTable("출고묶음");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.출고묶음번호).HasColumnName("출고묶음번호").HasMaxLength(100).IsRequired();
        builder.Property(x => x.주문참조번호).HasColumnName("주문참조번호").HasMaxLength(100).IsRequired();
        builder.Property(x => x.출고창고Id).HasColumnName("출고창고_id");
        builder.Property(x => x.판매자UserId).HasColumnName("판매자_user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.주문자UserId).HasColumnName("주문자_user_id").HasMaxLength(450).IsRequired();
        builder.Property(x => x.상태).HasColumnName("상태").HasMaxLength(50).IsRequired();
        builder.Property(x => x.피킹시작일시).HasColumnName("피킹시작일시");
        builder.Property(x => x.피킹완료일시).HasColumnName("피킹완료일시");
        builder.Property(x => x.포장완료일시).HasColumnName("포장완료일시");
        builder.Property(x => x.출고완료일시).HasColumnName("출고완료일시");
        builder.Property(x => x.운송의뢰Id).HasColumnName("운송의뢰_id").HasMaxLength(100);
        builder.Property(x => x.커뮤니티원장Id).HasColumnName("community_ledger_id").HasMaxLength(120);
        builder.Property(x => x.커뮤니티원장템플릿Key).HasColumnName("community_ledger_template_key").HasMaxLength(120);
        builder.Property(x => x.커뮤니티원장상태).HasColumnName("community_ledger_state").HasMaxLength(80);
        builder.Property(x => x.커뮤니티원장동기화시각Utc).HasColumnName("community_ledger_synced_at_utc");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.커뮤니티원장Id);
        builder.HasIndex(x => new { x.출고창고Id, x.상태, x.CreatedAt });
        builder.HasIndex(x => x.출고묶음번호).IsUnique();
        builder.HasIndex(x => x.운송의뢰Id);
    }
}
