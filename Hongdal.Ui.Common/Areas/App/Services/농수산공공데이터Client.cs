using System.Net.Http.Json;
using Hongdal.Contracts.Common.AgriculturalFisheries;

namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed class 농수산공공데이터Client(HttpClient httpClient) : I농수산공공데이터Client
{
    private const string BasePath = "api/v1/agricultural-fisheries";

    public Task<AgriculturalFisheriesInformationOverviewResponse> 개요조회Async(
        CancellationToken cancellationToken = default)
        => GetAsync<AgriculturalFisheriesInformationOverviewResponse>(BasePath, cancellationToken);

    public Task<AgriculturalFisheriesItemSearchResponse> 국내품목조회Async(
        string? query = null,
        string? categoryCode = null,
        int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<string>
        {
            $"page=1",
            $"pageSize={Math.Clamp(pageSize, 1, 100)}"
        };
        AddParameter(parameters, "query", query);
        AddParameter(parameters, "categoryCode", categoryCode);

        return GetAsync<AgriculturalFisheriesItemSearchResponse>(
            $"{BasePath}/items?{string.Join('&', parameters)}",
            cancellationToken);
    }

    public Task<AgriculturalFisheriesDomesticPriceResponse> 국내가격조회Async(
        string hsCode,
        int lookbackDays = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hsCode);
        var path = $"{BasePath}/items/{Uri.EscapeDataString(hsCode.Trim())}/domestic-price"
            + $"?lookbackDays={Math.Clamp(lookbackDays, 1, 31)}";
        return GetAsync<AgriculturalFisheriesDomesticPriceResponse>(path, cancellationToken);
    }

    public Task<미국농수산가격조회응답> 미국가격조회Async(
        string commodity,
        string program,
        int yearFrom,
        int yearTo,
        int maxItems = 100,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commodity);
        var parameters = new List<string>();
        AddParameter(parameters, "commodity", commodity);
        AddParameter(parameters, "program", program);
        AddParameter(parameters, "statisticCategory", "PRICE RECEIVED");
        AddParameter(parameters, "aggregationLevel", "NATIONAL");
        AddParameter(parameters, "domain", "TOTAL");
        parameters.Add($"yearFrom={yearFrom}");
        parameters.Add($"yearTo={yearTo}");
        parameters.Add($"maxItems={Math.Clamp(maxItems, 1, 500)}");

        return GetAsync<미국농수산가격조회응답>(
            $"{BasePath}/us-prices?{string.Join('&', parameters)}",
            cancellationToken);
    }

    private async Task<TResponse> GetAsync<TResponse>(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<TResponse>(
            cancellationToken: cancellationToken);
        if (payload is null)
        {
            throw new HttpRequestException(
                "공공데이터 API 응답을 읽지 못했습니다.",
                inner: null,
                response.StatusCode);
        }

        return payload;
    }

    private static void AddParameter(List<string> parameters, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
