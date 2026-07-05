using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.창고;

namespace 홍달.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 재고이동Configuration : IEntityTypeConfiguration<재고이동>
{
    public void Configure(EntityTypeBuilder<재고이동> builder)
    {
        builder.HasIndex(x => new { x.창고Id, x.SKU, x.발생일시 });
        builder.HasIndex(x => new { x.주문Id, x.이동유형 });
    }
}
