using Ssalddel.Contracts.Common.Orderer;

namespace OrdererApp.ViewModels;

/// <summary>
/// 공동구매 페이지가 공유해야 하는 상품 선택과 수량·단가만 소유합니다.
/// 선적, 시세, 배송권과 수요 등록 상태는 각 하위 컴포넌트가 따로 관리합니다.
/// </summary>
public sealed class GroupPurchaseIntentPageViewModel
{
    private readonly IReadOnlyList<HS먹거리공동구매상품카드> _productCards =
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

    public GroupPurchaseIntentPageViewModel()
    {
        SelectedProduct = _productCards[0];
        DesiredQuantityKg = DefaultQuantity(SelectedProduct);
        DesiredUnitPrice = SelectedProduct.ExpectedUnitPrice;
    }

    public IReadOnlyList<HS먹거리공동구매상품카드> ProductCards => _productCards;
    public HS먹거리공동구매상품카드 SelectedProduct { get; private set; }
    public decimal DesiredQuantityKg { get; private set; }
    public decimal DesiredUnitPrice { get; private set; }

    public void SelectProduct(string productId)
    {
        var product = _productCards.FirstOrDefault(item =>
            string.Equals(item.상품카드Id, productId, StringComparison.Ordinal));
        if (product is null || ReferenceEquals(product, SelectedProduct))
        {
            return;
        }

        SelectedProduct = product;
        DesiredQuantityKg = DefaultQuantity(product);
        DesiredUnitPrice = product.ExpectedUnitPrice;
    }

    public void UpdateDesiredQuantity(decimal quantityKg)
        => DesiredQuantityKg = Math.Max(1m, quantityKg);

    public void UpdateDesiredUnitPrice(decimal unitPrice)
        => DesiredUnitPrice = Math.Max(1m, unitPrice);

    private static decimal DefaultQuantity(HS먹거리공동구매상품카드 product)
        => product.온도코드 == 공동구매온도코드.냉동 ? 20m : 5m;
}
