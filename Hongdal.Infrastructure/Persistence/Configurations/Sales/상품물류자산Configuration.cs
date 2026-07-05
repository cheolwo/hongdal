using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.판매;

namespace 홍달.Infrastructure.Persistence.Configurations.Sales;

public sealed class 상품물류자산Configuration : IEntityTypeConfiguration<상품물류자산>
{
    public void Configure(EntityTypeBuilder<상품물류자산> builder)
    {
        builder.Property(x => x.자산유형)
            .HasConversion<int>();

        builder.HasIndex(x => new { x.상품Id, x.자산유형, x.등록시각 });
        builder.HasIndex(x => new { x.주문Id, x.통관절차Id });
    }
}
