using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class 도심마트공동주택주문자집단SimulationFixture
    {
        public const string OrdererGroupStableId =
            "orderer-group:residential:potato:1";
        public const string RequestedPickupPointStableId =
            "pickup-point:residential:sample-1";
        public const string RepresentativeNpcStableId =
            "npc:sim:residential-group-representative:1";
        public const string RepresentativeVisitStableId =
            "representative-visit:sim:potato:1";

        public static 도심마트주문자집단수요SimulationDataSnapshot Create()
        {
            var snapshot = new 도심마트주문자집단수요SimulationDataSnapshot
            {
                SnapshotStableId = "orderer-group-demand-snapshot:residential-potato:1",
                SessionStableId = "simulation-session:potato-fixture",
                ScenarioStableId = 도심마트감자공급SimulationFixture.ScenarioStableId,
                DataRevision = "residential-potato-group-demand:1",
                AsOfTick = 0,
                ModeCode = SimulationModeCodes.Simulation,
                IsOperationalState = false,
                SourceLineage = new[]
                {
                    new SimulationDataLineage
                    {
                        SourceStableId = "fixture-definition:residential-potato-group:1",
                        SourceDataRevision = "residential-potato-group-fixture:1",
                        RuleRevision = "residential-group-fixture-rule:1",
                    },
                },
                Groups = new[]
                {
                    new 도심마트주문자집단수요SimulationData
                    {
                        OrdererGroupStableId = OrdererGroupStableId,
                        DemandRequestStableId = "demand-request:residential:potato:1",
                        GroupContextCode = 도심마트주문자집단ContextCodes.ResidentialCommunity,
                        ProductStableId = 도심마트감자공급SimulationFixture.ProductStableId,
                        StateCode = 도심마트주문자집단StateCodes.MemberConfirmationPending,
                        IntentParticipantCount = 67,
                        IntentQuantity = 410m,
                        ConfirmedParticipantCount = 61,
                        ConfirmedQuantity = 385m,
                        QuantityUnitCode = 도심마트감자공급SimulationFixture.QuantityUnitCode,
                        RequestedFulfillmentStartsAtTick = 7,
                        RequestedFulfillmentEndsAtTick = 27,
                        RequestedPickupPointStableId = RequestedPickupPointStableId,
                        PickupPointStateCode = 도심마트공동수령지StateCodes.Candidate,
                        Representative = new 도심마트주문자집단대표SimulationData
                        {
                            RepresentativeStableId =
                                "representative:sim:residential-potato-group:1",
                            SocialContextCode =
                                도심마트대표SocialContextCodes.ResidentialCommunityRepresentative,
                            DisplayLabel = "주민자치 대표",
                            CanonicalRoleCode =
                                도심마트대표CanonicalRoleCodes.GroupPurchaseRepresentative,
                            RoleStateCode =
                                도심마트대표RoleStateCodes.AssignedSimulatedCoordinator,
                            NpcStableId = RepresentativeNpcStableId,
                            RepresentativeVisitStableId = RepresentativeVisitStableId,
                        },
                    },
                },
            };

            도심마트공급경영SimulationDataValidator.Validate(snapshot);
            return snapshot;
        }
    }
}
