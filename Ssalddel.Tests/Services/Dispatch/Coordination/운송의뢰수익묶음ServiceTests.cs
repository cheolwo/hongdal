using 살뜰.Services.Dispatch.Coordination;
using 살뜰.Services.Dispatch.Recommendation;

namespace Ssalddel.Tests.Services.Dispatch.Coordination;

public sealed class 운송의뢰수익묶음ServiceTests
{
    [Fact]
    public void 묶음생성은_플랫폼_예상순이익이_큰_묶음을_우선한다()
    {
        var service = new 운송의뢰수익묶음Service();
        var now = new DateTime(2026, 7, 11, 1, 0, 0, DateTimeKind.Utc);

        var result = service.묶음생성(new 운송의뢰수익묶음요청(
            [
                Request("REQ-A", "scope-1", 80_000m, 30_000m, 0m, 0m, 1m, 0m, now),
                Request("REQ-B", "scope-1", 75_000m, 25_000m, 0.2m, 0m, 1.2m, 0m, now.AddMinutes(20)),
                Request("REQ-C", "scope-2", 35_000m, 30_000m, 10m, 0m, 11m, 0m, now)
            ],
            단건후보포함: false));

        Assert.Equal("REQ-A+REQ-B", result[0].묶음키);
        Assert.All(result.Where(x => x.묶음가능여부), x => Assert.True(result[0].예상플랫폼순이익 >= x.예상플랫폼순이익));
        Assert.Contains("같은배달권", result[0].배지);
        Assert.Contains("DCT-POLICY-PLATFORM-BUNDLE", result[0].선택근거);
    }

    [Fact]
    public void 묶음생성은_멀티배차_미허용_의뢰가_있으면_묶음을_차단한다()
    {
        var service = new 운송의뢰수익묶음Service();

        var result = service.묶음생성(new 운송의뢰수익묶음요청(
            [
                Request("REQ-A", "scope-1", 80_000m, 30_000m, 0m, 0m, 1m, 0m, 멀티배차허용: false),
                Request("REQ-B", "scope-1", 75_000m, 25_000m, 0.2m, 0m, 1.2m, 0m)
            ],
            단건후보포함: false));

        var candidate = Assert.Single(result);
        Assert.False(candidate.묶음가능여부);
        Assert.Contains(candidate.제외사유, x => x.Contains("멀티배차", StringComparison.Ordinal));
    }

    [Fact]
    public void 묶음생성은_같은_권역_근접_의뢰에_보너스를_준다()
    {
        var service = new 운송의뢰수익묶음Service();

        var result = service.묶음생성(new 운송의뢰수익묶음요청(
            [
                Request("REQ-A", "scope-1", 50_000m, 25_000m, 0m, 0m, 1m, 0m),
                Request("REQ-B", "scope-1", 50_000m, 25_000m, 0.01m, 0m, 1.01m, 0m)
            ],
            단건후보포함: false));

        var candidate = Assert.Single(result);
        Assert.True(candidate.묶음가능여부);
        Assert.Contains("같은배달권", candidate.배지);
        Assert.Contains("상차지근접", candidate.배지);
        Assert.Contains("하차지근접", candidate.배지);
    }

    [Fact]
    public void 묶음생성은_정책상_세건_묶음도_후보로_만든다()
    {
        var service = new 운송의뢰수익묶음Service();

        var result = service.묶음생성(new 운송의뢰수익묶음요청(
            [
                Request("REQ-A", "scope-1", 50_000m, 20_000m, 0m, 0m, 1m, 0m),
                Request("REQ-B", "scope-1", 50_000m, 20_000m, 0.01m, 0m, 1.01m, 0m),
                Request("REQ-C", "scope-1", 50_000m, 20_000m, 0.02m, 0m, 1.02m, 0m)
            ],
            최대묶음크기: 3,
            단건후보포함: false));

        var candidate = Assert.Single(result, x => x.묶음키 == "REQ-A+REQ-B+REQ-C");
        Assert.Equal(3, candidate.묶음크기);
        Assert.True(candidate.묶음가능여부);
        Assert.Contains("3건묶음", candidate.배지);
    }

    [Fact]
    public void 묶음생성은_목표_건당_수익_미달_묶음을_차단한다()
    {
        var service = new 운송의뢰수익묶음Service();

        var result = service.묶음생성(new 운송의뢰수익묶음요청(
            [
                Request("REQ-A", "scope-1", 10_000m, 9_800m, 0m, 0m, 1m, 0m),
                Request("REQ-B", "scope-1", 10_000m, 9_800m, 0.01m, 0m, 1.01m, 0m)
            ],
            단건후보포함: false,
            목표건당플랫폼순이익: 1_500m));

        var candidate = Assert.Single(result);
        Assert.False(candidate.묶음가능여부);
        Assert.True(candidate.예상건당플랫폼순이익 < 1_500m);
        Assert.Contains(candidate.제외사유, x => x.Contains("건당 예상 순이익", StringComparison.Ordinal));
    }

    private static 운송의뢰수익묶음대상 Request(
        string id,
        string scope,
        decimal revenue,
        decimal cost,
        decimal pickupLat,
        decimal pickupLng,
        decimal dropoffLat,
        decimal dropoffLng,
        DateTime? pickupWindowEnd = null,
        bool 멀티배차허용 = true)
        => new(
            id,
            scope,
            revenue,
            cost,
            new 배차경로좌표(pickupLat, pickupLng),
            new 배차경로좌표(dropoffLat, dropoffLng),
            pickupWindowEnd,
            멀티배차허용);
}
