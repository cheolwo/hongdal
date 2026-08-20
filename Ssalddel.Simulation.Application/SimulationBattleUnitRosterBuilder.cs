using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
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
            if (state?.Cards == null) return Array.Empty<SimulationBattleCardModifierSnapshot>();
            var values = new List<SimulationBattleCardModifierSnapshot>();
            foreach (var card in state.Cards.Where(value =>
                         !string.IsNullOrWhiteSpace(value.EquippedActorStableId))
                         .OrderBy(value => value.CardCopyStableId, StringComparer.Ordinal))
            {
                foreach (var role in card.ActivityRoleCodes.OrderBy(value => value,
                             StringComparer.Ordinal))
                {
                    if (role == SimulationTeamRoleCardCodes.Exploration)
                    {
                        Add(values, card, "ReconnaissanceRadius", 1000);
                        Add(values, card, "FormationCohesion", -500);
                    }
                    else if (role == SimulationTeamRoleCardCodes.FarmWork)
                    {
                        Add(values, card, "FarmDefensiveReadiness", 1000);
                        Add(values, card, "PursuitSpeed", -500);
                    }
                    else if (role == SimulationTeamRoleCardCodes.Logistics)
                    {
                        Add(values, card, "SupplyEfficiency", 1000);
                        Add(values, card, "DeploymentSpeed", -500);
                    }
                }
            }
            return values.OrderBy(value => value.CardCopyStableId, StringComparer.Ordinal)
                .ThenBy(value => value.ModifierCode, StringComparer.Ordinal).ToArray();
        }

        private static void Add(ICollection<SimulationBattleCardModifierSnapshot> target,
            SimulationTeamRoleCardSnapshot card, string code, int basisPoints)
            => target.Add(new SimulationBattleCardModifierSnapshot
            {
                CardCopyStableId = card.CardCopyStableId,
                CardDefinitionStableId = card.CardDefinitionStableId,
                ActorStableId = card.EquippedActorStableId,
                ModifierCode = code,
                BasisPoints = basisPoints,
                RuleRevision = "battle-card-modifier.role-card.r1",
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
