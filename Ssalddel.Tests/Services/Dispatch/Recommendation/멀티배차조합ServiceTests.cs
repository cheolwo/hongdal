using 살뜰.Services.Dispatch.Recommendation;
using 살뜰.Services.Storage.Local;
using 살뜰.도메인.공통;
using 살뜰.도메인.기사;

namespace Ssalddel.Tests.Services.Dispatch.Recommendation;

public sealed class 음식멀티배차조합ServiceTests
{
    [Fact]
    public void 조합생성은_대기_의뢰를_두건씩_멀티배차_후보로_만든다()
    {
        var service = new 음식멀티배차조합Service(new 직선거리경로Service());
        var now = new DateTime(2026, 7, 10, 1, 0, 0, DateTimeKind.Utc);

        var result = service.조합생성(new 멀티배차조합요청(
            [
                CreateJob("A", 0m, 0m, 5m, 0m, now.AddMinutes(20)),
                CreateJob("B", 1m, 0m, 6m, 0m, now.AddMinutes(25)),
                CreateJob("C", 20m, 0m, 21m, 0m, now.AddMinutes(90))
            ],
            단건후보포함: false));

        Assert.Equal(3, result.Count);
        Assert.All(result, x => Assert.Equal("멀티배차", x.배차묶음유형));
        Assert.Equal("A+B", result[0].조합키);
        Assert.Equal(["A", "B"], result[0].의뢰Ids);
        Assert.True(result[0].조합가능여부);
        Assert.Contains("상차지근접", result[0].배지);
        Assert.Contains("하차권역근접", result[0].배지);
        Assert.Contains("판단근거반영", result[0].배지);
        Assert.Contains(result[0].경고, x => x.StartsWith("판단근거=", StringComparison.Ordinal));
    }

    [Fact]
    public void 조합생성은_단건_후보를_남겨_홀수_의뢰를_처리할_수_있게_한다()
    {
        var service = new 음식멀티배차조합Service(new 직선거리경로Service());

        var result = service.조합생성(new 멀티배차조합요청(
            [
                CreateJob("A", 0m, 0m, 5m, 0m, null),
                CreateJob("B", 1m, 0m, 6m, 0m, null),
                CreateJob("C", 20m, 0m, 21m, 0m, null)
            ]));

        Assert.Contains(result, x => x.배차묶음유형 == "단건배차" && x.조합키 == "C");
        Assert.Contains(result, x => x.배차묶음유형 == "멀티배차" && x.조합키 == "A+B");
    }

    [Fact]
    public void 조합생성은_좌표가_부족한_쌍에_경고를_남긴다()
    {
        var service = new 음식멀티배차조합Service(new 직선거리경로Service());

        var result = service.조합생성(new 멀티배차조합요청(
            [
                CreateJob("A", null, null, 5m, 0m, null),
                CreateJob("B", 1m, 0m, 6m, 0m, null)
            ],
            단건후보포함: false));

        var candidate = Assert.Single(result);
        Assert.False(candidate.조합가능여부);
        Assert.Contains(candidate.제외사유!, x => x.Contains("상차지 좌표", StringComparison.Ordinal));
    }

    [Fact]
    public void 조합생성은_용달운송에서는_멀티배차_후보를_만들지_않는다()
    {
        var service = new 음식멀티배차조합Service(new 직선거리경로Service());

        var result = service.조합생성(new 멀티배차조합요청(
            [
                CreateJob("A", 0m, 0m, 5m, 0m, null),
                CreateJob("B", 1m, 0m, 6m, 0m, null)
            ],
            배차업무유형: 상태값.배차업무유형.용달운송));

        Assert.All(result, x => Assert.Equal("단건배차", x.배차묶음유형));
    }

    [Fact]
    public void 조합생성은_상하차_거리나_시간창이_큰_음식묶음을_부적합으로_표시한다()
    {
        var service = new 음식멀티배차조합Service(new 직선거리경로Service());
        var now = new DateTime(2026, 7, 10, 1, 0, 0, DateTimeKind.Utc);

        var result = service.조합생성(new 멀티배차조합요청(
            [
                CreateJob("A", 0m, 0m, 0m, 0m, now.AddMinutes(10)),
                CreateJob("B", 10m, 0m, 12m, 0m, now.AddMinutes(80))
            ],
            단건후보포함: false,
            최대상차지간거리Km: 3m,
            최대하차지간거리Km: 6m,
            픽업시간창권장차이분: 20m));

        var candidate = Assert.Single(result);
        Assert.False(candidate.조합가능여부);
        Assert.Contains(candidate.제외사유!, x => x.Contains("상차지 간 거리", StringComparison.Ordinal));
        Assert.Contains(candidate.제외사유!, x => x.Contains("하차지 간 거리", StringComparison.Ordinal));
        Assert.Contains(candidate.제외사유!, x => x.Contains("상차 시간창", StringComparison.Ordinal));
    }

    [Fact]
    public void 조합생성은_같은배달권과_인접배달권만_음식멀티배차로_허용한다()
    {
        var service = new 음식멀티배차조합Service(new 직선거리경로Service());
        const string 마포구 = "bjd-sigungu:11440";
        const string 서대문구 = "bjd-sigungu:11410";
        const string 강동구 = "bjd-sigungu:11740";

        var sameScope = service.조합생성(new 멀티배차조합요청(
            [
                CreateJob("A", 0m, 0m, 1m, 0m, null, 마포구),
                CreateJob("B", 0.5m, 0m, 1.2m, 0m, null, 마포구)
            ],
            단건후보포함: false));
        Assert.True(Assert.Single(sameScope).조합가능여부);
        Assert.Contains("같은배달권", sameScope[0].배지);

        var adjacentScope = service.조합생성(new 멀티배차조합요청(
            [
                CreateJob("A", 0m, 0m, 1m, 0m, null, 마포구),
                CreateJob("B", 0.5m, 0m, 1.2m, 0m, null, 서대문구)
            ],
            단건후보포함: false));
        Assert.True(Assert.Single(adjacentScope).조합가능여부);
        Assert.Contains("인접배달권", adjacentScope[0].배지);

        var nonAdjacentScope = service.조합생성(new 멀티배차조합요청(
            [
                CreateJob("A", 0m, 0m, 1m, 0m, null, 마포구),
                CreateJob("B", 0.5m, 0m, 1.2m, 0m, null, 강동구)
            ],
            단건후보포함: false));
        var candidate = Assert.Single(nonAdjacentScope);
        Assert.False(candidate.조합가능여부);
        Assert.Contains(candidate.제외사유!, x => x.Contains("비인접 배달권", StringComparison.Ordinal));
    }

    [Fact]
    public void 조합생성은_묶음내_예상거리가_상한을_넘으면_제외한다()
    {
        var service = new 음식멀티배차조합Service(new 직선거리경로Service());

        var result = service.조합생성(new 멀티배차조합요청(
            [
                CreateJob("A", 0m, 0m, 4m, 0m, null, "bjd-sigungu:11440"),
                CreateJob("B", 1m, 0m, 5m, 0m, null, "bjd-sigungu:11440")
            ],
            단건후보포함: false,
            좌표근사총거리상한Km: 3m));

        var candidate = Assert.Single(result);
        Assert.False(candidate.조합가능여부);
        Assert.True(candidate.묶음내예상거리Km > 3m);
        Assert.Contains(candidate.제외사유!, x => x.Contains("예상 운행거리", StringComparison.Ordinal));
    }

    private static 픽업하차경로작업 CreateJob(
        string id,
        decimal? pickupLat,
        decimal? pickupLng,
        decimal? dropoffLat,
        decimal? dropoffLng,
        DateTime? pickupWindowEnd,
        string? dropoffScopeKey = null)
        => new(
            id,
            $"상차지 {id}",
            pickupLat.HasValue && pickupLng.HasValue ? new 배차경로좌표(pickupLat.Value, pickupLng.Value) : null,
            pickupWindowEnd,
            $"하차지 {id}",
            dropoffLat.HasValue && dropoffLng.HasValue ? new 배차경로좌표(dropoffLat.Value, dropoffLng.Value) : null,
            null,
            하차배달권키: dropoffScopeKey);

    private sealed class 직선거리경로Service : I배차추천경로Service
    {
        public Task<배차경로좌표?> ResolveOriginLocationAsync(string driverId, 용달기사? driver, DriverLocationSnapshot? currentLocation, 배차추천검색조건? criteria)
            => Task.FromResult<배차경로좌표?>(null);

        public Task<배차경로좌표?> ResolveRouteAnchorLocationAsync(string driverId, 용달기사? driver, DriverLocationSnapshot? currentLocation)
            => Task.FromResult<배차경로좌표?>(null);

        public Task<배차경로예상결과?> EstimateRouteAsync(배차경로좌표? origin, 배차경로좌표? destination)
            => Task.FromResult<배차경로예상결과?>(null);

        public Task<배차경로예상결과?> EstimateOrderedRouteAsync(배차경로좌표? origin, IReadOnlyList<배차경로좌표> orderedStops, CancellationToken cancellationToken = default)
            => Task.FromResult<배차경로예상결과?>(null);

        public Task<배차삽입경로예상결과?> EstimateInsertionDelayAsync(배차경로좌표? origin, 배차경로좌표? routeAnchor, 배차경로좌표? pickup, 배차경로좌표? dropoff)
            => Task.FromResult<배차삽입경로예상결과?>(null);

        public decimal? CalculateDistanceKm(배차경로좌표 source, 배차경로좌표 target)
            => Math.Abs(target.Latitude - source.Latitude) + Math.Abs(target.Longitude - source.Longitude);
    }
}
