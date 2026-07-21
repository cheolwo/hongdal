using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.창고;

namespace 살뜰.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 입고상품Configuration : IEntityTypeConfiguration<입고상품>
{
    public void Configure(EntityTypeBuilder<입고상품> builder)
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

        // 입고요청과 재고 aggregate는 수명주기가 달라 navigation/FK로 묶지 않지만,
        // 완료 재시도와 원장 추적은 이 scalar reference로 조회하므로 단일 인덱스를 둡니다.
        builder.HasIndex(x => x.입고요청Id);
        builder.HasIndex(x => x.커뮤니티원장Id);
        builder.HasIndex(x => new { x.창고Id, x.소유자UserId, x.상태 });
    }
}
