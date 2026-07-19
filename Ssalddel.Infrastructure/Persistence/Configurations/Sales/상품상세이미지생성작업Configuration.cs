using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.판매;

namespace 살뜰.Infrastructure.Persistence.Configurations.Sales;

public sealed class 상품상세이미지생성작업Configuration : IEntityTypeConfiguration<상품상세이미지생성작업>
{
    public void Configure(EntityTypeBuilder<상품상세이미지생성작업> builder)
    {
        builder.Property(x => x.상태)
            .HasConversion<int>();

        builder.HasIndex(x => new { x.상품Id, x.상태, x.생성시각 });
        builder.HasIndex(x => x.관련생성이미지작업Id);
    }
}
