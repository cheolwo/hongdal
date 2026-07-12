using 홍달.도메인.공통;
using 홍달.도메인.배차;
using 홍달.도메인.기사;
using 홍달.도메인.화주;
using 홍달.Services.Dispatch.Coordination;
using 홍달.Services.Storage.Local;

namespace Hongdal.Tests.Services.Dispatch.Coordination;

public sealed class 국내화물배차후보금지정책Tests
{
    [Fact]
    public void 운송의뢰후보금지는_추천잠금과_좌표누락을_하드_금지로_본다()
    {
        var now = DateTime.UtcNow;
        var queue = new 운송원장
        {
            의뢰Id = "REQ-1",
            배차업무유형 = 상태값.배차업무유형.용달운송,
            상태 = 상태값.배차대기상태.대기,
            배차큐단계 = 상태값.배차큐단계.배차추천,
            배차노출상태 = 상태값.배차노출상태.추천중,
            현재추천대상기사Id = "DRV-1",
            추천만료시각 = now.AddSeconds(30)
        };

        var reasons = 국내화물배차후보금지정책.운송의뢰후보금지사유(
            queue,
            new 화주운송의뢰 { 의뢰Id = "REQ-1" },
            now);

        Assert.Contains("다른 기사에게 추천 잠금 중인 운송 의뢰입니다.", reasons);
        Assert.Contains("상차지 좌표가 없습니다.", reasons);
        Assert.Contains("하차지 좌표가 없습니다.", reasons);
    }

    [Fact]
    public void 기사후보금지는_운행상태_위치_수락건수를_하드_금지로_본다()
    {
        var state = new 국내화물운송기사상태Snapshot(
            "DRV-1",
            ShiftId: null,
            운행상태: 상태값.기사운행상태.대기,
            운행시작시각Utc: null,
            Aging기준시각Utc: DateTime.UtcNow,
            Aging점수: 0m,
            Latitude: null,
            Longitude: null,
            AccuracyM: null,
            위치기록시각Utc: null,
            위치수신시각Utc: null,
            마지막추천시각Utc: null,
            마지막후보없음시각Utc: null,
            후보없음횟수: 0,
            StartMode: null,
            StartLocation: null,
            ReturnDestination: null);

        var reasons = 국내화물배차후보금지정책.기사후보금지사유(
            state,
            new 용달기사 { 기사Id = "DRV-1", 상태 = "활동중" },
            현재수락운송건수: 2,
            기사최대수락운송건수: 2);

        Assert.Contains("기사가 운행중 상태가 아닙니다.", reasons);
        Assert.Contains("기사 현재 위치가 없습니다.", reasons);
        Assert.Contains("이미 수락한 진행 중 운송이 2건입니다.", reasons);
    }

    [Fact]
    public void 조합후보금지는_이미_거절한_기사_조합을_금지한다()
    {
        var reasons = 국내화물배차후보금지정책.조합후보금지사유(
            "DRV-1",
            new HashSet<string>(["DRV-1"], StringComparer.Ordinal));

        Assert.Single(reasons);
        Assert.Equal("기사가 이미 거절한 의뢰입니다.", reasons[0]);
    }
}
