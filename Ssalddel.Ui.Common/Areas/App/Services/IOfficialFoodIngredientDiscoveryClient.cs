using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface IOfficialFoodIngredientDiscoveryClient
{
    Task<IReadOnlyList<OfficialFoodIngredientDto>> SearchAsync(
        OfficialFoodIngredientQuery query,
        CancellationToken cancellationToken = default);
}
