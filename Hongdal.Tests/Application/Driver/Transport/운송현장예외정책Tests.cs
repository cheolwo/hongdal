using Hongdal.Application.Driver.Transport;

namespace Hongdal.Tests.Application.Driver.Transport;

public class 운송현장예외정책Tests
{
    [Fact]
    public void 정리_상차물건없음은_상차단계와_관리자확인으로_분류한다()
    {
        var result = 운송현장예외정책.정리(null, "상차물건없음", null, false);

        Assert.Equal("상차", result.단계);
        Assert.Equal("상차물건없음", result.예외코드);
        Assert.True(result.관리자확인필요);
        Assert.Contains("관리자 확인", result.다음행동안내);
    }

    [Fact]
    public void 정리_증빙업로드실패는_임시보관과_재업로드를_안내한다()
    {
        var result = 운송현장예외정책.정리(null, "증빙업로드실패", null, false);

        Assert.Equal("증빙", result.단계);
        Assert.False(result.관리자확인필요);
        Assert.Contains("임시 보관", result.다음행동안내);
        Assert.Contains("다시 업로드", result.다음행동안내);
    }

    [Fact]
    public void 정리_직접입력한_사유와_관리자확인요청을_우선반영한다()
    {
        var result = 운송현장예외정책.정리("하차", "기타", "관리실에서 하차 보류 요청", true);

        Assert.Equal("하차", result.단계);
        Assert.Equal("기타", result.예외코드);
        Assert.Equal("관리실에서 하차 보류 요청", result.사유);
        Assert.True(result.관리자확인필요);
    }
}
