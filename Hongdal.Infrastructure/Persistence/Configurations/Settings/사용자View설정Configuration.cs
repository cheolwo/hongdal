using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.설정;

namespace 홍달.Infrastructure.Persistence.Configurations.Settings;

public sealed class 사용자View설정Configuration : IEntityTypeConfiguration<사용자View설정>
{
    public void Configure(EntityTypeBuilder<사용자View설정> builder)
    {
        builder.HasIndex(x => new { x.UserId, x.AppKey, x.ViewKey }).IsUnique();
    }
}
