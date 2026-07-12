using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.창고;

namespace 홍달.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 출고예정Configuration : IEntityTypeConfiguration<출고예정>
{
    public void Configure(EntityTypeBuilder<출고예정> builder)
    {
        builder.Property(x => x.커뮤니티원장Id)
            .HasColumnName("community_ledger_id")
            .HasMaxLength(120);

        builder.Property(x => x.커뮤니티원장템플릿Key)
            .HasColumnName("community_ledger_template_key")
            .HasMaxLength(120);

        builder.Property(x => x.커뮤니티원장상태)
            .HasColumnName("community_ledger_state")
            .HasMaxLength(80);

        builder.Property(x => x.커뮤니티원장동기화시각Utc)
            .HasColumnName("community_ledger_synced_at_utc");

        builder.HasIndex(x => x.커뮤니티원장Id);
        builder.HasIndex(x => new { x.판매자UserId, x.상태, x.CreatedAt });
        builder.HasIndex(x => new { x.주문Id, x.판매자UserId });
        builder.HasIndex(x => x.입고요청Id);
        builder.HasIndex(x => x.출고묶음Id);
    }
}
