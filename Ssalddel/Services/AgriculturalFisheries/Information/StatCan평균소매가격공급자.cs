using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed class StatCan평균소매가격공급자(HttpClient httpClient)
    : I국제농수산가격공급자
{
    private const string DatasetCode = "18100245";
    private const string DownloadDiscoveryPath =
        "t1/wds/rest/getFullTableDownloadCSV/18100245/en";
    private static readonly HashSet<string> ExcludedNonFoodProductMemberIds =
        ["75", "76", "77", "110"];

    public string SourceKey =>
        국제농수산가격SourceKeys.StatCan소비자평균소매가격;

    public async Task<국제농수산가격공급결과> CollectAsync(
        int yearFrom,
        int yearTo,
        DateTime collectedAtUtc,
        CancellationToken cancellationToken = default)
    {
        using var discoveryResponse = await httpClient.GetAsync(
            DownloadDiscoveryPath,
            cancellationToken);
        discoveryResponse.EnsureSuccessStatusCode();
        await using var discoveryStream = await discoveryResponse.Content
            .ReadAsStreamAsync(cancellationToken);
        using var discoveryDocument = await JsonDocument.ParseAsync(
            discoveryStream,
            cancellationToken: cancellationToken);
        var root = discoveryDocument.RootElement;
        var status = root.TryGetProperty("status", out var statusElement)
            ? statusElement.GetString()
            : null;
        var downloadUrl = root.TryGetProperty("object", out var objectElement)
            ? objectElement.GetString()
            : null;
        if (!string.Equals(status, "SUCCESS", StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(downloadUrl))
        {
            throw new InvalidOperationException(
                $"Statistics Canada WDS가 전체 표 다운로드 주소를 반환하지 않았습니다. Status={status}");
        }

        using var downloadResponse = await httpClient.GetAsync(
            downloadUrl,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        downloadResponse.EnsureSuccessStatusCode();
        await using var zipStream = await downloadResponse.Content
            .ReadAsStreamAsync(cancellationToken);
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
        var dataEntry = archive.Entries.FirstOrDefault(entry =>
            entry.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
            && !entry.Name.Contains("metadata", StringComparison.OrdinalIgnoreCase));
        if (dataEntry is null)
        {
            throw new InvalidOperationException(
                "Statistics Canada 전체 표 ZIP에서 데이터 CSV를 찾지 못했습니다.");
        }

        await using var dataStream = dataEntry.Open();
        using var reader = new StreamReader(
            dataStream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true);
        var headerLine = await reader.ReadLineAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            throw new InvalidOperationException(
                "Statistics Canada CSV에 header가 없습니다.");
        }

        var headers = ParseCsvLine(headerLine);
        var headerIndexes = headers
            .Select((name, index) => (Name: name.Trim(), Index: index))
            .ToDictionary(item => item.Name, item => item.Index, StringComparer.Ordinal);
        var requiredHeaders = new[]
        {
            "REF_DATE",
            "GEO",
            "DGUID",
            "Products",
            "UOM",
            "VECTOR",
            "COORDINATE",
            "VALUE",
            "STATUS",
            "SYMBOL",
            "TERMINATED",
            "DECIMALS"
        };
        var missingHeaders = requiredHeaders
            .Where(header => !headerIndexes.ContainsKey(header))
            .ToArray();
        if (missingHeaders.Length > 0)
        {
            throw new InvalidOperationException(
                $"Statistics Canada CSV 필수 열이 누락되었습니다: {string.Join(", ", missingHeaders)}");
        }

        var observations = new List<국제농수산가격관측>();
        var excludedNonFoodCount = 0;
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var fields = ParseCsvLine(line);
            if (fields.Length < headers.Length)
            {
                continue;
            }

            var referenceRaw = Read(fields, headerIndexes, "REF_DATE");
            if (!DateOnly.TryParseExact(
                    $"{referenceRaw}-01",
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var referenceDate)
                || referenceDate.Year < yearFrom
                || referenceDate.Year > yearTo)
            {
                continue;
            }

            var coordinate = Read(fields, headerIndexes, "COORDINATE");
            var coordinateParts = coordinate.Split(
                '.',
                StringSplitOptions.TrimEntries);
            var productMemberId = coordinateParts.Length >= 2
                ? coordinateParts[1]
                : string.Empty;
            if (ExcludedNonFoodProductMemberIds.Contains(productMemberId))
            {
                excludedNonFoodCount++;
                continue;
            }

            var valueRaw = Read(fields, headerIndexes, "VALUE");
            var price = decimal.TryParse(
                valueRaw,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var parsedPrice)
                ? parsedPrice
                : (decimal?)null;
            var productName = Read(fields, headerIndexes, "Products");
            var vector = Read(fields, headerIndexes, "VECTOR");
            var geographyCode = Read(fields, headerIndexes, "DGUID");
            var geographyName = Read(fields, headerIndexes, "GEO");
            var observationStatus = string.Join(
                " | ",
                new[]
                {
                    Read(fields, headerIndexes, "STATUS"),
                    Read(fields, headerIndexes, "SYMBOL"),
                    Read(fields, headerIndexes, "TERMINATED")
                }.Where(value => !string.IsNullOrWhiteSpace(value)));
            observations.Add(new 국제농수산가격관측
            {
                RecordKey =
                    $"statcan:{DatasetCode}:{vector}:{referenceDate:yyyyMM}",
                SourceKey = SourceKey,
                DatasetCode = DatasetCode,
                CountryCode = "CA",
                CountryName = "Canada",
                GeographyCode = geographyCode,
                GeographyName = geographyName,
                MarketStageCode = 농수산시세시장단계Codes.소비자평균소매,
                OfficialSeriesCode = vector,
                OfficialProductCode = productMemberId,
                ProductNameOriginal = productName,
                ReferenceDate = referenceDate,
                FrequencyCode = "Monthly",
                ValueRaw = valueRaw,
                Price = price,
                CurrencyCode = "CAD",
                OriginalUnit = ReadOriginalUnit(
                    productName,
                    Read(fields, headerIndexes, "UOM")),
                IsValueMissing = !price.HasValue,
                ObservationStatus = observationStatus,
                SourceUrl = downloadUrl,
                RawJson = JsonSerializer.Serialize(new
                {
                    ref_date = referenceRaw,
                    geography_code = geographyCode,
                    geography_name = geographyName,
                    product_member_id = productMemberId,
                    product_name = productName,
                    unit_of_measure = Read(fields, headerIndexes, "UOM"),
                    vector,
                    coordinate,
                    value = valueRaw,
                    status = Read(fields, headerIndexes, "STATUS"),
                    symbol = Read(fields, headerIndexes, "SYMBOL"),
                    terminated = Read(fields, headerIndexes, "TERMINATED"),
                    decimals = Read(fields, headerIndexes, "DECIMALS")
                }),
                FirstCollectedAtUtc = collectedAtUtc,
                LastSeenAtUtc = collectedAtUtc
            });
        }

        var unique = observations
            .GroupBy(item => item.RecordKey, StringComparer.Ordinal)
            .Select(group => group.Last())
            .ToArray();
        return new 국제농수산가격공급결과(
            SourceKey,
            downloadUrl,
            unique,
            [
                $"Statistics Canada 표 {DatasetCode}에서 {unique.Length:N0}개 식품 관측을 읽었습니다.",
                $"식품이 아닌 개인위생·세제 행 {excludedNonFoodCount:N0}개를 제외했습니다.",
                "공식 원자료의 캐나다 달러와 제품별 포장단위를 그대로 보존했습니다."
            ]);
    }

    internal static string[] ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var value = new StringBuilder();
        var quoted = false;
        for (var index = 0; index < line.Length; index++)
        {
            var character = line[index];
            if (character == '"')
            {
                if (quoted
                    && index + 1 < line.Length
                    && line[index + 1] == '"')
                {
                    value.Append('"');
                    index++;
                }
                else
                {
                    quoted = !quoted;
                }

                continue;
            }

            if (character == ',' && !quoted)
            {
                fields.Add(value.ToString());
                value.Clear();
                continue;
            }

            value.Append(character);
        }

        fields.Add(value.ToString());
        return fields.ToArray();
    }

    private static string Read(
        IReadOnlyList<string> fields,
        IReadOnlyDictionary<string, int> indexes,
        string name)
        => indexes.TryGetValue(name, out var index) && index < fields.Count
            ? fields[index].Trim()
            : string.Empty;

    private static string ReadOriginalUnit(string productName, string unitOfMeasure)
    {
        var commaIndex = productName.LastIndexOf(',');
        return commaIndex >= 0 && commaIndex + 1 < productName.Length
            ? productName[(commaIndex + 1)..].Trim()
            : string.IsNullOrWhiteSpace(unitOfMeasure)
                ? "published product unit"
                : unitOfMeasure.Trim();
    }
}
