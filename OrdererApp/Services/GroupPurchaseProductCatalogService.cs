using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace OrdererApp.Services;

public interface IGroupPurchaseProductCatalogService
{
    Task<IReadOnlyList<HS먹거리공동구매상품카드>> GetProductsAsync(
        CancellationToken cancellationToken = default);
}

public sealed class HttpGroupPurchaseProductCatalogService(
    ISsalddelJsonApiClient client) : IGroupPurchaseProductCatalogService
{
    public async Task<IReadOnlyList<HS먹거리공동구매상품카드>> GetProductsAsync(
        CancellationToken cancellationToken = default)
        => await client.GetAsync<IReadOnlyList<HS먹거리공동구매상품카드>>(
               "api/v1/orderer/group-purchase-products",
               "공동구매 상품 후보 목록 조회",
               allowNotFound: false,
               cancellationToken)
           ?? [];
}
