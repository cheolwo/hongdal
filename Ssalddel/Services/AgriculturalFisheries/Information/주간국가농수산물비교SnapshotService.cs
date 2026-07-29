using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using 살뜰.Services.Options;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface I주간국가농수산물비교SnapshotService
{
    Task<주간국가농수산물비교Snapshot?> UpsertPreviousCompletedWeekAsync(
        DateOnly publicationDate,
        CancellationToken cancellationToken = default);
}

public sealed class 주간국가농수산물비교SnapshotService
    : I주간국가농수산물비교SnapshotService
{
    private const string KamisSourceKey = "kamis-price-observations";
    private const string UsdaSourceKey = "usda-nass-price-observations";
    private const string ChinaUnconfiguredSourceKey = "china-official-price-source-unconfigured";
    private const string KamisDocumentationUrl = "https://www.kamis.or.kr";
    private const string UsdaDocumentationUrl = "https://quickstats.nass.usda.gov/api";

    private static readonly ProductDefinition[] Products =
    [
        new("apple", "사과", ["사과"], ["APPLES"]),
        new("potato", "감자", ["감자"], ["POTATOES"]),
        new("onion", "양파", ["양파"], ["ONIONS"]),
        new("rice", "쌀", ["쌀"], ["RICE"]),
        new("soybean", "콩·대두", ["콩", "대두"], ["SOYBEANS"]),
        new("mackerel", "고등어", ["고등어"], ["MACKEREL"])
    ];

    private static readonly IReadOnlyDictionary<string, int> MonthNumbers =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["JAN"] = 1, ["JANUARY"] = 1,
            ["FEB"] = 2, ["FEBRUARY"] = 2,
            ["MAR"] = 3, ["MARCH"] = 3,
            ["APR"] = 4, ["APRIL"] = 4,
            ["MAY"] = 5,
            ["JUN"] = 6, ["JUNE"] = 6,
            ["JUL"] = 7, ["JULY"] = 7,
            ["AUG"] = 8, ["AUGUST"] = 8,
            ["SEP"] = 9, ["SEPT"] = 9, ["SEPTEMBER"] = 9,
            ["OCT"] = 10, ["OCTOBER"] = 10,
            ["NOV"] = 11, ["NOVEMBER"] = 11,
            ["DEC"] = 12, ["DECEMBER"] = 12
        };

    private readonly AgriculturalFisheriesDbContext _db;
    private readonly CommunityEditorialBatchOptions _options;
    private readonly TimeProvider _timeProvider;

    public 주간국가농수산물비교SnapshotService(
        AgriculturalFisheriesDbContext db,
        IOptions<CommunityEditorialBatchOptions> options,
        TimeProvider timeProvider)
    {
        _db = db;
        _options = options.Value;
        _timeProvider = timeProvider;
    }

    public async Task<주간국가농수산물비교Snapshot?> UpsertPreviousCompletedWeekAsync(
        DateOnly publicationDate,
        CancellationToken cancellationToken = default)
    {
        var week = ResolvePreviousCompletedWeek(publicationDate);
        var existing = await _db.WeeklyCountryProductComparisonSnapshots
            .Include(snapshot => snapshot.Items)
            .FirstOrDefaultAsync(
                snapshot => snapshot.PeriodKey == week.PeriodKey,
                cancellationToken);
        var maxAgeDays = Math.Clamp(
            _options.WeeklyCountryProductComparisonMaxObservationAgeDays,
            7,
            366);
        var earliestReferenceDate = week.EndDate.AddDays(-(maxAgeDays - 1));

        var kamisRows = await _db.KamisPriceObservations
            .AsNoTracking()
            .Where(observation => observation.FrequencyCode == "Daily"
                                  && observation.SurveyDate >= earliestReferenceDate
                                  && observation.SurveyDate <= week.EndDate
                                  && !observation.IsPriceMissing
                                  && observation.PriceKrw.HasValue)
            .OrderByDescending(observation => observation.SurveyDate)
            .Take(5000)
            .ToListAsync(cancellationToken);
        var usdaRows = await _db.PriceObservations
            .AsNoTracking()
            .Where(observation => observation.Year >= earliestReferenceDate.Year - 1
                                  && observation.Year <= week.EndDate.Year
                                  && observation.FrequencyDesc == "MONTHLY"
                                  && observation.SourceDesc == "SURVEY"
                                  && observation.StatisticCategoryDesc == "PRICE RECEIVED"
                                  && observation.AggregationLevelDesc == "NATIONAL"
                                  && !observation.IsSuppressed
                                  && observation.NumericValue.HasValue)
            .OrderByDescending(observation => observation.Year)
            .ThenByDescending(observation => observation.EndCode)
            .ThenBy(observation => observation.CommodityDesc)
            .Take(10000)
            .ToListAsync(cancellationToken);

        var maxProducts = Math.Clamp(
            _options.WeeklyCountryProductComparisonMaxProducts,
            1,
            Products.Length);
        var candidateItems = Products
            .Take(maxProducts)
            .Select(product => BuildProductItems(
                product,
                kamisRows,
                usdaRows,
                earliestReferenceDate,
                week.EndDate))
            .Where(items => items.Any(item =>
                item.StatusCode == 주간국가농수산물비교상태Codes.관측값있음))
            .SelectMany(items => items)
            .ToArray();
        if (candidateItems.Length == 0)
        {
            return existing;
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var snapshot = existing ?? new 주간국가농수산물비교Snapshot
        {
            PeriodKey = week.PeriodKey,
            WeekStartDate = week.StartDate,
            WeekEndDate = week.EndDate,
            GeneratedAtUtc = now
        };
        if (existing is null)
        {
            _db.WeeklyCountryProductComparisonSnapshots.Add(snapshot);
        }
        else
        {
            _db.WeeklyCountryProductComparisonItems.RemoveRange(snapshot.Items);
            snapshot.Items.Clear();
        }

        snapshot.WeekStartDate = week.StartDate;
        snapshot.WeekEndDate = week.EndDate;
        snapshot.AvailableObservationCount = candidateItems.Count(item =>
            item.StatusCode == 주간국가농수산물비교상태Codes.관측값있음);
        snapshot.UpdatedAtUtc = now;
        foreach (var item in candidateItems)
        {
            snapshot.Items.Add(item);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return snapshot;
    }

    public static 주간비교기간 ResolvePreviousCompletedWeek(DateOnly publicationDate)
    {
        var daysSinceMonday = ((int)publicationDate.DayOfWeek + 6) % 7;
        var currentWeekStart = publicationDate.AddDays(-daysSinceMonday);
        var start = currentWeekStart.AddDays(-7);
        var end = start.AddDays(6);
        var periodKey =
            $"{ISOWeek.GetYear(start.ToDateTime(TimeOnly.MinValue)):0000}-W" +
            $"{ISOWeek.GetWeekOfYear(start.ToDateTime(TimeOnly.MinValue)):00}";
        return new 주간비교기간(periodKey, start, end);
    }

    private static IReadOnlyList<주간국가농수산물비교항목> BuildProductItems(
        ProductDefinition product,
        IReadOnlyList<KamisPriceObservation> kamisRows,
        IReadOnlyList<UsdaNassPriceObservation> usdaRows,
        DateOnly earliestReferenceDate,
        DateOnly weekEndDate)
    {
        var kamis = kamisRows
            .Where(row => Matches(row.ItemName, product.KamisAliases))
            .OrderByDescending(row => row.SurveyDate)
            .ThenBy(row => row.ProductClassName.Contains(
                "소매",
                StringComparison.Ordinal) ? 0 : 1)
            .ThenBy(row => row.KindName, StringComparer.Ordinal)
            .ThenBy(row => row.RankName, StringComparer.Ordinal)
            .FirstOrDefault();
        var usda = usdaRows
            .Where(IsConsolidatedUsdaSeries)
            .Where(row => Matches(row.CommodityDesc, product.UsdaAliases))
            .Select(row => TryResolveReferenceMonth(row, out var referenceDate)
                ? new DatedUsdaObservation(row, referenceDate)
                : null)
            .Where(item => item is not null
                           && item.ReferenceDate >= earliestReferenceDate
                           && item.ReferenceDate <= weekEndDate)
            .OrderByDescending(item => item!.ReferenceDate)
            .ThenBy(item => item!.Observation.UnitDesc, StringComparer.Ordinal)
            .FirstOrDefault();

        return
        [
            kamis is null
                ? MissingItem(
                    product,
                    "KR",
                    "한국",
                    KamisSourceKey,
                    "한국농수산식품유통공사 KAMIS",
                    KamisDocumentationUrl,
                    "완료 주 기준 허용 기간 안에 검증된 KAMIS 일별 관측값이 없습니다.")
                : new 주간국가농수산물비교항목
                {
                    ProductKey = product.Key,
                    ProductNameKo = product.NameKo,
                    CountryCode = "KR",
                    CountryNameKo = "한국",
                    StatusCode = 주간국가농수산물비교상태Codes.관측값있음,
                    SourceKey = KamisSourceKey,
                    SourceName = "한국농수산식품유통공사 KAMIS",
                    SourceUrl = PublicUrlOrFallback(kamis.SourceUrl, KamisDocumentationUrl),
                    ReferenceDate = kamis.SurveyDate,
                    OriginalProductName = kamis.ItemName,
                    MarketStage = $"{kamis.ProductClassName} 조사 가격",
                    Price = kamis.PriceKrw,
                    CurrencyCode = "KRW",
                    Unit = kamis.Unit,
                    ComparisonNote = BuildKamisNote(kamis)
                },
            usda is null
                ? MissingItem(
                    product,
                    "US",
                    "미국",
                    UsdaSourceKey,
                    "USDA NASS Quick Stats",
                    UsdaDocumentationUrl,
                    "완료 주 기준 허용 기간 안에 검증된 미국 전국 월별 생산자 수취가격이 없습니다.")
                : new 주간국가농수산물비교항목
                {
                    ProductKey = product.Key,
                    ProductNameKo = product.NameKo,
                    CountryCode = "US",
                    CountryNameKo = "미국",
                    StatusCode = 주간국가농수산물비교상태Codes.관측값있음,
                    SourceKey = UsdaSourceKey,
                    SourceName = "USDA NASS Quick Stats",
                    SourceUrl = PublicUrlOrFallback(
                        usda.Observation.SourceUrl,
                        UsdaDocumentationUrl),
                    ReferenceDate = usda.ReferenceDate,
                    OriginalProductName = usda.Observation.CommodityDesc,
                    MarketStage = "전국 생산자 수취가격(Prices Received)",
                    Price = usda.Observation.NumericValue,
                    CurrencyCode = ResolveUsdaCurrency(usda.Observation.UnitDesc),
                    Unit = usda.Observation.UnitDesc,
                    ComparisonNote =
                        "미국 생산자 단계 관측값이며 한국 유통가격이나 미국 소매가격이 아닙니다."
                },
            MissingItem(
                product,
                "CN",
                "중국",
                ChinaUnconfiguredSourceKey,
                "중국 공식 농수산물 가격 원천",
                string.Empty,
                "현재 서버에 검증된 중국 공식 품목가격 원천이 등록되지 않아 가격을 임의 생성하거나 대체하지 않습니다.",
                주간국가농수산물비교상태Codes.원천미등록)
        ];
    }

    private static 주간국가농수산물비교항목 MissingItem(
        ProductDefinition product,
        string countryCode,
        string countryName,
        string sourceKey,
        string sourceName,
        string sourceUrl,
        string note,
        string statusCode = 주간국가농수산물비교상태Codes.검증관측값없음)
        => new()
        {
            ProductKey = product.Key,
            ProductNameKo = product.NameKo,
            CountryCode = countryCode,
            CountryNameKo = countryName,
            StatusCode = statusCode,
            SourceKey = sourceKey,
            SourceName = sourceName,
            SourceUrl = sourceUrl,
            ComparisonNote = note
        };

    private static string BuildKamisNote(KamisPriceObservation observation)
    {
        var specifications = new[]
            {
                observation.KindName,
                observation.RankName
            }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return specifications.Length == 0
            ? "KAMIS 품목·거래단위 기준 관측값입니다."
            : $"KAMIS 규격: {string.Join(" · ", specifications)}";
    }

    private static bool IsConsolidatedUsdaSeries(UsdaNassPriceObservation row)
        => IsAllQualifier(row.ClassDesc)
           && IsAllQualifier(row.UtilPracticeDesc)
           && IsAllQualifier(row.ProductionPracticeDesc)
           && (string.IsNullOrWhiteSpace(row.DomainDesc) || row.DomainDesc == "TOTAL");

    private static bool IsAllQualifier(string value)
        => string.IsNullOrWhiteSpace(value)
           || value.StartsWith("ALL ", StringComparison.Ordinal)
           || value.Equals("ALL", StringComparison.Ordinal);

    private static bool TryResolveReferenceMonth(
        UsdaNassPriceObservation observation,
        out DateOnly referenceMonth)
    {
        referenceMonth = default;
        if (observation.Year is < 1900 or > 2100)
        {
            return false;
        }

        var month = int.TryParse(
                        observation.EndCode,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var endMonth)
                    && endMonth is >= 1 and <= 12
            ? endMonth
            : MonthNumbers.GetValueOrDefault(observation.ReferencePeriodDesc.Trim());
        if (month is < 1 or > 12)
        {
            return false;
        }

        referenceMonth = new DateOnly(observation.Year, month, 1);
        return true;
    }

    private static bool Matches(string value, IReadOnlyList<string> aliases)
    {
        var normalized = Normalize(value);
        return aliases.Any(alias =>
            string.Equals(normalized, Normalize(alias), StringComparison.Ordinal));
    }

    private static string Normalize(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var character in value.Normalize(NormalizationForm.FormKC))
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToUpperInvariant(character));
            }
        }

        return builder.ToString();
    }

    private static string ResolveUsdaCurrency(string unit)
        => unit.TrimStart().StartsWith('$')
           || unit.StartsWith("DOLLARS", StringComparison.OrdinalIgnoreCase)
            ? "USD"
            : string.Empty;

    private static string PublicUrlOrFallback(string? value, string fallback)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? value!
            : fallback;

    private sealed record ProductDefinition(
        string Key,
        string NameKo,
        IReadOnlyList<string> KamisAliases,
        IReadOnlyList<string> UsdaAliases);

    private sealed record DatedUsdaObservation(
        UsdaNassPriceObservation Observation,
        DateOnly ReferenceDate);
}

public sealed record 주간비교기간(
    string PeriodKey,
    DateOnly StartDate,
    DateOnly EndDate);
