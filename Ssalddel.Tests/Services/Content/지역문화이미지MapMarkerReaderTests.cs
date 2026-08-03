using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.Content;
using Ssalddel.Services.Content;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.Content;

public sealed class 지역문화이미지MapMarkerReaderTests
{
    [Fact]
    public void 대표점Catalog는_서울을제외한_97개지역을제공한다()
    {
        var anchors = 지역문화행정구역대표점Catalog.All;

        Assert.Equal(97, anchors.Count);
        Assert.Equal(16, anchors.Count(item => item.RegionKey.StartsWith("kr-", StringComparison.Ordinal)));
        Assert.Equal(50, anchors.Count(item => item.RegionKey.StartsWith("us-", StringComparison.Ordinal)));
        Assert.Equal(31, anchors.Count(item => item.RegionKey.StartsWith("cn-", StringComparison.Ordinal)));
        Assert.DoesNotContain(anchors, item => item.RegionKey == "kr-seoul");
        Assert.Equal(anchors.Count, anchors.Select(item => item.RegionKey).Distinct(StringComparer.Ordinal).Count());
        Assert.All(anchors, item =>
        {
            Assert.InRange(item.Latitude, -90, 90);
            Assert.InRange(item.Longitude, -180, 180);
        });
    }

    [Fact]
    public async Task 공개Marker는_Prompt와활성이미지가모두있는지역만제공한다()
    {
        await using var db = CreateContext();
        db.지역문화이미지Prompts.AddRange(
            CreatePrompt("kr-busan", "KR", "KR-26", "부산광역시"),
            CreatePrompt("kr-daegu", "KR", "KR-27", "대구광역시"));
        db.앱문맥이미지자산들.AddRange(
            CreateAsset("kr-busan--scene-01", true, 앱문맥이미지품질상태.미검토),
            CreateAsset("kr-daegu--scene-01", true, 앱문맥이미지품질상태.제외),
            CreateAsset("kr-gwangju--scene-01", true, 앱문맥이미지품질상태.미검토));
        await db.SaveChangesAsync();

        var reader = new 지역문화이미지MapMarkerReader(db);
        var markers = await reader.공개Marker조회Async();

        Assert.Equal(2, markers.Count);
        var marker = Assert.Single(markers, item => item.RegionKey == "kr-busan");
        Assert.Equal("kr-busan", marker.RegionKey);
        Assert.Equal("부산광역시", marker.RegionName);
        Assert.Equal("대한민국", marker.CountryName);
        Assert.InRange(marker.Latitude, 35.1, 35.3);
        var imageOnlyMarker = Assert.Single(markers, item => item.RegionKey == "kr-gwangju");
        Assert.Equal("광주광역시", imageOnlyMarker.RegionName);
        Assert.Equal("대한민국", imageOnlyMarker.CountryName);
    }

    private static 지역문화이미지Prompt CreatePrompt(
        string regionKey,
        string countryCode,
        string subdivisionCode,
        string regionName)
        => new()
        {
            RegionKey = regionKey,
            CountryCode = countryCode,
            SubdivisionCode = subdivisionCode,
            RegionNameKo = regionName,
            RegionNameEn = regionName,
            RegionNameLocal = regionName,
            RegionTypeCode = 지역문화행정구역유형Codes.KoreaMetropolitanCity,
            CultureSummaryKo = "생활문화 조사 요약",
            CreatedAtUtc = new DateTime(2026, 8, 3, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 8, 3, 0, 0, 0, DateTimeKind.Utc)
        };

    private static 앱문맥이미지자산 CreateAsset(
        string sceneKey,
        bool active,
        앱문맥이미지품질상태 quality)
        => new()
        {
            장면Key = sceneKey,
            앱PackId = "regional-culture-one-each-v1",
            장면번호 = 1,
            PromptVersion = 2,
            제목 = sceneKey,
            대체Text = sceneKey,
            이미지Url = $"https://cdn.example.test/{sceneKey}.jpg",
            StorageContainer = "public",
            StorageObjectName = $"{sceneKey}.jpg",
            화면비율 = "4:3",
            Sha256 = new string('a', 64),
            활성화여부 = active,
            품질상태 = quality,
            수정시각 = new DateTimeOffset(2026, 8, 3, 0, 0, 0, TimeSpan.Zero)
        };

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"regional-culture-map-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
