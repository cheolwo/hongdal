using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.판매;

namespace 살뜰.Infrastructure.Persistence.Configurations.Sales;

public sealed class 판매상품Configuration : IEntityTypeConfiguration<판매상품>
{
    public void Configure(EntityTypeBuilder<판매상품> builder)
    {
        builder.HasIndex(x => new { x.입고상품Id, x.판매SKU }).IsUnique();
        builder.HasIndex(x => new { x.샘플데이터여부, x.이미지생성상태, x.UpdatedAt });
    }
}
