using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class 입고예정정책Tests
{
    [Fact]
    public void 조회조건정책은_기존조건을보존하고상태만입고예정으로고정한다()
    {
        var request = new 목록조회요청
        {
            페이지 = -1,
            페이지크기 = 500,
            검색어 = "  SUP-01  ",
            필터조건 =
            [
                new 목록필터조건(nameof(입고요청항목응답.상태), "Equal", 입고상태코드.완료),
                new 목록필터조건(nameof(입고요청항목응답.공급처코드), "Contains", "SUP"),
                new 목록필터조건(string.Empty, "Equal", "제외")
            ]
        };

        var result = 입고예정조회조건정책.적용(request);

        Assert.Equal(0, result.페이지);
        Assert.Equal(200, result.페이지크기);
        Assert.Equal("SUP-01", result.검색어);
        Assert.Contains(result.필터조건, filter =>
            filter.필드 == nameof(입고요청항목응답.공급처코드)
            && filter.값 == "SUP");
        var status = Assert.Single(
            result.필터조건,
            filter => filter.필드 == nameof(입고요청항목응답.상태));
        Assert.Equal(입고상태코드.예정, status.값);
    }

    [Fact]
    public void 표시정책은_누락정보에안전한대체문구를제공한다()
    {
        var item = new 입고요청항목응답();

        Assert.Equal("업체명 미등록", 입고예정표시정책.공급처명(item));
        Assert.Equal("코드 미등록", 입고예정표시정책.공급처코드(item));
        Assert.Equal("품목 정보 없음", 입고예정표시정책.상품명(item));
        Assert.Equal(string.Empty, 입고예정표시정책.상품상세(item));
        Assert.Equal("참조번호 없음", 입고예정표시정책.참조번호(item));
    }

    [Fact]
    public void 표시정책은_SKU수량과우선참조번호를조합한다()
    {
        var item = new 입고요청항목응답
        {
            예정SKU = "SKU-100",
            예정수량 = 1234,
            주문참조번호 = "ORDER-1",
            원주문참조번호 = "ORIGINAL-1"
        };

        Assert.Equal("SKU-100 · 1,234개", 입고예정표시정책.상품상세(item));
        Assert.Equal("ORDER-1", 입고예정표시정책.참조번호(item));
    }
}
