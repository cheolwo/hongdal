using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.사용자;

namespace 살뜰.Infrastructure.Persistence.Configurations.User;

public sealed class 친구요청Configuration : IEntityTypeConfiguration<친구요청>
{
    public void Configure(EntityTypeBuilder<친구요청> builder)
    {
        builder.ToTable("인연연결요청");

        builder.Property(x => x.요청자역할)
            .HasConversion<int>();

        builder.Property(x => x.대상자역할)
            .HasConversion<int>();

        builder.Property(x => x.상태)
            .HasConversion<int>();

        builder.HasIndex(x => new { x.요청자참여자Id, x.상태, x.요청일시 });
        builder.HasIndex(x => new { x.대상자참여자Id, x.상태, x.요청일시 });
        builder.HasIndex(x => new { x.감사메시지Id, x.주문Id, x.통관절차Id });
    }
}
