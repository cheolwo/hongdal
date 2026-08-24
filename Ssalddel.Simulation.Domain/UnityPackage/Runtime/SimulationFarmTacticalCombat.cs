using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private readonly List<SimulationTacticalFrontSnapshot> tacticalFronts =
            new List<SimulationTacticalFrontSnapshot>();
        private readonly List<SimulationTacticalSquadSnapshot> tacticalSquads =
            new List<SimulationTacticalSquadSnapshot>();
        private readonly List<SimulationTacticalOpportunitySnapshot>
            tacticalOpportunities =
                new List<SimulationTacticalOpportunitySnapshot>();
        private readonly List<SimulationTacticalOrderWindowSnapshot>
            tacticalOrderWindows =
                new List<SimulationTacticalOrderWindowSnapshot>();
        private readonly List<SimulationTacticalOrderSnapshot> tacticalOrders =
            new List<SimulationTacticalOrderSnapshot>();
        private readonly List<SimulationTacticalResolutionSnapshot>
            tacticalResolutions =
                new List<SimulationTacticalResolutionSnapshot>();
        private readonly Dictionary<string, AppliedFarmCommand>
            appliedTacticalOrderCommands =
                new Dictionary<string, AppliedFarmCommand>(StringComparer.Ordinal);

        public SimulationTacticalOrderPreviewSnapshot PreviewTacticalOrder(
            SimulationTacticalOrderPreviewRequest request)
        {
            ValidateTacticalOrderPreviewRequest(request);
            lock (gate)
            {
                EnsureHeroTacticalCombatConfigured();
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException(
                        "SimulationExpectedRevisionMismatch");
                return CreateTacticalOrderPreview(request.OrderWindowStableId.Trim(),
                    request.FrontStableId.Trim(), request.ActorStableId.Trim(),
                    request.OrderCode.Trim(), request.OpportunityStableId.Trim());
            }
        }

        public SimulationFarmSurvivalStateSnapshot ConfirmTacticalOrder(
            SimulationTacticalOrderConfirmRequest request)
        {
            ValidateTacticalOrderConfirmRequest(request);
            lock (gate)
            {
                EnsureHeroTacticalCombatConfigured();
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildTacticalOrderPayloadKey(request);
                if (appliedTacticalOrderCommands.TryGetValue(commandId,
                    out var applied))
                    return ResolveAppliedCombatCommand(applied, payloadKey);
                EnsureNewCombatCommand(commandId, request.ExpectedRevision);

                var preview = CreateTacticalOrderPreview(
                    request.OrderWindowStableId.Trim(),
                    request.FrontStableId.Trim(), request.ActorStableId.Trim(),
                    request.OrderCode.Trim(), request.OpportunityStableId.Trim());
                if (!preview.CanConfirm)
                    throw new SimulationConflictException(
                        preview.BlockingReasonCodes.FirstOrDefault()
                        ?? "SimulationTacticalOrderBlocked");

                var window = FindTacticalOrderWindow(preview.OrderWindowStableId);
                var order = new SimulationTacticalOrderSnapshot
                {
                    OrderStableId = "tactical-order:" + commandId,
                    CommandId = commandId,
                    OrderWindowStableId = window.OrderWindowStableId,
                    FrontStableId = window.FrontStableId,
                    ActorStableId = preview.ActorStableId,
                    OrderCode = preview.OrderCode,
                    OpportunityStableId = preview.OpportunityStableId,
                    ConfirmedWorldTick = CurrentTick,
                    ResolvesWorldTick = window.ClosesWorldTick,
                    AutomaticallySelected = false,
                    StateCode = SimulationFarmTacticalCombatCodes.Confirmed,
                    PresentationKey = TacticalOrderPresentationKey(preview.OrderCode),
                };
                tacticalOrders.Add(order);
                window.ConfirmedOrderStableId = order.OrderStableId;
                window.StateCode = SimulationFarmTacticalCombatCodes.Confirmed;
                if (!string.IsNullOrEmpty(order.OpportunityStableId))
                {
                    var opportunity = FindTacticalOpportunity(
                        order.OpportunityStableId);
                    opportunity.StateCode =
                        SimulationFarmTacticalCombatCodes.Reserved;
                    opportunity.ReservedOrderStableId = order.OrderStableId;
                }

                Revision++;
                AppendTacticalOrderConfirmCommand(request);
                return RememberCombatCommand(appliedTacticalOrderCommands,
                    commandId, payloadKey);
            }
        }

        private void PrepareTacticalFront(
            SimulationThreatEncounterSnapshot encounter)
        {
            if (tacticalFronts.Any(value => value.EncounterStableId ==
                encounter.EncounterStableId)) return;

            var frontStableId = "tactical-front:" + encounter.EncounterStableId;
            tacticalFronts.Add(new SimulationTacticalFrontSnapshot
            {
                FrontStableId = frontStableId,
                EncounterStableId = encounter.EncounterStableId,
                AreaStableId = farmSurvivalCreationState!.AreaStableId,
                PositionCode = SimulationFarmTacticalCombatCodes.Perimeter,
                StateCode = SimulationFarmTacticalCombatCodes.Open,
                PresentationKey = "survival.tactical.front.farm-perimeter",
            });

            var npcIds = farmActors.Values
                .Where(value => value.ActorKindCode ==
                    SimulationFarmSurvivalCodes.Npc)
                .Select(value => value.ActorStableId)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();
            tacticalSquads.Add(new SimulationTacticalSquadSnapshot
            {
                SquadStableId = "tactical-squad:allied:" +
                    encounter.EncounterStableId,
                FrontStableId = frontStableId,
                SideCode = SimulationFarmTacticalCombatCodes.Allied,
                PositionCode = SimulationFarmTacticalCombatCodes.Perimeter,
                MemberCount = npcIds.Length,
                CombatStrength = npcIds.Length,
                MemberActorStableIds = npcIds,
                PresentationKey = "survival.tactical.squad.farm-defenders",
            });
            tacticalSquads.Add(new SimulationTacticalSquadSnapshot
            {
                SquadStableId = "tactical-squad:hostile:" +
                    encounter.EncounterStableId,
                FrontStableId = frontStableId,
                SideCode = SimulationFarmTacticalCombatCodes.Hostile,
                PositionCode = SimulationFarmTacticalCombatCodes.Perimeter,
                MemberCount = encounter.ThreatUnitCount,
                CombatStrength = encounter.ThreatUnitCount,
                PresentationKey = "survival.tactical.squad.zombie-pressure",
            });
        }

        private void OpenTacticalOrderWindow(
            SimulationCombatBeatSnapshot beat,
            SimulationCombatReactionSnapshot reaction,
            SimulationThreatEncounterSnapshot encounter)
        {
            PrepareTacticalFront(encounter);
            var front = tacticalFronts.Single(value => value.EncounterStableId ==
                encounter.EncounterStableId);
            var opportunityKind = TacticalOpportunityKind(reaction);
            if (!string.IsNullOrEmpty(opportunityKind))
            {
                tacticalOpportunities.Add(
                    new SimulationTacticalOpportunitySnapshot
                    {
                        OpportunityStableId = "tactical-opportunity:" +
                            reaction.ReactionStableId,
                        EncounterStableId = encounter.EncounterStableId,
                        FrontStableId = front.FrontStableId,
                        SourceReactionStableId = reaction.ReactionStableId,
                        EarningActorStableId = reaction.ActorStableId,
                        OpportunityKindCode = opportunityKind,
                        Quality = reaction.GradeCode ==
                            SimulationFarmCombatCodes.Perfect ? 2 : 1,
                        CreatedWorldTick = CurrentTick,
                        ExpiresWorldTick = CurrentTick + 1,
                        StateCode = SimulationFarmTacticalCombatCodes.Available,
                        PresentationKey = "survival.tactical.opportunity."
                            + opportunityKind.ToLowerInvariant(),
                    });
            }

            tacticalOrderWindows.Add(new SimulationTacticalOrderWindowSnapshot
            {
                OrderWindowStableId = "tactical-window:" + beat.BeatStableId,
                EncounterStableId = encounter.EncounterStableId,
                FrontStableId = front.FrontStableId,
                AuthorizedActorStableId = beat.ActorStableId,
                OpenedWorldTick = CurrentTick,
                ClosesWorldTick = CurrentTick + 1,
                StateCode = SimulationFarmTacticalCombatCodes.Open,
                AllowedOrderCodes = new[]
                {
                    SimulationFarmTacticalCombatCodes.AdvanceAndAttack,
                    SimulationFarmTacticalCombatCodes.HoldFormation,
                    SimulationFarmTacticalCombatCodes.TacticalRetreat,
                },
                PresentationKey = "survival.tactical.order-window.offer",
            });
            encounter.PresentationKey = "survival.tactical.order-window.ready";
            UpdateFarmThreatWorldEvent(encounter);
        }

        private SimulationTacticalOrderPreviewSnapshot CreateTacticalOrderPreview(
            string orderWindowStableId,
            string frontStableId,
            string actorStableId,
            string orderCode,
            string opportunityStableId)
        {
            var blocks = new List<string>();
            var window = tacticalOrderWindows.SingleOrDefault(value =>
                value.OrderWindowStableId == orderWindowStableId);
            var front = tacticalFronts.SingleOrDefault(value =>
                value.FrontStableId == frontStableId);
            SimulationTacticalOpportunitySnapshot? opportunity = null;

            if (!farmActors.TryGetValue(actorStableId, out var actor)
                || actor.ActorKindCode != SimulationFarmSurvivalCodes.Player)
                blocks.Add("SimulationCombatPlayerActorRequired");
            if (window == null)
                blocks.Add("SimulationTacticalOrderWindowNotFound");
            else
            {
                if (window.StateCode != SimulationFarmTacticalCombatCodes.Open)
                    blocks.Add("SimulationTacticalOrderWindowClosed");
                if (window.ClosesWorldTick <= CurrentTick)
                    blocks.Add("SimulationTacticalOrderWindowExpired");
                if (window.FrontStableId != frontStableId)
                    blocks.Add("SimulationTacticalFrontMismatch");
                if (window.AuthorizedActorStableId != actorStableId)
                    blocks.Add("SimulationTacticalOrderActorMismatch");
            }
            if (front == null)
                blocks.Add("SimulationTacticalFrontNotFound");

            if (!string.IsNullOrEmpty(opportunityStableId))
            {
                opportunity = tacticalOpportunities.SingleOrDefault(value =>
                    value.OpportunityStableId == opportunityStableId);
                if (opportunity == null)
                    blocks.Add("SimulationTacticalOpportunityNotFound");
                else
                {
                    if (opportunity.StateCode !=
                        SimulationFarmTacticalCombatCodes.Available
                        || opportunity.ExpiresWorldTick <= CurrentTick)
                        blocks.Add("SimulationTacticalOpportunityUnavailable");
                    if (opportunity.FrontStableId != frontStableId)
                        blocks.Add("SimulationTacticalOpportunityFrontMismatch");
                    if (opportunity.EarningActorStableId != actorStableId)
                        blocks.Add("SimulationTacticalOpportunityActorMismatch");
                    if (!OpportunityMatchesOrder(opportunity, orderCode))
                        blocks.Add("SimulationTacticalOpportunityOrderMismatch");
                }
            }

            var baseScore = orderCode ==
                SimulationFarmTacticalCombatCodes.HoldFormation ? 1 : 0;
            var bonus = opportunity?.Quality ?? 0;
            var preparedness = DefensePreparednessScore();
            var retreat = orderCode ==
                SimulationFarmTacticalCombatCodes.TacticalRetreat;
            var succeeded = !retreat && preparedness + baseScore + bonus >= 2;
            var projectedDamage = retreat || !succeeded ? 20m : 0m;
            var projectedSupplyLoss = retreat || !succeeded
                ? Math.Min(2m, farmSupplyUnits) : 0m;

            return new SimulationTacticalOrderPreviewSnapshot
            {
                OrderWindowStableId = orderWindowStableId,
                FrontStableId = frontStableId,
                ActorStableId = actorStableId,
                OrderCode = orderCode,
                OpportunityStableId = opportunityStableId,
                BaseResponseScore = baseScore,
                OpportunityBonusScore = bonus,
                PreparednessScore = preparedness,
                ProjectedResponseScore = preparedness + baseScore + bonus,
                ProjectedFrontPositionCode = retreat
                    ? SimulationFarmTacticalCombatCodes.InnerFarm
                    : orderCode == SimulationFarmTacticalCombatCodes.AdvanceAndAttack
                        && succeeded
                        ? SimulationFarmTacticalCombatCodes.Forward
                        : SimulationFarmTacticalCombatCodes.Perimeter,
                ProjectedCombatStrengthDelta = !retreat && !succeeded ? -1 : 0,
                ProjectedRecoverableInjuryCount = !retreat && !succeeded ? 1 : 0,
                ProjectedFacilityDamageUnits = projectedDamage,
                ProjectedSupplyLossUnits = projectedSupplyLoss,
                ProjectedDefenseSucceeded = succeeded,
                CanConfirm = blocks.Count == 0,
                BlockingReasonCodes = blocks.ToArray(),
                PresentationKey = TacticalOrderPresentationKey(orderCode),
                SimulationOnly = true,
                IsOperationalState = false,
            };
        }

        private void AdvanceFarmTacticalCombat(int currentTick)
        {
            if (!UsesHeroTacticalCombatRule()) return;
            foreach (var window in tacticalOrderWindows.Where(value =>
                (value.StateCode == SimulationFarmTacticalCombatCodes.Open
                    || value.StateCode == SimulationFarmTacticalCombatCodes.Confirmed)
                && value.ClosesWorldTick <= currentTick)
                .OrderBy(value => value.ClosesWorldTick)
                .ThenBy(value => value.OrderWindowStableId,
                    StringComparer.Ordinal).ToArray())
            {
                SimulationTacticalOrderSnapshot order;
                if (window.StateCode == SimulationFarmTacticalCombatCodes.Open)
                {
                    order = CreateAutomaticHoldOrder(window);
                    tacticalOrders.Add(order);
                    window.ConfirmedOrderStableId = order.OrderStableId;
                    window.StateCode = SimulationFarmTacticalCombatCodes.Confirmed;
                }
                else
                {
                    order = tacticalOrders.Single(value => value.OrderStableId ==
                        window.ConfirmedOrderStableId);
                }
                ResolveTacticalOrder(window, order, currentTick);
            }
        }

        private SimulationTacticalOrderSnapshot CreateAutomaticHoldOrder(
            SimulationTacticalOrderWindowSnapshot window)
            => new SimulationTacticalOrderSnapshot
            {
                OrderStableId = "tactical-order:auto-hold:" +
                    window.OrderWindowStableId,
                CommandId = "world-tick:auto-hold:" + window.OrderWindowStableId,
                OrderWindowStableId = window.OrderWindowStableId,
                FrontStableId = window.FrontStableId,
                ActorStableId = window.AuthorizedActorStableId,
                OrderCode = SimulationFarmTacticalCombatCodes.HoldFormation,
                ConfirmedWorldTick = window.ClosesWorldTick,
                ResolvesWorldTick = window.ClosesWorldTick,
                AutomaticallySelected = true,
                StateCode = SimulationFarmTacticalCombatCodes.Confirmed,
                PresentationKey = "survival.tactical.order.auto-hold",
            };

        private void ResolveTacticalOrder(
            SimulationTacticalOrderWindowSnapshot window,
            SimulationTacticalOrderSnapshot order,
            int currentTick)
        {
            var front = tacticalFronts.Single(value => value.FrontStableId ==
                window.FrontStableId);
            var encounter = FindZombieCombatEncounter(window.EncounterStableId);
            var allied = tacticalSquads.Single(value => value.FrontStableId ==
                front.FrontStableId && value.SideCode ==
                    SimulationFarmTacticalCombatCodes.Allied);
            var hostile = tacticalSquads.Single(value => value.FrontStableId ==
                front.FrontStableId && value.SideCode ==
                    SimulationFarmTacticalCombatCodes.Hostile);
            var opportunity = string.IsNullOrEmpty(order.OpportunityStableId)
                ? null : FindTacticalOpportunity(order.OpportunityStableId);
            var baseScore = order.OrderCode ==
                SimulationFarmTacticalCombatCodes.HoldFormation ? 1 : 0;
            var bonus = opportunity?.Quality ?? 0;
            var preparedness = DefensePreparednessScore();
            var retreat = order.OrderCode ==
                SimulationFarmTacticalCombatCodes.TacticalRetreat;
            var success = !retreat && preparedness + baseScore + bonus >= 2;
            var combatStrengthDelta = 0;
            var injuryCount = 0;

            if (retreat)
            {
                ApplyTacticalWithdrawal(encounter);
                front.PositionCode = SimulationFarmTacticalCombatCodes.InnerFarm;
                allied.PositionCode = SimulationFarmTacticalCombatCodes.InnerFarm;
            }
            else
            {
                var injuredActorStableId = string.Empty;
                if (!success)
                {
                    combatStrengthDelta = -1;
                    allied.CombatStrength = Math.Max(0,
                        allied.CombatStrength + combatStrengthDelta);
                    var injured = allied.MemberActorStableIds
                        .Select(id => farmActors[id])
                        .FirstOrDefault(value => !value.Injured);
                    if (injured != null)
                    {
                        injured.Injured = true;
                        injured.Health = Math.Max(1m, injured.Health - 10m);
                        injuredActorStableId = injured.ActorStableId;
                        allied.RecoverableInjuryCount++;
                        injuryCount = 1;
                    }
                }
                ResolveZombieCombatEncounter(encounter, baseScore + bonus,
                    injuredActorStableId);
                if (success)
                {
                    hostile.CombatStrength = 0;
                    if (order.OrderCode ==
                        SimulationFarmTacticalCombatCodes.AdvanceAndAttack)
                    {
                        front.PositionCode =
                            SimulationFarmTacticalCombatCodes.Forward;
                        allied.PositionCode =
                            SimulationFarmTacticalCombatCodes.Forward;
                    }
                }
            }

            if (opportunity != null)
            {
                opportunity.StateCode = SimulationFarmTacticalCombatCodes.Consumed;
                opportunity.ReservedOrderStableId = order.OrderStableId;
            }
            foreach (var unused in tacticalOpportunities.Where(value =>
                value.FrontStableId == front.FrontStableId
                && value.StateCode == SimulationFarmTacticalCombatCodes.Available))
                unused.StateCode = SimulationFarmTacticalCombatCodes.Expired;

            order.StateCode = SimulationFarmTacticalCombatCodes.Resolved;
            window.StateCode = SimulationFarmTacticalCombatCodes.Resolved;
            front.StateCode = SimulationFarmTacticalCombatCodes.Resolved;
            tacticalResolutions.Add(new SimulationTacticalResolutionSnapshot
            {
                ResolutionStableId = "tactical-resolution:" + order.OrderStableId,
                OrderStableId = order.OrderStableId,
                EncounterStableId = encounter.EncounterStableId,
                FrontStableId = front.FrontStableId,
                OrderCode = order.OrderCode,
                ConsumedOpportunityStableId = order.OpportunityStableId,
                ResolvedWorldTick = currentTick,
                PreparednessScore = preparedness,
                TacticalResponseScore = baseScore + bonus,
                DefenseSucceeded = success,
                OutcomeCode = encounter.OutcomeCode,
                FrontPositionCode = front.PositionCode,
                CombatStrengthDelta = combatStrengthDelta,
                RecoverableInjuryCount = injuryCount,
                FacilityDamageUnits = encounter.DamageUnits,
                SupplyLossUnits = encounter.SupplyLossUnits,
                PresentationKey = "survival.tactical.resolution."
                    + encounter.OutcomeCode.ToLowerInvariant(),
            });
        }

        private void ApplyTacticalWithdrawal(
            SimulationThreatEncounterSnapshot encounter)
        {
            encounter.SupplyLossUnits = Math.Min(2m, farmSupplyUnits);
            farmSupplyUnits -= encounter.SupplyLossUnits;
            encounter.DamageUnits = 20m;
            recoverableDamageUnits += encounter.DamageUnits;
            var fence = farmDefenses.Values.FirstOrDefault(value =>
                value.DefenseKindCode == SimulationFarmSurvivalCodes.Fence);
            if (fence != null)
                fence.Durability = Math.Max(0m,
                    fence.Durability - encounter.DamageUnits);
            encounter.OutcomeCode = SimulationFarmSurvivalCodes.TacticalWithdrawal;
            encounter.InjuredActorStableId = string.Empty;
            encounter.StateCode = SimulationFarmSurvivalCodes.Resolved;
            encounter.PresentationKey =
                SimulationFarmSurvivalCodes.DamageAssessmentPresentation;
            UpdateFarmThreatWorldEvent(encounter);
        }

        private bool HasOpenTacticalOrderWindow()
            => tacticalOrderWindows.Any(value => value.StateCode ==
                    SimulationFarmTacticalCombatCodes.Open
                || value.StateCode ==
                    SimulationFarmTacticalCombatCodes.Confirmed);

        private SimulationTacticalOrderWindowSnapshot FindTacticalOrderWindow(
            string stableId)
            => tacticalOrderWindows.SingleOrDefault(value =>
                value.OrderWindowStableId == stableId)
                ?? throw new SimulationNotFoundException(
                    "SimulationTacticalOrderWindowNotFound");

        private SimulationTacticalOpportunitySnapshot FindTacticalOpportunity(
            string stableId)
            => tacticalOpportunities.SingleOrDefault(value =>
                value.OpportunityStableId == stableId)
                ?? throw new SimulationNotFoundException(
                    "SimulationTacticalOpportunityNotFound");

        private void EnsureHeroTacticalCombatConfigured()
        {
            EnsureFarmSurvivalConfigured();
            if (!UsesHeroTacticalCombatRule())
                throw new SimulationConflictException(
                    "SimulationHeroTacticalCombatNotEnabled");
        }

        private static string TacticalOpportunityKind(
            SimulationCombatReactionSnapshot reaction)
        {
            if (reaction.GradeCode != SimulationFarmCombatCodes.OnTime
                && reaction.GradeCode != SimulationFarmCombatCodes.Perfect)
                return string.Empty;
            return reaction.ReactionActionCode == SimulationFarmCombatCodes.Guard
                ? SimulationFarmTacticalCombatCodes.Rally
                : reaction.ReactionActionCode == SimulationFarmCombatCodes.Counter
                    ? SimulationFarmTacticalCombatCodes.Breakthrough
                    : string.Empty;
        }

        private static bool OpportunityMatchesOrder(
            SimulationTacticalOpportunitySnapshot opportunity,
            string orderCode)
            => opportunity.OpportunityKindCode ==
                    SimulationFarmTacticalCombatCodes.Rally
                ? orderCode == SimulationFarmTacticalCombatCodes.HoldFormation
                : opportunity.OpportunityKindCode ==
                    SimulationFarmTacticalCombatCodes.Breakthrough
                    && orderCode ==
                        SimulationFarmTacticalCombatCodes.AdvanceAndAttack;

        private static string TacticalOrderPresentationKey(string orderCode)
            => "survival.tactical.order." + orderCode.ToLowerInvariant();

        private SimulationFarmTacticalCombatStateSnapshot
            CreateFarmTacticalCombatStateSnapshot()
            => new SimulationFarmTacticalCombatStateSnapshot
            {
                Fronts = tacticalFronts.Select(CloneTacticalFront).ToArray(),
                Squads = tacticalSquads.Select(CloneTacticalSquad).ToArray(),
                Opportunities = tacticalOpportunities
                    .Select(CloneTacticalOpportunity).ToArray(),
                OrderWindows = tacticalOrderWindows
                    .Select(CloneTacticalOrderWindow).ToArray(),
                Orders = tacticalOrders.Select(CloneTacticalOrder).ToArray(),
                Resolutions = tacticalResolutions
                    .Select(CloneTacticalResolution).ToArray(),
                SimulationOnly = true,
                IsOperationalState = false,
            };

        internal static SimulationFarmTacticalCombatStateSnapshot
            CloneFarmTacticalCombatState(
                SimulationFarmTacticalCombatStateSnapshot source)
            => new SimulationFarmTacticalCombatStateSnapshot
            {
                RuleRevision = source.RuleRevision,
                Fronts = source.Fronts.Select(CloneTacticalFront).ToArray(),
                Squads = source.Squads.Select(CloneTacticalSquad).ToArray(),
                Opportunities = source.Opportunities
                    .Select(CloneTacticalOpportunity).ToArray(),
                OrderWindows = source.OrderWindows
                    .Select(CloneTacticalOrderWindow).ToArray(),
                Orders = source.Orders.Select(CloneTacticalOrder).ToArray(),
                Resolutions = source.Resolutions
                    .Select(CloneTacticalResolution).ToArray(),
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        private static SimulationTacticalFrontSnapshot CloneTacticalFront(
            SimulationTacticalFrontSnapshot value)
            => new SimulationTacticalFrontSnapshot
            {
                FrontStableId = value.FrontStableId,
                EncounterStableId = value.EncounterStableId,
                AreaStableId = value.AreaStableId,
                PositionCode = value.PositionCode,
                StateCode = value.StateCode,
                PresentationKey = value.PresentationKey,
            };

        private static SimulationTacticalSquadSnapshot CloneTacticalSquad(
            SimulationTacticalSquadSnapshot value)
            => new SimulationTacticalSquadSnapshot
            {
                SquadStableId = value.SquadStableId,
                FrontStableId = value.FrontStableId,
                SideCode = value.SideCode,
                PositionCode = value.PositionCode,
                MemberCount = value.MemberCount,
                CombatStrength = value.CombatStrength,
                RecoverableInjuryCount = value.RecoverableInjuryCount,
                MemberActorStableIds = value.MemberActorStableIds.ToArray(),
                PresentationKey = value.PresentationKey,
            };

        private static SimulationTacticalOpportunitySnapshot
            CloneTacticalOpportunity(SimulationTacticalOpportunitySnapshot value)
            => new SimulationTacticalOpportunitySnapshot
            {
                OpportunityStableId = value.OpportunityStableId,
                EncounterStableId = value.EncounterStableId,
                FrontStableId = value.FrontStableId,
                SourceReactionStableId = value.SourceReactionStableId,
                EarningActorStableId = value.EarningActorStableId,
                OpportunityKindCode = value.OpportunityKindCode,
                Quality = value.Quality,
                CreatedWorldTick = value.CreatedWorldTick,
                ExpiresWorldTick = value.ExpiresWorldTick,
                StateCode = value.StateCode,
                ReservedOrderStableId = value.ReservedOrderStableId,
                PresentationKey = value.PresentationKey,
            };

        private static SimulationTacticalOrderWindowSnapshot
            CloneTacticalOrderWindow(SimulationTacticalOrderWindowSnapshot value)
            => new SimulationTacticalOrderWindowSnapshot
            {
                OrderWindowStableId = value.OrderWindowStableId,
                EncounterStableId = value.EncounterStableId,
                FrontStableId = value.FrontStableId,
                AuthorizedActorStableId = value.AuthorizedActorStableId,
                OpenedWorldTick = value.OpenedWorldTick,
                ClosesWorldTick = value.ClosesWorldTick,
                StateCode = value.StateCode,
                ConfirmedOrderStableId = value.ConfirmedOrderStableId,
                AllowedOrderCodes = value.AllowedOrderCodes.ToArray(),
                PresentationKey = value.PresentationKey,
            };

        private static SimulationTacticalOrderSnapshot CloneTacticalOrder(
            SimulationTacticalOrderSnapshot value)
            => new SimulationTacticalOrderSnapshot
            {
                OrderStableId = value.OrderStableId,
                CommandId = value.CommandId,
                OrderWindowStableId = value.OrderWindowStableId,
                FrontStableId = value.FrontStableId,
                ActorStableId = value.ActorStableId,
                OrderCode = value.OrderCode,
                OpportunityStableId = value.OpportunityStableId,
                ConfirmedWorldTick = value.ConfirmedWorldTick,
                ResolvesWorldTick = value.ResolvesWorldTick,
                AutomaticallySelected = value.AutomaticallySelected,
                StateCode = value.StateCode,
                PresentationKey = value.PresentationKey,
            };

        private static SimulationTacticalResolutionSnapshot
            CloneTacticalResolution(SimulationTacticalResolutionSnapshot value)
            => new SimulationTacticalResolutionSnapshot
            {
                ResolutionStableId = value.ResolutionStableId,
                OrderStableId = value.OrderStableId,
                EncounterStableId = value.EncounterStableId,
                FrontStableId = value.FrontStableId,
                OrderCode = value.OrderCode,
                ConsumedOpportunityStableId =
                    value.ConsumedOpportunityStableId,
                ResolvedWorldTick = value.ResolvedWorldTick,
                PreparednessScore = value.PreparednessScore,
                TacticalResponseScore = value.TacticalResponseScore,
                DefenseSucceeded = value.DefenseSucceeded,
                OutcomeCode = value.OutcomeCode,
                FrontPositionCode = value.FrontPositionCode,
                CombatStrengthDelta = value.CombatStrengthDelta,
                RecoverableInjuryCount = value.RecoverableInjuryCount,
                FacilityDamageUnits = value.FacilityDamageUnits,
                SupplyLossUnits = value.SupplyLossUnits,
                PresentationKey = value.PresentationKey,
            };

        internal static void ValidateTacticalOrderPreviewRequest(
            SimulationTacticalOrderPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateTacticalOrderFields(request.ExpectedRevision,
                request.OrderWindowStableId, request.FrontStableId,
                request.ActorStableId, request.OrderCode,
                request.OpportunityStableId);
        }

        internal static void ValidateTacticalOrderConfirmRequest(
            SimulationTacticalOrderConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            ValidateTacticalOrderFields(request.ExpectedRevision,
                request.OrderWindowStableId, request.FrontStableId,
                request.ActorStableId, request.OrderCode,
                request.OpportunityStableId);
        }

        private static void ValidateTacticalOrderFields(
            long expectedRevision,
            string orderWindowStableId,
            string frontStableId,
            string actorStableId,
            string orderCode,
            string opportunityStableId)
        {
            if (expectedRevision < 0)
                throw new SimulationContractException(
                    "SimulationExpectedRevisionInvalid");
            RequireStableId(orderWindowStableId,
                "SimulationTacticalOrderWindowInvalid");
            RequireStableId(frontStableId, "SimulationTacticalFrontInvalid");
            RequireStableId(actorStableId, "SimulationFarmActorInvalid");
            if (orderCode != SimulationFarmTacticalCombatCodes.AdvanceAndAttack
                && orderCode !=
                    SimulationFarmTacticalCombatCodes.HoldFormation
                && orderCode !=
                    SimulationFarmTacticalCombatCodes.TacticalRetreat)
                throw new SimulationContractException(
                    "SimulationTacticalOrderInvalid");
            if (!string.IsNullOrEmpty(opportunityStableId))
                RequireStableId(opportunityStableId,
                    "SimulationTacticalOpportunityInvalid");
        }

        internal static string BuildTacticalOrderPayloadKey(
            SimulationTacticalOrderConfirmRequest request)
            => string.Join("|", request.OrderWindowStableId.Trim(),
                request.FrontStableId.Trim(), request.ActorStableId.Trim(),
                request.OrderCode.Trim(), request.OpportunityStableId.Trim(),
                request.ExpectedRevision.ToString(CultureInfo.InvariantCulture));

        private static void AddFarmTacticalCombatKey(
            System.Text.StringBuilder key,
            SimulationFarmTacticalCombatStateSnapshot value)
        {
            AddFarmKey(key, value.RuleRevision);
            foreach (var front in value.Fronts)
            {
                AddFarmKey(key, front.FrontStableId);
                AddFarmKey(key, front.EncounterStableId);
                AddFarmKey(key, front.AreaStableId);
                AddFarmKey(key, front.PositionCode);
                AddFarmKey(key, front.StateCode);
                AddFarmKey(key, front.PresentationKey);
            }
            foreach (var squad in value.Squads)
            {
                AddFarmKey(key, squad.SquadStableId);
                AddFarmKey(key, squad.FrontStableId);
                AddFarmKey(key, squad.SideCode);
                AddFarmKey(key, squad.PositionCode);
                AddFarmKey(key, squad.MemberCount);
                AddFarmKey(key, squad.CombatStrength);
                AddFarmKey(key, squad.RecoverableInjuryCount);
                foreach (var actorId in squad.MemberActorStableIds)
                    AddFarmKey(key, actorId);
                AddFarmKey(key, squad.PresentationKey);
            }
            foreach (var opportunity in value.Opportunities)
            {
                AddFarmKey(key, opportunity.OpportunityStableId);
                AddFarmKey(key, opportunity.EncounterStableId);
                AddFarmKey(key, opportunity.FrontStableId);
                AddFarmKey(key, opportunity.SourceReactionStableId);
                AddFarmKey(key, opportunity.EarningActorStableId);
                AddFarmKey(key, opportunity.OpportunityKindCode);
                AddFarmKey(key, opportunity.Quality);
                AddFarmKey(key, opportunity.CreatedWorldTick);
                AddFarmKey(key, opportunity.ExpiresWorldTick);
                AddFarmKey(key, opportunity.StateCode);
                AddFarmKey(key, opportunity.ReservedOrderStableId);
                AddFarmKey(key, opportunity.PresentationKey);
            }
            foreach (var window in value.OrderWindows)
            {
                AddFarmKey(key, window.OrderWindowStableId);
                AddFarmKey(key, window.EncounterStableId);
                AddFarmKey(key, window.FrontStableId);
                AddFarmKey(key, window.AuthorizedActorStableId);
                AddFarmKey(key, window.OpenedWorldTick);
                AddFarmKey(key, window.ClosesWorldTick);
                AddFarmKey(key, window.StateCode);
                AddFarmKey(key, window.ConfirmedOrderStableId);
                foreach (var orderCode in window.AllowedOrderCodes)
                    AddFarmKey(key, orderCode);
                AddFarmKey(key, window.PresentationKey);
            }
            foreach (var order in value.Orders)
            {
                AddFarmKey(key, order.OrderStableId);
                AddFarmKey(key, order.CommandId);
                AddFarmKey(key, order.OrderWindowStableId);
                AddFarmKey(key, order.FrontStableId);
                AddFarmKey(key, order.ActorStableId);
                AddFarmKey(key, order.OrderCode);
                AddFarmKey(key, order.OpportunityStableId);
                AddFarmKey(key, order.ConfirmedWorldTick);
                AddFarmKey(key, order.ResolvesWorldTick);
                AddFarmKey(key, order.AutomaticallySelected);
                AddFarmKey(key, order.StateCode);
                AddFarmKey(key, order.PresentationKey);
            }
            foreach (var resolution in value.Resolutions)
            {
                AddFarmKey(key, resolution.ResolutionStableId);
                AddFarmKey(key, resolution.OrderStableId);
                AddFarmKey(key, resolution.EncounterStableId);
                AddFarmKey(key, resolution.FrontStableId);
                AddFarmKey(key, resolution.OrderCode);
                AddFarmKey(key, resolution.ConsumedOpportunityStableId);
                AddFarmKey(key, resolution.ResolvedWorldTick);
                AddFarmKey(key, resolution.PreparednessScore);
                AddFarmKey(key, resolution.TacticalResponseScore);
                AddFarmKey(key, resolution.DefenseSucceeded);
                AddFarmKey(key, resolution.OutcomeCode);
                AddFarmKey(key, resolution.FrontPositionCode);
                AddFarmKey(key, resolution.CombatStrengthDelta);
                AddFarmKey(key, resolution.RecoverableInjuryCount);
                AddFarmKey(key, resolution.FacilityDamageUnits);
                AddFarmKey(key, resolution.SupplyLossUnits);
                AddFarmKey(key, resolution.PresentationKey);
            }
        }
    }
}
