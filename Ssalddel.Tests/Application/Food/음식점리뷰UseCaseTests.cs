using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.Food;
using Ssalddel.Contracts.Food;
using Ssalddel.Contracts.Restaurants;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.음식;

namespace Ssalddel.Tests.Application.Food;

public sealed class 음식점리뷰UseCaseTests
{
    [Fact]
    public async Task 전달완료주문의_실제주문자만_리뷰를등록할수있다()
    {
        await using var context = CreateContext();
        context.음식주문.Add(CreateDeliveredOrder("FOOD-REVIEW-1", "actual-user", 71));
        await context.SaveChangesAsync();
        var useCase = new 음식점리뷰UseCase(context);

        var result = await useCase.등록Async(
            71,
            new 음식점리뷰등록요청
            {
                주문자UserId = "forged-user",
                주문번호 = "FOOD-REVIEW-1",
                별점 = 5,
                내용 = "주문한 음식과 같았어요.",
                사진Urls = ["https://example.test/review.jpg"]
            },
            "actual-user",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await context.음식점리뷰.SingleAsync();
        Assert.Equal("actual-user", saved.주문자UserId);
        Assert.NotEqual("forged-user", saved.주문자UserId);
        Assert.True(result.Value.사진포함여부);
        Assert.Equal("인증 주문자", result.Value.주문자UserId);
        Assert.Null(result.Value.주문번호);
    }

    [Fact]
    public async Task 다른사용자와_미완료주문과_중복리뷰는거부한다()
    {
        await using var context = CreateContext();
        context.음식주문.AddRange(
            CreateDeliveredOrder("FOOD-REVIEW-2", "owner", 72),
            CreateDeliveredOrder(
                "FOOD-REVIEW-3",
                "owner",
                72,
                음식주문상태코드.조리중));
        await context.SaveChangesAsync();
        var useCase = new 음식점리뷰UseCase(context);
        var request = new 음식점리뷰등록요청
        {
            주문번호 = "FOOD-REVIEW-2",
            별점 = 4,
            내용 = "재주문하고 싶어요."
        };

        var otherUser = await useCase.등록Async(
            72,
            request,
            "other-user",
            CancellationToken.None);
        var incomplete = await useCase.등록Async(
            72,
            new 음식점리뷰등록요청
            {
                주문번호 = "FOOD-REVIEW-3",
                별점 = 4,
                내용 = "아직 배달 중입니다."
            },
            "owner",
            CancellationToken.None);
        var first = await useCase.등록Async(
            72,
            request,
            "owner",
            CancellationToken.None);
        var duplicate = await useCase.등록Async(
            72,
            request,
            "owner",
            CancellationToken.None);

        Assert.Equal(404, StatusCode(otherUser));
        Assert.Equal(404, StatusCode(incomplete));
        Assert.True(first.IsSuccess);
        Assert.Equal(409, StatusCode(duplicate));
        Assert.Equal(1, await context.음식점리뷰.CountAsync());
    }

    [Fact]
    public async Task 공개리뷰목록은_원본주문자식별자를노출하지않는다()
    {
        await using var context = CreateContext();
        context.음식주문.Add(CreateDeliveredOrder("FOOD-REVIEW-4", "private-user-id", 73));
        await context.SaveChangesAsync();
        var useCase = new 음식점리뷰UseCase(context);
        await useCase.등록Async(
            73,
            new 음식점리뷰등록요청
            {
                주문번호 = "FOOD-REVIEW-4",
                별점 = 3,
                내용 = "무난했어요."
            },
            "private-user-id",
            CancellationToken.None);

        var result = await useCase.목록Async(73, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var review = Assert.Single(result.Value.Items);
        Assert.Equal("인증 주문자", review.주문자UserId);
        Assert.DoesNotContain("private-user-id", review.주문자UserId);
        Assert.Null(review.주문번호);
    }

    private static int StatusCode<T>(FluentResults.Result<T> result)
        => Assert.IsType<int>(result.Errors.Single().Metadata["StatusCode"]);

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"restaurant-review-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new PassThroughEncryptionService());
    }

    private static 음식주문 CreateDeliveredOrder(
        string orderNo,
        string userId,
        long restaurantId,
        string status = 음식주문상태코드.전달완료)
        => new()
        {
            주문번호 = orderNo,
            음식점Id = restaurantId,
            음식점명 = "검증 음식점",
            음식점주소 = "서울시 음식로 1",
            주문자UserId = userId,
            수령인명 = "수령자",
            수령인연락처 = "010-0000-0000",
            수령지주소 = "서울시 수령로 2",
            총주문금액 = 12000m,
            상태 = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private sealed class PassThroughEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
