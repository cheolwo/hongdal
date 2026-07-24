namespace Ssalddel.Contracts.Common.Community;

/// <summary>
/// 게시판 목록과 3단계 상세 화면을 왕복할 때 복원할 읽기 문맥입니다.
/// </summary>
public sealed record CommunityBoardNavigationContext
{
    private static readonly string[] SupportedFilters =
    [
        "전체글",
        CommunityPeriodicPostTopicCatalog.GeneralListFilter,
        CommunityPeriodicPostTopicCatalog.PeriodicListFilter,
        "공지",
        "추천글"
    ];

    public const string ListViewMode = "list";
    public const string CardViewMode = "cards";

    public string? BoardName { get; init; }
    public string? BoardKey { get; init; }
    public string? WorkflowTag { get; init; }
    public string? RoleTag { get; init; }
    public int Page { get; init; } = 1;
    public string SearchText { get; init; } = string.Empty;
    public string ListFilter { get; init; } = "전체글";
    public string ViewMode { get; init; } = ListViewMode;
    public string? FocusTarget { get; init; }

    public string ToPath()
        => CommunityPageRoutes.BoardsFor(
            BoardName,
            BoardKey,
            WorkflowTag,
            RoleTag,
            Page,
            SearchText,
            ListFilter,
            ViewMode,
            FocusTarget);

    public static int NormalizePage(int? page)
        => Math.Clamp(page ?? 1, 1, 10_000);

    public static string NormalizeSearch(string? search)
    {
        var value = search?.Trim() ?? string.Empty;
        return value.Length <= 100 ? value : value[..100];
    }

    public static string NormalizeFilter(string? filter)
        => SupportedFilters.FirstOrDefault(item => string.Equals(
               item,
               filter?.Trim(),
               StringComparison.OrdinalIgnoreCase))
           ?? "전체글";

    public static string NormalizeViewMode(string? viewMode)
        => string.Equals(viewMode?.Trim(), CardViewMode, StringComparison.OrdinalIgnoreCase)
            ? CardViewMode
            : ListViewMode;

    public static string FocusForPost(long postId)
    {
        if (postId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(postId), "게시글 ID는 양수여야 합니다.");
        }

        return $"community-post-{postId}";
    }

    public static long? PostIdFromFocus(string? focusTarget)
    {
        const string prefix = "community-post-";
        var normalized = PageNavigationContext.NormalizeFocusTarget(focusTarget);
        return normalized is not null
               && normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
               && long.TryParse(normalized[prefix.Length..], out var postId)
               && postId > 0
            ? postId
            : null;
    }
}

public static class CommunityBoardNavigationQueryNames
{
    public const string BoardKey = "boardKey";
    public const string BoardName = "board";
    public const string WorkflowTag = "workflowTag";
    public const string RoleTag = "roleTag";
    public const string Page = "page";
    public const string Search = "q";
    public const string ListFilter = "filter";
    public const string ViewMode = "view";
}
