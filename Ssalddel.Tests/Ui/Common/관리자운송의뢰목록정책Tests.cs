using Ssalddel.Contracts.Shipper.Request;
using Ssalddel.Ui.Common.Areas.BackOffice.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 관리자운송의뢰목록정책Tests
{
    [Fact]
    public void 의뢰Id검색은_대소문자와_앞뒤_공백을_무시한다()
    {
        var items = new[]
        {
            new 화주운송의뢰응답 { 의뢰Id = "HD-WEB-001" },
            new 화주운송의뢰응답 { 의뢰Id = "HD-WEB-002" }
        };

        var result = 관리자운송의뢰목록정책.의뢰Id검색(items, item => item.의뢰Id, " web-002 ");

        Assert.Single(result);
        Assert.Equal("HD-WEB-002", result[0].의뢰Id);
    }

    [Fact]
    public void 권역표시는_상세도로명과_번지를_목록에_노출하지_않는다()
    {
        var result = 관리자운송의뢰목록정책.권역표시("06236 서울특별시 강남구 테헤란로 123 10층");

        Assert.Equal("서울특별시 강남구", result);
        Assert.DoesNotContain("테헤란로", result);
        Assert.DoesNotContain("123", result);
    }

    [Fact]
    public void 공백없는_주소는_전체를_표시하지_않는다()
    {
        var result = 관리자운송의뢰목록정책.권역표시("경기도성남시분당구판교역로235");

        Assert.Equal("경기도성남시…", result);
        Assert.DoesNotContain("판교역로", result);
    }

    [Theory]
    [InlineData("REQ/2026 01", "/requests/REQ%2F2026%2001")]
    [InlineData("HD-WEB-001", "/requests/HD-WEB-001")]
    public void 상세경로는_의뢰Id를_안전한_단일_경로구간으로_만든다(string requestId, string expected)
    {
        Assert.Equal(expected, 관리자운송의뢰목록정책.상세경로(requestId));
    }
}
