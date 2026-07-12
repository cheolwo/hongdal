using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.사용자;

namespace 홍달.Infrastructure.Persistence.Configurations.User;

public sealed class 홍달참여자Configuration : IEntityTypeConfiguration<홍달참여자>
{
    public void Configure(EntityTypeBuilder<홍달참여자> builder)
    {
        builder.HasIndex(x => x.활성화여부);
    }
}
