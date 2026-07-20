using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.음식;

namespace 살뜰.Infrastructure.Persistence.Configurations.Food;

public sealed class 음식점공개프로필Configuration : IEntityTypeConfiguration<음식점공개프로필>
{
    public void Configure(EntityTypeBuilder<음식점공개프로필> builder)
    {
        builder.HasIndex(item => item.업체Id).IsUnique();
        builder.HasIndex(item => new { item.공개여부, item.주문가능여부, item.UpdatedAtUtc });
        builder.HasIndex(item => new { item.위도, item.경도 });

        builder.HasMany(item => item.메뉴목록)
            .WithOne(item => item.음식점공개프로필)
            .HasForeignKey(item => item.음식점공개프로필Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
