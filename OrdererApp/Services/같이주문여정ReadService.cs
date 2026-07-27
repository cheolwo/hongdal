using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace OrdererApp.Services;

public interface I같이주문여정ReadService
{
    Task<HS먹거리공동구매상품카드?> 상품조회Async(
        string 상품키,
        CancellationToken cancellationToken = default);

    Task<같이주문레시피활용응답?> 레시피활용조회Async(
        HS먹거리공동구매상품카드 상품,
        CancellationToken cancellationToken = default);

    Task<주문방식비교응답?> 주문방식비교Async(
        HS먹거리공동구매상품카드 상품,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<공동구매자동집단요약응답>> 같이주문목록조회Async(
        string? 배송권키,
        CancellationToken cancellationToken = default);

    Task<같이주문공개상세응답?> 같이주문상세조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default);
}

public sealed class Http같이주문여정ReadService(
    IGroupPurchaseProductCatalogService 상품Catalog,
    ISsalddelJsonApiClient apiClient) : I같이주문여정ReadService
{
    private const decimal 개인수령검토수량Kg = 25m;

    public async Task<HS먹거리공동구매상품카드?> 상품조회Async(
        string 상품키,
        CancellationToken cancellationToken = default)
        => (await 상품Catalog.GetProductsAsync(cancellationToken))
            .FirstOrDefault(item =>
                string.Equals(item.상품카드Id, 상품키, StringComparison.Ordinal));

    public Task<같이주문레시피활용응답?> 레시피활용조회Async(
        HS먹거리공동구매상품카드 상품,
        CancellationToken cancellationToken = default)
    {
        var query = string.Join(
            "&",
            $"상품키={Uri.EscapeDataString(상품.상품카드Id)}",
            $"상품명={Uri.EscapeDataString(상품.상품명)}",
            $"개인수령검토수량={개인수령검토수량Kg}",
            "수량단위=kg",
            "최대레시피수=3");

        return apiClient.GetAsync<같이주문레시피활용응답>(
            $"api/v1/orderer/order-mode-comparisons/recipe-uses?{query}",
            "같이 주문 레시피 활용 조회",
            allowNotFound: false,
            cancellationToken);
    }

    public Task<주문방식비교응답?> 주문방식비교Async(
        HS먹거리공동구매상품카드 상품,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var individualUnitPrice = Math.Max(1m, 상품.ExpectedUnitPrice);
        var request = new 주문방식비교요청
        {
            상품키 = 상품.상품카드Id,
            상품명 = 상품.상품명,
            요청수량 = 개인수령검토수량Kg,
            수량단위 = "kg",
            통화코드 = "KRW",
            기준시각Utc = now,
            최대대기가능시각Utc = now.AddDays(12),
            개별주문 = new 개별주문비용입력
            {
                상품단가 = individualUnitPrice,
                배송비 = 6_000m,
                예상수령시각Utc = now.AddDays(3),
                가격근거 = "상품 후보 단가와 25kg 수령 기준 배송비"
            },
            같이주문 = new 같이주문비용입력
            {
                현재참여자수 = 7,
                목표참여자수 = 12,
                현재확정수량 = 0m,
                현재잠재수량 = 175m,
                최소성립수량 = 250m,
                최대안전수량 = 10_000m,
                계산증분 = 25m,
                목표절감률 = 5m,
                위험예비비율 = 2m,
                모집마감시각Utc = now.AddDays(7),
                예상수령시각Utc = now.AddDays(9),
                공급가격구간 =
                [
                    new 같이주문공급가격구간입력
                    {
                        이름 = "10상자 이상",
                        최소수량 = 250m,
                        상품단가 = decimal.Round(individualUnitPrice * .9m, 2),
                        근거 = "화면 검토용 25kg 상자 공급 구간"
                    },
                    new 같이주문공급가격구간입력
                    {
                        이름 = "40상자 이상",
                        최소수량 = 1_000m,
                        상품단가 = decimal.Round(individualUnitPrice * .82m, 2),
                        근거 = "화면 검토용 25kg 상자 공급 구간"
                    }
                ],
                비용항목 =
                [
                    new 같이주문비용항목입력
                    {
                        코드 = "shared-delivery",
                        이름 = "같이 배송 준비비",
                        비용분류코드 = "local-cold-chain-delivery",
                        계산방식코드 = "fixed",
                        금액 = 35_000m,
                        근거 = "화면 비교용 임시 입력"
                    }
                ]
            }
        };

        return apiClient.SendAsync<주문방식비교요청, 주문방식비교응답>(
            HttpMethod.Post,
            "api/v1/orderer/order-mode-comparisons/preview",
            request,
            "개별 주문과 같이 주문 비교",
            allowNotFound: false,
            cancellationToken);
    }

    public async Task<IReadOnlyList<공동구매자동집단요약응답>> 같이주문목록조회Async(
        string? 배송권키,
        CancellationToken cancellationToken = default)
    {
        var query = string.IsNullOrWhiteSpace(배송권키)
            ? string.Empty
            : $"?deliveryScopeKey={Uri.EscapeDataString(배송권키.Trim())}";
        return await apiClient.GetAsync<IReadOnlyList<공동구매자동집단요약응답>>(
                   $"api/v1/orderer/group-purchase-auto-groups{query}",
                   "배송권의 같이 주문 목록 조회",
                   allowNotFound: false,
                   cancellationToken)
               ?? [];
    }

    public Task<같이주문공개상세응답?> 같이주문상세조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default)
        => apiClient.GetAsync<같이주문공개상세응답>(
            $"api/v1/orderer/group-purchase-auto-groups/{Uri.EscapeDataString(자동집단Id.Trim())}",
            "같이 주문 공개 상세 조회",
            allowNotFound: true,
            cancellationToken);
}
