namespace Ssalddel.Contracts.Common.Community;

public sealed record CommunityWorkBoardEditorialPlan(
    string BoardKey,
    IReadOnlyList<string> Topics,
    IReadOnlyList<string> ExecutableSourceKeys,
    IReadOnlyList<string> PlannedOfficialSources,
    string Cadence,
    bool RequiresEditorialReview);

/// <summary>
/// 정보 수집·편집 배치를 버전이 아니라 업무 게시판 key에 연결하는 기준입니다.
/// PlannedOfficialSources는 connector 구현 전 운영 후보이며 자동 fallback으로 사용하지 않습니다.
/// </summary>
public static class CommunityWorkBoardEditorialPlanCatalog
{
    public static IReadOnlyList<CommunityWorkBoardEditorialPlan> All { get; } =
        CommunityActivityBoardCatalog.Boards
            .Select(board => CommunityBoardInformationRelationCatalog.Find(board.Key)
                             ?? throw new InvalidOperationException(
                                 $"업무 게시판 정보 관계를 찾을 수 없습니다. BoardKey={board.Key}"))
            .Select(relation => new CommunityWorkBoardEditorialPlan(
                relation.BoardKey,
                relation.Topics,
                relation.Sources
                    .Where(source => source.IsConnectorImplemented)
                    .Select(source => source.SourceKey)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                relation.Sources
                    .Where(source => !source.IsConnectorImplemented)
                    .Select(source => source.DisplayName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                relation.PreferredCadence,
                RequiresEditorialReview: relation.Sources.Any(source =>
                    source.PublicationPolicy is
                        CommunityBoardInformationPublicationPolicies.EditorialReview
                        or CommunityBoardInformationPublicationPolicies.NoAutomaticPublication)))
            .ToArray();

    public static CommunityWorkBoardEditorialPlan Find(string boardKey)
        => All.First(plan => string.Equals(plan.BoardKey, boardKey, StringComparison.OrdinalIgnoreCase));
}
