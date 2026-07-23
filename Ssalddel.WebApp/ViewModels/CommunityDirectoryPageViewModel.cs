using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.WebApp.ViewModels;

public sealed record CommunityDirectoryBoardGroup(
    string Key,
    string DisplayName,
    IReadOnlyList<CommunityBoardSummaryResponse> Boards);

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ui,
    SsalddelModuleKind.ClientFeature,
    "공개 게시판 분류의 조회·fallback·검색 상태를 관리",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "게시글 수는 중립적인 현황으로만 표시하며 상대 추천·순위 판단에 사용하지 않습니다.")]
public sealed class CommunityDirectoryPageViewModel(
    Func<CancellationToken, Task<IReadOnlyList<CommunityBoardSummaryResponse>>> loadBoardSummaries)
{
    private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(3);
    private IReadOnlyList<CommunityBoardSummaryResponse> _boardSummaries = [];

    public string SearchText { get; private set; } = string.Empty;
    public string? StatusMessage { get; private set; }
    public bool IsLoading { get; private set; } = true;
    public int TotalPostCount => _boardSummaries.Sum(board => board.PostCount);
    public int VisibleBoardCount => VisibleGroups.Sum(group => group.Boards.Count);

    public IReadOnlyList<CommunityDirectoryBoardGroup> VisibleGroups
        => _boardSummaries
            .Where(MatchesSearch)
            .GroupBy(board => board.GroupCode)
            .Select(group => new CommunityDirectoryBoardGroup(
                group.Key,
                group.First().GroupDisplayName,
                group.ToArray()))
            .ToArray();

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        StatusMessage = null;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(LoadTimeout);
        try
        {
            var serverBoards = await loadBoardSummaries(timeout.Token);
            _boardSummaries = MergeWithCoreBoards(serverBoards);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _boardSummaries = MergeWithCoreBoards([]);
            StatusMessage = "게시글 수 조회가 지연되고 있습니다. 기본 게시판을 먼저 표시합니다.";
        }
        catch (Exception)
        {
            _boardSummaries = MergeWithCoreBoards([]);
            StatusMessage = "게시글 수를 불러오지 못했습니다. 기본 게시판을 먼저 표시합니다.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void UpdateSearch(string? value)
        => SearchText = value?.TrimStart() ?? string.Empty;

    private static IReadOnlyList<CommunityBoardSummaryResponse> MergeWithCoreBoards(
        IReadOnlyList<CommunityBoardSummaryResponse> serverBoards)
    {
        var byKey = serverBoards
            .Where(board => !string.IsNullOrWhiteSpace(board.BoardKey))
            .GroupBy(board => board.BoardKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
        var merged = CommunityBoardCatalog.PublicBoards
            .Select(board => byKey.TryGetValue(board.Key, out var serverBoard)
                ? serverBoard
                : new CommunityBoardSummaryResponse
                {
                    BoardKey = board.Key,
                    DisplayName = board.DisplayName,
                    Description = board.Description,
                    GroupCode = board.GroupCode,
                    GroupDisplayName = board.GroupDisplayName,
                    IsUserCreatable = board.IsUserCreatable,
                    PostingAccessCode = board.PostingAccessCode,
                    PostingAccessDisplayName = board.PostingAccessDisplayName,
                    AllowsAnonymousPosting = board.AllowsAnonymousPosting
                })
            .ToList();
        merged.AddRange(serverBoards
            .Where(board => board.IsCustom)
            .Where(board => !merged.Any(existing => string.Equals(
                existing.BoardKey,
                board.BoardKey,
                StringComparison.OrdinalIgnoreCase))));
        return merged;
    }

    private bool MatchesSearch(CommunityBoardSummaryResponse board)
    {
        var search = SearchText.Trim();
        var activityBundle = CommunityActivityBoardCatalog.FindBundle(board.BoardKey);
        return search.Length == 0
               || board.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)
               || board.Description.Contains(search, StringComparison.OrdinalIgnoreCase)
               || board.GroupDisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)
               || activityBundle is not null
               && (activityBundle.ProductVersion.Contains(search, StringComparison.OrdinalIgnoreCase)
                   || activityBundle.RoadmapStage.DisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)
                   || activityBundle.Activities.Any(activity =>
                       activity.SourceName.Contains(search, StringComparison.OrdinalIgnoreCase)
                       || activity.ActivityDisplayName.Contains(search, StringComparison.OrdinalIgnoreCase)
                       || activity.SourceKindDisplayName.Contains(search, StringComparison.OrdinalIgnoreCase))
                   || activityBundle.Pages.Any(page =>
                       page.Surface.Contains(search, StringComparison.OrdinalIgnoreCase)
                       || page.PageName.Contains(search, StringComparison.OrdinalIgnoreCase)
                       || page.Route.Contains(search, StringComparison.OrdinalIgnoreCase)
                       || page.Responsibility.Contains(search, StringComparison.OrdinalIgnoreCase)));
    }
}
