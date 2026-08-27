using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class PyeongchangSimulation공간StableIds
    {
        public const string 진부Hub검수공간 = "spatial:scenario:pyeongchang:jinbu-hub:inspection";
        public const string 진부Hub창고공간 = "spatial:scenario:pyeongchang:jinbu-hub:warehouse";
        public const string 대관령Farm수확공간 = "spatial:scenario:pyeongchang:daegwallyeong-farm:harvest";
        public const string 대관령Farm밭갈이공간 = "spatial:scenario:pyeongchang:daegwallyeong-farm:tilling";
        public const string 대관령Farm파종공간 = "spatial:scenario:pyeongchang:daegwallyeong-farm:sowing";
        public const string 대관령Farm재배관리공간 = "spatial:scenario:pyeongchang:daegwallyeong-farm:crop-care";
        public const string 대관령Farm집하공간 = "spatial:scenario:pyeongchang:daegwallyeong-farm:collection";
        public const string 대관령Farm포장공간 = "spatial:scenario:pyeongchang:daegwallyeong-farm:packing";
        public const string 대관령Farm상차공간 = "spatial:scenario:pyeongchang:daegwallyeong-farm:loading";
        public const string FarmHub운송회랑 = "spatial:scenario:pyeongchang:farm-hub-corridor";
        public const string 진부Hub하차공간 = "spatial:scenario:pyeongchang:jinbu-hub:unloading";
        public const string 진부Hub피킹공간 = "spatial:scenario:pyeongchang:jinbu-hub:picking";
        public const string 진부Hub출고상차공간 = "spatial:scenario:pyeongchang:jinbu-hub:outbound-loading";
        public const string HubTown운송회랑 = "spatial:scenario:pyeongchang:hub-town-corridor";
        public const string 평창Town마트하차공간 = "spatial:scenario:pyeongchang:town-market:unloading";
        public const string 평창Town마트검수공간 = "spatial:scenario:pyeongchang:town-market:inspection";
        public const string 평창Town마트후방공간 = "spatial:scenario:pyeongchang:town-market:backroom";
        public const string 평창Town마트진열공간 = "spatial:scenario:pyeongchang:town-market:display";
        public const string 평창Town마트수령공간 = "spatial:scenario:pyeongchang:town-market:pickup";
        public const string 대관령Farm수리공간 = "spatial:scenario:pyeongchang:daegwallyeong-farm:repair";
        public const string Nature위협관찰공간 = "spatial:scenario:pyeongchang:nature-home:threat-observation";
        public const string Nature긴급후퇴경로 = "spatial:scenario:pyeongchang:nature-home:emergency-retreat";
        public const string Nature복원작업공간 = "spatial:scenario:pyeongchang:nature-home:restoration";
        public const string Nature안전회복야영지 = "spatial:scenario:pyeongchang:nature-home:safe-recovery";
    }

    public static class PyeongchangSimulation공간상호작용Fixture
    {
        public static Simulation공간세계InitialStateRequest CreateNatureThreatObservation(
            string sourceStableId = "scenario:pyeongchang-nature-threat-observation.v1")
            => new Simulation공간세계InitialStateRequest
            {
                Definitions = new[]
                {
                    new Simulation공간정의InitialRequest
                    {
                        SpatialStableId = PyeongchangSimulation공간StableIds.Nature위협관찰공간,
                        FacilityStableId = SimulationNatureInteractionCodes.NatureHomeFacility,
                        AreaStableId = "area:pyeongchang:nature-home",
                        AreaSetStableId = "area-set:candidate:nature-home-exploration",
                        EvidenceKindCode = Simulation공간근거종류Codes.Scenario,
                        AccessStateCode = Simulation공간접근상태Codes.Available,
                        CapabilityCodes = new[]
                        {
                            Simulation공간능력Codes.Traversable,
                            Simulation공간능력Codes.ObservationArea,
                            Simulation공간능력Codes.ThreatMonitoringArea,
                        },
                        BaseCapacities = new[]
                        {
                            new Simulation공간용량Snapshot
                            {
                                CapacityCode = Simulation공간용량Codes.WorkArea,
                                Quantity = 1m,
                                UnitCode = "slot",
                            },
                        },
                        DefinitionRevision = "scenario-nature-observation.v1",
                        DefinitionHashSha256 = new string('f', 64),
                        SourceStableIds = new[] { sourceStableId },
                    },
                },
            };

        /// <summary>
        /// 승인된 actual-e5-regional-gameplay.r1의 WI-NATURE-01 결속을
        /// LocalProcess와 RemoteHost 시작 Fixture에 동일하게 공급한다.
        /// 생성 catalog가 바뀌면 hash 회귀가 이 고정 스냅샷의 갱신을 요구한다.
        /// </summary>
        public static Simulation공간세계InitialStateRequest
            CreateNatureTwilightActualE5Observation()
            => new Simulation공간세계InitialStateRequest
            {
                Definitions = new[]
                {
                    new Simulation공간정의InitialRequest
                    {
                        SpatialStableId = SimulationNatureSurvivalCodes
                            .ActualE5SpatialStableId("WI-NATURE-01"),
                        FacilityStableId = "facility:actual-e5:nature-home.v1",
                        AreaStableId = "area:actual-e5:nature-home.v1",
                        AreaSetStableId =
                            "area-set:sim:pyeongchang:nature-home.v1",
                        LandscapeGraphStableId = "landscape-graph:sim:"
                            + "pyeongchang:nature-threat-recovery.v1",
                        LandscapeNodeStableId = "node:actual-e5:nature-threat-"
                            + "recovery:space:nature-threat-response:"
                            + "nature-incident-trace",
                        EvidenceKindCode =
                            Simulation공간근거종류Codes.LandscapeGraph,
                        AccessStateCode =
                            Simulation공간접근상태Codes.Available,
                        CapabilityCodes = new[]
                        {
                            Simulation공간능력Codes.ObservationArea,
                            Simulation공간능력Codes.ThreatMonitoringArea,
                            Simulation공간능력Codes.Traversable,
                        },
                        BaseCapacities = new[]
                        {
                            Capacity("WorkArea", 1m, "slot"),
                            Capacity("Actor", 1m, "player"),
                            Capacity("MonitoredThreatRoute", 1m, "route"),
                        },
                        DefinitionRevision = "graph:4;binding:"
                            + "actual-e5-regional-gameplay.r1",
                        DefinitionHashSha256 = "291d4248d7510fddf0a2bec2667bb9"
                            + "e7decb5dd77e8d5555912755acffbd88a7",
                        SourceStableIds = new[]
                        {
                            "source:user-approved-plan:farm-immersive-living-region.r2",
                            "h1-stock:nature-incident-trace",
                            "h2-candidate:nature-threat-response",
                            "h3-candidate:nature-threat-recovery",
                            "wi-spatial-seedbed:nature-survival-encounter.v1",
                            "landscape-graph:sim:pyeongchang:nature-threat-recovery.v1",
                            "node:actual-e5:nature-threat-recovery:space:nature-threat-response:nature-incident-trace",
                            "graph-sha256:5658ade82761d62763443afa86763690e7860500fb98e87a4a8f68e38f657935",
                            "binding-sha256:ad13c3cec36dbb42227c74f120b41a9094dccba4098b3c1ae86111508178ff86",
                        },
                    },
                },
            };

        /// <summary>
        /// 승인된 actual-e5-regional-gameplay.r2의 WI-NATURE-18 결속을
        /// 벌목 결과물 획득 Fixture에 고정한다.
        /// </summary>
        public static Simulation공간세계InitialStateRequest
            CreateNatureDroppedTimberActualE5()
            => new Simulation공간세계InitialStateRequest
            {
                Definitions = new[]
                {
                    new Simulation공간정의InitialRequest
                    {
                        SpatialStableId = SimulationNatureSurvivalCodes
                            .ActualE5SpatialStableId("WI-NATURE-18"),
                        FacilityStableId = "facility:actual-e5:nature-home.v1",
                        AreaStableId = "area:actual-e5:nature-home.v1",
                        AreaSetStableId =
                            "area-set:sim:pyeongchang:nature-home.v1",
                        LandscapeGraphStableId = "landscape-graph:sim:"
                            + "pyeongchang:nature-trail-network.v1",
                        LandscapeNodeStableId = "node:actual-e5:nature-trail-"
                            + "network:space:nature-water-buffer:"
                            + "nature-exploration-buffer",
                        EvidenceKindCode =
                            Simulation공간근거종류Codes.LandscapeGraph,
                        AccessStateCode =
                            Simulation공간접근상태Codes.Available,
                        CapabilityCodes = new[]
                        {
                            Simulation공간능력Codes.DroppedTimberPickupAnchor,
                            Simulation공간능력Codes.WorkerAccessible,
                        },
                        BaseCapacities = new[]
                        {
                            Capacity("WorkArea", 1m, "slot"),
                            Capacity("ResourceNode", 3m, "tree"),
                            Capacity("Actor", 1m, "player"),
                            Capacity("Tool", 1m, "EA"),
                            Capacity("DroppedTimber", 3m, "bundle"),
                        },
                        DefinitionRevision = "graph:6;binding:"
                            + "actual-e5-regional-gameplay.r2",
                        DefinitionHashSha256 =
                            "dadfd49a18841c7628602637e2aa3e48"
                            + "e6d170c0c609eae35533413579a0a485",
                        SourceStableIds = new[]
                        {
                            "source:user-approved-plan:farm-immersive-living-region.r2",
                            "h1-stock:nature-exploration-buffer",
                            "h2-candidate:nature-water-buffer",
                            "h3-candidate:nature-trail-network",
                            "wi-spatial-seedbed:nature-survival-encounter.v1",
                            "graph-sha256:dadfd49a18841c7628602637e2aa3e48e6d170c0c609eae35533413579a0a485",
                        },
                    },
                },
            };

        private static Simulation공간용량Snapshot Capacity(string code,
            decimal quantity, string unitCode)
            => new Simulation공간용량Snapshot
            {
                CapacityCode = code,
                Quantity = quantity,
                UnitCode = unitCode,
            };

        public static Simulation공간세계InitialStateRequest CreateNatureThreatResponse(
            string sourceStableId = "scenario:pyeongchang-nature-threat-response.v1")
        {
            var observation = CreateNatureThreatObservation(sourceStableId);
            return new Simulation공간세계InitialStateRequest
            {
                Definitions = observation.Definitions.Concat(new[]
                {
                    new Simulation공간정의InitialRequest
                    {
                        SpatialStableId = PyeongchangSimulation공간StableIds.Nature긴급후퇴경로,
                        FacilityStableId = SimulationNatureInteractionCodes.NatureHomeFacility,
                        AreaStableId = "area:pyeongchang:nature-home",
                        AreaSetStableId = "area-set:candidate:nature-home-exploration",
                        EvidenceKindCode = Simulation공간근거종류Codes.Scenario,
                        AccessStateCode = Simulation공간접근상태Codes.Available,
                        CapabilityCodes = new[]
                        {
                            Simulation공간능력Codes.Traversable,
                            Simulation공간능력Codes.EmergencyAccess,
                            Simulation공간능력Codes.PlayerEscapeRoute,
                            Simulation공간능력Codes.SafeCore,
                        },
                        BaseCapacities = new[]
                        {
                            new Simulation공간용량Snapshot
                            {
                                CapacityCode = Simulation공간용량Codes.EscapeRouteCapacity,
                                Quantity = 1m,
                                UnitCode = "party",
                            },
                        },
                        DefinitionRevision = "scenario-nature-retreat.v1",
                        DefinitionHashSha256 = new string('e', 64),
                        SourceStableIds = new[] { sourceStableId },
                    },
                    new Simulation공간정의InitialRequest
                    {
                        SpatialStableId = PyeongchangSimulation공간StableIds.Nature복원작업공간,
                        FacilityStableId = SimulationNatureInteractionCodes.NatureHomeFacility,
                        AreaStableId = "area:pyeongchang:nature-home",
                        AreaSetStableId = "area-set:candidate:nature-home-exploration",
                        EvidenceKindCode = Simulation공간근거종류Codes.Scenario,
                        AccessStateCode = Simulation공간접근상태Codes.Available,
                        CapabilityCodes = new[]
                        {
                            Simulation공간능력Codes.WorkerAccessible,
                            Simulation공간능력Codes.CargoAccessible,
                            Simulation공간능력Codes.RestorationWorkArea,
                        },
                        BaseCapacities = new[]
                        {
                            new Simulation공간용량Snapshot
                            {
                                CapacityCode = Simulation공간용량Codes.WorkArea,
                                Quantity = 1m,
                                UnitCode = "slot",
                            },
                            new Simulation공간용량Snapshot
                            {
                                CapacityCode = Simulation공간용량Codes.RestorationMaterial,
                                Quantity = 1m,
                                UnitCode = "material-lot",
                            },
                        },
                        DefinitionRevision = "scenario-nature-restoration.v1",
                        DefinitionHashSha256 = new string('d', 64),
                        SourceStableIds = new[] { sourceStableId },
                    },
                    new Simulation공간정의InitialRequest
                    {
                        SpatialStableId = PyeongchangSimulation공간StableIds.Nature안전회복야영지,
                        FacilityStableId = SimulationNatureInteractionCodes.NatureHomeFacility,
                        AreaStableId = "area:pyeongchang:nature-home",
                        AreaSetStableId = "area-set:candidate:nature-home-exploration",
                        EvidenceKindCode = Simulation공간근거종류Codes.Scenario,
                        AccessStateCode = Simulation공간접근상태Codes.Available,
                        CapabilityCodes = new[]
                        {
                            Simulation공간능력Codes.Traversable,
                            Simulation공간능력Codes.RestArea,
                            Simulation공간능력Codes.SafeCore,
                        },
                        BaseCapacities = new[]
                        {
                            new Simulation공간용량Snapshot
                            {
                                CapacityCode = Simulation공간용량Codes.RestAreaParty,
                                Quantity = 1m,
                                UnitCode = "party",
                            },
                            new Simulation공간용량Snapshot
                            {
                                CapacityCode = Simulation공간용량Codes.RecoverySupply,
                                Quantity = 1m,
                                UnitCode = "supply-lot",
                            },
                        },
                        DefinitionRevision = "scenario-nature-recovery.v1",
                        DefinitionHashSha256 = new string('c', 64),
                        SourceStableIds = new[] { sourceStableId },
                    },
                }).ToArray(),
            };
        }

        public static Simulation공간세계InitialStateRequest CreateFarmHubSupply(
            string farmFacilityStableId,
            string marketFacilityStableId = "facility:scenario:pyeongchang:town-market",
            string sourceStableId = "scenario:pyeongchang-farm-hub-town.v1")
        {
            var hub = Create(sourceStableId: sourceStableId);
            var workArea = new[]
            {
                new Simulation공간용량Snapshot
                {
                    CapacityCode = Simulation공간용량Codes.WorkArea,
                    Quantity = 1m,
                    UnitCode = "slot",
                },
            };
            var areaSet = "area-set:pyeongchang-farm-hub-town";
            var definitions = new[]
            {
                ScenarioDefinition(PyeongchangSimulation공간StableIds.대관령Farm밭갈이공간,
                    farmFacilityStableId, "area:pyeongchang:daegwallyeong-farm", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.TillingWorkArea,
                    }, workArea, '6', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.대관령Farm파종공간,
                    farmFacilityStableId, "area:pyeongchang:daegwallyeong-farm", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.CropProduction,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.SowingWorkArea,
                    }, workArea, '7', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.대관령Farm재배관리공간,
                    farmFacilityStableId, "area:pyeongchang:daegwallyeong-farm", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.CropProduction,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.WaterAccessible,
                        Simulation공간능력Codes.CropCareWorkArea,
                    }, workArea, '8', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.대관령Farm수리공간,
                    farmFacilityStableId, "area:pyeongchang:daegwallyeong-farm", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.RepairWorkArea,
                    }, workArea, 'e', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.대관령Farm수확공간,
                    farmFacilityStableId, "area:pyeongchang:daegwallyeong-farm", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.CropProduction,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.HarvestWorkArea,
                    }, workArea, 'c', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.대관령Farm집하공간,
                    farmFacilityStableId, "area:pyeongchang:daegwallyeong-farm", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.CollectionWorkArea,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.CargoAccessible,
                    }, workArea, 'd', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.대관령Farm포장공간,
                    farmFacilityStableId, "area:pyeongchang:daegwallyeong-farm", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.PackingWorkArea,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.CargoAccessible,
                    }, workArea, 'e', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.대관령Farm상차공간,
                    farmFacilityStableId, "area:pyeongchang:daegwallyeong-farm", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.LoadingWorkArea,
                        Simulation공간능력Codes.VehicleAccessible,
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.WorkerAccessible,
                    }, workArea, 'f', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.FarmHub운송회랑,
                    "facility:scenario:pyeongchang:farm-hub-corridor",
                    "area:pyeongchang:farm-hub-corridor", areaSet,
                    new[] { Simulation공간능력Codes.CargoRoute },
                    new Simulation공간용량Snapshot[0], '1', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.진부Hub하차공간,
                    PyeongchangSimulationWorldStableIds.진부Hub시설,
                    PyeongchangSimulationWorldStableIds.진부Hub영역, areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.UnloadingWorkArea,
                        Simulation공간능력Codes.InspectionWorkArea,
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.WorkerAccessible,
                    }, workArea, '2', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.진부Hub피킹공간,
                    PyeongchangSimulationWorldStableIds.진부Hub시설,
                    PyeongchangSimulationWorldStableIds.진부Hub영역, areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.Storage,
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.PickingWorkArea,
                        Simulation공간능력Codes.OutboundStagingArea,
                    }, workArea, '9', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.진부Hub출고상차공간,
                    PyeongchangSimulationWorldStableIds.진부Hub시설,
                    PyeongchangSimulationWorldStableIds.진부Hub영역, areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.LoadingWorkArea,
                        Simulation공간능력Codes.VehicleAccessible,
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.WorkerAccessible,
                    }, workArea, '0', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.HubTown운송회랑,
                    "facility:scenario:pyeongchang:hub-town-corridor",
                    "area:pyeongchang:hub-town-corridor", areaSet,
                    new[] { Simulation공간능력Codes.CargoRoute },
                    new Simulation공간용량Snapshot[0], '3', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.평창Town마트하차공간,
                    marketFacilityStableId, "area:pyeongchang:town-market", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.UnloadingWorkArea,
                        Simulation공간능력Codes.InspectionWorkArea,
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.WorkerAccessible,
                    }, workArea, '4', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.평창Town마트검수공간,
                    marketFacilityStableId, "area:pyeongchang:town-market", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.InspectionWorkArea,
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.WorkerAccessible,
                    }, workArea, '5', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.평창Town마트후방공간,
                    marketFacilityStableId, "area:pyeongchang:town-market", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.Storage,
                        Simulation공간능력Codes.LoadingWorkArea,
                        Simulation공간능력Codes.CargoAccessible,
                        Simulation공간능력Codes.WorkerAccessible,
                    }, new[]
                    {
                        new Simulation공간용량Snapshot
                        {
                            CapacityCode = Simulation공간용량Codes.StorageCapacity,
                            Quantity = 5_000m,
                            UnitCode = "KGM",
                        },
                        workArea[0],
                    }, 'a', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.평창Town마트진열공간,
                    marketFacilityStableId, "area:pyeongchang:town-market", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.WorkerAccessible,
                        Simulation공간능력Codes.DisplayArea,
                    }, workArea, 'b', sourceStableId),
                ScenarioDefinition(PyeongchangSimulation공간StableIds.평창Town마트수령공간,
                    marketFacilityStableId, "area:pyeongchang:town-market", areaSet,
                    new[]
                    {
                        Simulation공간능력Codes.CustomerAccessible,
                        Simulation공간능력Codes.PickupArea,
                    }, workArea, 'd', sourceStableId),
            };
            return new Simulation공간세계InitialStateRequest
            {
                Definitions = hub.Definitions.Concat(definitions).ToArray(),
            };
        }

        public static Simulation공간세계InitialStateRequest Create(
            string facilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
            string sourceStableId = "scenario:pyeongchang-farm-hub-town.v1")
            => new Simulation공간세계InitialStateRequest
            {
                Definitions = new[]
                {
                    new Simulation공간정의InitialRequest
                    {
                        SpatialStableId = facilityStableId == PyeongchangSimulationWorldStableIds.진부Hub시설
                            ? PyeongchangSimulation공간StableIds.진부Hub검수공간
                            : "spatial:scenario:" + facilityStableId + ":inspection",
                        FacilityStableId = facilityStableId,
                        AreaStableId = facilityStableId == PyeongchangSimulationWorldStableIds.진부Hub시설
                            ? PyeongchangSimulationWorldStableIds.진부Hub영역 : string.Empty,
                        AreaSetStableId = facilityStableId == PyeongchangSimulationWorldStableIds.진부Hub시설
                            ? "area-set:pyeongchang-farm-hub-town" : string.Empty,
                        EvidenceKindCode = Simulation공간근거종류Codes.Scenario,
                        AccessStateCode = Simulation공간접근상태Codes.Available,
                        CapabilityCodes = new[]
                        {
                            Simulation공간능력Codes.CargoAccessible,
                            Simulation공간능력Codes.WorkerAccessible,
                            Simulation공간능력Codes.InspectionWorkArea,
                        },
                        BaseCapacities = new[]
                        {
                            new Simulation공간용량Snapshot
                            {
                                CapacityCode = Simulation공간용량Codes.WorkArea,
                                Quantity = 1m,
                                UnitCode = "slot",
                            },
                        },
                        DefinitionRevision = "scenario-spatial-definition.v1",
                        DefinitionHashSha256 = new string('a', 64),
                        SourceStableIds = new[] { sourceStableId },
                    },
                    new Simulation공간정의InitialRequest
                    {
                        SpatialStableId = facilityStableId == PyeongchangSimulationWorldStableIds.진부Hub시설
                            ? PyeongchangSimulation공간StableIds.진부Hub창고공간
                            : "spatial:scenario:" + facilityStableId + ":warehouse",
                        FacilityStableId = facilityStableId,
                        AreaStableId = facilityStableId == PyeongchangSimulationWorldStableIds.진부Hub시설
                            ? PyeongchangSimulationWorldStableIds.진부Hub영역 : string.Empty,
                        AreaSetStableId = facilityStableId == PyeongchangSimulationWorldStableIds.진부Hub시설
                            ? "area-set:pyeongchang-farm-hub-town" : string.Empty,
                        EvidenceKindCode = Simulation공간근거종류Codes.Scenario,
                        AccessStateCode = Simulation공간접근상태Codes.Available,
                        CapabilityCodes = new[]
                        {
                            Simulation공간능력Codes.Storage,
                            Simulation공간능력Codes.CargoAccessible,
                            Simulation공간능력Codes.WorkerAccessible,
                            Simulation공간능력Codes.LoadingWorkArea,
                        },
                        BaseCapacities = new[]
                        {
                            new Simulation공간용량Snapshot
                            {
                                CapacityCode = Simulation공간용량Codes.StorageCapacity,
                                Quantity = 10_000m,
                                UnitCode = "KGM",
                            },
                            new Simulation공간용량Snapshot
                            {
                                CapacityCode = Simulation공간용량Codes.WorkArea,
                                Quantity = 1m,
                                UnitCode = "slot",
                            },
                        },
                        DefinitionRevision = "scenario-spatial-definition.v1",
                        DefinitionHashSha256 = new string('b', 64),
                        SourceStableIds = new[] { sourceStableId },
                    },
                },
            };

        private static Simulation공간정의InitialRequest ScenarioDefinition(
            string spatialStableId,
            string facilityStableId,
            string areaStableId,
            string areaSetStableId,
            string[] capabilities,
            Simulation공간용량Snapshot[] capacities,
            char hashCharacter,
            string sourceStableId)
            => new Simulation공간정의InitialRequest
            {
                SpatialStableId = spatialStableId,
                FacilityStableId = facilityStableId,
                AreaStableId = areaStableId,
                AreaSetStableId = areaSetStableId,
                EvidenceKindCode = Simulation공간근거종류Codes.Scenario,
                AccessStateCode = Simulation공간접근상태Codes.Available,
                CapabilityCodes = capabilities,
                BaseCapacities = capacities.Select(value => new Simulation공간용량Snapshot
                {
                    CapacityCode = value.CapacityCode,
                    Quantity = value.Quantity,
                    UnitCode = value.UnitCode,
                }).ToArray(),
                DefinitionRevision = "scenario-spatial-definition.v1",
                DefinitionHashSha256 = new string(hashCharacter, 64),
                SourceStableIds = new[]
                {
                    sourceStableId,
                    "limitation:scenario-spatial-not-public-data-evidence",
                },
            };
    }
}
