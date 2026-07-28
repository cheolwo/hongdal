using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.Admin.Restaurants;
using Ssalddel.Contracts.Admin.Restaurants;
using Ssalddel.Controllers.Admin;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.음식;

namespace Ssalddel.Tests.Application.Admin.Restaurants;

public sealed class 음식운영관리UseCaseTests
{
    [Fact]
    public async Task 리뷰운영목록은_메인Db리뷰와음식점이름을조합한다()
    {
        await using var context = CreateContext();
        context.음식점공개프로필.Add(new 음식점공개프로필
        {
            Id = 81,
            상호명 = "메인 원장 식당",
            카테고리 = "한식",
            공개주소 = "서울시",
            공개여부 = true,
            주문가능여부 = true
        });
        context.음식점리뷰.Add(new 음식점리뷰
        {
            Id = 91,
            음식점Id = 81,
            주문자UserId = "review-owner",
            주문번호 = "FOOD-ADMIN-1",
            별점 = 2,
            내용 = "운영 검토가 필요합니다.",
            사진UrlsJson = "[\"https://example.test/photo.jpg\"]",
            관리자검토필요여부 = true,
            현재노출여부 = true
        });
        await context.SaveChangesAsync();
        var useCase = new 음식운영관리UseCase(context);

        var result = await useCase.리뷰목록Async(CancellationToken.None);

        Assert.True(result.IsSuccess);
        var review = Assert.Single(result.Value.Items);
        Assert.Equal("메인 원장 식당", review.음식점명);
        Assert.Equal("review-owner", review.주문자UserId);
        Assert.True(review.사진포함여부);
        Assert.True(review.관리자검토필요여부);
    }

    [Fact]
    public async Task 배달요금정책은_메인Db에저장되고_재조회와감사정보가일치한다()
    {
        await using var context = CreateContext();
        var useCase = new 음식운영관리UseCase(context);
        var request = new 음식배달요금정책응답
        {
            BaseFee = 3500m,
            IncludedDistanceMeters = 1200,
            DistanceUnitMeters = 100,
            DistanceUnitFee = 150m,
            MinimumFee = 3300m,
            DriverBasePayout = 2800m,
            DriverDistanceUnitPayout = 100m,
            DriverMinimumPayout = 2700m
        };

        var updated = await useCase.배달요금정책수정Async(
            request,
            "admin-user",
            CancellationToken.None);
        context.ChangeTracker.Clear();
        var reloaded = await new 음식운영관리UseCase(context)
            .배달요금정책조회Async(CancellationToken.None);

        Assert.True(updated.IsSuccess);
        Assert.True(reloaded.IsSuccess);
        Assert.Equal(3500m, reloaded.Value.BaseFee);
        Assert.Equal(150m, reloaded.Value.DistanceUnitFee);
        Assert.Equal("admin-user", reloaded.Value.UpdatedByUserId);
        Assert.NotEqual(default, reloaded.Value.UpdatedAtUtc);
        Assert.Equal(1, await context.음식운영정책.CountAsync());
    }

    [Fact]
    public async Task 잘못된요금정책은_저장하지않는다()
    {
        await using var context = CreateContext();
        var useCase = new 음식운영관리UseCase(context);

        var result = await useCase.배달요금정책수정Async(
            new 음식배달요금정책응답
            {
                BaseFee = -1,
                DistanceUnitMeters = 0
            },
            "admin-user",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Equal(400, result.Errors.Single().Metadata["StatusCode"]);
        Assert.Empty(context.음식운영정책);
    }

    [Theory]
    [InlineData(typeof(음식점리뷰관리Controller), "api/v1/admin/restaurant-reviews")]
    [InlineData(typeof(음식배달요금정책Controller), "api/v1/admin/food-delivery-pricing-policy")]
    public void 음식운영관리Api는_서버관리자정책과기존경로를유지한다(
        Type controllerType,
        string expectedRoute)
    {
        Assert.Equal(
            "서버관리자전용",
            controllerType.GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        Assert.Equal(
            expectedRoute,
            controllerType.GetCustomAttribute<RouteAttribute>()?.Template);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"food-operations-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new PassThroughEncryptionService());
    }

    private sealed class PassThroughEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
