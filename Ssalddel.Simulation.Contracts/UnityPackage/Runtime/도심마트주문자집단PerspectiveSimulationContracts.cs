using System;

namespace Ssalddel.Simulation.Contracts
{
    public sealed class 도심마트주민본인참여SimulationDataSnapshot : 도심마트SimulationDataSnapshot
    {
        public string OrdererGroupStableId { get; set; } = string.Empty;
        public string ParticipantStableId { get; set; } = string.Empty;
        public string ParticipationStateCode { get; set; } = string.Empty;
        public decimal IntentQuantity { get; set; }
        public decimal ConfirmedQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public string ParticipationRevision { get; set; } = string.Empty;
    }

    public sealed class 도심마트대표마트문의SimulationDataSnapshot : 도심마트SimulationDataSnapshot
    {
        public 도심마트대표마트문의SimulationData[] Inquiries { get; set; } =
            Array.Empty<도심마트대표마트문의SimulationData>();
    }

    public sealed class 도심마트대표마트문의SimulationData
    {
        public string InquiryStableId { get; set; } = string.Empty;
        public string OrdererGroupStableId { get; set; } = string.Empty;
        public string RepresentativeStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int RequestedAtTick { get; set; }
        public string RequestedConditionRevision { get; set; } = string.Empty;
        public string MarketQuoteRevision { get; set; } = string.Empty;
        public string DialogueRevision { get; set; } = string.Empty;
    }

    public static class 도심마트주민참여StateCodes
    {
        public const string Confirmed = "Confirmed";
    }

    public static class 도심마트대표마트문의StateCodes
    {
        public const string DraftPreparing = "DraftPreparing";
        public const string ManagerReviewPending = "ManagerReviewPending";
        public const string OfferReady = "OfferReady";
        public const string RelayingOffer = "RelayingOffer";
        public const string PickupCoordination = "PickupCoordination";
    }
}
