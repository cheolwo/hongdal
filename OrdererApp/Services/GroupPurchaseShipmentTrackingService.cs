using System.Net;
using System.Net.Http.Json;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.Operations;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace OrdererApp.Services;

public interface IGroupPurchaseShipmentTrackingService
{
    Task<공동구매해외선적공개Dto?> LookupAsync(
        string documentManagementNumber,
        CancellationToken cancellationToken = default);

    Task<HsCountryImportUnitPriceSimulationResult?> SimulateImportUnitPriceAsync(
        HsCountryMonthlyTradeUnitPriceRequest request,
        CancellationToken cancellationToken = default);

    Task<OperatingMarketRuntimeProfileResponse?> GetOperatingMarketAsync(
        CancellationToken cancellationToken = default);

    Task<OperatingMarketDeliveryScopePlan?> ResolveDeliveryScopesAsync(
        OperatingMarketDeliveryScopeResolveRequest request,
        CancellationToken cancellationToken = default);

    Task<공동구매자동집단사용자응답?> RegisterDemandAsync(
        공동구매자동수요등록Command request,
        CancellationToken cancellationToken = default);

    Task<공동구매자동수요철회응답?> WithdrawDemandAsync(
        string demandSourceKey,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<공동구매자동집단요약응답>?> ListGroupsAsync(
        string productKey,
        string deliveryScopeKey,
        CancellationToken cancellationToken = default);

    Task<OfficialFoodIngredientCompanyResearchResponse?> ResearchCompaniesAsync(
        string ingredientName,
        CancellationToken cancellationToken = default)
        => Task.FromResult<OfficialFoodIngredientCompanyResearchResponse?>(null);

    Task<OfficialFoodIngredientHsMappingResponse?> GetHsCandidatesAsync(
        string ingredientName,
        string countryCode,
        CancellationToken cancellationToken = default)
        => Task.FromResult<OfficialFoodIngredientHsMappingResponse?>(null);
}

public sealed class HttpGroupPurchaseShipmentTrackingService : IGroupPurchaseShipmentTrackingService
{
    private readonly HttpClient _httpClient;
    private readonly ISsalddelJsonApiClient _authenticatedApiClient;

    public HttpGroupPurchaseShipmentTrackingService(
        HttpClient httpClient,
        ISsalddelJsonApiClient authenticatedApiClient)
    {
        _httpClient = httpClient;
        _authenticatedApiClient = authenticatedApiClient;
    }

    public async Task<공동구매해외선적공개Dto?> LookupAsync(
        string documentManagementNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentManagementNumber))
        {
            return null;
        }

        try
        {
            var encodedNumber = Uri.EscapeDataString(documentManagementNumber.Trim());
            return await _httpClient.GetFromJsonAsync<공동구매해외선적공개Dto>(
                $"api/v1/orderer/group-purchase-overseas-shipments/lookup?documentManagementNumber={encodedNumber}",
                cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound || ex.StatusCode == HttpStatusCode.BadRequest)
        {
            return null;
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    public async Task<HsCountryImportUnitPriceSimulationResult?> SimulateImportUnitPriceAsync(
        HsCountryMonthlyTradeUnitPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/v1/orderer/public-data/customs/hs-country-import-unit-price-simulation",
                request,
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<HsCountryImportUnitPriceSimulationResult>(
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    public async Task<OperatingMarketRuntimeProfileResponse?> GetOperatingMarketAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<OperatingMarketRuntimeProfileResponse>(
                "api/v1/operations/market-profile",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    public async Task<OperatingMarketDeliveryScopePlan?> ResolveDeliveryScopesAsync(
        OperatingMarketDeliveryScopeResolveRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                "api/v1/orderer/public-data/group-purchase/delivery-scopes/resolve",
                request,
                cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<OperatingMarketDeliveryScopePlan>(
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    public Task<공동구매자동집단사용자응답?> RegisterDemandAsync(
        공동구매자동수요등록Command request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.수요출처키);
        request.요청멱등키 = string.IsNullOrWhiteSpace(request.요청멱등키)
            ? $"demand-save:{Guid.NewGuid():N}"
            : request.요청멱등키.Trim();

        return _authenticatedApiClient.SendWithHeadersAsync<공동구매자동수요등록Command, 공동구매자동집단사용자응답>(
            HttpMethod.Put,
            $"api/v1/orderer/group-purchase-auto-groups/demands/{Uri.EscapeDataString(request.수요출처키.Trim())}",
            request,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Idempotency-Key"] = request.요청멱등키
            },
            "공동주문 비구속 수요 저장",
            cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<공동구매자동집단요약응답>?> ListGroupsAsync(
        string productKey,
        string deliveryScopeKey,
        CancellationToken cancellationToken = default)
    {
        var path = "api/v1/orderer/group-purchase-auto-groups" +
                   $"?productKey={Uri.EscapeDataString(productKey.Trim())}" +
                   $"&deliveryScopeKey={Uri.EscapeDataString(deliveryScopeKey.Trim())}";
        return _authenticatedApiClient.GetAsync<IReadOnlyList<공동구매자동집단요약응답>>(
            path,
            "공동주문 자동집단 재조회",
            allowNotFound: false,
            cancellationToken);
    }

    public Task<공동구매자동수요철회응답?> WithdrawDemandAsync(
        string demandSourceKey,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(demandSourceKey);
        var path = $"api/v1/orderer/group-purchase-auto-groups/demands/{Uri.EscapeDataString(demandSourceKey.Trim())}";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            path += $"?reason={Uri.EscapeDataString(reason.Trim())}";
        }

        return _authenticatedApiClient.SendWithHeadersAsync<공동구매자동수요철회응답>(
            HttpMethod.Delete,
            path,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Idempotency-Key"] = $"demand-withdraw:{Guid.NewGuid():N}"
            },
            "공동주문 비구속 수요 철회",
            cancellationToken: cancellationToken);
    }

    public async Task<OfficialFoodIngredientCompanyResearchResponse?> ResearchCompaniesAsync(
        string ingredientName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ingredientName))
        {
            return null;
        }

        try
        {
            return await _httpClient.GetFromJsonAsync<OfficialFoodIngredientCompanyResearchResponse>(
                "api/v1/agricultural-fisheries/food-ingredients/companies" +
                $"?ingredientName={Uri.EscapeDataString(ingredientName.Trim())}&take=6",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    public async Task<OfficialFoodIngredientHsMappingResponse?> GetHsCandidatesAsync(
        string ingredientName,
        string countryCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ingredientName) || string.IsNullOrWhiteSpace(countryCode))
        {
            return null;
        }

        try
        {
            return await _httpClient.GetFromJsonAsync<OfficialFoodIngredientHsMappingResponse>(
                "api/v1/agricultural-fisheries/food-ingredients/hs-codes" +
                $"?ingredientName={Uri.EscapeDataString(ingredientName.Trim())}" +
                $"&countryCode={Uri.EscapeDataString(countryCode.Trim().ToUpperInvariant())}",
                cancellationToken);
        }
        catch (HttpRequestException)
        {
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }
}
