using System.Globalization;
using System.Net.Http.Json;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Contracts.Common.Customs;
using Ssalddel.Contracts.Common.PublicData;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public sealed class 농수산공공데이터Client(HttpClient httpClient) : I농수산공공데이터Client
{
    private const string BasePath = "api/v1/agricultural-fisheries";

    public Task<RegionalAgriculturalMapMarkerListResponse> 지역MapMarker조회Async(
        RegionalAgriculturalMapMarkerQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentException.ThrowIfNullOrWhiteSpace(query.CountryCode);
        var parameters = new List<string>
        {
            $"countryCode={Uri.EscapeDataString(query.CountryCode.Trim())}",
            $"maxItems={Math.Clamp(query.MaxItems, 1, 500)}"
        };
        AddParameter(parameters, "relationTypeCode", query.RelationTypeCode);
        AddParameter(parameters, "productName", query.ProductName);
        AddDateParameter(parameters, "fromDate", query.FromDate);
        AddDateParameter(parameters, "toDate", query.ToDate);

        return GetAsync<RegionalAgriculturalMapMarkerListResponse>(
            $"{RegionalAgriculturalMapRoutes.MarkerApi}?{string.Join('&', parameters)}",
            cancellationToken);
    }

    public Task<MarineFishingAreaOceanTileResponse> 해양수산Map바다Tile조회Async(
        CancellationToken cancellationToken = default)
        => GetAsync<MarineFishingAreaOceanTileResponse>(
            RegionalAgriculturalMapRoutes.OceanTileApi,
            cancellationToken);

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

    public Task<Hs식품국가가격Card응답> Hs식품국가가격Card조회Async(
        string hsCode,
        string? month = null,
        int lookbackMonths = 3,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hsCode);
        var normalizedHsCode = new string(hsCode.Where(char.IsDigit).Take(6).ToArray());
        if (normalizedHsCode.Length != 6)
        {
            throw new ArgumentException("HS 코드는 숫자 6자리 이상이어야 합니다.", nameof(hsCode));
        }

        var parameters = new List<string>
        {
            $"lookbackMonths={Math.Clamp(lookbackMonths, 1, 12)}"
        };
        AddParameter(parameters, "month", month);
        return GetAsync<Hs식품국가가격Card응답>(
            $"{BasePath}/items/{Uri.EscapeDataString(normalizedHsCode)}/country-price-card"
            + $"?{string.Join('&', parameters)}",
            cancellationToken);
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

    public Task<호주농수산식품가격Catalog응답> 호주가격원천Catalog조회Async(
        CancellationToken cancellationToken = default)
        => GetAsync<호주농수산식품가격Catalog응답>(
            $"{BasePath}/au-food-price-indexes/catalog",
            cancellationToken);

    public Task<호주농수산식품가격조회응답> 호주식품가격지수조회Async(
        호주농수산식품가격조회요청 request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var parameters = new List<string>();
        AddParameter(parameters, "sourceKey", request.SourceKey);
        AddParameter(parameters, "indexCode", request.IndexCode);
        AddParameter(parameters, "measureCode", request.MeasureCode);
        AddParameter(parameters, "regionCode", request.RegionCode);
        AddParameter(parameters, "startPeriod", request.StartPeriod);
        AddParameter(parameters, "endPeriod", request.EndPeriod);
        parameters.Add($"maxItems={Math.Clamp(request.MaxItems, 1, 120)}");

        return GetAsync<호주농수산식품가격조회응답>(
            $"{BasePath}/au-food-price-indexes?{string.Join('&', parameters)}",
            cancellationToken);
    }

    public Task<FoodPriceComparisonResponse> 식품가격비교Async(
        FoodPriceComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.HsCode);

        var hsCode = new string(request.HsCode.Where(char.IsDigit).ToArray());
        var parameters = new List<string>();
        AddParameter(parameters, "countryCode", request.CountryCode);
        AddParameter(parameters, "referenceDate", request.ReferenceDate);
        parameters.Add($"domesticLookbackDays={Math.Clamp(request.DomesticLookbackDays, 1, 31)}");
        AddParameter(parameters, "referenceMonth", request.ReferenceMonth);
        parameters.Add($"importLookbackMonths={Math.Clamp(request.ImportLookbackMonths, 1, 12)}");
        AddDecimalParameter(parameters, "fxRateKrwPerUsd", request.FxRateKrwPerUsd);
        AddDecimalParameter(
            parameters,
            "estimatedImportAdditionalCostKrwPerKg",
            request.EstimatedImportAdditionalCostKrwPerKg);

        return GetAsync<FoodPriceComparisonResponse>(
            $"api/v1/customs/hs-codes/{Uri.EscapeDataString(hsCode)}/food-price-comparison"
            + $"?{string.Join('&', parameters)}",
            cancellationToken);
    }

    public async Task<HsCountryImportUnitPriceSimulationResult> 수입평균단가조회Async(
        HsCountryMonthlyTradeUnitPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        using var response = await httpClient.PostAsJsonAsync(
            "api/v1/orderer/public-data/customs/hs-country-import-unit-price-simulation",
            request,
            cancellationToken);
        var payload = await response.Content.ReadFromJsonAsync<HsCountryImportUnitPriceSimulationResult>(
            cancellationToken: cancellationToken);
        if (payload is null)
        {
            throw new HttpRequestException(
                "수입 평균단가 API 응답을 읽지 못했습니다.",
                inner: null,
                response.StatusCode);
        }

        return payload;
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

    private static void AddDecimalParameter(
        List<string> parameters,
        string name,
        decimal? value)
    {
        if (value is > 0)
        {
            parameters.Add($"{name}={value.Value.ToString(CultureInfo.InvariantCulture)}");
        }
    }

    private static void AddDateParameter(
        List<string> parameters,
        string name,
        DateOnly? value)
    {
        if (value is { } date)
        {
            parameters.Add($"{name}={date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
        }
    }
}
