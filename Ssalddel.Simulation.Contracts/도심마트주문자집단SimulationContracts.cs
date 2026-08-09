using System;

namespace Ssalddel.Simulation.Contracts
{
    public sealed class 도심마트주문자집단수요SimulationDataSnapshot : 도심마트SimulationDataSnapshot
    {
        public 도심마트주문자집단수요SimulationData[] Groups { get; set; } =
            Array.Empty<도심마트주문자집단수요SimulationData>();
    }

    public sealed class 도심마트주문자집단수요SimulationData
    {
        public string OrdererGroupStableId { get; set; } = string.Empty;
        public string DemandRequestStableId { get; set; } = string.Empty;
        public string GroupContextCode { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public int IntentParticipantCount { get; set; }
        public decimal IntentQuantity { get; set; }
        public int ConfirmedParticipantCount { get; set; }
        public decimal ConfirmedQuantity { get; set; }
        public string QuantityUnitCode { get; set; } = string.Empty;
        public int RequestedFulfillmentStartsAtTick { get; set; }
        public int RequestedFulfillmentEndsAtTick { get; set; }
        public string RequestedPickupPointStableId { get; set; } = string.Empty;
        public string PickupPointStateCode { get; set; } = string.Empty;
        public 도심마트주문자집단대표SimulationData Representative { get; set; } =
            new 도심마트주문자집단대표SimulationData();
    }

    public sealed class 도심마트주문자집단대표SimulationData
    {
        public string RepresentativeStableId { get; set; } = string.Empty;
        public string SocialContextCode { get; set; } = string.Empty;
        public string DisplayLabel { get; set; } = string.Empty;
        public string CanonicalRoleCode { get; set; } = string.Empty;
        public string RoleStateCode { get; set; } = string.Empty;
        public string NpcStableId { get; set; } = string.Empty;
        public string RepresentativeVisitStableId { get; set; } = string.Empty;
    }

    public static class 도심마트주문자집단ContextCodes
    {
        public const string ResidentialCommunity = "ResidentialCommunity";
    }

    public static class 도심마트주문자집단StateCodes
    {
        public const string MemberConfirmationPending = "MemberConfirmationPending";
    }

    public static class 도심마트공동수령지StateCodes
    {
        public const string Candidate = "Candidate";
    }

    public static class 도심마트대표SocialContextCodes
    {
        public const string ResidentialCommunityRepresentative =
            "ResidentialCommunityRepresentative";
    }

    public static class 도심마트대표CanonicalRoleCodes
    {
        public const string GroupPurchaseRepresentative =
            "GroupPurchaseRepresentative";
    }

    public static class 도심마트대표RoleStateCodes
    {
        public const string AssignedSimulatedCoordinator =
            "AssignedSimulatedCoordinator";
    }
}
