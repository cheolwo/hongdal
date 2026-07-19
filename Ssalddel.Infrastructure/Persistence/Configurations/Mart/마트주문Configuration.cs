using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.마트;

namespace 살뜰.Infrastructure.Persistence.Configurations.Mart;

public sealed class 마트주문Configuration : IEntityTypeConfiguration<마트주문>
{
    public void Configure(EntityTypeBuilder<마트주문> builder)
    {
        builder.ToTable("마트주문");

        builder.HasIndex(x => x.주문참조번호).IsUnique();
        builder.HasIndex(x => new { x.주문자UserId, x.상태, x.CreatedAt });
        builder.HasIndex(x => new { x.판매자UserId, x.상태, x.CreatedAt });
        builder.HasIndex(x => x.커뮤니티원장Id);

        builder.HasMany(x => x.상품목록)
            .WithOne(x => x.마트주문)
            .HasForeignKey(x => x.마트주문Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
