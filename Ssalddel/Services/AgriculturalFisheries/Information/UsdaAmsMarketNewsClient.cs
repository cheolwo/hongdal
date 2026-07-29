using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed record UsdaAms보고서Descriptor(
    string SlugId,
    string SlugName,
    string ReportTitle,
    DateOnly LatestReportDate,
    IReadOnlyList<string> MarketTypes,
    IReadOnlyList<string> SectionNames);

public sealed class UsdaAms시장가격Row
{
    public string ReportDate { get; init; } = string.Empty;
    public string ReportBeginDate { get; init; } = string.Empty;
    public string ReportEndDate { get; init; } = string.Empty;
    public string PublishedDate { get; init; } = string.Empty;
    public string OfficeName { get; init; } = string.Empty;
    public string OfficeState { get; init; } = string.Empty;
    public string OfficeCity { get; init; } = string.Empty;
    public string MarketType { get; init; } = string.Empty;
    public string MarketLocationName { get; init; } = string.Empty;
    public string MarketLocationState { get; init; } = string.Empty;
    public string MarketLocationCity { get; init; } = string.Empty;
    public string SlugId { get; init; } = string.Empty;
    public string SlugName { get; init; } = string.Empty;
    public string ReportTitle { get; init; } = string.Empty;
    public string Community { get; init; } = string.Empty;
    public string Group { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Commodity { get; init; } = string.Empty;
    public string Variety { get; init; } = string.Empty;
    public string Repack { get; init; } = string.Empty;
    public string Package { get; init; } = string.Empty;
    public string Storage { get; init; } = string.Empty;
    public string TransportationMode { get; init; } = string.Empty;
    public string Grade { get; init; } = string.Empty;
    public string UnitSales { get; init; } = string.Empty;
    public string ItemSize { get; init; } = string.Empty;
    public string Appearance { get; init; } = string.Empty;
    public string Quality { get; init; } = string.Empty;
    public string Condition { get; init; } = string.Empty;
    public string Organic { get; init; } = string.Empty;
    public string Crop { get; init; } = string.Empty;
    public string Origin { get; init; } = string.Empty;
    public string District { get; init; } = string.Empty;
    public string Environment { get; init; } = string.Empty;
    public string LowPrice { get; init; } = string.Empty;
    public string HighPrice { get; init; } = string.Empty;
    public string MostlyLowPrice { get; init; } = string.Empty;
    public string MostlyHighPrice { get; init; } = string.Empty;
    public string WeightedAveragePrice { get; init; } = string.Empty;
    public string StoreCount { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string RawJson { get; init; } = "{}";
}

public sealed record UsdaAms시장가격Slice(
    int ReturnedRows,
    int TotalRows,
    int UserAllowedRows,
    IReadOnlyList<UsdaAms시장가격Row> Rows);

public interface IUsdaAmsMarketNewsClient
{
    Task<IReadOnlyList<UsdaAms보고서Descriptor>> GetReportsAsync(
        CancellationToken cancellationToken = default);

    Task<UsdaAms시장가격Slice> GetReportDetailsAsync(
        string slugId,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken cancellationToken = default);
}

public sealed class UsdaAmsMarketNewsClient(
    HttpClient httpClient,
    IOptions<PublicDataOptions> options) : IUsdaAmsMarketNewsClient
{
    private readonly UsdaAmsMarketNewsOptions _options =
        options.Value.UsdaAmsMarketNews;

    public async Task<IReadOnlyList<UsdaAms보고서Descriptor>> GetReportsAsync(
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(_options.ReportsPath);
        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                "USDA AMS 보고서 목록 응답이 배열 형식이 아닙니다.");
        }

        return document.RootElement
            .EnumerateArray()
            .Select(ReadReport)
            .Where(report =>
                !string.IsNullOrWhiteSpace(report.SlugId)
                && report.LatestReportDate != default)
            .ToArray();
    }

    public async Task<UsdaAms시장가격Slice> GetReportDetailsAsync(
        string slugId,
        DateOnly dateFrom,
        DateOnly dateTo,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slugId);
        var dateQuery = dateFrom == dateTo
            ? dateFrom.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
            : $"{dateFrom.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)}:"
              + dateTo.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
        var path =
            $"{_options.ReportsPath.TrimEnd('/')}/{Uri.EscapeDataString(slugId.Trim())}/Report%20Details"
            + $"?q=report_begin_date={Uri.EscapeDataString(dateQuery)}";
        using var request = CreateRequest(path);
        using var response = await httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(
            stream,
            cancellationToken: cancellationToken);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                $"USDA AMS 보고서 {slugId} 상세 응답이 객체 형식이 아닙니다.");
        }

        var root = document.RootElement;
        var stats = root.TryGetProperty("stats", out var statsElement)
            ? statsElement
            : default;
        var rows = root.TryGetProperty("results", out var results)
                   && results.ValueKind == JsonValueKind.Array
            ? results.EnumerateArray().Select(ReadRow).ToArray()
            : [];
        return new UsdaAms시장가격Slice(
            ReadInt(stats, "returnedRows", rows.Length),
            ReadInt(stats, "totalRows", rows.Length),
            ReadInt(stats, "userAllowedRows", 100_000),
            rows);
    }

    private HttpRequestMessage CreateRequest(string path)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "PublicData:UsdaAmsMarketNews:ApiKey 설정이 필요합니다.");
        }

        var request = new HttpRequestMessage(HttpMethod.Get, path);
        var token = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{_options.ApiKey.Trim()}:"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", token);
        return request;
    }

    private static UsdaAms보고서Descriptor ReadReport(JsonElement row)
        => new(
            Read(row, "slug_id"),
            Read(row, "slug_name"),
            Read(row, "report_title"),
            ParseDate(Read(row, "report_date")),
            ReadArray(row, "market_types"),
            ReadArray(row, "sectionNames"));

    private static UsdaAms시장가격Row ReadRow(JsonElement row)
        => new()
        {
            ReportDate = Read(row, "report_date"),
            ReportBeginDate = Read(row, "report_begin_date"),
            ReportEndDate = Read(row, "report_end_date"),
            PublishedDate = Read(row, "published_date"),
            OfficeName = Read(row, "office_name"),
            OfficeState = Read(row, "office_state"),
            OfficeCity = Read(row, "office_city"),
            MarketType = Read(row, "market_type"),
            MarketLocationName = Read(row, "market_location_name"),
            MarketLocationState = Read(row, "market_location_state"),
            MarketLocationCity = Read(row, "market_location_city"),
            SlugId = Read(row, "slug_id"),
            SlugName = Read(row, "slug_name"),
            ReportTitle = Read(row, "report_title"),
            Community = Read(row, "community"),
            Group = ReadFirst(row, "group", "grp"),
            Category = Read(row, "category"),
            Commodity = Read(row, "commodity"),
            Variety = ReadFirst(row, "variety", "var"),
            Repack = Read(row, "repack"),
            Package = ReadFirst(row, "package", "pkg"),
            Storage = Read(row, "storage"),
            TransportationMode = Read(row, "transportation_mode"),
            Grade = Read(row, "grade"),
            UnitSales = Read(row, "unit_sales"),
            ItemSize = ReadFirst(row, "item_size", "size"),
            Appearance = ReadFirst(row, "appearance", "appear"),
            Quality = Read(row, "quality"),
            Condition = ReadFirst(row, "condition", "cond"),
            Organic = Read(row, "organic"),
            Crop = Read(row, "crop"),
            Origin = Read(row, "origin"),
            District = Read(row, "district"),
            Environment = ReadFirst(row, "environment", "env"),
            LowPrice = Read(row, "low_price"),
            HighPrice = Read(row, "high_price"),
            MostlyLowPrice = Read(row, "mostly_low_price"),
            MostlyHighPrice = Read(row, "mostly_high_price"),
            WeightedAveragePrice = Read(row, "wtd_avg_price"),
            StoreCount = Read(row, "store_count"),
            Region = Read(row, "region"),
            RawJson = row.GetRawText()
        };

    private static string Read(JsonElement element, string name)
    {
        if (element.ValueKind != JsonValueKind.Object
            || !element.TryGetProperty(name, out var value)
            || value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return string.Empty;
        }

        return value.ValueKind == JsonValueKind.String
            ? value.GetString()?.Trim() ?? string.Empty
            : value.ToString().Trim();
    }

    private static string ReadFirst(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            var value = Read(element, name);
            if (value.Length > 0)
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static IReadOnlyList<string> ReadArray(JsonElement element, string name)
        => element.TryGetProperty(name, out var array)
           && array.ValueKind == JsonValueKind.Array
            ? array.EnumerateArray()
                .Select(item => item.GetString()?.Trim() ?? string.Empty)
                .Where(item => item.Length > 0)
                .ToArray()
            : [];

    private static int ReadInt(JsonElement element, string name, int fallback)
        => element.ValueKind == JsonValueKind.Object
           && element.TryGetProperty(name, out var value)
           && value.TryGetInt32(out var result)
            ? result
            : fallback;

    internal static DateOnly ParseDate(string value)
        => DateOnly.TryParseExact(
            value,
            ["MM/dd/yyyy", "yyyy-MM-dd"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date)
            ? date
            : default;
}
