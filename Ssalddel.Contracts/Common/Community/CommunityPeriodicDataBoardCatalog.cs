using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Customs;

namespace Ssalddel.Contracts.Common.Community;

public sealed record CommunityPeriodicDataBoardDefinition(
    string BoardKey,
    string Provider,
    string RepresentativeTitle,
    string RepresentativeSummary,
    IReadOnlyList<string> SourceKeys,
    IReadOnlyList<string> PublicationSourceKeys);

/// <summary>
/// 주기성 공공데이터 글의 단일 저장 게시판과 관련 게시판 안내 link를 정의합니다.
/// 관련 게시판에는 글을 복제하지 않고 이 카탈로그에서 만든 대표 안내만 표시합니다.
/// </summary>
public static class CommunityPeriodicDataBoardCatalog
{
    public static IReadOnlyList<CommunityPeriodicDataBoardDefinition> All { get; } =
    [
        new(
            CommunityBoardKeys.PeriodicDataKamis,
            "한국농수산식품유통공사",
            "KAMIS 농수산물 가격 누적",
            "조사일·품목·등급·단위가 있는 국내 농수산물 관측값과 정기 요약을 봅니다.",
            [CommunityInformationSourceKeys.KamisPriceObservations],
            [CommunityBoardInformationPublicationSourceKeys.KamisPriceBrief]),
        new(
            CommunityBoardKeys.PeriodicDataMfds,
            "식품의약품안전처(MFDS)",
            "MFDS 수입식품 제조업소 근거",
            "수입식품 표시사항과 해외제조업소 근거를 중국 권역·미국 주별로 나눠 봅니다.",
            [
                CommunityBoardInformationSourceKeys.MfdsDomesticIngredientProducts,
                CommunityBoardInformationSourceKeys.MfdsImportedFoodLabels,
                CommunityBoardInformationSourceKeys.MfdsOverseasManufacturers
            ],
            [
                CommunityBoardInformationPublicationSourceKeys.ChinaImportedFoodRegionBrief,
                CommunityBoardInformationPublicationSourceKeys.UnitedStatesImportedFoodStateBrief
            ]),
        new(
            CommunityBoardKeys.PeriodicDataUsda,
            "USDA NASS",
            "USDA 미국 생산자가격 누적",
            "미국 전국 생산자 수취가격을 기준월과 원 단위 그대로 확인합니다.",
            [CommunityInformationSourceKeys.UsdaNassPriceObservations],
            [CommunityBoardInformationPublicationSourceKeys.UsdaNassPriceBrief]),
        new(
            CommunityBoardKeys.PeriodicDataCustomsImportUnitPrice,
            "관세청",
            "관세청 품목·국가별 수입단가",
            "수입금액과 순중량으로 계산한 CIF 참고단가를 세금·국내 물류비와 구분해 봅니다.",
            [Hs공공데이터출처Keys.수입평균단가],
            [])
    ];

    public static bool IsDataBoard(string? boardKeyOrName)
    {
        var board = CommunityBoardCatalog.Find(boardKeyOrName);
        return board is not null
               && All.Any(item => string.Equals(
                   item.BoardKey,
                   board.Key,
                   StringComparison.OrdinalIgnoreCase));
    }

    public static CommunityPeriodicDataBoardDefinition? FindByBoard(
        string? boardKeyOrName)
    {
        var board = CommunityBoardCatalog.Find(boardKeyOrName);
        return board is null
            ? null
            : All.FirstOrDefault(item => string.Equals(
                item.BoardKey,
                board.Key,
                StringComparison.OrdinalIgnoreCase));
    }

    public static string? CanonicalBoardKeyForSource(string? sourceKey)
        => string.IsNullOrWhiteSpace(sourceKey)
            ? null
            : All.FirstOrDefault(item => item.SourceKeys.Contains(
                sourceKey.Trim(),
                StringComparer.OrdinalIgnoreCase))?.BoardKey;

    public static string? CanonicalBoardKeyForPublicationSource(
        string? publicationSourceKey)
        => string.IsNullOrWhiteSpace(publicationSourceKey)
            ? null
            : All.FirstOrDefault(item => item.PublicationSourceKeys.Contains(
                publicationSourceKey.Trim(),
                StringComparer.OrdinalIgnoreCase))?.BoardKey;

    public static IReadOnlyList<CommunityPeriodicDataBoardDefinition> ForRelatedBoard(
        string? boardKeyOrName)
    {
        var relation = CommunityBoardInformationRelationCatalog.Find(boardKeyOrName);
        if (relation is null || IsDataBoard(relation.BoardKey))
        {
            return [];
        }

        var relatedSourceKeys = relation.Sources
            .Select(source => source.SourceKey)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return All
            .Where(item => item.SourceKeys.Any(relatedSourceKeys.Contains))
            .ToArray();
    }
}
