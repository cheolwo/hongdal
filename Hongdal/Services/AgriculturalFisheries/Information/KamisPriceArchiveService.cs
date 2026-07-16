using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Hongdal.Domain.AgriculturalFisheries;
using Hongdal.Infrastructure.Persistence.AgriculturalFisheries;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Services.AgriculturalFisheries.Information;

public sealed record KamisPriceArchiveResult(
    long CollectionRunId,
    int FetchedCount,
    int InsertedCount,
    int UpdatedCount,
    int ExistingCount,
    DateOnly? LatestSurveyDate);

public interface IKamisPriceArchiveService
{
    Task<KamisPriceArchiveResult> CollectDailyPricesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default);
}

public sealed partial class KamisPriceArchiveService : IKamisPriceArchiveService
{
    private const string SourceUrl = "https://www.kamis.or.kr/service/price/xml.do";
    private const string NationwideCode = "ALL";
    private const string NationwideName = "전국";

    private static readonly IReadOnlyDictionary<string, string> ProductClasses =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["01"] = "소매",
            ["02"] = "도매"
        };

    private static readonly IReadOnlyDictionary<string, string> Categories =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["100"] = "식량작물",
            ["200"] = "채소류",
            ["300"] = "특용작물",
            ["400"] = "과일류",
            ["500"] = "축산물",
            ["600"] = "수산물"
        };

    private readonly HttpClient _httpClient;
    private readonly AgriculturalFisheriesDbContext _db;
    private readonly PublicDataOptions _options;
    private readonly ILogger<KamisPriceArchiveService> _logger;

    public KamisPriceArchiveService(
        HttpClient httpClient,
        AgriculturalFisheriesDbContext db,
        IOptions<PublicDataOptions> options,
        ILogger<KamisPriceArchiveService> logger)
    {
        _httpClient = httpClient;
        _db = db;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<KamisPriceArchiveResult> CollectDailyPricesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default)
    {
        if (requestedDate.Year is < 1990 or > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(requestedDate));
        }

        var kamis = _options.Kamis;
        if (string.IsNullOrWhiteSpace(kamis.CertificationKey)
            || string.IsNullOrWhiteSpace(kamis.RequesterId))
        {
            throw new InvalidOperationException(
                "KAMIS 인증값이 설정되지 않았습니다. PublicData:Kamis 설정을 확인해 주세요.");
        }

        var run = new KamisPriceCollectionRun
        {
            RequestedDate = requestedDate,
            QuerySummary =
                $"KAMIS 전국 일별 부류별 가격 / 도매·소매 / 6개 부류 / 요청일 {requestedDate:yyyy-MM-dd} / kg 환산",
            SourceUrl = SourceUrl,
            StartedAtUtc = DateTime.UtcNow
        };
        _db.KamisCollectionRuns.Add(run);
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            var collectedAtUtc = DateTime.UtcNow;
            var incoming = new List<KamisPriceObservation>();
            foreach (var productClass in ProductClasses)
            {
                foreach (var category in Categories)
                {
                    incoming.AddRange(await FetchCategoryAsync(
                        requestedDate,
                        productClass.Key,
                        productClass.Value,
                        category.Key,
                        category.Value,
                        collectedAtUtc,
                        cancellationToken));
                }
            }

            var deduplicated = incoming
                .GroupBy(item => item.RecordKey, StringComparer.Ordinal)
                .Select(group => group.Last())
                .ToArray();
            var recordKeys = deduplicated
                .Select(item => item.RecordKey)
                .ToHashSet(StringComparer.Ordinal);
            var existing = await _db.KamisPriceObservations
                .Where(item => recordKeys.Contains(item.RecordKey))
                .ToDictionaryAsync(item => item.RecordKey, StringComparer.Ordinal, cancellationToken);

            var updatedCount = 0;
            foreach (var item in deduplicated)
            {
                if (existing.TryGetValue(item.RecordKey, out var stored))
                {
                    if (HasMaterialChanges(stored, item))
                    {
                        CopyMutableValues(stored, item);
                        stored.UpdatedAtUtc = collectedAtUtc;
                        updatedCount++;
                    }

                    stored.LastSeenAtUtc = collectedAtUtc;
                    continue;
                }

                item.FirstCollectionRunId = run.Id;
                _db.KamisPriceObservations.Add(item);
            }

            var latestSurveyDate = deduplicated
                .Select(item => item.SurveyDate)
                .Cast<DateOnly?>()
                .DefaultIfEmpty()
                .Max();
            run.StatusCode = KamisArchiveStatusCodes.Completed;
            run.CompletedAtUtc = DateTime.UtcNow;
            run.LatestSurveyDate = latestSurveyDate;
            run.FetchedCount = deduplicated.Length;
            run.InsertedCount = deduplicated.Length - existing.Count;
            run.UpdatedCount = updatedCount;
            run.ExistingCount = existing.Count;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "KAMIS 국내 농수산물 가격 수집 완료. RunId={RunId}, Fetched={Fetched}, Inserted={Inserted}, Updated={Updated}, Existing={Existing}",
                run.Id,
                run.FetchedCount,
                run.InsertedCount,
                run.UpdatedCount,
                run.ExistingCount);

            return new KamisPriceArchiveResult(
                run.Id,
                run.FetchedCount,
                run.InsertedCount,
                run.UpdatedCount,
                run.ExistingCount,
                latestSurveyDate);
        }
        catch (Exception ex)
        {
            _db.ChangeTracker.Clear();
            var failedRun = await _db.KamisCollectionRuns
                .SingleAsync(item => item.Id == run.Id, CancellationToken.None);
            failedRun.StatusCode = KamisArchiveStatusCodes.Failed;
            failedRun.CompletedAtUtc = DateTime.UtcNow;
            failedRun.ErrorMessage = ex.Message.Length <= 2000 ? ex.Message : ex.Message[..2000];
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task<IReadOnlyList<KamisPriceObservation>> FetchCategoryAsync(
        DateOnly requestedDate,
        string productClassCode,
        string productClassName,
        string categoryCode,
        string categoryName,
        DateTime collectedAtUtc,
        CancellationToken cancellationToken)
    {
        var kamis = _options.Kamis;
        var requestPath = QueryHelpers.AddQueryString(
            kamis.DailyCategoryPricePath.TrimStart('/'),
            new Dictionary<string, string?>
            {
                ["action"] = "dailyPriceByCategoryList",
                ["p_product_cls_code"] = productClassCode,
                ["p_country_code"] = string.Empty,
                ["p_regday"] = requestedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                ["p_convert_kg_yn"] = "Y",
                ["p_item_category_code"] = categoryCode,
                ["p_cert_key"] = kamis.CertificationKey,
                ["p_cert_id"] = kamis.RequesterId,
                ["p_returntype"] = "json"
            });

        using var response = await _httpClient.GetAsync(
            requestPath,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var data = ReadDataObject(document.RootElement, productClassCode, categoryCode);
        var resultCode = ReadString(data, "error_code");
        if (!string.Equals(resultCode, "000", StringComparison.Ordinal))
        {
            if (string.Equals(resultCode, "001", StringComparison.Ordinal))
            {
                return [];
            }

            throw new InvalidOperationException(
                $"KAMIS 요청이 거부되었습니다. 가격구분={productClassCode}, 부류={categoryCode}, 코드={resultCode}");
        }

        if (!TryGetProperty(data, "item", out var items))
        {
            return [];
        }

        return items.ValueKind switch
        {
            JsonValueKind.Array => items.EnumerateArray()
                .Select(item => MapObservation(
                    item,
                    requestedDate,
                    productClassCode,
                    productClassName,
                    categoryCode,
                    categoryName,
                    collectedAtUtc))
                .ToArray(),
            JsonValueKind.Object =>
            [
                MapObservation(
                    items,
                    requestedDate,
                    productClassCode,
                    productClassName,
                    categoryCode,
                    categoryName,
                    collectedAtUtc)
            ],
            _ => []
        };
    }

    private static JsonElement ReadDataObject(
        JsonElement root,
        string productClassCode,
        string categoryCode)
    {
        if (!TryGetProperty(root, "data", out var data))
        {
            throw new InvalidOperationException("KAMIS 응답에 data 항목이 없습니다.");
        }

        if (data.ValueKind == JsonValueKind.Object)
        {
            return data;
        }

        if (data.ValueKind == JsonValueKind.Array)
        {
            var first = data.EnumerateArray().FirstOrDefault();
            if (first.ValueKind == JsonValueKind.Object)
            {
                return first;
            }

            var code = first.ValueKind == JsonValueKind.String
                ? first.GetString()
                : first.GetRawText();
            throw new InvalidOperationException(
                $"KAMIS 응답 형식이 올바르지 않습니다. 가격구분={productClassCode}, 부류={categoryCode}, 코드={code}");
        }

        throw new InvalidOperationException("KAMIS 응답의 data 항목 형식이 올바르지 않습니다.");
    }

    private static KamisPriceObservation MapObservation(
        JsonElement source,
        DateOnly requestedDate,
        string productClassCode,
        string productClassName,
        string categoryCode,
        string categoryName,
        DateTime collectedAtUtc)
    {
        var itemName = ReadString(source, "item_name", "itemname");
        var itemCode = ReadString(source, "item_code", "itemcode");
        var kindName = ReadString(source, "kind_name", "kindname");
        var kindCode = ReadString(source, "kind_code", "kindcode");
        var rankName = ReadString(source, "rank");
        var rankCode = ReadString(source, "rank_code", "rankcode");
        var unit = ReadString(source, "unit");
        var priceRaw = ReadString(source, "dpr1");
        var surveyDate = ParseSurveyDate(ReadString(source, "day1"), requestedDate);
        var identity = string.Join(
            '\u001f',
            productClassCode,
            categoryCode,
            NationwideCode,
            surveyDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            itemCode,
            kindCode,
            rankCode,
            unit);

        return new KamisPriceObservation
        {
            RecordKey = UsdaNassPriceArchiveService.Sha256(identity),
            ProductClassCode = productClassCode,
            ProductClassName = productClassName,
            CategoryCode = categoryCode,
            CategoryName = categoryName,
            CountryCode = NationwideCode,
            CountryName = NationwideName,
            RequestedDate = requestedDate,
            SurveyDate = surveyDate,
            ItemName = itemName,
            ItemCode = itemCode,
            KindName = kindName,
            KindCode = kindCode,
            RankName = rankName,
            RankCode = rankCode,
            Unit = unit,
            PriceRaw = priceRaw,
            PriceKrw = ParsePrice(priceRaw),
            PreviousDayLabel = ReadString(source, "day2"),
            PreviousDayPriceKrw = ParsePrice(ReadString(source, "dpr2")),
            OneWeekAgoLabel = ReadString(source, "day3"),
            OneWeekAgoPriceKrw = ParsePrice(ReadString(source, "dpr3")),
            TwoWeeksAgoLabel = ReadString(source, "day4"),
            TwoWeeksAgoPriceKrw = ParsePrice(ReadString(source, "dpr4")),
            OneMonthAgoLabel = ReadString(source, "day5"),
            OneMonthAgoPriceKrw = ParsePrice(ReadString(source, "dpr5")),
            OneYearAgoLabel = ReadString(source, "day6"),
            OneYearAgoPriceKrw = ParsePrice(ReadString(source, "dpr6")),
            NormalYearLabel = ReadString(source, "day7"),
            NormalYearPriceKrw = ParsePrice(ReadString(source, "dpr7")),
            IsPriceMissing = ParsePrice(priceRaw) is null,
            SourceUrl = SourceUrl,
            RawJson = source.GetRawText(),
            FirstCollectedAtUtc = collectedAtUtc,
            LastSeenAtUtc = collectedAtUtc,
            UpdatedAtUtc = collectedAtUtc
        };
    }

    internal static DateOnly ParseSurveyDate(string value, DateOnly requestedDate)
    {
        var fullDate = FullDateRegex().Match(value);
        if (fullDate.Success
            && DateOnly.TryParseExact(
                fullDate.Value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedFullDate))
        {
            return parsedFullDate;
        }

        var monthDay = MonthDayRegex().Match(value);
        if (!monthDay.Success
            || !int.TryParse(monthDay.Groups["month"].Value, out var month)
            || !int.TryParse(monthDay.Groups["day"].Value, out var day))
        {
            return requestedDate;
        }

        var candidate = new DateOnly(requestedDate.Year, month, day);
        return candidate > requestedDate.AddDays(1)
            ? candidate.AddYears(-1)
            : candidate;
    }

    internal static decimal? ParsePrice(string value)
    {
        var normalized = value.Replace(",", string.Empty, StringComparison.Ordinal).Trim();
        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var parsed)
            ? parsed
            : null;
    }

    private static string ReadString(JsonElement source, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (!TryGetProperty(source, propertyName, out var value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.GetRawText(),
                _ => string.Empty
            };
        }

        return string.Empty;
    }

    private static bool TryGetProperty(
        JsonElement source,
        string propertyName,
        out JsonElement value)
    {
        if (source.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in source.EnumerateObject())
            {
                if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                {
                    value = property.Value;
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    private static bool HasMaterialChanges(
        KamisPriceObservation stored,
        KamisPriceObservation incoming)
        => stored.ItemName != incoming.ItemName
           || stored.KindName != incoming.KindName
           || stored.RankName != incoming.RankName
           || stored.Unit != incoming.Unit
           || stored.PriceRaw != incoming.PriceRaw
           || stored.PriceKrw != incoming.PriceKrw
           || stored.PreviousDayLabel != incoming.PreviousDayLabel
           || stored.PreviousDayPriceKrw != incoming.PreviousDayPriceKrw
           || stored.OneWeekAgoLabel != incoming.OneWeekAgoLabel
           || stored.OneWeekAgoPriceKrw != incoming.OneWeekAgoPriceKrw
           || stored.TwoWeeksAgoLabel != incoming.TwoWeeksAgoLabel
           || stored.TwoWeeksAgoPriceKrw != incoming.TwoWeeksAgoPriceKrw
           || stored.OneMonthAgoLabel != incoming.OneMonthAgoLabel
           || stored.OneMonthAgoPriceKrw != incoming.OneMonthAgoPriceKrw
           || stored.OneYearAgoLabel != incoming.OneYearAgoLabel
           || stored.OneYearAgoPriceKrw != incoming.OneYearAgoPriceKrw
           || stored.NormalYearLabel != incoming.NormalYearLabel
           || stored.NormalYearPriceKrw != incoming.NormalYearPriceKrw;

    private static void CopyMutableValues(
        KamisPriceObservation stored,
        KamisPriceObservation incoming)
    {
        stored.RequestedDate = incoming.RequestedDate;
        stored.ItemName = incoming.ItemName;
        stored.KindName = incoming.KindName;
        stored.RankName = incoming.RankName;
        stored.Unit = incoming.Unit;
        stored.PriceRaw = incoming.PriceRaw;
        stored.PriceKrw = incoming.PriceKrw;
        stored.PreviousDayLabel = incoming.PreviousDayLabel;
        stored.PreviousDayPriceKrw = incoming.PreviousDayPriceKrw;
        stored.OneWeekAgoLabel = incoming.OneWeekAgoLabel;
        stored.OneWeekAgoPriceKrw = incoming.OneWeekAgoPriceKrw;
        stored.TwoWeeksAgoLabel = incoming.TwoWeeksAgoLabel;
        stored.TwoWeeksAgoPriceKrw = incoming.TwoWeeksAgoPriceKrw;
        stored.OneMonthAgoLabel = incoming.OneMonthAgoLabel;
        stored.OneMonthAgoPriceKrw = incoming.OneMonthAgoPriceKrw;
        stored.OneYearAgoLabel = incoming.OneYearAgoLabel;
        stored.OneYearAgoPriceKrw = incoming.OneYearAgoPriceKrw;
        stored.NormalYearLabel = incoming.NormalYearLabel;
        stored.NormalYearPriceKrw = incoming.NormalYearPriceKrw;
        stored.IsPriceMissing = incoming.IsPriceMissing;
        stored.RawJson = incoming.RawJson;
    }

    [GeneratedRegex(@"\d{4}-\d{2}-\d{2}", RegexOptions.CultureInvariant)]
    private static partial Regex FullDateRegex();

    [GeneratedRegex(@"(?<month>\d{1,2})/(?<day>\d{1,2})", RegexOptions.CultureInvariant)]
    private static partial Regex MonthDayRegex();
}
