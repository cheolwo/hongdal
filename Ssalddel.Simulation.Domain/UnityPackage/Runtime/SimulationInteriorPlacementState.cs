using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private SimulationInteriorPlanHandleSnapshot[] interiorPlanHandles =
            Array.Empty<SimulationInteriorPlanHandleSnapshot>();

        private bool HasInteriorPlanHandles => interiorPlanHandles.Length > 0;

        private void InitializeInteriorPlanHandles(
            SimulationInteriorPlanHandleSnapshot[]? handles)
        {
            interiorPlanHandles = CloneInteriorPlanHandles(handles);
        }

        private SimulationInteriorPlanHandleSnapshot[]
            CreateInteriorPlanHandleSnapshots()
            => CloneInteriorPlanHandles(interiorPlanHandles);

        internal static SimulationInteriorPlanHandleSnapshot[]
            CloneInteriorPlanHandles(
                SimulationInteriorPlanHandleSnapshot[]? source)
            => (source ?? Array.Empty<SimulationInteriorPlanHandleSnapshot>())
                .Select(CloneInteriorPlanHandle)
                .ToArray();

        internal static SimulationInteriorPlanHandleSnapshot
            CloneInteriorPlanHandle(SimulationInteriorPlanHandleSnapshot source)
        {
            if (source == null)
                throw new SimulationContractException(
                    "SimulationInteriorPlanHandleMissing");
            return new SimulationInteriorPlanHandleSnapshot
            {
                SchemaVersion = source.SchemaVersion,
                BuildingPlacementStableId = source.BuildingPlacementStableId,
                H1StableId = source.H1StableId,
                InteriorDefinitionRevision = source.InteriorDefinitionRevision,
                ReferenceCatalogRevision = source.ReferenceCatalogRevision,
                ReferenceCatalogHashSha256 = source.ReferenceCatalogHashSha256,
                PlacementControlRuleRevision = source.PlacementControlRuleRevision,
                VisualMetricCatalogRevision = source.VisualMetricCatalogRevision,
                VisualMetricCatalogHashSha256 = source.VisualMetricCatalogHashSha256,
                AdjustmentRevision = source.AdjustmentRevision,
                InteriorPlacementPlanHashSha256 =
                    source.InteriorPlacementPlanHashSha256,
            };
        }

        internal static void ValidateInteriorPlanHandles(
            SimulationInteriorPlanHandleSnapshot[]? handles)
        {
            if (handles == null)
                throw new SimulationContractException(
                    "SimulationInteriorPlanHandlesMissing");
            if (handles.Length > 64)
                throw new SimulationContractException(
                    "SimulationInteriorPlanHandleCountInvalid");

            var buildingIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var handle in handles)
            {
                if (handle == null)
                    throw new SimulationContractException(
                        "SimulationInteriorPlanHandleMissing");
                if (!string.Equals(handle.SchemaVersion,
                        SimulationInteriorPlanHandleCodes.SchemaVersion,
                        StringComparison.Ordinal))
                    throw new SimulationContractException(
                        "SimulationInteriorPlanSchemaVersionInvalid");
                if (!string.Equals(handle.PlacementControlRuleRevision,
                        SimulationInteriorPlanHandleCodes
                            .PlacementControlRuleRevision,
                        StringComparison.Ordinal))
                    throw new SimulationContractException(
                        "SimulationInteriorPlacementControlRevisionInvalid");
                RequireStableId(handle.BuildingPlacementStableId,
                    "SimulationInteriorBuildingPlacementStableIdInvalid");
                RequireStableId(handle.H1StableId,
                    "SimulationInteriorH1StableIdInvalid");
                RequireText(handle.InteriorDefinitionRevision,
                    "SimulationInteriorDefinitionRevisionMissing");
                RequireText(handle.ReferenceCatalogRevision,
                    "SimulationInteriorReferenceCatalogRevisionMissing");
                RequireText(handle.VisualMetricCatalogRevision,
                    "SimulationInteriorVisualMetricCatalogRevisionMissing");
                ValidateSha256(handle.ReferenceCatalogHashSha256,
                    "SimulationInteriorReferenceCatalogHashInvalid");
                ValidateSha256(handle.VisualMetricCatalogHashSha256,
                    "SimulationInteriorVisualMetricCatalogHashInvalid");
                ValidateSha256(handle.InteriorPlacementPlanHashSha256,
                    "SimulationInteriorPlacementPlanHashInvalid");
                if (!buildingIds.Add(handle.BuildingPlacementStableId))
                    throw new SimulationContractException(
                        "SimulationInteriorBuildingPlacementDuplicate");
            }
        }

        internal static bool InteriorPlanHandlesEqual(
            SimulationInteriorPlanHandleSnapshot[]? left,
            SimulationInteriorPlanHandleSnapshot[]? right)
        {
            var orderedLeft = (left
                    ?? Array.Empty<SimulationInteriorPlanHandleSnapshot>())
                .OrderBy(value => value.BuildingPlacementStableId,
                    StringComparer.Ordinal).ToArray();
            var orderedRight = (right
                    ?? Array.Empty<SimulationInteriorPlanHandleSnapshot>())
                .OrderBy(value => value.BuildingPlacementStableId,
                    StringComparer.Ordinal).ToArray();
            if (orderedLeft.Length != orderedRight.Length) return false;
            for (var index = 0; index < orderedLeft.Length; index++)
            {
                var a = orderedLeft[index];
                var b = orderedRight[index];
                if (!string.Equals(a.SchemaVersion, b.SchemaVersion,
                        StringComparison.Ordinal)
                    || !string.Equals(a.BuildingPlacementStableId,
                        b.BuildingPlacementStableId, StringComparison.Ordinal)
                    || !string.Equals(a.H1StableId, b.H1StableId,
                        StringComparison.Ordinal)
                    || !string.Equals(a.InteriorDefinitionRevision,
                        b.InteriorDefinitionRevision, StringComparison.Ordinal)
                    || !string.Equals(a.ReferenceCatalogRevision,
                        b.ReferenceCatalogRevision, StringComparison.Ordinal)
                    || !string.Equals(a.ReferenceCatalogHashSha256,
                        b.ReferenceCatalogHashSha256, StringComparison.Ordinal)
                    || !string.Equals(a.PlacementControlRuleRevision,
                        b.PlacementControlRuleRevision, StringComparison.Ordinal)
                    || !string.Equals(a.VisualMetricCatalogRevision,
                        b.VisualMetricCatalogRevision, StringComparison.Ordinal)
                    || !string.Equals(a.VisualMetricCatalogHashSha256,
                        b.VisualMetricCatalogHashSha256, StringComparison.Ordinal)
                    || !string.Equals(a.AdjustmentRevision,
                        b.AdjustmentRevision, StringComparison.Ordinal)
                    || !string.Equals(a.InteriorPlacementPlanHashSha256,
                        b.InteriorPlacementPlanHashSha256,
                        StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        private static void ValidateSha256(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length != 64
                || value.Any(character => !Uri.IsHexDigit(character)))
                throw new SimulationContractException(errorCode);
        }
    }
}
