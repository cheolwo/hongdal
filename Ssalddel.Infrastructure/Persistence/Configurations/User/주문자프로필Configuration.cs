using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.사용자;

namespace 살뜰.Infrastructure.Persistence.Configurations.User;

public sealed class 주문자프로필Configuration : IEntityTypeConfiguration<주문자프로필>
{
    public void Configure(EntityTypeBuilder<주문자프로필> builder)
    {
        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
