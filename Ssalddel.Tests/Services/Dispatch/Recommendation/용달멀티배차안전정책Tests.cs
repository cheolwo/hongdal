using 살뜰.Services.Dispatch.Coordination;
using 살뜰.Services.Dispatch.Recommendation;

namespace Ssalddel.Tests.Services.Dispatch.Recommendation;

public sealed class 용달멀티배차안전정책Tests
{
    [Fact]
    public void 판정은_기사_동의가_없으면_차단한다()
    {
        var result = 용달멀티배차안전정책.판정(기본요청(기사명시동의: false));

        Assert.False(result.허용여부);
        Assert.Contains(result.차단사유, x => x.Contains("기사님", StringComparison.Ordinal));
    }

    [Fact]
    public void 판정은_화주가_혼적을_허용하지_않으면_차단한다()
    {
        var result = 용달멀티배차안전정책.판정(기본요청(화주혼적허용: false));

        Assert.False(result.허용여부);
        Assert.Contains(result.차단사유, x => x.Contains("화주", StringComparison.Ordinal));
    }

    [Fact]
    public void 판정은_장거리_무휴식_멀티배차를_차단한다()
    {
        var result = 용달멀티배차안전정책.판정(기본요청(
            총운행거리Km: 170m,
            예상연속운전분: 270m,
            휴식삽입가능: false));

        Assert.False(result.허용여부);
        Assert.Contains(result.차단사유, x => x.Contains("연속 운전", StringComparison.Ordinal));
    }

    [Fact]
    public void 판정은_민감화물과_독차필수_화물을_차단한다()
    {
        var result = 용달멀티배차안전정책.판정(기본요청(
            독차필수: true,
            민감화물: true));

        Assert.False(result.허용여부);
        Assert.Contains(result.차단사유, x => x.Contains("독차", StringComparison.Ordinal));
        Assert.Contains(result.차단사유, x => x.Contains("민감 화물", StringComparison.Ordinal));
    }

    [Fact]
    public void 판정은_짧은_2건_용달묶음을_통과시킨다()
    {
        var result = 용달멀티배차안전정책.판정(기본요청(
            총운행거리Km: 58m,
            예상연속운전분: 110m,
            하차후복귀거리Km: 20m));

        Assert.True(result.허용여부);
        Assert.Empty(result.차단사유);
        Assert.True(result.우선순위감점 <= 1m);
    }

    [Fact]
    public void 판정은_복귀우선_기사의_큰_복귀부담을_차단한다()
    {
        var result = 용달멀티배차안전정책.판정(기본요청(
            하차후복귀거리Km: 120m,
            기사복귀선호: 기사복귀선호코드.복귀우선));

        Assert.False(result.허용여부);
        Assert.Contains(result.차단사유, x => x.Contains("복귀 우선", StringComparison.Ordinal));
    }

    private static 용달멀티배차안전검토요청 기본요청(
        bool 기사명시동의 = true,
        bool 화주혼적허용 = true,
        bool 독차필수 = false,
        bool 민감화물 = false,
        decimal? 총운행거리Km = 80m,
        decimal? 예상연속운전분 = 140m,
        bool 휴식삽입가능 = true,
        decimal? 하차후복귀거리Km = 10m,
        string? 기사복귀선호 = null)
        => new(
            작업수: 2,
            기사명시동의: 기사명시동의,
            화주혼적허용: 화주혼적허용,
            독차필수: 독차필수,
            민감화물: 민감화물,
            총운행거리Km: 총운행거리Km,
            예상연속운전분: 예상연속운전분,
            휴식삽입가능: 휴식삽입가능,
            하차후복귀거리Km: 하차후복귀거리Km,
            기사복귀선호: 기사복귀선호);
}
