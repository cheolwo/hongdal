using System;
using System.Linq;
using Ssalddel.Interior.Contracts;

namespace Ssalddel.Interior.Domain
{
    /// <summary>
    /// 외부 운송 없이 300 KGM Hub Fixture의 검수·적치·피킹·출고 준비를
    /// 한 건물 안에서 닫는 첫 창고 정밀 배치 문법입니다.
    /// </summary>
    public static class HubWarehouseInteriorGrammar
    {
        public const string GeneratorRevision =
            "hub-warehouse-layout.r1";
        public const string PlacementControlRevision =
            InteriorLayoutCodes.PlacementControlRevisionV2;
        public const string ReceivingH1StableId =
            "h1-stock:hub-receiving-storage";
        public const string OutboundH1StableId =
            "h1-stock:hub-outbound-staging";
        public const string BuildingPlacementStableId =
            "building:sim:pyeongchang:jinbu-hub-warehouse:01";

        public const string InboundZone = "hub-warehouse:zone:inbound";
        public const string InspectionZone = "hub-warehouse:zone:inspection";
        public const string StorageZone = "hub-warehouse:zone:storage";
        public const string PickingZone = "hub-warehouse:zone:picking";
        public const string OutboundZone = "hub-warehouse:zone:outbound";
        public const string CrossAisleZone = "hub-warehouse:zone:cross-aisle";
        public const string StoragePickingAisleZone =
            "hub-warehouse:zone:storage-picking-aisle";

        public const string PalletVisualKey = "Hub.Cargo.Pallet";
        public const string BoxStackVisualKey = "Hub.Cargo.BoxStack";
        public const string StorageShelfVisualKey = "Hub.Storage.Shelf.Small";
        public const string WorkbenchVisualKey = "Hub.Inspection.Workbench";
        public const string RollerTrackVisualKey = "Hub.Picking.RollerTrack";
        public const string HandTrolleyVisualKey = "Hub.Traversal.HandTrolley";
        public const string SafetyBarrierVisualKey = "Hub.Safety.Barrier";

        public static InteriorLayoutGenerationRequest CreateRequest(
            string worldSeed = "hub-independent-warehouse-01",
            InteriorPlacementAdjustment[]? adjustments = null,
            string adjustmentRevision = "")
        {
            var references = new ApprovedInteriorReferenceCatalog
            {
                StableId = "catalog:hub-warehouse:no-marketplace",
                Revision = "hub-warehouse-reference.empty.r1",
            };
            references.CatalogHashSha256 =
                InteriorLayoutHash.ComputeCatalogHash(references);
            var metrics = CreateVisualMetricCatalog();
            return new InteriorLayoutGenerationRequest
            {
                SchemaVersion = InteriorLayoutCodes.SchemaVersionV2,
                WorldSeed = worldSeed,
                BuildingPlacementStableId = BuildingPlacementStableId,
                GeneratorRevision = GeneratorRevision,
                Definition = CreateDefinition(),
                ReferenceCatalog = references,
                FixtureArchetypes = CreateFixtureArchetypes(),
                LooseItemArchetypes = Array.Empty<InteriorLooseItemArchetype>(),
                PlacementControlRuleRevision = PlacementControlRevision,
                VisualMetricCatalog = metrics,
                AdjustmentRevision = adjustmentRevision,
                Adjustments = adjustments ?? Array.Empty<InteriorPlacementAdjustment>(),
            };
        }

        public static InteriorDefinition CreateDefinition()
            => new()
            {
                StableId = "interior:hub-warehouse:jinbu:01",
                Revision = "hub-warehouse-interior.r1",
                H1StableId = ReceivingH1StableId,
                Structure = new InteriorStructureDefinition
                {
                    StableId = "structure:hub-warehouse:25x20",
                    UsableBounds = Bounds(0d, 0d, 24d, 4.1d, 19d),
                    TraversalAnchors = new[]
                    {
                        Point(-8d, -9.25d),
                        Point(8d, -9.25d),
                    },
                },
                Zones = new[]
                {
                    Zone(InboundZone, "HubInboundArea", ReceivingH1StableId,
                        -8d, -7.25d, 7d, 4d, "InboundPallet"),
                    Zone(InspectionZone, "HubInspectionArea", ReceivingH1StableId,
                        0d, -7.25d, 7d, 4d, "InspectionWorkbench"),
                    Zone(OutboundZone, "HubOutboundStagingArea", OutboundH1StableId,
                        8d, -7.25d, 7d, 4d,
                        "OutboundPallet", "SafetyBarrier"),
                    Zone(StorageZone, "HubStorageArea", ReceivingH1StableId,
                        -4d, 3d, 14d, 11d, "StorageShelf"),
                    Zone(PickingZone, "HubPickingArea", OutboundH1StableId,
                        7.75d, 3d, 6.5d, 11d,
                        "PickingRoller", "HandTrolley"),
                    Zone(CrossAisleZone, "HubCrossAisle", ReceivingH1StableId,
                        0d, -4.5d, 22d, 1.5d),
                    Zone(StoragePickingAisleZone, "HubStoragePickingAisle",
                        OutboundH1StableId, 3.75d, 3d, 1.5d, 11d),
                },
                Constraints = new InteriorConstraintProfile
                {
                    GridStepMeters = 0.25d,
                    FineAdjustmentStepMeters = 0.05d,
                    ObjectClearanceMeters = 0.1d,
                    TraversalClearanceMeters = 0.75d,
                    MaximumAuthoringAdjustmentMeters = 0.5d,
                    RotationSnapDegrees = 90d,
                },
            };

        public static InteriorFixtureArchetype[] CreateFixtureArchetypes()
            => new[]
            {
                Fixture("fixture:hub:inbound-pallet", "InboundPallet",
                    PalletVisualKey, 1.44d, 0.23d, 1.42d,
                    "HubInboundArea"),
                Fixture("fixture:hub:inspection-workbench",
                    "InspectionWorkbench", WorkbenchVisualKey,
                    2.74d, 0.95d, 1.21d, "HubInspectionArea"),
                Fixture("fixture:hub:storage-shelf", "StorageShelf",
                    StorageShelfVisualKey, 2.68d, 1.32d, 2.04d,
                    "HubStorageArea"),
                Fixture("fixture:hub:picking-roller", "PickingRoller",
                    RollerTrackVisualKey, 0.51d, 0.66d, 2d,
                    "HubPickingArea"),
                Fixture("fixture:hub:hand-trolley", "HandTrolley",
                    HandTrolleyVisualKey, 0.71d, 1.63d, 0.79d,
                    "HubPickingArea"),
                Fixture("fixture:hub:outbound-pallet", "OutboundPallet",
                    BoxStackVisualKey, 1.44d, 1.55d, 1.42d,
                    "HubOutboundStagingArea"),
                Fixture("fixture:hub:safety-barrier", "SafetyBarrier",
                    SafetyBarrierVisualKey, 0.65d, 1.02d, 1.8d,
                    "HubOutboundStagingArea"),
            };

        public static InteriorVisualMetricCatalog CreateVisualMetricCatalog()
        {
            var metrics = new InteriorVisualMetricCatalog
            {
                StableId = "visual-metric-catalog:hub-warehouse-synty",
                Revision = "synty-hub-visual-metrics.r1",
                Items = CreateFixtureArchetypes()
                    .Select(value => Metric(value.VisualKey, value.Size,
                        value.VisualKey == HandTrolleyVisualKey
                        || value.VisualKey == SafetyBarrierVisualKey
                            ? 15d
                            : 90d))
                    .ToArray(),
            };
            metrics.CatalogHashSha256 =
                InteriorLayoutHash.ComputeVisualMetricCatalogHash(metrics);
            return metrics;
        }

        private static InteriorZoneDefinition Zone(
            string stableId,
            string roleCode,
            string owningH1StableId,
            double x,
            double z,
            double sizeX,
            double sizeZ,
            params string[] fixtureRoles)
            => new()
            {
                StableId = stableId,
                OwningH1StableId = owningH1StableId,
                RoleCode = roleCode,
                Bounds = Bounds(x, z, sizeX, 4.1d, sizeZ),
                RequiredFixtureRoleCodes = fixtureRoles,
                AllowedFixtureRoleCodes = fixtureRoles,
                TraversalAnchors = new[] { Point(x, z) },
            };

        private static InteriorFixtureArchetype Fixture(
            string stableId,
            string roleCode,
            string visualKey,
            double sizeX,
            double sizeY,
            double sizeZ,
            string zoneRoleCode)
            => new()
            {
                StableId = stableId,
                FixtureRoleCode = roleCode,
                VisualKey = visualKey,
                Size = new InteriorSize3
                {
                    X = sizeX,
                    Y = sizeY,
                    Z = sizeZ,
                },
                AllowedZoneRoleCodes = new[] { zoneRoleCode },
            };

        private static InteriorVisualMetric Metric(
            string visualKey,
            InteriorSize3 size,
            double rotationSnap)
            => new()
            {
                StableId = "visual-metric:" + visualKey.ToLowerInvariant()
                    .Replace('.', '-'),
                VisualKey = visualKey,
                SourceAssetFingerprintSha256 =
                    DeterministicInteriorLayoutEngine.Hash(
                        visualKey + "|" + size.X + "|" + size.Y + "|"
                        + size.Z),
                SourceBoundsSize = new InteriorSize3
                {
                    X = size.X,
                    Y = size.Y,
                    Z = size.Z,
                },
                MinimumUniformScale = visualKey == SafetyBarrierVisualKey
                                      || visualKey == HandTrolleyVisualKey
                    ? 0.85d
                    : 0.9d,
                MaximumUniformScale = visualKey == SafetyBarrierVisualKey
                                      || visualKey == HandTrolleyVisualKey
                    ? 1.15d
                    : 1.1d,
                RotationSnapDegrees = rotationSnap,
            };

        private static InteriorBounds Bounds(
            double x,
            double z,
            double sizeX,
            double sizeY,
            double sizeZ)
            => new()
            {
                Center = Point(x, z),
                Size = new InteriorSize3
                {
                    X = sizeX,
                    Y = sizeY,
                    Z = sizeZ,
                },
            };

        private static InteriorVector3 Point(double x, double z)
            => new() { X = x, Z = z };
    }
}
