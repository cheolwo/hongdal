using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Sales;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Services.LogisticsProcessing.SalesOrders;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.창고;

namespace Ssalddel.Tests.Services.LogisticsProcessing.SalesOrders;

public sealed class SalesChannelOrderReadServiceTests
{
    [Fact]
    public async Task 판매자는_자기판매채널출고후보를주문별로묶어조회한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var service = new SalesChannelOrderReadService(
            db,
            new TestCurrentUserAccessor("seller-1", "판매자"));

        var result = await service.QueryAsync(new 판매채널주문목록조회요청(), default);
        var overseas = await service.QueryAsync(new 판매채널주문목록조회요청
        {
            SyncScope = CommerceChannelOrderSyncScopes.Overseas
        }, default);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        var amazon = Assert.Single(overseas.Items);
        Assert.Equal(101, amazon.OrderId);
        Assert.Equal(CommerceChannelKeys.Amazon, amazon.채널종류);
        Assert.Equal("A-100", amazon.채널주문번호);
        Assert.Equal(2, amazon.출고라인수);
        Assert.Equal(5, amazon.총수량);
        Assert.Equal("김포 허브", amazon.출고창고표시);
    }

    [Fact]
    public async Task 상세는_같은주문라인만반환하고_다른판매자Id는404로숨긴다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var service = new SalesChannelOrderReadService(
            db,
            new TestCurrentUserAccessor("seller-1", "판매자"));
        var useCase = new 판매채널주문조회UseCase(service);

        var own = await service.GetAsync(101, default);
        var hidden = await service.GetAsync(201, default);
        var hiddenResult = await useCase.상세Async(201, default);

        Assert.NotNull(own);
        Assert.Equal(101, own.주문.OrderId);
        Assert.Equal(2, own.출고라인목록.Count);
        Assert.All(own.출고라인목록, line => Assert.Equal("김포 허브", line.출고창고명));
        Assert.Null(hidden);
        Assert.True(hiddenResult.IsFailed);
        Assert.Equal(StatusCodes.Status404NotFound, hiddenResult.Errors[0].Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 서버관리자는_같은주문참조라도판매자별원장을합치지않는다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var service = new SalesChannelOrderReadService(
            db,
            new TestCurrentUserAccessor("admin-1", 역할명.서버관리자));

        var result = await service.QueryAsync(new 판매채널주문목록조회요청
        {
            Search = "A-100"
        }, default);

        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, item => item.OrderId == 101 && item.총수량 == 5);
        Assert.Contains(result.Items, item => item.OrderId == 201 && item.총수량 == 9);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"sales-channel-order-read-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static async Task SeedAsync(SsalddelContext db)
    {
        db.창고.Add(new 창고
        {
            Id = 7,
            소유자UserId = "warehouse-owner",
            창고명 = "김포 허브",
            주소 = "화면에 노출하지 않을 전체 주소"
        });
        db.출고예정.AddRange(
            Outbound(101, "seller-1", $"{CommerceChannelKeys.Amazon}:A-100", "캠핑 테이블", "SKU-A", 2),
            Outbound(102, "seller-1", $"{CommerceChannelKeys.Amazon}:A-100", "캠핑 의자", "SKU-B", 3),
            Outbound(103, "seller-1", $"{CommerceChannelKeys.SmartStore}:N-200", "간편식", "SKU-C", 4),
            Outbound(104, "seller-1", "GroupPurchase:GROUP-1", "공동구매 상품", "SKU-D", 5),
            Outbound(201, "seller-2", $"{CommerceChannelKeys.Amazon}:A-100", "다른 판매자 상품", "SKU-E", 9));
        await db.SaveChangesAsync();
    }

    private static 출고예정 Outbound(
        long id,
        string sellerUserId,
        string orderReference,
        string productName,
        string sku,
        int quantity)
        => new()
        {
            Id = id,
            판매자UserId = sellerUserId,
            주문자UserId = "응답에 포함하지 않을 구매자",
            주문참조번호 = orderReference,
            출고창고Id = 7,
            상품명 = productName,
            SKU = sku,
            수량 = quantity,
            상태 = 출고상태.예정,
            CreatedAt = new DateTime(2026, 7, 20, 1, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 7, 20, 2, 0, 0, DateTimeKind.Utc)
        };

    private sealed record TestCurrentUserAccessor(string? UserId, string? Role) : ICurrentUserAccessor;

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
