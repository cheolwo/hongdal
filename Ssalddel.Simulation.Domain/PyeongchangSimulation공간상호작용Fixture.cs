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
    }

    public static class PyeongchangSimulation공간상호작용Fixture
    {
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
