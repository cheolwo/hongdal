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
        private string localRuleRevision = SimulationLocalCombatCodes.RuleRevision;
        private bool localParticipationModeLocked;
        private bool localObserverTacticalPauseActive;
        private bool localObserverInterventionConsumed;
        private string localObserverActivatedCardCopyStableId = string.Empty;
        private string localObserverActivatedModifierCode = string.Empty;
        private int localObserverAutomaticActionCount;
        private SimulationLocalCombatPerformanceSnapshot localPerformance = new();

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
                        if (localParticipationModeLocked || combatTick != 0
                            || localCombatActions.Count != 0)
                            throw new SimulationConflictException(
                                "SimulationLocalCombatParticipationModeLocked");
                        if (!string.IsNullOrWhiteSpace(
                                request.ExpectedCardLoadoutHashSha256)
                            && !string.Equals(request.ExpectedCardLoadoutHashSha256.Trim(),
                                context.UnitRoster.CardModifierHashSha256,
                                StringComparison.Ordinal))
                            throw new SimulationConflictException(
                                "SimulationLocalCombatCardLoadoutChanged");
                        localControlModeCode = request.ControlModeCode.Trim();
                        localParticipationModeLocked = true;
                    });
            }
        }

        public SimulationBattleInstanceSnapshot ConfirmObserverIntervention(
            SimulationLocalCombatObserverInterventionConfirmRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CommandId)
                || string.IsNullOrWhiteSpace(request.RequestingActorStableId)
                || (request.ActionCode !=
                        SimulationLocalCombatCodes.PauseObserverIntervention
                    && request.ActionCode !=
                        SimulationLocalCombatCodes.ActivateObserverCard
                    && request.ActionCode !=
                        SimulationLocalCombatCodes.SkipObserverIntervention))
                throw new SimulationContractException(
                    "SimulationLocalCombatObserverInterventionInvalid");
            lock (gate)
            {
                return Apply(request.CommandId, request.ExpectedBattleRevision,
                    string.Join("~", request.RequestingActorStableId.Trim(),
                        request.ActionCode.Trim(), request.CardCopyStableId?.Trim()
                            ?? string.Empty), () => ResolveObserverIntervention(request));
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
                    if (localObserverTacticalPauseActive)
                        throw new SimulationConflictException(
                            "SimulationLocalCombatObserverTacticalPauseActive");
                    localParticipationModeLocked = true;
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
            localParticipationModeLocked = true;
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
            if (localControlModeCode == SimulationLocalCombatCodes.ObserverOperation)
                ResolveObserverPlayerAiTick();

            if (localControlModeCode == SimulationLocalCombatCodes.DirectAction
                || localControlModeCode == SimulationLocalCombatCodes.ObserverOperation)
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
                if (localControlModeCode == SimulationLocalCombatCodes.ObserverOperation)
                    damage = ApplyObserverIncomingDamage(damage);
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
                UsedDeterministicAutoCommand = localControlModeCode ==
                    SimulationLocalCombatCodes.ObserverOperation,
            };
            localPerformance = CalculateDirectPerformance(victory);
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

        private void ResolveObserverPlayerAiTick()
        {
            var player = PlayerActor();
            if (player.StateCode != SimulationLocalCombatCodes.Active
                || player.NextActionCombatTick > combatTick) return;
            var modifiers = ObserverModifiers();
            var cautious = modifiers.Any(value => value.ModifierCode ==
                SimulationLocalCombatCodes.ObserverCautiousDefense);
            if (localHostileTelegraphActive && cautious && combatTick % 10 == 0)
            {
                player.StaminaPermille = Math.Max(0, player.StaminaPermille - 50);
                AddLocalAction("ai:" + combatTick + ":observer:guard",
                    player.ActorStableId, string.Empty, SimulationLocalCombatCodes.Guard,
                    "ObserverGuardWindowOpened", 0, -50, 0,
                    new[] { SimulationLocalCombatCodes.ObserverCautiousDefense });
                player.NextActionCombatTick = combatTick
                    + SimulationLocalCombatCodes.DefaultActionCooldownTicks;
                localObserverAutomaticActionCount++;
                return;
            }

            var target = SelectObserverTarget(modifiers);
            if (target == null) return;
            if (target.RangeBandCode == SimulationLocalCombatCodes.Far)
            {
                target.RangeBandCode = SimulationLocalCombatCodes.Near;
                AddLocalAction("ai:" + combatTick + ":observer:approach",
                    player.ActorStableId, target.ActorStableId,
                    SimulationLocalCombatCodes.Approach, "ObserverRangeClosed",
                    0, 0, 0, ObserverModifierCodes(modifiers));
            }
            else
            {
                var damage = ApplyObserverOutgoingDamage(200);
                target.HealthPermille = Math.Max(0, target.HealthPermille - damage);
                if (target.HealthPermille == 0)
                    target.StateCode = SimulationLocalCombatCodes.Defeated;
                AddLocalAction("ai:" + combatTick + ":observer:attack",
                    player.ActorStableId, target.ActorStableId,
                    SimulationLocalCombatCodes.BasicAttack,
                    target.StateCode == SimulationLocalCombatCodes.Defeated
                        ? "TargetDefeated" : "ObserverHit", -damage, 0, 0,
                    ObserverModifierCodes(modifiers));
            }
            player.NextActionCombatTick = combatTick
                + SimulationLocalCombatCodes.DefaultActionCooldownTicks;
            localObserverAutomaticActionCount++;
        }

        private SimulationLocalCombatActorSnapshot? SelectObserverTarget(
            SimulationBattleCardModifierSnapshot[] modifiers)
        {
            var targets = localCombatActors.Where(value => value.SideCode ==
                    SimulationLocalCombatCodes.Hostile
                && value.StateCode == SimulationLocalCombatCodes.Active);
            return modifiers.Any(value => value.ModifierCode ==
                    SimulationLocalCombatCodes.ObserverWeaknessObservation)
                ? targets.OrderBy(value => value.HealthPermille)
                    .ThenBy(value => value.ActorStableId, StringComparer.Ordinal)
                    .FirstOrDefault()
                : targets.OrderBy(value => value.ActorStableId,
                    StringComparer.Ordinal).FirstOrDefault();
        }

        private int ApplyObserverOutgoingDamage(int baseValue)
        {
            var codes = ObserverModifiers().Select(value => value.ModifierCode).ToArray();
            var basisPoints = 0;
            if (codes.Contains(SimulationLocalCombatCodes.ObserverFocusedAssault,
                    StringComparer.Ordinal)) basisPoints += 1500;
            if (codes.Contains(SimulationLocalCombatCodes.ObserverCautiousDefense,
                    StringComparer.Ordinal)) basisPoints -= 1000;
            return ApplyBasisPoints(baseValue, basisPoints);
        }

        private int ApplyObserverIncomingDamage(int baseValue)
        {
            var codes = ObserverModifiers().Select(value => value.ModifierCode).ToArray();
            var basisPoints = 0;
            if (codes.Contains(SimulationLocalCombatCodes.ObserverFocusedAssault,
                    StringComparer.Ordinal)) basisPoints += 1000;
            if (codes.Contains(SimulationLocalCombatCodes.ObserverCautiousDefense,
                    StringComparer.Ordinal)) basisPoints -= 2000;
            if (codes.Contains(SimulationLocalCombatCodes.ObserverCabinCover,
                    StringComparer.Ordinal)
                && context.ScaleReasonCodes.Contains("CabinDefenseApplied",
                    StringComparer.Ordinal)) basisPoints -= 1500;
            return ApplyBasisPoints(baseValue, basisPoints);
        }

        private static int ApplyBasisPoints(int baseValue, int basisPoints)
        {
            var clamped = Math.Max(-3000, Math.Min(3000, basisPoints));
            return Math.Max(1, (int)Math.Round(baseValue *
                (10000m + clamped) / 10000m, MidpointRounding.AwayFromZero));
        }

        private SimulationBattleCardModifierSnapshot[] ObserverModifiers()
            => context.UnitRoster.CardModifiers.Where(value =>
                    value.ApplicableControlModeCode ==
                        SimulationLocalCombatCodes.ObserverOperation
                    && value.ActorStableId == PlayerActor().ActorStableId)
                .OrderBy(value => value.CardCopyStableId, StringComparer.Ordinal)
                .ThenBy(value => value.ModifierCode, StringComparer.Ordinal).ToArray();

        private static string[] ObserverModifierCodes(
            IEnumerable<SimulationBattleCardModifierSnapshot> values)
            => values.Select(value => value.ModifierCode)
                .Distinct(StringComparer.Ordinal).OrderBy(value => value,
                    StringComparer.Ordinal).ToArray();

        private void ResolveObserverIntervention(
            SimulationLocalCombatObserverInterventionConfirmRequest request)
        {
            if (context.CombatSpaceCode != SimulationLocalCombatCodes.WorldLocal
                || phaseCode != SimulationBattleInstanceCodes.Active
                || localControlModeCode != SimulationLocalCombatCodes.ObserverOperation
                || !IsCommander(request.RequestingActorStableId))
                throw new SimulationConflictException(
                    "SimulationLocalCombatObserverInterventionUnavailable");
            localParticipationModeLocked = true;
            if (localObserverInterventionConsumed)
                throw new SimulationConflictException(
                    "SimulationLocalCombatObserverInterventionConsumed");
            if (request.ActionCode ==
                SimulationLocalCombatCodes.PauseObserverIntervention)
            {
                if (localObserverTacticalPauseActive)
                    throw new SimulationConflictException(
                        "SimulationLocalCombatObserverTacticalPauseActive");
                localObserverTacticalPauseActive = true;
                localStateCode = SimulationLocalCombatCodes.ObserverPaused;
                return;
            }
            if (!localObserverTacticalPauseActive)
                throw new SimulationConflictException(
                    "SimulationLocalCombatObserverTacticalPauseRequired");

            if (request.ActionCode ==
                SimulationLocalCombatCodes.SkipObserverIntervention)
            {
                localObserverInterventionConsumed = true;
                localObserverTacticalPauseActive = false;
                localStateCode = SimulationLocalCombatCodes.Active;
                AddLocalAction(request.CommandId, PlayerActor().ActorStableId,
                    string.Empty, request.ActionCode, "ObserverInterventionSkipped",
                    0, 0, 0, Array.Empty<string>());
                return;
            }

            var modifier = ObserverModifiers().SingleOrDefault(value =>
                value.CardCopyStableId == request.CardCopyStableId?.Trim()
                && (value.ModifierCode ==
                        SimulationLocalCombatCodes.ObserverFieldRecovery
                    || value.ModifierCode ==
                        SimulationLocalCombatCodes.ObserverSafeRetreat));
            if (modifier == null)
                throw new SimulationConflictException(
                    "SimulationLocalCombatObserverEmergencyCardUnavailable");
            localObserverInterventionConsumed = true;
            localObserverTacticalPauseActive = false;
            localObserverActivatedCardCopyStableId = modifier.CardCopyStableId;
            localObserverActivatedModifierCode = modifier.ModifierCode;
            var player = PlayerActor();
            if (modifier.ModifierCode ==
                SimulationLocalCombatCodes.ObserverSafeRetreat)
            {
                player.StateCode = SimulationLocalCombatCodes.Retreated;
                player.RangeBandCode = SimulationLocalCombatCodes.RetreatBoundary;
                localStateCode = SimulationLocalCombatCodes.Retreated;
                AddLocalAction(request.CommandId, player.ActorStableId, string.Empty,
                    request.ActionCode, "ObserverSafeRetreatApplied", 0, 0, 0,
                    new[] { modifier.ModifierCode });
                ResolveLocalOutcome(false, true);
                return;
            }
            var before = player.HealthPermille;
            player.HealthPermille = Math.Min(1000, player.HealthPermille
                + SimulationLocalCombatCodes.ObserverRecoveryPermille);
            localStateCode = SimulationLocalCombatCodes.Active;
            AddLocalAction(request.CommandId, player.ActorStableId,
                player.ActorStableId, request.ActionCode,
                "ObserverFieldRecoveryApplied", player.HealthPermille - before,
                0, 0, new[] { modifier.ModifierCode });
        }

        private SimulationLocalCombatPerformanceSnapshot CalculateDirectPerformance(
            bool victory)
        {
            var player = PlayerActor();
            var hostileCount = localCombatActors.Count(value => value.SideCode ==
                SimulationLocalCombatCodes.Hostile);
            var defenses = localCombatActions.Count(value =>
                value.ActorStableId == player.ActorStableId
                && (value.ActionCode == SimulationLocalCombatCodes.Guard
                    || value.ActionCode == SimulationLocalCombatCodes.Dodge
                       && value.ResultCode == "DodgeSucceeded"
                    || value.ActionCode == SimulationLocalCombatCodes.Counter
                       && value.ReactionOffsetMs <= 200));
            var healthScore = Math.Max(0, Math.Min(500, player.HealthPermille / 2));
            var defenseScore = Math.Min(250, defenses * 50);
            var speedScore = Math.Max(0, 250 - Math.Max(0,
                combatTick - hostileCount * 20) * 5);
            var total = Math.Max(0, Math.Min(1000,
                healthScore + defenseScore + speedScore));
            var grade = victory && localControlModeCode ==
                SimulationLocalCombatCodes.DirectAction
                ? total >= SimulationLocalCombatCodes.PerformanceGradeSThreshold
                    ? SimulationLocalCombatCodes.GradeS
                    : total >= SimulationLocalCombatCodes.PerformanceGradeAThreshold
                        ? SimulationLocalCombatCodes.GradeA
                        : SimulationLocalCombatCodes.GradeB
                : string.Empty;
            return new SimulationLocalCombatPerformanceSnapshot
            {
                IsFinal = true,
                FinalHealthPermille = player.HealthPermille,
                SuccessfulDefenseCount = defenses,
                ElapsedCombatTicks = combatTick,
                HostileCount = hostileCount,
                HealthScore = healthScore,
                DefenseScore = defenseScore,
                SpeedScore = speedScore,
                TotalScore = total,
                GradeCode = grade,
                RewardBonusQuantity = grade == SimulationLocalCombatCodes.GradeS ? 2
                    : grade == SimulationLocalCombatCodes.GradeA ? 1 : 0,
            };
        }

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
                : localControlModeCode == SimulationLocalCombatCodes.ObserverOperation
                    ? false
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
            || value == SimulationLocalCombatCodes.TacticalCommand
            || value == SimulationLocalCombatCodes.ObserverOperation;

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
            RuleRevision = localRuleRevision,
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
            ParticipationModeLocked = localParticipationModeLocked,
            FrozenCardLoadoutHashSha256 = context.UnitRoster.CardModifierHashSha256,
            ObserverOperation = new SimulationLocalCombatObserverOperationSnapshot
            {
                TacticalPauseActive = localObserverTacticalPauseActive,
                InterventionOpportunityConsumed = localObserverInterventionConsumed,
                ActivatedCardCopyStableId = localObserverActivatedCardCopyStableId,
                ActivatedModifierCode = localObserverActivatedModifierCode,
                AutomaticActionCount = localObserverAutomaticActionCount,
                AvailableEmergencyCardCopyStableIds = ObserverModifiers()
                    .Where(value => value.ModifierCode ==
                            SimulationLocalCombatCodes.ObserverFieldRecovery
                        || value.ModifierCode ==
                            SimulationLocalCombatCodes.ObserverSafeRetreat)
                    .Select(value => value.CardCopyStableId)
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            },
            Performance = CloneLocalPerformance(localPerformance),
        };

        private void RestoreLocalCombat(SimulationLocalCombatStateSnapshot value)
        {
            localCombatActors.Clear();
            localCombatActions.Clear();
            if (value == null) return;
            localRuleRevision = string.IsNullOrWhiteSpace(value.RuleRevision)
                ? SimulationLocalCombatCodes.RuleRevisionR2
                : value.RuleRevision;
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
            localParticipationModeLocked = value.ParticipationModeLocked;
            localObserverTacticalPauseActive = value.ObserverOperation?.TacticalPauseActive
                ?? false;
            localObserverInterventionConsumed = value.ObserverOperation?.InterventionOpportunityConsumed
                ?? false;
            localObserverActivatedCardCopyStableId =
                value.ObserverOperation?.ActivatedCardCopyStableId ?? string.Empty;
            localObserverActivatedModifierCode =
                value.ObserverOperation?.ActivatedModifierCode ?? string.Empty;
            localObserverAutomaticActionCount = value.ObserverOperation?.AutomaticActionCount
                ?? 0;
            localPerformance = CloneLocalPerformance(value.Performance);
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
                ParticipationModeLocked = value?.ParticipationModeLocked ?? false,
                FrozenCardLoadoutHashSha256 = value?.FrozenCardLoadoutHashSha256
                    ?? string.Empty,
                ObserverOperation = new SimulationLocalCombatObserverOperationSnapshot
                {
                    PolicyRevision = value?.ObserverOperation?.PolicyRevision
                        ?? "observer-combat-policy.r1",
                    TacticalPauseActive = value?.ObserverOperation?.TacticalPauseActive
                        ?? false,
                    InterventionOpportunityConsumed =
                        value?.ObserverOperation?.InterventionOpportunityConsumed
                        ?? false,
                    ActivatedCardCopyStableId =
                        value?.ObserverOperation?.ActivatedCardCopyStableId ?? string.Empty,
                    ActivatedModifierCode = value?.ObserverOperation?.ActivatedModifierCode
                        ?? string.Empty,
                    AutomaticActionCount = value?.ObserverOperation?.AutomaticActionCount
                        ?? 0,
                    AvailableEmergencyCardCopyStableIds = value?.ObserverOperation?
                        .AvailableEmergencyCardCopyStableIds?.ToArray()
                        ?? Array.Empty<string>(),
                },
                Performance = CloneLocalPerformance(value?.Performance),
            };

        private static SimulationLocalCombatPerformanceSnapshot CloneLocalPerformance(
            SimulationLocalCombatPerformanceSnapshot? value) => new()
            {
                RuleRevision = value?.RuleRevision ?? "direct-combat-performance.r1",
                IsFinal = value?.IsFinal ?? false,
                FinalHealthPermille = value?.FinalHealthPermille ?? 0,
                SuccessfulDefenseCount = value?.SuccessfulDefenseCount ?? 0,
                ElapsedCombatTicks = value?.ElapsedCombatTicks ?? 0,
                HostileCount = value?.HostileCount ?? 0,
                HealthScore = value?.HealthScore ?? 0,
                DefenseScore = value?.DefenseScore ?? 0,
                SpeedScore = value?.SpeedScore ?? 0,
                TotalScore = value?.TotalScore ?? 0,
                GradeCode = value?.GradeCode ?? string.Empty,
                RewardBonusQuantity = value?.RewardBonusQuantity ?? 0,
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
            if (value.RuleRevision == SimulationLocalCombatCodes.RuleRevisionR3)
            {
                AddCanonical(target, value.ParticipationModeLocked);
                AddCanonical(target, value.FrozenCardLoadoutHashSha256);
                AddCanonical(target, value.ObserverOperation.TacticalPauseActive);
                AddCanonical(target, value.ObserverOperation.PolicyRevision);
                AddCanonical(target, value.ObserverOperation.InterventionOpportunityConsumed);
                AddCanonical(target, value.ObserverOperation.ActivatedCardCopyStableId);
                AddCanonical(target, value.ObserverOperation.ActivatedModifierCode);
                AddCanonical(target, value.ObserverOperation.AutomaticActionCount);
                foreach (var card in value.ObserverOperation
                             .AvailableEmergencyCardCopyStableIds.OrderBy(item => item,
                                 StringComparer.Ordinal))
                    AddCanonical(target, card);
                AddCanonical(target, value.Performance.RuleRevision);
                AddCanonical(target, value.Performance.IsFinal);
                AddCanonical(target, value.Performance.FinalHealthPermille);
                AddCanonical(target, value.Performance.SuccessfulDefenseCount);
                AddCanonical(target, value.Performance.ElapsedCombatTicks);
                AddCanonical(target, value.Performance.HostileCount);
                AddCanonical(target, value.Performance.HealthScore);
                AddCanonical(target, value.Performance.DefenseScore);
                AddCanonical(target, value.Performance.SpeedScore);
                AddCanonical(target, value.Performance.TotalScore);
                AddCanonical(target, value.Performance.GradeCode);
                AddCanonical(target, value.Performance.RewardBonusQuantity);
            }
        }
    }
}
