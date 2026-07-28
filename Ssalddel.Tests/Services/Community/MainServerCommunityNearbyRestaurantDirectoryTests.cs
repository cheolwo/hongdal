using Microsoft.EntityFrameworkCore;
using Ssalddel.Services.Community;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.음식;

namespace Ssalddel.Tests.Services.Community;

public sealed class MainServerCommunityNearbyRestaurantDirectoryTests
{
    [Fact]
    public async Task 주변음식점은_메인Db공개프로필과_현재노출리뷰를조합한다()
    {
        await using var db = CreateContext();
        db.음식점공개프로필.AddRange(
            Restaurant(101, "가까운 식당", 37.5500m, 126.8500m),
            Restaurant(102, "먼 식당", 37.6500m, 126.8500m),
            Restaurant(103, "비공개 식당", 37.5501m, 126.8501m, isPublic: false));
        db.음식점리뷰.AddRange(
            Review(101, 5),
            Review(101, 3),
            Review(101, 1, isVisible: false),
            Review(101, 1, expiresAtUtc: DateTime.UtcNow.AddMinutes(-1)));
        await db.SaveChangesAsync();
        var directory = new MainServerCommunityNearbyRestaurantDirectory(db);

        var result = await directory.FindAsync(
            37.5500m,
            126.8500m,
            3m,
            10,
            CancellationToken.None);

        Assert.True(result.SourceAvailable);
        Assert.False(result.IsSimulationSource);
        var restaurant = Assert.Single(result.Items);
        Assert.Equal(101, restaurant.Id);
        Assert.Equal("가까운 식당", restaurant.상호명);
        Assert.Equal(4m, restaurant.평균평점);
        Assert.Equal(2, restaurant.리뷰수);
        Assert.Equal(0m, restaurant.거리Km);
    }

    [Fact]
    public async Task 주변음식점은_거리와건수제한을적용한다()
    {
        await using var db = CreateContext();
        db.음식점공개프로필.AddRange(
            Restaurant(201, "첫 식당", 37.5500m, 126.8500m),
            Restaurant(202, "둘째 식당", 37.5510m, 126.8500m));
        await db.SaveChangesAsync();
        var directory = new MainServerCommunityNearbyRestaurantDirectory(db);

        var result = await directory.FindAsync(
            37.5500m,
            126.8500m,
            3m,
            1,
            CancellationToken.None);

        var restaurant = Assert.Single(result.Items);
        Assert.Equal(201, restaurant.Id);
    }

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"main-restaurant-directory-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new PassThroughEncryptionService());
    }

    private static 음식점공개프로필 Restaurant(
        long id,
        string name,
        decimal latitude,
        decimal longitude,
        bool isPublic = true)
        => new()
        {
            Id = id,
            상호명 = name,
            카테고리 = "한식",
            공개주소 = "서울시",
            위도 = latitude,
            경도 = longitude,
            공개여부 = isPublic,
            주문가능여부 = true
        };

    private static 음식점리뷰 Review(
        long restaurantId,
        int rating,
        bool isVisible = true,
        DateTime? expiresAtUtc = null)
        => new()
        {
            음식점Id = restaurantId,
            주문자UserId = Guid.NewGuid().ToString("N"),
            주문번호 = Guid.NewGuid().ToString("N"),
            별점 = rating,
            내용 = "검증 리뷰",
            현재노출여부 = isVisible,
            게시종료일시Utc = expiresAtUtc
        };

    private sealed class PassThroughEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
