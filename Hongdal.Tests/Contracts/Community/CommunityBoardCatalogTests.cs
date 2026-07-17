using Hongdal.Contracts.Common.Community;

namespace Hongdal.Tests.Contracts.Community;

public sealed class CommunityBoardCatalogTests
{
    [Fact]
    public void 공개게시판은_글목적별로구성하고_신고분쟁은제외한다()
    {
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.InformationPrices);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.Participation);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.CompletionReview);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.Prajna);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.Food);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.Cargo);
        Assert.DoesNotContain(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.SafetyReport);
        Assert.False(CommunityBoardCatalog.SafetyReport.IsPublic);
        Assert.Equal("신고·분쟁", CommunityBoardCatalog.SafetyReport.DisplayName);
        Assert.True(CommunityBoardCatalog.IsProtectedCategory("신고/분쟁"));
    }

    [Fact]
    public void 반야는_관리자선별용고정게시판이다()
    {
        Assert.True(CommunityBoardCatalog.Prajna.IsPublic);
        Assert.False(CommunityBoardCatalog.Prajna.IsUserCreatable);
        Assert.DoesNotContain(
            CommunityBoardCatalog.UserCreatableBoards,
            board => board.Key == CommunityBoardKeys.Prajna);
        Assert.Equal(
            CommunityBoardCatalog.Prajna,
            CommunityBoardCatalog.Find(CommunityBoardKeys.Prajna));
    }

    [Theory]
    [InlineData("업무 질문", CommunityBoardKeys.QuestionHelp)]
    [InlineData("운송 실무", CommunityBoardKeys.QuestionHelp)]
    [InlineData("공동구매", CommunityBoardKeys.Participation)]
    [InlineData("판매", CommunityBoardKeys.SalesSupply)]
    [InlineData("생활 원장", CommunityBoardKeys.LedgerProgress)]
    [InlineData("시스템 다이어그램", CommunityBoardKeys.LedgerProgress)]
    [InlineData("성립 사례", CommunityBoardKeys.CompletionReview)]
    [InlineData("맛집", CommunityBoardKeys.Food)]
    [InlineData("화물 운송", CommunityBoardKeys.Cargo)]
    public void 기존분류는_새게시판의별칭으로조회한다(string legacyCategory, string expectedBoardKey)
    {
        var board = Assert.IsType<CommunityBoardDefinition>(CommunityBoardCatalog.Find(legacyCategory));

        Assert.Equal(expectedBoardKey, board.Key);
        Assert.True(CommunityBoardCatalog.MatchesCategory(expectedBoardKey, legacyCategory));
        Assert.Equal(board.DisplayName, CommunityBoardCatalog.ResolveCanonicalCategory(legacyCategory));
    }

    [Fact]
    public void 업무영역과역할은_게시판정의에섞지않는다()
    {
        Assert.DoesNotContain(CommunityBoardCatalog.All, board => board.DisplayName == "운송");
        Assert.DoesNotContain(CommunityBoardCatalog.All, board => board.DisplayName == "창고");
        Assert.DoesNotContain(CommunityBoardCatalog.All, board => board.DisplayName == "창고 관리자");
    }
}
