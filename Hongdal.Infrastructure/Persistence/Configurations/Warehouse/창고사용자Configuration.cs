using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.창고;

namespace 홍달.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 창고사용자Configuration : IEntityTypeConfiguration<창고사용자>
{
    public void Configure(EntityTypeBuilder<창고사용자> builder)
    {
        builder.HasIndex(x => new { x.창고Id, x.UserId, x.역할명 }).IsUnique();
    }
}
