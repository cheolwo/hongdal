using Ssalddel.Contracts.Common.Orderer;
using OrdererApp.Services;

namespace OrdererApp.ViewModels;

/// <summary>
/// 공동구매 상품 카탈로그와 선택한 수요 초안 값만 소유합니다.
/// route, 선적, 시세, 배송권과 저장 상태는 각 화면과 업무 컴포넌트가 따로 관리합니다.
/// </summary>
public sealed class GroupPurchaseCatalogViewModel(
    IGroupPurchaseProductCatalogService service)
{
    private IReadOnlyList<HS먹거리공동구매상품카드> _productCards = [];
    private HS먹거리공동구매상품카드? _selectedProduct;

    public IReadOnlyList<HS먹거리공동구매상품카드> ProductCards => _productCards;
    public HS먹거리공동구매상품카드 SelectedProduct
        => _selectedProduct ?? throw new InvalidOperationException("공동구매 상품 카탈로그가 아직 준비되지 않았습니다.");
    public string? SelectedProductId => _selectedProduct?.상품카드Id;
    public bool IsLoaded { get; private set; }
    public string? ErrorMessage { get; private set; }
    public decimal DesiredQuantityKg { get; private set; }
    public decimal DesiredUnitPrice { get; private set; }

    public async Task<bool> LoadAsync(
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        if (IsLoaded && !force)
        {
            return true;
        }

        try
        {
            var selectedId = _selectedProduct?.상품카드Id;
            _productCards = await service.GetProductsAsync(cancellationToken);
            _selectedProduct = _productCards.FirstOrDefault(item =>
                                   string.Equals(item.상품카드Id, selectedId, StringComparison.Ordinal))
                               ?? _productCards.FirstOrDefault();
            if (_selectedProduct is not null)
            {
                DesiredQuantityKg = DefaultQuantity(_selectedProduct);
                DesiredUnitPrice = _selectedProduct.ExpectedUnitPrice;
            }

            IsLoaded = true;
            ErrorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            IsLoaded = false;
            return false;
        }
    }

    public bool TrySelectProduct(string? productId)
    {
        var product = _productCards.FirstOrDefault(item =>
            string.Equals(item.상품카드Id, productId, StringComparison.Ordinal));
        if (product is null)
        {
            return false;
        }

        if (ReferenceEquals(product, _selectedProduct))
        {
            return true;
        }

        _selectedProduct = product;
        DesiredQuantityKg = DefaultQuantity(product);
        DesiredUnitPrice = product.ExpectedUnitPrice;
        return true;
    }

    public void SelectProduct(string productId) => _ = TrySelectProduct(productId);

    public void UpdateDesiredQuantity(decimal quantityKg)
        => DesiredQuantityKg = Math.Max(1m, quantityKg);

    public void UpdateDesiredUnitPrice(decimal unitPrice)
        => DesiredUnitPrice = Math.Max(1m, unitPrice);

    private static decimal DefaultQuantity(HS먹거리공동구매상품카드 product)
        => product.온도코드 == 공동구매온도코드.냉동 ? 20m : 5m;
}
