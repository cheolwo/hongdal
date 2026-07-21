using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.WebApp.Models;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ui,
    SsalddelModuleKind.ClientFeature,
    "커뮤니티 글쓰기·게시글 상세·업무 원장·다이어그램 경로의 화면 문맥을 해석",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "경로와 명시적 query만 해석하며 게시글 저장이나 원장 상태를 직접 변경하지 않습니다.")]
public sealed class CommunityWorkspaceRouteContext
{
    private CommunityWorkspaceRouteContext(
        bool isWriteRoute,
        bool isWorkspaceRoute,
        long? effectivePostId,
        string? seedPostTitle,
        string? boardName,
        string? boardKey,
        string? diagramMode)
    {
        IsWriteRoute = isWriteRoute;
        EffectivePostId = effectivePostId;
        IsPostDetailRoute = effectivePostId is not null || !string.IsNullOrWhiteSpace(seedPostTitle);
        IsDiagramRoute = IsTruthyQueryValue(diagramMode);
        IsWorkspaceLandingRoute = isWorkspaceRoute && !IsPostDetailRoute && !IsDiagramRoute;

        WorkspaceKey = $"{(IsWriteRoute ? "write" : IsWorkspaceLandingRoute ? "workspace" : "detail")}-{EffectivePostId?.ToString() ?? "none"}-{seedPostTitle}-{boardName}-{boardKey}-{diagramMode}";
        WorkspaceTitle = IsWriteRoute
            ? "글쓰기 · 살뜰 커뮤니티"
            : IsWorkspaceLandingRoute
                ? "업무·원장 공간 · 살뜰 커뮤니티"
                : IsDiagramRoute
                    ? "업무 다이어그램 · 살뜰 커뮤니티"
                    : "게시글 · 살뜰 커뮤니티";
        WorkspaceEyebrow = IsWriteRoute
            ? "COMMUNITY COMPOSE"
            : IsWorkspaceLandingRoute
                ? "COMMUNITY WORKSPACE"
                : IsDiagramRoute
                    ? "LEDGER DIAGRAM"
                    : "COMMUNITY POST";
        WorkspaceHeading = IsWriteRoute
            ? "커뮤니티에 글쓰기"
            : IsWorkspaceLandingRoute
                ? "업무·원장 공간"
                : IsDiagramRoute
                    ? "업무 다이어그램"
                    : EffectivePostId is long id
                        ? $"게시글 #{id:N0}"
                        : "추천 게시글";
        WorkspaceDescription = IsWriteRoute
            ? "필요한 일과 가능한 일을 알리고, 당사자가 직접 선택하고 합의할 수 있도록 조건을 분명하게 적습니다."
            : IsWorkspaceLandingRoute
                ? "게시판에서 모인 필요를 게시판 운영, 업무 연결과 공동 원장 도구로 차분히 정리합니다."
                : IsDiagramRoute
                    ? "공동 원장의 참여자, 상태와 다음 업무를 하나의 흐름으로 확인합니다."
                    : "글의 대화와 참여 기록을 확인하고, 합의한 경우에만 공동 원장과 업무 흐름으로 이어갑니다.";
        BackHref = IsWorkspaceLandingRoute
            ? "/community"
            : !string.IsNullOrWhiteSpace(boardKey)
                ? $"/community/boards?boardKey={Uri.EscapeDataString(boardKey)}"
                : !string.IsNullOrWhiteSpace(boardName)
                    ? $"/community/boards?board={Uri.EscapeDataString(boardName)}"
                    : "/community/boards";
        BackLabel = IsWorkspaceLandingRoute ? "공개 커뮤니티" : "글 목록";
    }

    public bool IsWriteRoute { get; }
    public bool IsPostDetailRoute { get; }
    public bool IsDiagramRoute { get; }
    public bool IsWorkspaceLandingRoute { get; }
    public long? EffectivePostId { get; }
    public string WorkspaceKey { get; }
    public string WorkspaceTitle { get; }
    public string WorkspaceEyebrow { get; }
    public string WorkspaceHeading { get; }
    public string WorkspaceDescription { get; }
    public string BackHref { get; }
    public string BackLabel { get; }

    public static CommunityWorkspaceRouteContext Resolve(
        string? relativeUri,
        long? routePostId,
        long? queryPostId,
        string? seedPostTitle,
        string? boardName,
        string? boardKey,
        string? diagramMode)
    {
        var path = NormalizePath(relativeUri);
        return new CommunityWorkspaceRouteContext(
            path.Equals("community/write", StringComparison.OrdinalIgnoreCase),
            path.Equals("community/workspace", StringComparison.OrdinalIgnoreCase),
            routePostId ?? queryPostId,
            seedPostTitle,
            boardName,
            boardKey,
            diagramMode);
    }

    private static string NormalizePath(string? relativeUri)
        => (relativeUri ?? string.Empty)
            .Split('?', '#')[0]
            .Trim('/');

    private static bool IsTruthyQueryValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && (value.Equals("1", StringComparison.OrdinalIgnoreCase)
               || value.Equals("true", StringComparison.OrdinalIgnoreCase)
               || value.Equals("yes", StringComparison.OrdinalIgnoreCase)
               || value.Equals("diagram", StringComparison.OrdinalIgnoreCase));
}
