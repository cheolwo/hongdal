using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public static class SimulationBattleUnitRosterBuilder
    {
        public static SimulationBattleUnitRosterSnapshot Build(
            string encounterStableId,
            IEnumerable<SimulationFarmActorSnapshot> actors,
            int hostileCount,
            string hostileTypeCode,
            SimulationTeamRoleCardStateSnapshot? cards)
        {
            if (string.IsNullOrWhiteSpace(encounterStableId) || actors == null
                || hostileCount <= 0)
                throw new ArgumentException("SimulationBattleUnitRosterInputInvalid");

            var actorValues = actors.Where(value => value.ActorKindCode ==
                                                    SimulationFarmSurvivalCodes.Npc)
                .OrderBy(value => Role(cards, value.ActorStableId), StringComparer.Ordinal)
                .ThenBy(value => value.ActorStableId, StringComparer.Ordinal).ToArray();
            var units = new List<SimulationBattleUnitSnapshot>();
            var alliedGroups = actorValues.GroupBy(value => Role(cards, value.ActorStableId),
                StringComparer.Ordinal);
            var alliedIndex = 0;
            foreach (var group in alliedGroups)
            {
                var groupedMembers = group.ToArray();
                for (var offset = 0; offset < groupedMembers.Length; offset += 12)
                {
                var members = groupedMembers.Skip(offset).Take(12).ToArray();
                units.Add(new SimulationBattleUnitSnapshot
                {
                    UnitStableId = "battle-unit:allied:" + alliedIndex++.ToString("D3",
                        CultureInfo.InvariantCulture),
                    SideCode = SimulationFarmTacticalCombatCodes.Allied,
                    MemberActorStableIds = members.Select(value => value.ActorStableId).ToArray(),
                    MemberCount = members.Length,
                    CombatStrength = Math.Max(1, members.Sum(value =>
                        (int)Math.Round((value.Health + value.Stamina) / 40m,
                            MidpointRounding.AwayFromZero))),
                    HealthPermille = AveragePermille(members.Select(value => value.Health)),
                    StaminaPermille = AveragePermille(members.Select(value => value.Stamina)),
                    MoralePermille = members.Any(value => value.Injured) ? 850 : 1000,
                    RoleCodes = new[] { group.Key },
                    CapabilityCodes = members.SelectMany(value => value.CapabilityCodes)
                        .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                            StringComparer.Ordinal).ToArray(),
                    FormationCode = "LineFormation",
                    InitialPose = BattlePose(-60d + alliedIndex * 24d, 180d),
                });
                }
            }

            var hostileIndex = 0;
            for (var remaining = hostileCount; remaining > 0; remaining -= 12)
            {
                var count = Math.Min(12, remaining);
                units.Add(new SimulationBattleUnitSnapshot
                {
                    UnitStableId = "battle-unit:hostile:" + hostileIndex++.ToString("D3",
                        CultureInfo.InvariantCulture),
                    SideCode = SimulationFarmTacticalCombatCodes.Hostile,
                    ThreatTypeCode = hostileTypeCode ?? string.Empty,
                    MemberCount = count,
                    CombatStrength = count,
                    RoleCodes = new[] { "Threat" },
                    CapabilityCodes = new[] { "HostileAdvance" },
                    FormationCode = "LineFormation",
                    InitialPose = BattlePose(-60d + hostileIndex * 24d, -180d),
                });
            }

            var modifiers = BuildModifiers(cards);
            var roster = new SimulationBattleUnitRosterSnapshot
            {
                Units = units.OrderBy(value => value.UnitStableId,
                    StringComparer.Ordinal).ToArray(),
                CardModifiers = modifiers,
            };
            roster.BattleUnitRosterHashSha256 = Hash(CanonicalUnits(roster.Units));
            roster.CardModifierHashSha256 = Hash(CanonicalModifiers(modifiers));
            roster.CombatSeedHashSha256 = Hash(string.Join("|",
                SimulationBattlefieldDerivationCodes.CombatSimulationRevision,
                encounterStableId.Trim(), roster.BattleUnitRosterHashSha256,
                roster.CardModifierHashSha256));
            roster.CombatSeed = ulong.Parse(roster.CombatSeedHashSha256[..16],
                NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return roster;
        }

        public static void BindBattlefieldPlan(
            SimulationBattleUnitRosterSnapshot roster,
            string battlefieldPlanHashSha256)
        {
            if (roster == null || string.IsNullOrWhiteSpace(battlefieldPlanHashSha256))
                throw new ArgumentException("SimulationBattleCombatSeedInputInvalid");
            roster.CombatSeedHashSha256 = Hash(string.Join("|",
                roster.CombatSimulationRevision,
                battlefieldPlanHashSha256,
                roster.BattleUnitRosterHashSha256,
                roster.CardModifierHashSha256));
            roster.CombatSeed = ulong.Parse(roster.CombatSeedHashSha256[..16],
                NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        private static SimulationBattleCardModifierSnapshot[] BuildModifiers(
            SimulationTeamRoleCardStateSnapshot? state)
        {
            if (state?.Cards == null || state.CombatLoadouts == null)
                return Array.Empty<SimulationBattleCardModifierSnapshot>();
            var values = new List<SimulationBattleCardModifierSnapshot>();
            foreach (var loadout in state.CombatLoadouts
                         .OrderBy(value => value.ActorStableId, StringComparer.Ordinal)
                         .ThenBy(value => value.CombatControlModeCode,
                             StringComparer.Ordinal))
            {
                foreach (var slot in loadout.Slots.OrderBy(value => value.SlotCode,
                             StringComparer.Ordinal))
                {
                    var card = state.Cards.Single(value => value.CardCopyStableId ==
                        slot.CardCopyStableId);
                    if (loadout.CombatControlModeCode ==
                        SimulationTeamRoleCardCodes.ObserverOperation)
                    {
                        AddObserverModifier(values, state, card, loadout, slot.SlotCode);
                        continue;
                    }
                    foreach (var role in card.ActivityRoleCodes.OrderBy(value => value,
                                 StringComparer.Ordinal))
                    {
                        if (loadout.CombatControlModeCode ==
                            SimulationTeamRoleCardCodes.DirectAction)
                        {
                            if (role == SimulationTeamRoleCardCodes.Exploration)
                                Add(values, state, card, loadout,
                                    "DirectSkillPower", 1000);
                            else if (role == SimulationTeamRoleCardCodes.FarmWork)
                                Add(values, state, card, loadout,
                                    "DirectGuardEfficiency", 1000);
                            else if (role == SimulationTeamRoleCardCodes.Logistics)
                                Add(values, state, card, loadout,
                                    "DirectSkillPower", 500);
                        }
                        else if (role == SimulationTeamRoleCardCodes.Exploration)
                        {
                            Add(values, state, card, loadout,
                                "ReconnaissanceRadius", 1000);
                            Add(values, state, card, loadout,
                                "FormationCohesion", -500);
                        }
                        else if (role == SimulationTeamRoleCardCodes.FarmWork)
                        {
                            Add(values, state, card, loadout,
                                "FarmDefensiveReadiness", 1000);
                            Add(values, state, card, loadout,
                                "PursuitSpeed", -500);
                        }
                        else if (role == SimulationTeamRoleCardCodes.Logistics)
                        {
                            Add(values, state, card, loadout,
                                "SupplyEfficiency", 1000);
                            Add(values, state, card, loadout,
                                "DeploymentSpeed", -500);
                        }
                    }
                }
            }
            return values.OrderBy(value => value.CardCopyStableId, StringComparer.Ordinal)
                .ThenBy(value => value.ModifierCode, StringComparer.Ordinal).ToArray();
        }

        private static void AddObserverModifier(
            ICollection<SimulationBattleCardModifierSnapshot> target,
            SimulationTeamRoleCardStateSnapshot state,
            SimulationTeamRoleCardSnapshot card,
            SimulationCombatCardLoadoutSnapshot loadout,
            string slotCode)
        {
            var expectedSlot = card.CardDefinitionStableId switch
            {
                SimulationLocalCombatCodes.FocusedAssaultCardDefinition
                    or SimulationLocalCombatCodes.CautiousDefenseCardDefinition
                    => SimulationTeamRoleCardCodes.ObserverTactic,
                SimulationLocalCombatCodes.WeaknessObservationCardDefinition
                    or SimulationLocalCombatCodes.CabinCoverCardDefinition
                    => SimulationTeamRoleCardCodes.ObserverSupport,
                SimulationLocalCombatCodes.FieldRecoveryCardDefinition
                    or SimulationLocalCombatCodes.SafeRetreatCardDefinition
                    => SimulationTeamRoleCardCodes.ObserverEmergency,
                _ => string.Empty,
            };
            if (slotCode != expectedSlot)
                throw new SimulationContractException(
                    "SimulationObserverCombatCardSlotInvalid");
            var (modifierCode, basisPoints) = card.CardDefinitionStableId switch
            {
                SimulationLocalCombatCodes.FocusedAssaultCardDefinition
                    => (SimulationLocalCombatCodes.ObserverFocusedAssault, 1500),
                SimulationLocalCombatCodes.CautiousDefenseCardDefinition
                    => (SimulationLocalCombatCodes.ObserverCautiousDefense, -2000),
                SimulationLocalCombatCodes.WeaknessObservationCardDefinition
                    => (SimulationLocalCombatCodes.ObserverWeaknessObservation, 0),
                SimulationLocalCombatCodes.CabinCoverCardDefinition
                    => (SimulationLocalCombatCodes.ObserverCabinCover, -1500),
                SimulationLocalCombatCodes.FieldRecoveryCardDefinition
                    => (SimulationLocalCombatCodes.ObserverFieldRecovery,
                        SimulationLocalCombatCodes.ObserverRecoveryPermille),
                SimulationLocalCombatCodes.SafeRetreatCardDefinition
                    => (SimulationLocalCombatCodes.ObserverSafeRetreat, 0),
                _ => throw new SimulationContractException(
                    "SimulationObserverCombatCardDefinitionInvalid"),
            };
            Add(target, state, card, loadout, modifierCode, basisPoints);
        }

        private static void Add(ICollection<SimulationBattleCardModifierSnapshot> target,
            SimulationTeamRoleCardStateSnapshot state,
            SimulationTeamRoleCardSnapshot card,
            SimulationCombatCardLoadoutSnapshot loadout,
            string code, int basisPoints)
            => target.Add(new SimulationBattleCardModifierSnapshot
            {
                CardCopyStableId = card.CardCopyStableId,
                CardDefinitionStableId = card.CardDefinitionStableId,
                SourceCardRevision = state.Revision,
                ApplicableControlModeCode = loadout.CombatControlModeCode,
                ActorStableId = loadout.ActorStableId,
                ModifierCode = code,
                BasisPoints = basisPoints,
                RuleRevision = "battle-card-modifier.role-card.r2",
            });

        private static string Role(SimulationTeamRoleCardStateSnapshot? cards, string actorId)
            => cards?.MemberRoles?.FirstOrDefault(value => value.ActorStableId == actorId)
                   ?.CurrentRoleCode
               ?? SimulationTeamRoleCardCodes.Idle;

        private static int AveragePermille(IEnumerable<decimal> values)
        {
            var array = values.ToArray();
            return array.Length == 0 ? 1000 : Math.Max(0, Math.Min(1000,
                (int)Math.Round(array.Average() * 10m,
                    MidpointRounding.AwayFromZero)));
        }

        private static SimulationBattleSpatialPoseSnapshot BattlePose(double x, double z)
            => new()
            {
                CoordinateSpaceCode = SimulationBattlefieldDerivationCodes.BattleLocalMeters,
                XMeters = x,
                ZMeters = z,
            };

        private static string CanonicalUnits(IEnumerable<SimulationBattleUnitSnapshot> units)
        {
            var text = new StringBuilder();
            foreach (var unit in units.OrderBy(value => value.UnitStableId,
                         StringComparer.Ordinal))
            {
                Add(text, unit.UnitStableId); Add(text, unit.SideCode);
                foreach (var actor in unit.MemberActorStableIds.OrderBy(value => value,
                             StringComparer.Ordinal)) Add(text, actor);
                Add(text, unit.ThreatTypeCode); Add(text, unit.MemberCount);
                Add(text, unit.CombatStrength); Add(text, unit.HealthPermille);
                Add(text, unit.StaminaPermille); Add(text, unit.MoralePermille);
                foreach (var role in unit.RoleCodes.OrderBy(value => value,
                             StringComparer.Ordinal)) Add(text, role);
                foreach (var capability in unit.CapabilityCodes.OrderBy(value => value,
                             StringComparer.Ordinal)) Add(text, capability);
                Add(text, unit.FormationCode); Add(text, unit.InitialPose.XMeters);
                Add(text, unit.InitialPose.ZMeters);
            }
            return text.ToString();
        }

        private static string CanonicalModifiers(
            IEnumerable<SimulationBattleCardModifierSnapshot> values)
        {
            var text = new StringBuilder();
            foreach (var value in values.OrderBy(item => item.CardCopyStableId,
                         StringComparer.Ordinal).ThenBy(item => item.ModifierCode,
                         StringComparer.Ordinal))
            {
                Add(text, value.CardCopyStableId); Add(text, value.CardDefinitionStableId);
                Add(text, value.SourceCardRevision);
                Add(text, value.ApplicableControlModeCode);
                Add(text, value.ActorStableId); Add(text, value.ModifierCode);
                Add(text, value.BasisPoints); Add(text, value.RuleRevision);
            }
            return text.ToString();
        }

        private static void Add(StringBuilder target, object? value)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            target.Append(text.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(text).Append('|');
        }

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
