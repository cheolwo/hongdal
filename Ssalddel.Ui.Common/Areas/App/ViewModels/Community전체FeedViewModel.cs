using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Ui.Common.Areas.App.ViewModels;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ui,
    SsalddelModuleKind.ClientFeature,
    "게시판 경계를 넘은 공개 커뮤니티 전체 글을 서버 순서대로 page 단위로 이어서 조회",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "보호 속성이나 성사 가능성으로 글을 재정렬하지 않고 API가 반환한 공개 순서와 게시판 출처를 유지합니다.")]
public sealed class Community전체FeedViewModel(
    Func<int, int, CancellationToken, Task<PlatformCommunityPostListResponse>> loadPosts)
{
    public const int PageSize = 12;

    private static readonly TimeSpan LoadTimeout = TimeSpan.FromSeconds(8);
    private readonly List<PlatformCommunityPostResponse> items = [];
    private bool hasMore;

    public IReadOnlyList<PlatformCommunityPostResponse> Items => items;
    public int CurrentPage { get; private set; }
    public int TotalCount { get; private set; }
    public bool IsInitialLoading { get; private set; } = true;
    public bool IsLoadingMore { get; private set; }
    public string? ErrorMessage { get; private set; }
    public bool HasMore => hasMore && !IsInitialLoading;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsLoadingMore)
        {
            return;
        }

        items.Clear();
        CurrentPage = 0;
        TotalCount = 0;
        hasMore = false;
        ErrorMessage = null;
        IsInitialLoading = true;

        await LoadPageAsync(1, cancellationToken);
        IsInitialLoading = false;
    }

    public async Task LoadMoreAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitialLoading || IsLoadingMore || !HasMore)
        {
            return;
        }

        await LoadPageAsync(CurrentPage + 1, cancellationToken);
    }

    private async Task LoadPageAsync(int page, CancellationToken cancellationToken)
    {
        IsLoadingMore = true;
        ErrorMessage = null;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(LoadTimeout);
        try
        {
            var result = await loadPosts(page, PageSize, timeout.Token);
            var knownIds = items.Select(item => item.Id).ToHashSet();
            items.AddRange(result.Items.Where(item => knownIds.Add(item.Id)));
            CurrentPage = page;
            TotalCount = Math.Max(result.TotalCount, items.Count);
            hasMore = result.Items.Count > 0
                      && (result.TotalCount > 0
                          ? items.Count < result.TotalCount
                          : result.Items.Count >= PageSize);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            ErrorMessage = items.Count == 0
                ? "글을 불러오는 데 시간이 걸리고 있습니다. 네트워크 상태를 확인해 주세요."
                : "다음 글을 불러오는 데 시간이 걸리고 있습니다. 잠시 뒤 다시 시도해 주세요.";
        }
        catch (Exception)
        {
            ErrorMessage = items.Count == 0
                ? "커뮤니티 글을 불러오지 못했습니다. 게시판 보기는 계속 사용할 수 있습니다."
                : "다음 글을 불러오지 못했습니다. 이미 불러온 글은 그대로 볼 수 있습니다.";
        }
        finally
        {
            IsLoadingMore = false;
            if (items.Count == 0)
            {
                hasMore = false;
            }
        }
    }
}
