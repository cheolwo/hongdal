using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.판매;

namespace 홍달.Infrastructure.Persistence.Configurations.Sales;

public sealed class 상품판매이미지초안Configuration : IEntityTypeConfiguration<상품판매이미지초안>
{
    public void Configure(EntityTypeBuilder<상품판매이미지초안> builder)
    {
        builder.HasIndex(x => new { x.상품Id, x.생성시각 });
        builder.HasIndex(x => x.생성작업Id)
            .IsUnique();
    }
}
