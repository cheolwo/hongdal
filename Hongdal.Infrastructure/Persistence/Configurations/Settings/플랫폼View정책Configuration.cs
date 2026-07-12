using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.설정;

namespace 홍달.Infrastructure.Persistence.Configurations.Settings;

public sealed class 플랫폼View정책Configuration : IEntityTypeConfiguration<플랫폼View정책>
{
    public void Configure(EntityTypeBuilder<플랫폼View정책> builder)
    {
        builder.HasIndex(x => new { x.AppKey, x.ViewKey, x.RoleName }).IsUnique();
    }
}
