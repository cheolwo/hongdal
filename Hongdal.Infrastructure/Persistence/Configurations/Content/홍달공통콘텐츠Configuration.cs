using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.공통콘텐츠;

namespace 홍달.Infrastructure.Persistence.Configurations.Content;

public sealed class 홍달공통콘텐츠Configuration : IEntityTypeConfiguration<홍달공통콘텐츠>
{
    public void Configure(EntityTypeBuilder<홍달공통콘텐츠> builder)
    {
        builder.Property(x => x.노출위치).HasConversion<int>();
        builder.HasIndex(x => new { x.활성화여부, x.노출시작시각, x.노출종료시각 });

        builder.HasOne(x => x.보상정책)
            .WithMany()
            .HasForeignKey(x => x.보상정책Id)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
