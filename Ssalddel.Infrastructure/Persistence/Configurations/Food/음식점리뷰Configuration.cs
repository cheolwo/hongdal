using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.음식;

namespace 살뜰.Infrastructure.Persistence.Configurations.Food;

public sealed class 음식점리뷰Configuration : IEntityTypeConfiguration<음식점리뷰>
{
    public void Configure(EntityTypeBuilder<음식점리뷰> builder)
    {
        builder.HasIndex(item => item.주문번호).IsUnique();
        builder.HasIndex(item => new { item.음식점Id, item.현재노출여부, item.CreatedAtUtc });
        builder.HasIndex(item => new { item.관리자검토필요여부, item.CreatedAtUtc });
    }
}
