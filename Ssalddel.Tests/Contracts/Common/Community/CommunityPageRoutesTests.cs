using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Contracts.Common.Community;

public sealed class CommunityPageRoutesTests
{
    [Fact]
    public void 게시판과글쓰기경로는_선택문맥을encode한다()
    {
        Assert.Equal(
            "/community/boards?boardKey=free%20life",
            CommunityPageRoutes.BoardsFor(boardKey: "free life"));
        Assert.Equal(
            $"/community/write?board={Uri.EscapeDataString("자유·생활")}",
            CommunityPageRoutes.ComposeFor("자유·생활"));
        Assert.Equal(CommunityPageRoutes.Compose, CommunityPageRoutes.ComposeFor("전체"));
    }

    [Fact]
    public void 영속글과추천sample글은_서로다른상세route를사용한다()
    {
        Assert.Equal(
            $"/community/posts/42?board={Uri.EscapeDataString("업무 질문")}",
            CommunityPageRoutes.PostDetailFor(42, "업무 질문"));
        Assert.Equal(
            $"/community/posts/recommended/detail?seed={Uri.EscapeDataString("추천 글")}&board={Uri.EscapeDataString("자유")}",
            CommunityPageRoutes.RecommendedPostDetailFor("추천 글", "자유"));
    }

    [Fact]
    public void 다이어그램경로는_원장template문맥을보존한다()
    {
        Assert.Equal(
            "/community/workspace?diagram=true&ledgerTemplate=group-purchase",
            CommunityPageRoutes.DiagramFor("group-purchase"));
        Assert.Equal(
            "/community/ledgers/new?ledgerTemplate=group-purchase",
            CommunityPageRoutes.LedgerDraftFor("group-purchase"));
        Assert.Equal("/community/boards/manage", CommunityPageRoutes.BoardManagement);
    }
}
