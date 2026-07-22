using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface IOfficialFoodIngredientDiscoveryClient
{
    Task<IReadOnlyList<OfficialFoodRecipeDishDto>> SearchDishesAsync(
        OfficialFoodDishDiscoveryQuery query,
        CancellationToken cancellationToken = default);

    Task<OfficialFoodDishDetailDto?> GetDishAsync(
        string dishKey,
        CancellationToken cancellationToken = default);

    Task<OfficialFoodIngredientCompanyResearchResponse> SearchCompaniesAsync(
        OfficialFoodIngredientCompanyQuery query,
        CancellationToken cancellationToken = default);

    Task<OfficialFoodIngredientHsMappingResponse> GetHsCodesAsync(
        OfficialFoodIngredientHsQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OfficialFoodIngredientDto>> SearchAsync(
        OfficialFoodIngredientQuery query,
        CancellationToken cancellationToken = default);
}
