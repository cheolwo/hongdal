using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Warehouse;
using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Services.LogisticsProcessing.Warehouse;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.창고;

namespace Ssalddel.Tests.Services.LogisticsProcessing.Warehouse;

public sealed class WarehouseOperationDetailTests
{
    [Fact]
    public async Task 입고상세는_접근가능한같은Id와예정상품을투영한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("warehouse-owner"),
            null!);

        var result = await service.GetInboundAsync(41, default);

        Assert.NotNull(result);
        Assert.Equal(41, result.Id);
        Assert.Equal("공급사", result.공급처명);
        Assert.Equal("감자", result.예정상품명);
        Assert.Equal("POTATO-01", result.예정SKU);
        Assert.Equal(12, result.예정수량);
    }

    [Fact]
    public async Task 입고상세는_권한밖Id를숨기고_UseCase가404로분류한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("other-user"),
            null!);
        var useCase = new 창고작업UseCase(service, null!, null!);

        var hidden = await service.GetInboundAsync(41, default);
        var result = await useCase.입고상세Async(41, default);

        Assert.Null(hidden);
        Assert.True(result.IsFailed);
        Assert.Equal(StatusCodes.Status404NotFound, result.Errors[0].Metadata["StatusCode"]);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"warehouse-operation-detail-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static async Task SeedAsync(SsalddelContext db)
    {
        db.창고.Add(new 창고
        {
            Id = 7,
            소유자UserId = "warehouse-owner",
            창고명 = "공동 창고",
            주소 = "서울"
        });
        db.입고요청.Add(new 입고요청
        {
            Id = 41,
            창고Id = 7,
            주문자UserId = "orderer-1",
            판매자UserId = "seller-1",
            공급처코드 = "SUP-01",
            공급처명 = "공급사",
            상태 = 입고상태코드.운송중,
            예정도착일 = new DateTime(2026, 7, 21, 9, 0, 0)
        });
        db.출고예정.Add(new 출고예정
        {
            Id = 51,
            입고요청Id = 41,
            출고창고Id = 7,
            상품명 = "감자",
            SKU = "POTATO-01",
            수량 = 12
        });
        await db.SaveChangesAsync();
    }

    private sealed record TestCurrentUserAccessor(string? UserId) : ICurrentUserAccessor
    {
        public string? Role => null;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
