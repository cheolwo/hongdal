using Ssalddel.WebApp.Models;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.WebApp;

public sealed class CommunityWorkspaceRouteContextTests
{
    [Fact]
    public void WriteRoute_ProvidesComposePresentationAndBoardBackLink()
    {
        var context = CommunityWorkspaceRouteContext.Resolve(
            "community/write?board=자유·생활",
            routePostId: null,
            queryPostId: null,
            seedPostTitle: null,
            boardName: "자유·생활",
            boardKey: null,
            diagramMode: null);

        Assert.True(context.IsWriteRoute);
        Assert.False(context.IsPostDetailRoute);
        Assert.False(context.IsWorkspaceLandingRoute);
        Assert.Equal("글쓰기 · 살뜰 커뮤니티", context.WorkspaceTitle);
        Assert.Equal("커뮤니티에 글쓰기", context.WorkspaceHeading);
        Assert.Equal(
            $"/community/boards?board={Uri.EscapeDataString("자유·생활")}",
            context.BackHref);
        Assert.StartsWith("write-none-", context.WorkspaceKey);
    }

    [Fact]
    public void WorkspaceRoute_WithoutDetailOrDiagram_IsLandingContext()
    {
        var context = CommunityWorkspaceRouteContext.Resolve(
            "/community/workspace#community-ledger-draft",
            null,
            null,
            null,
            null,
            null,
            null);

        Assert.True(context.IsWorkspaceLandingRoute);
        Assert.False(context.IsPostDetailRoute);
        Assert.False(context.IsDiagramRoute);
        Assert.Equal("업무·원장 공간", context.WorkspaceHeading);
        Assert.Equal("/community", context.BackHref);
        Assert.Equal("공개 커뮤니티", context.BackLabel);
        Assert.StartsWith("workspace-none-", context.WorkspaceKey);
    }

    [Fact]
    public void DetailRoute_RoutePostIdWinsAndBoardKeyControlsBackLink()
    {
        var context = CommunityWorkspaceRouteContext.Resolve(
            "community/posts/42",
            routePostId: 42,
            queryPostId: 99,
            seedPostTitle: null,
            boardName: "무시할 게시판",
            boardKey: "free life",
            diagramMode: null);

        Assert.True(context.IsPostDetailRoute);
        Assert.Equal(42, context.EffectivePostId);
        Assert.Equal("게시글 #42", context.WorkspaceHeading);
        Assert.Equal("/community/boards?boardKey=free%20life", context.BackHref);
        Assert.Equal("글 목록", context.BackLabel);
    }

    [Fact]
    public void RecommendedSeedRoute_IsDetailWithoutInventingPostId()
    {
        var context = CommunityWorkspaceRouteContext.Resolve(
            CommunityPageRoutes.RecommendedPostDetail,
            null,
            null,
            "추천 글",
            null,
            null,
            null);

        Assert.True(context.IsPostDetailRoute);
        Assert.False(context.IsRecommendedListRoute);
        Assert.Null(context.EffectivePostId);
        Assert.Equal("추천 게시글", context.WorkspaceHeading);
        Assert.Equal("/community/boards", context.BackHref);
    }

    [Fact]
    public void RecommendedRoute_WithoutSeed_IsDedicatedListContext()
    {
        var context = CommunityWorkspaceRouteContext.Resolve(
            "community/posts/recommended",
            null,
            null,
            null,
            null,
            null,
            null);

        Assert.True(context.IsRecommendedListRoute);
        Assert.False(context.IsPostDetailRoute);
        Assert.False(context.IsWorkspaceLandingRoute);
        Assert.Equal("추천 글 · 살뜰 커뮤니티", context.WorkspaceTitle);
        Assert.Equal("추천 글 모아보기", context.WorkspaceHeading);
        Assert.StartsWith("recommended-list-none-", context.WorkspaceKey);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("YES")]
    [InlineData("diagram")]
    public void DiagramQuery_UsesDiagramContextAndDisablesWorkspaceLanding(string value)
    {
        var context = CommunityWorkspaceRouteContext.Resolve(
            "community/workspace",
            null,
            null,
            null,
            null,
            null,
            value);

        Assert.True(context.IsDiagramRoute);
        Assert.False(context.IsWorkspaceLandingRoute);
        Assert.Equal("업무 다이어그램 · 살뜰 커뮤니티", context.WorkspaceTitle);
        Assert.Equal("업무 다이어그램", context.WorkspaceHeading);
    }
}
