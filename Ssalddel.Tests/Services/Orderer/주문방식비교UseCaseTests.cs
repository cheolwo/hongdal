using Ssalddel.Contracts.Common.CollectiveProcurement;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.CollectiveProcurement;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 주문방식비교UseCaseTests
{
    [Fact]
    public void 같이주문이저렴하고대기범위안이면_비용과시간을함께비교한다()
    {
        var sut = Create();
        var request = CreateRequest();

        var result = sut.비교(request);

        Assert.Equal(27_000m, result.개별주문.총예상비용);
        Assert.Equal(24_750m, result.같이주문.총예상비용);
        Assert.Equal(2_250m, result.판단.예상절감액);
        Assert.Equal(8.33m, result.판단.예상절감률);
        Assert.Equal(48m, result.판단.추가대기시간Hours);
        Assert.True(result.판단.같이비용절감가능);
        Assert.True(result.판단.최대대기허용범위안);
        Assert.Equal(
            주문방식비교신호코드.같이비용절감성립대기,
            result.판단.신호코드);
        Assert.Equal(70m, result.같이주문모집.모집진척률);
        Assert.Equal(3m, result.같이주문모집.추가필요수량);
    }

    [Fact]
    public void 같이주문이저렴해도_사용자대기한도를넘으면그사실을우선표시한다()
    {
        var sut = Create();
        var request = CreateRequest();
        request.최대대기가능시각Utc = Utc(2026, 7, 28, 9);

        var result = sut.비교(request);

        Assert.True(result.판단.같이비용절감가능);
        Assert.False(result.판단.최대대기허용범위안);
        Assert.Equal(
            주문방식비교신호코드.같이비용절감대기초과,
            result.판단.신호코드);
    }

    [Fact]
    public void 같이주문이더비싸면_절감으로표현하지않고기본선택을두지않는다()
    {
        var sut = Create();
        var request = CreateRequest();
        request.같이주문.공급가격구간[0].상품단가 = 10_000m;

        var result = sut.비교(request);

        Assert.False(result.판단.같이비용절감가능);
        Assert.Equal(
            주문방식비교신호코드.개별비용우위,
            result.판단.신호코드);
        Assert.True(result.기본선택없음);
        Assert.True(result.자동같이주문금지);
        Assert.True(result.같이주문별도동의필수);
    }

    [Fact]
    public void 모집이마감되면_가격과무관하게같이주문검토를막는다()
    {
        var sut = Create();
        var request = CreateRequest();
        request.같이주문.모집마감시각Utc = Utc(2026, 7, 25, 9);

        var result = sut.비교(request);

        Assert.True(result.같이주문모집.모집마감);
        Assert.False(result.판단.같이주문검토가능);
        Assert.Equal(
            주문방식비교신호코드.같이모집마감,
            result.판단.신호코드);
    }

    private static 주문방식비교UseCase Create()
        => new(new CollectiveProcurementEconomicsEngine(), TimeProvider.System);

    internal static 주문방식비교요청 CreateRequest()
        => new()
        {
            상품키 = "apple-5kg",
            상품명 = "사과 5kg",
            요청수량 = 3m,
            수량단위 = "box",
            통화코드 = "KRW",
            기준시각Utc = Utc(2026, 7, 26, 9),
            최대대기가능시각Utc = Utc(2026, 7, 30, 9),
            개별주문 = new 개별주문비용입력
            {
                상품단가 = 8_000m,
                배송비 = 3_000m,
                예상수령시각Utc = Utc(2026, 7, 27, 9),
                가격근거 = "인근 매장 공개 판매가"
            },
            같이주문 = new 같이주문비용입력
            {
                현재참여자수 = 5,
                목표참여자수 = 8,
                현재확정수량 = 5m,
                현재잠재수량 = 7m,
                최소성립수량 = 10m,
                최대안전수량 = 20m,
                계산증분 = 1m,
                목표절감률 = 5m,
                모집마감시각Utc = Utc(2026, 7, 27, 18),
                예상수령시각Utc = Utc(2026, 7, 29, 9),
                공급가격구간 =
                [
                    new 같이주문공급가격구간입력
                    {
                        이름 = "10상자 같이 주문 가격",
                        최소수량 = 10m,
                        상품단가 = 7_500m,
                        근거 = "판매자 공개 공동가격"
                    }
                ],
                비용항목 =
                [
                    new 같이주문비용항목입력
                    {
                        코드 = "shared-delivery",
                        이름 = "같이 배송비",
                        비용분류코드 = CollectiveProcurementCostCategoryCodes.LocalColdChainDelivery,
                        계산방식코드 = CollectiveProcurementCostModelCodes.Fixed,
                        금액 = 7_500m,
                        근거 = "배송권 단위 견적"
                    }
                ]
            }
        };

    private static DateTimeOffset Utc(int year, int month, int day, int hour)
        => new(year, month, day, hour, 0, 0, TimeSpan.Zero);
}
