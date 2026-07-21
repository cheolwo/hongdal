using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public sealed class OfficialFoodIngredientDiscoveryClient(HttpClient httpClient)
    : IOfficialFoodIngredientDiscoveryClient
{
    private const string BasePath = "api/v1/agricultural-fisheries/food-ingredients";

    public async Task<IReadOnlyList<OfficialFoodIngredientDto>> SearchAsync(
        OfficialFoodIngredientQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var parameters = new List<string>
        {
            $"take={Math.Clamp(query.Take, 1, 50)}"
        };
        AddParameter(parameters, "categoryCode", query.CategoryCode);
        AddParameter(parameters, "languageCode", query.LanguageCode);
        AddParameter(parameters, "classificationState", query.ClassificationState);
        AddParameter(parameters, "searchText", query.SearchText);

        using var response = await httpClient.GetAsync(
            $"{BasePath}?{string.Join('&', parameters)}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OfficialFoodIngredientDto[]>(
                   cancellationToken: cancellationToken)
               ?? [];
    }

    private static void AddParameter(List<string> parameters, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
