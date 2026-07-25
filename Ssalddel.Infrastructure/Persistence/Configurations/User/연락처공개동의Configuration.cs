using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.사용자;

namespace 살뜰.Infrastructure.Persistence.Configurations.User;

public sealed class 연락처공개동의Configuration : IEntityTypeConfiguration<연락처공개동의>
{
    public void Configure(EntityTypeBuilder<연락처공개동의> builder)
    {
        builder.Property(x => x.친구요청Id)
            .HasColumnName("인연연결요청_id");

        builder.HasIndex(x => new { x.친구요청Id, x.동의자참여자Id })
            .IsUnique();

        builder.HasIndex(x => new { x.동의자참여자Id, x.동의일시 });
    }
}
