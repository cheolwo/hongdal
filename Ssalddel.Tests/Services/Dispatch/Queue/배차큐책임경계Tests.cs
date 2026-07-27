using 살뜰.Services.Dispatch.Queue;

namespace Ssalddel.Tests.Services.Dispatch.Queue;

public sealed class 배차큐책임경계Tests
{
    [Fact]
    public void 배차대기와_운송원장은_업무확정근거로_분류된다()
    {
        Assert.True(배차큐책임경계.업무확정근거인가("배차대기"));
        Assert.True(배차큐책임경계.업무확정근거인가("화주운송의뢰"));
        Assert.True(배차큐책임경계.업무확정근거인가("운송원장"));
    }

    [Fact]
    public void 기사상태와_위치Store는_재구성가능한_실행인덱스다()
    {
        Assert.True(배차큐책임경계.재구성가능한실행인덱스인가("I국내화물운송기사상태Store"));
        Assert.True(배차큐책임경계.재구성가능한실행인덱스인가("IDriverWorkQueueStore"));
        Assert.True(배차큐책임경계.재구성가능한실행인덱스인가("IDriverLocationStore"));
        Assert.True(배차큐책임경계.재구성가능한실행인덱스인가("I배달권실행공간Store"));
    }

    [Fact]
    public void 실행인덱스는_업무확정근거가_아니다()
    {
        foreach (var item in 배차큐책임경계.실행인덱스목록())
        {
            Assert.False(item.업무확정근거);
            Assert.True(item.서버재시작후재구성가능);
            Assert.True(item.장애시손실허용);
        }
    }

    [Fact]
    public void 배달권실행공간은_사전과_해시셋_성격의_실행인덱스다()
    {
        var item = Assert.Single(배차큐책임경계.실행인덱스목록(), x => x.이름 == "I배달권실행공간Store");

        Assert.Contains("사전", item.설명);
        Assert.Contains("해시셋", item.설명);
        Assert.False(item.업무확정근거);
    }

    [Fact]
    public void 원장배달권은_플랫폼_영속투영이고_기사실행인덱스와_구분된다()
    {
        var projection = Assert.Single(
            배차큐책임경계.업무투영목록(),
            x => x.이름 == "원장배달권투영");

        Assert.False(projection.업무확정근거);
        Assert.True(projection.서버재시작후재구성가능);
        Assert.False(projection.장애시손실허용);
        Assert.DoesNotContain(
            배차큐책임경계.실행인덱스목록(),
            x => x.이름 == projection.이름);
    }

    [Fact]
    public void 미처리운송의뢰_재구성기준은_확정과_종료를_제외한다()
    {
        Assert.Contains("배차대기.상태 == 대기", 배차큐책임경계.미처리운송의뢰쿼리기준);
        Assert.Contains("배차대기.배차큐단계 != 확정", 배차큐책임경계.미처리운송의뢰쿼리기준);
        Assert.Contains("배차대기.배차큐단계 != 종료", 배차큐책임경계.미처리운송의뢰쿼리기준);
    }
}
