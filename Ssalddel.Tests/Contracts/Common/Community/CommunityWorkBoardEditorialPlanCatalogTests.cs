using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Tests.Contracts.Common.Community;

public sealed class CommunityWorkBoardEditorialPlanCatalogTests
{
    [Fact]
    public void Catalog_AssignsOneEditorialPlanToEveryWorkBoard()
    {
        var plans = CommunityWorkBoardEditorialPlanCatalog.All;

        Assert.Equal(CommunityActivityBoardCatalog.Boards.Count, plans.Count);
        Assert.Equal(plans.Count, plans.Select(plan => plan.BoardKey).Distinct().Count());
        Assert.All(CommunityActivityBoardCatalog.Boards, board =>
            Assert.Contains(plans, plan => plan.BoardKey == board.Key));
        Assert.All(plans, plan =>
        {
            Assert.NotEmpty(plan.Topics);
            Assert.NotEmpty(plan.ExecutableSourceKeys);
            Assert.NotEmpty(plan.PlannedOfficialSources);
            Assert.True(plan.RequiresEditorialReview);
        });
    }

    [Fact]
    public void Catalog_ConnectsTradeAndTransportToTopicsWithoutPretendingPlannedConnectorsExist()
    {
        var customs = CommunityWorkBoardEditorialPlanCatalog.Find(
            CommunityActivityBoardKeys.CustomsProcess);
        var loading = CommunityWorkBoardEditorialPlanCatalog.Find(
            CommunityActivityBoardKeys.LoadingJourney);

        Assert.Contains(customs.Topics, topic => topic.Contains("수입신고"));
        Assert.Contains(customs.PlannedOfficialSources, source => source.Contains("관세청"));
        Assert.DoesNotContain(customs.ExecutableSourceKeys, source => source.Contains("customs"));
        Assert.Contains(loading.PlannedOfficialSources, source => source.Contains("안전보건공단"));
    }
}
