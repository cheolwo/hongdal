using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Ui.Common.Areas.App.Services;

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
public sealed class CommunityBoardPageViewModel : PageViewModelBase
{
    private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(5);
    private readonly Func<string, CancellationToken, Task<IReadOnlyList<CommunityBoardSummaryResponse>>> loadBoardSummaries;
    private readonly Func<string, CommunityBoardPostQuery, CancellationToken, Task<PlatformCommunityPostListResponse>> loadPosts;
    private readonly Func<long, string?, CancellationToken, Task>? deletePost;
    private IReadOnlyList<CommunityBoardSummaryResponse> boardSummaries = [];
    private CommunityBoardSummaryResponse? currentBoard;
    private PlatformCommunityPostListResponse result = new();
    private string selectedFilter = "전체글";
    private string searchText = string.Empty;
    private CommunityPostViewMode viewMode;
    private CommunityBoardPageRequest request = new(null, null, null, null, 1);
    private string appKey = "shipper";

    public CommunityBoardPageViewModel(ICommunityPostClient communityPostClient)
        : this(
            (key, cancellationToken) =>
                communityPostClient.GetBoardSummariesAsync(key, cancellationToken),
            (key, query, cancellationToken) =>
                communityPostClient.GetBoardPostsAsync(
                    key,
                    boardKey: query.BoardKey,
                    category: query.LegacyCategory,
                    workflowTag: query.WorkflowTag,
                    roleTag: query.RoleTag,
                    page: query.Page,
                    pageSize: query.PageSize,
                    periodicVisibility: query.PeriodicVisibility,
                    cancellationToken: cancellationToken),
            communityPostClient.DeletePostAsync)
    {
    }

    public CommunityBoardPageViewModel(
        Func<CancellationToken, Task<IReadOnlyList<CommunityBoardSummaryResponse>>> loadBoardSummaries,
        Func<CommunityBoardPostQuery, CancellationToken, Task<PlatformCommunityPostListResponse>> loadPosts)
        : this(
            (_, cancellationToken) => loadBoardSummaries(cancellationToken),
            (_, query, cancellationToken) => loadPosts(query, cancellationToken),
            null)
    {
    }

    private CommunityBoardPageViewModel(
        Func<string, CancellationToken, Task<IReadOnlyList<CommunityBoardSummaryResponse>>> loadBoardSummaries,
        Func<string, CommunityBoardPostQuery, CancellationToken, Task<PlatformCommunityPostListResponse>> loadPosts,
        Func<long, string?, CancellationToken, Task>? deletePost)
    {
        this.loadBoardSummaries = loadBoardSummaries;
        this.loadPosts = loadPosts;
        this.deletePost = deletePost;
    }

    public IReadOnlyList<CommunityBoardSummaryResponse> BoardSummaries
    {
        get => boardSummaries;
        private set => SetProperty(ref boardSummaries, value);
    }

    public IReadOnlyList<CommunityBoardSummaryResponse> VisibleBoardTabs
        => BoardSummaries
            .Where(board => !string.Equals(
                board.BoardKey,
                CommunityBoardKeys.Vow,
                StringComparison.OrdinalIgnoreCase))
            .ToArray();

    public CommunityBoardSummaryResponse? CurrentBoard
    {
        get => currentBoard;
        private set
        {
            if (SetProperty(ref currentBoard, value))
            {
                OnPropertyChanged(nameof(CurrentBoardName));
                OnPropertyChanged(nameof(CurrentBoardDescription));
                OnPropertyChanged(nameof(CanCompose));
            }
        }
    }

    public PlatformCommunityPostListResponse Result
    {
        get => result;
        private set
        {
            if (SetProperty(ref result, value))
            {
                OnPropertyChanged(nameof(TotalPages));
                OnPropertyChanged(nameof(VisiblePosts));
            }
        }
    }

    public string SelectedFilter
    {
        get => selectedFilter;
        private set
        {
            if (SetProperty(ref selectedFilter, value))
            {
                OnPropertyChanged(nameof(VisiblePosts));
            }
        }
    }

    public string SearchText
    {
        get => searchText;
        private set
        {
            if (SetProperty(ref searchText, value))
            {
                OnPropertyChanged(nameof(VisiblePosts));
            }
        }
    }

    public CommunityPostViewMode ViewMode
    {
        get => viewMode;
        private set => SetProperty(ref viewMode, value);
    }

    public string? ErrorMessage => 오류메시지;
    public bool IsLoading => 처리중;

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

    public void Configure(string? key)
        => appKey = string.IsNullOrWhiteSpace(key) ? "shipper" : key.Trim();

    public Task<bool> LoadAsync(
        CommunityBoardPageRequest request,
        CancellationToken cancellationToken = default)
    {
        this.request = request;
        RestoreListState(request.SearchText, request.ListFilter, request.ViewMode);
        return 새로고침Async(cancellationToken);
    }

    public async Task DeletePostAsync(
        long postId,
        string? password,
        CancellationToken cancellationToken = default)
    {
        if (deletePost is null)
        {
            throw new InvalidOperationException("게시글 삭제 client가 구성되지 않았습니다.");
        }

        await deletePost(postId, password, cancellationToken);
        await 새로고침Async(cancellationToken);
    }

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        BoardSummaries = MergeWithCoreBoards([]);
        CurrentBoard = ResolveCurrentBoard(BoardSummaries, request);
        NormalizePeriodicFilterForCurrentBoard();

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(LoadTimeout);
        try
        {
            var serverBoards = await loadBoardSummaries(appKey, timeout.Token);
            BoardSummaries = MergeWithCoreBoards(serverBoards);
            CurrentBoard = ResolveCurrentBoard(BoardSummaries, request);
            NormalizePeriodicFilterForCurrentBoard();

            var resolvedKey = CurrentBoard?.BoardKey
                              ?? CommunityBoardCatalog.Find(request.BoardKey ?? request.LegacyBoardName)?.Key;
            var legacyCategory = resolvedKey is null && !string.IsNullOrWhiteSpace(request.LegacyBoardName)
                ? request.LegacyBoardName
                : null;
            Result = await loadPosts(
                appKey,
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
            throw new TimeoutException(
                "응답이 지연되고 있습니다. 네트워크 상태를 확인한 뒤 다시 시도해 주세요.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            Result = new PlatformCommunityPostListResponse();
            throw new InvalidOperationException(
                "네트워크 상태를 확인한 뒤 다시 시도해 주세요.",
                exception);
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
