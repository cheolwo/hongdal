using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Content;
using Ssalddel.Contracts.Common.Customs;

namespace Ssalddel.Tests.Contracts.Common.Community;

public sealed class CommunityBoardInformationRelationCatalogTests
{
    [Fact]
    public void Catalog_AssignsOneRelationToEveryBoardIncludingProtectedBoards()
    {
        var relations = CommunityBoardInformationRelationCatalog.All;

        Assert.Equal(CommunityBoardCatalog.All.Count, relations.Count);
        Assert.Equal(
            relations.Count,
            relations.Select(relation => relation.BoardKey).Distinct().Count());
        Assert.All(CommunityBoardCatalog.All, board =>
            Assert.Contains(relations, relation => relation.BoardKey == board.Key));
        Assert.All(relations, relation =>
        {
            Assert.NotEmpty(relation.Topics);
            Assert.False(string.IsNullOrWhiteSpace(relation.PreferredCadence));
            Assert.False(string.IsNullOrWhiteSpace(relation.AutomationBoundary));
        });
    }

    [Fact]
    public void InformationPrices_ConnectsCurrentPriceCustomsAndImportedFoodBatches()
    {
        var relation = CommunityBoardInformationRelationCatalog.Find(
            CommunityBoardKeys.InformationPrices);

        Assert.NotNull(relation);
        Assert.Contains(
            relation.Sources,
            source => source.SourceKey == CommunityInformationSourceKeys.KamisPriceObservations
                      && source.HasPeriodicBatchModule
                      && !source.AllowsAutomaticPublication);
        Assert.Contains(
            relation.Sources,
            source => source.SourceKey == CommunityInformationSourceKeys.UsdaNassPriceObservations
                      && source.HasPeriodicBatchModule
                      && !source.AllowsAutomaticPublication);
        Assert.Contains(
            relation.Sources,
            source => source.SourceKey == Hs공공데이터출처Keys.세관장확인대상물품
                      && source.BatchStatus == CommunityBoardInformationBatchStatuses.OnDemand
                      && !source.AllowsAutomaticPublication);

        var importedLabels = Assert.Single(
            relation.Sources,
            source => source.SourceKey ==
                      CommunityBoardInformationSourceKeys.MfdsImportedFoodLabels);
        Assert.Contains(
            CommunityBoardInformationPublicationSourceKeys.ChinaImportedFoodRegionBrief,
            importedLabels.PublicationSourceKeys);
        Assert.Contains(
            CommunityBoardInformationPublicationSourceKeys.UnitedStatesImportedFoodStateBrief,
            importedLabels.PublicationSourceKeys);
    }

    [Fact]
    public void PlannedSources_AreNeverReportedAsImplementedOrPeriodic()
    {
        var planned = CommunityBoardInformationRelationCatalog.All
            .SelectMany(relation => relation.Sources)
            .Where(source =>
                source.ConnectorStatus == CommunityBoardInformationConnectorStatuses.Planned)
            .ToArray();

        Assert.NotEmpty(planned);
        Assert.All(planned, source =>
        {
            Assert.False(source.IsConnectorImplemented);
            Assert.False(source.HasPeriodicBatchModule);
            Assert.Equal(
                CommunityBoardInformationBatchStatuses.Planned,
                source.BatchStatus);
            Assert.Equal(
                CommunityBoardInformationPublicationPolicies.NoAutomaticPublication,
                source.PublicationPolicy);
        });
    }

    [Fact]
    public void AutomaticPublication_IsLimitedToExplicitEditorialBoards()
    {
        var automaticBoards = CommunityBoardInformationRelationCatalog.All
            .Where(relation => relation.Sources.Any(source =>
                source.AllowsAutomaticPublication))
            .Select(relation => relation.BoardKey)
            .ToArray();

        Assert.Contains(CommunityBoardKeys.PeriodicDataKamis, automaticBoards);
        Assert.Contains(CommunityBoardKeys.PeriodicDataMfds, automaticBoards);
        Assert.Contains(CommunityBoardKeys.PeriodicDataUsda, automaticBoards);
        Assert.DoesNotContain(CommunityBoardKeys.InformationPrices, automaticBoards);
        Assert.Contains(CommunityBoardKeys.FreeLife, automaticBoards);
        Assert.Contains(CommunityBoardKeys.CompletionReview, automaticBoards);
        Assert.DoesNotContain(CommunityBoardKeys.Participation, automaticBoards);
        Assert.DoesNotContain(CommunityBoardKeys.SalesSupply, automaticBoards);
        Assert.DoesNotContain(CommunityBoardKeys.Cargo, automaticBoards);
        Assert.DoesNotContain(CommunityBoardKeys.SafetyReport, automaticBoards);
    }

    [Fact]
    public void PeriodicBatchRelations_ReturnEachSourceAndOwnerOnlyOnce()
    {
        var periodic = CommunityBoardInformationRelationCatalog.PeriodicBatchRelations();

        Assert.NotEmpty(periodic);
        Assert.Equal(
            periodic.Count,
            periodic
                .Select(source => $"{source.BatchModuleKey}:{source.SourceKey}")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
        Assert.All(periodic, source =>
        {
            Assert.True(source.IsConnectorImplemented);
            Assert.True(source.HasPeriodicBatchModule);
            Assert.False(string.IsNullOrWhiteSpace(source.UpdateCycle));
            Assert.False(string.IsNullOrWhiteSpace(source.Limitations));
        });
    }

    [Fact]
    public void PeriodicBatchPlans_CollectTargetBoardsWithoutEnablingExecution()
    {
        var plans = CommunityBoardInformationRelationCatalog.PeriodicBatchPlans();
        var kamis = Assert.Single(
            plans,
            plan => plan.SourceKey == CommunityInformationSourceKeys.KamisPriceObservations);
        var importedLabels = Assert.Single(
            plans,
            plan => plan.SourceKey ==
                    CommunityBoardInformationSourceKeys.MfdsImportedFoodLabels);

        Assert.Contains(CommunityBoardKeys.InformationPrices, kamis.BoardKeys);
        Assert.Contains(CommunityBoardKeys.Food, kamis.BoardKeys);
        Assert.Contains(CommunityBoardKeys.Participation, kamis.BoardKeys);
        Assert.Equal(CommunityBoardKeys.PeriodicDataKamis, kamis.CanonicalBoardKey);
        Assert.True(kamis.AllowsAutomaticPublication);
        Assert.True(kamis.RequiresExplicitActivation);

        Assert.Contains(
            CommunityBoardInformationPublicationSourceKeys.ChinaImportedFoodRegionBrief,
            importedLabels.PublicationSourceKeys);
        Assert.Contains(
            CommunityBoardInformationPublicationSourceKeys.UnitedStatesImportedFoodStateBrief,
            importedLabels.PublicationSourceKeys);
        Assert.Equal(CommunityBoardKeys.PeriodicDataMfds, importedLabels.CanonicalBoardKey);
        Assert.True(importedLabels.RequiresExplicitActivation);
    }

    [Fact]
    public void PeriodicDataCatalog_UsesOneCanonicalBoardAndRelatedBoardsOnlyAsLinks()
    {
        Assert.Equal(
            CommunityBoardKeys.PeriodicDataUsda,
            CommunityPeriodicDataBoardCatalog.CanonicalBoardKeyForSource(
                CommunityInformationSourceKeys.UsdaNassPriceObservations));
        Assert.Equal(
            CommunityBoardKeys.PeriodicDataMfds,
            CommunityPeriodicDataBoardCatalog.CanonicalBoardKeyForPublicationSource(
                CommunityBoardInformationPublicationSourceKeys.ChinaImportedFoodRegionBrief));
        Assert.Equal(
            CommunityBoardKeys.PeriodicDataCustomsImportUnitPrice,
            CommunityPeriodicDataBoardCatalog.CanonicalBoardKeyForSource(
                Hs공공데이터출처Keys.수입평균단가));

        var informationGuides = CommunityPeriodicDataBoardCatalog.ForRelatedBoard(
            CommunityBoardKeys.InformationPrices);
        Assert.Equal(4, informationGuides.Count);
        Assert.Empty(CommunityPeriodicDataBoardCatalog.ForRelatedBoard(
            CommunityBoardKeys.PeriodicDataMfds));
    }

    [Fact]
    public void BoardsWithoutSafeDataRelationships_RemainExplicitlyEmpty()
    {
        var vow = CommunityBoardInformationRelationCatalog.Find(CommunityBoardKeys.Vow);
        var feedback = CommunityBoardInformationRelationCatalog.Find(
            CommunityBoardKeys.ProductFeedback);
        var safety = CommunityBoardInformationRelationCatalog.Find(
            CommunityBoardKeys.SafetyReport);

        Assert.NotNull(vow);
        Assert.NotNull(feedback);
        Assert.NotNull(safety);
        Assert.Empty(vow.Sources);
        Assert.Empty(feedback.Sources);
        Assert.Empty(safety.Sources);
        Assert.Contains("사용자 글", vow.AutomationBoundary);
        Assert.Contains("보호 기록", safety.AutomationBoundary);
    }

    [Fact]
    public void Catalog_ExcludesYouTubeFromEveryBoardAndBatchPlan()
    {
        Assert.DoesNotContain(
            CommunityBoardInformationRelationCatalog.All.SelectMany(
                relation => relation.Sources),
            source => source.SourceKey == CommunityInformationSourceKeys.YouTubeChannelVideos);
        Assert.DoesNotContain(
            CommunityBoardInformationRelationCatalog.PeriodicBatchPlans(),
            plan => plan.SourceKey == CommunityInformationSourceKeys.YouTubeChannelVideos);
    }
}
