using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.Food;
using Ssalddel.Services.Orderer;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.음식;

namespace Ssalddel.Tests.Application.Food;

public sealed class 음식점탐색조회UseCaseTests
{
    [Fact]
    public async Task 선택한공개권역기준반경안의공개음식점만반환한다()
    {
        await using var context = CreateContext();
        var (publicRestaurantId, _, _, _) = await SeedAsync(context);
        var useCase = new 음식점탐색조회UseCase(context, new InMemoryRestaurantSearchPolicyStore());

        var result = await useCase.목록Async(new()
        {
            배달권키 = "bjd-sigungu:11500",
            반경Km = 3m,
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Items.Count);
        var item = Assert.Single(result.Value.Items, candidate => candidate.Id == publicRestaurantId);
        Assert.Equal(publicRestaurantId, item.Id);
        Assert.Equal("공개분식", item.상호명);
        Assert.Equal(1, item.공개메뉴수);
        Assert.InRange(item.거리Km!.Value, 0m, 3m);
        Assert.Equal(3m, result.Value.적용반경Km);
        Assert.Contains("자동 수집하지 않습니다", result.Value.거리기준안내);
    }

    [Fact]
    public async Task 카테고리목록은_표준순서와_공개음식점수를제공한다()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var useCase = new 음식점탐색조회UseCase(context, new InMemoryRestaurantSearchPolicyStore());

        var result = await useCase.카테고리목록Async(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("분식", result.Value[0].카테고리키);
        Assert.Equal(1, result.Value.Single(item => item.카테고리키 == "분식").공개음식점수);
        Assert.Equal(1, result.Value.Single(item => item.카테고리키 == "피자").공개음식점수);
        Assert.Equal(0, result.Value.Single(item => item.카테고리키 == "치킨").공개음식점수);
    }

    [Fact]
    public async Task 선택한카테고리의_공개음식점만반환한다()
    {
        await using var context = CreateContext();
        var (_, _, _, pizzaRestaurantId) = await SeedAsync(context);
        var useCase = new 음식점탐색조회UseCase(context, new InMemoryRestaurantSearchPolicyStore());

        var result = await useCase.목록Async(new()
        {
            배달권키 = "bjd-sigungu:11500",
            반경Km = 3m,
            카테고리 = "피자",
            Page = 1,
            PageSize = 10
        }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var restaurant = Assert.Single(result.Value.Items);
        Assert.Equal(pizzaRestaurantId, restaurant.Id);
        Assert.Equal("피자", restaurant.카테고리);
    }

    [Fact]
    public async Task 정확한상세는공개메뉴만반환하고비공개음식점은404로숨긴다()
    {
        await using var context = CreateContext();
        var (publicRestaurantId, privateRestaurantId, _, _) = await SeedAsync(context);
        var useCase = new 음식점탐색조회UseCase(context, new InMemoryRestaurantSearchPolicyStore());

        var found = await useCase.상세Async(publicRestaurantId, CancellationToken.None);
        var hidden = await useCase.상세Async(privateRestaurantId, CancellationToken.None);

        Assert.True(found.IsSuccess);
        Assert.Equal(publicRestaurantId, found.Value.음식점.Id);
        Assert.Equal("김밥", Assert.Single(found.Value.메뉴목록).메뉴명);
        Assert.True(hidden.IsFailed);
        Assert.Equal(404, hidden.Errors.Single().Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 권역목록은개인위치없이공개기준점을제공하고정책밖반경은거부한다()
    {
        await using var context = CreateContext();
        var useCase = new 음식점탐색조회UseCase(context, new InMemoryRestaurantSearchPolicyStore());

        var scopes = await useCase.권역목록Async(CancellationToken.None);
        var invalid = await useCase.목록Async(new()
        {
            배달권키 = "bjd-sigungu:11500",
            반경Km = 15m
        }, CancellationToken.None);

        Assert.True(scopes.IsSuccess);
        var gangseo = Assert.Single(scopes.Value, item => item.배달권키 == "bjd-sigungu:11500");
        Assert.Equal("서울특별시 강서구", gangseo.표시명);
        Assert.Contains("현재 위치", gangseo.거리기준안내);
        Assert.True(invalid.IsFailed);
        Assert.Contains("1km부터 10km", invalid.Errors.Single().Message);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static async Task<(long PublicId, long PrivateId, long FarId, long PizzaId)> SeedAsync(SsalddelContext context)
    {
        var publicRestaurant = new 음식점공개프로필
        {
            상호명 = "공개분식",
            카테고리 = "분식",
            소개 = "권역 안 공개 음식점",
            공개주소 = "서울특별시 강서구",
            위도 = 37.555000m,
            경도 = 126.850000m,
            최소주문금액 = 12000m,
            예상조리분 = 20,
            공개여부 = true,
            주문가능여부 = true,
            메뉴목록 =
            [
                new 음식점메뉴 { 메뉴명 = "김밥", 설명 = "공개 메뉴", 판매가 = 4500m, 공개여부 = true, 표시순서 = 1 },
                new 음식점메뉴 { 메뉴명 = "비공개 메뉴", 설명 = "숨김", 판매가 = 1m, 공개여부 = false, 표시순서 = 2 }
            ]
        };
        var privateRestaurant = new 음식점공개프로필
        {
            상호명 = "비공개식당",
            카테고리 = "한식",
            공개주소 = "서울특별시 강서구",
            위도 = 37.551000m,
            경도 = 126.850000m,
            공개여부 = false,
            주문가능여부 = true
        };
        var farRestaurant = new 음식점공개프로필
        {
            상호명 = "먼식당",
            카테고리 = "한식",
            공개주소 = "서울특별시 종로구",
            위도 = 37.573504m,
            경도 = 126.978989m,
            공개여부 = true,
            주문가능여부 = true
        };
        var pizzaRestaurant = new 음식점공개프로필
        {
            상호명 = "공개피자",
            카테고리 = "피자",
            소개 = "권역 안 공개 피자 음식점",
            공개주소 = "서울특별시 강서구",
            위도 = 37.556000m,
            경도 = 126.851000m,
            최소주문금액 = 18000m,
            예상조리분 = 25,
            공개여부 = true,
            주문가능여부 = true,
            메뉴목록 =
            [
                new 음식점메뉴 { 메뉴명 = "치즈피자", 설명 = "공개 메뉴", 판매가 = 19000m, 공개여부 = true, 표시순서 = 1 }
            ]
        };
        context.AddRange(publicRestaurant, privateRestaurant, farRestaurant, pizzaRestaurant);
        await context.SaveChangesAsync();
        return (publicRestaurant.Id, privateRestaurant.Id, farRestaurant.Id, pizzaRestaurant.Id);
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
