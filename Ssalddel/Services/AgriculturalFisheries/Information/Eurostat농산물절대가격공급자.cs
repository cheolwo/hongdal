using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed partial class Eurostat농산물절대가격공급자(HttpClient httpClient)
    : I국제농수산가격공급자
{
    private static readonly IReadOnlyDictionary<string, string> DatasetProductDimensions =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["apri_ap_crpouta"] = "prod_veg",
            ["apri_ap_anouta"] = "prod_ani"
        };

    public string SourceKey =>
        국제농수산가격SourceKeys.Eurostat농산물절대생산자가격;

    public async Task<국제농수산가격공급결과> CollectAsync(
        int yearFrom,
        int yearTo,
        DateTime collectedAtUtc,
        CancellationToken cancellationToken = default)
    {
        var observations = new List<국제농수산가격관측>();
        var messages = new List<string>();
        var sourceUrls = new List<string>();
        foreach (var dataset in DatasetProductDimensions)
        {
            var requestPath =
                $"eurostat/api/dissemination/statistics/1.0/data/{dataset.Key}"
                + $"?lang=en&currency=EUR&sinceTimePeriod={yearFrom.ToString(CultureInfo.InvariantCulture)}"
                + $"&untilTimePeriod={yearTo.ToString(CultureInfo.InvariantCulture)}";
            using var response = await httpClient.GetAsync(
                requestPath,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(
                stream,
                cancellationToken: cancellationToken);
            var datasetObservations = ReadDataset(
                document.RootElement,
                dataset.Key,
                dataset.Value,
                new Uri(httpClient.BaseAddress!, requestPath).ToString(),
                collectedAtUtc);
            observations.AddRange(datasetObservations);
            sourceUrls.Add(new Uri(httpClient.BaseAddress!, requestPath).ToString());
            messages.Add(
                $"Eurostat {dataset.Key}에서 국가별 유로 표시 절대가격 {datasetObservations.Count:N0}건을 읽었습니다.");
        }

        var unique = observations
            .GroupBy(item => item.RecordKey, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToArray();
        messages.Add(
            "두 데이터셋을 모두 정상 수신한 뒤에만 저장 대상으로 확정했습니다.");
        return new 국제농수산가격공급결과(
            SourceKey,
            string.Join(" | ", sourceUrls),
            unique,
            messages);
    }

    internal static IReadOnlyList<국제농수산가격관측> ReadDataset(
        JsonElement root,
        string datasetCode,
        string productDimension,
        string sourceUrl,
        DateTime collectedAtUtc)
    {
        var dimensionIds = root.GetProperty("id")
            .EnumerateArray()
            .Select(item => item.GetString() ?? string.Empty)
            .ToArray();
        var sizes = root.GetProperty("size")
            .EnumerateArray()
            .Select(item => item.GetInt32())
            .ToArray();
        if (dimensionIds.Length != sizes.Length
            || !dimensionIds.Contains(productDimension, StringComparer.Ordinal)
            || !dimensionIds.Contains("geo", StringComparer.Ordinal)
            || !dimensionIds.Contains("time", StringComparer.Ordinal)
            || !dimensionIds.Contains("currency", StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                $"Eurostat {datasetCode} 응답 차원을 확인할 수 없습니다.");
        }

        var dimensions = dimensionIds
            .Select((id, index) => ReadDimension(root, id, sizes[index]))
            .ToArray();
        var values = root.GetProperty("value");
        if (values.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                $"Eurostat {datasetCode} 응답에 sparse value object가 없습니다.");
        }

        var statusByIndex = root.TryGetProperty("status", out var status)
                            && status.ValueKind == JsonValueKind.Object
            ? status.EnumerateObject().ToDictionary(
                item => item.Name,
                item => item.Value.ToString(),
                StringComparer.Ordinal)
            : new Dictionary<string, string>(StringComparer.Ordinal);
        var productIndex = Array.IndexOf(dimensionIds, productDimension);
        var geoIndex = Array.IndexOf(dimensionIds, "geo");
        var timeIndex = Array.IndexOf(dimensionIds, "time");
        var currencyIndex = Array.IndexOf(dimensionIds, "currency");
        var observations = new List<국제농수산가격관측>();
        foreach (var property in values.EnumerateObject())
        {
            if (!int.TryParse(
                    property.Name,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var flatIndex)
                || property.Value.ValueKind is not JsonValueKind.Number
                || !property.Value.TryGetDecimal(out var price))
            {
                continue;
            }

            var coordinates = DecodeCoordinates(flatIndex, sizes);
            var geo = dimensions[geoIndex][coordinates[geoIndex]];
            if (!CountryCodeRegex().IsMatch(geo.Code))
            {
                continue;
            }

            var time = dimensions[timeIndex][coordinates[timeIndex]];
            if (!int.TryParse(
                    time.Code,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var year))
            {
                continue;
            }

            var product = dimensions[productIndex][coordinates[productIndex]];
            var currency = dimensions[currencyIndex][coordinates[currencyIndex]];
            var (productName, originalUnit) = SplitProductAndUnit(product.Label);
            observations.Add(new 국제농수산가격관측
            {
                RecordKey =
                    $"eurostat:{datasetCode}:{geo.Code}:{currency.Code}:{product.Code}:{year}",
                SourceKey =
                    국제농수산가격SourceKeys.Eurostat농산물절대생산자가격,
                DatasetCode = datasetCode,
                CountryCode = geo.Code,
                CountryName = geo.Label,
                GeographyCode = geo.Code,
                GeographyName = geo.Label,
                MarketStageCode = 농수산시세시장단계Codes.생산자수취,
                OfficialSeriesCode =
                    $"{datasetCode}:{geo.Code}:{currency.Code}:{product.Code}",
                OfficialProductCode = product.Code,
                ProductNameOriginal = productName,
                ReferenceDate = new DateOnly(year, 1, 1),
                FrequencyCode = "Annual",
                ValueRaw = property.Value.GetRawText(),
                Price = price,
                CurrencyCode = currency.Code,
                OriginalUnit = originalUnit,
                ObservationStatus = statusByIndex.GetValueOrDefault(property.Name, string.Empty),
                SourceUrl = sourceUrl,
                RawJson = JsonSerializer.Serialize(new
                {
                    dataset = datasetCode,
                    frequency = dimensions[0][coordinates[0]].Code,
                    currency = currency.Code,
                    product_code = product.Code,
                    product_label = product.Label,
                    geography_code = geo.Code,
                    geography_label = geo.Label,
                    year,
                    value = price,
                    status = statusByIndex.GetValueOrDefault(property.Name, string.Empty)
                }),
                FirstCollectedAtUtc = collectedAtUtc,
                LastSeenAtUtc = collectedAtUtc
            });
        }

        return observations;
    }

    private static IReadOnlyList<DimensionMember> ReadDimension(
        JsonElement root,
        string dimensionId,
        int expectedSize)
    {
        var category = root
            .GetProperty("dimension")
            .GetProperty(dimensionId)
            .GetProperty("category");
        var positions = category.GetProperty("index")
            .EnumerateObject()
            .ToDictionary(
                item => item.Name,
                item => item.Value.GetInt32(),
                StringComparer.Ordinal);
        var labels = category.TryGetProperty("label", out var labelElement)
                     && labelElement.ValueKind == JsonValueKind.Object
            ? labelElement.EnumerateObject().ToDictionary(
                item => item.Name,
                item => item.Value.GetString() ?? item.Name,
                StringComparer.Ordinal)
            : new Dictionary<string, string>(StringComparer.Ordinal);
        var members = positions
            .Select(item => new
            {
                Position = item.Value,
                Member = new DimensionMember(
                    item.Key,
                    labels.GetValueOrDefault(item.Key, item.Key))
            })
            .OrderBy(item => item.Position)
            .Select(item => item.Member)
            .ToArray();
        if (members.Length != expectedSize)
        {
            throw new InvalidOperationException(
                $"Eurostat 차원 {dimensionId}의 크기가 size metadata와 일치하지 않습니다.");
        }

        return members;
    }

    private static int[] DecodeCoordinates(int flatIndex, IReadOnlyList<int> sizes)
    {
        var coordinates = new int[sizes.Count];
        for (var index = sizes.Count - 1; index >= 0; index--)
        {
            coordinates[index] = flatIndex % sizes[index];
            flatIndex /= sizes[index];
        }

        return coordinates;
    }

    private static (string ProductName, string OriginalUnit) SplitProductAndUnit(
        string label)
    {
        const string separator = " - prices per ";
        var separatorIndex = label.LastIndexOf(
            separator,
            StringComparison.OrdinalIgnoreCase);
        if (separatorIndex < 0)
        {
            return (label.Trim(), "official published unit");
        }

        return (
            label[..separatorIndex].Trim(),
            $"per {label[(separatorIndex + separator.Length)..].Trim()}");
    }

    private sealed record DimensionMember(string Code, string Label);

    [GeneratedRegex("^[A-Z]{2}$", RegexOptions.CultureInvariant)]
    private static partial Regex CountryCodeRegex();
}
