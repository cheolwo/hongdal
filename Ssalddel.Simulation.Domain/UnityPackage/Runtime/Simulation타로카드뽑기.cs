using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class Simulation타로카드뽑기
    {
        public Simulation타로DeckCardCopySnapshot[] CreateStarterDeck()
        {
            var cards = CreateCardCatalog();
            return cards.SelectMany(card => Enumerable.Range(1, 3).Select(copyNumber =>
                new Simulation타로DeckCardCopySnapshot
                {
                    CardCopyStableId = card.CardStableId + ":copy-"
                        + copyNumber.ToString(CultureInfo.InvariantCulture),
                    CopyNumber = copyNumber,
                    Card = CloneCard(card),
                })).ToArray();
        }

        public Simulation타로DrawSnapshot Draw(
            int scenarioSeed,
            int turnNumber,
            IEnumerable<string> previousSelectionStableIds)
        {
            if (turnNumber <= 0)
                throw new SimulationContractException("SimulationTarotDrawTurnInvalid");
            if (previousSelectionStableIds == null)
                throw new ArgumentNullException(nameof(previousSelectionStableIds));
            var history = previousSelectionStableIds.ToArray();
            if (history.Any(string.IsNullOrWhiteSpace))
                throw new SimulationContractException("SimulationTarotDrawHistoryInvalid");

            var historyHash = StableHashHex(string.Join("|", history));
            var drawStableId = "tarot-draw:turn-"
                + turnNumber.ToString(CultureInfo.InvariantCulture) + ":"
                + StableHashHex(scenarioSeed.ToString(CultureInfo.InvariantCulture)
                    + "|" + Simulation타로DeckCodes.StarterDeckRevision
                    + "|" + turnNumber.ToString(CultureInfo.InvariantCulture)
                    + "|" + historyHash);
            var selected = CreateStarterDeck()
                .Select(copy => new
                {
                    Copy = copy,
                    Score = StableHash(scenarioSeed.ToString(CultureInfo.InvariantCulture)
                        + "|" + Simulation타로DeckCodes.StarterDeckRevision
                        + "|" + turnNumber.ToString(CultureInfo.InvariantCulture)
                        + "|" + historyHash + "|" + copy.CardCopyStableId),
                })
                .OrderBy(value => value.Score)
                .ThenBy(value => value.Copy.CardCopyStableId, StringComparer.Ordinal)
                .Take(3)
                .ToArray();

            return new Simulation타로DrawSnapshot
            {
                DrawStableId = drawStableId,
                DeckStableId = Simulation타로DeckCodes.StarterDeckStableId,
                DeckRevision = Simulation타로DeckCodes.StarterDeckRevision,
                DrawRuleRevision = Simulation타로DeckCodes.DrawRuleRevision,
                TurnNumber = turnNumber,
                TurnHistoryHash = historyHash,
                Offers = selected.Select((value, index) =>
                {
                    var slot = index + 1;
                    var orientation = (StableHash(drawStableId + "|orientation|"
                        + value.Copy.CardCopyStableId) & 1UL) == 0UL
                        ? Simulation타로카드방향Codes.Upright
                        : Simulation타로카드방향Codes.Reversed;
                    return new Simulation타로CardOfferSnapshot
                    {
                        OfferStableId = drawStableId + ":offer-"
                            + slot.ToString(CultureInfo.InvariantCulture),
                        OfferSlotNumber = slot,
                        CardCopyStableId = value.Copy.CardCopyStableId,
                        OrientationCode = orientation,
                        Card = CloneCard(value.Copy.Card),
                    };
                }).ToArray(),
            };
        }

        private static SimulationTurnCardSnapshot[] CreateCardCatalog()
            => new[]
            {
                Card("tarot:major.empress", "III. 여제", "생산과 돌봄이 풍요를 키우지만 자원 부담도 늘어난다.",
                    SimulationTurnCardEffectCodes.EmpressProductionGrowth, "Production"),
                Card("tarot:major.chariot", "VII. 전차", "전진 속도를 높이되 연료·노동·위험을 함께 감당한다.",
                    SimulationTurnCardEffectCodes.ChariotFastTransport, "Transport"),
                Card("tarot:major.justice", "XI. 정의", "거래와 분배의 기준을 명확히 하며 불균형을 드러낸다.",
                    SimulationTurnCardEffectCodes.JusticeTradeBalance, "Trade"),
                Card("tarot:major.temperance", "XIV. 절제", "생산·운송·재고의 흐름을 조율해 낭비를 줄인다.",
                    SimulationTurnCardEffectCodes.TemperanceFlowBalance, "Flow"),
            };

        private static SimulationTurnCardSnapshot Card(
            string stableId,
            string title,
            string summary,
            string effectCode,
            string targetStatCode)
            => new SimulationTurnCardSnapshot
            {
                CardStableId = stableId,
                CardRevision = "tarot-card:r1",
                CardKindCode = SimulationTurnCardKindCodes.Tarot,
                Title = title,
                Summary = summary,
                EffectTimingCode = SimulationTurnCardEffectTimingCodes.NextTurn,
                EffectCode = effectCode,
                TargetStatCode = targetStatCode,
                SourceStableId = "source:tarot-rider-waite-smith.interpretation-r1",
                EffectRuleRevision = "tarot-game-interpretation:r1",
            };

        private static SimulationTurnCardSnapshot CloneCard(SimulationTurnCardSnapshot source)
            => new SimulationTurnCardSnapshot
            {
                CardStableId = source.CardStableId,
                CardRevision = source.CardRevision,
                CardKindCode = source.CardKindCode,
                Title = source.Title,
                Summary = source.Summary,
                EffectTimingCode = source.EffectTimingCode,
                EffectCode = source.EffectCode,
                TargetStatCode = source.TargetStatCode,
                StatDelta = source.StatDelta,
                SourceStableId = source.SourceStableId,
                EffectRuleRevision = source.EffectRuleRevision,
            };

        private static string StableHashHex(string value)
            => StableHash(value).ToString("x16", CultureInfo.InvariantCulture);

        private static ulong StableHash(string value)
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            var hash = offset;
            foreach (var item in Encoding.UTF8.GetBytes(value))
            {
                hash ^= item;
                hash *= prime;
            }
            hash ^= hash >> 33;
            hash *= 0xff51afd7ed558ccdUL;
            hash ^= hash >> 33;
            hash *= 0xc4ceb9fe1a85ec53UL;
            hash ^= hash >> 33;
            return hash;
        }
    }
}
