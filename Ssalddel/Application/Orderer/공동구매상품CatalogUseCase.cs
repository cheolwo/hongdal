using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Application.Orderer;

public interface I공동구매상품CatalogUseCase
{
    Task<IReadOnlyList<HS먹거리공동구매상품카드>> 목록조회Async(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 주문자 앱에 노출할 공동구매 후보를 서버 경계에서 관리합니다.
/// 후보 조회는 읽기 전용이며 수요 등록, 결제 또는 주문 확정을 수행하지 않습니다.
/// </summary>
public sealed class 공동구매상품CatalogUseCase : I공동구매상품CatalogUseCase
{
    private static readonly IReadOnlyList<HS먹거리공동구매상품카드> Items =
    [
        new(
            상품카드Id: "hs-food-0203-pork-frozen",
            상품명: "냉동 삼겹살",
            HS코드: "0203.29",
            HS표시명: "돼지고기 냉동 기타",
            온도코드: 공동구매온도코드.냉동,
            예상물류방식: 공동구매물류방식코드.FCL,
            SuggestedTargetQuantityKg: 12000m,
            ExpectedUnitPrice: 8500m),
        new(
            상품카드Id: "hs-food-1602-prepared-meat",
            상품명: "가공육 세트",
            HS코드: "1602.49",
            HS표시명: "조제 또는 저장 처리한 육류",
            온도코드: 공동구매온도코드.냉장,
            예상물류방식: 공동구매물류방식코드.LCL,
            SuggestedTargetQuantityKg: 3000m,
            ExpectedUnitPrice: 7200m),
        new(
            상품카드Id: "hs-food-2106-prepared-food",
            상품명: "간편식 소스",
            HS코드: "2106.90",
            HS표시명: "기타 조제 식료품",
            온도코드: 공동구매온도코드.상온,
            예상물류방식: 공동구매물류방식코드.국내벌크,
            SuggestedTargetQuantityKg: 1500m,
            ExpectedUnitPrice: 3900m,
            RequiresImportFoodReview: true,
            RequiresMfdsManufacturerReview: true)
    ];

    public Task<IReadOnlyList<HS먹거리공동구매상품카드>> 목록조회Async(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Items);
    }
}
