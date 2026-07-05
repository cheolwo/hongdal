using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.판매;

namespace 홍달.Infrastructure.Persistence.Configurations.Sales;

public sealed class 상품식별코드맵Configuration : IEntityTypeConfiguration<상품식별코드맵>
{
    public void Configure(EntityTypeBuilder<상품식별코드맵> builder)
    {
        builder.Property(x => x.코드유형)
            .HasConversion<int>();

        builder.HasIndex(x => x.코드값)
            .IsUnique();

        builder.HasIndex(x => new { x.상품Id, x.활성여부 });
    }
}
