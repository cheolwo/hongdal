using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.창고;

namespace 살뜰.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 재고이동Configuration : IEntityTypeConfiguration<재고이동>
{
    public void Configure(EntityTypeBuilder<재고이동> builder)
    {
        // 재고이동은 입고상품과 별도 append-only 원장이므로 FK/navigation은 두지 않되,
        // 한 재고 단위의 이동 근거를 시간순으로 재구성할 수 있게 scalar reference를 인덱싱합니다.
        builder.HasIndex(x => new { x.입고상품Id, x.발생일시 });
        builder.HasIndex(x => new { x.창고Id, x.SKU, x.발생일시 });
        builder.HasIndex(x => new { x.주문Id, x.이동유형 });
    }
}
