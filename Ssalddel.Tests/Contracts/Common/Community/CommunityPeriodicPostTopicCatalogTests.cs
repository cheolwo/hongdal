using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Contracts.Common.Community;

public sealed class CommunityPeriodicPostTopicCatalogTests
{
    [Fact]
    public void 업무단위_게시판만_주기성_주제분류를_지원한다()
    {
        Assert.True(CommunityPeriodicPostTopicCatalog.SupportsBoard(
            CommunityActivityBoardKeys.FoundationEvidence));
        Assert.True(CommunityPeriodicPostTopicCatalog.SupportsBoard(
            CommunityActivityBoardKeys.CustomsProcess));
        Assert.False(CommunityPeriodicPostTopicCatalog.SupportsBoard(
            CommunityBoardKeys.InformationPrices));
        Assert.False(CommunityPeriodicPostTopicCatalog.SupportsBoard(
            CommunityBoardKeys.FreeLife));
    }

    [Theory]
    [InlineData("전체글", CommunityPeriodicPostVisibilityModes.All)]
    [InlineData("일반글", CommunityPeriodicPostVisibilityModes.Exclude)]
    [InlineData("주기성", CommunityPeriodicPostVisibilityModes.Only)]
    [InlineData("공지", CommunityPeriodicPostVisibilityModes.All)]
    public void 목록_주제분류를_서버_조회범위로_변환한다(
        string listFilter,
        string expected)
        => Assert.Equal(
            expected,
            CommunityPeriodicPostTopicCatalog.VisibilityFor(listFilter));

    [Theory]
    [InlineData("GENERAL", "general")]
    [InlineData("unknown", "general")]
    [InlineData("PERIODIC", "periodic")]
    public void 주제분류_표시명을_안정코드에서_결정한다(
        string code,
        string normalized)
    {
        Assert.Equal(
            normalized == CommunityPostTopicClassificationCodes.Periodic
                ? "주기성"
                : "일반",
            CommunityPostTopicClassificationCodes.DisplayName(code));
    }
}
