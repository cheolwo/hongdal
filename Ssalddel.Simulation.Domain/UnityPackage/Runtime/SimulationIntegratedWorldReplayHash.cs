using System;
using System.Linq;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    internal static partial class SimulationReplayHasher
    {
        private static void AddIntegratedWorldInitialState(StringBuilder target,
            SimulationIntegratedWorldInitialStateRequest value)
        {
            Add(target, value.ScenarioRevision); Add(target, value.ScenarioHashSha256);
            foreach (var definition in value.FacilityDefinitions.OrderBy(item =>
                         item.FacilityDefinitionStableId, StringComparer.Ordinal))
            {
                Add(target, definition.FacilityDefinitionStableId); Add(target, definition.Revision);
                Add(target, definition.HashSha256); Add(target, definition.FacilityTypeCode);
                foreach (var code in definition.CapabilityCodes.OrderBy(item => item,
                             StringComparer.Ordinal)) Add(target, code);
                foreach (var capacity in definition.Capacities.OrderBy(item =>
                             item.CapacityCode, StringComparer.Ordinal))
                {
                    Add(target, capacity.CapacityCode); Add(target, capacity.Quantity);
                    Add(target, capacity.UnitCode);
                }
            }
            foreach (var seed in value.FacilitySeeds.OrderBy(item => item.FacilityStableId,
                         StringComparer.Ordinal))
            {
                Add(target, seed.FacilityStableId); Add(target, seed.FacilityDefinitionStableId);
                Add(target, seed.PlacementH1StableId);
                foreach (var connector in seed.AccessConnectorStableIds.OrderBy(item => item,
                             StringComparer.Ordinal)) Add(target, connector);
            }
            foreach (var actor in value.Actors.OrderBy(item => item.ActorStableId,
                         StringComparer.Ordinal))
            {
                Add(target, actor.ActorStableId); Add(target, actor.EligibilityRank);
                Add(target, actor.FarmLaborEligible);
            }
            foreach (var lot in value.Lots.OrderBy(item => item.LotStableId,
                         StringComparer.Ordinal))
            {
                Add(target, lot.LotStableId); Add(target, lot.ItemCode); Add(target, lot.Quantity);
                Add(target, lot.UnitCode); Add(target, lot.FacilityStableId);
            }
            foreach (var recipe in value.ManufacturingRecipes.OrderBy(item => item.RecipeStableId,
                         StringComparer.Ordinal))
            {
                Add(target, recipe.RecipeStableId); Add(target, recipe.Revision);
                Add(target, recipe.HashSha256); Add(target, recipe.ProcessingTicks);
                AddRequirements(target, recipe.Inputs); AddRequirements(target, recipe.Outputs);
            }
            foreach (var blueprint in value.FacilityBlueprints.OrderBy(item =>
                         item.BlueprintStableId, StringComparer.Ordinal))
            {
                Add(target, blueprint.BlueprintStableId); Add(target, blueprint.Revision);
                Add(target, blueprint.HashSha256); Add(target, blueprint.FacilityDefinitionStableId);
                Add(target, blueprint.SettlementFacilityTypeCode);
                Add(target, blueprint.SettlementDistrictStableId);
                Add(target, blueprint.ConstructionTicks); AddRequirements(target, blueprint.Materials);
                if (!string.IsNullOrWhiteSpace(blueprint.PlacementKindCode))
                {
                    Add(target, blueprint.PlacementKindCode);
                    Add(target, blueprint.FootprintWidthCentimeters);
                    Add(target, blueprint.FootprintDepthCentimeters);
                    Add(target, blueprint.ClearanceCentimeters);
                    Add(target, blueprint.MaxSlopeMilliDegrees);
                    Add(target, blueprint.RequiresRoadAccess);
                    AddIntegratedStrings(target, blueprint.AllowedPlacementZoneTypeCodes);
                }
            }
            foreach (var zone in value.ConstructionPlacementZones.OrderBy(item =>
                         item.PlacementZoneStableId, StringComparer.Ordinal))
            {
                Add(target, zone.PlacementZoneStableId); Add(target, zone.TargetH2StableId);
                Add(target, zone.ZoneTypeCode); Add(target, zone.PlacementProfileRevision);
                Add(target, zone.MinXCentimeters); Add(target, zone.MaxXCentimeters);
                Add(target, zone.MinZCentimeters); Add(target, zone.MaxZCentimeters);
                Add(target, zone.TerrainSlopeMilliDegrees); Add(target, zone.FenceChainStableId);
                Add(target, zone.FenceStartXCentimeters ?? int.MinValue);
                Add(target, zone.FenceStartZCentimeters ?? int.MinValue);
                AddIntegratedStrings(target, zone.RoadAccessConnectorStableIds);
            }
        }

        private static void AddIntegratedWorldSnapshot(StringBuilder target,
            SimulationIntegratedWorldSnapshot value)
        {
            Add(target, value.ScenarioRevision); Add(target, value.ScenarioHashSha256);
            foreach (var facility in value.Facilities.OrderBy(item => item.FacilityStableId,
                         StringComparer.Ordinal))
            {
                Add(target, facility.FacilityStableId); Add(target, facility.FacilityDefinitionStableId);
                Add(target, facility.FacilityDefinitionRevision); Add(target, facility.FacilityDefinitionHashSha256);
                Add(target, facility.PlacementH1StableId);
                if (!string.IsNullOrWhiteSpace(facility.PlacementZoneStableId))
                {
                    Add(target, facility.PlacementZoneStableId); Add(target, facility.TargetH2StableId);
                    Add(target, facility.PlacementKindCode); Add(target, facility.LocalXCentimeters);
                    Add(target, facility.LocalZCentimeters); Add(target, facility.RotationQuarterTurns);
                    Add(target, facility.PlacementProfileRevision); Add(target, facility.FenceChainStableId);
                }
                Add(target, facility.SettlementFacilityTypeCode);
                Add(target, facility.SettlementDistrictStableId);
                Add(target, facility.LifecycleCode);
                Add(target, facility.IntegrityCode); Add(target, facility.MaintenanceCode);
                foreach (var connector in facility.AccessConnectorStableIds.OrderBy(item => item,
                             StringComparer.Ordinal)) Add(target, connector);
                foreach (var capability in facility.EffectiveCapabilities.OrderBy(item =>
                             item.CapabilityCode, StringComparer.Ordinal))
                {
                    Add(target, capability.CapabilityCode); Add(target, capability.StateCode);
                    foreach (var source in capability.SourceRestrictionStableIds.OrderBy(item => item,
                                 StringComparer.Ordinal)) Add(target, source);
                }
                foreach (var capacity in facility.DefinedCapacities.OrderBy(item =>
                             item.CapacityCode, StringComparer.Ordinal))
                {
                    Add(target, capacity.CapacityCode); Add(target, capacity.Quantity);
                    Add(target, capacity.UnitCode);
                }
            }
            foreach (var restriction in value.FacilityRestrictions.OrderBy(item =>
                         item.RestrictionStableId, StringComparer.Ordinal))
            {
                Add(target, restriction.RestrictionStableId); Add(target, restriction.SourceEffectStableId);
                Add(target, restriction.FacilityStableId); Add(target, restriction.CapabilityCode);
                Add(target, restriction.RestrictionLevelCode); Add(target, restriction.ResolvedByEffectStableId);
            }
            foreach (var job in value.ManufacturingJobs.OrderBy(item => item.ManufacturingJobStableId,
                         StringComparer.Ordinal))
            {
                Add(target, job.ManufacturingJobStableId); Add(target, job.RecipeStableId);
                Add(target, job.RecipeRevision); Add(target, job.RecipeHashSha256); Add(target, job.StateCode);
                Add(target, job.ProcessingStartsAtTick); Add(target, job.ProcessingCompletesAtTick);
                Add(target, job.ActorStableId); Add(target, job.FacilityStableId);
                AddRequirements(target, job.ResolvedInputRequirements);
                AddRequirements(target, job.ResolvedOutputSpecification);
                AddIntegratedStrings(target, job.ReservedInputLotStableIds); AddIntegratedStrings(target, job.ConsumedInputLotStableIds);
                AddIntegratedStrings(target, job.OutputLotStableIds);
            }
            foreach (var project in value.ConstructionProjects.OrderBy(item =>
                         item.ConstructionProjectStableId, StringComparer.Ordinal))
            {
                Add(target, project.ConstructionProjectStableId); Add(target, project.BlueprintStableId);
                Add(target, project.BlueprintRevision); Add(target, project.BlueprintHashSha256);
                Add(target, project.StateCode); Add(target, project.TargetFacilityStableId);
                Add(target, project.BuildSiteH1StableId); Add(target, project.ConstructionStartsAtTick);
                if (!string.IsNullOrWhiteSpace(project.PlacementZoneStableId))
                {
                    Add(target, project.PlacementProposalStableId);
                    Add(target, project.PlacementPreviewHashSha256);
                    Add(target, project.PlacementZoneStableId); Add(target, project.TargetH2StableId);
                    Add(target, project.PlacementKindCode); Add(target, project.LocalXCentimeters);
                    Add(target, project.LocalZCentimeters); Add(target, project.RotationQuarterTurns);
                    Add(target, project.PlacementProfileRevision); Add(target, project.FenceChainStableId);
                    if (!string.IsNullOrWhiteSpace(project.DevelopmentOpportunityStableId))
                        Add(target, project.DevelopmentOpportunityStableId);
                }
                Add(target, project.ConstructionCompletesAtTick); Add(target, project.ActorStableId);
                AddRequirements(target, project.ResolvedMaterialRequirements);
                AddIntegratedStrings(target, project.ReservedMaterialLotStableIds);
                AddIntegratedStrings(target, project.ConsumedMaterialLotStableIds);
            }
            foreach (var actor in value.Actors.OrderBy(item => item.ActorStableId,
                         StringComparer.Ordinal))
            {
                Add(target, actor.ActorStableId); Add(target, actor.EligibilityRank);
                Add(target, actor.FarmLaborEligible);
            }
            foreach (var formation in value.Formations.OrderBy(item => item.FormationStableId,
                         StringComparer.Ordinal))
            {
                Add(target, formation.FormationStableId); Add(target, formation.StateCode);
                Add(target, formation.GarrisonFacilityStableId); Add(target, formation.StateCompletesAtTick ?? -1);
                AddIntegratedStrings(target, formation.MemberActorStableIds);
            }
            foreach (var commitment in value.ActorCommitments.OrderBy(item =>
                         item.CommitmentStableId, StringComparer.Ordinal))
            {
                Add(target, commitment.CommitmentStableId); Add(target, commitment.ActorStableId);
                Add(target, commitment.CommitmentCode); Add(target, commitment.SourceStableId);
                Add(target, commitment.Active);
            }
            foreach (var injury in value.ActorInjuries.OrderBy(item => item.InjuryStableId,
                         StringComparer.Ordinal))
            {
                Add(target, injury.InjuryStableId); Add(target, injury.ActorStableId);
                Add(target, injury.SourceEffectStableId); Add(target, injury.Active);
            }
            foreach (var lot in value.Lots.OrderBy(item => item.LotStableId,
                         StringComparer.Ordinal))
            {
                Add(target, lot.LotStableId); Add(target, lot.ItemCode); Add(target, lot.Quantity);
                Add(target, lot.UnitCode); Add(target, lot.FacilityStableId); Add(target, lot.SourceStableId);
            }
            foreach (var reservation in value.Reservations.OrderBy(item =>
                         item.ReservationStableId, StringComparer.Ordinal))
            {
                Add(target, reservation.ReservationStableId); Add(target, reservation.OwnerStableId);
                Add(target, reservation.TargetStableId); Add(target, reservation.ReservationKindCode);
                Add(target, reservation.Quantity); Add(target, reservation.StateCode);
            }
            foreach (var effect in value.WorldEffects.OrderBy(item => item.EffectStableId,
                         StringComparer.Ordinal))
            {
                Add(target, effect.EffectStableId); Add(target, effect.EffectCode);
                Add(target, effect.SourceStableId); Add(target, effect.TargetStableId);
                Add(target, effect.PayloadCanonical);
            }
            foreach (var pending in value.PendingWorldEffects.OrderBy(item => item.EffectStableId,
                         StringComparer.Ordinal))
            {
                Add(target, pending.EffectStableId); Add(target, pending.EarliestWorldTick);
            }
            foreach (var receipt in value.AppliedWorldEffectReceipts.OrderBy(item =>
                         item.EffectStableId, StringComparer.Ordinal))
            {
                Add(target, receipt.EffectStableId); Add(target, receipt.AppliedWorldTick);
                Add(target, receipt.AppliedWorldRevision);
            }
            foreach (var repair in value.RepairJobs.OrderBy(item => item.RepairJobStableId,
                         StringComparer.Ordinal))
            {
                Add(target, repair.RepairJobStableId); Add(target, repair.FacilityStableId);
                Add(target, repair.ActorStableId); Add(target, repair.CompletesAtTick);
                Add(target, repair.StateCode); AddIntegratedStrings(target, repair.TargetRestrictionStableIds);
                AddIntegratedStrings(target, repair.ReservedMaterialLotStableIds);
            }
            foreach (var movement in value.CargoMovements.OrderBy(item => item.MovementStableId,
                         StringComparer.Ordinal))
            {
                Add(target, movement.MovementStableId); Add(target, movement.SourceLotStableId);
                Add(target, movement.TargetFacilityStableId); Add(target, movement.Quantity);
                Add(target, movement.ActorStableId); Add(target, movement.CompletesAtTick);
                Add(target, movement.StateCode); Add(target, movement.OutputLotStableId);
            }
        }

        private static void AddIntegratedWorldCommand(StringBuilder target,
            SimulationIntegratedWorldCommandRequest value)
        {
            Add(target, value.ActionCode); Add(target, value.CommandId); Add(target, value.ExpectedRevision);
            Add(target, 경영SimulationSessionAggregate.BuildIntegratedCommandFingerprint(value));
        }

        private static void AddRequirements(StringBuilder target,
            SimulationIntegratedItemRequirement[] values)
        {
            foreach (var value in values.OrderBy(item => item.ItemCode, StringComparer.Ordinal))
            {
                Add(target, value.ItemCode); Add(target, value.Quantity); Add(target, value.UnitCode);
            }
        }

        private static void AddIntegratedStrings(StringBuilder target, string[] values)
        {
            foreach (var value in values.OrderBy(item => item, StringComparer.Ordinal))
                Add(target, value);
        }
    }
}
