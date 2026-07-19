using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.사용자;

namespace 살뜰.Infrastructure.Persistence.Configurations.User;

public sealed class 살뜰참여자역할Configuration : IEntityTypeConfiguration<살뜰참여자역할>
{
    public void Configure(EntityTypeBuilder<살뜰참여자역할> builder)
    {
        builder.Property(x => x.역할유형).HasConversion<int>();

        builder.HasIndex(x => new { x.참여자Id, x.역할유형, x.활성화여부 });

        builder.HasOne(x => x.참여자)
            .WithMany(x => x.역할목록)
            .HasForeignKey(x => x.참여자Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
