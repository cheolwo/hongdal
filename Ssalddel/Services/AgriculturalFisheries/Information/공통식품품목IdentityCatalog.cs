using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.PublicData;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface I공통식품품목Identity조회UseCase
{
    Task<공통식품품목IdentityListResponse> 목록조회Async(
        CancellationToken cancellationToken = default);

    Task<공통식품품목IdentityResponse?> 단건조회Async(
        string canonicalProductStableId,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommonFoodProductIdentity,
    SsalddelCodeLayer.Application,
    "공통 상품 stable ID 아래에 출처별 품목코드와 검토 상태를 보존한 read-only 관계를 제공한다.",
    ContractType = typeof(I공통식품품목Identity조회UseCase),
    FlowOrder = 20,
    Effects = SsalddelCodeEffect.PersistentRead,
    Boundary = "출처별 코드를 동일 코드로 간주하지 않으며 Candidate와 Unlinked 관계를 운영 확정값으로 사용하지 않는다.")]
public sealed class 공통식품품목Identity조회UseCase(
    AgriculturalFisheriesDbContext db) : I공통식품품목Identity조회UseCase
{
    public async Task<공통식품품목IdentityListResponse> 목록조회Async(
        CancellationToken cancellationToken = default)
    {
        var items = await Query()
            .OrderBy(item => item.CanonicalProductStableId)
            .ToArrayAsync(cancellationToken);
        return new(
            공통식품품목IdentityCatalog.Revision,
            items.Select(Map).ToArray());
    }

    public async Task<공통식품품목IdentityResponse?> 단건조회Async(
        string canonicalProductStableId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(canonicalProductStableId))
        {
            return null;
        }

        var stableId = canonicalProductStableId.Trim();
        var item = await Query().SingleOrDefaultAsync(
            candidate => candidate.CanonicalProductStableId == stableId,
            cancellationToken);
        return item is null ? null : Map(item);
    }

    private IQueryable<Ssalddel.Domain.AgriculturalFisheries.공통식품품목Identity> Query()
        => db.CommonFoodProductIdentities
            .AsNoTracking()
            .AsSplitQuery()
            .Where(item => item.IsActive)
            .Include(item => item.CodeRelations.Where(relation => relation.IsActive))
            .ThenInclude(relation => relation.ReviewHistory);

    private static 공통식품품목IdentityResponse Map(
        Ssalddel.Domain.AgriculturalFisheries.공통식품품목Identity item)
        => new(
            item.CanonicalProductStableId,
            item.DisplayName,
            item.Revision,
            item.CodeRelations
                .OrderBy(relation => relation.CodeScheme, StringComparer.Ordinal)
                .ThenBy(relation => relation.SourceKey, StringComparer.Ordinal)
                .Select(relation => new 공통식품품목Code관계Response(
                    relation.SourceKey,
                    relation.CodeScheme,
                    relation.ExternalCode,
                    relation.ParentCode,
                    relation.Label,
                    relation.RelationStatusCode,
                    relation.MatchQualityCode,
                    relation.EvidenceNote,
                    relation.Revision,
                    relation.ReviewHistory
                        .OrderBy(history => history.Revision)
                        .Select(history => new 공통식품품목Code관계검토이력Response(
                            history.Revision,
                            history.RelationStatusCode,
                            history.ExternalCode,
                            history.ReviewActionCode,
                            history.ReviewReason,
                            history.ReviewedAtUtc))
                        .ToArray()))
                .ToArray(),
            공통식품품목IdentityCatalog.GetLimitations(item.CanonicalProductStableId));
}

public static class 공통식품품목IdentityCatalog
{
    public const string Revision = "common-food-product-identity.v1";
    public const string 감자ProductStableId = "product:potato";
    public const string 감자KamisCategoryCode = "100";
    public const string 감자KamisItemCode = "152";
    public const string 감자Hs4 = "0701";
    public const string 감자UsdaAmsCommodity = "Potatoes";

    private static readonly DateTime InitialReviewedAtUtc =
        new(2026, 8, 10, 0, 0, 0, DateTimeKind.Utc);

    private static readonly IReadOnlyList<string> CommonLimitations =
    [
        "CanonicalProductStableId는 서버 내부 식별자이며 외부 기관의 공식 코드가 아닙니다.",
        "Candidate 관계는 표시·검색 후보일 뿐 세관 신고, 가격 직접 비교 또는 운영 상태 확정에 사용할 수 없습니다.",
        "농사로 관계는 공식 품목구분Code와 출처 근거를 확인한 뒤 별도 revision에서 연결해야 합니다."
    ];

    private static readonly IReadOnlyList<공통식품품목IdentityResponse> Items =
    [
        new(
            감자ProductStableId,
            "감자",
            Revision,
            [
                new(
                    "kamis",
                    공통식품품목CodeSchemes.KamisItem,
                    감자KamisItemCode,
                    감자KamisCategoryCode,
                    "감자",
                    공통식품품목관계StatusCodes.Confirmed,
                    "SourceCodeConfirmed",
                    "KAMIS 식량작물 100의 품목코드 152로 저장·조회되는 관계입니다.",
                    1,
                    [InitialHistory(공통식품품목관계StatusCodes.Confirmed, 감자KamisItemCode, "공식 source code 초기 등록")]),
                new(
                    "wco-hs",
                    공통식품품목CodeSchemes.Hs4,
                    감자Hs4,
                    null,
                    "감자",
                    공통식품품목관계StatusCodes.Candidate,
                    "ExactCommodityCandidate",
                    "국제 HS 4단위 후보이며 종자용 여부·가공도·용도에 따라 국가 세번이 달라질 수 있습니다.",
                    1,
                    [InitialHistory(공통식품품목관계StatusCodes.Candidate, 감자Hs4, "국제 HS 후보 초기 등록")]),
                new(
                    "usda-ams-market-news",
                    공통식품품목CodeSchemes.UsdaAmsCommodity,
                    감자UsdaAmsCommodity,
                    null,
                    "Potatoes",
                    공통식품품목관계StatusCodes.Candidate,
                    "DirectCommodityCandidate",
                    "식용 감자 공통 품목 후보이며 종서용 감자와 품종·등급·시장 단계는 별도로 검토합니다.",
                    1,
                    [InitialHistory(공통식품품목관계StatusCodes.Candidate, 감자UsdaAmsCommodity, "USDA AMS Commodity 후보 초기 등록")]),
                new(
                    "nongsaro:farm-working-plan-new",
                    공통식품품목CodeSchemes.NongsaroKindOfCommodity,
                    null,
                    null,
                    "농사로 감자 품목구분",
                    공통식품품목관계StatusCodes.Unlinked,
                    "OfficialCodeRequired",
                    "농사로 공식 품목구분Code를 현재 근거에서 확인하지 못해 이름으로 연결하지 않습니다.",
                    1,
                    [InitialHistory(공통식품품목관계StatusCodes.Unlinked, null, "공식 농사로 품목구분Code 확인 전 미연결 등록")])
            ],
            CommonLimitations)
    ];

    private static readonly IReadOnlyDictionary<string, 공통식품품목IdentityResponse> ByStableId =
        Items.ToDictionary(
            item => item.CanonicalProductStableId,
            StringComparer.Ordinal);

    public static IReadOnlyList<공통식품품목IdentityResponse> GetAll()
        => Items;

    public static 공통식품품목IdentityResponse? Find(string? canonicalProductStableId)
    {
        if (string.IsNullOrWhiteSpace(canonicalProductStableId))
        {
            return null;
        }

        return ByStableId.GetValueOrDefault(canonicalProductStableId.Trim());
    }

    public static 공통식품품목IdentityResponse GetRequired(string canonicalProductStableId)
        => Find(canonicalProductStableId)
           ?? throw new InvalidOperationException(
               $"공통 식품 품목 identity를 찾을 수 없습니다: {canonicalProductStableId}");

    public static IReadOnlyList<string> GetLimitations(string canonicalProductStableId)
        => Find(canonicalProductStableId)?.Limitations ?? CommonLimitations;

    private static 공통식품품목Code관계검토이력Response InitialHistory(
        string statusCode,
        string? externalCode,
        string reason)
        => new(
            1,
            statusCode,
            externalCode,
            "Initialized",
            reason,
            InitialReviewedAtUtc);
}
