using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.사용자;

namespace 홍달.Infrastructure.Persistence.Configurations.User;

public sealed class 홍달참여자역할Configuration : IEntityTypeConfiguration<홍달참여자역할>
{
    public void Configure(EntityTypeBuilder<홍달참여자역할> builder)
    {
        builder.Property(x => x.역할유형).HasConversion<int>();

        builder.HasIndex(x => new { x.참여자Id, x.역할유형, x.활성화여부 });

        builder.HasOne(x => x.참여자)
            .WithMany(x => x.역할목록)
            .HasForeignKey(x => x.참여자Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
