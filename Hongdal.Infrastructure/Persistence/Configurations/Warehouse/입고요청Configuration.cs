using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.창고;

namespace 홍달.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 입고요청Configuration : IEntityTypeConfiguration<입고요청>
{
    public void Configure(EntityTypeBuilder<입고요청> builder)
    {
        builder.HasIndex(x => new { x.창고Id, x.주문자UserId, x.상태 });
        builder.HasIndex(x => new { x.주문Id, x.주문자UserId });
        builder.HasIndex(x => x.출고예정Id);
    }
}
