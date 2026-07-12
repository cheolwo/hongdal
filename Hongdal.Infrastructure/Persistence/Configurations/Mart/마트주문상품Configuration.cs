using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.마트;

namespace 홍달.Infrastructure.Persistence.Configurations.Mart;

public sealed class 마트주문상품Configuration : IEntityTypeConfiguration<마트주문상품>
{
    public void Configure(EntityTypeBuilder<마트주문상품> builder)
    {
        builder.ToTable("마트주문상품");

        builder.HasIndex(x => new { x.마트주문Id, x.출고예정Id }).IsUnique();
        builder.HasIndex(x => x.출고예정Id);
        builder.HasIndex(x => x.SKU);
    }
}
