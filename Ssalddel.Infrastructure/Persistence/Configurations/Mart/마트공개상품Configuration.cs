using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.마트;

namespace 살뜰.Infrastructure.Persistence.Configurations.Mart;

public sealed class 마트공개상품Configuration : IEntityTypeConfiguration<마트공개상품>
{
    public void Configure(EntityTypeBuilder<마트공개상품> builder)
    {
        builder.HasIndex(item => item.판매상품Id).IsUnique();
        builder.HasIndex(item => new { item.공개여부, item.판매허용여부, item.UpdatedAtUtc });
        builder.HasIndex(item => new { item.카테고리, item.상품명 });
    }
}
