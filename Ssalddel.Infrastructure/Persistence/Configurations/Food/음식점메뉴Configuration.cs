using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.음식;

namespace 살뜰.Infrastructure.Persistence.Configurations.Food;

public sealed class 음식점메뉴Configuration : IEntityTypeConfiguration<음식점메뉴>
{
    public void Configure(EntityTypeBuilder<음식점메뉴> builder)
    {
        builder.HasIndex(item => new { item.음식점공개프로필Id, item.메뉴명 }).IsUnique();
        builder.HasIndex(item => new { item.음식점공개프로필Id, item.공개여부, item.표시순서 });
    }
}
