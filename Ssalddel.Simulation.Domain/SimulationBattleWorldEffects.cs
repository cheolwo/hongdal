using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly HashSet<string> appliedBattleWorldEffectKeys =
            new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> appliedFarmBattleOutcomeIds =
            new HashSet<string>(StringComparer.Ordinal);

        public 경영SimulationSessionSnapshot ApplyBattleSemanticEffects(
            string battleStableId,
            string encounterStableId,
            SimulationBattleOutcomeSnapshot outcome,
            IEnumerable<SimulationBattleSemanticEffectSnapshot> effects)
        {
            if (string.IsNullOrWhiteSpace(battleStableId)
                || string.IsNullOrWhiteSpace(encounterStableId)
                || outcome == null || effects == null)
                throw new SimulationContractException(
                    "SimulationBattleWorldEffectInputInvalid");
            lock (gate)
            {
                var changed = false;
                var farmEncounter = threatEncounters.Any(value =>
                    value.EncounterStableId == encounterStableId.Trim());
                foreach (var effect in effects.OrderBy(value =>
                             value.WorldEffectApplicationKey, StringComparer.Ordinal))
                {
                    if (string.IsNullOrWhiteSpace(effect.WorldEffectApplicationKey))
                        throw new SimulationContractException(
                            "SimulationBattleWorldEffectApplicationKeyInvalid");
                    if (!appliedBattleWorldEffectKeys.Add(
                            effect.WorldEffectApplicationKey))
                        continue;
                    changed |= ApplyBattleSemanticEffect(effect, farmEncounter);
                }

                var outcomeKey = battleStableId.Trim() + "~" +
                    encounterStableId.Trim();
                if (farmEncounter && farmSurvivalCreationState != null
                    && appliedFarmBattleOutcomeIds.Add(outcomeKey))
                {
                    var encounter = threatEncounters.FirstOrDefault(value =>
                        value.EncounterStableId == encounterStableId.Trim());
                    if (encounter != null
                        && encounter.StateCode != SimulationFarmSurvivalCodes.Resolved)
                    {
                        encounter.StateCode = SimulationFarmSurvivalCodes.Resolved;
                        encounter.OutcomeCode = outcome.ResultCode ==
                            SimulationBattleInstanceCodes.Victory
                            ? SimulationFarmSurvivalCodes.DefenseSucceeded
                            : SimulationFarmSurvivalCodes.FacilityDamaged;
                        encounter.DamageUnits = outcome.FacilityDamageUnits;
                        encounter.SupplyLossUnits = Math.Min(farmSupplyUnits,
                            outcome.SupplyLossUnits);
                        farmSupplyUnits -= encounter.SupplyLossUnits;
                        encounter.PresentationKey =
                            SimulationFarmSurvivalCodes.DamageAssessmentPresentation;
                        UpdateFarmThreatWorldEvent(encounter);
                        changed = true;
                    }
                }
                if (changed) Revision++;
                return CreateSnapshot();
            }
        }

        private bool ApplyBattleSemanticEffect(
            SimulationBattleSemanticEffectSnapshot effect,
            bool farmEncounter)
        {
            if (effect.SemanticEffectCode ==
                SimulationBattlefieldDerivationCodes.ActorCombatCasualty)
            {
                if (!farmActors.TryGetValue(effect.WorldEffectTargetStableId,
                        out var actor)) return false;
                actor.Health = Math.Max(1m, actor.Health - DamageUnits(effect.SeverityCode));
                actor.Injured = true;
                return true;
            }
            if (effect.SemanticEffectCode ==
                    SimulationBattlefieldDerivationCodes.FacilityCombatDamage
                || effect.SemanticEffectCode ==
                    SimulationBattlefieldDerivationCodes.GateCombatDamage)
            {
                if (!farmEncounter) return false;
                var damage = DamageUnits(effect.SeverityCode);
                recoverableDamageUnits += damage;
                var defense = farmDefenses.Values.OrderBy(value => value.DefenseStableId,
                    StringComparer.Ordinal).FirstOrDefault();
                if (defense != null)
                    defense.Durability = Math.Max(0m, defense.Durability - damage);
                return farmSurvivalCreationState != null;
            }
            return effect.SemanticEffectCode ==
                    SimulationBattlefieldDerivationCodes.ObjectiveLost
                || effect.SemanticEffectCode ==
                    SimulationBattlefieldDerivationCodes.ObjectiveSecured;
        }

        private static decimal DamageUnits(string severityCode)
            => severityCode == SimulationBattlefieldDerivationCodes.Destroyed ? 40m
                : severityCode == SimulationBattlefieldDerivationCodes.Severe ? 25m
                : severityCode == SimulationBattlefieldDerivationCodes.Moderate ? 15m
                : 5m;
    }
}
