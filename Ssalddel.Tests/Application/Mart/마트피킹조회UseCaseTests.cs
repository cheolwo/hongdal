using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Mart;
using Ssalddel.Contracts.Mart;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.마트;
using 살뜰.도메인.창고;

namespace Ssalddel.Tests.Application.Mart;

public sealed class 마트피킹조회UseCaseTests
{
    [Fact]
    public async Task 목록은_소유창고작업이있는마트주문만반환하고전체작업진척을계산한다()
    {
        await using var context = CreateContext();
        var seeded = await SeedAsync(context);
        var useCase = new 마트피킹조회UseCase(
            context,
            new FakeCurrentUserAccessor("owner-a", 역할명.창고관리자));

        var result = await useCase.목록Async(new 마트피킹주문목록조회요청
        {
            검색어 = "생수",
            작업상태 = 마트피킹작업상태코드.대기,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var order = Assert.Single(result.Value.Items);
        Assert.Equal(seeded.OwnOrderId, order.주문Id);
        Assert.Equal(2, order.작업수);
        Assert.Equal(1, order.완료작업수);
        Assert.Equal(5, order.작업수량);
        Assert.Equal(2, order.완료작업수량);
        Assert.Equal(["도심 A 창고"], order.창고목록);
    }

    [Fact]
    public async Task 상세는_정확한OrderId만조회하고접근불가주문은404로숨긴다()
    {
        await using var context = CreateContext();
        var seeded = await SeedAsync(context);
        var useCase = new 마트피킹조회UseCase(
            context,
            new FakeCurrentUserAccessor("owner-a", 역할명.창고관리자));

        var found = await useCase.상세Async(seeded.OwnOrderId, CancellationToken.None);
        var hidden = await useCase.상세Async(seeded.OtherOrderId, CancellationToken.None);

        Assert.True(found.IsSuccess);
        Assert.Equal(seeded.OwnOrderId, found.Value.주문Id);
        Assert.Equal("MART-A", found.Value.주문참조번호);
        Assert.Equal(2, found.Value.작업목록.Count);
        Assert.All(found.Value.작업목록, task => Assert.Equal(seeded.OwnWarehouseId, task.창고Id));
        Assert.True(hidden.IsFailed);
        Assert.Equal(404, hidden.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 창고배정사용자와직접담당작업자는각자범위의주문을조회한다()
    {
        await using var context = CreateContext();
        var seeded = await SeedAsync(context);
        context.창고사용자.Add(new 창고사용자
        {
            창고Id = seeded.OtherWarehouseId,
            UserId = "member-b",
            역할명 = "피킹"
        });
        await context.SaveChangesAsync();

        var memberUseCase = new 마트피킹조회UseCase(
            context,
            new FakeCurrentUserAccessor("member-b", 역할명.창고관리자));
        var workerUseCase = new 마트피킹조회UseCase(
            context,
            new FakeCurrentUserAccessor("worker-a", 역할명.창고관리자));

        var memberResult = await memberUseCase.목록Async(new(), CancellationToken.None);
        var workerResult = await workerUseCase.목록Async(new(), CancellationToken.None);

        Assert.Equal(seeded.OtherOrderId, Assert.Single(memberResult.Value.Items).주문Id);
        Assert.Equal(seeded.OwnOrderId, Assert.Single(workerResult.Value.Items).주문Id);
    }

    [Fact]
    public async Task 인증사용자Id가없으면401결과를반환한다()
    {
        await using var context = CreateContext();
        var useCase = new 마트피킹조회UseCase(
            context,
            new FakeCurrentUserAccessor(null, null));

        var result = await useCase.목록Async(new(), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(401, result.Errors.Single().Metadata["StatusCode"]);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static async Task<SeededIds> SeedAsync(SsalddelContext context)
    {
        var now = new DateTime(2026, 7, 20, 9, 0, 0, DateTimeKind.Utc);
        var ownWarehouse = new 창고
        {
            소유자UserId = "owner-a",
            창고명 = "도심 A 창고",
            CreatedAt = now,
            UpdatedAt = now
        };
        var otherWarehouse = new 창고
        {
            소유자UserId = "owner-b",
            창고명 = "도심 B 창고",
            CreatedAt = now,
            UpdatedAt = now
        };
        context.창고.AddRange(ownWarehouse, otherWarehouse);
        await context.SaveChangesAsync();

        var ownOrder = new 마트주문
        {
            주문참조번호 = "MART-A",
            주문자UserId = "buyer-a",
            판매자UserId = "seller-a",
            상태 = "출고 예정",
            현재단계 = "피킹",
            CreatedAt = now,
            UpdatedAt = now,
            상품목록 =
            [
                new 마트주문상품 { 상품명 = "생수 6입", SKU = "WATER-6", 수량 = 3, 상태 = "출고 예정" }
            ]
        };
        var otherOrder = new 마트주문
        {
            주문참조번호 = "MART-B",
            주문자UserId = "buyer-b",
            판매자UserId = "seller-b",
            상태 = "출고 예정",
            현재단계 = "피킹",
            CreatedAt = now,
            UpdatedAt = now,
            상품목록 =
            [
                new 마트주문상품 { 상품명 = "휴지", SKU = "TISSUE", 수량 = 1, 상태 = "출고 예정" }
            ]
        };
        context.마트주문.AddRange(ownOrder, otherOrder);
        await context.SaveChangesAsync();

        context.피킹포장작업.AddRange(
            new 피킹포장작업
            {
                작업Key = "PICK-A",
                작업유형 = 피킹포장작업유형.피킹,
                처리방식 = "피킹포장분리",
                상태 = 피킹포장작업상태.대기,
                창고Id = ownWarehouse.Id,
                창고명 = ownWarehouse.창고명,
                작업자UserId = "worker-a",
                작업자표시명 = "작업자 A",
                주문참조번호 = ownOrder.주문참조번호,
                라인Key = "LINE-A",
                상품명 = "생수 6입",
                SKU = "WATER-6",
                수량 = 3,
                CreatedAt = now,
                UpdatedAt = now
            },
            new 피킹포장작업
            {
                작업Key = "PACK-A",
                작업유형 = 피킹포장작업유형.포장,
                처리방식 = "피킹포장분리",
                상태 = 피킹포장작업상태.완료,
                창고Id = ownWarehouse.Id,
                창고명 = ownWarehouse.창고명,
                작업자UserId = "packer-a",
                작업자표시명 = "포장 A",
                상대작업자UserId = "worker-a",
                주문참조번호 = ownOrder.주문참조번호,
                라인Key = "LINE-A",
                상품명 = "생수 6입",
                SKU = "WATER-6",
                수량 = 2,
                CreatedAt = now,
                UpdatedAt = now.AddMinutes(10)
            },
            new 피킹포장작업
            {
                작업Key = "PICK-B",
                작업유형 = 피킹포장작업유형.피킹,
                처리방식 = "피킹포장통합",
                상태 = 피킹포장작업상태.대기,
                창고Id = otherWarehouse.Id,
                창고명 = otherWarehouse.창고명,
                작업자UserId = "worker-b",
                작업자표시명 = "작업자 B",
                주문참조번호 = otherOrder.주문참조번호,
                라인Key = "LINE-B",
                상품명 = "휴지",
                SKU = "TISSUE",
                수량 = 1,
                CreatedAt = now,
                UpdatedAt = now
            });
        await context.SaveChangesAsync();

        return new SeededIds(ownOrder.Id, otherOrder.Id, ownWarehouse.Id, otherWarehouse.Id);
    }

    private sealed record SeededIds(
        long OwnOrderId,
        long OtherOrderId,
        long OwnWarehouseId,
        long OtherWarehouseId);

    private sealed class FakeCurrentUserAccessor(string? userId, string? role) : ICurrentUserAccessor
    {
        public string? UserId { get; } = userId;
        public string? Role { get; } = role;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
