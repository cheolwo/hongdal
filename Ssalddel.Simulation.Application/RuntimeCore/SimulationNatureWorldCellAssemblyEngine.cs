using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// LH가 준비한 셀·지면과 Nature 권위 상태를 읽어 실외·실내 배치 계획을
    /// 같은 세계 개정 번호로 묶는다. LH와 Unity는 이 규칙을 다시 구현하지 않는다.
    /// </summary>
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Application,
        "LH 셀과 Nature 상태에서 독립 실외·실내 배치 인계 자료를 조립한다.",
        StepKey = "application.nature-world-cell-assembly",
        DependsOnStepKeys = new[] { "contract.world-asset-placement" },
        ExecutionStage = SsalddelCodeExecutionStage.Projection,
        ReadsFrom = SsalddelCodeDataScope.SimulationState |
                    SsalddelCodeDataScope.DerivedWorld,
        FlowOrder = 31,
        Boundary = "지면·셀은 LH, Prefab 생명주기는 Unity 배치 Runtime이 소유하며 이 엔진은 권위 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E5,
        "Nature 권위 결과를 같은 revision의 실외·실내 셀 조립 계획으로 결속한다.",
        Boundary = "배치 계획은 실제 Unity 표현 또는 플레이 입력 증거가 아니다.",
        WorkOrderIds = new[] { "E9-WO-WORLD-MAP-ASSET-PLACEMENT" })]
    public sealed class SimulationNatureWorldCellAssemblyEngine
    {
        public const int DefaultNatureOwnerL3X = 2800;
        public const int DefaultNatureOwnerL3Y = 4581;
        private const string CabinInteriorRevision =
            "nature-cabin-interior.fixed.r1";
        private const string CabinReferenceRevision =
            "nature-cabin-visual-reference.r1";
        private const string CabinMetricRevision =
            "nature-cabin-visual-metric.r1";

        public SimulationWorldCellAssemblyResponse Compose(
            SimulationLhCellPlanResponse lhCell,
            SimulationNatureSurvivalStateSnapshot nature,
            long worldRevision,
            bool ownsNatureAuthority,
            string surfaceModeCode = "",
            string surfaceStateHashSha256 = "",
            double ownerLocalOriginXMeters = 0d,
            double ownerLocalOriginZMeters = 0d)
        {
            Validate(lhCell, nature, worldRevision,
                surfaceStateHashSha256);

            var exteriorPlacements = AmbientNature(lhCell).ToList();
            if (ownsNatureAuthority && nature.IsEnabled)
                AddAuthoritativeNature(exteriorPlacements, lhCell.CellKey,
                    nature, ownerLocalOriginXMeters,
                    ownerLocalOriginZMeters);
            var exterior = CreateExterior(lhCell, worldRevision,
                surfaceStateHashSha256, exteriorPlacements);
            var interior = CreateInterior(lhCell, nature, worldRevision,
                ownsNatureAuthority);
            var response = new SimulationWorldCellAssemblyResponse
            {
                CellStableId = lhCell.CellKey,
                SourceWorldRevision = worldRevision,
                SourceLhBasePlanHashSha256 = lhCell.BasePlanHashSha256,
                SurfaceModeCode = surfaceModeCode ?? string.Empty,
                SurfaceStateHashSha256 = surfaceStateHashSha256
                    ?? string.Empty,
                ExteriorPlacement = exterior,
                InteriorPlacement = interior,
                IsAvailable = true,
            };
            response.AssemblyHashSha256 = Simulation세계자산CanonicalHash.Hash(
                string.Join("|", new[]
                {
                    response.SchemaVersion,
                    response.CellStableId,
                    response.SourceWorldRevision.ToString(
                        CultureInfo.InvariantCulture),
                    response.SourceLhBasePlanHashSha256,
                    response.SurfaceModeCode,
                    response.SurfaceStateHashSha256,
                    exterior.ExteriorPlacementPlanHashSha256,
                    interior.InteriorPlacementPlanHashSha256,
                }));
            return response;
        }

        private static IEnumerable<Simulation세계자산PlacementSnapshot>
            AmbientNature(SimulationLhCellPlanResponse cell)
            => (cell.Placements ?? Array.Empty<SimulationLhPlacementResponse>())
                .Where(value => !string.IsNullOrWhiteSpace(
                                    value.CompositionKey)
                                && value.CompositionKey.StartsWith(
                                    "nature:", StringComparison.Ordinal))
                .OrderBy(value => value.GeneratedStableId,
                    StringComparer.Ordinal)
                .Select(value => new Simulation세계자산PlacementSnapshot
                {
                    PlacementStableId = "ambient:" +
                        value.GeneratedStableId,
                    OwnerCellStableId = cell.CellKey,
                    PlacementKindCode =
                        Simulation세계자산배치Codes.Environment,
                    LayerCode = value.LayerCode,
                    CategoryCode = "NatureAmbient",
                    CompositionKey = value.CompositionKey,
                    H1StableId = value.H1StableId,
                    AuthorityKindCode =
                        Simulation세계자산배치Codes.AmbientPresentation,
                    PersistenceKindCode =
                        Simulation세계자산배치Codes.Transient,
                    LocalXMeters = value.LocalXMeters,
                    LocalZMeters = value.LocalZMeters,
                    RotationDegrees = value.RotationDegrees,
                    UniformScale = value.UniformScale,
                    FixedAnchor = value.FixedAnchor,
                    CollisionEligible = value.CollisionEligible,
                    PresentationOnly = true,
                });

        private static void AddAuthoritativeNature(
            ICollection<Simulation세계자산PlacementSnapshot> target,
            string cellStableId,
            SimulationNatureSurvivalStateSnapshot nature,
            double ownerLocalOriginXMeters,
            double ownerLocalOriginZMeters)
        {
            foreach (var node in (nature.ResourceNodes
                                  ?? Array.Empty<
                                      SimulationNatureResourceNodeSnapshot>())
                         .OrderBy(value => value.ResourceNodeStableId,
                             StringComparer.Ordinal))
            {
                var standing = string.Equals(node.StateCode,
                    SimulationNatureSurvivalCodes.Standing,
                    StringComparison.Ordinal);
                target.Add(Placement(cellStableId,
                    "nature-resource:" + node.ResourceNodeStableId,
                    "NatureResourceNode",
                    standing ? "Nature.Tree.Standing"
                        : "Nature.Tree.Stump",
                    node.H1StableId, node.StateCode,
                    ownerLocalOriginXMeters + node.LocalX, 0d,
                    ownerLocalOriginZMeters + node.LocalZ,
                    0d, 1d, node.ResourceNodeStableId));
            }

            foreach (var timber in (nature.DroppedTimber
                                    ?? Array.Empty<
                                        SimulationNatureDroppedTimberSnapshot>())
                         .Where(value => string.Equals(value.StateCode,
                             SimulationNatureSurvivalCodes
                                 .DroppedTimberAvailable,
                             StringComparison.Ordinal))
                         .OrderBy(value => value.DroppedTimberStableId,
                             StringComparer.Ordinal))
                target.Add(Placement(cellStableId,
                    "nature-dropped-timber:" +
                    timber.DroppedTimberStableId,
                    "NatureDroppedTimber", "Nature.Timber.Dropped",
                    timber.H1StableId, timber.StateCode,
                    ownerLocalOriginXMeters + timber.LocalX, 0d,
                    ownerLocalOriginZMeters + timber.LocalZ, 0d,
                    Math.Max(1d, Math.Min(1.4d,
                        1d + timber.Quantity * .08d)),
                    timber.DroppedTimberStableId));

            var cabin = nature.Cabin;
            if (cabin != null
                && !string.IsNullOrWhiteSpace(cabin.CabinStableId)
                && !string.Equals(cabin.StateCode,
                    SimulationNatureSurvivalCodes.Planned,
                    StringComparison.Ordinal))
                target.Add(Placement(cellStableId,
                    CabinPlacementId(cabin.CabinStableId), "NatureCabin",
                    string.Equals(cabin.StateCode,
                        SimulationNatureSurvivalCodes.Completed,
                        StringComparison.Ordinal)
                        ? "Nature.Cabin.Completed"
                        : "Nature.Cabin.Construction",
                    cabin.H1StableId, cabin.StateCode,
                    ownerLocalOriginXMeters + cabin.LocalX, 0d,
                    ownerLocalOriginZMeters + cabin.LocalZ,
                    cabin.YawDegrees, 1d,
                    cabin.CabinStableId));

            foreach (var building in nature.BuildingProgression?.Nodes
                         ?? Array.Empty<Simulation건물발전NodeSnapshot>())
            {
                if (!string.Equals(building.BlueprintStableId,
                        Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                        StringComparison.Ordinal)
                    || (building.StateCode !=
                        Simulation영역건물발전Codes.Building
                        && building.StateCode !=
                        Simulation영역건물발전Codes.Operational))
                    continue;
                target.Add(Placement(cellStableId,
                    "nature-building:" + building.FacilityStableId,
                    "NatureWorkbench",
                    building.StateCode ==
                    Simulation영역건물발전Codes.Operational
                        ? "Nature.Workbench.Operational"
                        : "Nature.Workbench.Construction",
                    building.H1StableId, building.StateCode,
                    ownerLocalOriginXMeters + building.LocalX, 0d,
                    ownerLocalOriginZMeters + building.LocalZ,
                    building.YawDegrees, 1d, building.FacilityStableId));
            }
        }

        private static Simulation세계자산PlacementSnapshot Placement(
            string cellStableId,
            string placementStableId,
            string categoryCode,
            string compositionKey,
            string h1StableId,
            string stateCode,
            double x,
            double y,
            double z,
            double rotation,
            double scale,
            string sourceStableId)
            => new Simulation세계자산PlacementSnapshot
            {
                PlacementStableId = placementStableId,
                OwnerCellStableId = cellStableId,
                PlacementKindCode = categoryCode == "NatureCabin"
                    || categoryCode == "NatureWorkbench"
                    ? Simulation세계자산배치Codes.Building
                    : Simulation세계자산배치Codes.Environment,
                LayerCode = "NatureAuthorityProjection",
                CategoryCode = categoryCode,
                CompositionKey = compositionKey,
                H1StableId = h1StableId ?? string.Empty,
                AuthorityKindCode =
                    Simulation세계자산배치Codes.SimulationEntity,
                PersistenceKindCode =
                    Simulation세계자산배치Codes.DerivedPersistent,
                StateCode = stateCode ?? string.Empty,
                LocalXMeters = x,
                LocalYMeters = y,
                LocalZMeters = z,
                RotationDegrees = rotation,
                UniformScale = scale,
                FixedAnchor = true,
                CollisionEligible = true,
                PresentationOnly = true,
                SourceChangeStableIds = string.IsNullOrWhiteSpace(
                    sourceStableId) ? Array.Empty<string>()
                    : new[] { sourceStableId },
            };

        private static Simulation실외자산배치Plan CreateExterior(
            SimulationLhCellPlanResponse cell,
            long worldRevision,
            string surfaceHash,
            IEnumerable<Simulation세계자산PlacementSnapshot> placements)
        {
            var ordered = placements.OrderBy(value =>
                value.PlacementStableId, StringComparer.Ordinal).ToArray();
            var result = new Simulation실외자산배치Plan
            {
                CellStableId = cell.CellKey,
                SourceWorldRevision = worldRevision,
                SourceLhBasePlanHashSha256 = cell.BasePlanHashSha256,
                SourceSurfaceStateHashSha256 = surfaceHash ?? string.Empty,
                SourceCombinedPlanHashSha256 = cell.BasePlanHashSha256,
                Placements = ordered,
            };
            result.ExteriorPlacementPlanHashSha256 =
                Simulation세계자산CanonicalHash.Hash(string.Join("|",
                    new[]
                    {
                        result.SchemaVersion,
                        result.RuleRevision,
                        result.CellStableId,
                        result.SourceWorldRevision.ToString(
                            CultureInfo.InvariantCulture),
                        result.SourceLhBasePlanHashSha256,
                        result.SourceSurfaceStateHashSha256,
                        string.Join(",", ordered.Select(
                            PlacementCanonical)),
                    }));
            return result;
        }

        private static Simulation실내자산배치Plan CreateInterior(
            SimulationLhCellPlanResponse cell,
            SimulationNatureSurvivalStateSnapshot nature,
            long worldRevision,
            bool ownsNatureAuthority)
        {
            var bodies = new List<
                SimulationInteriorPlacementPlanBodySnapshot>();
            var handles = new List<SimulationInteriorPlanHandleSnapshot>();
            var overlays = new List<Simulation세계자산PlacementSnapshot>();
            var cabin = nature.Cabin;
            if (ownsNatureAuthority && nature.IsEnabled && cabin != null
                && string.Equals(cabin.StateCode,
                    SimulationNatureSurvivalCodes.Completed,
                    StringComparison.Ordinal))
            {
                var body = CabinBody(cabin);
                bodies.Add(body);
                handles.Add(CabinHandle(body));
                if (nature.StoredTimberQuantity > 0)
                    overlays.Add(StoredTimberOverlay(cell.CellKey, cabin,
                        nature.StoredTimberQuantity));
            }

            var result = new Simulation실내자산배치Plan
            {
                CellStableId = cell.CellKey,
                SourceWorldRevision = worldRevision,
                SourceLhBasePlanHashSha256 = cell.BasePlanHashSha256,
                SourceCombinedPlanHashSha256 = cell.BasePlanHashSha256,
                InteriorPlanHandles = handles.ToArray(),
                InteriorPlanBodies = bodies.ToArray(),
                OverlayPlacements = overlays.ToArray(),
            };
            result.InteriorPlacementPlanHashSha256 =
                Simulation세계자산CanonicalHash.Hash(string.Join("|",
                    new[]
                    {
                        result.SchemaVersion,
                        result.RuleRevision,
                        result.CellStableId,
                        result.SourceWorldRevision.ToString(
                            CultureInfo.InvariantCulture),
                        result.SourceLhBasePlanHashSha256,
                        string.Join(",", handles.Select(value =>
                            value.InteriorPlacementPlanHashSha256)),
                        string.Join(",", bodies.Select(value =>
                            value.BodyHashSha256)),
                        string.Join(",", overlays.Select(
                            PlacementCanonical)),
                    }));
            return result;
        }

        private static SimulationInteriorPlacementPlanBodySnapshot CabinBody(
            SimulationNatureCabinSnapshot cabin)
        {
            var buildingId = CabinPlacementId(cabin.CabinStableId);
            var h1 = cabin.H1StableId ?? string.Empty;
            var items = new[]
            {
                InteriorItem(buildingId + ":zone:rest", string.Empty,
                    "zone:nature-cabin:rest", h1, "Zone", "RestZone",
                    string.Empty, -1d, 0d, -1.2d),
                InteriorItem(buildingId + ":bed", string.Empty,
                    "zone:nature-cabin:rest", h1, "Fixture", "Sleep",
                    "Nature.Shelter.Bedroll", -1d, 0d, -1.2d),
                InteriorItem(buildingId + ":zone:storage", string.Empty,
                    "zone:nature-cabin:storage", h1, "Zone", "StorageZone",
                    string.Empty, 1.35d, 0d, .75d),
                InteriorItem(buildingId + ":storage-rack", string.Empty,
                    "zone:nature-cabin:storage", h1, "Surface", "Storage",
                    "Nature.Storage.Rack.Small", 1.35d, 0d, .75d, 90d),
                InteriorItem(buildingId + ":zone:work", string.Empty,
                    "zone:nature-cabin:work", h1, "Zone", "WorkZone",
                    string.Empty, 1.1d, 0d, -1d),
                InteriorItem(buildingId + ":work-table", string.Empty,
                    "zone:nature-cabin:work", h1, "Fixture", "Work",
                    "Nature.Work.Table.Small", 1.1d, 0d, -1d, 180d),
            };
            var body = new SimulationInteriorPlacementPlanBodySnapshot
            {
                BuildingPlacementStableId = buildingId,
                H1StableId = h1,
                SourceInteriorPlanSchemaVersion =
                    SimulationInteriorPlanHandleCodes.SchemaVersion,
                InteriorPlacementPlanHashSha256 =
                    Simulation세계자산CanonicalHash.Hash(string.Join("|",
                        new[]
                        {
                            CabinInteriorRevision, buildingId, h1,
                            string.Join(",", items.Select(value =>
                                value.PlacementStableId + ":" +
                                value.VisualKey)),
                        })),
                Placements = items,
            };
            body.BodyHashSha256 = Simulation세계자산CanonicalHash
                .ComputeInteriorBodyHash(body);
            return body;
        }

        private static SimulationInteriorPlacementBodyItemSnapshot
            InteriorItem(string id, string parentId, string zoneId,
                string h1, string layer, string role, string visualKey,
                double x, double y, double z, double rotation = 0d)
            => new SimulationInteriorPlacementBodyItemSnapshot
            {
                PlacementStableId = id,
                ParentPlacementStableId = parentId,
                ZoneStableId = zoneId,
                OwningH1StableId = h1,
                PlacementLayerCode = layer,
                PlacementRoleCode = role,
                VisualKey = visualKey,
                LocalX = x,
                LocalY = y,
                LocalZ = z,
                LocalRotationDegrees = rotation,
                UniformScale = 1d,
                ReferenceStableId = visualKey,
            };

        private static SimulationInteriorPlanHandleSnapshot CabinHandle(
            SimulationInteriorPlacementPlanBodySnapshot body)
        {
            var referenceHash = Simulation세계자산CanonicalHash.Hash(
                CabinReferenceRevision);
            var metricHash = Simulation세계자산CanonicalHash.Hash(
                CabinMetricRevision);
            return new SimulationInteriorPlanHandleSnapshot
            {
                SchemaVersion =
                    SimulationInteriorPlanHandleCodes.SchemaVersion,
                BuildingPlacementStableId =
                    body.BuildingPlacementStableId,
                H1StableId = body.H1StableId,
                InteriorDefinitionRevision = CabinInteriorRevision,
                ReferenceCatalogRevision = CabinReferenceRevision,
                ReferenceCatalogHashSha256 = referenceHash,
                PlacementControlRuleRevision =
                    SimulationInteriorPlanHandleCodes
                        .PlacementControlRuleRevision,
                VisualMetricCatalogRevision = CabinMetricRevision,
                VisualMetricCatalogHashSha256 = metricHash,
                AdjustmentRevision = "nature-cabin-adjustment.r1",
                InteriorPlacementPlanHashSha256 =
                    body.InteriorPlacementPlanHashSha256,
            };
        }

        private static Simulation세계자산PlacementSnapshot
            StoredTimberOverlay(string cellStableId,
                SimulationNatureCabinSnapshot cabin, int quantity)
        {
            var buildingId = CabinPlacementId(cabin.CabinStableId);
            return new Simulation세계자산PlacementSnapshot
            {
                PlacementStableId = buildingId + ":stored-timber",
                ParentPlacementStableId = buildingId + ":storage-rack",
                OwnerCellStableId = cellStableId,
                PlacementKindCode =
                    Simulation세계자산배치Codes.InteriorOverlay,
                LayerCode = "LooseItem",
                CategoryCode = "StoredMaterialOverlay",
                CompositionKey = quantity >= 14
                    ? "Nature.Storage.Timber.Full"
                    : quantity >= 7 ? "Nature.Storage.Timber.Half"
                        : "Nature.Storage.Timber.Small",
                H1StableId = cabin.H1StableId,
                AuthorityKindCode =
                    Simulation세계자산배치Codes.DerivedWorldProp,
                PersistenceKindCode =
                    Simulation세계자산배치Codes.DerivedPersistent,
                StateCode = "Stored",
                UniformScale = Math.Max(.8d,
                    Math.Min(1.35d, .75d + quantity / 20d)),
                PresentationOnly = true,
                SourceChangeStableIds = new[]
                {
                    SimulationNatureSurvivalCodes
                        .CabinStorageTimberStackStableId,
                },
            };
        }

        private static string CabinPlacementId(string cabinStableId)
            => "nature-cabin:" + (cabinStableId ?? string.Empty).Trim();

        private static string PlacementCanonical(
            Simulation세계자산PlacementSnapshot value)
            => string.Join(":", new[]
            {
                value.PlacementStableId,
                value.ParentPlacementStableId,
                value.OwnerCellStableId,
                value.PlacementKindCode,
                value.CategoryCode,
                value.CompositionKey,
                value.H1StableId,
                value.AuthorityKindCode,
                value.PersistenceKindCode,
                value.StateCode,
                value.LocalXMeters.ToString("R",
                    CultureInfo.InvariantCulture),
                value.LocalYMeters.ToString("R",
                    CultureInfo.InvariantCulture),
                value.LocalZMeters.ToString("R",
                    CultureInfo.InvariantCulture),
                value.RotationDegrees.ToString("R",
                    CultureInfo.InvariantCulture),
                value.UniformScale.ToString("R",
                    CultureInfo.InvariantCulture),
                value.FixedAnchor.ToString(),
                value.CollisionEligible.ToString(),
                string.Join(",", value.SourceChangeStableIds
                    ?? Array.Empty<string>()),
            });

        private static void Validate(SimulationLhCellPlanResponse cell,
            SimulationNatureSurvivalStateSnapshot nature,
            long worldRevision,
            string surfaceStateHashSha256)
        {
            if (cell == null) throw new ArgumentNullException(nameof(cell));
            if (nature == null)
                throw new ArgumentNullException(nameof(nature));
            if (string.IsNullOrWhiteSpace(cell.CellKey)
                || !IsSha256(cell.BasePlanHashSha256)
                || worldRevision < 0
                || (!string.IsNullOrWhiteSpace(surfaceStateHashSha256)
                    && !IsSha256(surfaceStateHashSha256)))
                throw new ArgumentException(
                    "SimulationNatureWorldCellAssemblyInputInvalid");
        }

        private static bool IsSha256(string value)
            => !string.IsNullOrWhiteSpace(value) && value.Length == 64
               && value.All(Uri.IsHexDigit);
    }
}
