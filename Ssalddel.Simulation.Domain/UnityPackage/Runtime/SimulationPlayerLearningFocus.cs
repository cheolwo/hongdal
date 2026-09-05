using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class Simulation기본Npc학습카드Catalog
    {
        public static Simulation학습중점InitialState CreateHansInitialState(
            string sessionStableId,
            string playerStableId,
            string scheduleRevision,
            Simulation학습구간Snapshot[] segments)
        {
            var cards = new[]
            {
                Card(
                    Simulation학습중점Codes.HansFarmingCardStableId,
                    "hans-farming.r1",
                    "한스의 농장일",
                    Simulation학습중점Codes.Wood,
                    new[]
                    {
                        Binding("WI-FARM-01", Simulation플레이어분야Codes.농업생산, "soil"),
                        Binding("WI-FARM-02", Simulation플레이어분야Codes.농업생산, "sowing"),
                        Binding("WI-FARM-03", Simulation플레이어분야Codes.농업생산, "growth"),
                        Binding("WI-FARM-04", Simulation플레이어분야Codes.농업생산, "harvest"),
                        Binding("WI-FARM-05", Simulation플레이어분야Codes.농업생산, "collection"),
                        Binding("WI-FARM-06", Simulation플레이어분야Codes.농업생산, "packing"),
                    }),
                Card(
                    Simulation학습중점Codes.HansAxeCardStableId,
                    "hans-axe.r1",
                    "한스의 도끼 운용",
                    Simulation학습중점Codes.Metal,
                    new[]
                    {
                        Binding("WI-NATURE-06", Simulation플레이어분야Codes.채집자원, "logging"),
                        Binding("WI-NATURE-11", Simulation플레이어분야Codes.전투사냥, "encounter-combat"),
                    }),
            };
            foreach (var card in cards)
                card.DefinitionHashSha256 =
                    Simulation학습중점State.CalculateCardDefinitionHash(card);
            return new Simulation학습중점InitialState
            {
                SessionStableId = sessionStableId,
                PlayerStableId = playerStableId,
                ScheduleRevision = scheduleRevision,
                Segments = segments ?? Array.Empty<Simulation학습구간Snapshot>(),
                Cards = cards,
                OwnedCardStableIds = cards.Select(value => value.CardStableId)
                    .ToArray(),
            };
        }

        private static Simulation학습카드DefinitionSnapshot Card(
            string stableId,
            string revision,
            string title,
            string primaryElement,
            Simulation학습카드BindingSnapshot[] bindings)
            => new Simulation학습카드DefinitionSnapshot
            {
                CardStableId = stableId,
                CardRevision = revision,
                SourceActorStableId = Simulation학습중점Codes.HansActorStableId,
                Title = title,
                PrimaryFiveElementCode = primaryElement,
                Bindings = bindings,
                UnderstandingDelta = 1,
                EffectRuleRevision = "npc-learning-understanding.r1",
            };

        private static Simulation학습카드BindingSnapshot Binding(
            string worldInteractionId,
            string domainStableId,
            string skillStableId)
            => new Simulation학습카드BindingSnapshot
            {
                WorldInteractionId = worldInteractionId,
                DomainStableId = domainStableId,
                SkillStableId = skillStableId,
            };
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "NPC 학습 카드의 한 슬롯 예약·활성화와 행위별 약한 이해도 기여를 판정한다.",
        Boundary = "산출량·속도·피해·회복·위협·이데아 확률을 직접 변경하지 않는다.")]
    public sealed class Simulation학습중점State
    {
        private readonly object gate = new object();
        private Simulation학습중점StateSnapshot state;

        public Simulation학습중점State(Simulation학습중점InitialState initial)
        {
            ValidateInitial(initial);
            state = new Simulation학습중점StateSnapshot
            {
                SessionStableId = initial.SessionStableId.Trim(),
                PlayerStableId = initial.PlayerStableId.Trim(),
                RuleRevision = initial.RuleRevision.Trim(),
                ScheduleRevision = initial.ScheduleRevision.Trim(),
                Segments = initial.Segments.Select(Clone).ToArray(),
                Cards = initial.Cards.Select(Clone).ToArray(),
                OwnedCardStableIds = Normalize(initial.OwnedCardStableIds),
                ActiveCardStableId = (initial.ActiveCardStableId ?? string.Empty).Trim(),
                ActiveFromSegmentStableId =
                    (initial.ActiveFromSegmentStableId ?? string.Empty).Trim(),
            };
            RefreshHash();
        }

        private Simulation학습중점State(Simulation학습중점StateSnapshot snapshot)
        {
            ValidateState(snapshot);
            state = Clone(snapshot);
        }

        public static Simulation학습중점State Restore(
            Simulation학습중점StateSnapshot snapshot)
            => new Simulation학습중점State(snapshot);

        public Simulation학습중점StateSnapshot Snapshot()
        {
            lock (gate) return Clone(state);
        }

        public Simulation학습중점ProjectionSnapshot Project(int currentWorldTick)
        {
            lock (gate)
            {
                var current = CurrentSegment(currentWorldTick);
                return new Simulation학습중점ProjectionSnapshot
                {
                    SessionStableId = state.SessionStableId,
                    PlayerStableId = state.PlayerStableId,
                    Revision = state.Revision,
                    CurrentSegmentStableId = current?.SegmentStableId ?? string.Empty,
                    ActiveCardStableId = state.ActiveCardStableId,
                    PendingCardStableId = state.PendingChange?.CardStableId
                        ?? string.Empty,
                    PendingEffectiveSegmentStableId =
                        state.PendingChange?.EffectiveSegmentStableId
                        ?? string.Empty,
                    PendingEffectiveWorldTick =
                        state.PendingChange?.EffectiveWorldTick ?? -1,
                    OwnedCards = state.Cards.Where(value =>
                            state.OwnedCardStableIds.Contains(value.CardStableId,
                                StringComparer.Ordinal))
                        .OrderBy(value => value.CardStableId,
                            StringComparer.Ordinal)
                        .Select(Clone).ToArray(),
                    LastEffectReceipt = state.EffectReceipts
                        .OrderByDescending(value => value.AppliedWorldRevision)
                        .ThenByDescending(value => value.ReceiptStableId,
                            StringComparer.Ordinal)
                        .Select(Clone).FirstOrDefault(),
                    StateHashSha256 = state.StateHashSha256,
                };
            }
        }

        public Simulation학습중점PreviewSnapshot Preview(
            Simulation학습중점ChangeRequest request,
            int currentWorldTick)
        {
            if (request == null)
                throw new SimulationContractException(
                    "SimulationLearningFocusRequestRequired");
            lock (gate)
            {
                ValidateChangeRequest(request);
                var effective = ResolveEffectiveSegment(currentWorldTick);
                return new Simulation학습중점PreviewSnapshot
                {
                    PlayerStableId = state.PlayerStableId,
                    CurrentRevision = state.Revision,
                    CurrentSegmentStableId =
                        CurrentSegment(currentWorldTick)?.SegmentStableId
                        ?? string.Empty,
                    ActiveCardStableId = state.ActiveCardStableId,
                    RequestedCardStableId = request.CardStableId.Trim(),
                    EffectiveSegmentStableId = effective.SegmentStableId,
                    EffectiveWorldTick = effective.StartWorldTickInclusive,
                    AppliesAtCurrentBoundary =
                        effective.StartWorldTickInclusive == currentWorldTick,
                    WouldReplacePendingChange = state.PendingChange != null
                        && string.Equals(
                            state.PendingChange.EffectiveSegmentStableId,
                            effective.SegmentStableId,
                            StringComparison.Ordinal),
                };
            }
        }

        public Simulation학습중점StateSnapshot Confirm(
            Simulation학습중점ChangeRequest request,
            int currentWorldTick)
        {
            if (request == null)
                throw new SimulationContractException(
                    "SimulationLearningFocusRequestRequired");
            lock (gate)
            {
                var existing = state.ChangeReceipts.SingleOrDefault(value =>
                    value.ClientRequestId == request.ClientRequestId);
                if (existing != null)
                {
                    if (existing.ExpectedRevision != request.ExpectedRevision
                        || !string.Equals(existing.PlayerStableId,
                            request.PlayerStableId?.Trim(),
                            StringComparison.Ordinal)
                        || !string.Equals(existing.CardStableId,
                            request.CardStableId?.Trim(),
                            StringComparison.Ordinal))
                        throw new SimulationConflictException(
                            "SimulationLearningFocusRequestPayloadConflict");
                    return Clone(state);
                }

                ValidateChangeRequest(request);
                if (request.ExpectedRevision != state.Revision)
                    throw new SimulationConflictException(
                        "SimulationLearningFocusExpectedRevisionMismatch");
                var preview = Preview(request, currentWorldTick);
                var nextRevision = state.Revision + 1;
                if (preview.AppliesAtCurrentBoundary)
                {
                    state.ActiveCardStableId = request.CardStableId.Trim();
                    state.ActiveFromSegmentStableId =
                        preview.EffectiveSegmentStableId;
                    state.PendingChange = null;
                    state.ActivationHistory = state.ActivationHistory.Concat(new[]
                    {
                        new Simulation학습중점ActivationSnapshot
                        {
                            CardStableId = request.CardStableId.Trim(),
                            SegmentStableId = preview.EffectiveSegmentStableId,
                            ActivatedWorldTick = preview.EffectiveWorldTick,
                            SourceClientRequestId = request.ClientRequestId,
                            ResultingRevision = nextRevision,
                        },
                    }).ToArray();
                }
                else
                {
                    state.PendingChange =
                        new Simulation학습중점PendingChangeSnapshot
                        {
                            CardStableId = request.CardStableId.Trim(),
                            EffectiveSegmentStableId =
                                preview.EffectiveSegmentStableId,
                            EffectiveWorldTick = preview.EffectiveWorldTick,
                            SourceClientRequestId = request.ClientRequestId,
                        };
                }
                state.ChangeReceipts = state.ChangeReceipts.Concat(new[]
                {
                    new Simulation학습중점ChangeReceiptSnapshot
                    {
                        ClientRequestId = request.ClientRequestId,
                        ExpectedRevision = request.ExpectedRevision,
                        PlayerStableId = state.PlayerStableId,
                        CardStableId = request.CardStableId.Trim(),
                        EffectiveSegmentStableId =
                            preview.EffectiveSegmentStableId,
                        EffectiveWorldTick = preview.EffectiveWorldTick,
                        ResultingRevision = nextRevision,
                    },
                }).ToArray();
                state.Revision = nextRevision;
                RefreshHash();
                return Clone(state);
            }
        }

        public void Advance(int previousWorldTick, int currentWorldTick)
        {
            lock (gate)
            {
                if (currentWorldTick < previousWorldTick)
                    throw new SimulationContractException(
                        "SimulationLearningFocusWorldTickInvalid");
                var pending = state.PendingChange;
                if (pending == null
                    || pending.EffectiveWorldTick <= previousWorldTick
                    || pending.EffectiveWorldTick > currentWorldTick)
                    return;
                var nextRevision = state.Revision + 1;
                state.ActiveCardStableId = pending.CardStableId;
                state.ActiveFromSegmentStableId =
                    pending.EffectiveSegmentStableId;
                state.ActivationHistory = state.ActivationHistory.Concat(new[]
                {
                    new Simulation학습중점ActivationSnapshot
                    {
                        CardStableId = pending.CardStableId,
                        SegmentStableId = pending.EffectiveSegmentStableId,
                        ActivatedWorldTick = pending.EffectiveWorldTick,
                        SourceClientRequestId = pending.SourceClientRequestId,
                        ResultingRevision = nextRevision,
                    },
                }).ToArray();
                state.PendingChange = null;
                state.Revision = nextRevision;
                RefreshHash();
            }
        }

        internal bool TryCreateContribution(
            Simulation행위발현Record record,
            string playerStableId,
            out SimulationNpc학습중점기여Request request,
            out Simulation학습효과ReceiptSnapshot receipt)
        {
            lock (gate)
            {
                request = new SimulationNpc학습중점기여Request();
                receipt = new Simulation학습효과ReceiptSnapshot();
                if (string.IsNullOrWhiteSpace(state.ActiveCardStableId)
                    || !string.Equals(state.PlayerStableId, playerStableId,
                        StringComparison.Ordinal)
                    || !string.Equals(record.TriggerSourceCode,
                        SimulationWorldInteractionTriggerSourceCodes.PlayerDriven,
                        StringComparison.Ordinal)
                    || !string.Equals(record.ActorStableId, playerStableId,
                        StringComparison.Ordinal)
                    || !IsLearningResult(record.결과분류Code))
                    return false;
                var card = state.Cards.Single(value => string.Equals(
                    value.CardStableId, state.ActiveCardStableId,
                    StringComparison.Ordinal));
                var binding = card.Bindings.SingleOrDefault(value =>
                    string.Equals(value.WorldInteractionId,
                        record.WorldInteractionId, StringComparison.Ordinal));
                if (binding == null) return false;
                var receiptId = "learning-focus-effect:" + Hash(string.Join("|",
                    state.PlayerStableId, card.CardStableId, card.CardRevision,
                    record.행위기록StableId, card.EffectRuleRevision));
                if (state.EffectReceipts.Any(value => string.Equals(
                        value.ReceiptStableId, receiptId,
                        StringComparison.Ordinal)))
                    return false;
                receipt = new Simulation학습효과ReceiptSnapshot
                {
                    ReceiptStableId = receiptId,
                    CardStableId = card.CardStableId,
                    CardRevision = card.CardRevision,
                    SourceActorStableId = card.SourceActorStableId,
                    SourceActionRecordStableId = record.행위기록StableId,
                    WorldInteractionId = record.WorldInteractionId,
                    DomainStableId = binding.DomainStableId,
                    SkillStableId = binding.SkillStableId,
                    ResultCode = record.결과분류Code,
                    UnderstandingDelta = card.UnderstandingDelta,
                    AppliedWorldRevision = record.AfterWorldRevision,
                    RuleRevision = card.EffectRuleRevision,
                };
                request = new SimulationNpc학습중점기여Request
                {
                    PlayerStableId = state.PlayerStableId,
                    CardStableId = card.CardStableId,
                    CardRevision = card.CardRevision,
                    CardDefinitionHashSha256 = card.DefinitionHashSha256,
                    SourceActorStableId = card.SourceActorStableId,
                    EffectReceiptStableId = receiptId,
                    ActionRecord = record,
                    EffectLine = new Simulation분야이해효과선Snapshot
                    {
                        분야StableId = binding.DomainStableId,
                        세부숙련StableId = binding.SkillStableId,
                        이해도증가량 = card.UnderstandingDelta,
                        RuleRevision = card.EffectRuleRevision,
                    },
                };
                return true;
            }
        }

        internal void CommitContribution(
            Simulation학습효과ReceiptSnapshot receipt)
        {
            lock (gate)
            {
                if (state.EffectReceipts.Any(value => string.Equals(
                        value.ReceiptStableId, receipt.ReceiptStableId,
                        StringComparison.Ordinal)))
                    return;
                state.EffectReceipts = state.EffectReceipts.Concat(new[]
                {
                    Clone(receipt),
                }).ToArray();
                state.Revision++;
                RefreshHash();
            }
        }

        public static void ValidateInitial(Simulation학습중점InitialState initial)
        {
            if (initial == null)
                throw new SimulationContractException(
                    "SimulationLearningFocusInitialStateRequired");
            Require(initial.SessionStableId,
                "SimulationLearningFocusSessionInvalid");
            Require(initial.PlayerStableId,
                "SimulationLearningFocusPlayerInvalid");
            if (!string.Equals(initial.RuleRevision,
                    Simulation학습중점Codes.RuleRevision,
                    StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationLearningFocusRuleRevisionInvalid");
            Require(initial.ScheduleRevision,
                "SimulationLearningFocusScheduleRevisionInvalid");
            ValidateSegments(initial.Segments);
            ValidateCards(initial.Cards);
            var owned = Normalize(initial.OwnedCardStableIds);
            if (owned.Length != (initial.OwnedCardStableIds?.Length ?? 0)
                || owned.Any(id => !initial.Cards.Any(card =>
                    string.Equals(card.CardStableId, id,
                        StringComparison.Ordinal))))
                throw new SimulationContractException(
                    "SimulationLearningFocusOwnedCardInvalid");
            if (!string.IsNullOrWhiteSpace(initial.ActiveCardStableId)
                && !owned.Contains(initial.ActiveCardStableId.Trim(),
                    StringComparer.Ordinal))
                throw new SimulationContractException(
                    "SimulationLearningFocusActiveCardNotOwned");
            if (string.IsNullOrWhiteSpace(initial.ActiveCardStableId)
                != string.IsNullOrWhiteSpace(
                    initial.ActiveFromSegmentStableId))
                throw new SimulationContractException(
                    "SimulationLearningFocusActiveSegmentInvalid");
            if (!string.IsNullOrWhiteSpace(initial.ActiveFromSegmentStableId)
                && !initial.Segments.Any(value => string.Equals(
                    value.SegmentStableId,
                    initial.ActiveFromSegmentStableId.Trim(),
                    StringComparison.Ordinal)))
                throw new SimulationContractException(
                    "SimulationLearningFocusActiveSegmentInvalid");
        }

        public static string CalculateCardDefinitionHash(
            Simulation학습카드DefinitionSnapshot card)
        {
            var canonical = new StringBuilder();
            Add(canonical, card.CardStableId);
            Add(canonical, card.CardRevision);
            Add(canonical, card.SourceKindCode);
            Add(canonical, card.SourceActorStableId);
            Add(canonical, card.Title);
            Add(canonical, card.PrimaryFiveElementCode);
            AddStrings(canonical, card.SupportingFiveElementCodes);
            foreach (var binding in (card.Bindings
                         ?? Array.Empty<Simulation학습카드BindingSnapshot>())
                     .OrderBy(value => value.WorldInteractionId,
                         StringComparer.Ordinal))
            {
                Add(canonical, binding.WorldInteractionId);
                Add(canonical, binding.DomainStableId);
                Add(canonical, binding.SkillStableId);
            }
            Add(canonical, card.UnderstandingDelta);
            Add(canonical, card.EffectRuleRevision);
            return Hash(canonical.ToString());
        }

        public static string CalculateStateHash(
            Simulation학습중점StateSnapshot value)
        {
            var canonical = new StringBuilder();
            Add(canonical, value.SchemaVersion);
            Add(canonical, value.SessionStableId);
            Add(canonical, value.PlayerStableId);
            Add(canonical, value.Revision);
            Add(canonical, value.RuleRevision);
            Add(canonical, value.ScheduleRevision);
            foreach (var segment in value.Segments)
            {
                Add(canonical, segment.SegmentStableId);
                Add(canonical, segment.SolarTermStableId);
                Add(canonical, segment.SolarTermRevision);
                Add(canonical, segment.PhaseCode);
                Add(canonical, segment.StartWorldTickInclusive);
                Add(canonical, segment.EndWorldTickExclusive);
            }
            foreach (var card in value.Cards.OrderBy(item => item.CardStableId,
                         StringComparer.Ordinal))
                Add(canonical, card.DefinitionHashSha256);
            AddStrings(canonical, value.OwnedCardStableIds);
            Add(canonical, value.ActiveCardStableId);
            Add(canonical, value.ActiveFromSegmentStableId);
            Add(canonical, value.PendingChange?.CardStableId);
            Add(canonical, value.PendingChange?.EffectiveSegmentStableId);
            Add(canonical, value.PendingChange?.EffectiveWorldTick ?? -1);
            Add(canonical, value.PendingChange?.SourceClientRequestId
                .ToString("N") ?? string.Empty);
            foreach (var receipt in value.ChangeReceipts.OrderBy(item =>
                         item.ClientRequestId))
            {
                Add(canonical, receipt.ClientRequestId.ToString("N"));
                Add(canonical, receipt.ExpectedRevision);
                Add(canonical, receipt.PlayerStableId);
                Add(canonical, receipt.CardStableId);
                Add(canonical, receipt.EffectiveSegmentStableId);
                Add(canonical, receipt.EffectiveWorldTick);
                Add(canonical, receipt.ResultingRevision);
            }
            foreach (var activation in value.ActivationHistory)
            {
                Add(canonical, activation.CardStableId);
                Add(canonical, activation.SegmentStableId);
                Add(canonical, activation.ActivatedWorldTick);
                Add(canonical, activation.SourceClientRequestId.ToString("N"));
                Add(canonical, activation.ResultingRevision);
            }
            foreach (var receipt in value.EffectReceipts.OrderBy(item =>
                         item.ReceiptStableId, StringComparer.Ordinal))
            {
                Add(canonical, receipt.ReceiptStableId);
                Add(canonical, receipt.CardStableId);
                Add(canonical, receipt.CardRevision);
                Add(canonical, receipt.SourceActorStableId);
                Add(canonical, receipt.SourceActionRecordStableId);
                Add(canonical, receipt.WorldInteractionId);
                Add(canonical, receipt.DomainStableId);
                Add(canonical, receipt.SkillStableId);
                Add(canonical, receipt.ResultCode);
                Add(canonical, receipt.UnderstandingDelta);
                Add(canonical, receipt.AppliedWorldRevision);
                Add(canonical, receipt.RuleRevision);
            }
            Add(canonical, value.SimulationOnly);
            Add(canonical, value.IsOperationalState);
            return Hash(canonical.ToString());
        }

        private void ValidateChangeRequest(
            Simulation학습중점ChangeRequest request)
        {
            if (request.ClientRequestId == Guid.Empty)
                throw new SimulationContractException(
                    "SimulationLearningFocusClientRequestIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException(
                    "SimulationLearningFocusExpectedRevisionInvalid");
            if (!string.Equals(request.PlayerStableId?.Trim(),
                    state.PlayerStableId, StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationLearningFocusPlayerMismatch");
            var cardId = request.CardStableId?.Trim() ?? string.Empty;
            if (cardId.Length > 0
                && !state.OwnedCardStableIds.Contains(cardId,
                    StringComparer.Ordinal))
                throw new SimulationContractException(
                    "SimulationLearningFocusCardNotOwned");
        }

        private Simulation학습구간Snapshot ResolveEffectiveSegment(
            int currentWorldTick)
        {
            var first = state.Segments[0];
            if (currentWorldTick == first.StartWorldTickInclusive
                && state.Revision == 0
                && string.IsNullOrWhiteSpace(state.ActiveCardStableId)
                && state.PendingChange == null
                && state.ChangeReceipts.Length == 0)
                return first;
            return state.Segments.FirstOrDefault(value =>
                       value.StartWorldTickInclusive > currentWorldTick)
                   ?? throw new SimulationConflictException(
                       "SimulationLearningFocusNextSegmentUnavailable");
        }

        private Simulation학습구간Snapshot? CurrentSegment(int worldTick)
            => state.Segments.SingleOrDefault(value =>
                value.StartWorldTickInclusive <= worldTick
                && worldTick < value.EndWorldTickExclusive);

        private void RefreshHash()
            => state.StateHashSha256 = CalculateStateHash(state);

        private static void ValidateState(
            Simulation학습중점StateSnapshot snapshot)
        {
            if (snapshot == null
                || !string.Equals(snapshot.SchemaVersion,
                    Simulation학습중점Codes.SchemaVersion,
                    StringComparison.Ordinal)
                || snapshot.Revision < 0
                || !snapshot.SimulationOnly
                || snapshot.IsOperationalState)
                throw new SimulationContractException(
                    "SimulationLearningFocusStateInvalid");
            ValidateInitial(new Simulation학습중점InitialState
            {
                SessionStableId = snapshot.SessionStableId,
                PlayerStableId = snapshot.PlayerStableId,
                RuleRevision = snapshot.RuleRevision,
                ScheduleRevision = snapshot.ScheduleRevision,
                Segments = snapshot.Segments,
                Cards = snapshot.Cards,
                OwnedCardStableIds = snapshot.OwnedCardStableIds,
                ActiveCardStableId = snapshot.ActiveCardStableId,
                ActiveFromSegmentStableId = snapshot.ActiveFromSegmentStableId,
            });
            if (!string.Equals(snapshot.StateHashSha256,
                    CalculateStateHash(snapshot), StringComparison.Ordinal))
                throw new SimulationContractException(
                    "SimulationLearningFocusStateHashMismatch");
        }

        private static void ValidateSegments(
            Simulation학습구간Snapshot[]? segments)
        {
            if (segments == null || segments.Length != 3)
                throw new SimulationContractException(
                    "SimulationLearningFocusSegmentCountInvalid");
            var phases = new[] { Simulation학습중점Codes.Early,
                Simulation학습중점Codes.Middle, Simulation학습중점Codes.Late };
            for (var index = 0; index < segments.Length; index++)
            {
                var value = segments[index];
                Require(value.SegmentStableId,
                    "SimulationLearningFocusSegmentIdInvalid");
                Require(value.SolarTermStableId,
                    "SimulationLearningFocusSolarTermInvalid");
                Require(value.SolarTermRevision,
                    "SimulationLearningFocusSolarTermRevisionInvalid");
                if (!string.Equals(value.PhaseCode, phases[index],
                        StringComparison.Ordinal)
                    || value.StartWorldTickInclusive < 0
                    || value.EndWorldTickExclusive
                        <= value.StartWorldTickInclusive
                    || (index > 0 && segments[index - 1]
                        .EndWorldTickExclusive
                        != value.StartWorldTickInclusive)
                    || !string.Equals(value.SolarTermStableId,
                        segments[0].SolarTermStableId,
                        StringComparison.Ordinal)
                    || !string.Equals(value.SolarTermRevision,
                        segments[0].SolarTermRevision,
                        StringComparison.Ordinal))
                    throw new SimulationContractException(
                        "SimulationLearningFocusSegmentScheduleInvalid");
            }
            if (segments.Select(value => value.SegmentStableId).Distinct(
                    StringComparer.Ordinal).Count() != segments.Length)
                throw new SimulationContractException(
                    "SimulationLearningFocusSegmentIdDuplicate");
        }

        private static void ValidateCards(
            Simulation학습카드DefinitionSnapshot[]? cards)
        {
            if (cards == null || cards.Length == 0
                || cards.Select(value => value.CardStableId).Distinct(
                    StringComparer.Ordinal).Count() != cards.Length)
                throw new SimulationContractException(
                    "SimulationLearningFocusCardCatalogInvalid");
            var catalog = Simulation기본플레이어분야Catalog.Create();
            foreach (var card in cards)
            {
                Require(card.CardStableId,
                    "SimulationLearningFocusCardIdInvalid");
                Require(card.CardRevision,
                    "SimulationLearningFocusCardRevisionInvalid");
                Require(card.SourceActorStableId,
                    "SimulationLearningFocusSourceActorInvalid");
                Require(card.Title,
                    "SimulationLearningFocusCardTitleInvalid");
                Require(card.PrimaryFiveElementCode,
                    "SimulationLearningFocusPrimaryElementInvalid");
                Require(card.EffectRuleRevision,
                    "SimulationLearningFocusEffectRuleRevisionInvalid");
                if (!string.Equals(card.SourceKindCode,
                        Simulation학습중점Codes.NpcSource,
                        StringComparison.Ordinal)
                    || card.UnderstandingDelta != 1
                    || card.Bindings == null || card.Bindings.Length == 0
                    || card.Bindings.Select(value => value.WorldInteractionId)
                        .Distinct(StringComparer.Ordinal).Count()
                        != card.Bindings.Length
                    || !string.Equals(card.DefinitionHashSha256,
                        CalculateCardDefinitionHash(card),
                        StringComparison.Ordinal))
                    throw new SimulationContractException(
                        "SimulationLearningFocusCardDefinitionInvalid");
                foreach (var binding in card.Bindings)
                {
                    var wi = catalog.Wi결속들.SingleOrDefault(value =>
                        string.Equals(value.WorldInteractionId,
                            binding.WorldInteractionId,
                            StringComparison.Ordinal));
                    if (wi == null || !wi.결속선들.Any(value =>
                            string.Equals(value.분야StableId,
                                binding.DomainStableId,
                                StringComparison.Ordinal)
                            && string.Equals(value.세부숙련StableId,
                                binding.SkillStableId,
                                StringComparison.Ordinal)))
                        throw new SimulationContractException(
                            "SimulationLearningFocusBindingInvalid");
                }
            }
        }

        private static bool IsLearningResult(string value)
            => string.Equals(value, Simulation행위결과분류Codes.성공,
                   StringComparison.Ordinal)
               || string.Equals(value,
                   Simulation행위결과분류Codes.의미있는실패,
                   StringComparison.Ordinal)
               || string.Equals(value,
                   Simulation행위결과분류Codes.후퇴복구,
                   StringComparison.Ordinal);

        private static string[] Normalize(string[]? values)
            => (values ?? Array.Empty<string>()).Select(value =>
                    Require(value,
                        "SimulationLearningFocusStableIdInvalid"))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal).ToArray();

        private static string Require(string? value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new SimulationContractException(errorCode);
            return value.Trim();
        }

        private static void Add(StringBuilder target, object? value)
            => target.Append(value?.ToString() ?? string.Empty).Append('\n');

        private static void AddStrings(StringBuilder target, string[]? values)
        {
            foreach (var value in (values ?? Array.Empty<string>())
                         .OrderBy(item => item, StringComparer.Ordinal))
                Add(target, value);
        }

        private static string Hash(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }

        internal static Simulation학습중점InitialState Clone(
            Simulation학습중점InitialState source)
            => new Simulation학습중점InitialState
            {
                SessionStableId = source.SessionStableId,
                PlayerStableId = source.PlayerStableId,
                RuleRevision = source.RuleRevision,
                ScheduleRevision = source.ScheduleRevision,
                Segments = source.Segments.Select(Clone).ToArray(),
                Cards = source.Cards.Select(Clone).ToArray(),
                OwnedCardStableIds = source.OwnedCardStableIds.ToArray(),
                ActiveCardStableId = source.ActiveCardStableId,
                ActiveFromSegmentStableId = source.ActiveFromSegmentStableId,
            };

        internal static Simulation학습중점StateSnapshot Clone(
            Simulation학습중점StateSnapshot source)
            => new Simulation학습중점StateSnapshot
            {
                SchemaVersion = source.SchemaVersion,
                SessionStableId = source.SessionStableId,
                PlayerStableId = source.PlayerStableId,
                Revision = source.Revision,
                RuleRevision = source.RuleRevision,
                ScheduleRevision = source.ScheduleRevision,
                Segments = source.Segments.Select(Clone).ToArray(),
                Cards = source.Cards.Select(Clone).ToArray(),
                OwnedCardStableIds = source.OwnedCardStableIds.ToArray(),
                ActiveCardStableId = source.ActiveCardStableId,
                ActiveFromSegmentStableId = source.ActiveFromSegmentStableId,
                PendingChange = source.PendingChange == null ? null
                    : new Simulation학습중점PendingChangeSnapshot
                    {
                        CardStableId = source.PendingChange.CardStableId,
                        EffectiveSegmentStableId = source.PendingChange
                            .EffectiveSegmentStableId,
                        EffectiveWorldTick = source.PendingChange
                            .EffectiveWorldTick,
                        SourceClientRequestId = source.PendingChange
                            .SourceClientRequestId,
                    },
                ChangeReceipts = source.ChangeReceipts.Select(value =>
                    new Simulation학습중점ChangeReceiptSnapshot
                    {
                        ClientRequestId = value.ClientRequestId,
                        ExpectedRevision = value.ExpectedRevision,
                        PlayerStableId = value.PlayerStableId,
                        CardStableId = value.CardStableId,
                        EffectiveSegmentStableId = value.EffectiveSegmentStableId,
                        EffectiveWorldTick = value.EffectiveWorldTick,
                        ResultingRevision = value.ResultingRevision,
                    }).ToArray(),
                ActivationHistory = source.ActivationHistory.Select(value =>
                    new Simulation학습중점ActivationSnapshot
                    {
                        CardStableId = value.CardStableId,
                        SegmentStableId = value.SegmentStableId,
                        ActivatedWorldTick = value.ActivatedWorldTick,
                        SourceClientRequestId = value.SourceClientRequestId,
                        ResultingRevision = value.ResultingRevision,
                    }).ToArray(),
                EffectReceipts = source.EffectReceipts.Select(Clone).ToArray(),
                StateHashSha256 = source.StateHashSha256,
                SimulationOnly = source.SimulationOnly,
                IsOperationalState = source.IsOperationalState,
            };

        internal static Simulation학습구간Snapshot Clone(
            Simulation학습구간Snapshot value)
            => new Simulation학습구간Snapshot
            {
                SegmentStableId = value.SegmentStableId,
                SolarTermStableId = value.SolarTermStableId,
                SolarTermRevision = value.SolarTermRevision,
                PhaseCode = value.PhaseCode,
                StartWorldTickInclusive = value.StartWorldTickInclusive,
                EndWorldTickExclusive = value.EndWorldTickExclusive,
            };

        internal static Simulation학습카드DefinitionSnapshot Clone(
            Simulation학습카드DefinitionSnapshot value)
            => new Simulation학습카드DefinitionSnapshot
            {
                CardStableId = value.CardStableId,
                CardRevision = value.CardRevision,
                DefinitionHashSha256 = value.DefinitionHashSha256,
                SourceKindCode = value.SourceKindCode,
                SourceActorStableId = value.SourceActorStableId,
                Title = value.Title,
                PrimaryFiveElementCode = value.PrimaryFiveElementCode,
                SupportingFiveElementCodes = value.SupportingFiveElementCodes
                    .ToArray(),
                Bindings = value.Bindings.Select(binding =>
                    new Simulation학습카드BindingSnapshot
                    {
                        WorldInteractionId = binding.WorldInteractionId,
                        DomainStableId = binding.DomainStableId,
                        SkillStableId = binding.SkillStableId,
                    }).ToArray(),
                UnderstandingDelta = value.UnderstandingDelta,
                EffectRuleRevision = value.EffectRuleRevision,
            };

        internal static Simulation학습효과ReceiptSnapshot Clone(
            Simulation학습효과ReceiptSnapshot value)
            => new Simulation학습효과ReceiptSnapshot
            {
                ReceiptStableId = value.ReceiptStableId,
                CardStableId = value.CardStableId,
                CardRevision = value.CardRevision,
                SourceActorStableId = value.SourceActorStableId,
                SourceActionRecordStableId = value.SourceActionRecordStableId,
                WorldInteractionId = value.WorldInteractionId,
                DomainStableId = value.DomainStableId,
                SkillStableId = value.SkillStableId,
                ResultCode = value.ResultCode,
                UnderstandingDelta = value.UnderstandingDelta,
                AppliedWorldRevision = value.AppliedWorldRevision,
                RuleRevision = value.RuleRevision,
            };
    }
}
