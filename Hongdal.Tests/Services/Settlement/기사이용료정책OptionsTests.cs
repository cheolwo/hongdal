using 홍달.Services.Options;

namespace Hongdal.Tests.Services.Settlement;

public sealed class 기사이용료정책OptionsTests
{
    [Fact]
    public void 건당이용료를_월상한까지만_계산한다()
    {
        var policy = new 기사이용료정책Options
        {
            무료배차 = false,
            기본이용료 = 500m,
            월상한이용료 = 5000m
        };

        Assert.Equal(0m, policy.월누적이용료계산(0));
        Assert.Equal(500m, policy.월누적이용료계산(1));
        Assert.Equal(5000m, policy.월누적이용료계산(10));
        Assert.Equal(5000m, policy.월누적이용료계산(11));
    }

    [Fact]
    public void 무료배차면_이용료와_월상한을_0원으로_본다()
    {
        var policy = new 기사이용료정책Options
        {
            무료배차 = true,
            기본이용료 = 500m,
            월상한이용료 = 5000m
        };

        Assert.Equal(0m, policy.적용월상한이용료);
        Assert.Equal(0m, policy.월누적이용료계산(20));
    }

    [Fact]
    public void 기존_추가이용료_설정명도_월상한이용료로_호환한다()
    {
        var policy = new 기사이용료정책Options
        {
            무료배차 = false,
            기본이용료 = 500m,
            추가이용료 = 3000m
        };

        Assert.Equal(3000m, policy.월상한이용료);
        Assert.Equal(3000m, policy.적용월상한이용료);
        Assert.Equal(3000m, policy.월누적이용료계산(10));
    }
}
