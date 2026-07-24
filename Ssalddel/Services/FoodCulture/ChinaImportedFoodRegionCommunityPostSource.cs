using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.Community;

namespace Ssalddel.Services.FoodCulture;

public interface IChinaImportedFoodRegionCommunityPostSource
{
    Task<CommunityAutomatedPostDraft?> BuildAsync(
        DateOnly publicationDate,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken = default);
}

public sealed class ChinaImportedFoodRegionCommunityPostSource(
    AgriculturalFisheriesDbContext db)
    : IChinaImportedFoodRegionCommunityPostSource,
        ICommunityAutomatedPostSource
{
    public const string SourceKeyValue = "china-imported-food-region-brief";

    public const string OfficialSourceUrl =
        "https://www.data.go.kr/data/15098434/fileData.do?recommendDataYn=Y";

    private static readonly IReadOnlyDictionary<string, int> RegionOrder =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [ChinaImportedFoodManufacturerRegionCodes.LiaoningLiaodong] = 1,
            [ChinaImportedFoodManufacturerRegionCodes.Shandong] = 2,
            [ChinaImportedFoodManufacturerRegionCodes.LowerYangtzeJiangnan] = 3,
            [ChinaImportedFoodManufacturerRegionCodes.OtherOrUnclassified] = 4
        };

    public string SourceKey => SourceKeyValue;

    public async Task<CommunityAutomatedPostDraft?> BuildAsync(
        DateOnly publicationDate,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken = default)
    {
        var regionCodes = RegionOrder.Keys.ToArray();
        var evidence = await db.OfficialFoodIngredientCompanyEvidence
            .AsNoTracking()
            .Where(item => item.IsCurrent
                           && item.RelationCode
                           == OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer
                           && (item.CountryCode == "CN" || item.CountryName == "중국")
                           && Enumerable.Contains(regionCodes, item.ManufacturerRegionCode))
            .Select(item => new RegionEvidence(
                item.ManufacturerRegionCode,
                item.ManufacturerRegionName,
                item.OrganizationKey,
                item.IngredientId,
                item.Ingredient!.CanonicalName,
                item.FirstObservedAtUtc,
                item.LastObservedAtUtc,
                item.ObservationCount))
            .ToArrayAsync(cancellationToken);
        if (evidence.Length == 0)
        {
            return null;
        }

        var monthStart = new DateOnly(publicationDate.Year, publicationDate.Month, 1);
        var monthEnd = monthStart.AddMonths(1);
        var summaries = RegionOrder
            .OrderBy(item => item.Value)
            .Select(region =>
            {
                var items = evidence
                    .Where(item => item.RegionCode == region.Key)
                    .ToArray();
                return new RegionSummary(
                    region.Key,
                    items.Select(item => item.RegionName)
                        .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name))
                    ?? RegionDisplayName(region.Key),
                    items.Length,
                    items.Select(item => item.OrganizationKey)
                        .Distinct(StringComparer.Ordinal)
                        .Count(),
                    items.Select(item => item.IngredientId).Distinct().Count(),
                    items.Count(item =>
                    {
                        var observedDate = LocalDate(item.FirstObservedAtUtc, timeZone);
                        return observedDate >= monthStart && observedDate < monthEnd;
                    }),
                    items.Count(item =>
                    {
                        var observedDate = LocalDate(item.LastObservedAtUtc, timeZone);
                        return item.ObservationCount > 1
                               && observedDate >= monthStart
                               && observedDate < monthEnd;
                    }),
                    items.GroupBy(item => new
                        {
                            item.IngredientId,
                            item.IngredientName
                        })
                        .Select(ingredient => new IngredientCount(
                            ingredient.Key.IngredientName,
                            ingredient.Count()))
                        .OrderByDescending(item => item.EvidenceCount)
                        .ThenBy(item => item.IngredientName, StringComparer.Ordinal)
                        .Take(3)
                        .ToArray());
            })
            .ToArray();
        var latestObservedDate = evidence.Max(item => LocalDate(item.LastObservedAtUtc, timeZone));
        var totalOrganizations = evidence
            .Select(item => item.OrganizationKey)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var totalIngredients = evidence.Select(item => item.IngredientId).Distinct().Count();

        return new CommunityAutomatedPostDraft(
            SourceKey,
            publicationDate.ToString("yyyyMM", CultureInfo.InvariantCulture),
            CommunityBoardCatalog.PeriodicDataMfds.DisplayName,
            CommunityImportedFoodCountryFilterCatalog.China.WorkflowTag,
            "자동 정보",
            $"[수입식품·공공데이터] {publicationDate:yyyy년 M월} 중국 제조업소 권역 누적",
            BuildBody(
                publicationDate,
                latestObservedDate,
                evidence.Length,
                totalOrganizations,
                totalIngredients,
                summaries),
            "살뜰 수입식품 정보봇",
            OfficialSourceUrl);
    }

    internal static string BuildBody(
        DateOnly publicationDate,
        DateOnly latestObservedDate,
        int totalEvidenceCount,
        int totalOrganizationCount,
        int totalIngredientCount,
        IReadOnlyList<RegionSummary> summaries)
    {
        var lines = new List<string>
        {
            "[자동 작성 안내] 식약처 수입식품 표시·해외제조업소 공식 근거를 재료별 조사 원장에서 누적 집계했습니다.",
            $"게시 주기: {publicationDate:yyyy년 M월} 월간 누적",
            $"최근 관찰일: {latestObservedDate:yyyy-MM-dd}",
            $"현재 근거: 제품 근거 {totalEvidenceCount:N0}행 · 업체 후보 {totalOrganizationCount:N0}개 · 재료 {totalIngredientCount:N0}종",
            "단위: 현재 상태인 재료-업체-제품 근거 행 수",
            string.Empty
        };

        foreach (var summary in summaries)
        {
            lines.Add(
                $"- {summary.RegionName}: 제품 근거 {summary.EvidenceCount:N0}행 · " +
                $"업체 후보 {summary.OrganizationCount:N0}개 · 재료 {summary.IngredientCount:N0}종 · " +
                $"이번 달 신규 {summary.NewEvidenceCount:N0}행 · 재확인 {summary.ReobservedEvidenceCount:N0}행");
            if (summary.TopIngredients.Count > 0)
            {
                lines.Add(
                    "  자주 관찰된 재료: " +
                    string.Join(
                        ", ",
                        summary.TopIngredients.Select(item =>
                            $"{item.IngredientName} {item.EvidenceCount:N0}행")));
            }
        }

        lines.AddRange(
        [
            string.Empty,
            "출처: 식품의약품안전처 수입식품 제품별 한글표시사항·해외제조업소 정보 및 공공데이터포털 수입식품 제품 목록",
            "주의: 이 집계는 수입량·수입액·거래 가능 업체 수가 아닙니다. 제조업소 소재 권역은 원재료 재배지·어획지나 법정 원산지 표기를 대신하지 않습니다.",
            "랴오닝성 전체를 요동산으로 단정하지 않으며, 강남·장강하류권은 상하이·장쑤·저장으로 한정한 분석용 운영 권역입니다.",
            "업체 자동 추천·선정·연락에는 사용하지 않으며 실제 거래 전 최신 공식 상태를 다시 확인해야 합니다."
        ]);
        return string.Join(Environment.NewLine, lines);
    }

    private static DateOnly LocalDate(DateTime utcDateTime, TimeZoneInfo timeZone)
    {
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone));
    }

    private static string RegionDisplayName(string regionCode)
        => regionCode switch
        {
            ChinaImportedFoodManufacturerRegionCodes.LiaoningLiaodong =>
                "랴오닝성·랴오둥권",
            ChinaImportedFoodManufacturerRegionCodes.Shandong => "산둥성",
            ChinaImportedFoodManufacturerRegionCodes.LowerYangtzeJiangnan =>
                "강남·장강하류권",
            _ => "중국 기타·미분류"
        };

    private sealed record RegionEvidence(
        string RegionCode,
        string RegionName,
        string OrganizationKey,
        long IngredientId,
        string IngredientName,
        DateTime FirstObservedAtUtc,
        DateTime LastObservedAtUtc,
        int ObservationCount);

    internal sealed record RegionSummary(
        string RegionCode,
        string RegionName,
        int EvidenceCount,
        int OrganizationCount,
        int IngredientCount,
        int NewEvidenceCount,
        int ReobservedEvidenceCount,
        IReadOnlyList<IngredientCount> TopIngredients);

    internal sealed record IngredientCount(string IngredientName, int EvidenceCount);
}
