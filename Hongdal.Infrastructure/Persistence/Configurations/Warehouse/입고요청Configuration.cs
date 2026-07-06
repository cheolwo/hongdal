using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using 홍달.도메인.창고;

namespace 홍달.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class 입고요청Configuration : IEntityTypeConfiguration<입고요청>
{
    public void Configure(EntityTypeBuilder<입고요청> builder)
    {
        builder.Property(x => x.입고흐름유형)
            .HasDefaultValue("ContractBased");

        builder.Property(x => x.입고생성경로)
            .HasDefaultValue("계약 DB 기반 등록");

        builder.Property(x => x.계약선행여부)
            .HasDefaultValue(true);

        builder.Property(x => x.자동생성여부)
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.창고Id, x.주문자UserId, x.상태 });
        builder.HasIndex(x => new { x.주문Id, x.주문자UserId });
        builder.HasIndex(x => x.출고예정Id);
        builder.HasIndex(x => new { x.입고흐름유형, x.자동생성여부 });
    }
}
