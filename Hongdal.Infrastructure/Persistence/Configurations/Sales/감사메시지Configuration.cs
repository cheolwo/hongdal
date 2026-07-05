using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.판매;

namespace 홍달.Infrastructure.Persistence.Configurations.Sales;

public sealed class 감사메시지Configuration : IEntityTypeConfiguration<감사메시지>
{
    public void Configure(EntityTypeBuilder<감사메시지> builder)
    {
        builder.Property(x => x.검수상태)
            .HasConversion<int>();

        builder.HasIndex(x => new { x.상품Id, x.작성일시 });
        builder.HasIndex(x => new { x.대상역할, x.대상참여자Id, x.작성일시 });
        builder.HasIndex(x => new { x.통관절차Id, x.주문Id });
    }
}
