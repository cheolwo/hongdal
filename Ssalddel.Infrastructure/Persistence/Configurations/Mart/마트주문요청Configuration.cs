using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.마트;

namespace 살뜰.Infrastructure.Persistence.Configurations.Mart;

public sealed class 마트주문요청Configuration : IEntityTypeConfiguration<마트주문요청>
{
    public void Configure(EntityTypeBuilder<마트주문요청> builder)
    {
        builder.HasIndex(item => new { item.요청자UserId, item.클라이언트요청Id }).IsUnique();
        builder.HasIndex(item => new { item.요청자UserId, item.CreatedAtUtc });
        builder.HasIndex(item => new { item.공개상품Id, item.CreatedAtUtc });
    }
}
