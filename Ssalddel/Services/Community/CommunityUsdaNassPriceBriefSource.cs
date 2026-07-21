using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public sealed record UsdaNassPriceBriefItem(
    string SectorName,
    string GroupName,
    string CommodityName,
    string Unit,
    decimal Price);

public sealed class CommunityUsdaNassPriceBriefSource : ICommunityAutomatedPostSource
{
    private const string DocumentationUrl = "https://quickstats.nass.usda.gov/api";

    private static readonly IReadOnlyDictionary<string, int> MonthNumbers =
        new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["JAN"] = 1,
            ["JANUARY"] = 1,
            ["FEB"] = 2,
            ["FEBRUARY"] = 2,
            ["MAR"] = 3,
            ["MARCH"] = 3,
            ["APR"] = 4,
            ["APRIL"] = 4,
            ["MAY"] = 5,
            ["JUN"] = 6,
            ["JUNE"] = 6,
            ["JUL"] = 7,
            ["JULY"] = 7,
            ["AUG"] = 8,
            ["AUGUST"] = 8,
            ["SEP"] = 9,
            ["SEPT"] = 9,
            ["SEPTEMBER"] = 9,
            ["OCT"] = 10,
            ["OCTOBER"] = 10,
            ["NOV"] = 11,
            ["NOVEMBER"] = 11,
            ["DEC"] = 12,
            ["DECEMBER"] = 12
        };

    private readonly AgriculturalFisheriesDbContext _db;
    private readonly CommunityEditorialBatchOptions _options;

    public CommunityUsdaNassPriceBriefSource(
        AgriculturalFisheriesDbContext db,
        IOptions<CommunityEditorialBatchOptions> options)
    {
        _db = db;
        _options = options.Value;
    }

    public string SourceKey => CommunityAutomatedPostSourceKeys.UsdaNassPriceBrief;

    public async Task<CommunityAutomatedPostDraft?> BuildAsync(
        DateOnly publicationDate,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken = default)
    {
        var observations = _db.PriceObservations
            .AsNoTracking()
            .Where(observation => observation.FrequencyDesc == "MONTHLY"
                                  && observation.SourceDesc == "SURVEY"
                                  && observation.StatisticCategoryDesc == "PRICE RECEIVED"
                                  && observation.AggregationLevelDesc == "NATIONAL"
                                  && !observation.IsSuppressed
                                  && observation.NumericValue.HasValue);
        var latestYear = await observations
            .MaxAsync(observation => (int?)observation.Year, cancellationToken);
        if (!latestYear.HasValue)
        {
            return null;
        }

        var rows = await observations
            .Where(observation => observation.Year >= latestYear.Value - 1)
            .OrderByDescending(observation => observation.Year)
            .ThenBy(observation => observation.CommodityDesc)
            .Take(5000)
            .ToArrayAsync(cancellationToken);
        var datedRows = rows
            .Where(IsConsolidatedSeries)
            .Select(observation => TryResolveReferenceMonth(observation, out var referenceMonth)
                ? new DatedObservation(observation, referenceMonth)
                : null)
            .Where(item => item is not null && item.ReferenceMonth <= publicationDate)
            .Select(item => item!)
            .ToArray();
        if (datedRows.Length == 0)
        {
            return null;
        }

        var latestReferenceMonth = datedRows.Max(item => item.ReferenceMonth);
        var maxItems = Math.Clamp(_options.UsdaNassPriceBriefMaxItems, 1, 12);
        var selected = datedRows
            .Where(item => item.ReferenceMonth == latestReferenceMonth)
            .GroupBy(
                item => new
                {
                    item.Observation.CommodityDesc,
                    item.Observation.UnitDesc
                })
            .Select(group => group
                .OrderBy(item => item.Observation.SectorDesc, StringComparer.Ordinal)
                .ThenBy(item => item.Observation.GroupDesc, StringComparer.Ordinal)
                .First().Observation)
            .OrderBy(observation => observation.SectorDesc, StringComparer.Ordinal)
            .ThenBy(observation => observation.GroupDesc, StringComparer.Ordinal)
            .ThenBy(observation => observation.CommodityDesc, StringComparer.Ordinal)
            .Take(maxItems)
            .Select(observation => new UsdaNassPriceBriefItem(
                observation.SectorDesc,
                observation.GroupDesc,
                observation.CommodityDesc,
                observation.UnitDesc,
                observation.NumericValue!.Value))
            .ToArray();
        if (selected.Length == 0)
        {
            return null;
        }

        var sourceUrl = datedRows
            .Where(item => item.ReferenceMonth == latestReferenceMonth)
            .Select(item => item.Observation.SourceUrl)
            .FirstOrDefault(IsPublicHttpUrl) ?? DocumentationUrl;
        return new CommunityAutomatedPostDraft(
            SourceKey,
            latestReferenceMonth.ToString("yyyyMM", CultureInfo.InvariantCulture),
            CommunityBoardCatalog.InformationPrices.DisplayName,
            "농수축산물 가격 정보",
            "자동 정보",
            $"[자동 가격정보] {latestReferenceMonth:yyyy-MM} USDA NASS 미국 생산자가격",
            BuildBody(latestReferenceMonth, selected),
            "살뜰 정보봇",
            sourceUrl);
    }

    public static string BuildBody(
        DateOnly referenceMonth,
        IReadOnlyList<UsdaNassPriceBriefItem> items)
    {
        var lines = new List<string>
        {
            "[자동 작성 안내] USDA NASS에 보관된 미국 전국 월별 공식 관측값 일부를 게시판 형식으로 정리했습니다.",
            $"기준월: {referenceMonth:yyyy-MM}",
            "시장 단계: 생산자 수취가격(Prices Received) — 미국 소매가격이 아닙니다.",
            string.Empty
        };
        foreach (var item in items)
        {
            var classification = string.Join(
                " · ",
                new[] { item.SectorName, item.GroupName }
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.Ordinal));
            lines.Add(
                $"- {item.CommodityName}{(classification.Length == 0 ? string.Empty : $" ({classification})")}: " +
                FormatPrice(item.Price, item.Unit));
        }

        lines.AddRange(
        [
            string.Empty,
            "출처: USDA National Agricultural Statistics Service (NASS) Quick Stats",
            "귀속: This product uses the NASS API but is not endorsed or certified by NASS.",
            "주의: 품목·등급·용도·생산방식·단위가 같은 계열만 비교할 수 있습니다. 한국 유통가격, 미국 소매가격 또는 개별 판매 견적으로 해석하지 마세요."
        ]);
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatPrice(decimal price, string unit)
    {
        var normalizedUnit = unit.Trim();
        if (normalizedUnit.StartsWith('$'))
        {
            var denominator = normalizedUnit[1..].Trim().TrimStart('/').Trim();
            return denominator.Length == 0
                ? $"USD {price.ToString("N2", CultureInfo.InvariantCulture)}"
                : $"USD {price.ToString("N2", CultureInfo.InvariantCulture)}/{denominator}";
        }

        if (normalizedUnit.StartsWith("DOLLARS", StringComparison.OrdinalIgnoreCase))
        {
            var denominator = normalizedUnit["DOLLARS".Length..]
                .Trim()
                .TrimStart('/')
                .Trim();
            return denominator.Length == 0
                ? $"USD {price.ToString("N2", CultureInfo.InvariantCulture)}"
                : $"USD {price.ToString("N2", CultureInfo.InvariantCulture)}/{denominator}";
        }

        var formatted = price.ToString("N2", CultureInfo.InvariantCulture);
        return normalizedUnit.Length == 0 ? formatted : $"{formatted} {normalizedUnit}";
    }

    private static bool IsConsolidatedSeries(UsdaNassPriceObservation observation)
        => IsAllQualifier(observation.ClassDesc)
           && IsAllQualifier(observation.UtilPracticeDesc)
           && IsAllQualifier(observation.ProductionPracticeDesc)
           && (string.IsNullOrWhiteSpace(observation.DomainDesc)
               || observation.DomainDesc == "TOTAL");

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

    private static bool IsPublicHttpUrl(string? value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private sealed record DatedObservation(
        UsdaNassPriceObservation Observation,
        DateOnly ReferenceMonth);
}
