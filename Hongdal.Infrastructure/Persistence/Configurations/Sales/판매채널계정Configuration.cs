using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.판매;

namespace 홍달.Infrastructure.Persistence.Configurations.Sales;

public sealed class 판매채널계정Configuration : IEntityTypeConfiguration<판매채널계정>
{
    public void Configure(EntityTypeBuilder<판매채널계정> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.채널종류, x.상점명 });
    }
}
