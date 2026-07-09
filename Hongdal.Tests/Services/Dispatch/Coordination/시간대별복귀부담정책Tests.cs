using 홍달.Services.Dispatch.Coordination;

namespace Hongdal.Tests.Services.Dispatch.Coordination;

public sealed class 시간대별복귀부담정책Tests
{
    [Fact]
    public void 평가_퇴근시간대_수익우선은_복귀부담감점을_낮춘다()
    {
        var 기준시각Utc = new DateTime(2026, 7, 9, 9, 0, 0, DateTimeKind.Utc);

        var 균형 = 시간대별복귀부담정책.평가(기준시각Utc, 100m, 기사복귀선호코드.균형);
        var 수익우선 = 시간대별복귀부담정책.평가(기준시각Utc, 100m, 기사복귀선호코드.수익우선);
        var 복귀우선 = 시간대별복귀부담정책.평가(기준시각Utc, 100m, 기사복귀선호코드.복귀우선);

        Assert.True(수익우선.부담점수 < 균형.부담점수);
        Assert.True(복귀우선.부담점수 > 균형.부담점수);
        Assert.Equal(기사복귀선호코드.수익우선, 수익우선.복귀콜선호);
    }

    [Fact]
    public void 평가_퇴근시간대_복귀우선은_복귀방향콜_보너스를_높인다()
    {
        var 기준시각Utc = new DateTime(2026, 7, 9, 9, 0, 0, DateTimeKind.Utc);

        var 균형 = 시간대별복귀부담정책.평가(기준시각Utc, 10m, 기사복귀선호코드.균형);
        var 복귀우선 = 시간대별복귀부담정책.평가(기준시각Utc, 10m, 기사복귀선호코드.복귀우선);
        var 수익우선 = 시간대별복귀부담정책.평가(기준시각Utc, 10m, 기사복귀선호코드.수익우선);

        Assert.True(복귀우선.보너스점수 > 균형.보너스점수);
        Assert.True(수익우선.보너스점수 < 균형.보너스점수);
        Assert.False(복귀우선.퇴근시간대부담여부);
    }
}
