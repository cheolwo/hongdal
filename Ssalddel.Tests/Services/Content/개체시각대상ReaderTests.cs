using FluentResults;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Mart;
using Ssalddel.Application.Warehouse;
using Ssalddel.Application.WorldProjection;
using Ssalddel.Contracts.Admin.Content;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Contracts.Mart;
using Ssalddel.Services.AgriculturalFisheries.Information;
using Ssalddel.Services.Content;

namespace Ssalddel.Tests.Services.Content;

public sealed class 개체시각대상ReaderTests
{
    [Theory]
    [InlineData("farm", "farm:1", "Building", "Operating")]
    [InlineData("farm.plot", "plot:1", "Surface", "Reference")]
    [InlineData("farm.cultivation", "crop:1", "Crop", "Growing")]
    [InlineData("warehouse.inventory", "item:1", "Cargo", "Available")]
    [InlineData("mart.product", "1", "Product", "Available")]
    [InlineData("food.product", "product:potato", "Product", "Reference")]
    public async Task 분야별기존권한조회결과를공통선택입력으로만변환한다(string kind, string id, string representation, string state)
    {
        var sources = new Sources();
        var reader = new 개체시각대상Reader(sources, sources, sources, sources, new Current());
        var result = await reader.ReadAsync(new(kind, id, "Summary", 1), default);
        Assert.Equal("Found", result.Diagnostic);
        Assert.Equal(representation, result.Target!.Representation);
        Assert.Equal(state, result.Target.StateCode);
        Assert.Equal(kind.StartsWith("farm") || kind.StartsWith("warehouse"), result.Target.AccessScope.StartsWith("viewer:"));
        Assert.DoesNotContain("admin", result.Target.AccessScope);
        Assert.Equal(1, sources.Calls);
        Assert.Equal("Unmapped", 개체시각선택Policy.Select(result.Target, [], new 개체시각자산Catalog(new 개체시각대응Tests.Monitor())).Diagnostic);
    }

    [Theory]
    [InlineData("농장")]
    [InlineData("System.User")]
    [InlineData("farm.observation")]
    [InlineData("warehouse")]
    [InlineData("mart")]
    public async Task 테이블명_관측행_미준비건물은새객체로만들지않는다(string kind)
    {
        var s = new Sources();
        var result = await new 개체시각대상Reader(s, s, s, s, new Current()).ReadAsync(new(kind, "1", "Summary"), default);
        Assert.Equal("UnsupportedKind", result.Diagnostic);
        Assert.Equal(0, s.Calls);
    }

    [Fact]
    public async Task 권한범위의기존목록에없으면새농장을만들지않는다()
    {
        var s = new Sources();
        var reader = new 개체시각대상Reader(s, s, s, s, new Current());
        Assert.Equal("NotFoundOrNotAuthorized", (await reader.ReadAsync(new("farm", "other", "Summary"), default)).Diagnostic);
        Assert.Equal("NotFoundOrOutsideAuthorizedWindow", (await reader.ReadAsync(new("warehouse.inventory", "other", "Summary", 1), default)).Diagnostic);
    }

    [Fact]
    public async Task 기존조회실패와취소를없음으로가리지않는다()
    {
        var s = new Sources { Fail = true };
        var reader = new 개체시각대상Reader(s, s, s, s, new Current());
        Assert.Equal("SourceAccessOrQueryFailed", (await reader.ReadAsync(new("farm", "farm:1", "Summary"), default)).Diagnostic);
        s.Fail = false;
        await Assert.ThrowsAsync<OperationCanceledException>(() => reader.ReadAsync(new("farm", "farm:1", "Summary"), new CancellationToken(true)));
    }

    [Theory]
    [InlineData("01")]
    [InlineData("-1")]
    [InlineData("1 OR 1=1")]
    public async Task 마트식별자표기를정규화없이중복허용하지않는다(string id)
    {
        var s = new Sources();
        Assert.Equal("InvalidTarget", (await new 개체시각대상Reader(s, s, s, s, new Current()).ReadAsync(new("mart.product", id, "Summary"), default)).Diagnostic);
        Assert.Equal(0, s.Calls);
    }

    private sealed class Current : ICurrentUserAccessor { public string? UserId => "admin"; public string? Role => "ServerAdmin"; }
    private sealed class Sources : IFarmProducerPerspectiveUseCase, I창고WorldSnapshot조회UseCase, I마트공개상품조회UseCase, I공통식품품목Identity조회UseCase
    {
        public int Calls;
        public bool Fail;
        public Task<Result<FarmProducerPerspectiveResponse>> QueryAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested(); Calls++;
            return Task.FromResult(Fail ? Result.Fail<FarmProducerPerspectiveResponse>("SourceFailed") : Result.Ok(new FarmProducerPerspectiveResponse(
                "snapshot", 1, "Producer", "farm", "Owned", "OperationalProjection", "auth:1", DateTimeOffset.UtcNow,
                [new("farm:1", 1, "농장", "Operating", [new("plot:1", 1, "밭", null,
                    [new("crop:1", 1, "감자", "product:potato", "CommonFoodIdentity", "Growing", null, null)], [])])], [])));
        }
        public Task<Result<WarehouseWorldSnapshotResponse>> 조회Async(long? warehouseId, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(Result.Ok(new WarehouseWorldSnapshotResponse { Revision = "r1", InventoryItems =
                [new() { StableId = "item:1", Status = "Available", ProductName = "상자", WarehouseStableId = "warehouse:1" }] }));
        }
        public Task<Result<마트공개상품상세응답>> 상세Async(long productId, CancellationToken cancellationToken)
        { Calls++; return Task.FromResult(Result.Ok(new 마트공개상품상세응답 { Id = 1, 상품명 = "공개상품", 판매가능여부 = true, 수정일시Utc = new(2026, 8, 31) })); }
        public Task<Result<마트공개상품목록응답>> 목록Async(마트공개상품목록조회요청 request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<공통식품품목IdentityResponse?> 단건조회Async(string canonicalProductStableId, CancellationToken cancellationToken = default)
        { Calls++; return Task.FromResult<공통식품품목IdentityResponse?>(new("product:potato", "감자", "r1", [], [])); }
        public Task<공통식품품목IdentityListResponse> 목록조회Async(CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
