using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.Mart;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.마트;

namespace Ssalddel.Tests.Application.Mart;

public sealed class 마트공개상품조회UseCaseTests
{
    [Fact]
    public async Task 목록은_공개투영만중립정렬해반환하고내부원장을읽지않는다()
    {
        await using var context = CreateContext();
        var (availableId, _, _) = await SeedAsync(context);
        var useCase = new 마트공개상품조회UseCase(context);

        var result = await useCase.목록Async(new()
        {
            검색어 = "생수",
            판매가능만 = true,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(availableId, item.Id);
        Assert.Equal("생수 6입", item.상품명);
        Assert.True(item.판매가능여부);
        Assert.Equal(8, item.판매가능수량);
        Assert.Contains("직접 공개한 값이 아니라", result.Value.재고기준안내);
    }

    [Fact]
    public async Task 판매가능필터를끄면_공개품절상품도반환하지만비공개상품은숨긴다()
    {
        await using var context = CreateContext();
        var (_, unavailableId, _) = await SeedAsync(context);
        var useCase = new 마트공개상품조회UseCase(context);

        var result = await useCase.목록Async(new()
        {
            판매가능만 = false,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Contains(result.Value.Items, item => item.Id == unavailableId && !item.판매가능여부);
        Assert.DoesNotContain(result.Value.Items, item => item.상품명 == "비공개 상품");
    }

    [Fact]
    public async Task 정확한상세는_비공개ProductId를404로숨기고다른상품으로대체하지않는다()
    {
        await using var context = CreateContext();
        var (availableId, _, privateId) = await SeedAsync(context);
        var useCase = new 마트공개상품조회UseCase(context);

        var found = await useCase.상세Async(availableId, CancellationToken.None);
        var hidden = await useCase.상세Async(privateId, CancellationToken.None);

        Assert.True(found.IsSuccess);
        Assert.Equal(availableId, found.Value.Id);
        Assert.Equal("한 묶음", found.Value.판매단위);
        Assert.True(hidden.IsFailed);
        Assert.Equal(404, hidden.Errors.Single().Metadata["StatusCode"]);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static async Task<(long AvailableId, long UnavailableId, long PrivateId)> SeedAsync(
        SsalddelContext context)
    {
        var now = new DateTime(2026, 7, 20, 8, 30, 0, DateTimeKind.Utc);
        var available = new 마트공개상품
        {
            상품명 = "생수 6입",
            카테고리 = "상온",
            짧은설명 = "묶음 포장 상품",
            설명 = "공개 상품 설명",
            판매단위 = "한 묶음",
            판매가 = 4200m,
            판매가능수량 = 8,
            공개여부 = true,
            판매허용여부 = true,
            재고기준시각Utc = now,
            UpdatedAtUtc = now
        };
        var unavailable = new 마트공개상품
        {
            상품명 = "휴지 12롤",
            카테고리 = "생활",
            짧은설명 = "현재 투영 수량 없음",
            설명 = "공개 품절 상품",
            판매단위 = "한 팩",
            판매가 = 9800m,
            판매가능수량 = 0,
            공개여부 = true,
            판매허용여부 = true,
            재고기준시각Utc = now,
            UpdatedAtUtc = now
        };
        var privateItem = new 마트공개상품
        {
            상품명 = "비공개 상품",
            카테고리 = "내부",
            판매단위 = "개",
            판매가 = 1m,
            판매가능수량 = 99,
            공개여부 = false,
            판매허용여부 = true,
            재고기준시각Utc = now,
            UpdatedAtUtc = now
        };
        context.마트공개상품.AddRange(available, unavailable, privateItem);
        await context.SaveChangesAsync();
        return (available.Id, unavailable.Id, privateItem.Id);
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
