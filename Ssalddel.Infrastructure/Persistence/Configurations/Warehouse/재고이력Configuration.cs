using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.창고;

namespace 살뜰.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 재고이력Configuration : IEntityTypeConfiguration<재고이력>
{
    public void Configure(EntityTypeBuilder<재고이력> builder)
    {
        builder.HasIndex(x => new { x.입고상품Id, x.처리일시 });
    }
}
