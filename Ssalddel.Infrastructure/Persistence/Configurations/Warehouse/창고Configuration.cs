using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.창고;

namespace 살뜰.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 창고Configuration : IEntityTypeConfiguration<창고>
{
    public void Configure(EntityTypeBuilder<창고> builder)
    {
        builder.HasIndex(x => new { x.소유자UserId, x.창고명 });
        builder.HasIndex(x => new { x.소유자UserId, x.소유자유형, x.기본창고여부 });
        builder.HasIndex(x => x.국가코드);
    }
}
