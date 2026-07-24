using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.Community;

namespace Ssalddel.Services.FoodCulture;

public interface IUnitedStatesImportedFoodStateCommunityPostSource
{
    Task<CommunityAutomatedPostDraft?> BuildAsync(
        DateOnly publicationDate,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken = default);
}

public sealed class UnitedStatesImportedFoodStateCommunityPostSource(
    AgriculturalFisheriesDbContext db)
    : IUnitedStatesImportedFoodStateCommunityPostSource,
        ICommunityAutomatedPostSource
{
    public const string SourceKeyValue = "us-imported-food-state-brief";

    public const string OfficialSourceUrl =
        "https://www.data.go.kr/data/15098434/fileData.do?recommendDataYn=Y";

    public string SourceKey => SourceKeyValue;

    public async Task<CommunityAutomatedPostDraft?> BuildAsync(
        DateOnly publicationDate,
        TimeZoneInfo timeZone,
        CancellationToken cancellationToken = default)
    {
        var evidence = await db.OfficialFoodIngredientCompanyEvidence
            .AsNoTracking()
            .Where(item => item.IsCurrent
                           && item.RelationCode
                           == OfficialFoodIngredientCompanyRelationCodes.ForeignManufacturer
                           && (item.CountryCode == "US" || item.CountryName == "미국")
                           && item.ManufacturerRegionCode.StartsWith(
                               UnitedStatesImportedFoodManufacturerRegionCodes.Prefix))
            .Select(item => new StateEvidence(
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
        var summaries = evidence
            .GroupBy(item => item.RegionCode, StringComparer.Ordinal)
            .Select(group => BuildSummary(
                group.Key,
                group.ToArray(),
                monthStart,
                monthEnd,
                timeZone))
            .ToArray();
        var stateSummaries = summaries
            .Where(summary =>
                UnitedStatesImportedFoodManufacturerRegionClassifier.IsStateRegionCode(
                    summary.RegionCode))
            .OrderByDescending(summary => summary.EvidenceCount)
            .ThenBy(summary => summary.RegionName, StringComparer.Ordinal)
            .ToArray();
        var districtOrTerritorySummaries = summaries
            .Where(summary =>
                UnitedStatesImportedFoodManufacturerRegionClassifier
                    .IsDistrictOrTerritoryRegionCode(summary.RegionCode))
            .OrderByDescending(summary => summary.EvidenceCount)
            .ThenBy(summary => summary.RegionName, StringComparer.Ordinal)
            .ToArray();
        var unclassifiedSummary = summaries.FirstOrDefault(summary =>
            summary.RegionCode
            == UnitedStatesImportedFoodManufacturerRegionCodes.OtherOrUnclassified);
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
            CommunityImportedFoodCountryFilterCatalog.UnitedStates.WorkflowTag,
            "자동 정보",
            $"[수입식품·공공데이터] {publicationDate:yyyy년 M월} 미국 주별 제조업소 누적",
            BuildBody(
                publicationDate,
                latestObservedDate,
                evidence.Length,
                totalOrganizations,
                totalIngredients,
                stateSummaries,
                districtOrTerritorySummaries,
                unclassifiedSummary),
            "살뜰 수입식품 정보봇",
            OfficialSourceUrl);
    }

    internal static string BuildBody(
        DateOnly publicationDate,
        DateOnly latestObservedDate,
        int totalEvidenceCount,
        int totalOrganizationCount,
        int totalIngredientCount,
        IReadOnlyList<StateSummary> stateSummaries,
        IReadOnlyList<StateSummary> districtOrTerritorySummaries,
        StateSummary? unclassifiedSummary)
    {
        var stateEvidenceCount = stateSummaries.Sum(item => item.EvidenceCount);
        var districtOrTerritoryEvidenceCount =
            districtOrTerritorySummaries.Sum(item => item.EvidenceCount);
        var unclassifiedEvidenceCount = unclassifiedSummary?.EvidenceCount ?? 0;
        var visibleStates = stateSummaries.Take(10).ToArray();
        var hiddenStates = stateSummaries.Skip(10).ToArray();
        var lines = new List<string>
        {
            "[자동 작성 안내] 식약처 수입식품 표시·해외제조업소 공식 근거를 재료별 조사 원장에서 누적 집계했습니다.",
            $"게시 주기: {publicationDate:yyyy년 M월} 월간 누적",
            $"최근 관찰일: {latestObservedDate:yyyy-MM-dd}",
            $"현재 근거: 제품 근거 {totalEvidenceCount:N0}행 · 업체 후보 {totalOrganizationCount:N0}개 · 재료 {totalIngredientCount:N0}종",
            $"분류 상태: 미국 50개 주 {stateEvidenceCount:N0}행/{stateSummaries.Count:N0}개 주 · " +
            $"워싱턴 D.C.·미국령 {districtOrTerritoryEvidenceCount:N0}행 · " +
            $"기타·미분류 {unclassifiedEvidenceCount:N0}행",
            "단위: 현재 상태인 재료-업체-제품 근거 행 수",
            string.Empty,
            "제품 근거가 많은 주(최대 10개):"
        };

        foreach (var summary in visibleStates)
        {
            AddSummary(lines, summary);
        }

        if (hiddenStates.Length > 0)
        {
            lines.Add(
                $"- 그 밖의 분류 주 {hiddenStates.Length:N0}개: 제품 근거 " +
                $"{hiddenStates.Sum(item => item.EvidenceCount):N0}행");
        }

        if (districtOrTerritorySummaries.Count > 0)
        {
            lines.Add(string.Empty);
            lines.Add("워싱턴 D.C.·미국령 지역:");
            foreach (var summary in districtOrTerritorySummaries)
            {
                AddSummary(lines, summary);
            }
        }

        if (unclassifiedSummary is not null)
        {
            lines.Add(string.Empty);
            AddSummary(lines, unclassifiedSummary);
        }

        lines.AddRange(
        [
            string.Empty,
            "출처: 식품의약품안전처 수입식품 제품별 한글표시사항·해외제조업소 정보 및 공공데이터포털 수입식품 제품 목록",
            "주의: 이 집계는 수입량·수입액·거래 가능 업체 수가 아닙니다. 제조업소 소재 주는 원재료 생산·재배·어획 주나 법정 원산지를 대신하지 않습니다.",
            "제품 국가가 미국이어도 제조업소 주소가 미국 외 국가이거나 주 근거가 부족하면 기타·미분류로 보존합니다.",
            "업체 자동 추천·선정·연락에는 사용하지 않으며 실제 거래 전 최신 공식 상태를 다시 확인해야 합니다."
        ]);
        return string.Join(Environment.NewLine, lines);
    }

    private static StateSummary BuildSummary(
        string regionCode,
        IReadOnlyList<StateEvidence> items,
        DateOnly monthStart,
        DateOnly monthEnd,
        TimeZoneInfo timeZone)
    {
        var definition =
            UnitedStatesImportedFoodManufacturerRegionClassifier.FindByRegionCode(regionCode);
        var regionName = items.Select(item => item.RegionName)
            .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name))
            ?? definition?.KoreanName
            ?? "미국 기타·미분류";
        return new StateSummary(
            regionCode,
            regionName,
            items.Count,
            items.Select(item => item.OrganizationKey).Distinct(StringComparer.Ordinal).Count(),
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
                .Take(2)
                .ToArray());
    }

    private static void AddSummary(ICollection<string> lines, StateSummary summary)
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

    private static DateOnly LocalDate(DateTime utcDateTime, TimeZoneInfo timeZone)
    {
        var utc = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone));
    }

    private sealed record StateEvidence(
        string RegionCode,
        string RegionName,
        string OrganizationKey,
        long IngredientId,
        string IngredientName,
        DateTime FirstObservedAtUtc,
        DateTime LastObservedAtUtc,
        int ObservationCount);

    internal sealed record StateSummary(
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
