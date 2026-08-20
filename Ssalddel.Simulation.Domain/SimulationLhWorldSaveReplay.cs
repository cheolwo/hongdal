using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private static readonly HashSet<string> LhDeltaKinds = new(
            new[]
            {
                SimulationLhWorldCodes.DeltaDiscovered,
                SimulationLhWorldCodes.DeltaStateChanged,
                SimulationLhWorldCodes.DeltaPlaced,
                SimulationLhWorldCodes.DeltaRemoved,
            }, StringComparer.Ordinal);

        private SimulationLhWorldStateSnapshot? lhWorldState;

        internal void RestoreLhWorldState(SimulationLhWorldStateSnapshot? value)
        {
            ValidateLhWorldState(value, Revision);
            lhWorldState = SimulationSaveReplayCloner.CloneLhWorld(value);
        }

        private static void ValidateLhWorldState(
            SimulationLhWorldStateSnapshot? value,
            long expectedRevision)
        {
            if (value == null) return;
            if (value.WorldSeed != SimulationLhWorldCodes.WorldSeed
                || value.GeneratorVersion != SimulationLhWorldCodes.GeneratorVersion)
                throw new SimulationContractException(
                    "SimulationLhGeneratorVersionUnsupported");
            if (string.IsNullOrWhiteSpace(value.AreaSetStableId)
                || string.IsNullOrWhiteSpace(value.AreaSetRevision)
                || value.AreaSetBoundaryHashSha256.Length != 64
                || !TryParseL3CellKey(value.LastL3CellKey))
                throw new SimulationContractException("SimulationLhWorldStateInvalid");
            var hasWorldLayout = !string.IsNullOrWhiteSpace(value.WorldLayoutStableId);
            if (hasWorldLayout && (value.WorldLayoutRevision <= 0
                    || value.WorldLayoutHashSha256.Length != 64
                    || (value.PlacementAuthorityCode != SimulationWorldLayoutCodes.ScenarioRelative
                        && value.PlacementAuthorityCode != SimulationWorldLayoutCodes.E6Grounded)
                    || (value.WorldGroundingStateCode != SimulationWorldLayoutCodes.NotApplied
                        && value.WorldGroundingStateCode != SimulationWorldLayoutCodes.Grounded)
                    || (value.WorldGroundingStateCode == SimulationWorldLayoutCodes.Grounded
                        && value.GroundingEvidenceHashSha256.Length != 64)
                    || (value.WorldGroundingStateCode == SimulationWorldLayoutCodes.NotApplied
                        && !string.IsNullOrEmpty(value.GroundingEvidenceHashSha256))))
                throw new SimulationContractException("SimulationLhWorldLayoutProvenanceInvalid");
            if (!hasWorldLayout && (value.WorldLayoutRevision != 0
                    || !string.IsNullOrEmpty(value.WorldLayoutHashSha256)
                    || !string.IsNullOrEmpty(value.PlacementAuthorityCode)
                    || !string.IsNullOrEmpty(value.WorldGroundingStateCode)
                    || !string.IsNullOrEmpty(value.GroundingEvidenceHashSha256)))
                throw new SimulationContractException("SimulationLhWorldLayoutProvenanceInvalid");
            if (value.Deltas == null
                || value.Deltas.Any(delta => delta == null
                    || string.IsNullOrWhiteSpace(delta.GeneratedStableId)
                    || !delta.GeneratedStableId.StartsWith("lh-", StringComparison.Ordinal)
                    || !LhDeltaKinds.Contains(delta.DeltaKindCode)
                    || string.IsNullOrWhiteSpace(delta.StateCode)
                    || delta.AppliedWorldRevision < 0
                    || delta.AppliedWorldRevision > expectedRevision)
                || value.Deltas.Select(delta => delta.GeneratedStableId)
                    .Distinct(StringComparer.Ordinal).Count() != value.Deltas.Length)
                throw new SimulationContractException("SimulationLhWorldDeltaInvalid");
        }

        private static bool TryParseL3CellKey(string value)
        {
            var parts = (value ?? string.Empty).Split(':');
            return parts.Length == 4
                   && parts[0] == "kr5186"
                   && parts[1] == "l3"
                   && int.TryParse(parts[2], out _)
                   && int.TryParse(parts[3], out _);
        }
    }
}
