using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ui,
    SsalddelModuleKind.ClientFeature,
    "Figma 01 Community의 생활·업무 게시판 탐색과 실제 게시글 수 조회 상태를 관리",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "게시글 수는 중립적인 현황으로만 표시하며 추천·가입·배차·계약 판단에 사용하지 않습니다.")]
public sealed class CommunityMobileBoardDirectoryViewModel : PageViewModelBase
{
    private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(3);
    private readonly Func<string, CancellationToken, Task<IReadOnlyList<CommunityBoardSummaryResponse>>> loadBoardSummaries;
    private IReadOnlyDictionary<string, CommunityBoardSummaryResponse> boardSummaries =
        new Dictionary<string, CommunityBoardSummaryResponse>(StringComparer.OrdinalIgnoreCase);
    private string appKey = "shipper";
    private string searchText = string.Empty;
    private string? statusMessage;
    private bool hasLivePostCounts;
    private bool workMode;

    public CommunityMobileBoardDirectoryViewModel(ICommunityPostClient communityPostClient)
        : this((key, cancellationToken) =>
            communityPostClient.GetBoardSummariesAsync(key, cancellationToken))
    {
    }

    public CommunityMobileBoardDirectoryViewModel(
        Func<CancellationToken, Task<IReadOnlyList<CommunityBoardSummaryResponse>>> loadBoardSummaries)
        : this((_, cancellationToken) => loadBoardSummaries(cancellationToken))
    {
    }

    private CommunityMobileBoardDirectoryViewModel(
        Func<string, CancellationToken, Task<IReadOnlyList<CommunityBoardSummaryResponse>>> loadBoardSummaries)
        => this.loadBoardSummaries = loadBoardSummaries;

    public string SearchText
    {
        get => searchText;
        private set
        {
            if (SetProperty(ref searchText, value))
            {
                NotifyVisibleCollectionsChanged();
            }
        }
    }

    public string? StatusMessage
    {
        get => statusMessage;
        private set => SetProperty(ref statusMessage, value);
    }

    public bool IsLoading => 처리중;

    public bool HasLivePostCounts
    {
        get => hasLivePostCounts;
        private set => SetProperty(ref hasLivePostCounts, value);
    }

    public bool WorkMode
    {
        get => workMode;
        private set => SetProperty(ref workMode, value);
    }

    public IReadOnlyList<CommunityMobileLifeBoardPresentation> VisibleLifeBoards
        => CommunityMobileBoardPresentation.LifeBoards
            .Where(board => MatchesSearch(board.DisplayName, board.Description))
            .ToArray();

    public IReadOnlyList<CommunityMobileWorkGroupPresentation> VisibleWorkGroups
        => CommunityMobileBoardPresentation.WorkGroups
            .Where(group => MatchesSearch(
                group.DisplayName,
                group.Description,
                group.BoardKeys
                    .Select(CommunityBoardCatalog.Find)
                    .Where(board => board is not null)
                    .SelectMany(board => new[] { board!.DisplayName, board.Description })
                    .ToArray()))
            .ToArray();

    public IReadOnlyList<CommunityMobilePublicDataBoardPresentation> VisiblePublicDataBoards
        => CommunityMobileBoardPresentation.PublicDataBoards
            .Where(board => MatchesSearch(
                board.DisplayName,
                board.Description,
                board.Provider,
                board.UpdateCycle))
            .ToArray();

    public void Configure(string? key, bool initialWorkMode)
    {
        appKey = string.IsNullOrWhiteSpace(key) ? "shipper" : key.Trim();
        WorkMode = initialWorkMode;
    }

    public Task<bool> LoadAsync(CancellationToken cancellationToken = default)
        => 새로고침Async(cancellationToken);

    public void ToggleMode()
    {
        WorkMode = !WorkMode;
        UpdateSearch(string.Empty);
    }

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        StatusMessage = null;
        HasLivePostCounts = false;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(LoadTimeout);
        try
        {
            var summaries = await loadBoardSummaries(appKey, timeout.Token);
            boardSummaries = summaries
                .Where(board => !string.IsNullOrWhiteSpace(board.BoardKey))
                .GroupBy(board => board.BoardKey, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.First(),
                    StringComparer.OrdinalIgnoreCase);
            HasLivePostCounts = true;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            boardSummaries = EmptySummaries();
            StatusMessage = "게시글 수 조회가 지연되어 기본 게시판부터 표시합니다.";
        }
        catch (Exception)
        {
            boardSummaries = EmptySummaries();
            StatusMessage = "게시글 수를 불러오지 못해 기본 게시판부터 표시합니다.";
        }
        finally
        {
            NotifyVisibleCollectionsChanged();
        }
    }

    public void UpdateSearch(string? value)
        => SearchText = value?.TrimStart() ?? string.Empty;

    public int PostCountFor(string boardKey)
        => boardSummaries.TryGetValue(boardKey, out var summary)
            ? summary.PostCount
            : 0;

    public int PostCountFor(CommunityMobileWorkGroupPresentation group)
        => group.BoardKeys.Sum(PostCountFor);

    private bool MatchesSearch(string name, string description, params string[] relatedText)
    {
        var search = SearchText.Trim();
        return search.Length == 0
               || name.Contains(search, StringComparison.OrdinalIgnoreCase)
               || description.Contains(search, StringComparison.OrdinalIgnoreCase)
               || relatedText.Any(value => value.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyDictionary<string, CommunityBoardSummaryResponse> EmptySummaries()
        => new Dictionary<string, CommunityBoardSummaryResponse>(StringComparer.OrdinalIgnoreCase);

    private void NotifyVisibleCollectionsChanged()
    {
        OnPropertyChanged(nameof(VisibleLifeBoards));
        OnPropertyChanged(nameof(VisibleWorkGroups));
        OnPropertyChanged(nameof(VisiblePublicDataBoards));
    }
}
