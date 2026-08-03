using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Domain.Geography;
using Ssalddel.Services.Community;
using Ssalddel.Services.Content;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Tests.Services.Community;

public sealed class 경기데이터드림가축사육MapProjectionTests
{
    private static readonly DateTimeOffset CollectedAt =
        new(2026, 8, 3, 1, 2, 3, TimeSpan.Zero);

    [Fact]
    public async Task 갱신성공시_시군상태집계만Snapshot으로교체한다()
    {
        var store = new 경기데이터드림가축사육MapSnapshotStore();
        var client = new Stub가축사육집계Client(Response(
            true,
            new 경기데이터드림가축사육지역집계
            {
                RegionCode = "41110",
                RegionName = "수원시",
                BusinessStatus = "영업",
                BusinessCount = 7
            }));
        var sut = new 경기데이터드림가축사육MapProjectionRefresher(client, store);

        var refreshed = await sut.RefreshAsync();

        Assert.True(refreshed);
        var snapshot = store.Read();
        Assert.Equal(CollectedAt, snapshot.CollectedAtUtc);
        var item = Assert.Single(snapshot.Items);
        Assert.Equal("41110", item.RegionCode);
        Assert.Equal(7, item.BusinessCount);
        var serialized = JsonSerializer.Serialize(snapshot);
        Assert.DoesNotContain("농장명", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("전화", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("주소", serialized, StringComparison.Ordinal);
        Assert.DoesNotContain("좌표", serialized, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 갱신실패시_이전성공Snapshot을유지한다()
    {
        var previous = new 경기데이터드림가축사육MapSnapshot(
            CollectedAt.AddHours(-1),
            [new()
            {
                RegionCode = "41110",
                RegionName = "수원시",
                BusinessStatus = "영업",
                BusinessCount = 3
            }]);
        var store = new 경기데이터드림가축사육MapSnapshotStore();
        store.Replace(previous);
        var sut = new 경기데이터드림가축사육MapProjectionRefresher(
            new Stub가축사육집계Client(Response(success: false)),
            store);

        var refreshed = await sut.RefreshAsync();

        Assert.False(refreshed);
        Assert.Same(previous, store.Read());
    }

    [Fact]
    public async Task 검증된행정코드와경계가있는시군만_비식별집계Marker로반환한다()
    {
        await using var geographyDb = CreateGeographyDb();
        var suwon = Region("kr-gg-suwon", "수원시");
        geographyDb.지역농수산Map행정구역들.Add(suwon);
        geographyDb.지역농수산Map행정구역CodeAssignments.Add(new()
        {
            Region = suwon,
            SchemeCode = RegionalAgriculturalMapCodeSchemeCodes.KoreaMoisAdministrative,
            ExternalCode = "41110",
            SourceVintage = "2026",
            SourceUrl = "https://www.mois.go.kr/",
            VerifiedAtUtc = Utc(2026, 8, 2),
            CreatedAtUtc = Utc(2026, 8, 2),
            UpdatedAtUtc = Utc(2026, 8, 2)
        });
        geographyDb.지역농수산Map행정구역Boundaries.Add(new()
        {
            Region = suwon,
            BoundarySourceCode = "KR-SGIS-HADM",
            BoundaryVintage = "2025",
            GeometryReference = "object://verified-suwon-boundary",
            AnchorLatitude = 37.2636m,
            AnchorLongitude = 127.0286m,
            SimplificationLevel = 0,
            SourceUrl = "https://sgis.kostat.go.kr/",
            VerifiedAtUtc = Utc(2026, 8, 2),
            CreatedAtUtc = Utc(2026, 8, 2),
            UpdatedAtUtc = Utc(2026, 8, 2)
        });
        await geographyDb.SaveChangesAsync();
        var store = new 경기데이터드림가축사육MapSnapshotStore();
        store.Replace(new 경기데이터드림가축사육MapSnapshot(
            CollectedAt,
            [
                new() { RegionCode = "41110", RegionName = "수원시", BusinessStatus = "영업", BusinessCount = 7 },
                new() { RegionCode = "41110", RegionName = "수원시", BusinessStatus = "폐업", BusinessCount = 2 },
                new() { RegionCode = "99999", RegionName = "미연결", BusinessStatus = "영업", BusinessCount = 99 }
            ]));
        var sut = new 경기데이터드림가축사육MapMarkerReader(
            store,
            geographyDb,
            new FixedTimeProvider(CollectedAt.AddDays(1)));

        var observations = await sut.공개Marker조회Async();

        var marker = Assert.Single(observations);
        Assert.Equal(커뮤니티세계지도LayerCodes.GyeonggiLivestockPublicEvidence, marker.LayerCode);
        Assert.Equal("gyeonggi-livestock:kr-gg-suwon", marker.StableId);
        Assert.Equal(37.2636, marker.Latitude, 4);
        Assert.Equal(127.0286, marker.Longitude, 4);
        Assert.Equal(
            커뮤니티세계지도위치정밀도Codes.AdministrativeRegionRepresentative,
            marker.LocationPrecisionCode);
        Assert.Equal(경기데이터드림가축사육MapMarkerReader.DatasetKey, marker.SourceDatasetKey);
        Assert.Equal(9m, marker.Metrics?.Single(metric => metric.Code == "total").Value);
        Assert.Equal(7m, marker.Metrics?.Single(metric => metric.DisplayName == "영업").Value);
        Assert.Equal(커뮤니티세계지도FreshnessCodes.Fresh, marker.FreshnessCode);
        Assert.Contains("실제 농장", marker.BoundaryNotice, StringComparison.Ordinal);
        Assert.DoesNotContain(observations, item => item.Summary.Contains("99", StringComparison.Ordinal));
    }

    [Fact]
    public async Task 시군코드가없는현재원본은_NaturalEarth경기도대표점의도단위집계Marker로반환한다()
    {
        await using var geographyDb = CreateGeographyDb();
        var store = new 경기데이터드림가축사육MapSnapshotStore();
        store.Replace(new 경기데이터드림가축사육MapSnapshot(
            CollectedAt,
            [
                new()
                {
                    RegionCode = 경기데이터드림가축사육집계Client.DatasetScopeRegionCode,
                    RegionName = 경기데이터드림가축사육집계Client.DatasetScopeRegionName,
                    BusinessStatus = "영업",
                    BusinessCount = 21000
                },
                new()
                {
                    RegionCode = 경기데이터드림가축사육집계Client.DatasetScopeRegionCode,
                    RegionName = 경기데이터드림가축사육집계Client.DatasetScopeRegionName,
                    BusinessStatus = "폐업",
                    BusinessCount = 696
                }
            ]));
        var sut = new 경기데이터드림가축사육MapMarkerReader(
            store,
            geographyDb,
            new FixedTimeProvider(CollectedAt.AddDays(1)));

        var observations = await sut.공개Marker조회Async();

        var marker = Assert.Single(observations);
        var anchor = 지역문화행정구역대표점Catalog.All.Single(item => item.RegionKey == "kr-gyeonggi");
        Assert.Equal("gyeonggi-livestock:kr-gyeonggi", marker.StableId);
        Assert.Equal(anchor.Latitude, marker.Latitude, 4);
        Assert.Equal(anchor.Longitude, marker.Longitude, 4);
        Assert.Equal(21696m, marker.Metrics?.Single(metric => metric.Code == "total").Value);
        Assert.Contains(지역문화행정구역대표점Catalog.SourceName, marker.SourceName, StringComparison.Ordinal);
        Assert.Contains("경기도 행정구역 대표점", marker.BoundaryNotice, StringComparison.Ordinal);
    }

    private static 경기데이터드림가축사육집계Response Response(
        bool success,
        params 경기데이터드림가축사육지역집계[] items)
        => new()
        {
            Success = success,
            ErrorMessage = success ? string.Empty : "temporary failure",
            ObservedAt = CollectedAt,
            Items = items
        };

    private static 지역농수산Map행정구역 Region(string key, string name)
        => new()
        {
            Id = Guid.NewGuid(),
            PublicRegionKey = key,
            CountryCode = RegionalAgriculturalMapCountryCodes.Korea,
            RegionTypeCode = RegionalAgriculturalMapRegionTypeCodes.CountyMunicipality,
            DisplayNameKo = name,
            DisplayNameEn = name,
            DisplayNameLocal = name,
            CreatedAtUtc = Utc(2026, 8, 2),
            UpdatedAtUtc = Utc(2026, 8, 2)
        };

    private static SsalddelContext CreateGeographyDb()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"gyeonggi-livestock-map-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static DateTime Utc(int year, int month, int day)
        => new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    private sealed class Stub가축사육집계Client(경기데이터드림가축사육집계Response response)
        : I경기데이터드림가축사육집계Client
    {
        public Task<경기데이터드림가축사육집계Response> QueryAsync(
            string? regionCode = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(response);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
