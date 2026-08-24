using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using static Ssalddel.Simulation.Domain.도심마트주문자집단PerspectiveSupport;

namespace Ssalddel.Simulation.Domain
{
    public static class 도심마트주문자집단PerspectiveActionCodes
    {
        public const string ReviewOwnParticipation = "ReviewOwnParticipation";
        public const string WithdrawOwnParticipation = "WithdrawOwnParticipation";
        public const string ReviewAggregateDemand = "ReviewAggregateDemand";
        public const string PrepareMarketInquiry = "PrepareMarketInquiry";
        public const string ReviewInquiryStatus = "ReviewInquiryStatus";
        public const string RelayMarketOffer = "RelayMarketOffer";
        public const string CoordinatePickup = "CoordinatePickup";
        public const string ReviewOrdererGroupDemand = "ReviewOrdererGroupDemand";
        public const string PreviewSupplyPlan = "PreviewSupplyPlan";
    }

    public static class 도심마트주문자집단ManagerQueueCodes
    {
        public const string ConfirmedDemandSupplyReviewRequired =
            "ConfirmedDemandSupplyReviewRequired";
        public const string ResidentConfirmationPending = "ResidentConfirmationPending";
        public const string NoActionNeeded = "NoActionNeeded";
    }

    public static class 도심마트주문자집단ManagerReasonCodes
    {
        public const string GroupConfirmedDemandPresent = "GroupConfirmedDemandPresent";
        public const string MemberConfirmationPending = "MemberConfirmationPending";
        public const string RepresentativeInquiryPending = "RepresentativeInquiryPending";
    }

    public static class 도심마트DialogueCommandEffectCodes
    {
        public const string None = "None";
    }

    public sealed class 도심마트주문자집단DialogueWorldState
    {
        public string DialogueStableId { get; set; } = string.Empty;
        public string InquiryStableId { get; set; } = string.Empty;
        public string TopicCode { get; set; } = string.Empty;
        public bool CanOpen { get; set; }
        public string CommandEffectCode { get; set; } = 도심마트DialogueCommandEffectCodes.None;
        public string DialogueRevision { get; set; } = string.Empty;
    }

    public sealed class 도심마트주민주문자집단PerspectiveWorldState
    {
        public bool IsAuthorized { get; set; }
        public string PerspectiveRevision { get; set; } = string.Empty;
        public string OrdererGroupStableId { get; set; } = string.Empty;
        public string ParticipantStableId { get; set; } = string.Empty;
        public string ParticipationStateCode { get; set; } = string.Empty;
        public decimal MyIntentQuantity { get; set; }
        public decimal MyConfirmedQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public int RequestedFulfillmentStartsAtTick { get; set; }
        public int RequestedFulfillmentEndsAtTick { get; set; }
        public string PickupPointStableId { get; set; } = string.Empty;
        public string[] AvailableActionCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class 도심마트대표주문자집단PerspectiveWorldState
    {
        public bool IsAuthorized { get; set; }
        public string PerspectiveRevision { get; set; } = string.Empty;
        public string OrdererGroupStableId { get; set; } = string.Empty;
        public string RepresentativeStableId { get; set; } = string.Empty;
        public string SocialContextCode { get; set; } = string.Empty;
        public string CanonicalRoleCode { get; set; } = string.Empty;
        public int IntentParticipantCount { get; set; }
        public decimal IntentQuantity { get; set; }
        public int ConfirmedParticipantCount { get; set; }
        public decimal ConfirmedQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public string InquiryStateCode { get; set; } = string.Empty;
        public string PickupPointStableId { get; set; } = string.Empty;
        public string PickupPointStateCode { get; set; } = string.Empty;
        public string[] AvailableActionCodes { get; set; } = Array.Empty<string>();
        public 도심마트주문자집단DialogueWorldState Dialogue { get; set; } =
            new 도심마트주문자집단DialogueWorldState();
    }

    public sealed class 마트관리자주문자집단PerspectiveWorldState
    {
        public bool IsAuthorized { get; set; }
        public string PerspectiveRevision { get; set; } = string.Empty;
        public string OrdererGroupStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string GroupContextCode { get; set; } = string.Empty;
        public string GroupStateCode { get; set; } = string.Empty;
        public int IntentParticipantCount { get; set; }
        public decimal IntentQuantity { get; set; }
        public int ConfirmedParticipantCount { get; set; }
        public decimal ConfirmedQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public int RequestedFulfillmentStartsAtTick { get; set; }
        public int RequestedFulfillmentEndsAtTick { get; set; }
        public string PickupPointStateCode { get; set; } = string.Empty;
        public string InquiryStateCode { get; set; } = string.Empty;
        public string QueueCode { get; set; } = string.Empty;
        public string[] PriorityReasonCodes { get; set; } = Array.Empty<string>();
        public string[] AvailableActionCodes { get; set; } = Array.Empty<string>();
        public 도심마트주문자집단DialogueWorldState Dialogue { get; set; } =
            new 도심마트주문자집단DialogueWorldState();
    }

    public sealed class 도심마트주민주문자집단PerspectiveInterpreter
    {
        public 도심마트주민주문자집단PerspectiveWorldState Interpret(
            도심마트주문자집단수요SimulationDataSnapshot groupSnapshot,
            도심마트주민본인참여SimulationDataSnapshot ownParticipation,
            bool isAuthorized)
        {
            var group = ValidateAndFind(groupSnapshot, ownParticipation.SessionStableId,
                ownParticipation.ScenarioStableId, ownParticipation.OrdererGroupStableId);
            ValidateOwnParticipation(ownParticipation, group);
            if (!isAuthorized) return new 도심마트주민주문자집단PerspectiveWorldState();
            return new 도심마트주민주문자집단PerspectiveWorldState
            {
                IsAuthorized = true,
                PerspectiveRevision = "resident-group-perspective:" + groupSnapshot.DataRevision
                    + ":" + ownParticipation.ParticipationRevision,
                OrdererGroupStableId = group.OrdererGroupStableId,
                ParticipantStableId = ownParticipation.ParticipantStableId,
                ParticipationStateCode = ownParticipation.ParticipationStateCode,
                MyIntentQuantity = ownParticipation.IntentQuantity,
                MyConfirmedQuantity = ownParticipation.ConfirmedQuantity,
                QuantityUnitCode = ownParticipation.QuantityUnitCode,
                RequestedFulfillmentStartsAtTick = group.RequestedFulfillmentStartsAtTick,
                RequestedFulfillmentEndsAtTick = group.RequestedFulfillmentEndsAtTick,
                PickupPointStableId = group.RequestedPickupPointStableId,
                AvailableActionCodes = new[]
                {
                    도심마트주문자집단PerspectiveActionCodes.ReviewOwnParticipation,
                    도심마트주문자집단PerspectiveActionCodes.WithdrawOwnParticipation,
                },
            };
        }
    }

    public sealed class 도심마트대표주문자집단PerspectiveInterpreter
    {
        public 도심마트대표주문자집단PerspectiveWorldState Interpret(
            도심마트주문자집단수요SimulationDataSnapshot groupSnapshot,
            도심마트대표마트문의SimulationDataSnapshot inquirySnapshot,
            bool hasCanonicalRepresentativeCapability)
        {
            var inquiry = ValidateAndSingleInquiry(groupSnapshot, inquirySnapshot);
            var group = groupSnapshot.Groups.Single(value =>
                value.OrdererGroupStableId == inquiry.OrdererGroupStableId);
            if (!hasCanonicalRepresentativeCapability)
                return new 도심마트대표주문자집단PerspectiveWorldState();
            var actions = inquiry.StateCode == 도심마트대표마트문의StateCodes.DraftPreparing
                ? new[] { 도심마트주문자집단PerspectiveActionCodes.ReviewAggregateDemand,
                    도심마트주문자집단PerspectiveActionCodes.PrepareMarketInquiry }
                : inquiry.StateCode == 도심마트대표마트문의StateCodes.OfferReady
                    ? new[] { 도심마트주문자집단PerspectiveActionCodes.ReviewAggregateDemand,
                        도심마트주문자집단PerspectiveActionCodes.RelayMarketOffer,
                        도심마트주문자집단PerspectiveActionCodes.CoordinatePickup }
                    : new[] { 도심마트주문자집단PerspectiveActionCodes.ReviewAggregateDemand,
                        도심마트주문자집단PerspectiveActionCodes.ReviewInquiryStatus };
            return new 도심마트대표주문자집단PerspectiveWorldState
            {
                IsAuthorized = true,
                PerspectiveRevision = Revision("representative", groupSnapshot, inquirySnapshot),
                OrdererGroupStableId = group.OrdererGroupStableId,
                RepresentativeStableId = group.Representative.RepresentativeStableId,
                SocialContextCode = group.Representative.SocialContextCode,
                CanonicalRoleCode = group.Representative.CanonicalRoleCode,
                IntentParticipantCount = group.IntentParticipantCount,
                IntentQuantity = group.IntentQuantity,
                ConfirmedParticipantCount = group.ConfirmedParticipantCount,
                ConfirmedQuantity = group.ConfirmedQuantity,
                QuantityUnitCode = group.QuantityUnitCode,
                InquiryStateCode = inquiry.StateCode,
                PickupPointStableId = group.RequestedPickupPointStableId,
                PickupPointStateCode = group.PickupPointStateCode,
                AvailableActionCodes = actions,
                Dialogue = Dialogue(inquiry, true, "ResidentialGroupSupplyInquiry"),
            };
        }
    }

    public sealed class 마트관리자주문자집단PerspectiveInterpreter
    {
        public 마트관리자주문자집단PerspectiveWorldState Interpret(
            도심마트주문자집단수요SimulationDataSnapshot groupSnapshot,
            도심마트대표마트문의SimulationDataSnapshot inquirySnapshot,
            bool hasMarketManagerCapability)
        {
            var inquiry = ValidateAndSingleInquiry(groupSnapshot, inquirySnapshot);
            var group = groupSnapshot.Groups.Single(value =>
                value.OrdererGroupStableId == inquiry.OrdererGroupStableId);
            if (!hasMarketManagerCapability)
                return new 마트관리자주문자집단PerspectiveWorldState();
            var submitted = inquiry.StateCode != 도심마트대표마트문의StateCodes.DraftPreparing;
            var reasons = submitted
                ? new[]
                {
                    도심마트주문자집단ManagerReasonCodes.GroupConfirmedDemandPresent,
                    도심마트주문자집단ManagerReasonCodes.MemberConfirmationPending,
                    도심마트주문자집단ManagerReasonCodes.RepresentativeInquiryPending,
                }
                : Array.Empty<string>();
            return new 마트관리자주문자집단PerspectiveWorldState
            {
                IsAuthorized = true,
                PerspectiveRevision = Revision("market-manager", groupSnapshot, inquirySnapshot),
                OrdererGroupStableId = group.OrdererGroupStableId,
                ProductStableId = group.ProductStableId,
                GroupContextCode = group.GroupContextCode,
                GroupStateCode = group.StateCode,
                IntentParticipantCount = group.IntentParticipantCount,
                IntentQuantity = group.IntentQuantity,
                ConfirmedParticipantCount = group.ConfirmedParticipantCount,
                ConfirmedQuantity = group.ConfirmedQuantity,
                QuantityUnitCode = group.QuantityUnitCode,
                RequestedFulfillmentStartsAtTick = group.RequestedFulfillmentStartsAtTick,
                RequestedFulfillmentEndsAtTick = group.RequestedFulfillmentEndsAtTick,
                PickupPointStateCode = group.PickupPointStateCode,
                InquiryStateCode = inquiry.StateCode,
                QueueCode = submitted
                    ? 도심마트주문자집단ManagerQueueCodes.ConfirmedDemandSupplyReviewRequired
                    : 도심마트주문자집단ManagerQueueCodes.NoActionNeeded,
                PriorityReasonCodes = reasons.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                AvailableActionCodes = submitted
                    ? new[]
                    {
                        도심마트주문자집단PerspectiveActionCodes.ReviewOrdererGroupDemand,
                        도심마트주문자집단PerspectiveActionCodes.PreviewSupplyPlan,
                    }
                    : Array.Empty<string>(),
                Dialogue = Dialogue(inquiry, submitted, "ReviewOrdererGroupDemand"),
            };
        }
    }

    internal static class 도심마트주문자집단PerspectiveSupport
    {
        public static 도심마트주문자집단수요SimulationData ValidateAndFind(
            도심마트주문자집단수요SimulationDataSnapshot snapshot,
            string sessionStableId,
            string scenarioStableId,
            string groupStableId)
        {
            도심마트공급경영SimulationDataValidator.Validate(snapshot);
            if (snapshot.SessionStableId != sessionStableId)
                throw new SimulationContractException("OrdererGroupPerspectiveSessionMismatch");
            if (snapshot.ScenarioStableId != scenarioStableId)
                throw new SimulationContractException("OrdererGroupPerspectiveScenarioMismatch");
            return snapshot.Groups.SingleOrDefault(value => value.OrdererGroupStableId == groupStableId)
                ?? throw new SimulationContractException("OrdererGroupPerspectiveGroupMissing");
        }

        public static 도심마트대표마트문의SimulationData ValidateAndSingleInquiry(
            도심마트주문자집단수요SimulationDataSnapshot groups,
            도심마트대표마트문의SimulationDataSnapshot inquiries)
        {
            도심마트공급경영SimulationDataValidator.Validate(groups);
            ValidateInquirySnapshot(inquiries);
            if (groups.SessionStableId != inquiries.SessionStableId)
                throw new SimulationContractException("OrdererGroupPerspectiveSessionMismatch");
            if (groups.ScenarioStableId != inquiries.ScenarioStableId)
                throw new SimulationContractException("OrdererGroupPerspectiveScenarioMismatch");
            var inquiry = inquiries.Inquiries.Single();
            var group = groups.Groups.SingleOrDefault(value =>
                value.OrdererGroupStableId == inquiry.OrdererGroupStableId)
                ?? throw new SimulationContractException("OrdererGroupPerspectiveGroupMissing");
            if (group.Representative.RepresentativeStableId != inquiry.RepresentativeStableId)
                throw new SimulationContractException("OrdererGroupPerspectiveRepresentativeMismatch");
            return inquiry;
        }

        public static void ValidateOwnParticipation(
            도심마트주민본인참여SimulationDataSnapshot value,
            도심마트주문자집단수요SimulationData group)
        {
            if (value.ModeCode != SimulationModeCodes.Simulation || value.IsOperationalState
                || string.IsNullOrWhiteSpace(value.SnapshotStableId)
                || string.IsNullOrWhiteSpace(value.SessionStableId)
                || string.IsNullOrWhiteSpace(value.ScenarioStableId)
                || string.IsNullOrWhiteSpace(value.DataRevision)
                || string.IsNullOrWhiteSpace(value.ParticipantStableId)
                || value.ParticipationStateCode != 도심마트주민참여StateCodes.Confirmed
                || value.IntentQuantity < value.ConfirmedQuantity || value.ConfirmedQuantity < 0m
                || value.QuantityUnitCode != group.QuantityUnitCode
                || string.IsNullOrWhiteSpace(value.ParticipationRevision))
                throw new SimulationContractException("ResidentOwnParticipationInvalid");
        }

        public static void ValidateInquirySnapshot(도심마트대표마트문의SimulationDataSnapshot value)
        {
            if (value.ModeCode != SimulationModeCodes.Simulation || value.IsOperationalState
                || string.IsNullOrWhiteSpace(value.SnapshotStableId)
                || string.IsNullOrWhiteSpace(value.SessionStableId)
                || string.IsNullOrWhiteSpace(value.ScenarioStableId)
                || string.IsNullOrWhiteSpace(value.DataRevision)
                || value.Inquiries == null || value.Inquiries.Length != 1)
                throw new SimulationContractException("RepresentativeInquirySnapshotInvalid");
            var inquiry = value.Inquiries[0];
            var states = new[] { 도심마트대표마트문의StateCodes.DraftPreparing,
                도심마트대표마트문의StateCodes.ManagerReviewPending,
                도심마트대표마트문의StateCodes.OfferReady,
                도심마트대표마트문의StateCodes.RelayingOffer,
                도심마트대표마트문의StateCodes.PickupCoordination };
            if (string.IsNullOrWhiteSpace(inquiry.InquiryStableId)
                || string.IsNullOrWhiteSpace(inquiry.OrdererGroupStableId)
                || string.IsNullOrWhiteSpace(inquiry.RepresentativeStableId)
                || !states.Contains(inquiry.StateCode)
                || inquiry.RequestedAtTick < 0
                || string.IsNullOrWhiteSpace(inquiry.RequestedConditionRevision)
                || string.IsNullOrWhiteSpace(inquiry.DialogueRevision)
                || (inquiry.StateCode == 도심마트대표마트문의StateCodes.OfferReady
                    && string.IsNullOrWhiteSpace(inquiry.MarketQuoteRevision)))
                throw new SimulationContractException("RepresentativeInquiryInvalid");
        }

        public static string Revision(string role,
            도심마트주문자집단수요SimulationDataSnapshot group,
            도심마트대표마트문의SimulationDataSnapshot inquiry)
            => role + "-group-perspective:" + group.DataRevision + ":" + inquiry.DataRevision;

        public static 도심마트주문자집단DialogueWorldState Dialogue(
            도심마트대표마트문의SimulationData inquiry,
            bool canOpen,
            string topic)
            => new 도심마트주문자집단DialogueWorldState
            {
                DialogueStableId = "dialogue:" + inquiry.InquiryStableId,
                InquiryStableId = inquiry.InquiryStableId,
                TopicCode = topic,
                CanOpen = canOpen,
                CommandEffectCode = 도심마트DialogueCommandEffectCodes.None,
                DialogueRevision = inquiry.DialogueRevision,
            };
    }

    public static class 도심마트주문자집단PerspectiveSimulationFixture
    {
        public static 도심마트주민본인참여SimulationDataSnapshot OwnParticipation()
            => new 도심마트주민본인참여SimulationDataSnapshot
            {
                SnapshotStableId = "own-participation-snapshot:sim:001",
                SessionStableId = "simulation-session:potato-fixture",
                ScenarioStableId = 도심마트감자공급SimulationFixture.ScenarioStableId,
                DataRevision = "own-participation-data:1",
                ModeCode = SimulationModeCodes.Simulation,
                OrdererGroupStableId = 도심마트공동주택주문자집단SimulationFixture.OrdererGroupStableId,
                ParticipantStableId = "participant:sim:001",
                ParticipationStateCode = 도심마트주민참여StateCodes.Confirmed,
                IntentQuantity = 5m,
                ConfirmedQuantity = 5m,
                QuantityUnitCode = 도심마트감자공급SimulationFixture.QuantityUnitCode,
                ParticipationRevision = "participant:sim:001:revision:1",
            };

        public static 도심마트대표마트문의SimulationDataSnapshot Inquiry()
        {
            var group = 도심마트공동주택주문자집단SimulationFixture.Create().Groups.Single();
            return new 도심마트대표마트문의SimulationDataSnapshot
            {
                SnapshotStableId = "representative-inquiry-snapshot:potato:1",
                SessionStableId = "simulation-session:potato-fixture",
                ScenarioStableId = 도심마트감자공급SimulationFixture.ScenarioStableId,
                DataRevision = "representative-inquiry-data:1",
                ModeCode = SimulationModeCodes.Simulation,
                Inquiries = new[]
                {
                    new 도심마트대표마트문의SimulationData
                    {
                        InquiryStableId = "market-inquiry:sim:potato:1",
                        OrdererGroupStableId = group.OrdererGroupStableId,
                        RepresentativeStableId = group.Representative.RepresentativeStableId,
                        StateCode = 도심마트대표마트문의StateCodes.ManagerReviewPending,
                        RequestedAtTick = 5,
                        RequestedConditionRevision = "requested-condition:potato:1",
                        DialogueRevision = "representative-dialogue:potato:1",
                    },
                },
            };
        }
    }
}
