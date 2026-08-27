using Ssalddel.Interior.Contracts;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Unity.Data.Interiors;

namespace Ssalddel.Unity.Tests;

[SsalddelEvidenceResponsibility(
    SsalddelEvidenceStage.E3,
    "Unity 실내 Focus·Reference 경계와 pinned plan fail-closed를 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3Unity소비자회귀,
    WorkOrderIds = new[] { "E9-WO-TOWN-HOUSE-INTERIOR-LAYOUT" },
    Boundary = "엔진 독립 소비자 시험은 저장 Scene·Play Mode·Game View 증거가 아니다.")]
public sealed class InteriorPresentationProjectionTests
{
    [Fact]
    public void FocusLevelsOnlyExposeTheDetailOwnedByUnityPresentation()
    {
        var plan = Plan();
        var handle = Handle(plan);
        var catalog = Catalog();
        var projection = new InteriorPresentationProjection();

        var overview = projection.Project(plan, handle, catalog, InteriorLayoutCodes.OverviewFocus);
        var zone = projection.Project(plan, handle, catalog, InteriorLayoutCodes.ZoneFocus);
        var detail = projection.Project(plan, handle, catalog, InteriorLayoutCodes.ObjectFocus);

        Assert.Empty(overview.VisualPlacements);
        Assert.False(overview.InteriorRootActive);
        Assert.Single(zone.VisualPlacements);
        Assert.False(zone.SmallItemsActive);
        Assert.Equal(2, detail.VisualPlacements.Length);
        Assert.True(detail.InteriorRootActive);
        Assert.Single(detail.ReferenceCards);
        Assert.Equal(InteriorPresentationProjection.ReferenceBoundaryNotice,
            detail.ReferenceCards[0].BoundaryNotice);
    }

    [Fact]
    public void PinnedPlanMismatchFailsClosed()
    {
        var plan = Plan();
        var handle = Handle(plan);
        handle.InteriorPlacementPlanHashSha256 = new string('f', 64);

        var result = new InteriorPresentationProjection().Project(
            plan,
            handle,
            Catalog(),
            InteriorLayoutCodes.ObjectFocus);

        Assert.False(result.IsAvailable);
        Assert.Equal("PinnedInteriorPlanMismatch", result.UnavailableReasonCode);
        Assert.Empty(result.VisualPlacements);
    }

    private static InteriorPlacementPlan Plan()
        => new()
        {
            BuildingPlacementStableId = "town-building:01",
            InteriorDefinitionRevision = "house.r1",
            ReferenceCatalogRevision = "catalog.r1",
            ReferenceCatalogHashSha256 = "catalog-hash",
            InteriorPlacementPlanHashSha256 = new string('a', 64),
            Placements =
            [
                new InteriorPlacement
                {
                    PlacementStableId = "fixture:desk",
                    PlacementLayerCode = InteriorLayoutCodes.Fixture,
                    VisualKey = "Residential.Table.Work.Small",
                },
                new InteriorPlacement
                {
                    PlacementStableId = "loose:tool",
                    ParentPlacementStableId = "fixture:desk",
                    PlacementLayerCode = InteriorLayoutCodes.LooseItem,
                    VisualKey = "Residential.Work.Tool.Small",
                    ReferenceStableId = "reference:tool",
                },
            ],
        };

    private static InteriorPlanHandle Handle(InteriorPlacementPlan plan)
        => new()
        {
            BuildingPlacementStableId = plan.BuildingPlacementStableId,
            InteriorDefinitionRevision = plan.InteriorDefinitionRevision,
            ReferenceCatalogRevision = plan.ReferenceCatalogRevision,
            ReferenceCatalogHashSha256 = plan.ReferenceCatalogHashSha256,
            InteriorPlacementPlanHashSha256 = plan.InteriorPlacementPlanHashSha256,
        };

    private static ApprovedInteriorReferenceCatalog Catalog()
        => new()
        {
            Revision = "catalog.r1",
            CatalogHashSha256 = "catalog-hash",
            Items =
            [
                new ApprovedInteriorReference
                {
                    ReferenceStableId = "reference:tool",
                    MarketplaceCode = "Amazon",
                    ApprovedOriginalTitle = "Small hand tool",
                    SourceUrl = "https://www.amazon.com/example",
                    ObservedAtUtc = "2026-08-24T00:00:00Z",
                },
            ],
        };
}
