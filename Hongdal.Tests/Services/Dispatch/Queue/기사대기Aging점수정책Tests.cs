using 홍달.Services.Dispatch.Queue;

namespace Hongdal.Tests.Services.Dispatch.Queue;

public sealed class 기사대기Aging점수정책Tests
{
    [Fact]
    public void 추천대기_30분_미만이면_보정하지_않는다()
    {
        var now = new DateTime(2026, 7, 8, 12, 0, 0, DateTimeKind.Utc);

        var score = 기사대기Aging점수정책.계산(now.AddMinutes(-29), now);

        Assert.Equal(0m, score);
    }

    [Fact]
    public void 추천대기_시간이_길어지면_30분마다_점수를_올린다()
    {
        var now = new DateTime(2026, 7, 8, 12, 0, 0, DateTimeKind.Utc);

        var score = 기사대기Aging점수정책.계산(now.AddMinutes(-95), now);

        Assert.Equal(9m, score);
    }

    [Fact]
    public void 추천대기_보정은_최대점수를_넘지_않는다()
    {
        var now = new DateTime(2026, 7, 8, 12, 0, 0, DateTimeKind.Utc);

        var score = 기사대기Aging점수정책.계산(now.AddHours(-24), now);

        Assert.Equal(기사대기Aging점수정책.최대점수, score);
    }
}
