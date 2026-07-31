using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Contracts.Community;

public sealed class CommunityBoardCatalogTests
{
    [Theory]
    [InlineData("모집·함께하기", true)]
    [InlineData("공동구매", true)]
    [InlineData("서원", false)]
    [InlineData("자유·생활", false)]
    public void 마음모으기는_공동구매모집게시판에서만_활성화한다(
        string category,
        bool expected)
    {
        Assert.Equal(expected, CommunityPostInterestGatheringPolicy.IsGroupPurchaseCategory(category));
        Assert.Equal(expected, CommunityPostInterestGatheringPolicy.ResolveEnabled(category, requested: true));
        Assert.False(CommunityPostInterestGatheringPolicy.ResolveEnabled(category, requested: false));
    }

    [Fact]
    public void 공개게시판은_글목적별로구성하고_신고분쟁은제외한다()
    {
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.Vow);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.RegionalCulture);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.InformationPrices);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.Participation);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.CompletionReview);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.Prajna);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.Food);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.Cargo);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.PeriodicDataKamis);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.PeriodicDataMfds);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.PeriodicDataUsda);
        Assert.Contains(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.PeriodicDataCustomsImportUnitPrice);
        Assert.DoesNotContain(CommunityBoardCatalog.PublicBoards, board => board.Key == CommunityBoardKeys.SafetyReport);
        Assert.False(CommunityBoardCatalog.SafetyReport.IsPublic);
        Assert.Equal("신고·분쟁", CommunityBoardCatalog.SafetyReport.DisplayName);
        Assert.True(CommunityBoardCatalog.IsProtectedCategory("신고/분쟁"));
    }

    [Fact]
    public void 주요게시판탭은_서원_자유생활_지역문화_농수산물가격만노출한다()
    {
        Assert.Equal(
            [
                CommunityBoardKeys.Vow,
                CommunityBoardKeys.FreeLife,
                CommunityBoardKeys.RegionalCulture,
                CommunityBoardKeys.InformationPrices
            ],
            CommunityBoardCatalog.FeaturedBoards.Select(board => board.Key));
        Assert.Equal(
            ["서원", "자유·생활", "지역 문화", "농수산물 가격"],
            CommunityBoardCatalog.FeaturedBoards.Select(board => board.DisplayName));
    }

    [Fact]
    public void 주기성데이터게시판은_운영자만작성하고_주기성필터를지원한다()
    {
        var dataBoards = CommunityBoardCatalog.PublicBoards
            .Where(board => board.GroupCode == CommunityBoardGroupCodes.PeriodicData)
            .ToArray();

        Assert.Equal(4, dataBoards.Length);
        Assert.All(dataBoards, board =>
        {
            Assert.False(board.IsUserCreatable);
            Assert.Equal(
                CommunityBoardPostingAccessCodes.OperatorOnly,
                board.PostingAccessCode);
            Assert.True(CommunityPeriodicPostTopicCatalog.SupportsBoard(board.Key));
        });
    }

    [Fact]
    public void 정보시세수입식품은_중국과미국의안정업무태그로분류한다()
    {
        Assert.Equal(
            ["CN", "US"],
            CommunityImportedFoodCountryFilterCatalog.All.Select(item => item.CountryCode));
        Assert.Equal(
            "중국 수입식품 공개근거",
            CommunityImportedFoodCountryFilterCatalog.China.WorkflowTag);
        Assert.Equal(
            "미국 수입식품 공개근거",
            CommunityImportedFoodCountryFilterCatalog.UnitedStates.WorkflowTag);
        Assert.Equal(
            "중국",
            CommunityImportedFoodCountryFilterCatalog.FindByWorkflowTag(
                " 중국 수입식품 공개근거 ")?.DisplayName);
        Assert.Null(
            CommunityImportedFoodCountryFilterCatalog.FindByWorkflowTag("국내 가격 정보"));
    }

    [Fact]
    public void 시스템활동게시판은_버전이아니라_업무단위산으로확정한다()
    {
        var activityBoards = CommunityBoardCatalog.PublicBoards
            .Where(board => CommunityBoardGroupCodes.IsActivityWorkflow(board.GroupCode))
            .ToArray();

        Assert.Equal(16, activityBoards.Length);
        Assert.Equal(
            CommunityActivityBoardCatalog.Boards.Select(board => board.Key),
            activityBoards.Select(board => board.Key));
        Assert.All(activityBoards, board =>
        {
            Assert.False(board.IsUserCreatable);
            Assert.Equal(CommunityBoardPostingAccessCodes.OperatorOnly, board.PostingAccessCode);
        });
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

    [Fact]
    public void 게시판마다_비로그인작성_로그인작성_운영자작성조건을구분한다()
    {
        Assert.Equal(
            CommunityBoardPostingAccessCodes.Anonymous,
            CommunityBoardCatalog.FreeLife.PostingAccessCode);
        Assert.True(CommunityBoardCatalog.Vow.AllowsAnonymousPosting);
        Assert.True(CommunityBoardCatalog.Food.AllowsAnonymousPosting);
        Assert.True(CommunityBoardCatalog.SafetyReport.AllowsAnonymousPosting);

        Assert.Equal(
            CommunityBoardPostingAccessCodes.Authenticated,
            CommunityBoardCatalog.Cargo.PostingAccessCode);
        Assert.True(CommunityBoardCatalog.Participation.RequiresAuthenticatedPosting);
        Assert.True(CommunityBoardCatalog.SalesSupply.RequiresAuthenticatedPosting);
        Assert.True(CommunityBoardCatalog.LedgerProgress.RequiresAuthenticatedPosting);

        Assert.Equal(
            CommunityBoardPostingAccessCodes.OperatorOnly,
            CommunityBoardCatalog.NoticeGuide.PostingAccessCode);
        Assert.Equal(
            CommunityBoardPostingAccessCodes.OperatorOnly,
            CommunityBoardCatalog.Prajna.PostingAccessCode);
    }

    [Theory]
    [InlineData(CommunityBoardKeys.Vow, "서원 적는 이웃-A1B2")]
    [InlineData(CommunityBoardKeys.FreeLife, "지나가는 이웃-A1B2")]
    [InlineData(CommunityBoardKeys.QuestionHelp, "궁금한 이웃-A1B2")]
    [InlineData(CommunityBoardKeys.InformationPrices, "시세 살피는 이웃-A1B2")]
    [InlineData(CommunityBoardKeys.Food, "골목 미식가-A1B2")]
    [InlineData(CommunityBoardKeys.SafetyReport, "익명 신고자")]
    public void 익명닉네임은_게시판특색을반영한다(string boardKey, string expected)
    {
        Assert.Equal(expected, CommunityAnonymousNicknameCatalog.Create(boardKey, "a1-b2-c3"));
    }

    [Theory]
    [InlineData("발원", CommunityBoardKeys.Vow)]
    [InlineData("지역 문화·특산물", CommunityBoardKeys.RegionalCulture)]
    [InlineData("정보·시세", CommunityBoardKeys.InformationPrices)]
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
        Assert.Contains(CommunityBoardCatalog.All, board => board.DisplayName == "배차 결정");
        Assert.Contains(CommunityBoardCatalog.All, board => board.DisplayName == "창고 입고·검수·적재");
    }
}
