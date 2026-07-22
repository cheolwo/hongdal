using System.Net.Http.Json;
using System.Net;
using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public sealed class OfficialFoodIngredientDiscoveryClient(HttpClient httpClient)
    : IOfficialFoodIngredientDiscoveryClient
{
    private const string RootPath = "api/v1/agricultural-fisheries";
    private const string IngredientPath = $"{RootPath}/food-ingredients";
    private const string CompanyPath = $"{IngredientPath}/companies";
    private const string HsCodePath = $"{IngredientPath}/hs-codes";
    private const string DishPath = $"{RootPath}/food-dishes";

    public async Task<IReadOnlyList<OfficialFoodRecipeDishDto>> SearchDishesAsync(
        OfficialFoodDishDiscoveryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var parameters = new List<string>
        {
            $"take={Math.Clamp(query.Take, 1, 50)}"
        };
        AddParameter(parameters, "countryCode", query.CountryCode);
        AddParameter(parameters, "searchText", query.SearchText);

        using var response = await httpClient.GetAsync(
            $"{DishPath}?{string.Join('&', parameters)}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OfficialFoodRecipeDishDto[]>(
                   cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<OfficialFoodDishDetailDto?> GetDishAsync(
        string dishKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dishKey);
        using var response = await httpClient.GetAsync(
            $"{DishPath}/{Uri.EscapeDataString(dishKey.Trim())}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OfficialFoodDishDetailDto>(
            cancellationToken: cancellationToken);
    }

    public async Task<OfficialFoodIngredientCompanyResearchResponse> SearchCompaniesAsync(
        OfficialFoodIngredientCompanyQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrWhiteSpace(query.IngredientName);
        var parameters = new List<string>
        {
            $"take={Math.Clamp(query.Take, 1, 20)}"
        };
        AddParameter(parameters, "ingredientKey", query.IngredientKey);
        AddParameter(parameters, "ingredientName", query.IngredientName);

        using var response = await httpClient.GetAsync(
            $"{CompanyPath}?{string.Join('&', parameters)}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OfficialFoodIngredientCompanyResearchResponse>(
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("재료 관련 기업 조사 응답이 비어 있습니다.");
    }

    public async Task<OfficialFoodIngredientHsMappingResponse> GetHsCodesAsync(
        OfficialFoodIngredientHsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.IngredientKey)
            && string.IsNullOrWhiteSpace(query.IngredientName))
        {
            throw new ArgumentException("재료 키 또는 재료명이 필요합니다.", nameof(query));
        }

        var parameters = new List<string>();
        AddParameter(parameters, "ingredientKey", query.IngredientKey);
        AddParameter(parameters, "ingredientName", query.IngredientName);
        AddParameter(parameters, "countryCode", query.CountryCode);
        if (query.Refresh)
        {
            parameters.Add("refresh=true");
        }

        using var response = await httpClient.GetAsync(
            $"{HsCodePath}?{string.Join('&', parameters)}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OfficialFoodIngredientHsMappingResponse>(
                   cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("재료 HS 코드 후보 응답이 비어 있습니다.");
    }

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
            $"{IngredientPath}?{string.Join('&', parameters)}",
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
