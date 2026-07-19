using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.공통콘텐츠;

namespace 살뜰.Infrastructure.Persistence.Configurations.Content;

public sealed class 살뜰콘텐츠시청세션Configuration : IEntityTypeConfiguration<살뜰콘텐츠시청세션>
{
    public void Configure(EntityTypeBuilder<살뜰콘텐츠시청세션> builder)
    {
        builder.HasIndex(x => new { x.사용자Id, x.콘텐츠Id, x.시작시각 });

        builder.HasOne(x => x.콘텐츠)
            .WithMany()
            .HasForeignKey(x => x.콘텐츠Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
