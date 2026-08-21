using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private const string FoolCardStableId =
            SimulationTarotJourneyRootCodes.FoolCardStableId;
        private const string ChariotCardStableId = "learning:hongik.chariot.integrated-progress";
        internal const string TarotChariotCardStableId = "tarot:major.chariot";
        internal const string SeoulLivingCultureCardStableId =
            "culture:kr-seoul.living-culture-question.2026";
        private readonly List<SimulationTurnClosingSnapshot> turnClosings =
            new List<SimulationTurnClosingSnapshot>();
        private readonly Dictionary<string, 적용된TurnClosingCommand> appliedTurnClosingCommands =
            new Dictionary<string, 적용된TurnClosingCommand>(StringComparer.Ordinal);
        private SimulationActiveTurnCardEffectSnapshot[] activeTurnCardEffects =
            Array.Empty<SimulationActiveTurnCardEffectSnapshot>();

        public SimulationTurnClosingContextSnapshot GetTurnClosingContext()
        {
            lock (gate)
            {
                return new SimulationTurnClosingContextSnapshot
                {
                    SessionStableId = SessionStableId,
                    TurnNumber = CurrentTick + 1,
                    GameDate = GameDateStartsOn.AddDays(CurrentTick),
                    Revision = Revision,
                    PendingTaskCount = CountPendingTasks(),
                    CanCloseTurn = CurrentTick < DurationTicks,
                    BlockReasonCodes = CurrentTick < DurationTicks
                        ? Array.Empty<string>()
                        : new[] { "SimulationDurationCompleted" },
                    AvailableCards = CreateAvailableTurnCards(),
                    TarotDraw = CreateTarotDraw(),
                    TarotContext = CreateTarotContextSnapshot(),
                };
            }
        }

        public SimulationTurnClosingPreviewSnapshot PreviewTurnClosing(
            SimulationTurnClosingPreviewRequest request)
        {
            ValidateTurnClosingPreviewRequest(request);
            lock (gate)
            {
                if (request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
                return CreateTurnClosingPreview(request);
            }
        }

        public 경영SimulationSessionSnapshot ConfirmTurnClosing(
            SimulationTurnClosingConfirmRequest request)
        {
            ValidateTurnClosingConfirmRequest(request);
            lock (gate)
            {
                var commandId = request.CommandId.Trim();
                var payloadKey = BuildTurnClosingPayloadKey(request.Preview);
                if (appliedTurnClosingCommands.TryGetValue(commandId, out var applied))
                {
                    if (!string.Equals(applied.PayloadKey, payloadKey, StringComparison.Ordinal))
                        throw new SimulationConflictException("SimulationCommandPayloadConflict");
                    return Clone(applied.Snapshot);
                }
                if (appliedCommands.ContainsKey(commandId) || HasAppliedDecisionCommand(commandId))
                    throw new SimulationConflictException("SimulationCommandKindConflict");
                if (request.ExpectedRevision != Revision
                    || request.Preview.ExpectedRevision != Revision)
                {
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
                }

                var preview = CreateTurnClosingPreview(request.Preview);
                if (preview.BlockReasonCodes.Length > 0)
                    throw new SimulationConflictException("SimulationTurnClosingBlocked");

                CurrentTick++;
                AdvanceDecisionWork(CurrentTick);
                Revision++;

                var closing = new SimulationTurnClosingSnapshot
                {
                    TurnClosingStableId = preview.PreviewStableId,
                    ClosedTurnNumber = preview.ClosingTurnNumber,
                    ClosedGameDate = preview.ClosingGameDate,
                    ResultingWorldTick = CurrentTick,
                    ResultingRevision = Revision,
                    SelectedCards = preview.SelectedCards.Select(CloneTurnCard).ToArray(),
                };
                turnClosings.Add(closing);
                activeTurnCardEffects = preview.SelectedCards.Select(card =>
                    new SimulationActiveTurnCardEffectSnapshot
                    {
                        CardStableId = card.CardStableId,
                        CardRevision = card.CardRevision,
                        CardKindCode = card.CardKindCode,
                        CardCopyStableId = card.CardCopyStableId,
                        OfferStableId = card.OfferStableId,
                        OrientationCode = card.OrientationCode,
                        EffectCode = card.EffectCode,
                        TargetStatCode = card.TargetStatCode,
                        StatDelta = card.StatDelta,
                        ActiveTurnNumber = CurrentTick + 1,
                        SourceTurnClosingStableId = closing.TurnClosingStableId,
                        SourceStableId = card.SourceStableId,
                        RegionKey = card.RegionKey,
                        CalendarRevision = card.CalendarRevision,
                        EffectRuleRevision = card.EffectRuleRevision,
                        SourceUrl = card.SourceUrl,
                        EvidenceCheckedAtUtc = card.EvidenceCheckedAtUtc,
                    }).ToArray();
                ApplyTarotContext(closing);
                ObserveRegionalCausalityTurnCards(closing);
                RebuildNatureThreat(CurrentTick);

                AppendTurnClosingCommand(request);
                var snapshot = CreateSnapshot();
                appliedTurnClosingCommands.Add(
                    commandId,
                    new 적용된TurnClosingCommand(payloadKey, Clone(snapshot)));
                return snapshot;
            }
        }

        internal static void ValidateTurnClosingPreviewRequest(
            SimulationTurnClosingPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            if (request.SelectedCardStableIds == null)
                throw new SimulationContractException("SimulationTurnCardSelectionInvalid");
            if (request.SelectedCardStableIds.Length > 1)
                throw new SimulationContractException("SimulationTurnCardSelectionLimitExceeded");
            if (request.SelectedCardStableIds.Length > 0
                && request.SelectedTarotCard != null)
            {
                throw new SimulationContractException("SimulationTurnCardSelectionLimitExceeded");
            }
            foreach (var cardStableId in request.SelectedCardStableIds)
                RequireStableId(cardStableId, "SimulationTurnCardStableIdInvalid");
            if (request.SelectedCardStableIds.Distinct(StringComparer.Ordinal).Count()
                != request.SelectedCardStableIds.Length)
            {
                throw new SimulationContractException("SimulationTurnCardSelectionDuplicate");
            }
            if (request.SelectedTarotCard != null)
            {
                RequireStableId(request.SelectedTarotCard.OfferStableId,
                    "SimulationTarotOfferStableIdInvalid");
                RequireStableId(request.SelectedTarotCard.CardStableId,
                    "SimulationTurnCardStableIdInvalid");
                if (request.SelectedTarotCard.OrientationCode
                        != Simulation타로카드방향Codes.Upright
                    && request.SelectedTarotCard.OrientationCode
                        != Simulation타로카드방향Codes.Reversed)
                {
                    throw new SimulationContractException(
                        "SimulationTarotOrientationInvalid");
                }
            }
        }

        internal static void ValidateTurnClosingConfirmRequest(
            SimulationTurnClosingConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            ValidateTurnClosingPreviewRequest(request.Preview);
        }

        internal static string BuildTurnClosingPayloadKey(
            SimulationTurnClosingPreviewRequest request)
        {
            ValidateTurnClosingPreviewRequest(request);
            var tarot = request.SelectedTarotCard;
            return request.ExpectedRevision + "|"
                + string.Join(",", request.SelectedCardStableIds.Select(value => value.Trim()))
                + "|" + (tarot?.OfferStableId.Trim() ?? string.Empty)
                + "|" + (tarot?.CardStableId.Trim() ?? string.Empty)
                + "|" + (tarot?.OrientationCode ?? string.Empty);
        }

        private SimulationTurnClosingPreviewSnapshot CreateTurnClosingPreview(
            SimulationTurnClosingPreviewRequest request)
        {
            var available = CreateAvailableTurnCards()
                .ToDictionary(value => value.CardStableId, StringComparer.Ordinal);
            var selected = new List<SimulationTurnCardSnapshot>();
            foreach (var stableId in request.SelectedCardStableIds)
            {
                if (!available.TryGetValue(stableId.Trim(), out var card))
                    throw new SimulationConflictException("SimulationTurnCardUnavailable");
                selected.Add(CloneTurnCard(card));
            }
            if (request.SelectedTarotCard != null)
            {
                var selection = request.SelectedTarotCard;
                var offer = CreateTarotDraw().Offers.SingleOrDefault(value =>
                    value.OfferStableId == selection.OfferStableId.Trim());
                if (offer == null)
                    throw new SimulationConflictException("SimulationTarotOfferUnavailable");
                if (offer.Card.CardStableId != selection.CardStableId.Trim()
                    || offer.OrientationCode != selection.OrientationCode)
                {
                    throw new SimulationConflictException("SimulationTarotOfferMismatch");
                }
                var card = CloneTurnCard(offer.Card);
                card.CardCopyStableId = offer.CardCopyStableId;
                card.OfferStableId = offer.OfferStableId;
                card.OrientationCode = offer.OrientationCode;
                selected.Add(card);
            }

            var closingTurnNumber = CurrentTick + 1;
            return new SimulationTurnClosingPreviewSnapshot
            {
                PreviewStableId = "turn-closing:" + SessionStableId + ":" + closingTurnNumber,
                BaseRevision = Revision,
                ClosingTurnNumber = closingTurnNumber,
                ClosingGameDate = GameDateStartsOn.AddDays(CurrentTick),
                NextTurnNumber = closingTurnNumber + 1,
                NextGameDate = GameDateStartsOn.AddDays(CurrentTick + 1),
                PendingTaskCount = CountPendingTasks(),
                SelectedCards = selected.ToArray(),
                BlockReasonCodes = CurrentTick < DurationTicks
                    ? Array.Empty<string>()
                    : new[] { "SimulationDurationCompleted" },
            };
        }

        private int CountPendingTasks()
            => tasks.Values.Count(value =>
                value.StateCode != SimulationTaskStateCodes.Completed
                && value.StateCode != SimulationTaskStateCodes.Cancelled);

        private SimulationTurnCardSnapshot[] CreateAvailableTurnCards()
        {
            var gameDate = GameDateStartsOn.AddDays(CurrentTick);
            var cards = new[]
            {
                new SimulationTurnCardSnapshot
                {
                    CardStableId = FoolCardStableId,
                    CardRevision = "evening-hakdang.fixture-r1",
                    CardKindCode = SimulationTurnCardKindCodes.Philosophy,
                    Title = "0. 바보 · 모를 뿐",
                    Summary = "모름을 인정하고 다음 경영일을 초심으로 바라본다.",
                    EffectTimingCode = SimulationTurnCardEffectTimingCodes.NextTurn,
                    EffectCode = SimulationTurnCardEffectCodes.BeginnerMind,
                    TargetStatCode = "Awareness",
                    StatDelta = 1,
                    SourceStableId = "source:hongik-hakdang.fool.beginner-mind",
                },
                new SimulationTurnCardSnapshot
                {
                    CardStableId = ChariotCardStableId,
                    CardRevision = "evening-hakdang.fixture-r1",
                    CardKindCode = SimulationTurnCardKindCodes.Philosophy,
                    Title = "7. 전차 · 통합된 정진",
                    Summary = "힘과 지혜를 함께 써서 다음 경영일의 실천을 잇는다.",
                    EffectTimingCode = SimulationTurnCardEffectTimingCodes.NextTurn,
                    EffectCode = SimulationTurnCardEffectCodes.IntegratedProgress,
                    TargetStatCode = "Resolve",
                    StatDelta = 1,
                    SourceStableId = "source:hongik-hakdang.chariot.integrated-progress",
                },
                new SimulationTurnCardSnapshot
                {
                    CardStableId = SeoulLivingCultureCardStableId,
                    CardRevision = "culture-card.fixture-r1",
                    CardKindCode = SimulationTurnCardKindCodes.Culture,
                    Title = "서울 생활문화 질문",
                    Summary = "지역 생활을 하나의 대표 이미지로 단정하지 않고 주민의 현재 경험과 공식 지역문화 원천을 함께 확인한다.",
                    EffectTimingCode = SimulationTurnCardEffectTimingCodes.NextTurn,
                    EffectCode = SimulationTurnCardEffectCodes.LocalContextAwareness,
                    TargetStatCode = "CommunityInsight",
                    StatDelta = 1,
                    SourceStableId = "source:kr-regional-culture-promotion-agency",
                    RegionKey = "kr-seoul",
                    AvailableFromGameDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                    AvailableThroughGameDate = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero),
                    CalendarRevision = "simulation-culture-calendar:kr-seoul:2026.r1",
                    EffectRuleRevision = "culture-local-context-awareness:r1",
                    SourceUrl = "https://www.mcst.go.kr/site/s_data/corpNaru/corpView.jsp?pSeq=615",
                    EvidenceCheckedAtUtc = new DateTimeOffset(2026, 7, 26, 0, 0, 0, TimeSpan.Zero),
                },
            };

            foreach (var card in cards.Where(value =>
                         value.CardKindCode == SimulationTurnCardKindCodes.Culture))
                ValidateCultureCard(card);

            return cards.Where(card =>
                    card.CardKindCode != SimulationTurnCardKindCodes.Culture
                    || (card.AvailableFromGameDate <= gameDate
                        && card.AvailableThroughGameDate >= gameDate))
                .ToArray();
        }

        private Simulation타로DrawSnapshot CreateTarotDraw()
            => new Simulation타로카드뽑기().Draw(
                ScenarioSeed,
                CurrentTick + 1,
                turnClosings.SelectMany(value => value.SelectedCards)
                    .Where(value => value.CardKindCode == SimulationTurnCardKindCodes.Tarot)
                    .Select(value => value.OfferStableId));

        public static void ValidateCultureCard(SimulationTurnCardSnapshot card)
        {
            if (card == null
                || card.CardKindCode != SimulationTurnCardKindCodes.Culture
                || string.IsNullOrWhiteSpace(card.RegionKey)
                || !card.AvailableFromGameDate.HasValue
                || !card.AvailableThroughGameDate.HasValue
                || card.AvailableFromGameDate > card.AvailableThroughGameDate
                || string.IsNullOrWhiteSpace(card.CalendarRevision)
                || string.IsNullOrWhiteSpace(card.EffectRuleRevision)
                || string.IsNullOrWhiteSpace(card.SourceStableId)
                || !Uri.TryCreate(card.SourceUrl, UriKind.Absolute, out var sourceUrl)
                || sourceUrl.Scheme != Uri.UriSchemeHttps
                || !card.EvidenceCheckedAtUtc.HasValue)
            {
                throw new SimulationContractException("SimulationCultureTurnCardProvenanceInvalid");
            }
        }

        private SimulationTurnClosingSnapshot[] CreateTurnClosingSnapshots()
            => turnClosings.Select(CloneTurnClosing).ToArray();

        private SimulationActiveTurnCardEffectSnapshot[] CreateActiveTurnCardEffectSnapshots()
            => activeTurnCardEffects.Select(CloneActiveTurnCardEffect).ToArray();

        private void ExpireActiveTurnCardEffects()
            => activeTurnCardEffects = Array.Empty<SimulationActiveTurnCardEffectSnapshot>();

        internal static SimulationTurnCardSnapshot CloneTurnCard(SimulationTurnCardSnapshot source)
            => new SimulationTurnCardSnapshot
            {
                CardStableId = source.CardStableId,
                CardRevision = source.CardRevision,
                CardKindCode = source.CardKindCode,
                CardCopyStableId = source.CardCopyStableId,
                OfferStableId = source.OfferStableId,
                OrientationCode = source.OrientationCode,
                Title = source.Title,
                Summary = source.Summary,
                EffectTimingCode = source.EffectTimingCode,
                EffectCode = source.EffectCode,
                TargetStatCode = source.TargetStatCode,
                StatDelta = source.StatDelta,
                SourceStableId = source.SourceStableId,
                RegionKey = source.RegionKey,
                AvailableFromGameDate = source.AvailableFromGameDate,
                AvailableThroughGameDate = source.AvailableThroughGameDate,
                CalendarRevision = source.CalendarRevision,
                EffectRuleRevision = source.EffectRuleRevision,
                SourceUrl = source.SourceUrl,
                EvidenceCheckedAtUtc = source.EvidenceCheckedAtUtc,
            };

        internal static SimulationTurnClosingSnapshot CloneTurnClosing(
            SimulationTurnClosingSnapshot source)
            => new SimulationTurnClosingSnapshot
            {
                TurnClosingStableId = source.TurnClosingStableId,
                ClosedTurnNumber = source.ClosedTurnNumber,
                ClosedGameDate = source.ClosedGameDate,
                ResultingWorldTick = source.ResultingWorldTick,
                ResultingRevision = source.ResultingRevision,
                SelectedCards = source.SelectedCards.Select(CloneTurnCard).ToArray(),
            };

        internal static SimulationActiveTurnCardEffectSnapshot CloneActiveTurnCardEffect(
            SimulationActiveTurnCardEffectSnapshot source)
            => new SimulationActiveTurnCardEffectSnapshot
            {
                CardStableId = source.CardStableId,
                CardRevision = source.CardRevision,
                CardKindCode = source.CardKindCode,
                CardCopyStableId = source.CardCopyStableId,
                OfferStableId = source.OfferStableId,
                OrientationCode = source.OrientationCode,
                EffectCode = source.EffectCode,
                TargetStatCode = source.TargetStatCode,
                StatDelta = source.StatDelta,
                ActiveTurnNumber = source.ActiveTurnNumber,
                SourceTurnClosingStableId = source.SourceTurnClosingStableId,
                SourceStableId = source.SourceStableId,
                RegionKey = source.RegionKey,
                CalendarRevision = source.CalendarRevision,
                EffectRuleRevision = source.EffectRuleRevision,
                SourceUrl = source.SourceUrl,
                EvidenceCheckedAtUtc = source.EvidenceCheckedAtUtc,
            };

        private sealed class 적용된TurnClosingCommand
        {
            public 적용된TurnClosingCommand(
                string payloadKey,
                경영SimulationSessionSnapshot snapshot)
            {
                PayloadKey = payloadKey;
                Snapshot = snapshot;
            }

            public string PayloadKey { get; }
            public 경영SimulationSessionSnapshot Snapshot { get; }
        }
    }
}
