using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

public sealed record CommunityBoardPageRequest(
    string? BoardKey,
    string? LegacyBoardName,
    string? WorkflowTag,
    string? RoleTag,
    int Page,
    string? SearchText = null,
    string? ListFilter = null,
    string? ViewMode = null);

public sealed record CommunityBoardPostQuery(
    string? BoardKey,
    string? LegacyCategory,
    string? WorkflowTag,
    string? RoleTag,
    string? PeriodicVisibility,
    int Page,
    int PageSize);

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ui,
    SsalddelModuleKind.ClientFeature,
    "Web과 모바일에서 선택한 공개 게시판 문맥과 글 목록의 조회·검색·필터 상태를 관리",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "게시글은 사용자가 선택한 게시판과 명시적 필터로만 좁히며 상대 적합성·성사 가능성으로 순위화하지 않습니다.")]
public sealed class CommunityBoardPageViewModel(
    Func<CancellationToken, Task<IReadOnlyList<CommunityBoardSummaryResponse>>> loadBoardSummaries,
    Func<CommunityBoardPostQuery, CancellationToken, Task<PlatformCommunityPostListResponse>> loadPosts)
{
    private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(5);

    public IReadOnlyList<CommunityBoardSummaryResponse> BoardSummaries { get; private set; } = [];
    public CommunityBoardSummaryResponse? CurrentBoard { get; private set; }
    public PlatformCommunityPostListResponse Result { get; private set; } = new();
    public string SelectedFilter { get; private set; } = "전체글";
    public string SearchText { get; private set; } = string.Empty;
    public CommunityPostViewMode ViewMode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool IsLoading { get; private set; }

    public string CurrentBoardName => CurrentBoard?.DisplayName ?? "전체 글";
    public string CurrentBoardDescription => CurrentBoard?.Description ?? "공개 게시판의 글을 한 번에 봅니다.";
    public bool CanCompose => CurrentBoard?.IsUserCreatable != false;
    public int TotalPages => Math.Max(
        1,
        (int)Math.Ceiling(Result.TotalCount / (double)Math.Max(1, Result.PageSize)));

    public IReadOnlyList<PlatformCommunityPostResponse> VisiblePosts
        => Result.Items
            .Where(MatchesListFilter)
            .Where(MatchesSearch)
            .ToArray();

    public async Task LoadAsync(
        CommunityBoardPageRequest request,
        CancellationToken cancellationToken = default)
    {
        RestoreListState(request.SearchText, request.ListFilter, request.ViewMode);
        IsLoading = true;
        ErrorMessage = null;
        BoardSummaries = MergeWithCoreBoards([]);
        CurrentBoard = ResolveCurrentBoard(BoardSummaries, request);
        NormalizePeriodicFilterForCurrentBoard();

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(LoadTimeout);
        try
        {
            var serverBoards = await loadBoardSummaries(timeout.Token);
            BoardSummaries = MergeWithCoreBoards(serverBoards);
            CurrentBoard = ResolveCurrentBoard(BoardSummaries, request);
            NormalizePeriodicFilterForCurrentBoard();

            var resolvedKey = CurrentBoard?.BoardKey
                              ?? CommunityBoardCatalog.Find(request.BoardKey ?? request.LegacyBoardName)?.Key;
            var legacyCategory = resolvedKey is null && !string.IsNullOrWhiteSpace(request.LegacyBoardName)
                ? request.LegacyBoardName
                : null;
            Result = await loadPosts(
                new CommunityBoardPostQuery(
                    resolvedKey,
                    legacyCategory,
                    request.WorkflowTag,
                    request.RoleTag,
                    CommunityPeriodicPostTopicCatalog.SupportsBoard(resolvedKey)
                        ? CommunityPeriodicPostTopicCatalog.VisibilityFor(SelectedFilter)
                        : CommunityPeriodicPostVisibilityModes.All,
                    CommunityBoardNavigationContext.NormalizePage(request.Page),
                    50),
                timeout.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            Result = new PlatformCommunityPostListResponse();
            ErrorMessage = "응답이 지연되고 있습니다. 네트워크 상태를 확인한 뒤 다시 시도해 주세요.";
        }
        catch (Exception)
        {
            Result = new PlatformCommunityPostListResponse();
            ErrorMessage = "네트워크 상태를 확인한 뒤 다시 시도해 주세요.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void RestoreListState(string? searchText, string? listFilter, string? viewMode)
    {
        SearchText = CommunityBoardNavigationContext.NormalizeSearch(searchText);
        SelectedFilter = CommunityBoardNavigationContext.NormalizeFilter(listFilter);
        ViewMode = CommunityBoardNavigationContext.NormalizeViewMode(viewMode)
                   == CommunityBoardNavigationContext.CardViewMode
            ? CommunityPostViewMode.Cards
            : CommunityPostViewMode.List;
    }

    public void SelectListFilter(string? value)
        => SelectedFilter = CommunityBoardNavigationContext.NormalizeFilter(value);

    public void SelectViewMode(CommunityPostViewMode value) => ViewMode = value;

    public void SetSearchText(string? value)
        => SearchText = CommunityBoardNavigationContext.NormalizeSearch(value);

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

    private static CommunityBoardSummaryResponse? ResolveCurrentBoard(
        IReadOnlyList<CommunityBoardSummaryResponse> boards,
        CommunityBoardPageRequest request)
    {
        var requested = request.BoardKey ?? request.LegacyBoardName;
        if (string.IsNullOrWhiteSpace(requested))
        {
            return null;
        }

        var catalogBoard = CommunityBoardCatalog.Find(requested);
        var key = catalogBoard?.Key ?? requested.Trim();
        return boards.FirstOrDefault(board =>
                   string.Equals(board.BoardKey, key, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(board.DisplayName, requested, StringComparison.OrdinalIgnoreCase))
               ?? (catalogBoard is null
                   ? null
                   : new CommunityBoardSummaryResponse
                   {
                       BoardKey = catalogBoard.Key,
                       DisplayName = catalogBoard.DisplayName,
                       Description = catalogBoard.Description,
                       GroupCode = catalogBoard.GroupCode,
                       GroupDisplayName = catalogBoard.GroupDisplayName,
                       IsUserCreatable = catalogBoard.IsUserCreatable,
                       PostingAccessCode = catalogBoard.PostingAccessCode,
                       PostingAccessDisplayName = catalogBoard.PostingAccessDisplayName,
                       AllowsAnonymousPosting = catalogBoard.AllowsAnonymousPosting
                   });
    }

    private bool MatchesListFilter(PlatformCommunityPostResponse post)
        => SelectedFilter switch
        {
            CommunityPeriodicPostTopicCatalog.GeneralListFilter => !post.IsPeriodic,
            CommunityPeriodicPostTopicCatalog.PeriodicListFilter => post.IsPeriodic,
            "공지" => post.IsOperatorPinned,
            "추천글" => post.IsTrending || post.RecommendationCount >= 5,
            _ => true
        };

    private void NormalizePeriodicFilterForCurrentBoard()
    {
        if (CommunityPeriodicPostTopicCatalog.IsTopicFilter(SelectedFilter)
            && !CommunityPeriodicPostTopicCatalog.SupportsBoard(CurrentBoard?.BoardKey))
        {
            SelectedFilter = "전체글";
        }
    }

    private bool MatchesSearch(PlatformCommunityPostResponse post)
    {
        var search = SearchText.Trim();
        return search.Length == 0
               || Contains(post.Title, search)
               || Contains(post.Body, search)
               || Contains(post.Nickname, search)
               || Contains(post.WorkflowTag, search)
               || Contains(post.RoleTag, search);
    }

    private static bool Contains(string? value, string search)
        => !string.IsNullOrWhiteSpace(value)
           && value.Contains(search, StringComparison.OrdinalIgnoreCase);
}
