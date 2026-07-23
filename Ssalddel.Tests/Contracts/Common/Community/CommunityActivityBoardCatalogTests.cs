using Ssalddel.Application.Behaviors;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Versioning;

namespace Ssalddel.Tests.Contracts.Common.Community;

public sealed class CommunityActivityBoardCatalogTests
{
    [Fact]
    public void Catalog_ConfirmsSevenVersionMountainsThroughV35()
    {
        Assert.Equal(SsalddelProductRoadmapCatalog.All.Count, CommunityActivityBoardCatalog.Bundles.Count);
        Assert.Equal(7, CommunityActivityBoardCatalog.Boards.Count);
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
                Assert.Equal(CommunityBoardGroupCodes.ActivityRoadmap, bundle.Board.GroupCode);
                Assert.NotEmpty(bundle.Activities);
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
        var transport = Assert.IsType<CommunityActivityBoardBundleDefinition>(
            CommunityActivityBoardCatalog.FindBundle("activity-transport"));

        Assert.Equal("2.0", transport.ProductVersion);
        Assert.Equal(1, transport.CommandCount);
        Assert.Equal(7, transport.EventCount);
        Assert.Equal(
            transport.Board,
            CommunityActivityBoardCatalog.FindBundle("activity-loading-completed")?.Board);
        Assert.Equal(
            transport.Board,
            CommunityBoardCatalog.Find("상차 완료"));
        Assert.Same(
            transport.Board,
            CommunityBoardCatalog.Find(transport.Board.Key));
    }

    [Fact]
    public void Catalog_MapsEachBoardToCommandsEventsAndSingleResponsibilityPages()
    {
        var mart = Assert.IsType<CommunityActivityBoardBundleDefinition>(
            CommunityActivityBoardCatalog.FindBundle("activity-mart"));

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
        Assert.Contains("일곱 개", CommunityActivityBoardCatalog.SurfaceMappingBoundary);
        Assert.Contains("단일책임", CommunityActivityBoardCatalog.SurfaceMappingBoundary);
    }

    [Fact]
    public void Catalog_HasUniqueSourcesAndReferencesRealApplicationTypes()
    {
        var definitions = CommunityActivityBoardCatalog.All;
        var applicationTypeNames = typeof(CommunityActivityCommandPostBehavior<,>)
            .Assembly
            .GetTypes()
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
