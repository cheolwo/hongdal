using Ssalddel.Application.Behaviors;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Tests.Contracts.Common.Community;

public sealed class CommunityActivityBoardCatalogTests
{
    [Fact]
    public void Catalog_ConfirmsIndependentWorkUnitMountainsThroughV35()
    {
        Assert.Equal(16, CommunityActivityBoardCatalog.Bundles.Count);
        Assert.Equal(16, CommunityActivityBoardCatalog.Boards.Count);
        Assert.Equal("☶", CommunityActivityBoardBundleDefinition.MountainSymbol);
        Assert.Equal("간", CommunityActivityBoardBundleDefinition.MountainName);

        Assert.All(
            SsalddelProductRoadmapCatalog.All,
            stage => Assert.Contains(
                CommunityActivityBoardCatalog.Bundles,
                bundle => bundle.ProductVersion == stage.Version));
        Assert.All(
            CommunityActivityBoardCatalog.Bundles,
            bundle =>
            {
                Assert.True(bundle.Board.IsPublic);
                Assert.False(bundle.Board.IsUserCreatable);
                Assert.Equal(
                    CommunityBoardPostingAccessCodes.OperatorOnly,
                    bundle.Board.PostingAccessCode);
                Assert.True(CommunityBoardGroupCodes.IsActivityWorkflow(bundle.Board.GroupCode));
                Assert.NotEmpty(bundle.Pages);
                Assert.All(bundle.Pages, page =>
                {
                    Assert.False(string.IsNullOrWhiteSpace(page.Surface));
                    Assert.False(string.IsNullOrWhiteSpace(page.PageName));
                    Assert.False(string.IsNullOrWhiteSpace(page.Route));
                    Assert.False(string.IsNullOrWhiteSpace(page.Responsibility));
                });
            });
    }

    [Fact]
    public void Catalog_GroupsSourcesByBoardAndPreservesLegacyBoardKeys()
    {
        var request = Assert.IsType<CommunityActivityBoardBundleDefinition>(
            CommunityActivityBoardCatalog.FindBundle("activity-transport"));

        Assert.Equal("2.0", request.ProductVersion);
        Assert.Equal(5, request.CommandCount);
        Assert.Equal(0, request.EventCount);
        Assert.Equal(1, request.PublishedActivityCount);

        var journey = Assert.IsType<CommunityActivityBoardBundleDefinition>(
            CommunityActivityBoardCatalog.FindBundle("activity-loading-completed"));
        Assert.Equal(CommunityActivityBoardKeys.LoadingJourney, journey.Board.Key);
        Assert.Equal(
            journey.Board,
            CommunityBoardCatalog.Find("상차 완료"));
        Assert.Same(
            request.Board,
            CommunityBoardCatalog.Find(request.Board.Key));
    }

    [Fact]
    public void Catalog_MapsEachBoardToCommandsEventsAndSingleResponsibilityPages()
    {
        var mart = Assert.IsType<CommunityActivityBoardBundleDefinition>(
            CommunityActivityBoardCatalog.FindBundle(CommunityActivityBoardKeys.MartFulfillment));

        Assert.Equal(0, mart.CommandCount);
        Assert.Equal(3, mart.EventCount);
        Assert.Contains(mart.Activities, activity => activity.SourceName == "창고피킹완료됨Event");
        Assert.Contains(mart.Pages, page => page.Route == "/food/mart");
        Assert.Contains(mart.Pages, page => page.Route == "/warehouse/mart/picking");
        Assert.Contains(mart.Pages, page => page.Route.Contains("{OrderId:long}", StringComparison.Ordinal));
    }

    [Fact]
    public void SurfaceMappingBoundary_IsFinalizedThroughV35()
    {
        Assert.Contains("0.0~3.5", CommunityActivityBoardCatalog.SurfaceMappingBoundary);
        Assert.Contains("독립된 업무단위", CommunityActivityBoardCatalog.SurfaceMappingBoundary);
        Assert.DoesNotContain("버전 게시판에 연결", CommunityActivityBoardCatalog.SurfaceMappingBoundary);
    }

    [Fact]
    public void Catalog_HasUniqueSourcesAndReferencesRealApplicationTypes()
    {
        var definitions = CommunityActivityBoardCatalog.All;
        var applicationTypeNames = new[]
            {
                typeof(CommunityActivityCommandPostBehavior<,>).Assembly,
                typeof(CommunityActivityBoardCatalog).Assembly
            }
            .Distinct()
            .SelectMany(assembly => assembly.GetTypes())
            .Select(type => type.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(
            definitions.Count,
            definitions.Select(definition => $"{definition.SourceKind}:{definition.SourceName}")
                .Distinct(StringComparer.Ordinal)
                .Count());
        Assert.Equal(
            CommunityActivityBoardCatalog.Boards.Count,
            CommunityActivityBoardCatalog.Boards
                .Select(board => board.Key)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
        Assert.All(
            definitions,
            definition => Assert.Contains(definition.SourceName, applicationTypeNames));
    }

    [Fact]
    public void Catalog_SeparatesWorkflowRelationshipsFromPublicActivityProjection()
    {
        Assert.Equal(27, CommunityActivityBoardCatalog.Bundles.Sum(bundle => bundle.CommandCount));
        Assert.Equal(25, CommunityActivityBoardCatalog.Bundles.Sum(bundle => bundle.EventCount));
        Assert.Equal(
            28,
            CommunityActivityBoardCatalog.All.Count(activity => activity.PublishesActivityPost));

        var groupPurchase = Assert.IsType<CommunityActivityBoardBundleDefinition>(
            CommunityActivityBoardCatalog.FindBundle(CommunityActivityBoardKeys.IndividualDemand));
        Assert.Equal(
            SsalddelProductRoadmapCatalog.IndividualOrderVersion,
            groupPurchase.ProductVersion);
        Assert.Contains(
            groupPurchase.Activities,
            activity => activity.SourceName == "공동구매자동수요등록Command"
                        && !activity.PublishesActivityPost);

        var foodDelivery = Assert.IsType<CommunityActivityBoardBundleDefinition>(
            CommunityActivityBoardCatalog.FindBundle(CommunityActivityBoardKeys.FoodOrderAcceptance));
        Assert.Contains(
            foodDelivery.Activities,
            activity => activity.SourceName == "음식주문등록Command"
                        && !activity.PublishesActivityPost);

        var mart = Assert.IsType<CommunityActivityBoardBundleDefinition>(
            CommunityActivityBoardCatalog.FindBundle(CommunityActivityBoardKeys.MartFulfillment));
        Assert.Equal(0, mart.CommandCount);
    }

    [Fact]
    public void Catalog_ExposesMissingFoodDeliveryBoundaryInsteadOfHidingItInVersionBoard()
    {
        var handoff = Assert.IsType<CommunityActivityBoardBundleDefinition>(
            CommunityActivityBoardCatalog.FindBundle(CommunityActivityBoardKeys.FoodDeliveryHandoff));

        Assert.Empty(handoff.Activities);
        Assert.NotEmpty(handoff.Pages);
        Assert.Contains("보완", handoff.Board.Description);
    }

    [Theory]
    [InlineData("운송문제신고됨Event")]
    [InlineData("결제승인완료Event")]
    [InlineData("VisaSupportRequestedEvent")]
    [InlineData("파일업로드완료됨Event")]
    public void Catalog_ExcludesSensitiveOrHighRiskOccurrences(string sourceName)
        => Assert.DoesNotContain(
            CommunityActivityBoardCatalog.All,
            definition => definition.SourceName == sourceName);
}
