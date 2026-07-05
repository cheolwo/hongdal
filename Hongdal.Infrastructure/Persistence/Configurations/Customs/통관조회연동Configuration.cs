using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.통관;

namespace 홍달.Infrastructure.Persistence.Configurations.Customs;

public sealed class 통관조회연동Configuration : IEntityTypeConfiguration<통관조회연동>
{
    public void Configure(EntityTypeBuilder<통관조회연동> builder)
    {
        builder.Property(x => x.연동상태)
            .HasConversion<int>();

        builder.Property(x => x.마지막진행단계)
            .HasConversion<int>();

        builder.HasIndex(x => new { x.주문Id, x.사용자Id })
            .IsUnique();

        builder.HasIndex(x => new { x.통관절차Id, x.연동상태, x.마지막조회시각 });

        builder.HasIndex(x => new { x.화물관리번호, x.MasterBl, x.HouseBl });
    }
}
