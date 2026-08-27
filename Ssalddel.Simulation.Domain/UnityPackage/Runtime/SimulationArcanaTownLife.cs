using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string ArcanaActivationRuleRevision =
            "major-arcana-activation.r1";
        private const string ArcanaOrientationRuleRevision =
            "major-arcana-orientation-recovery-share-51.r1";
        private const decimal TownLowerCardMultiplier = 1.05m;

        private string tarotOrientationPolicyCode =
            Simulation타로방향결정정책Codes.SeededHash;
        private string townNpcLifeProfileStableId = string.Empty;
        private string townContextPlayerStableId = string.Empty;
        private readonly Dictionary<string, TownNpcLifeState> townNpcLifeStates =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationTown물품Snapshot> townLifeItems =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationTown목표Snapshot> townLifeGoals =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, SimulationTown주문Snapshot> townLifeOrders =
            new(StringComparer.Ordinal);

        private bool UsesContextualArcana => string.Equals(
            tarotOrientationPolicyCode,
            Simulation타로방향결정정책Codes.RecoveryShare51,
            StringComparison.Ordinal);

        private bool HasTownNpcLife => !string.IsNullOrWhiteSpace(
            townNpcLifeProfileStableId);

        private void InitializeArcanaTownLife(
            string? orientationPolicyCode,
            string? townProfileStableId)
        {
            tarotOrientationPolicyCode = string.IsNullOrWhiteSpace(orientationPolicyCode)
                ? Simulation타로방향결정정책Codes.SeededHash
                : orientationPolicyCode.Trim();
            townNpcLifeProfileStableId = (townProfileStableId ?? string.Empty).Trim();

            if (tarotOrientationPolicyCode != Simulation타로방향결정정책Codes.SeededHash
                && tarotOrientationPolicyCode
                    != Simulation타로방향결정정책Codes.RecoveryShare51)
            {
                throw new SimulationContractException(
                    "SimulationTarotOrientationPolicyInvalid");
            }
            if (townNpcLifeProfileStableId.Length > 0
                && townNpcLifeProfileStableId
                    != SimulationTown생활복구Codes.ApprovedFixtureProfile)
            {
                throw new SimulationContractException(
                    "SimulationTownNpcLifeProfileInvalid");
            }
            if (!UsesContextualArcana && HasTownNpcLife)
            {
                throw new SimulationContractException(
                    "SimulationTownNpcLifeContextualArcanaRequired");
            }
            if (!UsesContextualArcana) return;
            if (natureMindPlayers.Count == 0)
            {
                throw new SimulationContractException(
                    "SimulationTarotOrientationEvidenceUnavailable");
            }

            townContextPlayerStableId = natureMindPlayers.Keys
                .OrderBy(value => value, StringComparer.Ordinal).First();
            tarotContextState.ContextStateHashSha256 =
                BuildTarotContextStateV3Hash(tarotContextState);
            if (!HasTownNpcLife) return;

            AddTownNpc(SimulationTown생활복구Codes.ResidentAStableId, "주민 A",
                (SimulationTown욕구Codes.Utility, 90m),
                (SimulationTown욕구Codes.Sustenance, 70m));
            AddTownNpc(SimulationTown생활복구Codes.ResidentBStableId, "주민 B",
                (SimulationTown욕구Codes.Utility, 80m),
                (SimulationTown욕구Codes.Shelter, 75m),
                (SimulationTown욕구Codes.Sustenance, 65m));
            AddTownItem(SimulationTown생활복구Codes.PortableBatteryItemStableId,
                "휴대용 배터리", SimulationTown욕구Codes.Utility);
            AddTownItem(SimulationTown생활복구Codes.WeatherproofTarpItemStableId,
                "방수포", SimulationTown욕구Codes.Shelter);
            AddTownItem(SimulationTown생활복구Codes.EmergencyFoodItemStableId,
                "비상식량 묶음", SimulationTown욕구Codes.Sustenance);
        }

        private void AddTownNpc(string stableId, string displayName,
            params (string NeedCode, decimal Severity)[] needs)
        {
            townNpcLifeStates.Add(stableId, new TownNpcLifeState
            {
                NpcStableId = stableId,
                DisplayName = displayName,
                Needs = needs.ToDictionary(value => value.NeedCode,
                    value => new SimulationTown욕구Snapshot
                    {
                        NeedCode = value.NeedCode,
                        Severity = value.Severity,
                        Revision = 1,
                    }, StringComparer.Ordinal),
                Revision = 1,
            });
        }

        private void AddTownItem(string stableId, string koreanName, string roleCode)
            => townLifeItems.Add(stableId, new SimulationTown물품Snapshot
            {
                ItemStableId = stableId,
                KoreanName = koreanName,
                ItemRoleCode = roleCode,
                AvailableQuantity = 1,
                BaseLifeRecovery = 30m,
                UnitCode = "item",
                Revision = 1,
            });

        private Simulation메이저아르카나방향판정Snapshot
            ResolveContextualArcanaDirectionDecision()
        {
            if (!natureMindPlayers.TryGetValue(townContextPlayerStableId, out var player))
                throw new SimulationConflictException(
                    "SimulationTarotOrientationEvidenceUnavailable");
            var balance = CreateNatureMindBalanceSnapshot(player);
            var total = balance.RecoveryOutput + balance.ThreatOutput;
            if (total <= 0m || string.IsNullOrWhiteSpace(balance.BalanceHashSha256))
                throw new SimulationConflictException(
                    "SimulationTarotOrientationEvidenceUnavailable");
            var recoveryShareMicro = decimal.ToInt64(decimal.Round(
                balance.RecoveryShare * 1_000_000m, 0,
                MidpointRounding.AwayFromZero));
            return new Simulation메이저아르카나방향판정Snapshot
            {
                DirectionCode = recoveryShareMicro >= 510_000L
                    ? Simulation타로카드방향Codes.Upright
                    : Simulation타로카드방향Codes.Reversed,
                RecoveryShareMicro = recoveryShareMicro,
                RecoveryOutput = balance.RecoveryOutput,
                ThreatOutput = balance.ThreatOutput,
                ContextPlayerStableId = townContextPlayerStableId,
                EvidenceRevision = balance.Revision,
                EvidenceHashSha256 = balance.BalanceHashSha256,
                RuleRevision = ArcanaOrientationRuleRevision,
                DecidedAtWorldRevision = Revision,
                DecidedAtWorldTick = CurrentTick,
            };
        }

        private void ApplyContextualTarotContext(SimulationTurnClosingSnapshot closing)
        {
            var activations = tarotContextState.MajorArcanaActivations
                .Select(CloneMajorArcanaActivation).ToList();
            var active = activations.SingleOrDefault(value =>
                value.StateCode == Simulation메이저아르카나활성상태Codes.Active);
            var selected = closing.SelectedCards.SingleOrDefault(value =>
                value.CardKindCode == SimulationTurnCardKindCodes.Tarot);

            if (closing.DeactivatedActiveMajorArcana)
            {
                if (active == null)
                    throw new SimulationConflictException(
                        "SimulationMajorArcanaActivationMissing");
                EndActivation(active, Simulation메이저아르카나종료이유Codes.Deactivated,
                    string.Empty, closing.ResultingRevision, closing.ResultingWorldTick);
                tarotContextState = CreateContextualTarotState(
                    activations, null, Array.Empty<Simulation상위아르카나방향상속Snapshot>(),
                    tarotContextState.FrameSet.Revision + 1, closing);
                return;
            }

            if (selected == null) return;
            if (active != null && active.Selection.CardStableId == selected.CardStableId)
                throw new SimulationConflictException("SimulationMajorArcanaAlreadyActive");
            var decision = closing.MajorArcanaDirectionDecision
                ?? throw new SimulationConflictException(
                    "SimulationTarotOrientationEvidenceUnavailable");
            var sequence = activations.Count == 0
                ? 1 : activations.Max(value => value.ActivationSequence) + 1;
            var activationStableId = "major-arcana-activation:"
                + Hash(string.Join("|", SessionStableId,
                    sequence.ToString(CultureInfo.InvariantCulture),
                    selected.CardStableId,
                    closing.ResultingRevision.ToString(CultureInfo.InvariantCulture)));
            if (active != null)
            {
                EndActivation(active, Simulation메이저아르카나종료이유Codes.Replaced,
                    activationStableId, closing.ResultingRevision, closing.ResultingWorldTick);
            }
            var activation = new Simulation메이저아르카나활성Snapshot
            {
                MajorArcanaActivationStableId = activationStableId,
                ActivationSequence = sequence,
                StateCode = Simulation메이저아르카나활성상태Codes.Active,
                Selection = new Simulation메이저아르카나선택Snapshot
                {
                    CardStableId = selected.CardStableId,
                    CardCopyStableId = selected.CardCopyStableId,
                    CardRevision = selected.CardRevision,
                    OfferStableId = selected.OfferStableId,
                    SelectionSourceCode =
                        SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                    SelectedAtWorldRevision = decision.DecidedAtWorldRevision,
                    SelectedAtWorldTick = decision.DecidedAtWorldTick,
                },
                OrientationDecision = CloneDirectionDecision(decision),
                ActivatedAtWorldRevision = closing.ResultingRevision,
                ActivatedAtWorldTick = closing.ResultingWorldTick,
            };
            activations.Add(activation);
            var inheritances = BuildOrientationInheritances(activation);
            tarotContextState = CreateContextualTarotState(activations, selected,
                inheritances, tarotContextState.FrameSet.Revision + 1, closing);
        }

        private SimulationTarotContextStateSnapshot CreateContextualTarotState(
            IReadOnlyCollection<Simulation메이저아르카나활성Snapshot> activations,
            SimulationTurnCardSnapshot? selected,
            Simulation상위아르카나방향상속Snapshot[] inheritances,
            long frameSetRevision,
            SimulationTurnClosingSnapshot closing)
        {
            var frames = Array.Empty<SimulationTarotFrameSnapshot>();
            var proposals = Array.Empty<SimulationTarotContextProposalSnapshot>();
            var relations = Array.Empty<SimulationCardContextRelationSnapshot>();
            if (selected != null)
            {
                var frameId = activations.Single(value =>
                    value.StateCode == Simulation메이저아르카나활성상태Codes.Active)
                    .MajorArcanaActivationStableId + ":frame";
                var proposalCode = ProposalCode(selected);
                var proposalId = frameId + ":proposal:" + proposalCode.ToLowerInvariant();
                var frame = new SimulationTarotFrameSnapshot
                {
                    FrameStableId = frameId,
                    CardStableId = selected.CardStableId,
                    CardCopyStableId = selected.CardCopyStableId,
                    CardRevision = selected.CardRevision,
                    OrientationCode = selected.OrientationCode,
                    ParentJourneyFrameStableId =
                        SimulationTarotJourneyRootCodes.FoolFrameStableId,
                    MetaLayerCode = SimulationTarotMetaLayerCodes.ActiveMajorArcana,
                    FrameScopeCode = SimulationTarotFrameScopeCodes.UntilReplaced,
                    ScopeTargetStableId = SessionStableId,
                    StartsAtTurnNumber = CurrentTick,
                    EndsAtTurnNumber = int.MaxValue,
                    ThemeCodes = ThemeCodes(selected),
                    ContextProposalStableIds = new[] { proposalId },
                    SourceDrawStableId = closing.TurnClosingStableId + ":tarot-draw",
                    SourceOfferStableId = selected.OfferStableId,
                    RuleRevision = ArcanaActivationRuleRevision,
                    SourceStableId = selected.SourceStableId,
                };
                var proposal = new SimulationTarotContextProposalSnapshot
                {
                    ProposalStableId = proposalId,
                    ContextProposalCode = proposalCode,
                    SourceFrameStableId = frameId,
                    FrameScopeCode = frame.FrameScopeCode,
                    ScopeTargetStableId = frame.ScopeTargetStableId,
                    SourceThemeCode = frame.ThemeCodes.First(),
                    SourceWorldRevision = closing.ResultingRevision,
                    SourceTurnNumber = CurrentTick,
                    RuleRevision = "tarot-context-proposal.r2",
                };
                frames = new[] { frame };
                proposals = new[] { proposal };
                relations = BuildTarotRelations(frame, proposal);
            }
            var frameSet = new SimulationTarotFrameSetSnapshot
            {
                Revision = frameSetRevision,
                SourceWorldRevision = closing.ResultingRevision,
                SourceTurnNumber = CurrentTick,
                JourneyRoot = BuildFoolJourneyRoot(),
                ActiveFrames = frames,
            };
            frameSet.FrameSetHashSha256 = Hash(CanonicalFrameSet(frameSet));
            var result = new SimulationTarotContextStateSnapshot
            {
                FrameSet = frameSet,
                Proposals = proposals,
                Relations = relations,
                MajorArcanaActivations = activations
                    .OrderBy(value => value.ActivationSequence)
                    .Select(CloneMajorArcanaActivation).ToArray(),
                OrientationInheritances = inheritances
                    .OrderBy(value => value.InheritanceStableId, StringComparer.Ordinal)
                    .Select(CloneOrientationInheritance).ToArray(),
            };
            result.ContextStateHashSha256 = BuildTarotContextStateV3Hash(result);
            return result;
        }

        private Simulation상위아르카나방향상속Snapshot[] BuildOrientationInheritances(
            Simulation메이저아르카나활성Snapshot activation)
        {
            var targets = new List<(string Family, string Card, string Copy, string Mode,
                string[] Bindings)>();
            if (HasTownNpcLife)
            {
                targets.Add((SimulationCardFamilyCodes.TeamRole,
                    SimulationTown생활복구Codes.ResidentLifeCardStableId, string.Empty,
                    Simulation상위아르카나영향방식Codes.Numeric,
                    new[] { SimulationTown생활복구Codes.EffectBindingCode }));
                targets.Add((SimulationCardFamilyCodes.TeamRole,
                    SimulationTown생활복구Codes.ClerkCardStableId, string.Empty,
                    Simulation상위아르카나영향방식Codes.DirectionOnly,
                    Array.Empty<string>()));
                targets.Add((SimulationCardFamilyCodes.ResearchSeedbed,
                    "research-card:town:life-recovery-fixture", string.Empty,
                    Simulation상위아르카나영향방식Codes.Ordering,
                    Array.Empty<string>()));
            }
            foreach (var card in CreateAvailableTurnCards().Where(value =>
                         value.CardKindCode != SimulationTurnCardKindCodes.Tarot))
            {
                targets.Add((card.CardKindCode == SimulationTurnCardKindCodes.Culture
                        ? SimulationCardFamilyCodes.Culture
                        : SimulationCardFamilyCodes.TurnClosing,
                    card.CardStableId, card.CardCopyStableId,
                    Simulation상위아르카나영향방식Codes.Interpretive,
                    Array.Empty<string>()));
            }
            foreach (var definition in CollectibleCatalog)
            {
                targets.Add((SimulationCardFamilyCodes.CollectibleReward,
                    definition.CardDefinitionStableId, string.Empty,
                    Simulation상위아르카나영향방식Codes.Ordering,
                    Array.Empty<string>()));
            }
            if (realityContext != null)
            {
                foreach (var evidence in realityContext.SourceEvidence)
                {
                    targets.Add((SimulationCardFamilyCodes.ConceptInformation,
                        "concept-card:" + evidence.SourceEvidenceStableId, string.Empty,
                        Simulation상위아르카나영향방식Codes.Interpretive,
                        Array.Empty<string>()));
                }
            }

            var direction = activation.OrientationDecision.DirectionCode;
            var interpretation = direction == Simulation타로카드방향Codes.Upright
                ? Simulation상위아르카나해석Codes.OpportunityEmphasis
                : Simulation상위아르카나해석Codes.RiskEmphasis;
            var multiplier = direction == Simulation타로카드방향Codes.Upright
                ? 1.15m : .85m;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            return targets.OrderBy(value => value.Family, StringComparer.Ordinal)
                .ThenBy(value => value.Card, StringComparer.Ordinal)
                .ThenBy(value => value.Copy, StringComparer.Ordinal)
                .Select(value =>
                {
                    var inheritanceStableId = "arcana-inheritance:" + Hash(string.Join("|",
                        activation.MajorArcanaActivationStableId, value.Card,
                        value.Copy, SimulationTown생활복구Codes.InfluencePolicyRevision));
                    if (!ids.Add(inheritanceStableId))
                        throw new SimulationConflictException("DuplicateInheritance");
                    return new Simulation상위아르카나방향상속Snapshot
                    {
                        InheritanceStableId = inheritanceStableId,
                        MajorArcanaActivationStableId =
                            activation.MajorArcanaActivationStableId,
                        SourceCardStableId = activation.Selection.CardStableId,
                        DirectionCode = direction,
                        TargetCardFamilyCode = value.Family,
                        TargetCardStableId = value.Card,
                        TargetCardCopyStableId = value.Copy,
                        InfluenceModeCode = value.Mode,
                        NumericMultiplier = value.Mode
                            == Simulation상위아르카나영향방식Codes.Numeric
                            ? multiplier : null,
                        InterpretationCode = interpretation,
                        AllowedEffectBindingCodes = value.Bindings,
                        InfluencePolicyRevision =
                            SimulationTown생활복구Codes.InfluencePolicyRevision,
                    };
                }).ToArray();
        }

        private static void EndActivation(Simulation메이저아르카나활성Snapshot activation,
            string reasonCode, string supersededBy, long worldRevision, int worldTick)
        {
            activation.StateCode = Simulation메이저아르카나활성상태Codes.Ended;
            activation.EndedAtWorldRevision = worldRevision;
            activation.EndedAtWorldTick = worldTick;
            activation.EndReasonCode = reasonCode;
            activation.SupersededByActivationStableId = supersededBy;
        }

        private void AdvanceTownNpcLife(int previousTick, int currentTick)
        {
            if (!HasTownNpcLife) return;
            for (var tick = previousTick + 1; tick <= currentTick; tick++)
            {
                AdvanceTownOrders(tick);
                SelectTownGoalsAndReserve(tick);
            }
        }

        private void AdvanceTownOrders(int tick)
        {
            foreach (var order in townLifeOrders.Values
                .Where(value => value.StageCode != SimulationTown주문단계Codes.Consumed
                    && value.StageChangedAtWorldTick < tick)
                .OrderBy(value => value.RequestedAtWorldRevision)
                .ThenBy(value => value.NpcStableId, StringComparer.Ordinal)
                .ToArray())
            {
                switch (order.StageCode)
                {
                    case SimulationTown주문단계Codes.Reserved:
                        ChangeTownOrderStage(order, SimulationTown주문단계Codes.Picked,
                            "WI-ORDER-03", tick);
                        break;
                    case SimulationTown주문단계Codes.Picked:
                        ChangeTownOrderStage(order, SimulationTown주문단계Codes.Packed,
                            "WI-ORDER-04", tick);
                        break;
                    case SimulationTown주문단계Codes.Packed:
                        ChangeTownOrderStage(order,
                            SimulationTown주문단계Codes.ReadyForPickup,
                            "WI-ORDER-05", tick);
                        break;
                    case SimulationTown주문단계Codes.ReadyForPickup:
                        ChangeTownOrderStage(order, SimulationTown주문단계Codes.Received,
                            "WI-ORDER-06", tick);
                        break;
                    case SimulationTown주문단계Codes.Received:
                        ConsumeTownOrder(order, tick);
                        break;
                }
            }
        }

        private static void ChangeTownOrderStage(SimulationTown주문Snapshot order,
            string stageCode, string wiId, int tick)
        {
            order.StageCode = stageCode;
            order.WorldInteractionId = wiId;
            order.WorldInteractionHistoryIds = order.WorldInteractionHistoryIds
                .Concat(new[] { wiId }).ToArray();
            order.StageChangedAtWorldTick = tick;
            order.Revision++;
        }

        private void ConsumeTownOrder(SimulationTown주문Snapshot order, int tick)
        {
            var npc = townNpcLifeStates[order.NpcStableId];
            var goal = townLifeGoals[order.GoalStableId];
            var item = townLifeItems[order.ItemStableId];
            var need = npc.Needs[goal.NeedCode];
            var breakdown = CalculateTownLifeRecovery(order, item, tick);
            need.Severity = Math.Max(0m, decimal.Round(
                need.Severity - breakdown.FinalValue, 2,
                MidpointRounding.AwayFromZero));
            need.Revision++;
            item.ReservedByNpcStableId = string.Empty;
            item.Revision++;
            goal.StateCode = SimulationTown목표상태Codes.Completed;
            goal.CompletedAtWorldTick = tick;
            goal.Revision++;
            order.StageCode = SimulationTown주문단계Codes.Consumed;
            order.WorldInteractionId = "WI-ORDER-07";
            order.WorldInteractionHistoryIds = order.WorldInteractionHistoryIds
                .Concat(new[] { "WI-ORDER-07" }).ToArray();
            order.StageChangedAtWorldTick = tick;
            order.ConsumptionBreakdown = breakdown;
            order.Revision++;
            npc.CurrentGoalStableId = string.Empty;
            npc.CurrentGoalStateCode = SimulationTown목표상태Codes.Completed;
            npc.CurrentOrderStableId = string.Empty;
            npc.LastConsumedItemStableId = item.ItemStableId;
            npc.NextGoalReasonCode = "NeedsChangedAfterConsumption";
            npc.Revision++;
        }

        private SimulationEffect배율계보Snapshot CalculateTownLifeRecovery(
            SimulationTown주문Snapshot order, SimulationTown물품Snapshot item, int tick)
        {
            var active = tarotContextState.MajorArcanaActivations.SingleOrDefault(value =>
                value.StateCode == Simulation메이저아르카나활성상태Codes.Active);
            var inheritance = active == null ? null
                : tarotContextState.OrientationInheritances.SingleOrDefault(value =>
                    value.TargetCardStableId
                        == SimulationTown생활복구Codes.ResidentLifeCardStableId
                    && value.AllowedEffectBindingCodes.Contains(
                        SimulationTown생활복구Codes.EffectBindingCode,
                        StringComparer.Ordinal));
            var arcanaMultiplier = inheritance?.NumericMultiplier ?? 1m;
            var period = ResolveNaturePeriodForActor(townContextPlayerStableId);
            var periodMultiplier = period.PeriodStateCode switch
            {
                SimulationNaturePeriodCodes.GwangbokPeriod => 1.10m,
                SimulationNaturePeriodCodes.DarkAgePeriod => .90m,
                _ => 1m,
            };
            var raw = TownLowerCardMultiplier * arcanaMultiplier * periodMultiplier;
            var clamped = Math.Max(.75m, Math.Min(1.30m, raw));
            return new SimulationEffect배율계보Snapshot
            {
                BreakdownStableId = order.OrderStableId + ":life-recovery",
                EffectBindingCode = SimulationTown생활복구Codes.EffectBindingCode,
                BaseValue = item.BaseLifeRecovery,
                LowerCardMultiplier = TownLowerCardMultiplier,
                ArcanaOrientationMultiplier = arcanaMultiplier,
                PsychologicalPeriodMultiplier = periodMultiplier,
                RawMultiplier = raw,
                ClampedMultiplier = clamped,
                FinalValue = decimal.Round(item.BaseLifeRecovery * clamped, 2,
                    MidpointRounding.AwayFromZero),
                ValueUnitCode = "need-point",
                MajorArcanaActivationStableId =
                    active?.MajorArcanaActivationStableId ?? string.Empty,
                InheritanceStableId = inheritance?.InheritanceStableId ?? string.Empty,
                LowerCardStableId =
                    SimulationTown생활복구Codes.ResidentLifeCardStableId,
                PeriodStateCode = period.PeriodStateCode,
                PeriodInstanceStableId = period.PeriodInstanceStableId,
                PeriodRevision = period.Revision,
                PeriodStateHashSha256 = period.PeriodStateHashSha256,
                RuleRevision = SimulationTown생활복구Codes.RuleRevision,
            };
        }

        private void SelectTownGoalsAndReserve(int tick)
        {
            var candidates = new List<TownGoalCandidate>();
            foreach (var npc in townNpcLifeStates.Values
                .Where(value => string.IsNullOrWhiteSpace(value.CurrentOrderStableId))
                .OrderBy(value => value.NpcStableId, StringComparer.Ordinal))
            {
                var choice = npc.Needs.Values
                    .Where(value => value.Severity > 0m)
                    .Select(value => new
                    {
                        Need = value,
                        Item = townLifeItems.Values.FirstOrDefault(item =>
                            item.ItemRoleCode == value.NeedCode
                            && item.AvailableQuantity > 0
                            && string.IsNullOrWhiteSpace(item.ReservedByNpcStableId)),
                    })
                    .Where(value => value.Item != null)
                    .OrderByDescending(value => value.Need.Severity)
                    .ThenBy(value => value.Need.NeedCode, StringComparer.Ordinal)
                    .FirstOrDefault();
                if (choice != null)
                {
                    candidates.Add(new TownGoalCandidate(npc, choice.Need,
                        choice.Item!, Revision, tick));
                }
                else if (npc.Needs.Values.Any(value => value.Severity > 0m))
                {
                    npc.NextGoalReasonCode = "NoCompatibleAvailableItem";
                    npc.CurrentGoalStateCode =
                        SimulationTown목표상태Codes.NoEligibleGoal;
                    npc.Revision++;
                }
            }

            foreach (var group in candidates.GroupBy(value => value.Item.ItemStableId,
                         StringComparer.Ordinal))
            {
                var winner = group.OrderByDescending(value => value.Need.Severity)
                    .ThenBy(value => value.RequestWorldRevision)
                    .ThenBy(value => value.Npc.NpcStableId, StringComparer.Ordinal)
                    .First();
                ReserveTownGoal(winner, tick);
                foreach (var loser in group.Where(value => value != winner))
                {
                    loser.Npc.NextGoalReasonCode = "ItemContentionLost";
                    loser.Npc.Revision++;
                }
            }

            if (candidates.GroupBy(value => value.Item.ItemStableId,
                    StringComparer.Ordinal).Any(group => group.Count() > 1))
            {
                SelectTownGoalsAndReserveWithoutRecursion(tick);
            }
        }

        private void SelectTownGoalsAndReserveWithoutRecursion(int tick)
        {
            foreach (var npc in townNpcLifeStates.Values
                .Where(value => string.IsNullOrWhiteSpace(value.CurrentOrderStableId))
                .OrderBy(value => value.NpcStableId, StringComparer.Ordinal))
            {
                var choice = npc.Needs.Values.Where(value => value.Severity > 0m)
                    .Select(value => new
                    {
                        Need = value,
                        Item = townLifeItems.Values.FirstOrDefault(item =>
                            item.ItemRoleCode == value.NeedCode
                            && item.AvailableQuantity > 0
                            && string.IsNullOrWhiteSpace(item.ReservedByNpcStableId)),
                    })
                    .Where(value => value.Item != null)
                    .OrderByDescending(value => value.Need.Severity)
                    .ThenBy(value => value.Need.NeedCode, StringComparer.Ordinal)
                    .FirstOrDefault();
                if (choice != null)
                    ReserveTownGoal(new TownGoalCandidate(npc, choice.Need,
                        choice.Item!, Revision, tick), tick);
            }
        }

        private void ReserveTownGoal(TownGoalCandidate candidate, int tick)
        {
            var sequence = townLifeGoals.Count + 1;
            var goalId = "town-goal:" + candidate.Npc.NpcStableId + ":"
                + sequence.ToString(CultureInfo.InvariantCulture);
            var orderId = "town-order:" + candidate.Npc.NpcStableId + ":"
                + sequence.ToString(CultureInfo.InvariantCulture);
            var goal = new SimulationTown목표Snapshot
            {
                GoalStableId = goalId,
                NpcStableId = candidate.Npc.NpcStableId,
                NeedCode = candidate.Need.NeedCode,
                SourceSeverity = candidate.Need.Severity,
                TargetItemStableId = candidate.Item.ItemStableId,
                StateCode = SimulationTown목표상태Codes.InProgress,
                ReasonCode = candidate.Npc.NextGoalReasonCode.Length > 0
                    ? candidate.Npc.NextGoalReasonCode : "HighestEligibleNeed",
                TriggerSourceCode =
                    SimulationWorldInteractionTriggerSourceCodes.WorldDerived,
                SelectedAtWorldTick = tick,
                Revision = 1,
            };
            var order = new SimulationTown주문Snapshot
            {
                OrderStableId = orderId,
                NpcStableId = candidate.Npc.NpcStableId,
                GoalStableId = goalId,
                ItemStableId = candidate.Item.ItemStableId,
                StageCode = SimulationTown주문단계Codes.Reserved,
                WorldInteractionId = "WI-ORDER-02",
                TriggerSourceCode =
                    SimulationWorldInteractionTriggerSourceCodes.NpcDriven,
                WorldInteractionHistoryIds = new[] { "WI-ORDER-01", "WI-ORDER-02" },
                AssignedClerkNpcStableId = SimulationTown생활복구Codes.ClerkNpcStableId,
                RequestedAtWorldRevision = candidate.RequestWorldRevision,
                RequestedAtWorldTick = tick,
                StageChangedAtWorldTick = tick,
                Revision = 1,
            };
            candidate.Item.AvailableQuantity--;
            candidate.Item.ReservedByNpcStableId = candidate.Npc.NpcStableId;
            candidate.Item.Revision++;
            candidate.Npc.CurrentGoalStableId = goalId;
            candidate.Npc.CurrentGoalStateCode = goal.StateCode;
            candidate.Npc.CurrentOrderStableId = orderId;
            candidate.Npc.NextGoalReasonCode = goal.ReasonCode;
            candidate.Npc.Revision++;
            townLifeGoals.Add(goalId, goal);
            townLifeOrders.Add(orderId, order);
        }

        public SimulationTownNpcLifeStateSnapshot GetTownNpcLifeState()
        {
            lock (gate) return CreateTownNpcLifeStateSnapshot();
        }

        private SimulationTownNpcLifeStateSnapshot CreateTownNpcLifeStateSnapshot()
        {
            var state = new SimulationTownNpcLifeStateSnapshot
            {
                ProfileStableId = townNpcLifeProfileStableId,
                RuleRevision = HasTownNpcLife
                    ? SimulationTown생활복구Codes.RuleRevision : string.Empty,
                ContentionRuleRevision = HasTownNpcLife
                    ? SimulationTown생활복구Codes.ContentionRuleRevision : string.Empty,
                ContextPlayerStableId = townContextPlayerStableId,
                WorldTick = CurrentTick,
                WorldRevision = Revision,
                Items = townLifeItems.Values.OrderBy(value => value.ItemStableId,
                    StringComparer.Ordinal).Select(CloneTownItem).ToArray(),
                Npcs = townNpcLifeStates.Values.OrderBy(value => value.NpcStableId,
                    StringComparer.Ordinal).Select(CreateTownNpcSnapshot).ToArray(),
                Goals = townLifeGoals.Values.OrderBy(value => value.GoalStableId,
                    StringComparer.Ordinal).Select(CloneTownGoal).ToArray(),
                Orders = townLifeOrders.Values.OrderBy(value => value.OrderStableId,
                    StringComparer.Ordinal).Select(CloneTownOrder).ToArray(),
                SimulationOnly = true,
                IsOperationalState = false,
            };
            state.StateHashSha256 = Hash(BuildTownNpcLifePayloadKey(state));
            return state;
        }

        private SimulationTownNpcLifeSnapshot CreateTownNpcSnapshot(TownNpcLifeState value)
            => new()
            {
                NpcStableId = value.NpcStableId,
                DisplayName = value.DisplayName,
                Needs = value.Needs.Values.OrderBy(item => item.NeedCode,
                    StringComparer.Ordinal).Select(CloneTownNeed).ToArray(),
                CurrentGoalStableId = value.CurrentGoalStableId,
                CurrentGoalStateCode = value.CurrentGoalStateCode,
                CurrentOrderStableId = value.CurrentOrderStableId,
                CurrentOrderStageCode = value.CurrentOrderStableId.Length > 0
                    && townLifeOrders.TryGetValue(value.CurrentOrderStableId, out var order)
                    ? order.StageCode : string.Empty,
                ReservedItemStableId = townLifeItems.Values.FirstOrDefault(item =>
                    item.ReservedByNpcStableId == value.NpcStableId)?.ItemStableId
                    ?? string.Empty,
                LastConsumedItemStableId = value.LastConsumedItemStableId,
                NextGoalReasonCode = value.NextGoalReasonCode,
                Revision = value.Revision,
            };

        internal static string BuildTownNpcLifePayloadKey(
            SimulationTownNpcLifeStateSnapshot value)
        {
            var target = new StringBuilder();
            Add(target, value.ProfileStableId); Add(target, value.RuleRevision);
            Add(target, value.ContentionRuleRevision); Add(target, value.ContextPlayerStableId);
            Add(target, value.WorldTick); Add(target, value.WorldRevision);
            foreach (var item in value.Items.OrderBy(item => item.ItemStableId,
                         StringComparer.Ordinal))
            {
                Add(target, item.ItemStableId); Add(target, item.KoreanName);
                Add(target, item.ItemRoleCode); Add(target, item.AvailableQuantity);
                Add(target, item.BaseLifeRecovery); Add(target, item.UnitCode);
                Add(target, item.ReservedByNpcStableId); Add(target, item.Revision);
            }
            foreach (var npc in value.Npcs.OrderBy(item => item.NpcStableId,
                         StringComparer.Ordinal))
            {
                Add(target, npc.NpcStableId); Add(target, npc.DisplayName);
                foreach (var need in npc.Needs.OrderBy(item => item.NeedCode,
                             StringComparer.Ordinal))
                {
                    Add(target, need.NeedCode); Add(target, need.Severity);
                    Add(target, need.Revision);
                }
                Add(target, npc.CurrentGoalStableId); Add(target, npc.CurrentGoalStateCode);
                Add(target, npc.CurrentOrderStableId); Add(target, npc.CurrentOrderStageCode);
                Add(target, npc.ReservedItemStableId); Add(target, npc.LastConsumedItemStableId);
                Add(target, npc.NextGoalReasonCode); Add(target, npc.Revision);
            }
            foreach (var goal in value.Goals.OrderBy(item => item.GoalStableId,
                         StringComparer.Ordinal))
            {
                Add(target, goal.GoalStableId); Add(target, goal.NpcStableId);
                Add(target, goal.NeedCode); Add(target, goal.SourceSeverity);
                Add(target, goal.TargetItemStableId); Add(target, goal.StateCode);
                Add(target, goal.ReasonCode); Add(target, goal.TriggerSourceCode);
                Add(target, goal.SelectedAtWorldTick);
                Add(target, goal.CompletedAtWorldTick ?? -1); Add(target, goal.Revision);
            }
            foreach (var order in value.Orders.OrderBy(item => item.OrderStableId,
                         StringComparer.Ordinal))
            {
                Add(target, order.OrderStableId); Add(target, order.NpcStableId);
                Add(target, order.GoalStableId); Add(target, order.ItemStableId);
                Add(target, order.StageCode); Add(target, order.WorldInteractionId);
                Add(target, order.TriggerSourceCode);
                foreach (var wiId in order.WorldInteractionHistoryIds) Add(target, wiId);
                Add(target, order.AssignedClerkNpcStableId);
                Add(target, order.RequestedAtWorldRevision); Add(target, order.RequestedAtWorldTick);
                Add(target, order.StageChangedAtWorldTick); Add(target, order.Revision);
                AddBreakdown(target, order.ConsumptionBreakdown);
            }
            return target.ToString();
        }

        internal static string BuildTarotContextStateV3PayloadKey(
            SimulationTarotContextStateSnapshot value)
        {
            var target = new StringBuilder(CanonicalTarotContext(value));
            foreach (var activation in value.MajorArcanaActivations.OrderBy(item =>
                         item.ActivationSequence))
            {
                AddActivation(target, activation);
            }
            foreach (var inheritance in value.OrientationInheritances.OrderBy(item =>
                         item.InheritanceStableId, StringComparer.Ordinal))
            {
                Add(target, inheritance.InheritanceStableId);
                Add(target, inheritance.MajorArcanaActivationStableId);
                Add(target, inheritance.SourceCardStableId);
                Add(target, inheritance.DirectionCode);
                Add(target, inheritance.TargetCardFamilyCode);
                Add(target, inheritance.TargetCardStableId);
                Add(target, inheritance.TargetCardCopyStableId);
                Add(target, inheritance.InfluenceModeCode);
                Add(target, inheritance.NumericMultiplier?.ToString(
                    CultureInfo.InvariantCulture) ?? string.Empty);
                Add(target, inheritance.InterpretationCode);
                foreach (var binding in inheritance.AllowedEffectBindingCodes.OrderBy(
                             item => item, StringComparer.Ordinal)) Add(target, binding);
                Add(target, inheritance.InfluencePolicyRevision);
            }
            return target.ToString();
        }

        internal static string BuildTarotContextStateV3Hash(
            SimulationTarotContextStateSnapshot value)
            => Hash(BuildTarotContextStateV3PayloadKey(value));

        private static void AddActivation(StringBuilder target,
            Simulation메이저아르카나활성Snapshot value)
        {
            Add(target, value.MajorArcanaActivationStableId);
            Add(target, value.ActivationSequence); Add(target, value.StateCode);
            Add(target, value.Selection.CardStableId); Add(target, value.Selection.CardCopyStableId);
            Add(target, value.Selection.CardRevision); Add(target, value.Selection.OfferStableId);
            Add(target, value.Selection.SelectionSourceCode);
            Add(target, value.Selection.SelectedAtWorldRevision);
            Add(target, value.Selection.SelectedAtWorldTick);
            Add(target, value.OrientationDecision.DirectionCode);
            Add(target, value.OrientationDecision.RecoveryShareMicro);
            Add(target, value.OrientationDecision.RecoveryOutput);
            Add(target, value.OrientationDecision.ThreatOutput);
            Add(target, value.OrientationDecision.ContextPlayerStableId);
            Add(target, value.OrientationDecision.EvidenceRevision);
            Add(target, value.OrientationDecision.EvidenceHashSha256);
            Add(target, value.OrientationDecision.RuleRevision);
            Add(target, value.OrientationDecision.DecidedAtWorldRevision);
            Add(target, value.OrientationDecision.DecidedAtWorldTick);
            Add(target, value.ActivatedAtWorldRevision); Add(target, value.ActivatedAtWorldTick);
            Add(target, value.EndedAtWorldRevision ?? -1);
            Add(target, value.EndedAtWorldTick ?? -1);
            Add(target, value.EndReasonCode); Add(target, value.SupersededByActivationStableId);
        }

        private static void AddBreakdown(StringBuilder target,
            SimulationEffect배율계보Snapshot? value)
        {
            Add(target, value != null);
            if (value == null) return;
            Add(target, value.BreakdownStableId); Add(target, value.EffectBindingCode);
            Add(target, value.BaseValue); Add(target, value.LowerCardMultiplier);
            Add(target, value.ArcanaOrientationMultiplier);
            Add(target, value.PsychologicalPeriodMultiplier);
            Add(target, value.RawMultiplier); Add(target, value.ClampedMultiplier);
            Add(target, value.FinalValue); Add(target, value.ValueUnitCode);
            Add(target, value.MajorArcanaActivationStableId);
            Add(target, value.InheritanceStableId); Add(target, value.LowerCardStableId);
            Add(target, value.PeriodStateCode); Add(target, value.PeriodInstanceStableId);
            Add(target, value.PeriodRevision); Add(target, value.PeriodStateHashSha256);
            Add(target, value.RuleRevision);
        }

        internal static Simulation메이저아르카나활성Snapshot CloneMajorArcanaActivation(
            Simulation메이저아르카나활성Snapshot value) => new()
        {
            MajorArcanaActivationStableId = value.MajorArcanaActivationStableId,
            ActivationSequence = value.ActivationSequence,
            StateCode = value.StateCode,
            Selection = new Simulation메이저아르카나선택Snapshot
            {
                CardStableId = value.Selection.CardStableId,
                CardCopyStableId = value.Selection.CardCopyStableId,
                CardRevision = value.Selection.CardRevision,
                OfferStableId = value.Selection.OfferStableId,
                SelectionSourceCode = value.Selection.SelectionSourceCode,
                SelectedAtWorldRevision = value.Selection.SelectedAtWorldRevision,
                SelectedAtWorldTick = value.Selection.SelectedAtWorldTick,
            },
            OrientationDecision = CloneDirectionDecision(value.OrientationDecision),
            ActivatedAtWorldRevision = value.ActivatedAtWorldRevision,
            ActivatedAtWorldTick = value.ActivatedAtWorldTick,
            EndedAtWorldRevision = value.EndedAtWorldRevision,
            EndedAtWorldTick = value.EndedAtWorldTick,
            EndReasonCode = value.EndReasonCode,
            SupersededByActivationStableId = value.SupersededByActivationStableId,
        };

        internal static Simulation메이저아르카나방향판정Snapshot CloneDirectionDecision(
            Simulation메이저아르카나방향판정Snapshot value) => new()
        {
            DirectionCode = value.DirectionCode,
            RecoveryShareMicro = value.RecoveryShareMicro,
            RecoveryOutput = value.RecoveryOutput,
            ThreatOutput = value.ThreatOutput,
            ContextPlayerStableId = value.ContextPlayerStableId,
            EvidenceRevision = value.EvidenceRevision,
            EvidenceHashSha256 = value.EvidenceHashSha256,
            RuleRevision = value.RuleRevision,
            DecidedAtWorldRevision = value.DecidedAtWorldRevision,
            DecidedAtWorldTick = value.DecidedAtWorldTick,
        };

        internal static Simulation상위아르카나방향상속Snapshot CloneOrientationInheritance(
            Simulation상위아르카나방향상속Snapshot value) => new()
        {
            InheritanceStableId = value.InheritanceStableId,
            MajorArcanaActivationStableId = value.MajorArcanaActivationStableId,
            SourceCardStableId = value.SourceCardStableId,
            DirectionCode = value.DirectionCode,
            TargetCardFamilyCode = value.TargetCardFamilyCode,
            TargetCardStableId = value.TargetCardStableId,
            TargetCardCopyStableId = value.TargetCardCopyStableId,
            InfluenceModeCode = value.InfluenceModeCode,
            NumericMultiplier = value.NumericMultiplier,
            InterpretationCode = value.InterpretationCode,
            AllowedEffectBindingCodes = value.AllowedEffectBindingCodes.ToArray(),
            InfluencePolicyRevision = value.InfluencePolicyRevision,
        };

        internal static SimulationTownNpcLifeStateSnapshot CloneTownNpcLifeState(
            SimulationTownNpcLifeStateSnapshot value) => new()
        {
            ProfileStableId = value.ProfileStableId,
            RuleRevision = value.RuleRevision,
            ContentionRuleRevision = value.ContentionRuleRevision,
            ContextPlayerStableId = value.ContextPlayerStableId,
            WorldTick = value.WorldTick,
            WorldRevision = value.WorldRevision,
            Items = value.Items.Select(CloneTownItem).ToArray(),
            Npcs = value.Npcs.Select(CloneTownNpc).ToArray(),
            Goals = value.Goals.Select(CloneTownGoal).ToArray(),
            Orders = value.Orders.Select(CloneTownOrder).ToArray(),
            StateHashSha256 = value.StateHashSha256,
            SimulationOnly = value.SimulationOnly,
            IsOperationalState = value.IsOperationalState,
        };

        private static SimulationTown욕구Snapshot CloneTownNeed(
            SimulationTown욕구Snapshot value) => new()
        {
            NeedCode = value.NeedCode,
            Severity = value.Severity,
            Revision = value.Revision,
        };

        private static SimulationTown물품Snapshot CloneTownItem(
            SimulationTown물품Snapshot value) => new()
        {
            ItemStableId = value.ItemStableId,
            KoreanName = value.KoreanName,
            ItemRoleCode = value.ItemRoleCode,
            AvailableQuantity = value.AvailableQuantity,
            BaseLifeRecovery = value.BaseLifeRecovery,
            UnitCode = value.UnitCode,
            ReservedByNpcStableId = value.ReservedByNpcStableId,
            Revision = value.Revision,
        };

        private static SimulationTown목표Snapshot CloneTownGoal(
            SimulationTown목표Snapshot value) => new()
        {
            GoalStableId = value.GoalStableId,
            NpcStableId = value.NpcStableId,
            NeedCode = value.NeedCode,
            SourceSeverity = value.SourceSeverity,
            TargetItemStableId = value.TargetItemStableId,
            StateCode = value.StateCode,
            ReasonCode = value.ReasonCode,
            TriggerSourceCode = value.TriggerSourceCode,
            SelectedAtWorldTick = value.SelectedAtWorldTick,
            CompletedAtWorldTick = value.CompletedAtWorldTick,
            Revision = value.Revision,
        };

        private static SimulationTown주문Snapshot CloneTownOrder(
            SimulationTown주문Snapshot value) => new()
        {
            OrderStableId = value.OrderStableId,
            NpcStableId = value.NpcStableId,
            GoalStableId = value.GoalStableId,
            ItemStableId = value.ItemStableId,
            StageCode = value.StageCode,
            WorldInteractionId = value.WorldInteractionId,
            TriggerSourceCode = value.TriggerSourceCode,
            WorldInteractionHistoryIds = value.WorldInteractionHistoryIds.ToArray(),
            AssignedClerkNpcStableId = value.AssignedClerkNpcStableId,
            RequestedAtWorldRevision = value.RequestedAtWorldRevision,
            RequestedAtWorldTick = value.RequestedAtWorldTick,
            StageChangedAtWorldTick = value.StageChangedAtWorldTick,
            Revision = value.Revision,
            ConsumptionBreakdown = value.ConsumptionBreakdown == null ? null
                : CloneBreakdown(value.ConsumptionBreakdown),
        };

        private static SimulationEffect배율계보Snapshot CloneBreakdown(
            SimulationEffect배율계보Snapshot value) => new()
        {
            BreakdownStableId = value.BreakdownStableId,
            EffectBindingCode = value.EffectBindingCode,
            BaseValue = value.BaseValue,
            LowerCardMultiplier = value.LowerCardMultiplier,
            ArcanaOrientationMultiplier = value.ArcanaOrientationMultiplier,
            PsychologicalPeriodMultiplier = value.PsychologicalPeriodMultiplier,
            RawMultiplier = value.RawMultiplier,
            ClampedMultiplier = value.ClampedMultiplier,
            FinalValue = value.FinalValue,
            ValueUnitCode = value.ValueUnitCode,
            MajorArcanaActivationStableId = value.MajorArcanaActivationStableId,
            InheritanceStableId = value.InheritanceStableId,
            LowerCardStableId = value.LowerCardStableId,
            PeriodStateCode = value.PeriodStateCode,
            PeriodInstanceStableId = value.PeriodInstanceStableId,
            PeriodRevision = value.PeriodRevision,
            PeriodStateHashSha256 = value.PeriodStateHashSha256,
            RuleRevision = value.RuleRevision,
        };

        private static SimulationTownNpcLifeSnapshot CloneTownNpc(
            SimulationTownNpcLifeSnapshot value) => new()
        {
            NpcStableId = value.NpcStableId,
            DisplayName = value.DisplayName,
            Needs = value.Needs.Select(CloneTownNeed).ToArray(),
            CurrentGoalStableId = value.CurrentGoalStableId,
            CurrentGoalStateCode = value.CurrentGoalStateCode,
            CurrentOrderStableId = value.CurrentOrderStableId,
            CurrentOrderStageCode = value.CurrentOrderStageCode,
            ReservedItemStableId = value.ReservedItemStableId,
            LastConsumedItemStableId = value.LastConsumedItemStableId,
            NextGoalReasonCode = value.NextGoalReasonCode,
            Revision = value.Revision,
        };

        private sealed class TownNpcLifeState
        {
            public string NpcStableId { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public Dictionary<string, SimulationTown욕구Snapshot> Needs { get; set; }
                = new(StringComparer.Ordinal);
            public string CurrentGoalStableId { get; set; } = string.Empty;
            public string CurrentGoalStateCode { get; set; } = string.Empty;
            public string CurrentOrderStableId { get; set; } = string.Empty;
            public string LastConsumedItemStableId { get; set; } = string.Empty;
            public string NextGoalReasonCode { get; set; } = string.Empty;
            public long Revision { get; set; }
        }

        private sealed class TownGoalCandidate
        {
            public TownGoalCandidate(TownNpcLifeState npc,
                SimulationTown욕구Snapshot need, SimulationTown물품Snapshot item,
                long requestWorldRevision, int requestWorldTick)
            {
                Npc = npc;
                Need = need;
                Item = item;
                RequestWorldRevision = requestWorldRevision;
                RequestWorldTick = requestWorldTick;
            }

            public TownNpcLifeState Npc { get; }
            public SimulationTown욕구Snapshot Need { get; }
            public SimulationTown물품Snapshot Item { get; }
            public long RequestWorldRevision { get; }
            public int RequestWorldTick { get; }
        }
    }
}
