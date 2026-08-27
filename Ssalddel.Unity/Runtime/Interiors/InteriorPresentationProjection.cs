using System;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Interior.Contracts;

namespace Ssalddel.Unity.Data.Interiors
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.ViewModel,
        "고정된 실내 계획을 LH Focus 상세도에 맞는 Unity VisualKey와 Reference 카드로 투영한다.",
        StepKey = "unity.interior-plan-presentation",
        DependsOnStepKeys = new[] { "application.lh-interior-plan-handle" },
        ExecutionStage = SsalddelCodeExecutionStage.Presentation,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        WritesTo = SsalddelCodeDataScope.ClientPresentation,
        Effects = SsalddelCodeEffect.UiStateMutation,
        FlowOrder = 40,
        Boundary = "Pinned hash 불일치는 닫고 Unity는 Plan·상품 승인·Simulation 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E7,
        "실내 계획을 플레이어 Focus에 따른 Unity 표현으로 투영한다.",
        WorkOrderIds = new[] { "E9-WO-TOWN-HOUSE-INTERIOR-LAYOUT" },
        Boundary = "Projection 코드와 단위 시험은 저장 Scene·Play Mode·Game View의 실제 플레이 증거가 아니다.")]
    public sealed class InteriorPresentationProjection
    {
        public const string ReferenceBoundaryNotice =
            "현실 상품 관측을 기반으로 한 공간 참고 자료이며 게임 재고나 소유 물품이 아닙니다.";

        public InteriorPresentationSnapshot Project(
            InteriorPlacementPlan plan,
            InteriorPlanHandle handle,
            ApprovedInteriorReferenceCatalog catalog,
            string focusLevelCode)
        {
            if (plan is null) throw new ArgumentNullException(nameof(plan));
            if (handle is null) throw new ArgumentNullException(nameof(handle));
            if (catalog is null) throw new ArgumentNullException(nameof(catalog));
            if (!string.Equals(
                    plan.BuildingPlacementStableId,
                    handle.BuildingPlacementStableId,
                    StringComparison.Ordinal)
                || !string.Equals(
                    plan.InteriorPlacementPlanHashSha256,
                    handle.InteriorPlacementPlanHashSha256,
                    StringComparison.Ordinal)
                || !string.Equals(
                    plan.InteriorDefinitionRevision,
                    handle.InteriorDefinitionRevision,
                    StringComparison.Ordinal)
                || !string.Equals(
                    plan.ReferenceCatalogRevision,
                    handle.ReferenceCatalogRevision,
                    StringComparison.Ordinal)
                || !string.Equals(
                    catalog.Revision,
                    handle.ReferenceCatalogRevision,
                    StringComparison.Ordinal)
                || !string.Equals(
                    catalog.CatalogHashSha256,
                    handle.ReferenceCatalogHashSha256,
                    StringComparison.Ordinal))
            {
                return InteriorPresentationSnapshot.Unavailable(
                    handle.BuildingPlacementStableId,
                    "PinnedInteriorPlanMismatch");
            }

            var objectFocus = string.Equals(
                focusLevelCode,
                InteriorLayoutCodes.ObjectFocus,
                StringComparison.Ordinal);
            var zoneFocus = objectFocus || string.Equals(
                focusLevelCode,
                InteriorLayoutCodes.ZoneFocus,
                StringComparison.Ordinal);
            var placements = plan.Placements
                .Where(value => objectFocus
                                || (zoneFocus
                                    && (value.PlacementLayerCode == InteriorLayoutCodes.Fixture
                                        || value.PlacementLayerCode == InteriorLayoutCodes.Surface)))
                .OrderBy(value => value.PlacementStableId, StringComparer.Ordinal)
                .Select(value => new InteriorVisualPlacement
                {
                    PlacementStableId = value.PlacementStableId,
                    ParentPlacementStableId = value.ParentPlacementStableId,
                    ZoneStableId = value.ZoneStableId,
                    PlacementLayerCode = value.PlacementLayerCode,
                    VisualKey = value.VisualKey,
                    LocalPosition = value.LocalPosition,
                    LocalRotationDegrees = value.LocalRotationDegrees,
                    ReferenceStableId = value.ReferenceStableId,
                })
                .ToArray();
            var visibleReferenceIds = placements
                .Select(value => value.ReferenceStableId)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToHashSet(StringComparer.Ordinal);

            return new InteriorPresentationSnapshot
            {
                BuildingPlacementStableId = plan.BuildingPlacementStableId,
                FocusLevelCode = focusLevelCode,
                InteriorRootActive = objectFocus,
                ZoneSummaryActive = zoneFocus,
                SmallItemsActive = objectFocus,
                IsAvailable = true,
                VisualPlacements = placements,
                ReferenceCards = catalog.Items
                    .Where(value => visibleReferenceIds.Contains(value.ReferenceStableId))
                    .OrderBy(value => value.ReferenceStableId, StringComparer.Ordinal)
                    .Select(value => new InteriorReferenceCard
                    {
                        ReferenceStableId = value.ReferenceStableId,
                        MarketplaceCode = value.MarketplaceCode,
                        ApprovedOriginalTitle = value.ApprovedOriginalTitle,
                        SourceUrl = value.SourceUrl,
                        ObservedAtUtc = value.ObservedAtUtc,
                        BoundaryNotice = ReferenceBoundaryNotice,
                    })
                    .ToArray(),
            };
        }
    }

    public sealed class InteriorPresentationSnapshot
    {
        public string BuildingPlacementStableId { get; set; } = string.Empty;
        public string FocusLevelCode { get; set; } = InteriorLayoutCodes.OverviewFocus;
        public bool InteriorRootActive { get; set; }
        public bool ZoneSummaryActive { get; set; }
        public bool SmallItemsActive { get; set; }
        public bool IsAvailable { get; set; }
        public string UnavailableReasonCode { get; set; } = string.Empty;
        public InteriorVisualPlacement[] VisualPlacements { get; set; }
            = Array.Empty<InteriorVisualPlacement>();
        public InteriorReferenceCard[] ReferenceCards { get; set; }
            = Array.Empty<InteriorReferenceCard>();

        public static InteriorPresentationSnapshot Unavailable(string buildingStableId, string reasonCode)
            => new()
            {
                BuildingPlacementStableId = buildingStableId,
                IsAvailable = false,
                UnavailableReasonCode = reasonCode,
            };
    }

    public sealed class InteriorVisualPlacement
    {
        public string PlacementStableId { get; set; } = string.Empty;
        public string ParentPlacementStableId { get; set; } = string.Empty;
        public string ZoneStableId { get; set; } = string.Empty;
        public string PlacementLayerCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public InteriorVector3 LocalPosition { get; set; } = new();
        public double LocalRotationDegrees { get; set; }
        public string ReferenceStableId { get; set; } = string.Empty;
    }

    public sealed class InteriorReferenceCard
    {
        public string ReferenceStableId { get; set; } = string.Empty;
        public string MarketplaceCode { get; set; } = string.Empty;
        public string ApprovedOriginalTitle { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string ObservedAtUtc { get; set; } = string.Empty;
        public string BoundaryNotice { get; set; } = string.Empty;
    }
}
