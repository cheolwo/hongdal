using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
        Boundary = "계약과 도메인 정의는 실행 위치나 E 단계 달성 증거를 소유하지 않는다.")]
    public static class SimulationCombatScalePolicy
    {
        private static readonly string[] ForcedBattlefieldEvents =
        {
            SimulationLocalCombatCodes.LargeRaid,
            SimulationLocalCombatCodes.StrongholdDefense,
            SimulationLocalCombatCodes.WithdrawalOperation,
            SimulationLocalCombatCodes.BossOperation,
        };

        public static SimulationCombatScaleDecisionSnapshot Evaluate(
            int hostileCount, int companionCount, string threatTypeCode,
            string eventCode = "")
        {
            var reasons = new List<string>();
            var forced = ForcedBattlefieldEvents.Contains(eventCode ?? string.Empty,
                    StringComparer.Ordinal)
                || (threatTypeCode ?? string.Empty).IndexOf("Boss",
                    StringComparison.OrdinalIgnoreCase) >= 0;
            if (forced) reasons.Add("EventRequiresBattlefield");
            if (hostileCount > SimulationLocalCombatCodes.LocalMaximumThreatUnits)
                reasons.Add("HostileCountExceedsLocalLimit");
            if (companionCount > SimulationLocalCombatCodes.LocalMaximumCompanionUnits)
                reasons.Add("CompanionCountExceedsLocalLimit");
            if (forced || hostileCount > SimulationLocalCombatCodes.LocalMaximumThreatUnits
                || companionCount > SimulationLocalCombatCodes.LocalMaximumCompanionUnits)
                return Decision(SimulationLocalCombatCodes.Battlefield,
                    SimulationLocalCombatCodes.DerivedBattlefield, reasons);

            var elite = (threatTypeCode ?? string.Empty).IndexOf("Elite",
                StringComparison.OrdinalIgnoreCase) >= 0;
            if (elite) reasons.Add("EliteThreat");
            if (hostileCount > SimulationLocalCombatCodes.InstantMaximumThreatUnits)
                reasons.Add("HostileGroupRequiresFieldCombat");
            if (companionCount > 0) reasons.Add("CompanionParticipation");
            if (elite || hostileCount > SimulationLocalCombatCodes.InstantMaximumThreatUnits
                || companionCount > 0)
                return Decision(SimulationLocalCombatCodes.Field,
                    SimulationLocalCombatCodes.WorldLocal, reasons);

            reasons.Add("SmallHostileGroup");
            return Decision(SimulationLocalCombatCodes.Instant,
                SimulationLocalCombatCodes.WorldLocal, reasons);
        }

        public static bool IsKnownSpace(string code) =>
            code == SimulationLocalCombatCodes.WorldLocal
            || code == SimulationLocalCombatCodes.DerivedBattlefield;

        public static bool IsKnownScale(string code) =>
            code == SimulationLocalCombatCodes.Instant
            || code == SimulationLocalCombatCodes.Field
            || code == SimulationLocalCombatCodes.Battlefield;

        private static SimulationCombatScaleDecisionSnapshot Decision(
            string scale, string space, IEnumerable<string> reasons) => new()
            {
                EncounterScaleCode = scale,
                CombatSpaceCode = space,
                ReasonCodes = reasons.Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            };
    }

    public sealed partial class SimulationBattleInstanceState
    {
        private readonly List<SimulationLocalCombatActorSnapshot> localCombatActors = new();
        private readonly List<SimulationLocalCombatActionSnapshot> localCombatActions = new();
        private string localFocusedTarget = string.Empty;
        private string localStateCode = SimulationLocalCombatCodes.Active;
        private bool localEscalationRequired;
        private string[] localEscalationReasons = Array.Empty<string>();
        private bool localHostileTelegraphActive;
        private int localHostileTelegraphOpenedCombatTick;
        private string localControlModeCode = SimulationLocalCombatCodes.DirectAction;

        public SimulationBattleInstanceSnapshot ConfirmLocalControlMode(
            SimulationLocalCombatControlModeConfirmRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CommandId)
                || string.IsNullOrWhiteSpace(request.RequestingActorStableId)
                || !KnownControlMode(request.ControlModeCode))
                throw new SimulationContractException(
                    "SimulationLocalCombatControlModeInvalid");
            lock (gate)
            {
                return Apply(request.CommandId, request.ExpectedBattleRevision,
                    string.Join("~", request.RequestingActorStableId.Trim(),
                        request.ControlModeCode.Trim()), () =>
                    {
                        RequireLocalCommander(request.RequestingActorStableId);
                        localControlModeCode = request.ControlModeCode.Trim();
                    });
            }
        }

        public SimulationBattleInstanceSnapshot ConfirmLocalFocus(
            SimulationLocalCombatFocusConfirmRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CommandId)
                || string.IsNullOrWhiteSpace(request.RequestingActorStableId)
                || string.IsNullOrWhiteSpace(request.TargetActorStableId))
                throw new SimulationContractException("SimulationLocalCombatFocusInvalid");
            lock (gate)
            {
                return Apply(request.CommandId, request.ExpectedBattleRevision,
                    string.Join("~", request.RequestingActorStableId.Trim(),
                        request.TargetActorStableId.Trim()), () =>
                    {
                        RequireLocalCommander(request.RequestingActorStableId);
                        var target = FindActiveLocalActor(request.TargetActorStableId,
                            SimulationLocalCombatCodes.Hostile);
                        localFocusedTarget = target.ActorStableId;
                        PlayerActor().FocusedTargetStableId = target.ActorStableId;
                    });
            }
        }

        public SimulationBattleInstanceSnapshot ConfirmLocalAction(
            SimulationLocalCombatActionConfirmRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CommandId)
                || string.IsNullOrWhiteSpace(request.RequestingActorStableId)
                || !KnownLocalAction(request.ActionCode)
                || request.ReactionOffsetMs < 0)
                throw new SimulationContractException("SimulationLocalCombatActionInvalid");
            lock (gate)
            {
                return Apply(request.CommandId, request.ExpectedBattleRevision,
                    string.Join("~", request.RequestingActorStableId.Trim(),
                        request.TargetActorStableId.Trim(), request.ActionCode.Trim(),
                        request.ReactionOffsetMs.ToString(CultureInfo.InvariantCulture)), () =>
                    ResolveLocalAction(request));
            }
        }

        public void PrepareEscalation(IEnumerable<string> reasonCodes)
        {
            lock (gate)
            {
                EnsureLocalActive();
                localEscalationReasons = (reasonCodes ?? Array.Empty<string>())
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Select(value => value.Trim()).Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray();
                if (localEscalationReasons.Length == 0)
                    localEscalationReasons = new[] { "EventRequiresBattlefield" };
                localEscalationRequired = true;
                localStateCode = SimulationLocalCombatCodes.EscalationWarning;
            }
        }

        public SimulationBattleInstanceSnapshot ConfirmEscalation(
            SimulationBattleEscalationConfirmRequest request,
            SimulationBattlefieldDerivationSnapshot derivation,
            SimulationBattleUnitRosterSnapshot roster)
        {
            if (request == null || derivation == null || !derivation.CanConfirm
                || roster == null
                || string.IsNullOrWhiteSpace(request.CommandId))
                throw new SimulationContractException("SimulationBattleEscalationInvalid");
            lock (gate)
            {
                return Apply(request.CommandId, request.ExpectedBattleRevision,
                    string.Join("~", request.RequestingActorStableId.Trim(),
                        request.ExpectedBattleWorldContextHashSha256.Trim(),
                        request.ExpectedBattlefieldDerivationInputHashSha256.Trim()), () =>
                    {
                        RequireLocalCommander(request.RequestingActorStableId);
                        if (!localEscalationRequired)
                            throw new SimulationConflictException(
                                "SimulationBattleEscalationNotRequired");
                        if (derivation.WorldContext.ContextHashSha256 != request
                                .ExpectedBattleWorldContextHashSha256.Trim())
                            throw new SimulationConflictException(
                                "SimulationBattleWorldContextChanged");
                        if (derivation.BattlefieldDerivationInputHashSha256 != request
                                .ExpectedBattlefieldDerivationInputHashSha256.Trim())
                            throw new SimulationConflictException(
                                "SimulationBattleDerivationInputChanged");
                        context.CombatSpaceCode = SimulationLocalCombatCodes.DerivedBattlefield;
                        context.EncounterScaleCode = SimulationLocalCombatCodes.Battlefield;
                        context.ScaleReasonCodes = localEscalationReasons.ToArray();
                        context.BattlefieldDerivation =
                            SimulationBattlefieldSnapshotCloner.Derivation(derivation);
                        context.UnitRoster = SimulationBattlefieldSnapshotCloner.Roster(roster);
                        localStateCode = SimulationLocalCombatCodes.Transitioning;
                        phaseCode = SimulationBattleInstanceCodes.Deploying;
                        deploymentCode = string.Empty;
                    });
            }
        }

        private void InitializeLocalCombat()
        {
            if (context.CombatSpaceCode != SimulationLocalCombatCodes.WorldLocal) return;
            phaseCode = SimulationBattleInstanceCodes.Active;
            deploymentCode = SimulationBattleInstanceCodes.Balanced;
            var commanderRoles = context.UnitRoster.Units
                .Where(value => value.MemberActorStableIds.Contains(
                    context.CommanderActorStableId, StringComparer.Ordinal))
                .SelectMany(value => value.RoleCodes).Distinct(StringComparer.Ordinal).ToArray();
            localCombatActors.Add(NewLocalActor(context.CommanderActorStableId,
                SimulationLocalCombatCodes.Player, string.Empty, commanderRoles,
                SimulationLocalCombatCodes.Near));

            var companions = context.UnitRoster.Units
                .Where(value => value.SideCode == SimulationFarmTacticalCombatCodes.Allied)
                .SelectMany(value => value.MemberActorStableIds.Select(actor => new
                {
                    Actor = actor,
                    Roles = value.RoleCodes,
                }))
                .Where(value => value.Actor != context.CommanderActorStableId)
                .GroupBy(value => value.Actor, StringComparer.Ordinal).Select(value => value.First())
                .OrderBy(value => value.Actor, StringComparer.Ordinal)
                .Take(SimulationLocalCombatCodes.LocalMaximumCompanionUnits).ToArray();
            foreach (var companion in companions)
                localCombatActors.Add(NewLocalActor(companion.Actor,
                    SimulationLocalCombatCodes.Companion, string.Empty,
                    companion.Roles, SimulationLocalCombatCodes.Near));

            var hostileIndex = 0;
            foreach (var unit in context.UnitRoster.Units.Where(value =>
                         value.SideCode == SimulationFarmTacticalCombatCodes.Hostile)
                         .OrderBy(value => value.UnitStableId, StringComparer.Ordinal))
            {
                for (var member = 0; member < unit.MemberCount; member++)
                {
                    var id = string.Concat("threat:", context.EncounterStableId, ":",
                        hostileIndex++.ToString("D2", CultureInfo.InvariantCulture));
                    localCombatActors.Add(NewLocalActor(id,
                        SimulationLocalCombatCodes.Hostile, unit.ThreatTypeCode,
                        unit.RoleCodes, hostileIndex <= 2
                            ? SimulationLocalCombatCodes.Near
                            : SimulationLocalCombatCodes.Far));
                }
            }
            localFocusedTarget = localCombatActors.FirstOrDefault(value =>
                value.SideCode == SimulationLocalCombatCodes.Hostile)?.ActorStableId
                ?? string.Empty;
            PlayerActor().FocusedTargetStableId = localFocusedTarget;
            if (!participationReservations.Any(value => value.ActorStableId ==
                    context.CommanderActorStableId))
                participationReservations.Add(new SimulationBattleParticipationReservationSnapshot
                {
                    ActorStableId = context.CommanderActorStableId,
                    BattleStableId = context.BattleStableId,
                    ReservedWorldTick = context.StartedWorldTick,
                    EnteredBattleTick = 0,
                });
        }

        private SimulationBattleInstanceSnapshot AdvanceLocalCombat(
            SimulationBattleAdvanceRequest request, int currentWorldTick)
        {
            if (currentWorldTick != context.StartedWorldTick)
                throw new SimulationConflictException("SimulationLocalCombatWorldTickMustRemainFrozen");
            return Apply(request.CommandId, request.ExpectedBattleRevision,
                request.CombatTickCount.ToString(CultureInfo.InvariantCulture), () =>
                {
                    EnsureLocalActive();
                    for (var i = 0; i < request.CombatTickCount
                        && phaseCode == SimulationBattleInstanceCodes.Active; i++)
                    {
                        combatTick++;
                        if (combatTick % SimulationLocalCombatCodes.DefaultActionCooldownTicks == 1)
                        {
                            localHostileTelegraphActive = true;
                            localHostileTelegraphOpenedCombatTick = combatTick;
                        }
                        if (combatTick % SimulationLocalCombatCodes.DefaultActionCooldownTicks == 0)
                            ResolveLocalAiTick();
                    }
                });
        }

        private void ResolveLocalAction(SimulationLocalCombatActionConfirmRequest request)
        {
            RequireLocalCommander(request.RequestingActorStableId);
            var actor = PlayerActor();
            if (actor.NextActionCombatTick > combatTick)
                throw new SimulationConflictException("SimulationLocalCombatActionCooldown");
            var action = request.ActionCode.Trim();
            EnsureActionAllowedForControlMode(action);
            var targetId = string.IsNullOrWhiteSpace(request.TargetActorStableId)
                ? localFocusedTarget : request.TargetActorStableId.Trim();
            var result = "Confirmed";
            var healthDelta = 0;
            var staminaDelta = 0;
            var appliedCardCodes = Array.Empty<string>();
            if (action == SimulationLocalCombatCodes.Approach)
            {
                var target = FindActiveLocalActor(targetId, SimulationLocalCombatCodes.Hostile);
                target.RangeBandCode = target.RangeBandCode == SimulationLocalCombatCodes.Far
                    ? SimulationLocalCombatCodes.Near : SimulationLocalCombatCodes.Contact;
                appliedCardCodes = ActiveCardModifierCodes();
                result = localControlModeCode == SimulationLocalCombatCodes.TacticalCommand
                    ? "TacticalAdvanceOrdered" : "RangeClosed";
            }
            else if (action == SimulationLocalCombatCodes.Retreat)
            {
                actor.StateCode = SimulationLocalCombatCodes.Retreated;
                actor.RangeBandCode = SimulationLocalCombatCodes.RetreatBoundary;
                localStateCode = SimulationLocalCombatCodes.Retreated;
                ResolveLocalOutcome(false, true);
                result = "RetreatedToRecovery";
            }
            else if (action == SimulationLocalCombatCodes.Guard)
            {
                staminaDelta = -50;
                actor.StaminaPermille = Math.Max(0, actor.StaminaPermille + staminaDelta);
                appliedCardCodes = ActiveCardModifierCodes()
                    .Where(value => value == "DirectGuardEfficiency").ToArray();
                result = "GuardWindowOpened";
            }
            else if (action == SimulationLocalCombatCodes.Dodge)
            {
                staminaDelta = -80;
                actor.StaminaPermille = Math.Max(0, actor.StaminaPermille + staminaDelta);
                result = request.ReactionOffsetMs <= 320 ? "DodgeSucceeded" : "DodgeLate";
            }
            else if (action == SimulationLocalCombatCodes.HoldPosition)
            {
                appliedCardCodes = ActiveCardModifierCodes();
                result = "FormationHoldOrdered";
            }
            else
            {
                var target = FindActiveLocalActor(targetId, SimulationLocalCombatCodes.Hostile);
                if (target.RangeBandCode == SimulationLocalCombatCodes.Far)
                    throw new SimulationConflictException(
                        "SimulationLocalCombatTargetOutOfRange");
                var tactical = localControlModeCode ==
                    SimulationLocalCombatCodes.TacticalCommand;
                var baseDamage = tactical ? 200
                    : action == SimulationLocalCombatCodes.RoleCardSkill ? 350
                    : action == SimulationLocalCombatCodes.Counter
                      && request.ReactionOffsetMs <= 200 ? 300 : 250;
                var applyCards = tactical
                    || action == SimulationLocalCombatCodes.RoleCardSkill;
                healthDelta = -(applyCards
                    ? ApplyControlCardModifier(actor.ActorStableId, baseDamage,
                        tactical, false, out appliedCardCodes,
                        action == SimulationLocalCombatCodes.RoleCardSkill
                            ? "DirectSkillPower" : string.Empty)
                    : baseDamage);
                target.HealthPermille = Math.Max(0, target.HealthPermille + healthDelta);
                if (target.HealthPermille == 0)
                    target.StateCode = SimulationLocalCombatCodes.Defeated;
                result = target.StateCode == SimulationLocalCombatCodes.Defeated
                    ? "TargetDefeated"
                    : tactical ? "TacticalBasicAttackOrdered" : "Hit";
            }
            actor.NextActionCombatTick = combatTick + ControlModeCooldownTicks();
            AddLocalAction(request.CommandId, actor.ActorStableId, targetId, action,
                result, healthDelta, staminaDelta, request.ReactionOffsetMs,
                appliedCardCodes);
            if (!localCombatActors.Any(value => value.SideCode ==
                    SimulationLocalCombatCodes.Hostile
                    && value.StateCode == SimulationLocalCombatCodes.Active))
                ResolveLocalOutcome(true, false);
        }

        private void ResolveLocalAiTick()
        {
            if (localControlModeCode == SimulationLocalCombatCodes.DirectAction)
            foreach (var companion in localCombatActors.Where(value =>
                         value.SideCode == SimulationLocalCombatCodes.Companion
                         && value.StateCode == SimulationLocalCombatCodes.Active))
            {
                var target = localCombatActors.FirstOrDefault(value => value.SideCode ==
                    SimulationLocalCombatCodes.Hostile
                    && value.StateCode == SimulationLocalCombatCodes.Active);
                if (target == null) break;
                var damage = 120;
                target.HealthPermille = Math.Max(0, target.HealthPermille - damage);
                if (target.HealthPermille == 0)
                    target.StateCode = SimulationLocalCombatCodes.Defeated;
                AddLocalAction("ai:" + combatTick + ":" + companion.ActorStableId,
                    companion.ActorStableId, target.ActorStableId,
                    SimulationLocalCombatCodes.BasicAttack,
                    target.StateCode == SimulationLocalCombatCodes.Defeated
                        ? "TargetDefeated" : "Hit", -damage, 0, 0,
                    Array.Empty<string>());
            }
            var player = PlayerActor();
            foreach (var hostile in localCombatActors.Where(value => value.SideCode ==
                         SimulationLocalCombatCodes.Hostile
                         && value.StateCode == SimulationLocalCombatCodes.Active
                         && value.RangeBandCode != SimulationLocalCombatCodes.Far))
            {
                var recent = localCombatActions.LastOrDefault(value =>
                    value.ActorStableId == player.ActorStableId
                    && combatTick - value.CombatTick <= SimulationLocalCombatCodes.GuardWindowTicks);
                var damage = recent?.ActionCode == SimulationLocalCombatCodes.Guard
                    ? ApplyControlCardModifier(player.ActorStableId, 40, false,
                        true, out _, "DirectGuardEfficiency")
                    : recent?.ActionCode == SimulationLocalCombatCodes.Dodge
                      && recent.ResultCode == "DodgeSucceeded" ? 0
                    : recent?.ActionCode == SimulationLocalCombatCodes.HoldPosition
                        ? ApplyControlCardModifier(player.ActorStableId, 45, true,
                            true, out _) : 80;
                player.HealthPermille = Math.Max(0, player.HealthPermille - damage);
                AddLocalAction("ai:" + combatTick + ":" + hostile.ActorStableId,
                    hostile.ActorStableId, player.ActorStableId,
                    SimulationLocalCombatCodes.BasicAttack,
                    damage == 0 ? "Dodged" : "Hit", -damage, 0, 0,
                    Array.Empty<string>());
                if (player.HealthPermille == 0)
                {
                    player.StateCode = SimulationLocalCombatCodes.Defeated;
                    localStateCode = SimulationLocalCombatCodes.Defeated;
                    ResolveLocalOutcome(false, false);
                    break;
                }
            }
            if (!localCombatActors.Any(value => value.SideCode ==
                    SimulationLocalCombatCodes.Hostile
                    && value.StateCode == SimulationLocalCombatCodes.Active))
                ResolveLocalOutcome(true, false);
            localHostileTelegraphActive = false;
        }

        private void ResolveLocalOutcome(bool victory, bool retreated)
        {
            if (phaseCode == SimulationBattleInstanceCodes.Completed) return;
            var injured = localCombatActors.Count(value =>
                value.SideCode != SimulationLocalCombatCodes.Hostile
                && value.HealthPermille < 500);
            outcome = new SimulationBattleOutcomeSnapshot
            {
                OutcomeStableId = "battle-outcome:" + BattleStableId,
                ResultCode = victory ? SimulationBattleInstanceCodes.Victory
                    : SimulationBattleInstanceCodes.Defeat,
                CompletedWorldTick = context.StartedWorldTick,
                AlliedStrengthDelta = victory ? 0 : -1,
                RecoverableInjuryCount = Math.Max(injured, victory ? 0 : 1),
                SecurityDelta = victory ? 1 : retreated ? -1 : -2,
                MoraleDelta = victory ? 1 : -2,
                UsedDeterministicAutoCommand = false,
            };
            semanticEffects.Clear();
            semanticEffects.AddRange(BuildSemanticEffects(outcome));
            phaseCode = SimulationBattleInstanceCodes.Completed;
        }

        private void EnsureLocalActive()
        {
            if (context.CombatSpaceCode != SimulationLocalCombatCodes.WorldLocal
                || phaseCode != SimulationBattleInstanceCodes.Active
                || localStateCode == SimulationLocalCombatCodes.Transitioning)
                throw new SimulationConflictException("SimulationLocalCombatNotActive");
        }

        private void RequireLocalCommander(string actorStableId)
        {
            EnsureLocalActive();
            if (!IsCommander(actorStableId))
                throw new SimulationConflictException("SimulationBattleCommanderRequired");
        }

        private SimulationLocalCombatActorSnapshot PlayerActor() =>
            localCombatActors.First(value => value.SideCode == SimulationLocalCombatCodes.Player);

        private SimulationLocalCombatActorSnapshot FindActiveLocalActor(string actorId,
            string sideCode)
        {
            var actor = localCombatActors.FirstOrDefault(value =>
                value.ActorStableId == (actorId ?? string.Empty).Trim()
                && value.SideCode == sideCode
                && value.StateCode == SimulationLocalCombatCodes.Active);
            return actor ?? throw new SimulationConflictException(
                "SimulationLocalCombatTargetUnavailable");
        }

        private int ApplyControlCardModifier(string actorId, int baseValue,
            bool tactical, bool reducesIncoming, out string[] appliedCodes,
            string directModifierCode = "")
        {
            var modifiers = context.UnitRoster.CardModifiers.Where(value =>
                    value.ApplicableControlModeCode == (tactical
                        ? SimulationLocalCombatCodes.TacticalCommand
                        : SimulationLocalCombatCodes.DirectAction)
                    && (tactical || value.ActorStableId == actorId))
                .Where(value => tactical || string.IsNullOrWhiteSpace(directModifierCode)
                    || value.ModifierCode == directModifierCode)
                .OrderBy(value => value.CardCopyStableId, StringComparer.Ordinal)
                .ThenBy(value => value.ModifierCode, StringComparer.Ordinal).ToArray();
            appliedCodes = modifiers.Select(value => value.ModifierCode)
                .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();
            var basisPoints = Math.Max(-2000, Math.Min(2000,
                modifiers.Sum(value => value.BasisPoints)));
            if (reducesIncoming) basisPoints = -basisPoints;
            return Math.Max(1, (int)Math.Round(baseValue *
                (10000m + basisPoints) / 10000m, MidpointRounding.AwayFromZero));
        }

        private int ControlModeCooldownTicks()
        {
            if (localControlModeCode != SimulationLocalCombatCodes.TacticalCommand)
                return SimulationLocalCombatCodes.DefaultActionCooldownTicks;
            var basisPoints = Math.Max(-2000, Math.Min(2000,
                context.UnitRoster.CardModifiers.Where(value =>
                        value.ApplicableControlModeCode ==
                        SimulationLocalCombatCodes.TacticalCommand)
                    .Sum(value => value.BasisPoints)));
            return Math.Max(3, (int)Math.Round(
                SimulationLocalCombatCodes.DefaultActionCooldownTicks
                * (10000m - basisPoints) / 10000m,
                MidpointRounding.AwayFromZero));
        }

        private string[] ActiveCardModifierCodes() => context.UnitRoster.CardModifiers
            .Where(value => value.ApplicableControlModeCode == localControlModeCode
                && (localControlModeCode == SimulationLocalCombatCodes.TacticalCommand
                    || value.ActorStableId == PlayerActor().ActorStableId))
            .Select(value => value.ModifierCode).Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal).ToArray();

        private void AddLocalAction(string commandId, string actorId, string targetId,
            string actionCode, string resultCode, int healthDelta, int staminaDelta,
            int reactionOffsetMs, string[] appliedCardModifierCodes)
        {
            localCombatActions.Add(new SimulationLocalCombatActionSnapshot
            {
                ActionStableId = string.Concat("local-action:", BattleStableId, ":",
                    localCombatActions.Count.ToString("D4", CultureInfo.InvariantCulture)),
                CommandId = commandId,
                CombatTick = combatTick,
                ActorStableId = actorId,
                TargetActorStableId = targetId,
                ActionCode = actionCode,
                ResultCode = resultCode,
                HealthDeltaPermille = healthDelta,
                StaminaDeltaPermille = staminaDelta,
                ReactionOffsetMs = reactionOffsetMs,
                AppliedCardModifierHashSha256 = context.UnitRoster.CardModifierHashSha256,
                ControlModeCode = localControlModeCode,
                AppliedCardModifierCodes = appliedCardModifierCodes?.ToArray()
                    ?? Array.Empty<string>(),
            });
        }

        private void EnsureActionAllowedForControlMode(string action)
        {
            var allowed = localControlModeCode == SimulationLocalCombatCodes.DirectAction
                ? action != SimulationLocalCombatCodes.HoldPosition
                : action == SimulationLocalCombatCodes.BasicAttack
                  || action == SimulationLocalCombatCodes.Approach
                  || action == SimulationLocalCombatCodes.Retreat
                  || action == SimulationLocalCombatCodes.HoldPosition;
            if (!allowed)
                throw new SimulationConflictException(
                    "SimulationLocalCombatActionNotAllowedForControlMode");
        }

        private static bool KnownControlMode(string value) =>
            value == SimulationLocalCombatCodes.DirectAction
            || value == SimulationLocalCombatCodes.TacticalCommand;

        private static bool KnownLocalAction(string value) => new[]
        {
            SimulationLocalCombatCodes.BasicAttack,
            SimulationLocalCombatCodes.Guard,
            SimulationLocalCombatCodes.Counter,
            SimulationLocalCombatCodes.Dodge,
            SimulationLocalCombatCodes.Approach,
            SimulationLocalCombatCodes.Retreat,
            SimulationLocalCombatCodes.RoleCardSkill,
            SimulationLocalCombatCodes.HoldPosition,
        }.Contains(value ?? string.Empty, StringComparer.Ordinal);

        private static SimulationLocalCombatActorSnapshot NewLocalActor(string id,
            string side, string threat, IEnumerable<string> roles, string range) => new()
            {
                ActorStableId = id,
                SideCode = side,
                ThreatTypeCode = threat ?? string.Empty,
                RoleCodes = (roles ?? Array.Empty<string>()).ToArray(),
                RangeBandCode = range,
            };

        private SimulationLocalCombatStateSnapshot CreateLocalCombatSnapshot() => new()
        {
            FrozenWorldTick = context.StartedWorldTick,
            FrozenWorldRevision = context.StartedWorldRevision,
            StateCode = localStateCode,
            FocusedTargetStableId = localFocusedTarget,
            ControlModeCode = localControlModeCode,
            ActiveCardModifierCodes = ActiveCardModifierCodes(),
            WorldContext = CloneLocalWorldContext(context.LocalWorldContext),
            Actors = localCombatActors.Select(CloneLocalActor).ToArray(),
            Actions = localCombatActions.Select(CloneLocalAction).ToArray(),
            EscalationRequired = localEscalationRequired,
            EscalationReasonCodes = localEscalationReasons.ToArray(),
            HostileTelegraphActive = localHostileTelegraphActive,
            HostileTelegraphOpenedCombatTick = localHostileTelegraphOpenedCombatTick,
        };

        private void RestoreLocalCombat(SimulationLocalCombatStateSnapshot value)
        {
            localCombatActors.Clear();
            localCombatActions.Clear();
            if (value == null) return;
            localCombatActors.AddRange((value.Actors ?? Array.Empty<SimulationLocalCombatActorSnapshot>())
                .Select(CloneLocalActor));
            localCombatActions.AddRange((value.Actions ?? Array.Empty<SimulationLocalCombatActionSnapshot>())
                .Select(CloneLocalAction));
            localFocusedTarget = value.FocusedTargetStableId ?? string.Empty;
            localStateCode = value.StateCode ?? SimulationLocalCombatCodes.Active;
            localControlModeCode = KnownControlMode(value.ControlModeCode)
                ? value.ControlModeCode : SimulationLocalCombatCodes.DirectAction;
            localEscalationRequired = value.EscalationRequired;
            localEscalationReasons = value.EscalationReasonCodes?.ToArray()
                ?? Array.Empty<string>();
            localHostileTelegraphActive = value.HostileTelegraphActive;
            localHostileTelegraphOpenedCombatTick = value.HostileTelegraphOpenedCombatTick;
        }

        private static SimulationLocalCombatStateSnapshot CloneLocalCombat(
            SimulationLocalCombatStateSnapshot value) => new()
            {
                RuleRevision = value?.RuleRevision ?? SimulationLocalCombatCodes.RuleRevision,
                FrozenWorldTick = value?.FrozenWorldTick ?? 0,
                FrozenWorldRevision = value?.FrozenWorldRevision ?? 0,
                StateCode = value?.StateCode ?? SimulationLocalCombatCodes.Active,
                FocusedTargetStableId = value?.FocusedTargetStableId ?? string.Empty,
                ControlModeCode = KnownControlMode(value?.ControlModeCode ?? string.Empty)
                    ? value!.ControlModeCode : SimulationLocalCombatCodes.DirectAction,
                ActiveCardModifierCodes = value?.ActiveCardModifierCodes?.ToArray()
                    ?? Array.Empty<string>(),
                WorldContext = CloneLocalWorldContext(value?.WorldContext ?? new()),
                Actors = (value?.Actors ?? Array.Empty<SimulationLocalCombatActorSnapshot>())
                    .Select(CloneLocalActor).ToArray(),
                Actions = (value?.Actions ?? Array.Empty<SimulationLocalCombatActionSnapshot>())
                    .Select(CloneLocalAction).ToArray(),
                EscalationRequired = value?.EscalationRequired ?? false,
                EscalationReasonCodes = value?.EscalationReasonCodes?.ToArray()
                    ?? Array.Empty<string>(),
                HostileTelegraphActive = value?.HostileTelegraphActive ?? false,
                HostileTelegraphOpenedCombatTick =
                    value?.HostileTelegraphOpenedCombatTick ?? 0,
            };

        private static SimulationLocalCombatActorSnapshot CloneLocalActor(
            SimulationLocalCombatActorSnapshot value) => new()
            {
                ActorStableId = value.ActorStableId, SideCode = value.SideCode,
                ThreatTypeCode = value.ThreatTypeCode, HealthPermille = value.HealthPermille,
                StaminaPermille = value.StaminaPermille, RangeBandCode = value.RangeBandCode,
                FocusedTargetStableId = value.FocusedTargetStableId,
                StateCode = value.StateCode, NextActionCombatTick = value.NextActionCombatTick,
                RoleCodes = value.RoleCodes?.ToArray() ?? Array.Empty<string>(),
            };

        private static SimulationLocalCombatActionSnapshot CloneLocalAction(
            SimulationLocalCombatActionSnapshot value) => new()
            {
                ActionStableId = value.ActionStableId, CommandId = value.CommandId,
                CombatTick = value.CombatTick, ActorStableId = value.ActorStableId,
                TargetActorStableId = value.TargetActorStableId, ActionCode = value.ActionCode,
                ResultCode = value.ResultCode, HealthDeltaPermille = value.HealthDeltaPermille,
                StaminaDeltaPermille = value.StaminaDeltaPermille,
                ReactionOffsetMs = value.ReactionOffsetMs,
                AppliedCardModifierHashSha256 = value.AppliedCardModifierHashSha256,
                ControlModeCode = value.ControlModeCode,
                AppliedCardModifierCodes = value.AppliedCardModifierCodes?.ToArray()
                    ?? Array.Empty<string>(),
            };

        private static SimulationLocalCombatWorldContextSnapshot CloneLocalWorldContext(
            SimulationLocalCombatWorldContextSnapshot value) => new()
            {
                WorldLayoutStableId = value?.WorldLayoutStableId ?? string.Empty,
                WorldLayoutRevision = value?.WorldLayoutRevision ?? 0,
                WorldLayoutHashSha256 = value?.WorldLayoutHashSha256 ?? string.Empty,
                AreaSetInstanceStableId = value?.AreaSetInstanceStableId ?? string.Empty,
                H3Ref = value?.H3Ref ?? string.Empty, H2Ref = value?.H2Ref ?? string.Empty,
                H1Ref = value?.H1Ref ?? string.Empty,
                FocusL3CellKey = value?.FocusL3CellKey ?? string.Empty,
                RetreatConnectorStableId = value?.RetreatConnectorStableId ?? string.Empty,
                ContextHashSha256 = value?.ContextHashSha256 ?? string.Empty,
                PinsDetailWindow = value?.PinsDetailWindow ?? true,
                PinsActiveWindow = value?.PinsActiveWindow ?? true,
            };

        private static void AddLocalWorldContextCanonical(StringBuilder target,
            SimulationLocalCombatWorldContextSnapshot value)
        {
            AddCanonical(target, value.WorldLayoutStableId);
            AddCanonical(target, value.WorldLayoutRevision);
            AddCanonical(target, value.WorldLayoutHashSha256);
            AddCanonical(target, value.AreaSetInstanceStableId);
            AddCanonical(target, value.H3Ref); AddCanonical(target, value.H2Ref);
            AddCanonical(target, value.H1Ref); AddCanonical(target, value.FocusL3CellKey);
            AddCanonical(target, value.RetreatConnectorStableId);
            AddCanonical(target, value.ContextHashSha256);
            AddCanonical(target, value.PinsDetailWindow);
            AddCanonical(target, value.PinsActiveWindow);
        }

        private static void AddLocalCombatCanonical(StringBuilder target,
            SimulationLocalCombatStateSnapshot value)
        {
            AddCanonical(target, value.RuleRevision); AddCanonical(target, value.FrozenWorldTick);
            AddCanonical(target, value.FrozenWorldRevision); AddCanonical(target, value.StateCode);
            AddCanonical(target, value.FocusedTargetStableId);
            AddCanonical(target, value.ControlModeCode);
            foreach (var card in value.ActiveCardModifierCodes.OrderBy(item => item,
                         StringComparer.Ordinal)) AddCanonical(target, card);
            AddLocalWorldContextCanonical(target, value.WorldContext);
            foreach (var actor in value.Actors.OrderBy(item => item.ActorStableId,
                         StringComparer.Ordinal))
            {
                AddCanonical(target, actor.ActorStableId); AddCanonical(target, actor.SideCode);
                AddCanonical(target, actor.ThreatTypeCode); AddCanonical(target, actor.HealthPermille);
                AddCanonical(target, actor.StaminaPermille); AddCanonical(target, actor.RangeBandCode);
                AddCanonical(target, actor.FocusedTargetStableId); AddCanonical(target, actor.StateCode);
                AddCanonical(target, actor.NextActionCombatTick);
                foreach (var role in actor.RoleCodes.OrderBy(item => item,
                             StringComparer.Ordinal)) AddCanonical(target, role);
            }
            foreach (var action in value.Actions)
            {
                AddCanonical(target, action.ActionStableId); AddCanonical(target, action.CommandId);
                AddCanonical(target, action.CombatTick); AddCanonical(target, action.ActorStableId);
                AddCanonical(target, action.TargetActorStableId); AddCanonical(target, action.ActionCode);
                AddCanonical(target, action.ResultCode); AddCanonical(target, action.HealthDeltaPermille);
                AddCanonical(target, action.StaminaDeltaPermille); AddCanonical(target, action.ReactionOffsetMs);
                AddCanonical(target, action.AppliedCardModifierHashSha256);
                AddCanonical(target, action.ControlModeCode);
                foreach (var card in action.AppliedCardModifierCodes.OrderBy(item => item,
                             StringComparer.Ordinal)) AddCanonical(target, card);
            }
            AddCanonical(target, value.EscalationRequired);
            foreach (var reason in value.EscalationReasonCodes.OrderBy(item => item,
                         StringComparer.Ordinal)) AddCanonical(target, reason);
            AddCanonical(target, value.HostileTelegraphActive);
            AddCanonical(target, value.HostileTelegraphOpenedCombatTick);
        }
    }
}
