namespace Ssalddel.Contracts.Common.Community;

/// <summary>
/// 게시글의 비구속적 관심 모집은 공동구매 모집 게시판에서 작성자가 명시적으로 선택한 경우에만 허용합니다.
/// </summary>
public static class CommunityPostInterestGatheringPolicy
{
    public static bool IsGroupPurchaseCategory(string? category)
        => CommunityBoardCatalog.Find(category)?.Key == CommunityBoardKeys.Participation;

    public static bool ResolveEnabled(string? category, bool requested)
        => requested && IsGroupPurchaseCategory(category);

    public static bool IsEnabledFor(string? category, bool enabled)
        => enabled && IsGroupPurchaseCategory(category);
}
