using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.창고;

namespace 홍달.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 출고예정Configuration : IEntityTypeConfiguration<출고예정>
{
    public void Configure(EntityTypeBuilder<출고예정> builder)
    {
        builder.HasIndex(x => new { x.판매자UserId, x.상태, x.CreatedAt });
        builder.HasIndex(x => new { x.주문Id, x.판매자UserId });
        builder.HasIndex(x => x.입고요청Id);
    }
}
