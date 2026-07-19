using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.판매;

namespace 살뜰.Infrastructure.Persistence.Configurations.Sales;

public sealed class 채널출품Configuration : IEntityTypeConfiguration<채널출품>
{
    public void Configure(EntityTypeBuilder<채널출품> builder)
    {
        builder.HasIndex(x => new { x.판매상품Id, x.판매채널계정Id }).IsUnique();
    }
}
