using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.음식;

namespace 홍달.Infrastructure.Persistence.Configurations.Food;

public sealed class 음식주문상품Configuration : IEntityTypeConfiguration<음식주문상품>
{
    public void Configure(EntityTypeBuilder<음식주문상품> builder)
    {
        builder.ToTable("음식주문상품");

        builder.HasIndex(x => x.음식주문Id);
        builder.HasIndex(x => x.상품명);
    }
}
