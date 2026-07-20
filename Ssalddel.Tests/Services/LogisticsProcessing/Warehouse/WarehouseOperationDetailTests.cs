using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Warehouse;
using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Inventory;
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

    [Fact]
    public async Task 현장입고요청은_같은클라이언트Id로한번만저장하고_재고를만들지않는다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("warehouse-owner"),
            null!);
        var request = BuildUnplannedRequest();

        var created = await service.CreateUnplannedInboundRequestAsync(request, default);
        var retried = await service.CreateUnplannedInboundRequestAsync(request, default);
        var reloaded = await service.GetInboundAsync(created.Id, default);

        Assert.Equal(created.Id, retried.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(created.Id, reloaded.Id);
        Assert.Equal(입고상태코드.예정, reloaded.상태);
        Assert.Equal(입고흐름유형코드.현장임시입고, reloaded.입고흐름유형);
        Assert.Equal("SKU:FIELD-001", reloaded.예정SKU);
        Assert.Equal("BND:FIELD-001", reloaded.입고묶음바코드);
        Assert.Equal("냉장", reloaded.보관조건);
        Assert.Equal(현장입고요청안내.현재버전, reloaded.안내버전);
        Assert.Equal(1, await db.입고요청.CountAsync(item =>
            item.현장입고클라이언트요청Id == request.클라이언트요청Id));
        Assert.Equal(0, await db.입고상품.CountAsync());
    }

    [Fact]
    public async Task 현장입고요청은_같은클라이언트Id의다른내용을거부한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("warehouse-owner"),
            null!);
        var request = BuildUnplannedRequest();
        await service.CreateUnplannedInboundRequestAsync(request, default);
        request.입고수량++;

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateUnplannedInboundRequestAsync(request, default));

        Assert.Contains("이미 사용한", error.Message);
        Assert.Equal(1, await db.입고요청.CountAsync(item =>
            item.현장입고클라이언트요청Id == request.클라이언트요청Id));
    }

    [Fact]
    public async Task 현장입고요청은_현재안내확인과묶음바코드를필수로검증한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("warehouse-owner"),
            null!);
        var request = BuildUnplannedRequest();
        request.임시입고안내확인 = false;

        var noticeError = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateUnplannedInboundRequestAsync(request, default));

        request.임시입고안내확인 = true;
        request.입고묶음바코드 = "BOX:FIELD-001";
        var barcodeError = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateUnplannedInboundRequestAsync(request, default));

        Assert.Contains("안내", noticeError.Message);
        Assert.Contains("BND", barcodeError.Message);
        Assert.Equal(0, await db.입고요청.CountAsync(item =>
            item.현장입고클라이언트요청Id == request.클라이언트요청Id));
    }

    [Fact]
    public async Task 현장입고요청은_접근할수없는창고에저장하지않는다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("other-user"),
            null!);
        var request = BuildUnplannedRequest();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateUnplannedInboundRequestAsync(request, default));

        Assert.Contains("접근", error.Message);
        Assert.Equal(0, await db.입고요청.CountAsync(item =>
            item.현장입고클라이언트요청Id == request.클라이언트요청Id));
    }

    [Fact]
    public async Task 입고목록은_선택창고의정확한Sku와예정상태만조회한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("warehouse-owner"),
            null!);
        var created = await service.CreateUnplannedInboundRequestAsync(BuildUnplannedRequest(), default);

        var result = await service.QueryInboundsAsync(new 입고요청목록조회요청
        {
            WarehouseId = 7,
            Status = 입고상태코드.예정,
            Sku = " sku:field-001 "
        }, default);

        var item = Assert.Single(result.Items);
        Assert.Equal(created.Id, item.Id);
        Assert.Equal("SKU:FIELD-001", item.예정SKU);
    }

    [Fact]
    public async Task 입고검수대상은_접근가능한보관중상품만조회하고_정확한Id상세를반환한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        await SeedInspectionItemAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("warehouse-owner"),
            null!);

        var list = await service.QueryInboundInspectionTargetsAsync(new 입고검수대상목록조회요청
        {
            InspectionStatus = 입고검수조회상태코드.대기,
            Search = " potato "
        }, default);
        var detail = await service.GetInboundInspectionTargetAsync(71, default);

        var item = Assert.Single(list.Items);
        Assert.Equal(71, item.InboundItemId);
        Assert.True(item.CanInspect);
        Assert.Equal("공동 창고", item.WarehouseName);
        Assert.NotNull(detail);
        Assert.Equal(71, detail.InboundItemId);
        Assert.Equal("냉장", detail.StorageCondition);
        Assert.Equal("공급사", detail.SupplierName);
        Assert.True(detail.CanInspect);
    }

    [Fact]
    public async Task 입고검수상세는_권한밖Id를숨기고_UseCase가404로분류한다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        await SeedInspectionItemAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("other-user"),
            null!);
        var useCase = new 창고작업UseCase(service, null!, null!);

        var hidden = await service.GetInboundInspectionTargetAsync(71, default);
        var result = await useCase.입고검수대상상세Async(71, default);

        Assert.Null(hidden);
        Assert.True(result.IsFailed);
        Assert.Equal(StatusCodes.Status404NotFound, result.Errors[0].Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 입고검수는_상품소유만으로는조회하거나처리할수없다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        await SeedInspectionItemAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("orderer-1"),
            null!);

        var hidden = await service.GetInboundInspectionTargetAsync(71, default);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InspectInboundItemAsync(71, new 입고검수요청 { 검수수량 = 12 }, default));

        Assert.Null(hidden);
        Assert.Contains("접근", error.Message);
        Assert.Equal("보관중", (await db.입고상품.SingleAsync(x => x.Id == 71)).상태);
    }

    [Fact]
    public async Task 입고검수는_보관중상품을한번만변경하고_같은결과재시도는멱등하다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        await SeedInspectionItemAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("warehouse-owner"),
            null!);
        var request = new 입고검수요청
        {
            검수수량 = 12,
            불량수량 = 2,
            검수메모 = "박스 눌림 2개 분리"
        };

        var first = await service.InspectInboundItemAsync(71, request, default);
        var retried = await service.InspectInboundItemAsync(71, request, default);
        var reloaded = await service.GetInboundInspectionTargetAsync(71, default);

        Assert.False(first.멱등재시도여부);
        Assert.True(retried.멱등재시도여부);
        Assert.Equal("검수완료-불량포함", reloaded!.InventoryStatus);
        Assert.Equal(10, reloaded.AvailableQuantity);
        Assert.Equal(2, reloaded.DefectiveQuantity);
        Assert.False(reloaded.CanInspect);
        Assert.Equal(1, await db.재고이력.CountAsync(x => x.입고상품Id == 71 && x.이력유형 == "입고검수"));
    }

    [Fact]
    public async Task 입고검수는_완료된결과를다른수량으로덮어쓰지않는다()
    {
        await using var db = CreateContext();
        await SeedAsync(db);
        await SeedInspectionItemAsync(db);
        var service = new WarehouseOperationService(
            db,
            new TestCurrentUserAccessor("warehouse-owner"),
            null!);
        await service.InspectInboundItemAsync(71, new 입고검수요청 { 검수수량 = 12, 불량수량 = 1 }, default);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.InspectInboundItemAsync(71, new 입고검수요청 { 검수수량 = 11, 불량수량 = 1 }, default));

        Assert.Contains("다른 수량", error.Message);
        Assert.Equal(1, await db.재고이력.CountAsync(x => x.입고상품Id == 71 && x.이력유형 == "입고검수"));
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

    private static async Task SeedInspectionItemAsync(SsalddelContext db)
    {
        var inbound = await db.입고요청.SingleAsync(x => x.Id == 41);
        inbound.상태 = 입고상태코드.완료;
        inbound.보관조건 = 현장입고보관조건.냉장;
        inbound.입고완료일시 = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc);
        db.입고상품.Add(new 입고상품
        {
            Id = 71,
            입고요청Id = inbound.Id,
            창고Id = inbound.창고Id,
            소유자UserId = "orderer-1",
            판매자UserId = "seller-1",
            상품명 = "감자",
            SKU = "POTATO-01",
            옵션명 = "10kg",
            입고수량 = 12,
            가용수량 = 12,
            불량수량 = 0,
            상태 = "보관중",
            보관위치 = "검수 대기 구역",
            입고완료일시 = inbound.입고완료일시,
            CreatedAt = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc)
        });
        await db.SaveChangesAsync();
    }

    private static 현장입고요청등록요청 BuildUnplannedRequest()
        => new()
        {
            클라이언트요청Id = Guid.NewGuid(),
            창고Id = 7,
            상품바코드 = " sku:field-001 ",
            입고묶음바코드 = " bnd:field-001 ",
            상품명 = "현장 반입 냉장 상품",
            공급처명 = "현장 공급처",
            입고수량 = 8,
            보관조건 = 현장입고보관조건.냉장,
            현장입고사유 = "입고 예정 연결 전 현장 선반입",
            임시입고안내확인 = true,
            안내버전 = 현장입고요청안내.현재버전
        };

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
