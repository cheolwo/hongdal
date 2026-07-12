using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.사용자;

namespace 홍달.Infrastructure.Persistence.Configurations.User;

public sealed class 주문자프로필Configuration : IEntityTypeConfiguration<주문자프로필>
{
    public void Configure(EntityTypeBuilder<주문자프로필> builder)
    {
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
