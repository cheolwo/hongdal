using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Tests.Contracts.Common.Orderer;

public sealed class 해외구매통관목적정책Tests
{
    [Fact]
    public void 개인소비는_개인통관부호입력대상이지만_면세확정으로표시하지않는다()
    {
        var result = 주문자해외구매통관정책.안내(공동구매거래유형코드.B2C);

        Assert.Equal(해외구매통관목적코드.개인자가사용, result.수입목적코드);
        Assert.Equal(해외구매소액면세판정코드.추가정보필요, result.판정코드);
        Assert.True(result.개인통관고유부호입력대상);
        Assert.True(result.자가사용소액면세검토대상);
        Assert.False(result.관세부가세예상비용검토필요);
        Assert.Contains("면세 보증이 아닙니다", result.핵심안내, StringComparison.Ordinal);
    }

    [Fact]
    public void 사업판매목적은_개인통관과자가사용면세경로를열지않는다()
    {
        var result = 주문자해외구매통관정책.안내(공동구매거래유형코드.B2B);

        Assert.Equal(해외구매통관목적코드.사업판매사용, result.수입목적코드);
        Assert.Equal(해외구매소액면세판정코드.사업수입경로, result.판정코드);
        Assert.False(result.개인통관고유부호입력대상);
        Assert.False(result.자가사용소액면세검토대상);
        Assert.True(result.관세부가세예상비용검토필요);
    }

    [Theory]
    [InlineData(150, false, 해외구매소액면세판정코드.자가사용면세후보)]
    [InlineData(151, false, 해외구매소액면세판정코드.과세검토필요)]
    [InlineData(200, true, 해외구매소액면세판정코드.자가사용면세후보)]
    [InlineData(201, true, 해외구매소액면세판정코드.과세검토필요)]
    public void 자가사용후보는_일반150달러와_미국발목록통관조건부200달러를구분한다(
        double 물품가격Usd,
        bool 미국발목록통관조건충족,
        string expected)
    {
        var result = 주문자해외구매통관정책.소액면세후보판정(
            공동구매거래유형코드.B2C,
            (decimal)물품가격Usd,
            미국발목록통관조건충족);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void 사업판매목적은_금액이작아도_소액면세후보가아니다()
    {
        var result = 주문자해외구매통관정책.소액면세후보판정(
            공동구매거래유형코드.B2B,
            10m,
            미국발목록통관조건충족: true);

        Assert.Equal(해외구매소액면세판정코드.사업수입경로, result);
    }

    [Fact]
    public void 개인자가사용식품150달러이하는_일반수입신고와면세후보를함께표시한다()
    {
        var result = 주문자해외구매통관정책.수입물류경로검토(
            new 주문자수입물류경로검토요청
            {
                거래유형 = 공동구매거래유형코드.B2C,
                물품가격Usd = 150m,
                식품류여부 = true,
                미국발목록통관조건충족 = true
            });

        Assert.False(result.목록통관검토가능);
        Assert.True(result.일반수입신고필요);
        Assert.False(result.과세검토필요);
        Assert.True(result.식품수입요건검토필요);
        Assert.Equal(해외구매소액면세판정코드.자가사용면세후보, result.판정코드);
        Assert.Equal(주문자수입3PL권유수준코드.직접수령가능, result.물류3PL권유수준코드);
    }

    [Fact]
    public void 개인식품은_미국발이어도150달러를넘으면_과세와3PL검토대상이다()
    {
        var result = 주문자해외구매통관정책.수입물류경로검토(
            new 주문자수입물류경로검토요청
            {
                거래유형 = 공동구매거래유형코드.B2C,
                물품가격Usd = 151m,
                식품류여부 = true,
                미국발목록통관조건충족 = true
            });

        Assert.Equal(해외구매소액면세판정코드.과세검토필요, result.판정코드);
        Assert.True(result.과세검토필요);
        Assert.Equal(주문자수입3PL권유수준코드.이용검토, result.물류3PL권유수준코드);
    }

    [Fact]
    public void 판매목적은_금액과무관하게_일반수입신고와3PL이용권유대상이다()
    {
        var result = 주문자해외구매통관정책.수입물류경로검토(
            new 주문자수입물류경로검토요청
            {
                거래유형 = 공동구매거래유형코드.B2B,
                물품가격Usd = 20m,
                식품류여부 = true,
                냉장냉동보관필요 = true
            });

        Assert.True(result.일반수입신고필요);
        Assert.True(result.과세검토필요);
        Assert.Equal(주문자수입3PL권유수준코드.이용권유, result.물류3PL권유수준코드);
        Assert.Contains(result.확인할3PL역량목록, item => item.Contains("냉장·냉동", StringComparison.Ordinal));
        Assert.Contains(result.주의사항목록, item => item.Contains("면제하거나 줄여", StringComparison.Ordinal));
    }
}
