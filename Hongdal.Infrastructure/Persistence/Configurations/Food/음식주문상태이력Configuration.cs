using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.음식;

namespace 홍달.Infrastructure.Persistence.Configurations.Food;

public sealed class 음식주문상태이력Configuration : IEntityTypeConfiguration<음식주문상태이력>
{
    public void Configure(EntityTypeBuilder<음식주문상태이력> builder)
    {
        builder.ToTable("음식주문상태이력");

        builder.HasIndex(x => new { x.음식주문Id, x.전이시각Utc });
    }
}
