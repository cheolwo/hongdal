using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class PyeongchangSimulationNpcStableIds
    {
        public const string 진부물류조직 = "organization:sim:pyeongchang:jinbu-logistics";
        public const string 진부Hub관리자 = "actor:sim:pyeongchang:jinbu-hub-manager";
        public const string 진부입고검수담당 = "actor:sim:pyeongchang:jinbu-inbound-operator";
        public const string 진부물류보조 = "actor:sim:pyeongchang:jinbu-logistics-assistant";
        public const string 진부출고준비담당 = "actor:sim:pyeongchang:jinbu-outbound-operator";
        public const string 진부적재담당 = 진부물류보조;
        public const string 진부입고검수정책 = "npc-policy:sim:pyeongchang:jinbu-inbound-inspection";
        public const string 진부적재정책 = "npc-policy:sim:pyeongchang:jinbu-put-away";
        public const string 진부출고준비정책 = "npc-policy:sim:pyeongchang:jinbu-outbound-preparation";
        public const string Nature생활조직 =
            "organization:sim:pyeongchang:nature-homestead";
        public const string Nature거점관리자 =
            "actor:sim:pyeongchang:nature-homestead-manager";
        public const string Nature보급담당 =
            "actor:sim:pyeongchang:nature-field-supply-worker";
        public const string Nature현장보급정책 =
            "npc-policy:sim:pyeongchang:nature-field-supply";
    }

    public static class PyeongchangSimulationNpcWorkforceFixture
    {
        public const string 진부Hub출고대기재고 =
            "npc-inventory:sim:pyeongchang:jinbu-hub:potato-fixture-1";

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
                            SimulationNpcCapabilityCodes.WarehouseOutboundPreparation,
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
                    Actor(
                        PyeongchangSimulationNpcStableIds.진부출고준비담당,
                        "진부 출고 준비 담당 NPC",
                        "Warehouse.OutboundOperator",
                        new[] { SimulationNpcCapabilityCodes.WarehouseOutboundPreparation },
                        85),
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
                        GrantStableId = "grant:sim:pyeongchang:jinbu-outbound-operator",
                        OrganizationStableId = PyeongchangSimulationNpcStableIds.진부물류조직,
                        ActorStableId = PyeongchangSimulationNpcStableIds.진부출고준비담당,
                        FacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                        CapabilityCode = SimulationNpcCapabilityCodes.WarehouseOutboundPreparation,
                        GrantedByActorStableId = PyeongchangSimulationNpcStableIds.진부Hub관리자,
                        CanDelegate = false,
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
                        PolicyStableId = PyeongchangSimulationNpcStableIds.진부출고준비정책,
                        OrganizationStableId = PyeongchangSimulationNpcStableIds.진부물류조직,
                        FacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                        ActionCode = SimulationNpcActionCodes.WarehouseOutboundFlow,
                        RequiredCapabilityCode = SimulationNpcCapabilityCodes.WarehouseOutboundPreparation,
                        AutomationEnabled = true,
                        Priority = 80,
                        PreferredActorStableId = PyeongchangSimulationNpcStableIds.진부출고준비담당,
                        AutoDelegationEnabled = false,
                        AutoDelegationBacklogThreshold = 2,
                        TravelDurationTicks = 1,
                        WorkDurationTicks = 1,
                        InteractionPointKey = "interaction-point:jinbu-hub:picking",
                        ActionVisualKey = "action-visual:warehouse:outbound-preparation",
                        SourceStableIds = new[]
                        {
                            "rule:npc-routine-control.r1",
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

        /// <summary>
        /// Nature가 다른 업무 영역이나 운송을 요구하지 않고 현장 보급 위임만
        /// 결정적으로 검증하는 독립 Fixture다. 정책은 플레이어 선택 전까지 꺼져 있다.
        /// </summary>
        public static SimulationNpcWorkforceInitialStateRequest
            CreateNatureFieldSupplyFixture()
            => new SimulationNpcWorkforceInitialStateRequest
            {
                Organizations = new[]
                {
                    new SimulationNpcOrganizationInitialRequest
                    {
                        OrganizationStableId =
                            PyeongchangSimulationNpcStableIds.Nature생활조직,
                        DisplayName = "Nature 생활 거점 조직",
                        FacilityStableIds = new[]
                        {
                            Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                        },
                        AllowedCapabilityCodes = new[]
                        {
                            SimulationNpcCapabilityCodes.WorkforceDelegate,
                            SimulationNpcCapabilityCodes
                                .NatureFieldSupplyPreparation,
                        },
                        SourceStableIds = new[]
                        {
                            "fixture:nature-field-supply-delegation.r1",
                        },
                    },
                },
                Actors = new[]
                {
                    new SimulationNpcActorInitialRequest
                    {
                        ActorStableId =
                            PyeongchangSimulationNpcStableIds.Nature거점관리자,
                        OrganizationStableId =
                            PyeongchangSimulationNpcStableIds.Nature생활조직,
                        DisplayName = "Nature 거점 관리자 NPC",
                        HomeFacilityStableId =
                            Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                        ReferenceRoleCode = "Nature.HomesteadManager",
                        MaximumConcurrentTasks = 1,
                        AssignableCapabilityCodes = new[]
                        {
                            SimulationNpcCapabilityCodes.WorkforceDelegate,
                        },
                        Skills = new[]
                        {
                            new SimulationNpcSkillInitialRequest
                            {
                                CapabilityCode = SimulationNpcCapabilityCodes
                                    .WorkforceDelegate,
                                Score = 80,
                            },
                        },
                        SourceStableIds = new[]
                        {
                            "fixture:nature-field-supply-delegation.r1",
                        },
                    },
                    new SimulationNpcActorInitialRequest
                    {
                        ActorStableId =
                            PyeongchangSimulationNpcStableIds.Nature보급담당,
                        OrganizationStableId =
                            PyeongchangSimulationNpcStableIds.Nature생활조직,
                        DisplayName = "Nature 현장 보급 담당 NPC",
                        HomeFacilityStableId =
                            Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                        ReferenceRoleCode = "Nature.FieldSupplyWorker",
                        MaximumConcurrentTasks = 1,
                        AssignableCapabilityCodes = new[]
                        {
                            SimulationNpcCapabilityCodes
                                .NatureFieldSupplyPreparation,
                        },
                        Skills = new[]
                        {
                            new SimulationNpcSkillInitialRequest
                            {
                                CapabilityCode = SimulationNpcCapabilityCodes
                                    .NatureFieldSupplyPreparation,
                                Score = 85,
                            },
                        },
                        SourceStableIds = new[]
                        {
                            "fixture:nature-field-supply-delegation.r1",
                        },
                    },
                },
                CapabilityGrants = new[]
                {
                    new SimulationNpcCapabilityGrantInitialRequest
                    {
                        GrantStableId =
                            "grant:sim:pyeongchang:nature-manager-delegation",
                        OrganizationStableId =
                            PyeongchangSimulationNpcStableIds.Nature생활조직,
                        ActorStableId =
                            PyeongchangSimulationNpcStableIds.Nature거점관리자,
                        FacilityStableId =
                            Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                        CapabilityCode =
                            SimulationNpcCapabilityCodes.WorkforceDelegate,
                        GrantedByActorStableId =
                            PyeongchangSimulationNpcStableIds.Nature거점관리자,
                        CanDelegate = true,
                        SourceStableIds = new[]
                        {
                            "fixture:nature-field-supply-delegation.r1",
                        },
                    },
                    new SimulationNpcCapabilityGrantInitialRequest
                    {
                        GrantStableId =
                            "grant:sim:pyeongchang:nature-field-supply-worker",
                        OrganizationStableId =
                            PyeongchangSimulationNpcStableIds.Nature생활조직,
                        ActorStableId =
                            PyeongchangSimulationNpcStableIds.Nature보급담당,
                        FacilityStableId =
                            Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                        CapabilityCode = SimulationNpcCapabilityCodes
                            .NatureFieldSupplyPreparation,
                        GrantedByActorStableId =
                            PyeongchangSimulationNpcStableIds.Nature거점관리자,
                        CanDelegate = false,
                        SourceStableIds = new[]
                        {
                            "fixture:nature-field-supply-delegation.r1",
                        },
                    },
                },
                Policies = new[]
                {
                    new SimulationNpcWorkPolicyInitialRequest
                    {
                        PolicyStableId = PyeongchangSimulationNpcStableIds
                            .Nature현장보급정책,
                        OrganizationStableId =
                            PyeongchangSimulationNpcStableIds.Nature생활조직,
                        FacilityStableId =
                            Simulation영역건물발전Codes.NatureWorkbenchBlueprint,
                        ActionCode = SimulationNpcActionCodes
                            .NatureFieldSupplyPreparation,
                        RequiredCapabilityCode = SimulationNpcCapabilityCodes
                            .NatureFieldSupplyPreparation,
                        AutomationEnabled = false,
                        Priority = 100,
                        PreferredActorStableId =
                            PyeongchangSimulationNpcStableIds.Nature보급담당,
                        AutoDelegationEnabled = false,
                        AutoDelegationBacklogThreshold = 1,
                        TravelDurationTicks = 0,
                        WorkDurationTicks =
                            SimulationNatureSurvivalCodes.FieldSupplyCraftSeconds,
                        InteractionPointKey =
                            "interaction-point:nature:field-supply-workbench",
                        ActionVisualKey =
                            "action-visual:nature:field-supply-preparation",
                        SourceStableIds = new[]
                        {
                            "rule:npc-routine-control.r3",
                            "fixture:nature-field-supply-delegation.r1",
                        },
                    },
                },
            };

        /// <summary>
        /// Farm 생산·운송을 선행하지 않고 Hub 내부 출고 준비만
        /// 결정적으로 검증하는 Simulation Fixture다.
        /// </summary>
        public static SimulationNpcWorkforceInitialStateRequest
            CreateHubOutboundReadyFixture()
        {
            var request = Create();
            request.Inventories = new[]
            {
                new SimulationNpcFacilityInventoryInitialRequest
                {
                    InventoryStableId = 진부Hub출고대기재고,
                    LotStableId = "lot:sim:pyeongchang:jinbu-hub:potato-fixture-1",
                    FacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                    ProductStableId = "product:potato",
                    StateCode = SimulationNpcInventoryStateCodes.PutAwayCompleted,
                    Quantity = 300m,
                    UnitCode = "KGM",
                    UpdatedTick = 0,
                    SourceStableIds = new[]
                    {
                        "fixture:hub-internal-outbound-ready.r1",
                        "rule:npc-routine-control.r1",
                    },
                },
            };
            return request;
        }

        /// <summary>
        /// Farm·외부 운송 없이 Hub 내부의 300 KGM 감자 재고를
        /// 입고검수 대기부터 출고 준비까지 닫는 r2 Fixture다.
        /// </summary>
        public static SimulationNpcWorkforceInitialStateRequest
            CreateHubWarehouseFullLoopFixture()
        {
            var request = Create();
            request.Inventories = new[]
            {
                new SimulationNpcFacilityInventoryInitialRequest
                {
                    InventoryStableId = 진부Hub출고대기재고,
                    LotStableId =
                        "lot:sim:pyeongchang:jinbu-hub:potato-fixture-1",
                    FacilityStableId =
                        PyeongchangSimulationWorldStableIds.진부Hub시설,
                    ProductStableId = "product:potato",
                    StateCode =
                        SimulationNpcInventoryStateCodes.PendingInspection,
                    Quantity = 300m,
                    UnitCode = "KGM",
                    UpdatedTick = 0,
                    SourceStableIds = new[]
                    {
                        "fixture:hub-internal-full-loop.r1",
                        "rule:npc-routine-control.r2",
                    },
                },
            };
            return request;
        }

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
