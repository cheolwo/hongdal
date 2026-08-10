using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using 살뜰.Services.External.PublicData;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface I공통식품품목기존Data대조UseCase
{
    Task<공통식품품목기존Data대조Response> PreviewAsync(
        int year,
        CancellationToken cancellationToken = default);

    Task<공통식품품목기존Data승격Response> PromoteCandidatesAsync(
        int year,
        string expectedPreviewHash,
        string confirmedBySubjectId,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommonFoodProductIdentity,
    SsalddelCodeLayer.Application,
    "기존 KAMIS 관측을 HS·USDA AMS 후보와 대조하고 canonical 연결 가능 여부를 read-only Preview로 분류한다.",
    ContractType = typeof(I공통식품품목기존Data대조UseCase),
    FlowOrder = 25,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "Preview는 상품 identity나 code relation을 생성·승격하지 않으며 농사로 코드를 이름으로 추정하지 않는다.")]
public sealed class 공통식품품목기존Data대조UseCase(
    AgriculturalFisheriesDbContext db,
    IFoodPriceCrosswalkCatalog crosswalkCatalog)
    : I공통식품품목기존Data대조UseCase
{
    public async Task<공통식품품목기존Data대조Response> PreviewAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        if (year is < 1990 or > 2100)
        {
            throw new ArgumentOutOfRangeException(nameof(year));
        }

        var yearFrom = new DateOnly(year, 1, 1);
        var yearToExclusive = yearFrom.AddYears(1);
        var kamisRows = await db.KamisPriceObservations
            .AsNoTracking()
            .Where(item =>
                item.SurveyDate >= yearFrom
                && item.SurveyDate < yearToExclusive
                && item.CategoryCode != string.Empty
                && item.ItemCode != string.Empty)
            .GroupBy(item => new
            {
                item.CategoryCode,
                item.CategoryName,
                item.ItemCode,
                item.ItemName
            })
            .Select(group => new
            {
                group.Key.CategoryCode,
                group.Key.CategoryName,
                group.Key.ItemCode,
                group.Key.ItemName,
                LatestSurveyDate = group.Max(item => item.SurveyDate)
            })
            .ToArrayAsync(cancellationToken);
        var availableAmsCommodities = await db.UsdaAmsYearCommodityCatalog
            .AsNoTracking()
            .Where(item => item.Year == year)
            .Select(item => item.Commodity)
            .ToArrayAsync(cancellationToken);

        var crosswalks = crosswalkCatalog.GetAll();
        var canonicalByKamis = await BuildCanonicalByKamisAsync(cancellationToken);
        var conflictingKamisKeys = kamisRows
            .GroupBy(item => (item.CategoryCode, item.ItemCode))
            .Where(group => group.Select(item => item.ItemName).Distinct().Count() > 1)
            .Select(group => group.Key)
            .ToHashSet();
        var items = kamisRows
            .OrderBy(item => item.CategoryCode, StringComparer.Ordinal)
            .ThenBy(item => item.ItemCode, StringComparer.Ordinal)
            .ThenBy(item => item.ItemName, StringComparer.Ordinal)
            .Select(item =>
            {
                var key = (item.CategoryCode, item.ItemCode);
                var hasConflict = conflictingKamisKeys.Contains(key);
                var hsCandidates = crosswalks
                    .Where(crosswalk =>
                        crosswalk.AtCategoryCode == item.CategoryCode
                        && crosswalk.AtItemCode == item.ItemCode)
                    .Select(crosswalk => crosswalk.HsPrefix)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(code => code, StringComparer.Ordinal)
                    .ToArray();
                var ams = Kamis중심UsdaAms품목MappingCatalog.Resolve(
                    item.ItemCode,
                    availableAmsCommodities);
                canonicalByKamis.TryGetValue(key, out var canonicalStableId);
                if (hasConflict)
                {
                    canonicalStableId = null;
                }

                var statusCode = hasConflict
                    ? 공통식품품목대조StatusCodes.SourceConflict
                    : canonicalStableId is not null
                        ? 공통식품품목대조StatusCodes.CanonicalLinked
                        : hsCandidates.Length > 0 || ams.MatchedCommodities.Count > 0
                            ? 공통식품품목대조StatusCodes.CandidateOnly
                            : 공통식품품목대조StatusCodes.Unmapped;
                var reviewNote = hasConflict
                    ? "같은 KAMIS 분류·품목코드에 여러 품목명이 있어 자동 연결하지 않습니다."
                    : canonicalStableId is not null
                        ? "기존 canonical 상품의 Confirmed KAMIS relation과 일치합니다."
                        : "내부 canonical 상품이 없으므로 HS·AMS 후보를 검토한 뒤 별도 상품 identity를 승인해야 합니다.";
                return new 공통식품품목기존Data대조항목Response(
                    statusCode,
                    canonicalStableId,
                    item.CategoryCode,
                    item.CategoryName,
                    item.ItemCode,
                    item.ItemName,
                    item.LatestSurveyDate,
                    hsCandidates,
                    ams.MatchedCommodities,
                    공통식품품목관계StatusCodes.Unlinked,
                    reviewNote);
            })
            .ToArray();

        var previewHash = ComputePreviewHash(year, items);
        return new 공통식품품목기존Data대조Response(
            year,
            previewHash,
            items.Length,
            items.Count(item => item.StatusCode == 공통식품품목대조StatusCodes.CanonicalLinked),
            items.Count(item => item.StatusCode == 공통식품품목대조StatusCodes.CandidateOnly),
            items.Count(item => item.StatusCode == 공통식품품목대조StatusCodes.Unmapped),
            items.Count(item => item.StatusCode == 공통식품품목대조StatusCodes.SourceConflict),
            items,
            [
                "KAMIS 관측의 분류·품목코드를 기준축으로 정렬하되 KAMIS가 canonical 상품 authority가 되지는 않습니다.",
                "HS와 USDA AMS는 검토 후보이며 이 Preview는 DB code relation을 생성하거나 Confirmed로 승격하지 않습니다.",
                "농사로 공식 품목구분Code는 현재 crosswalk 근거가 없어 모든 항목을 Unlinked로 유지합니다."
            ]);
    }

    public async Task<공통식품품목기존Data승격Response> PromoteCandidatesAsync(
        int year,
        string expectedPreviewHash,
        string confirmedBySubjectId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(expectedPreviewHash))
        {
            throw new ArgumentException("PreviewHashRequired", nameof(expectedPreviewHash));
        }

        if (string.IsNullOrWhiteSpace(confirmedBySubjectId))
        {
            throw new ArgumentException("ConfirmedBySubjectIdRequired", nameof(confirmedBySubjectId));
        }

        var preview = await PreviewAsync(year, cancellationToken);
        if (!string.Equals(preview.PreviewHash, expectedPreviewHash, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("CommonFoodProductPreviewChanged");
        }

        var candidates = preview.Items
            .Where(item => item.StatusCode == 공통식품품목대조StatusCodes.CandidateOnly)
            .ToArray();
        var existingKamisKeys = await db.CommonFoodProductCodeRelations
            .AsNoTracking()
            .Where(item =>
                item.CodeScheme == 공통식품품목CodeSchemes.KamisItem
                && item.RelationStatusCode == 공통식품품목관계StatusCodes.Confirmed
                && item.ExternalCode != null
                && item.ParentCode != null)
            .Select(item => new { item.ParentCode, item.ExternalCode })
            .ToArrayAsync(cancellationToken);
        var existing = existingKamisKeys
            .Select(item => (item.ParentCode!, item.ExternalCode!))
            .ToHashSet();
        var promoted = new List<string>();
        var createdRelationCount = 0;
        var now = DateTime.UtcNow;

        foreach (var candidate in candidates)
        {
            if (existing.Contains((candidate.KamisCategoryCode, candidate.KamisItemCode)))
            {
                continue;
            }

            var stableId = CreateCanonicalStableId(
                candidate.KamisCategoryCode,
                candidate.KamisItemCode);
            var identity = new Ssalddel.Domain.AgriculturalFisheries.공통식품품목Identity
            {
                CanonicalProductStableId = stableId,
                DisplayName = candidate.KamisItemName,
                Revision = "common-food-product-identity.v1",
                IsActive = true,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            AddRelation(identity, "kamis", 공통식품품목CodeSchemes.KamisItem,
                candidate.KamisItemCode, candidate.KamisCategoryCode, candidate.KamisItemName,
                공통식품품목관계StatusCodes.Confirmed, "SourceCodeConfirmed",
                "KAMIS 관측의 분류·품목코드를 명시적 승인으로 canonical 상품에 연결했습니다.",
                confirmedBySubjectId, now);
            foreach (var hs in candidate.HsCandidates)
            {
                AddRelation(identity, "wco-hs", 공통식품품목CodeSchemes.Hs4,
                    hs, null, candidate.KamisItemName,
                    공통식품품목관계StatusCodes.Candidate, "CrosswalkCandidate",
                    "기존 FoodPrice crosswalk에서 찾은 HS 후보이며 세부 세번과 용도는 별도 검토가 필요합니다.",
                    confirmedBySubjectId, now);
            }

            foreach (var commodity in candidate.UsdaAmsCommodityCandidates)
            {
                AddRelation(identity, "usda-ams-market-news", 공통식품품목CodeSchemes.UsdaAmsCommodity,
                    commodity, null, commodity,
                    공통식품품목관계StatusCodes.Candidate, "CommodityNameCandidate",
                    "해당 연도 USDA AMS Commodity catalog의 품목명 후보이며 등급·시장 단계는 별도 검토가 필요합니다.",
                    confirmedBySubjectId, now);
            }

            AddRelation(identity, "nongsaro:farm-working-plan-new",
                공통식품품목CodeSchemes.NongsaroKindOfCommodity,
                null, null, $"농사로 {candidate.KamisItemName} 품목구분",
                공통식품품목관계StatusCodes.Unlinked, "OfficialCodeRequired",
                "농사로 공식 품목구분Code crosswalk를 확인하기 전에는 이름으로 연결하지 않습니다.",
                confirmedBySubjectId, now);
            createdRelationCount += identity.CodeRelations.Count;
            promoted.Add(stableId);
            db.CommonFoodProductIdentities.Add(identity);
        }

        await db.SaveChangesAsync(cancellationToken);
        return new 공통식품품목기존Data승격Response(
            year,
            preview.PreviewHash,
            promoted.Count,
            createdRelationCount,
            preview.CanonicalLinkedCount,
            promoted,
            [
                "KAMIS relation만 Confirmed이며 HS·USDA AMS 후보는 Candidate로 유지됩니다.",
                "농사로 relation은 공식 품목구분Code crosswalk가 확인될 때까지 Unlinked입니다.",
                "canonical stable ID는 Ssalddel 내부 식별자이며 외부 source code나 Unity prefab 이름에 종속되지 않습니다."
            ]);
    }

    private async Task<IReadOnlyDictionary<(string CategoryCode, string ItemCode), string>>
        BuildCanonicalByKamisAsync(CancellationToken cancellationToken)
    {
        var rows = await db.CommonFoodProductCodeRelations
            .AsNoTracking()
            .Where(relation =>
                relation.CodeScheme == 공통식품품목CodeSchemes.KamisItem
                && relation.RelationStatusCode == 공통식품품목관계StatusCodes.Confirmed
                && relation.ExternalCode != null
                && relation.ParentCode != null
                && relation.IsActive)
            .Select(relation => new
            {
                relation.ProductIdentity!.CanonicalProductStableId,
                CategoryCode = relation.ParentCode!,
                ItemCode = relation.ExternalCode!
            })
            .ToArrayAsync(cancellationToken);
        return rows.ToDictionary(
            item => (item.CategoryCode, item.ItemCode),
            item => item.CanonicalProductStableId);
    }

    private static string ComputePreviewHash(
        int year,
        IReadOnlyList<공통식품품목기존Data대조항목Response> items)
    {
        var source = new StringBuilder().Append(year).Append('\n');
        foreach (var item in items)
        {
            source.Append(item.StatusCode).Append('|')
                .Append(item.KamisCategoryCode).Append('|')
                .Append(item.KamisItemCode).Append('|')
                .Append(item.KamisItemName).Append('|')
                .AppendJoin(',', item.HsCandidates).Append('|')
                .AppendJoin(',', item.UsdaAmsCommodityCandidates).Append('\n');
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source.ToString())))
            .ToLowerInvariant();
    }

    private static string CreateCanonicalStableId(string categoryCode, string itemCode)
        => $"product:food:{categoryCode}:{itemCode}";

    private static void AddRelation(
        Ssalddel.Domain.AgriculturalFisheries.공통식품품목Identity identity,
        string sourceKey,
        string codeScheme,
        string? externalCode,
        string? parentCode,
        string label,
        string statusCode,
        string matchQualityCode,
        string evidenceNote,
        string confirmedBySubjectId,
        DateTime now)
    {
        var relationKey = $"{sourceKey}|{codeScheme}|{parentCode}|{externalCode}|{label}";
        var suffix = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(relationKey)))
            .ToLowerInvariant()[..16];
        var relation = new Ssalddel.Domain.AgriculturalFisheries.공통식품품목Code관계
        {
            RelationStableId = $"relation:{identity.CanonicalProductStableId}:{suffix}",
            SourceKey = sourceKey,
            CodeScheme = codeScheme,
            ExternalCode = externalCode,
            ParentCode = parentCode,
            Label = label,
            RelationStatusCode = statusCode,
            MatchQualityCode = matchQualityCode,
            EvidenceNote = evidenceNote,
            Revision = 1,
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        relation.ReviewHistory.Add(new Ssalddel.Domain.AgriculturalFisheries.공통식품품목Code관계검토이력
        {
            Revision = 1,
            RelationStatusCode = statusCode,
            ExternalCode = externalCode,
            ReviewActionCode = "ConfirmedCatalogExpansion",
            ReviewReason = evidenceNote,
            ReviewedBySubjectId = confirmedBySubjectId,
            ReviewedAtUtc = now
        });
        identity.CodeRelations.Add(relation);
    }
}
