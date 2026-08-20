using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public static class H5IntegratedWorldScenarioFixture
    {
        public static SimulationIntegratedWorldInitialStateRequest Create()
            => new()
            {
                ScenarioRevision = "h5-integrated-world.r1",
                ScenarioHashSha256 = "sha256:h5-integrated-world-r1",
                FacilityDefinitions = new[]
                {
                    Definition("facility-definition:farm-warehouse", "Warehouse",
                        SimulationIntegratedCapabilityCodes.Storage,
                        SimulationIntegratedCapabilityCodes.CargoAccessible,
                        SimulationIntegratedCapabilityCodes.LoadingWorkArea,
                        SimulationIntegratedCapabilityCodes.WorkerAccessible),
                    Definition("facility-definition:hub-workshop", "ManufacturingWorkshop",
                        SimulationIntegratedCapabilityCodes.ManufacturingWorkArea,
                        SimulationIntegratedCapabilityCodes.FinishedGoodsInspectionArea,
                        SimulationIntegratedCapabilityCodes.CargoAccessible,
                        SimulationIntegratedCapabilityCodes.LoadingWorkArea,
                        SimulationIntegratedCapabilityCodes.WorkerAccessible),
                    Definition("facility-definition:barracks", "Barracks",
                        SimulationIntegratedCapabilityCodes.Recruitment,
                        SimulationIntegratedCapabilityCodes.Training,
                        SimulationIntegratedCapabilityCodes.Garrison,
                        SimulationIntegratedCapabilityCodes.WorkerAccessible),
                    Definition("facility-definition:town-market", "Market",
                        SimulationIntegratedCapabilityCodes.Storage,
                        SimulationIntegratedCapabilityCodes.CargoAccessible,
                        SimulationIntegratedCapabilityCodes.WorkerAccessible),
                },
                FacilitySeeds = new[]
                {
                    Seed("facility:farm:warehouse", "facility-definition:farm-warehouse",
                        "h1:Farm:warehouse", "connector:FarmGate"),
                    Seed("facility:hub:workshop", "facility-definition:hub-workshop",
                        "h1:Hub:manufacturing", "connector:HubInbound", "connector:HubOutbound"),
                    Seed("facility:town:market", "facility-definition:town-market",
                        "h1:Town:market", "connector:TownReceiving"),
                },
                Actors = Enumerable.Range(1, 12).Select(index =>
                    new SimulationIntegratedActorSeedRequest
                    {
                        ActorStableId = "actor:farm:" + index.ToString("D2"),
                        EligibilityRank = index,
                        FarmLaborEligible = true,
                    }).ToArray(),
                Lots = new[]
                {
                    Lot("lot:hub:raw-material", SimulationIntegratedItemCodes.ManufacturingRawMaterial,
                        30m, "unit", "facility:hub:workshop"),
                    Lot("lot:farm:harvest-potato", SimulationIntegratedItemCodes.HarvestPotato,
                        300m, "kg", "facility:farm:warehouse"),
                },
                ManufacturingRecipes = new[]
                {
                    Recipe("recipe:transport-box", 2m,
                        SimulationIntegratedItemCodes.TransportBox, 5m),
                    Recipe("recipe:facility-component", 3m,
                        SimulationIntegratedItemCodes.FacilityComponent, 3m),
                },
                FacilityBlueprints = new[]
                {
                    new SimulationFacilityBlueprintRequest
                    {
                        BlueprintStableId = "blueprint:barracks",
                        Revision = "r1",
                        HashSha256 = "sha256:blueprint-barracks-r1",
                        FacilityDefinitionStableId = "facility-definition:barracks",
                        ConstructionTicks = 2,
                        Materials = new[]
                        {
                            new SimulationIntegratedItemRequirement
                            {
                                ItemCode = SimulationIntegratedItemCodes.FacilityComponent,
                                Quantity = 2m,
                                UnitCode = "unit",
                            },
                        },
                    },
                },
            };

        private static SimulationFacilityDefinitionRequest Definition(string id, string type,
            params string[] capabilities) => new()
        {
            FacilityDefinitionStableId = id,
            Revision = "r1",
            HashSha256 = "sha256:" + id + ":r1",
            FacilityTypeCode = type,
            CapabilityCodes = capabilities,
        };

        private static SimulationScenarioFacilitySeedRequest Seed(string id, string definition,
            string h1, params string[] connectors) => new()
        {
            FacilityStableId = id,
            FacilityDefinitionStableId = definition,
            PlacementH1StableId = h1,
            AccessConnectorStableIds = connectors,
        };

        private static SimulationIntegratedLotSeedRequest Lot(string id, string item,
            decimal quantity, string unit, string facility) => new()
        {
            LotStableId = id,
            ItemCode = item,
            Quantity = quantity,
            UnitCode = unit,
            FacilityStableId = facility,
        };

        private static SimulationManufacturingRecipeRequest Recipe(string id, decimal input,
            string outputItem, decimal output) => new()
        {
            RecipeStableId = id,
            Revision = "r1",
            HashSha256 = "sha256:" + id + ":r1",
            ProcessingTicks = 1,
            Inputs = new[]
            {
                new SimulationIntegratedItemRequirement
                {
                    ItemCode = SimulationIntegratedItemCodes.ManufacturingRawMaterial,
                    Quantity = input,
                    UnitCode = "unit",
                },
            },
            Outputs = new[]
            {
                new SimulationIntegratedItemRequirement
                {
                    ItemCode = outputItem,
                    Quantity = output,
                    UnitCode = "unit",
                },
            },
        };
    }
}
