using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Interior.Contracts
{
    public static class InteriorLayoutCodes
    {
        public const string SchemaVersionV1 = "interior-placement-plan.v1";
        public const string SchemaVersionV2 = "interior-placement-plan.v2";
        public const string SchemaVersion = SchemaVersionV1;
        public const string PlacementControlRevisionV2 =
            "placement-control-hierarchy.v2";
        public const string PresentationOnly = "PresentationOnly";
        public const string PlacementAccepted = "PlacementAccepted";
        public const string Structure = "Structure";
        public const string Zone = "Zone";
        public const string Fixture = "Fixture";
        public const string Surface = "Surface";
        public const string LooseItem = "LooseItem";
        public const string InteractionAnchor = "InteractionAnchor";
        public const string Floor = "Floor";
        public const string Wall = "Wall";
        public const string ParentSurface = "ParentSurface";
        public const string Living = "Living";
        public const string Kitchen = "Kitchen";
        public const string Bedroom = "Bedroom";
        public const string Bathroom = "Bathroom";
        public const string Work = "Work";
        public const string OverviewFocus = "OverviewFocus";
        public const string ZoneFocus = "ZoneFocus";
        public const string ObjectFocus = "ObjectFocus";
        public const string UniformScale = "Uniform";
    }

    public sealed class InteriorVector3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public sealed class InteriorSize3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public sealed class InteriorBounds
    {
        public InteriorVector3 Center { get; set; } = new();
        public InteriorSize3 Size { get; set; } = new();
    }

    public sealed class InteriorStructureDefinition
    {
        public string StableId { get; set; } = string.Empty;
        public InteriorBounds UsableBounds { get; set; } = new();
        public InteriorBounds[] ExclusionBounds { get; set; } = Array.Empty<InteriorBounds>();
        public InteriorVector3[] TraversalAnchors { get; set; } = Array.Empty<InteriorVector3>();
    }

    public sealed class InteriorZoneDefinition
    {
        public string StableId { get; set; } = string.Empty;
        public string OwningH1StableId { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public InteriorBounds Bounds { get; set; } = new();
        public string[] RequiredFixtureRoleCodes { get; set; } = Array.Empty<string>();
        public string[] AllowedFixtureRoleCodes { get; set; } = Array.Empty<string>();
        public string[] AllowedLooseItemCategoryCodes { get; set; } = Array.Empty<string>();
        public InteriorVector3[] TraversalAnchors { get; set; } = Array.Empty<InteriorVector3>();
    }

    public sealed class InteriorDefinition
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public InteriorStructureDefinition Structure { get; set; } = new();
        public InteriorZoneDefinition[] Zones { get; set; } = Array.Empty<InteriorZoneDefinition>();
        public InteriorConstraintProfile Constraints { get; set; } = new();
    }

    public sealed class InteriorConstraintProfile
    {
        public double GridStepMeters { get; set; } = 0.5d;
        public double FineAdjustmentStepMeters { get; set; } = 0.05d;
        public double ObjectClearanceMeters { get; set; } = 0.1d;
        public double TraversalClearanceMeters { get; set; } = 0.45d;
        public double MaximumAuthoringAdjustmentMeters { get; set; } = 0.5d;
        public double RotationSnapDegrees { get; set; } = 90d;
    }

    public sealed class InteriorPlacementTransform
    {
        public InteriorVector3 LocalPosition { get; set; } = new();
        public double LocalRotationDegrees { get; set; }
        public double UniformScale { get; set; } = 1d;
    }

    public sealed class InteriorVisualMetric
    {
        public string StableId { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string SourceAssetFingerprintSha256 { get; set; } = string.Empty;
        public InteriorSize3 SourceBoundsSize { get; set; } = new();
        public double MinimumUniformScale { get; set; } = 0.9d;
        public double MaximumUniformScale { get; set; } = 1.1d;
        public double RotationSnapDegrees { get; set; } = 90d;
        public bool RequiresProjectOwnedCollider { get; set; }
    }

    public sealed class InteriorVisualMetricCatalog
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string CatalogHashSha256 { get; set; } = string.Empty;
        public InteriorVisualMetric[] Items { get; set; }
            = Array.Empty<InteriorVisualMetric>();
    }

    public sealed class InteriorPlacementAdjustment
    {
        public string AdjustmentStableId { get; set; } = string.Empty;
        public string PlacementStableId { get; set; } = string.Empty;
        public string ExpectedBasePlanHashSha256 { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
        public InteriorVector3 PositionDelta { get; set; } = new();
        public double RotationDeltaDegrees { get; set; }
        public double UniformScale { get; set; } = 1d;
    }

    public sealed class InteriorSurfaceSlotDefinition
    {
        public string StableId { get; set; } = string.Empty;
        public string[] AllowedPlacementRoleCodes { get; set; } = Array.Empty<string>();
        public string[] AllowedCategoryCodes { get; set; } = Array.Empty<string>();
        public InteriorVector3 LocalPosition { get; set; } = new();
        public InteriorSize3 MaximumSize { get; set; } = new();
        public string DetailLevelCode { get; set; } = "ObjectFocus";
    }

    public sealed class InteriorSurfaceDefinition
    {
        public string StableId { get; set; } = string.Empty;
        public string SupportKindCode { get; set; } = InteriorLayoutCodes.ParentSurface;
        public InteriorVector3 LocalPosition { get; set; } = new();
        public InteriorSurfaceSlotDefinition[] Slots { get; set; }
            = Array.Empty<InteriorSurfaceSlotDefinition>();
    }

    public sealed class InteriorFixtureArchetype
    {
        public string StableId { get; set; } = string.Empty;
        public string FixtureRoleCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public InteriorSize3 Size { get; set; } = new();
        public string[] AllowedZoneRoleCodes { get; set; } = Array.Empty<string>();
        public string SupportKindCode { get; set; } = InteriorLayoutCodes.Floor;
        public InteriorSurfaceDefinition[] Surfaces { get; set; } = Array.Empty<InteriorSurfaceDefinition>();
    }

    public sealed class InteriorLooseItemArchetype
    {
        public string StableId { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string PlacementRoleCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public InteriorSize3 Size { get; set; } = new();
    }

    public sealed class ApprovedInteriorReference
    {
        public string ReferenceStableId { get; set; } = string.Empty;
        public string MarketplaceCode { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string[] RoomRoleCodes { get; set; } = Array.Empty<string>();
        public string[] PlacementRoleCodes { get; set; } = Array.Empty<string>();
        public string ApprovedOriginalTitle { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string ObservedAtUtc { get; set; } = string.Empty;
        public string RawObservationHashSha256 { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public string UsageRestrictionCode { get; set; } = "ReferenceOnly";
    }

    public sealed class ApprovedInteriorReferenceCatalog
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string CatalogHashSha256 { get; set; } = string.Empty;
        public ApprovedInteriorReference[] Items { get; set; } = Array.Empty<ApprovedInteriorReference>();
    }

    public sealed class InteriorLayoutGenerationRequest
    {
        public string SchemaVersion { get; set; } = InteriorLayoutCodes.SchemaVersionV1;
        public string WorldSeed { get; set; } = string.Empty;
        public string BuildingPlacementStableId { get; set; } = string.Empty;
        public string GeneratorRevision { get; set; } = string.Empty;
        public InteriorDefinition Definition { get; set; } = new();
        public ApprovedInteriorReferenceCatalog ReferenceCatalog { get; set; } = new();
        public InteriorFixtureArchetype[] FixtureArchetypes { get; set; }
            = Array.Empty<InteriorFixtureArchetype>();
        public InteriorLooseItemArchetype[] LooseItemArchetypes { get; set; }
            = Array.Empty<InteriorLooseItemArchetype>();
        public string PlacementControlRuleRevision { get; set; } = string.Empty;
        public InteriorVisualMetricCatalog VisualMetricCatalog { get; set; } = new();
        public string AdjustmentRevision { get; set; } = string.Empty;
        public InteriorPlacementAdjustment[] Adjustments { get; set; }
            = Array.Empty<InteriorPlacementAdjustment>();
    }

    public sealed class InteriorZonePlan
    {
        public string ZoneStableId { get; set; } = string.Empty;
        public string OwningH1StableId { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public InteriorBounds Bounds { get; set; } = new();
    }

    public sealed class InteriorPlacement
    {
        public string PlacementStableId { get; set; } = string.Empty;
        public string ParentPlacementStableId { get; set; } = string.Empty;
        public string ZoneStableId { get; set; } = string.Empty;
        public string OwningH1StableId { get; set; } = string.Empty;
        public string PlacementLayerCode { get; set; } = string.Empty;
        public string PlacementRoleCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public InteriorVector3 LocalPosition { get; set; } = new();
        public double LocalRotationDegrees { get; set; }
        public InteriorSize3 Size { get; set; } = new();
        public InteriorPlacementTransform RequestedTransform { get; set; } = new();
        public InteriorPlacementTransform AppliedTransform { get; set; } = new();
        public string VisualMetricStableId { get; set; } = string.Empty;
        public string SourceAssetFingerprintSha256 { get; set; } = string.Empty;
        public string AdjustmentStableId { get; set; } = string.Empty;
        public string[] ValidationCodes { get; set; } = Array.Empty<string>();
        public string ReferenceStableId { get; set; } = string.Empty;
        public string[] PresentationFlags { get; set; } = Array.Empty<string>();
    }

    public sealed class InteriorPlacementPlan
    {
        public string SchemaVersion { get; set; } = InteriorLayoutCodes.SchemaVersion;
        public string BuildingPlacementStableId { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public string InteriorDefinitionRevision { get; set; } = string.Empty;
        public string ReferenceCatalogRevision { get; set; } = string.Empty;
        public string ReferenceCatalogHashSha256 { get; set; } = string.Empty;
        public string GeneratorRevision { get; set; } = string.Empty;
        public string SeedFingerprintSha256 { get; set; } = string.Empty;
        public string PlacementControlRuleRevision { get; set; } = string.Empty;
        public string VisualMetricCatalogRevision { get; set; } = string.Empty;
        public string VisualMetricCatalogHashSha256 { get; set; } = string.Empty;
        public string AdjustmentRevision { get; set; } = string.Empty;
        public string BaseInteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
        public InteriorZonePlan[] Zones { get; set; } = Array.Empty<InteriorZonePlan>();
        public InteriorPlacement[] Placements { get; set; } = Array.Empty<InteriorPlacement>();
        public string[] UnresolvedRequiredFixtureCodes { get; set; } = Array.Empty<string>();
        public bool TraversalValidated { get; set; }
        public string InteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
    }

    public sealed class InteriorPlanHandle
    {
        public string SchemaVersion { get; set; } = InteriorLayoutCodes.SchemaVersionV1;
        public string BuildingPlacementStableId { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public string InteriorDefinitionRevision { get; set; } = string.Empty;
        public string ReferenceCatalogRevision { get; set; } = string.Empty;
        public string ReferenceCatalogHashSha256 { get; set; } = string.Empty;
        public string PlacementControlRuleRevision { get; set; } = string.Empty;
        public string VisualMetricCatalogRevision { get; set; } = string.Empty;
        public string VisualMetricCatalogHashSha256 { get; set; } = string.Empty;
        public string AdjustmentRevision { get; set; } = string.Empty;
        public string InteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
    }

    public sealed class PresentationWorldDefinition
    {
        public string WorldLayoutStableId { get; set; } = string.Empty;
        public int WorldLayoutRevision { get; set; }
        public string WorldLayoutHashSha256 { get; set; } = string.Empty;
        public InteriorPlanHandle[] InteriorPlanHandles { get; set; } = Array.Empty<InteriorPlanHandle>();
    }

    public sealed class InteriorLhCellActivationPlan
    {
        public string CellKey { get; set; } = string.Empty;
        public string FocusLevelCode { get; set; } = InteriorLayoutCodes.OverviewFocus;
        public InteriorPlanHandle[] PlanHandles { get; set; } = Array.Empty<InteriorPlanHandle>();
        public bool LhDeterminesPlacement { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "H 의미와 건물 문맥을 결정적 InteriorPlacementPlan으로 만드는 엔진 계약이다.",
        StepKey = "contract.interior-layout-plan",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 18,
        Boundary = "새 H 단계, Simulation 상태, Prefab 경로, 재고·가격·소유권을 만들지 않는다.")]
    public interface I실내공간조립Engine
    {
        InteriorPlacementPlan Generate(InteriorLayoutGenerationRequest request);
    }
}
