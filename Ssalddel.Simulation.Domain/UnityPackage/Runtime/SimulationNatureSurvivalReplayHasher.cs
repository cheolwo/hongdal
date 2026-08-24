using System;
using System.Linq;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    internal static partial class SimulationReplayHasher
    {
        private static void AddNatureSurvivalInitialState(StringBuilder canonical,
            SimulationNatureSurvivalInitialStateRequest source)
        {
            Add(canonical, source.ProfileRevision);
            Add(canonical, source.PlayerStableId);
            Add(canonical, source.AreaSetStableId);
            Add(canonical, source.H3StableId);
            Add(canonical, source.SpawnH2StableId);
            Add(canonical, source.SpawnH1StableId);
            Add(canonical, source.InventoryCapacityUnits);
            Add(canonical, source.StartsWithAxe);
            Add(canonical, source.ResourceNodes.Length);
            foreach (var node in source.ResourceNodes.OrderBy(value =>
                         value.ResourceNodeStableId, StringComparer.Ordinal))
            {
                Add(canonical, node.ResourceNodeStableId);
                Add(canonical, node.H2StableId);
                Add(canonical, node.H1StableId);
                Add(canonical, node.LocalX);
                Add(canonical, node.LocalZ);
            }
        }

        private static void AddNatureSurvivalState(StringBuilder canonical,
            SimulationNatureSurvivalStateSnapshot source)
        {
            Add(canonical, source.IsEnabled);
            Add(canonical, source.ProfileRevision);
            Add(canonical, source.PlayerStableId);
            Add(canonical, source.AreaSetStableId);
            Add(canonical, source.H3StableId);
            Add(canonical, source.CurrentH2StableId);
            Add(canonical, source.CurrentH1StableId);
            Add(canonical, source.CycleIndex);
            Add(canonical, source.ElapsedSecondsInCycle);
            Add(canonical, source.ClockPhaseCode);
            Add(canonical, source.ClockPaused);
            Add(canonical, source.PauseReasonCode);
            Add(canonical, source.HasAxe);
            Add(canonical, source.TimberQuantity);
            Add(canonical, source.NoiseEventCount);
            Add(canonical, source.PlayerInsideCabin);
            Add(canonical, source.ResourceNodes.Length);
            foreach (var node in source.ResourceNodes.OrderBy(value =>
                         value.ResourceNodeStableId, StringComparer.Ordinal))
            {
                Add(canonical, node.ResourceNodeStableId);
                Add(canonical, node.H2StableId);
                Add(canonical, node.H1StableId);
                Add(canonical, node.LocalX);
                Add(canonical, node.LocalZ);
                Add(canonical, node.StateCode);
                Add(canonical, node.RegrowsAtCycleIndex);
            }
            Add(canonical, source.ActiveWork == null);
            if (source.ActiveWork != null)
            {
                Add(canonical, source.ActiveWork.WorkKindCode);
                Add(canonical, source.ActiveWork.TargetStableId);
                Add(canonical, source.ActiveWork.RequiredWorkSeconds);
                Add(canonical, source.ActiveWork.CompletedWorkSeconds);
            }
            Add(canonical, source.Cabin.CabinStableId);
            Add(canonical, source.Cabin.H2StableId);
            Add(canonical, source.Cabin.H1StableId);
            Add(canonical, source.Cabin.StateCode);
            Add(canonical, source.Cabin.LocalX);
            Add(canonical, source.Cabin.LocalZ);
            Add(canonical, source.Cabin.YawDegrees);
            Add(canonical, source.Cabin.ReservedTimberQuantity);
            Add(canonical, source.Cabin.CompletedWorkSeconds);
            Add(canonical, source.Cabin.RequiredWorkSeconds);
            Add(canonical, source.Cabin.StorageCapacity);
            Add(canonical, source.Cabin.RecoveryAvailable);
            Add(canonical, source.Cabin.DefenseAvailable);
            Add(canonical, source.Encounter == null);
            if (source.Encounter != null)
            {
                Add(canonical, source.Encounter.EncounterStableId);
                Add(canonical, source.Encounter.StateCode);
                Add(canonical, source.Encounter.ThreatPresentationCode);
                Add(canonical, source.Encounter.TriggeredCycleIndex);
                Add(canonical, source.Encounter.ResolutionCode);
                Add(canonical, source.Encounter.CabinDefenseApplied);
            }
        }
    }
}
