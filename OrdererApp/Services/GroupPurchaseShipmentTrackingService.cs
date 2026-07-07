using System.Net;
using System.Net.Http.Json;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Contracts.Common.PublicData;

namespace OrdererApp.Services;

public interface IGroupPurchaseShipmentTrackingService
{
    Task<GroupPurchaseOverseasShipmentPublicDto?> LookupAsync(
        string documentManagementNumber,
        CancellationToken cancellationToken = default);

    Task<HsCountryImportUnitPriceSimulationResult?> SimulateImportUnitPriceAsync(
        HsCountryMonthlyTradeUnitPriceRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class HttpGroupPurchaseShipmentTrackingService : IGroupPurchaseShipmentTrackingService
{
    private readonly HttpClient _httpClient;

    public HttpGroupPurchaseShipmentTrackingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GroupPurchaseOverseasShipmentPublicDto?> LookupAsync(
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
            return await _httpClient.GetFromJsonAsync<GroupPurchaseOverseasShipmentPublicDto>(
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
}
