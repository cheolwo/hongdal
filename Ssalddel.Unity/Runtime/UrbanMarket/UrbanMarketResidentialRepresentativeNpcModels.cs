using System;
using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.WorldProjection;

namespace Ssalddel.Unity.UrbanMarket
{
    public static class ResidentialGroupRepresentativeNpcCodes
    {
        public const string ActorRole = "ResidentialGroupRepresentative";
        public const string ResidentialRoute = "residential-group-representative-briefing";
        public const string MarketRoute = "market-group-representative-consultation";
    }

    public static class ResidentialGroupRepresentativeVisitLegCodes
    {
        public const string ResidentialBriefing = "ResidentialBriefing";
        public const string MarketConsultation = "MarketConsultation";
    }

    public static class ResidentialGroupRepresentativeJourneyStageCodes
    {
        public const string ReviewingGroupDemand = "ReviewingGroupDemand";
        public const string WaitingForManagerReview = "WaitingForManagerReview";
        public const string RelayingOffer = "RelayingOffer";
        public const string CoordinatingPickup = "CoordinatingPickup";
    }

    public static class ResidentialGroupRepresentativeArrivalActionCodes
    {
        public const string ReviewDemandBoard = "ReviewDemandBoard";
        public const string WaitForManagerReview = "WaitForManagerReview";
        public const string RelayOffer = "RelayOffer";
        public const string CoordinatePickup = "CoordinatePickup";
    }

    public static class RepresentativeVisitCommandEffectCodes
    {
        public const string None = "None";
    }

    public sealed class ResidentialGroupRepresentativeVisitSnapshot
    {
        public string RepresentativeVisitStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string OrdererGroupStableId { get; set; } = string.Empty;
        public string InquiryStableId { get; set; } = string.Empty;
        public string NpcStableId { get; set; } = string.Empty;
        public string JourneyStageCode { get; set; } = string.Empty;
        public string ActiveLegCode { get; set; } = string.Empty;
        public string CommandEffectCode { get; set; } = RepresentativeVisitCommandEffectCodes.None;
        public NpcMovementSnapshot ResidentialBriefingLeg { get; set; } = new NpcMovementSnapshot();
        public NpcMovementSnapshot MarketConsultationLeg { get; set; } = new NpcMovementSnapshot();

        public NpcMovementSnapshot ActiveMovement()
        {
            if (ActiveLegCode == ResidentialGroupRepresentativeVisitLegCodes.ResidentialBriefing)
                return ResidentialBriefingLeg;
            if (ActiveLegCode == ResidentialGroupRepresentativeVisitLegCodes.MarketConsultation)
                return MarketConsultationLeg;
            throw new InvalidOperationException("RepresentativeVisitActiveLegInvalid");
        }
    }

    public sealed class ResidentialGroupRepresentativeVisitValidator
    {
        public void Validate(ResidentialGroupRepresentativeVisitSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            Require(snapshot.RepresentativeVisitStableId, "RepresentativeVisitStableIdInvalid");
            Require(snapshot.OrdererGroupStableId, "RepresentativeVisitGroupStableIdInvalid");
            Require(snapshot.InquiryStableId, "RepresentativeVisitInquiryStableIdInvalid");
            Require(snapshot.NpcStableId, "RepresentativeVisitNpcStableIdInvalid");
            if (snapshot.Revision < 0) throw new InvalidOperationException("RepresentativeVisitRevisionInvalid");
            if (snapshot.CommandEffectCode != RepresentativeVisitCommandEffectCodes.None)
                throw new InvalidOperationException("RepresentativeVisitCommandEffectForbidden");
            if (snapshot.ResidentialBriefingLeg == null || snapshot.MarketConsultationLeg == null)
                throw new InvalidOperationException("RepresentativeVisitMovementLegMissing");
            ValidateLeg(snapshot.ResidentialBriefingLeg, snapshot.NpcStableId,
                WorldZoneCodes.ResidentialCommunity,
                ResidentialGroupRepresentativeNpcCodes.ResidentialRoute);
            ValidateLeg(snapshot.MarketConsultationLeg, snapshot.NpcStableId,
                WorldZoneCodes.MarketOrder,
                ResidentialGroupRepresentativeNpcCodes.MarketRoute);
            var active = snapshot.ActiveMovement();
            if (snapshot.JourneyStageCode == ResidentialGroupRepresentativeJourneyStageCodes.ReviewingGroupDemand
                || snapshot.JourneyStageCode == ResidentialGroupRepresentativeJourneyStageCodes.RelayingOffer
                || snapshot.JourneyStageCode == ResidentialGroupRepresentativeJourneyStageCodes.CoordinatingPickup)
            {
                if (!ReferenceEquals(active, snapshot.ResidentialBriefingLeg))
                    throw new InvalidOperationException("RepresentativeVisitJourneyLegMismatch");
            }
            else if (snapshot.JourneyStageCode
                == ResidentialGroupRepresentativeJourneyStageCodes.WaitingForManagerReview)
            {
                if (!ReferenceEquals(active, snapshot.MarketConsultationLeg))
                    throw new InvalidOperationException("RepresentativeVisitJourneyLegMismatch");
            }
            else
            {
                throw new InvalidOperationException("RepresentativeVisitJourneyStageInvalid");
            }
        }

        private static void ValidateLeg(
            NpcMovementSnapshot leg,
            string npcStableId,
            string zoneCode,
            string routeCode)
        {
            if (leg.NpcStableId != npcStableId
                || leg.ActorRoleCode != ResidentialGroupRepresentativeNpcCodes.ActorRole
                || leg.WorldZoneCode != zoneCode
                || leg.RouteCode != routeCode)
                throw new InvalidOperationException("RepresentativeVisitMovementLegMismatch");
            if (leg.SourceTypeCode != NpcMovementSourceTypeCodes.SimulatedFixture
                || !string.IsNullOrWhiteSpace(leg.CanonicalTaskStableId))
                throw new InvalidOperationException("RepresentativeVisitSimulationBoundaryInvalid");
        }

        private static void Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(error);
        }
    }

    public static class ResidentialGroupRepresentativeVisitFixture
    {
        private static readonly DateTimeOffset GeneratedAt =
            DateTimeOffset.Parse("2026-08-09T04:00:00Z");

        public static ResidentialGroupRepresentativeVisitSnapshot Create()
        {
            const string npc = "npc:sim:residential-group-representative:1";
            var mapper = new NpcMovementMapper();
            var snapshot = new ResidentialGroupRepresentativeVisitSnapshot
            {
                RepresentativeVisitStableId = "representative-visit:sim:potato:1",
                Revision = 1,
                OrdererGroupStableId = "orderer-group:residential:potato:1",
                InquiryStableId = "market-inquiry:sim:potato:1",
                NpcStableId = npc,
                JourneyStageCode =
                    ResidentialGroupRepresentativeJourneyStageCodes.WaitingForManagerReview,
                ActiveLegCode = ResidentialGroupRepresentativeVisitLegCodes.MarketConsultation,
                CommandEffectCode = RepresentativeVisitCommandEffectCodes.None,
                ResidentialBriefingLeg = mapper.Map(Movement(
                    "npc-movement:representative:residential:1", npc,
                    WorldZoneCodes.ResidentialCommunity,
                    ResidentialGroupRepresentativeNpcCodes.ResidentialRoute,
                    "residential.community-board", "residential.departure-point",
                    ResidentialGroupRepresentativeArrivalActionCodes.ReviewDemandBoard)),
                MarketConsultationLeg = mapper.Map(Movement(
                    "npc-movement:representative:market:1", npc,
                    WorldZoneCodes.MarketOrder,
                    ResidentialGroupRepresentativeNpcCodes.MarketRoute,
                    "market.entrance", "market.manager-desk",
                    ResidentialGroupRepresentativeArrivalActionCodes.WaitForManagerReview)),
            };
            new ResidentialGroupRepresentativeVisitValidator().Validate(snapshot);
            return snapshot;
        }

        private static NpcMovementApiModel Movement(
            string stableId,
            string npcStableId,
            string zoneCode,
            string routeCode,
            string currentWaypoint,
            string destinationWaypoint,
            string arrivalAction)
            => new NpcMovementApiModel
            {
                StableId = stableId,
                Revision = 1,
                NpcStableId = npcStableId,
                ActorRoleCode = ResidentialGroupRepresentativeNpcCodes.ActorRole,
                WorldZoneCode = zoneCode,
                RouteCode = routeCode,
                CurrentWaypointKey = currentWaypoint,
                DestinationWaypointKey = destinationWaypoint,
                MovementStateCode = NpcMovementStateCodes.Moving,
                ArrivalActionCode = arrivalAction,
                SourceTypeCode = NpcMovementSourceTypeCodes.SimulatedFixture,
                CanonicalTaskStableId = string.Empty,
                GeneratedAt = GeneratedAt,
            };
    }
}
