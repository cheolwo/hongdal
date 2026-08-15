using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class PyeongchangSimulationNpcStableIds
    {
        public const string 진부물류조직 = "organization:sim:pyeongchang:jinbu-logistics";
        public const string 진부Hub관리자 = "actor:sim:pyeongchang:jinbu-hub-manager";
        public const string 진부입고검수담당 = "actor:sim:pyeongchang:jinbu-inbound-operator";
        public const string 진부물류보조 = "actor:sim:pyeongchang:jinbu-logistics-assistant";
        public const string 진부적재담당 = 진부물류보조;
        public const string 진부입고검수정책 = "npc-policy:sim:pyeongchang:jinbu-inbound-inspection";
        public const string 진부적재정책 = "npc-policy:sim:pyeongchang:jinbu-put-away";
    }

    public static class PyeongchangSimulationNpcWorkforceFixture
    {
        public static SimulationNpcWorkforceInitialStateRequest Create()
            => new SimulationNpcWorkforceInitialStateRequest
            {
                Organizations = new[]
                {
                    new SimulationNpcOrganizationInitialRequest
                    {
                        OrganizationStableId = PyeongchangSimulationNpcStableIds.진부물류조직,
                        DisplayName = "진부면 가상 물류조직",
                        FacilityStableIds = new[] { PyeongchangSimulationWorldStableIds.진부Hub시설 },
                        AllowedCapabilityCodes = new[]
                        {
                            SimulationNpcCapabilityCodes.WorkforceDelegate,
                            SimulationNpcCapabilityCodes.WarehouseInboundInspection,
                            SimulationNpcCapabilityCodes.WarehouseStorageMove,
                        },
                        SourceStableIds = new[] { "scenario:pyeongchang-farm-hub-town.v1" },
                    },
                },
                Actors = new[]
                {
                    Actor(
                        PyeongchangSimulationNpcStableIds.진부Hub관리자,
                        "진부면 물류 거점 관리자 NPC",
                        "Warehouse.Manager",
                        new[] { SimulationNpcCapabilityCodes.WorkforceDelegate },
                        80),
                    Actor(
                        PyeongchangSimulationNpcStableIds.진부입고검수담당,
                        "진부 입고 검수 담당 NPC",
                        "Warehouse.InboundOperator",
                        new[] { SimulationNpcCapabilityCodes.WarehouseInboundInspection },
                        90),
                    Actor(
                        PyeongchangSimulationNpcStableIds.진부물류보조,
                        "진부 물류 보조 NPC",
                        "Warehouse.Assistant",
                        new[]
                        {
                            SimulationNpcCapabilityCodes.WarehouseInboundInspection,
                            SimulationNpcCapabilityCodes.WarehouseStorageMove,
                        },
                        60),
                },
                CapabilityGrants = new[]
                {
                    new SimulationNpcCapabilityGrantInitialRequest
                    {
                        GrantStableId = "grant:sim:pyeongchang:jinbu-manager-delegation",
                        OrganizationStableId = PyeongchangSimulationNpcStableIds.진부물류조직,
                        ActorStableId = PyeongchangSimulationNpcStableIds.진부Hub관리자,
                        FacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                        CapabilityCode = SimulationNpcCapabilityCodes.WorkforceDelegate,
                        GrantedByActorStableId = PyeongchangSimulationNpcStableIds.진부Hub관리자,
                        CanDelegate = true,
                        SourceStableIds = new[] { "scenario:pyeongchang-farm-hub-town.v1" },
                    },
                    new SimulationNpcCapabilityGrantInitialRequest
                    {
                        GrantStableId = "grant:sim:pyeongchang:jinbu-storage-operator",
                        OrganizationStableId = PyeongchangSimulationNpcStableIds.진부물류조직,
                        ActorStableId = PyeongchangSimulationNpcStableIds.진부적재담당,
                        FacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                        CapabilityCode = SimulationNpcCapabilityCodes.WarehouseStorageMove,
                        GrantedByActorStableId = PyeongchangSimulationNpcStableIds.진부Hub관리자,
                        CanDelegate = false,
                        SourceStableIds = new[] { "scenario:pyeongchang-farm-hub-town.v1" },
                    },
                    new SimulationNpcCapabilityGrantInitialRequest
                    {
                        GrantStableId = "grant:sim:pyeongchang:jinbu-inbound-operator",
                        OrganizationStableId = PyeongchangSimulationNpcStableIds.진부물류조직,
                        ActorStableId = PyeongchangSimulationNpcStableIds.진부입고검수담당,
                        FacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                        CapabilityCode = SimulationNpcCapabilityCodes.WarehouseInboundInspection,
                        GrantedByActorStableId = PyeongchangSimulationNpcStableIds.진부Hub관리자,
                        CanDelegate = false,
                        SourceStableIds = new[] { "scenario:pyeongchang-farm-hub-town.v1" },
                    },
                },
                Policies = new[]
                {
                    new SimulationNpcWorkPolicyInitialRequest
                    {
                        PolicyStableId = PyeongchangSimulationNpcStableIds.진부입고검수정책,
                        OrganizationStableId = PyeongchangSimulationNpcStableIds.진부물류조직,
                        FacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                        ActionCode = SimulationNpcActionCodes.WarehouseInboundInspection,
                        RequiredCapabilityCode = SimulationNpcCapabilityCodes.WarehouseInboundInspection,
                        AutomationEnabled = true,
                        Priority = 100,
                        PreferredActorStableId = PyeongchangSimulationNpcStableIds.진부입고검수담당,
                        AutoDelegationEnabled = true,
                        AutoDelegationBacklogThreshold = 2,
                        TravelDurationTicks = 1,
                        WorkDurationTicks = 2,
                        InteractionPointKey = "interaction-point:jinbu-hub:inbound-inspection",
                        ActionVisualKey = "action-visual:warehouse:inbound-inspection",
                        SourceStableIds = new[]
                        {
                            PyeongchangSimulationWorldStableIds.창고입고검수규칙,
                            "scenario:pyeongchang-farm-hub-town.v1",
                        },
                    },
                    new SimulationNpcWorkPolicyInitialRequest
                    {
                        PolicyStableId = PyeongchangSimulationNpcStableIds.진부적재정책,
                        OrganizationStableId = PyeongchangSimulationNpcStableIds.진부물류조직,
                        FacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                        ActionCode = SimulationNpcActionCodes.WarehouseStorageMove,
                        RequiredCapabilityCode = SimulationNpcCapabilityCodes.WarehouseStorageMove,
                        AutomationEnabled = true,
                        Priority = 90,
                        PreferredActorStableId = PyeongchangSimulationNpcStableIds.진부적재담당,
                        AutoDelegationEnabled = false,
                        AutoDelegationBacklogThreshold = 2,
                        TravelDurationTicks = 1,
                        WorkDurationTicks = 2,
                        InteractionPointKey = "interaction-point:jinbu-hub:storage-rack",
                        ActionVisualKey = "action-visual:warehouse:put-away",
                        SourceStableIds = new[]
                        {
                            PyeongchangSimulationWorldStableIds.창고적재규칙,
                            "scenario:pyeongchang-farm-hub-town.v1",
                        },
                    },
                },
            };

        private static SimulationNpcActorInitialRequest Actor(
            string stableId,
            string displayName,
            string referenceRoleCode,
            string[] assignableCapabilities,
            int skillScore)
            => new SimulationNpcActorInitialRequest
            {
                ActorStableId = stableId,
                OrganizationStableId = PyeongchangSimulationNpcStableIds.진부물류조직,
                DisplayName = displayName,
                HomeFacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                ReferenceRoleCode = referenceRoleCode,
                MaximumConcurrentTasks = 1,
                AssignableCapabilityCodes = assignableCapabilities,
                Skills = System.Array.ConvertAll(assignableCapabilities, capability =>
                    new SimulationNpcSkillInitialRequest
                    {
                        CapabilityCode = capability,
                        Score = skillScore,
                    }),
                SourceStableIds = new[] { "scenario:pyeongchang-farm-hub-town.v1" },
            };
    }
}
