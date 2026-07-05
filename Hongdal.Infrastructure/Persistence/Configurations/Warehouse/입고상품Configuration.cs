using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.창고;

namespace 홍달.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 입고상품Configuration : IEntityTypeConfiguration<입고상품>
{
    public void Configure(EntityTypeBuilder<입고상품> builder)
    {
        builder.HasIndex(x => new { x.창고Id, x.소유자UserId, x.상태 });
    }
}
