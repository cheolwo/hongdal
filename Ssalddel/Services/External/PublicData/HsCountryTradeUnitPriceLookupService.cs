using System.Globalization;
using System.Text.RegularExpressions;
using Ssalddel.Contracts.Common.PublicData;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace 살뜰.Services.External.PublicData;

public sealed class HsCountryTradeUnitPriceLookupService : IHsCountryTradeUnitPriceLookupService
{
    private readonly HttpClient _httpClient;
    private readonly PublicDataOptions _options;

    public HsCountryTradeUnitPriceLookupService(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<HsCountryImportUnitPriceSimulationResult> SimulateImportUnitPriceAsync(
        HsCountryMonthlyTradeUnitPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        var hsCode = NormalizeHsCode(request.HsCode);
        var countryCode = NormalizeCountryCode(request.CountryCode);
        var endMonth = NormalizeMonth(request.Month);
        if (string.IsNullOrWhiteSpace(hsCode))
        {
            return Fail(request, "HS code is required.");
        }

        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return Fail(request, "Country code is required.");
        }

        if (endMonth is null)
        {
            return Fail(request, "Month must be yyyyMM.");
        }

        var serviceKey = ResolveServiceKey();
        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            return Fail(request, "PublicData:CustomsTradeStatistics:ServiceKey or PublicData:DataGoKrServiceKey is required.");
        }

        var lookbackMonths = Math.Clamp(request.LookbackMonths <= 0 ? 1 : request.LookbackMonths, 1, 12);
        var startMonth = AddMonths(endMonth, -(lookbackMonths - 1));
        var body = await ReadStatisticsBodyAsync(hsCode, countryCode, startMonth, endMonth, serviceKey, cancellationToken);
        var monthlyItems = PublicDataParsing.ReadItems(body)
            .Select(item => ToMonthlyItem(
                item,
                request,
                hsCode,
                countryCode,
                request.ExpectedFxRateKrwPerUsd))
            .Where(x =>
                x.ImportWeightKg > 0
                || x.ImportValueUsd > 0
                || x.ExportWeightKg > 0
                || x.ExportValueUsd > 0)
            .OrderBy(x => x.Month)
            .ToArray();

        if (monthlyItems.Length == 0)
        {
            return Fail(request, "No import statistics were returned for the HS code, country, and month range.", startMonth, endMonth);
        }

        var totalWeight = monthlyItems.Sum(x => x.ImportWeightKg);
        var totalValue = monthlyItems.Sum(x => x.ImportValueUsd);
        var totalExportWeight = monthlyItems.Sum(x => x.ExportWeightKg);
        var totalExportValue = monthlyItems.Sum(x => x.ExportValueUsd);
        var averageUsd = totalWeight > 0
            ? decimal.Round(totalValue / totalWeight, 4, MidpointRounding.AwayFromZero)
            : (decimal?)null;
        var averageKrw = averageUsd.HasValue && request.ExpectedFxRateKrwPerUsd is > 0
            ? decimal.Round(averageUsd.Value * request.ExpectedFxRateKrwPerUsd.Value, 0, MidpointRounding.AwayFromZero)
            : (decimal?)null;
        var averageExportUsd = totalExportWeight > 0
            ? decimal.Round(totalExportValue / totalExportWeight, 4, MidpointRounding.AwayFromZero)
            : (decimal?)null;
        var averageExportKrw = averageExportUsd.HasValue && request.ExpectedFxRateKrwPerUsd is > 0
            ? decimal.Round(
                averageExportUsd.Value * request.ExpectedFxRateKrwPerUsd.Value,
                0,
                MidpointRounding.AwayFromZero)
            : (decimal?)null;
        var landedCost = ResolveLandedCost(request, averageKrw);
        var grossMargin = landedCost.HasValue && request.ExpectedSellingUnitPriceKrwPerKg.HasValue
            ? request.ExpectedSellingUnitPriceKrwPerKg.Value - landedCost.Value
            : (decimal?)null;
        var marginRate = grossMargin.HasValue && request.ExpectedSellingUnitPriceKrwPerKg is > 0
            ? decimal.Round(grossMargin.Value / request.ExpectedSellingUnitPriceKrwPerKg.Value, 4, MidpointRounding.AwayFromZero)
            : (decimal?)null;
        var participantMargin = grossMargin.HasValue && request.ParticipantQuantityKg is > 0
            ? decimal.Round(grossMargin.Value * request.ParticipantQuantityKg.Value, 0, MidpointRounding.AwayFromZero)
            : (decimal?)null;

        return new HsCountryImportUnitPriceSimulationResult
        {
            Success = true,
            InternalProductCode = request.InternalProductCode.Trim(),
            ProductName = request.ProductName.Trim(),
            HsCode = hsCode,
            HsCodeScheme = ResolveHsCodeScheme(request.HsCodeScheme, hsCode),
            NationalTariffCodeScheme = request.NationalTariffCodeScheme.Trim().ToUpperInvariant(),
            NationalTariffCode = NormalizeTariffCode(request.NationalTariffCode),
            CountryCode = countryCode,
            StartMonth = startMonth,
            EndMonth = endMonth,
            MonthlyItems = monthlyItems,
            TotalImportWeightKg = totalWeight,
            TotalImportValueUsd = totalValue,
            AverageImportUnitValueUsdPerKg = averageUsd,
            AverageImportUnitValueKrwPerKg = averageKrw,
            TotalExportWeightKg = totalExportWeight,
            TotalExportValueUsd = totalExportValue,
            AverageExportUnitValueUsdPerKg = averageExportUsd,
            AverageExportUnitValueKrwPerKg = averageExportKrw,
            ExpectedLandedCostKrwPerKg = landedCost,
            ExpectedGrossMarginKrwPerKg = grossMargin,
            ExpectedGrossMarginRate = marginRate,
            ExpectedParticipantGrossMarginKrw = participantMargin,
            PriceSignalCode = ResolvePriceSignal(request, averageKrw, landedCost, marginRate),
            Summary = BuildSummary(
                averageUsd,
                averageKrw,
                averageExportUsd,
                averageExportKrw,
                landedCost,
                marginRate,
                participantMargin)
        };
    }

    private async Task<string> ReadStatisticsBodyAsync(
        string hsCode,
        string countryCode,
        string startMonth,
        string endMonth,
        string serviceKey,
        CancellationToken cancellationToken)
    {
        var query = new Dictionary<string, string?>
        {
            ["serviceKey"] = serviceKey,
            ["strtYymm"] = startMonth,
            ["endYymm"] = endMonth,
            ["hsSgn"] = hsCode,
            ["cntyCd"] = countryCode,
            ["pageNo"] = "1",
            ["numOfRows"] = "100",
            ["_type"] = "json",
            ["type"] = "json"
        };
        var relative = QueryHelpers.AddQueryString(
            _options.CustomsTradeStatistics.HsCountryMonthlyPath.TrimStart('/'),
            query);

        using var response = await _httpClient.GetAsync(relative, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"KCS trade statistics request failed. HTTP {(int)response.StatusCode}");
        }

        return body;
    }

    private static HsCountryMonthlyTradeUnitPriceItem ToMonthlyItem(
        Dictionary<string, string?> source,
        HsCountryMonthlyTradeUnitPriceRequest request,
        string fallbackHsCode,
        string fallbackCountryCode,
        decimal? fxRate)
    {
        var month = PublicDataParsing.FirstValue(
            source,
            "year",
            "balPaymentsYymm",
            "yyyymm",
            "yymm",
            "trdYymm",
            "statisYymm") ?? string.Empty;
        month = NormalizeMonth(month) ?? month;
        var weightKg = PublicDataParsing.FirstDecimal(source, "impWgt", "importWeight", "importWgt", "wgt", "netWgt") ?? 0m;
        var importValueUsd = PublicDataParsing.FirstDecimal(source, "impDlr", "importDlr", "importValue", "impAmt", "impUsd") ?? 0m;
        var exportWeightKg = PublicDataParsing.FirstDecimal(
            source,
            "expWgt",
            "exportWeight",
            "exportWgt") ?? 0m;
        var exportValueUsd = PublicDataParsing.FirstDecimal(
            source,
            "expDlr",
            "exportDlr",
            "exportValue",
            "expAmt",
            "expUsd") ?? 0m;
        var unitUsd = weightKg > 0
            ? decimal.Round(importValueUsd / weightKg, 4, MidpointRounding.AwayFromZero)
            : (decimal?)null;
        var exportUnitUsd = exportWeightKg > 0
            ? decimal.Round(exportValueUsd / exportWeightKg, 4, MidpointRounding.AwayFromZero)
            : (decimal?)null;
        var itemHsCode = NormalizeHsCode(PublicDataParsing.FirstValue(source, "hsSgn", "hsCd", "hsCode"));

        return new HsCountryMonthlyTradeUnitPriceItem
        {
            InternalProductCode = request.InternalProductCode.Trim(),
            ProductName = PublicDataParsing.FirstValue(source, "statKor", "itemName", "productName")
                ?? request.ProductName.Trim(),
            HsCode = itemHsCode.Length > 0
                ? itemHsCode
                : fallbackHsCode,
            HsCodeScheme = ResolveHsCodeScheme(
                request.HsCodeScheme,
                itemHsCode.Length > 0 ? itemHsCode : fallbackHsCode),
            CountryCode = NormalizeCountryCode(
                PublicDataParsing.FirstValue(source, "cntyCd", "countryCd", "natCd", "statCd")) is { Length: > 0 } country
                ? country
                : fallbackCountryCode,
            Month = month,
            ImportWeightKg = weightKg,
            ImportValueUsd = importValueUsd,
            AverageImportUnitValueUsdPerKg = unitUsd,
            AverageImportUnitValueKrwPerKg = unitUsd.HasValue && fxRate is > 0
                ? decimal.Round(unitUsd.Value * fxRate.Value, 0, MidpointRounding.AwayFromZero)
                : null,
            ExportWeightKg = exportWeightKg,
            ExportValueUsd = exportValueUsd,
            AverageExportUnitValueUsdPerKg = exportUnitUsd,
            AverageExportUnitValueKrwPerKg = exportUnitUsd.HasValue && fxRate is > 0
                ? decimal.Round(exportUnitUsd.Value * fxRate.Value, 0, MidpointRounding.AwayFromZero)
                : null
        };
    }

    private static decimal? ResolveLandedCost(
        HsCountryMonthlyTradeUnitPriceRequest request,
        decimal? averageKrw)
    {
        var purchase = request.ExpectedPurchaseUnitPriceKrwPerKg ?? averageKrw;
        if (!purchase.HasValue)
        {
            return null;
        }

        return purchase.Value + Math.Max(0m, request.ExpectedDomesticLogisticsCostKrwPerKg ?? 0m);
    }

    private static string ResolvePriceSignal(
        HsCountryMonthlyTradeUnitPriceRequest request,
        decimal? averageKrw,
        decimal? landedCost,
        decimal? marginRate)
    {
        if (marginRate.HasValue)
        {
            if (marginRate.Value >= 0.25m) return "Attractive";
            if (marginRate.Value >= 0.1m) return "Viable";
            if (marginRate.Value >= 0m) return "ThinMargin";
            return "LossRisk";
        }

        if (request.ExpectedPurchaseUnitPriceKrwPerKg.HasValue && averageKrw.HasValue)
        {
            var ratio = request.ExpectedPurchaseUnitPriceKrwPerKg.Value / averageKrw.Value;
            if (ratio <= 0.9m) return "BelowMarketImportAverage";
            if (ratio <= 1.1m) return "NearMarketImportAverage";
            return "AboveMarketImportAverage";
        }

        return landedCost.HasValue ? "CostReady" : "Unknown";
    }

    private static string BuildSummary(
        decimal? averageUsd,
        decimal? averageKrw,
        decimal? averageExportUsd,
        decimal? averageExportKrw,
        decimal? landedCost,
        decimal? marginRate,
        decimal? participantMargin)
    {
        if (!averageUsd.HasValue && !averageExportUsd.HasValue)
        {
            return "Trade statistics were found, but unit values could not be calculated because net weight is missing.";
        }

        var parts = new List<string>();
        if (averageUsd.HasValue)
        {
            var import = $"Average CIF import unit value is USD {averageUsd.Value:N2}/kg";
            import += averageKrw.HasValue
                ? $" (about KRW {averageKrw.Value:N0}/kg)."
                : ".";
            parts.Add(import);
        }

        if (averageExportUsd.HasValue)
        {
            var export = $"Average FOB export unit value is USD {averageExportUsd.Value:N2}/kg";
            export += averageExportKrw.HasValue
                ? $" (about KRW {averageExportKrw.Value:N0}/kg)."
                : ".";
            parts.Add(export);
        }

        var summary = string.Join(" ", parts);
        if (landedCost.HasValue)
        {
            summary += $" Expected landed cost is KRW {landedCost.Value:N0}/kg.";
        }

        if (marginRate.HasValue)
        {
            summary += $" Expected gross margin rate is {marginRate.Value:P1}.";
        }

        if (participantMargin.HasValue)
        {
            summary += $" Participant quantity gross margin is about KRW {participantMargin.Value:N0}.";
        }

        return summary;
    }

    private string ResolveServiceKey()
    {
        if (!string.IsNullOrWhiteSpace(_options.CustomsTradeStatistics.ServiceKey))
        {
            return _options.CustomsTradeStatistics.ServiceKey;
        }

        if (!string.IsNullOrWhiteSpace(_options.DataGoKrServiceKey))
        {
            return _options.DataGoKrServiceKey;
        }

        return _options.ServiceKey;
    }

    private static string NormalizeHsCode(string? value)
        => Regex.Replace(value ?? string.Empty, "[^0-9]", string.Empty);

    private static string NormalizeCountryCode(string? value)
        => Regex.Replace(value ?? string.Empty, "[^0-9A-Za-z]", string.Empty).ToUpperInvariant();

    private static string NormalizeTariffCode(string? value)
        => Regex.Replace(value ?? string.Empty, "[^0-9A-Za-z]", string.Empty).ToUpperInvariant();

    private static string ResolveHsCodeScheme(string? requestedScheme, string hsCode)
    {
        if (!string.IsNullOrWhiteSpace(requestedScheme))
        {
            return requestedScheme.Trim().ToUpperInvariant();
        }

        return hsCode.Length == 10 ? "HSK" : "HS";
    }

    private static string? NormalizeMonth(string? value)
    {
        var digits = Regex.Replace(value ?? string.Empty, "[^0-9]", string.Empty);
        return digits.Length == 6 ? digits : null;
    }

    private static string AddMonths(string yyyymm, int months)
    {
        var date = DateTime.ParseExact(yyyymm + "01", "yyyyMMdd", CultureInfo.InvariantCulture);
        return date.AddMonths(months).ToString("yyyyMM", CultureInfo.InvariantCulture);
    }

    private static HsCountryImportUnitPriceSimulationResult Fail(
        HsCountryMonthlyTradeUnitPriceRequest request,
        string message,
        string? startMonth = null,
        string? endMonth = null)
        => new()
        {
            Success = false,
            ErrorMessage = message,
            InternalProductCode = request.InternalProductCode.Trim(),
            ProductName = request.ProductName.Trim(),
            HsCode = NormalizeHsCode(request.HsCode),
            HsCodeScheme = ResolveHsCodeScheme(request.HsCodeScheme, NormalizeHsCode(request.HsCode)),
            NationalTariffCodeScheme = request.NationalTariffCodeScheme.Trim().ToUpperInvariant(),
            NationalTariffCode = NormalizeTariffCode(request.NationalTariffCode),
            CountryCode = NormalizeCountryCode(request.CountryCode),
            StartMonth = startMonth ?? string.Empty,
            EndMonth = endMonth ?? NormalizeMonth(request.Month) ?? string.Empty,
            Summary = message
        };
}
