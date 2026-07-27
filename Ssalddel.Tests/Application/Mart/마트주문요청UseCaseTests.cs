using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Mart;
using Ssalddel.Contracts.Mart;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.마트;

namespace Ssalddel.Tests.Application.Mart;

public sealed class 마트주문요청UseCaseTests
{
    [Fact]
    public async Task 조회와작성은_로그인사용자Id가없으면401을반환한다()
    {
        await using var context = CreateContext();
        var currentUser = new TestCurrentUserAccessor(null);

        var write = await new 마트주문요청작성UseCase(context, currentUser)
            .등록Async(CreateRequest(), CancellationToken.None);
        var read = await new 마트주문요청조회UseCase(context, currentUser)
            .상세Async(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(401, write.Errors.Single().Metadata["StatusCode"]);
        Assert.Equal(401, read.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 작성은_현재안내확인과유효한수량을요구한다()
    {
        await using var context = CreateContext();
        context.마트공개상품.Add(CreateProduct());
        await context.SaveChangesAsync();
        var useCase = new 마트주문요청작성UseCase(context, new TestCurrentUserAccessor("orderer-a"));
        var missingConsent = CreateRequest();
        missingConsent.비구속주문요청확인 = false;
        var excessiveQuantity = CreateRequest();
        excessiveQuantity.수량 = 101;

        var consentResult = await useCase.등록Async(missingConsent, CancellationToken.None);
        var quantityResult = await useCase.등록Async(excessiveQuantity, CancellationToken.None);

        Assert.Equal(400, consentResult.Errors.Single().Metadata["StatusCode"]);
        Assert.Equal(400, quantityResult.Errors.Single().Metadata["StatusCode"]);
        Assert.Empty(context.마트주문요청);
    }

    [Fact]
    public async Task 작성은_서버상품스냅샷을멱등저장하고운영원장과재고를변경하지않는다()
    {
        await using var context = CreateContext();
        var product = CreateProduct();
        context.마트공개상품.Add(product);
        await context.SaveChangesAsync();
        var useCase = new 마트주문요청작성UseCase(context, new TestCurrentUserAccessor("orderer-a"));
        var request = CreateRequest();

        var first = await useCase.등록Async(request, CancellationToken.None);
        var repeated = await useCase.등록Async(request, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(first.Value.주문요청Id, repeated.Value.주문요청Id);
        Assert.Equal(2, first.Value.수량);
        Assert.Equal(3_600m, first.Value.합계);
        Assert.Equal(12, first.Value.제출시판매가능수량);
        Assert.False(first.Value.재고예약됨);
        Assert.False(first.Value.결제됨);
        Assert.Single(context.마트주문요청);
        Assert.Empty(context.마트주문);
        Assert.Empty(context.마트주문상품);
        Assert.Equal(12, (await context.마트공개상품.SingleAsync()).판매가능수량);
    }

    [Fact]
    public async Task 같은요청Id를다른수량에사용하면409를반환한다()
    {
        await using var context = CreateContext();
        context.마트공개상품.Add(CreateProduct());
        await context.SaveChangesAsync();
        var useCase = new 마트주문요청작성UseCase(context, new TestCurrentUserAccessor("orderer-a"));
        var request = CreateRequest();
        await useCase.등록Async(request, CancellationToken.None);
        request.수량 = 3;

        var result = await useCase.등록Async(request, CancellationToken.None);

        Assert.Equal(409, result.Errors.Single().Metadata["StatusCode"]);
        Assert.Single(context.마트주문요청);
    }

    [Fact]
    public async Task 판매가능수량보다큰요청은409이고원장을만들지않는다()
    {
        await using var context = CreateContext();
        context.마트공개상품.Add(CreateProduct());
        await context.SaveChangesAsync();
        var request = CreateRequest();
        request.수량 = 13;

        var result = await new 마트주문요청작성UseCase(context, new TestCurrentUserAccessor("orderer-a"))
            .등록Async(request, CancellationToken.None);

        Assert.Equal(409, result.Errors.Single().Metadata["StatusCode"]);
        Assert.Empty(context.마트주문요청);
    }

    [Fact]
    public async Task 상세은_본인요청만읽고저장시점가격을유지한다()
    {
        await using var context = CreateContext();
        var product = CreateProduct();
        context.마트공개상품.Add(product);
        await context.SaveChangesAsync();
        var owner = new TestCurrentUserAccessor("orderer-a");
        var created = await new 마트주문요청작성UseCase(context, owner)
            .등록Async(CreateRequest(), CancellationToken.None);
        product.판매가 = 2_500m;
        await context.SaveChangesAsync();

        var ownerRead = await new 마트주문요청조회UseCase(context, owner)
            .상세Async(created.Value.주문요청Id, CancellationToken.None);
        var otherRead = await new 마트주문요청조회UseCase(context, new TestCurrentUserAccessor("orderer-b"))
            .상세Async(created.Value.주문요청Id, CancellationToken.None);

        Assert.Equal(1_800m, ownerRead.Value.단가);
        Assert.Equal(3_600m, ownerRead.Value.합계);
        Assert.Equal(404, otherRead.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 목록은_현재사용자의요청만_상태와검색조건으로조회한다()
    {
        await using var context = CreateContext();
        context.마트공개상품.Add(CreateProduct());
        await context.SaveChangesAsync();
        var owner = new TestCurrentUserAccessor("orderer-a");
        var other = new TestCurrentUserAccessor("orderer-b");
        await new 마트주문요청작성UseCase(context, owner)
            .등록Async(CreateRequest(), CancellationToken.None);
        await new 마트주문요청작성UseCase(context, other)
            .등록Async(CreateRequest(), CancellationToken.None);

        var result = await new 마트주문요청조회UseCase(context, owner)
            .목록Async(
                new 마트주문요청목록조회요청
                {
                    상태코드 = 마트주문요청상태코드.제출됨,
                    Search = "생수",
                    PageSize = 10
                },
                CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Single(result.Value.Items);
        Assert.Equal("동네 생수", result.Value.Items[0].상품명);
    }

    [Fact]
    public async Task 제출된비구속요청은_재고차감없이수량을변경하고_철회를멱등처리한다()
    {
        await using var context = CreateContext();
        var product = CreateProduct();
        context.마트공개상품.Add(product);
        await context.SaveChangesAsync();
        var currentUser = new TestCurrentUserAccessor("orderer-a");
        var useCase = new 마트주문요청작성UseCase(context, currentUser);
        var created = await useCase.등록Async(CreateRequest(), CancellationToken.None);
        product.판매가 = 2_000m;
        await context.SaveChangesAsync();

        var changed = await useCase.수량변경Async(
            created.Value.주문요청Id,
            new 마트주문요청수량변경요청
            {
                수량 = 3,
                비구속주문요청확인 = true,
                안내버전 = 마트주문요청안내.현재버전
            },
            CancellationToken.None);
        var withdrawn = await useCase.철회Async(
            created.Value.주문요청Id,
            new 마트주문요청철회요청(),
            CancellationToken.None);
        var repeated = await useCase.철회Async(
            created.Value.주문요청Id,
            new 마트주문요청철회요청(),
            CancellationToken.None);
        var changeAfterWithdrawal = await useCase.수량변경Async(
            created.Value.주문요청Id,
            new 마트주문요청수량변경요청
            {
                수량 = 4,
                비구속주문요청확인 = true,
                안내버전 = 마트주문요청안내.현재버전,
                기대상태코드 = 마트주문요청상태코드.철회됨
            },
            CancellationToken.None);

        Assert.True(changed.IsSuccess);
        Assert.Equal(3, changed.Value.수량);
        Assert.Equal(6_000m, changed.Value.합계);
        Assert.True(withdrawn.IsSuccess);
        Assert.Equal(마트주문요청상태코드.철회됨, withdrawn.Value.상태코드);
        Assert.Equal(withdrawn.Value.상태코드, repeated.Value.상태코드);
        Assert.Equal(409, changeAfterWithdrawal.Errors.Single().Metadata["StatusCode"]);
        Assert.Equal(12, (await context.마트공개상품.SingleAsync()).판매가능수량);
        Assert.Empty(context.마트주문);
        Assert.Empty(context.마트주문상품);
    }

    private static 마트주문요청등록요청 CreateRequest()
        => new()
        {
            클라이언트요청Id = Guid.NewGuid(),
            공개상품Id = 41,
            수량 = 2,
            비구속주문요청확인 = true,
            안내버전 = 마트주문요청안내.현재버전
        };

    private static 마트공개상품 CreateProduct()
        => new()
        {
            Id = 41,
            상품명 = "동네 생수",
            카테고리 = "생활",
            판매단위 = "묶음",
            판매가 = 1_800m,
            판매가능수량 = 12,
            공개여부 = true,
            판매허용여부 = true,
            재고기준시각Utc = new DateTime(2026, 7, 20, 3, 0, 0, DateTimeKind.Utc)
        };

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class TestCurrentUserAccessor(string? userId) : ICurrentUserAccessor
    {
        public string? UserId { get; } = userId;
        public string? Role => null;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
