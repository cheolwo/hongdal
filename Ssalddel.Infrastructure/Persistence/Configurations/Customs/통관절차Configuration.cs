using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 살뜰.도메인.통관;

namespace 살뜰.Infrastructure.Persistence.Configurations.Customs;

public sealed class 통관절차Configuration : IEntityTypeConfiguration<통관절차>
{
    public void Configure(EntityTypeBuilder<통관절차> builder)
    {
        builder.Property(x => x.물류거래방향).HasConversion<int>();
        builder.Property(x => x.상태).HasConversion<int>();

        builder.HasIndex(x => new { x.주문Id, x.주문참조번호, x.상태 });
        builder.HasIndex(x => new { x.출고예정Id, x.입고요청Id });
    }
}
