using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Ui.Common.Areas.App.Models;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ui,
    SsalddelModuleKind.ClientFeature,
    "Figma 01 Community의 생활·업무 게시판 탐색과 실제 게시글 수 조회 상태를 관리",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "게시글 수는 중립적인 현황으로만 표시하며 추천·가입·배차·계약 판단에 사용하지 않습니다.")]
public sealed class CommunityMobileBoardDirectoryViewModel(
    Func<CancellationToken, Task<IReadOnlyList<CommunityBoardSummaryResponse>>> loadBoardSummaries)
{
    private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(3);
    private IReadOnlyDictionary<string, CommunityBoardSummaryResponse> boardSummaries =
        new Dictionary<string, CommunityBoardSummaryResponse>(StringComparer.OrdinalIgnoreCase);

    public string SearchText { get; private set; } = string.Empty;
    public string? StatusMessage { get; private set; }
    public bool IsLoading { get; private set; } = true;
    public bool HasLivePostCounts { get; private set; }

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

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        StatusMessage = null;
        HasLivePostCounts = false;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(LoadTimeout);
        try
        {
            var summaries = await loadBoardSummaries(timeout.Token);
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
            IsLoading = false;
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
}
