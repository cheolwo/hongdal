using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Contracts.Common.Metadata;
using 살뜰.도메인.공급중개;

namespace 살뜰.Data.Configurations.ContractManagement;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformSupplyBrokerage,
    SsalddelCodeLayer.Infrastructure,
    "공급조건 계약·이용등록·개별발주의 관계와 멱등성 제약을 구성합니다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    FlowOrder = 40,
    Boundary = "공급중개 원장만 구성하며 기존 판매·결제·재고·입고 원장과의 자동 관계를 만들지 않습니다.")]
public sealed class 플랫폼공급조건계약Configuration : IEntityTypeConfiguration<플랫폼공급조건계약>
{
    public void Configure(EntityTypeBuilder<플랫폼공급조건계약> builder)
    {
        builder.HasIndex(item => item.계약번호).IsUnique();
        builder.HasIndex(item => new { item.생성자UserId, item.클라이언트요청Id }).IsUnique();
        builder.HasIndex(item => new { item.상태코드, item.유효시작Utc, item.유효종료Utc });

        builder.HasMany(item => item.품목목록)
            .WithOne(item => item.공급계약)
            .HasForeignKey(item => item.공급계약Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class 플랫폼공급조건계약품목Configuration : IEntityTypeConfiguration<플랫폼공급조건계약품목>
{
    public void Configure(EntityTypeBuilder<플랫폼공급조건계약품목> builder)
    {
        builder.HasIndex(item => new { item.공급계약Id, item.계약품목Key }).IsUnique();
        builder.HasIndex(item => new { item.공급계약Id, item.SKU });
    }
}

public sealed class 공급계약이용등록Configuration : IEntityTypeConfiguration<공급계약이용등록>
{
    public void Configure(EntityTypeBuilder<공급계약이용등록> builder)
    {
        builder.HasIndex(item => new
        {
            item.조직유형코드,
            item.조직참조Key,
            item.클라이언트요청Id
        }).IsUnique();
        builder.HasIndex(item => new
        {
            item.공급계약Id,
            item.조직유형코드,
            item.조직참조Key
        }).IsUnique();

        builder.HasOne(item => item.공급계약)
            .WithMany()
            .HasForeignKey(item => item.공급계약Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class 조직개별공급발주Configuration : IEntityTypeConfiguration<조직개별공급발주>
{
    public void Configure(EntityTypeBuilder<조직개별공급발주> builder)
    {
        builder.HasIndex(item => new
        {
            item.구매조직유형코드,
            item.구매조직참조Key,
            item.클라이언트요청Id
        }).IsUnique();
        builder.HasIndex(item => new
        {
            item.구매조직유형코드,
            item.구매조직참조Key,
            item.상태코드,
            item.제출시각Utc
        });

        builder.HasOne(item => item.공급계약이용등록)
            .WithMany()
            .HasForeignKey(item => item.공급계약이용등록Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(item => item.공급계약)
            .WithMany()
            .HasForeignKey(item => item.공급계약Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(item => item.공급계약품목)
            .WithMany()
            .HasForeignKey(item => item.공급계약품목Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
