using System.Globalization;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Services.Community;

public sealed class Community주간국가농수산물비교Source : ICommunityAutomatedPostSource
{
    private readonly I주간국가농수산물비교SnapshotService _snapshotService;

    public Community주간국가농수산물비교Source(
        I주간국가농수산물비교SnapshotService snapshotService)
    {
        _snapshotService = snapshotService;
    }

    public string SourceKey =>
        CommunityAutomatedPostSourceKeys.WeeklyCountryProductComparison;

    public async Task<CommunityAutomatedPostDraft?> BuildAsync(
        DateOnly publicationDate,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await _snapshotService.UpsertPreviousCompletedWeekAsync(
            publicationDate,
            cancellationToken);
        if (snapshot is null || snapshot.AvailableObservationCount == 0)
        {
            return null;
        }

        return new CommunityAutomatedPostDraft(
            SourceKey,
            snapshot.PeriodKey,
            CommunityBoardCatalog.InformationPrices.DisplayName,
            CultureTransportContentCatalog.PriceEvidenceWorkflowTag,
            "자동 정보",
            $"[주간 시세 비교] {snapshot.WeekStartDate:MM-dd}~{snapshot.WeekEndDate:MM-dd} 한·미·중 농수산물",
            BuildBody(snapshot),
            "살뜰 정보봇");
    }

    public static string BuildBody(주간국가농수산물비교Snapshot snapshot)
    {
        var lines = new List<string>
        {
            "[자동 작성 안내] DB에 보관된 공식 관측값을 직전 완료 주 기준으로 품목별 정리했습니다.",
            $"비교 기간: {snapshot.WeekStartDate:yyyy-MM-dd} ~ {snapshot.WeekEndDate:yyyy-MM-dd}",
            "비교 원칙: 통화·단위·시장 단계가 다르면 가격차나 순위를 계산하지 않고 원 관측값만 나란히 표시합니다.",
            string.Empty
        };

        foreach (var productGroup in snapshot.Items
                     .GroupBy(item => new { item.ProductKey, item.ProductNameKo })
                     .OrderBy(group => group.Key.ProductKey, StringComparer.Ordinal))
        {
            lines.Add($"■ {productGroup.Key.ProductNameKo}");
            foreach (var countryCode in new[] { "KR", "US", "CN" })
            {
                var item = productGroup.First(row => row.CountryCode == countryCode);
                lines.Add(FormatItem(item));
            }

            lines.Add(string.Empty);
        }

        lines.AddRange(
        [
            "출처: 한국농수산식품유통공사 KAMIS, USDA NASS Quick Stats",
            "중국: 현재 검증된 공식 품목가격 원천이 서버에 등록되지 않아 자료 없음으로 표시합니다.",
            "주의: 한국 값은 KAMIS 유통 관측, 미국 값은 생산자 수취가격입니다. 판매 권고가·수입원가·견적이 아니며 서로 다른 규격을 직접 환산하지 않습니다."
        ]);
        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatItem(주간국가농수산물비교항목 item)
    {
        if (item.StatusCode != 주간국가농수산물비교상태Codes.관측값있음
            || !item.Price.HasValue)
        {
            return $"- {item.CountryNameKo}: 자료 없음 — {item.ComparisonNote}";
        }

        var price = item.Price.Value.ToString(
            item.CurrencyCode == "KRW" ? "N0" : "N2",
            CultureInfo.InvariantCulture);
        var unit = FormatUnit(item.CurrencyCode, item.Unit);
        var currency = string.IsNullOrWhiteSpace(item.CurrencyCode)
            ? string.Empty
            : $"{item.CurrencyCode} ";
        return $"- {item.CountryNameKo}: {currency}{price}{unit} · " +
               $"기준 {item.ReferenceDate:yyyy-MM-dd} · {item.MarketStage} · {item.ComparisonNote}";
    }

    private static string FormatUnit(string currencyCode, string value)
    {
        var unit = value.Trim();
        if (unit.Length == 0)
        {
            return string.Empty;
        }

        if (currencyCode == "USD" && unit.StartsWith('$'))
        {
            unit = unit[1..].Trim().TrimStart('/').Trim();
        }
        else if (currencyCode == "USD"
                 && unit.StartsWith("DOLLARS", StringComparison.OrdinalIgnoreCase))
        {
            unit = unit["DOLLARS".Length..].Trim().TrimStart('/').Trim();
        }

        return unit.Length == 0 ? string.Empty : $"/{unit}";
    }
}
