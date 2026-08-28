using System;
using System.Linq;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    internal static partial class SimulationReplayHasher
    {
        private static void AddNatureSurvivalInitialState(StringBuilder canonical,
            SimulationNatureSurvivalInitialStateRequest source, bool includesR3)
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
            if (!includesR3) return;
            Add(canonical, source.BuildingProgressionCatalog == null);
            if (source.BuildingProgressionCatalog != null)
            {
                Add(canonical, source.BuildingProgressionCatalog.Revision);
                Add(canonical, source.BuildingProgressionCatalog.HashSha256);
            }
        }

        private static void AddNatureSurvivalState(StringBuilder canonical,
            SimulationNatureSurvivalStateSnapshot source, bool includesR2,
            bool includesR3, bool includesR4, bool includesR5)
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
                if (!string.IsNullOrEmpty(source.ActiveWork.ActorStableId))
                    Add(canonical, source.ActiveWork.ActorStableId);
                Add(canonical, source.ActiveWork.WorkKindCode);
                Add(canonical, source.ActiveWork.TargetStableId);
                Add(canonical, source.ActiveWork.RequiredWorkSeconds);
                Add(canonical, source.ActiveWork.CompletedWorkSeconds);
                if (includesR4)
                {
                    Add(canonical, source.ActiveWork.ReservedTimberQuantity);
                    Add(canonical, source.ActiveWork.ReservedRebuildPartQuantity);
                }
            }
            if (source.CooperativeActors != null
                && source.CooperativeActors.Length > 0)
            {
                Add(canonical, source.CooperativeActors.Length);
                foreach (var actor in source.CooperativeActors.OrderBy(value =>
                             value.ActorStableId, StringComparer.Ordinal))
                {
                    Add(canonical, actor.ActorStableId);
                    Add(canonical, actor.InventoryCapacityUnits);
                    Add(canonical, actor.HasAxe);
                    Add(canonical, actor.TimberQuantity);
                    Add(canonical, actor.RegisteredWorldRevision);
                }
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
            if (!includesR2) return;
            Add(canonical, source.StoredTimberQuantity);
            Add(canonical, source.RawThreatTier);
            Add(canonical, source.EffectiveThreatTier);
            Add(canonical, source.RebuildPartQuantity);
            Add(canonical, source.LinkedCombatStableId);
            Add(canonical, source.LastCombatResultCode);
            Add(canonical, source.Sleeping);
            Add(canonical, source.SelectedExpansionPlanCode);
            Add(canonical, source.Day2Ready);
            if (source.Encounter != null)
            {
                Add(canonical, source.Encounter.RawThreatTier);
                Add(canonical, source.Encounter.EffectiveThreatTier);
                Add(canonical, source.Encounter.HostileCount);
                Add(canonical, source.Encounter.LinkedCombatStableId);
            }
            if (!includesR3) return;
            Add(canonical, source.BuildingProgression == null);
            if (source.BuildingProgression != null)
            {
                Add(canonical, source.BuildingProgression.CatalogRevision);
                Add(canonical, source.BuildingProgression.CatalogHashSha256);
                Add(canonical, source.BuildingProgression.AreaCode);
                foreach (var node in source.BuildingProgression.Nodes.OrderBy(value =>
                             value.BlueprintStableId, StringComparer.Ordinal))
                {
                    Add(canonical, node.BlueprintStableId);
                    Add(canonical, node.StateCode);
                    Add(canonical, node.IsDay2Priority);
                    Add(canonical, node.CompletedWorkSeconds);
                    Add(canonical, node.LocalX);
                    Add(canonical, node.LocalZ);
                    Add(canonical, node.YawDegrees);
                    Add(canonical, node.CompletedLearningVisitCount);
                }
            }
            Add(canonical, source.LearningVisit == null);
            if (source.LearningVisit != null)
            {
                Add(canonical, source.LearningVisit.VisitStableId);
                Add(canonical, source.LearningVisit.NpcStableId);
                Add(canonical, source.LearningVisit.BuildingFacilityStableId);
                Add(canonical, source.LearningVisit.TeachingMaterialStableId);
                Add(canonical, source.LearningVisit.StateCode);
                Add(canonical, source.LearningVisit.StartedCycleIndex);
                Add(canonical, source.LearningVisit.StartedAtSecond);
                Add(canonical, source.LearningVisit.CompletedAtSecond);
            }
            if (!includesR4) return;
            Add(canonical, source.FieldSupplyPackQuantity);
            Add(canonical, source.ExpeditionPrepared);
            Add(canonical, source.LastProtectedMaterialItemCode);
            if (!includesR5) return;
            Add(canonical, source.DroppedTimber.Length);
            foreach (var dropped in source.DroppedTimber.OrderBy(value =>
                         value.DroppedTimberStableId, StringComparer.Ordinal))
            {
                Add(canonical, dropped.DroppedTimberStableId);
                Add(canonical, dropped.SourceResourceNodeStableId);
                Add(canonical, dropped.H2StableId);
                Add(canonical, dropped.H1StableId);
                Add(canonical, dropped.LocalX);
                Add(canonical, dropped.LocalZ);
                Add(canonical, dropped.Quantity);
                Add(canonical, dropped.UnitCode);
                Add(canonical, dropped.StateCode);
                Add(canonical, dropped.CreatedWorldRevision);
                Add(canonical, dropped.CollectedWorldRevision);
            }
        }
    }
}
