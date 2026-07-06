using Hongdal.Application.Shipper.Request;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Tests.Application.Shipper.Request;

public class 화주운송요금정책검토ServiceTests
{
    private readonly 화주운송요금정책검토Service _service = new();

    [Fact]
    public void 검토_재알선금지인데_2차알선이면_정책위반으로_판정한다()
    {
        var pricing = new PricingDTO
        {
            알선정책 = new 화주운송알선정책DTO
            {
                재알선금지 = true,
                알선단계 = 2
            }
        };

        var result = _service.검토(pricing, 결제예정금액: 50000);

        Assert.True(result.정책위반);
        Assert.True(result.재알선의심);
        Assert.Contains(화주운송요금정책이벤트코드.재알선차단필요, result.이벤트코드목록);
    }

    [Fact]
    public void 검토_결제예정금액이_기준운임보다_낮으면_기준운임미달_이벤트를_남긴다()
    {
        var pricing = new PricingDTO
        {
            기본운임 = 30000,
            예상거리Km = 10,
            Km당단가 = 1200,
            최소운임 = 45000
        };

        var result = _service.검토(pricing, 결제예정금액: 40000);

        Assert.True(result.정책위반);
        Assert.Equal(45000, result.기준운임);
        Assert.Contains(화주운송요금정책이벤트코드.기준운임미달, result.이벤트코드목록);
    }

    [Fact]
    public void 검토_화주결제액과_기사지급액_사이에_설명되지않은_차액이_있으면_재알선의심으로_판정한다()
    {
        var pricing = new PricingDTO
        {
            플랫폼수수료 = 3000,
            기사지급예정운임 = 42000,
            알선정책 = new 화주운송알선정책DTO
            {
                재알선금지 = true,
                알선단계 = 1
            }
        };

        var result = _service.검토(pricing, 결제예정금액: 50000);

        Assert.False(result.정책위반);
        Assert.True(result.재알선의심);
        Assert.Equal(5000, result.화주기사운임차액);
        Assert.Contains(화주운송요금정책이벤트코드.재알선의심, result.이벤트코드목록);
    }
}
