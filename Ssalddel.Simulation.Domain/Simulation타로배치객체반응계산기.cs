using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed class Simulation타로배치객체반응계산기
    {
        public const string ObjectCatalogRevision = "integrated-seedbed:o6.r1";

        public Simulation타로CardObjectReactionSnapshot Calculate(
            Simulation타로CardOfferSnapshot offer,
            IEnumerable<Simulation타로객체상태Snapshot> objectStates)
        {
            if (offer == null) throw new ArgumentNullException(nameof(offer));
            if (objectStates == null) throw new ArgumentNullException(nameof(objectStates));
            if (offer.Card == null
                || offer.Card.CardKindCode != SimulationTurnCardKindCodes.Tarot
                || string.IsNullOrWhiteSpace(offer.OfferStableId)
                || string.IsNullOrWhiteSpace(offer.CardCopyStableId)
                || (offer.OrientationCode != Simulation타로카드방향Codes.Upright
                    && offer.OrientationCode != Simulation타로카드방향Codes.Reversed))
            {
                throw new SimulationContractException("SimulationTarotObjectOfferInvalid");
            }

            var states = objectStates.ToDictionary(
                value => value.ObjectStableId,
                StringComparer.Ordinal);
            var definitions = Definitions().Where(value =>
                value.CardStableId == offer.Card.CardStableId).ToArray();
            if (definitions.Length == 0)
                throw new SimulationContractException("SimulationTarotObjectCardUnsupported");

            var reactions = definitions.Select(definition =>
            {
                if (!states.TryGetValue(definition.ObjectStableId, out var state)
                    || state.PlacementStableId != definition.PlacementStableId)
                {
                    throw new SimulationContractException(
                        "SimulationTarotObjectCatalogStateMismatch");
                }
                return new Simulation타로객체반응Snapshot
                {
                    ObjectStableId = definition.ObjectStableId,
                    PlacementStableId = definition.PlacementStableId,
                    RuleDomainCode = definition.RuleDomainCode,
                    ReactionStateCode = state.HasRelevantState
                        ? Simulation타로객체반응상태Codes.CurrentlyAffected
                        : Simulation타로객체반응상태Codes.StateUnavailable,
                    CanHighlightInWorld = state.HasRelevantState,
                    KoreanSummary = Summary(
                        offer.Card.CardStableId,
                        offer.OrientationCode,
                        definition.KoreanObjectName),
                    StateSourceStableIds = state.StateSourceStableIds.ToArray(),
                    BlockReasonCodes = state.HasRelevantState
                        ? Array.Empty<string>()
                        : new[] { "SimulationTarotRelevantObjectStateUnavailable" },
                };
            }).ToArray();

            return new Simulation타로CardObjectReactionSnapshot
            {
                OfferStableId = offer.OfferStableId,
                CardStableId = offer.Card.CardStableId,
                CardCopyStableId = offer.CardCopyStableId,
                OrientationCode = offer.OrientationCode,
                ObjectReactions = reactions,
                HighlightObjectStableIds = reactions
                    .Where(value => value.CanHighlightInWorld)
                    .Select(value => value.ObjectStableId)
                    .OrderBy(value => value, StringComparer.Ordinal)
                    .ToArray(),
            };
        }

        public static Simulation타로객체상태Snapshot[] CreateEmptyO6ObjectStates()
            => Definitions().Select(value => new Simulation타로객체상태Snapshot
            {
                ObjectStableId = value.ObjectStableId,
                PlacementStableId = value.PlacementStableId,
            }).Distinct(new ObjectStateIdentityComparer()).ToArray();

        private static string Summary(string cardStableId, string orientation, string objectName)
        {
            var direction = orientation == Simulation타로카드방향Codes.Upright
                ? "정방향"
                : "역방향";
            var question = cardStableId == "tarot:major.empress" ? "생산과 돌봄"
                : cardStableId == "tarot:major.chariot" ? "이동 속도와 통제"
                : cardStableId == "tarot:major.justice" ? "검증과 공정한 분배"
                : "자원 흐름의 균형";
            return direction + "의 " + question + "이 " + objectName + "에 미치는 후보 영향";
        }

        private static ObjectReactionDefinition[] Definitions()
            => new[]
            {
                Definition("tarot:major.empress", SimulationO6배치객체StableIds.HarvestBox, SimulationO6배치객체StableIds.HarvestBoxPlacement,
                    Simulation업무규칙영역Codes.Production, "감자 수확 상자"),
                Definition("tarot:major.empress", SimulationO6배치객체StableIds.Market, SimulationO6배치객체StableIds.MarketPlacement,
                    Simulation업무규칙영역Codes.Market, "도심마트 Shop"),
                Definition("tarot:major.chariot", SimulationO6배치객체StableIds.FarmCrate, SimulationO6배치객체StableIds.FarmCratePlacement,
                    Simulation업무규칙영역Codes.Transport, "농장 출하 Pallet Crate"),
                Definition("tarot:major.chariot", SimulationO6배치객체StableIds.DeliveryTruck, SimulationO6배치객체StableIds.DeliveryTruckPlacement,
                    Simulation업무규칙영역Codes.Transport, "화물 배송 차량"),
                Definition("tarot:major.chariot", SimulationO6배치객체StableIds.CargoPallet, SimulationO6배치객체StableIds.CargoPalletPlacement,
                    Simulation업무규칙영역Codes.Transport, "공용 화물 Pallet"),
                Definition("tarot:major.chariot", SimulationO6배치객체StableIds.HubGate, SimulationO6배치객체StableIds.HubGatePlacement,
                    Simulation업무규칙영역Codes.Transport, "Hub 입고 Gate"),
                Definition("tarot:major.justice", SimulationO6배치객체StableIds.GroupCart, SimulationO6배치객체StableIds.GroupCartPlacement,
                    Simulation업무규칙영역Codes.Market, "집단수요 Cart Table"),
                Definition("tarot:major.justice", SimulationO6배치객체StableIds.Market, SimulationO6배치객체StableIds.MarketPlacement,
                    Simulation업무규칙영역Codes.Market, "도심마트 Shop"),
                Definition("tarot:major.justice", SimulationO6배치객체StableIds.FarmCrate, SimulationO6배치객체StableIds.FarmCratePlacement,
                    Simulation업무규칙영역Codes.Transport, "농장 출하 Pallet Crate"),
                Definition("tarot:major.temperance", SimulationO6배치객체StableIds.HarvestBox, SimulationO6배치객체StableIds.HarvestBoxPlacement,
                    Simulation업무규칙영역Codes.Production, "감자 수확 상자"),
                Definition("tarot:major.temperance", SimulationO6배치객체StableIds.CargoPallet, SimulationO6배치객체StableIds.CargoPalletPlacement,
                    Simulation업무규칙영역Codes.Transport, "공용 화물 Pallet"),
                Definition("tarot:major.temperance", SimulationO6배치객체StableIds.Market, SimulationO6배치객체StableIds.MarketPlacement,
                    Simulation업무규칙영역Codes.Market, "도심마트 Shop"),
            };

        private static ObjectReactionDefinition Definition(
            string card,
            string objectId,
            string placement,
            string domain,
            string name)
            => new ObjectReactionDefinition(card, objectId, placement, domain, name);

        private sealed class ObjectReactionDefinition
        {
            public ObjectReactionDefinition(
                string cardStableId,
                string objectStableId,
                string placementStableId,
                string ruleDomainCode,
                string koreanObjectName)
            {
                CardStableId = cardStableId;
                ObjectStableId = objectStableId;
                PlacementStableId = placementStableId;
                RuleDomainCode = ruleDomainCode;
                KoreanObjectName = koreanObjectName;
            }

            public string CardStableId { get; }
            public string ObjectStableId { get; }
            public string PlacementStableId { get; }
            public string RuleDomainCode { get; }
            public string KoreanObjectName { get; }
        }

        private sealed class ObjectStateIdentityComparer
            : IEqualityComparer<Simulation타로객체상태Snapshot>
        {
            public bool Equals(
                Simulation타로객체상태Snapshot? x,
                Simulation타로객체상태Snapshot? y)
                => x?.ObjectStableId == y?.ObjectStableId;

            public int GetHashCode(Simulation타로객체상태Snapshot value)
                => StringComparer.Ordinal.GetHashCode(value.ObjectStableId);
        }
    }
}
