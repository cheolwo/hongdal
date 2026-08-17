using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldInteractionFarmSupplyTests
{
    private const string Player = "actor:wi-farm:player";
    private const string FarmFacility = "facility:wi-farm:daegwallyeong";
    private const string MarketFacility = "facility:wi-farm:market";
    private const string CultivationUnit = "cultivation-unit:wi-farm:potato-1";
    private const string PreparationSoil = "soil:wi-farm:preparation-1";
    private const string FarmFence = "defense:wi-farm:fence-1";

    [Fact]
    public void WI_FARM_01_02_03은_밭갈이_파종_재배관리를_예약과상태전이로완료한다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var before = session.Snapshot();
        var tillingPreview = session.PreviewFarmWork(Preview(before.Revision,
            PreparationSoil, SimulationFarmSurvivalCodes.Tilling,
            PyeongchangSimulation공간StableIds.대관령Farm밭갈이공간));

        Assert.True(tillingPreview.CanConfirm);
        Assert.Equal(before.Revision, session.Snapshot().Revision);
        Assert.Contains(Simulation공간능력Codes.TillingWorkArea,
            tillingPreview.SpatialInteraction!.RequiredCapabilityCodes);

        var tilled = RunWork(session, "prepare-till", PreparationSoil,
            SimulationFarmSurvivalCodes.Tilling,
            PyeongchangSimulation공간StableIds.대관령Farm밭갈이공간);
        Assert.Equal(SimulationFarmSurvivalCodes.Tilled,
            tilled.FarmSurvival!.SoilTiles.Single(value =>
                value.SoilTileStableId == PreparationSoil).StateCode);

        var sowingPreview = session.PreviewFarmWork(Preview(tilled.Revision,
            PreparationSoil, SimulationFarmSurvivalCodes.Sowing,
            PyeongchangSimulation공간StableIds.대관령Farm파종공간));
        Assert.True(sowingPreview.CanConfirm);
        Assert.Equal(1m, sowingPreview.SeedCost);
        var sowingConfirmed = session.ConfirmFarmWork(Confirm(
            "command:wi-farm:prepare-sow", tilled.Revision, PreparationSoil,
            SimulationFarmSurvivalCodes.Sowing,
            PyeongchangSimulation공간StableIds.대관령Farm파종공간));
        Assert.Equal(1m, sowingConfirmed.ReservedSeedUnits);
        Assert.Equal(1m, sowingConfirmed.SeedUnits);
        var sown = session.Advance(Tick("command:wi-farm:prepare-sow:tick",
            sowingConfirmed.WorldRevision));
        var cultivation = sown.FarmSurvival!.CultivationUnits.Single(value =>
            value.TileStableId == PreparationSoil);
        Assert.Equal(Simulation재배단위상태Codes.Growing, cultivation.StateCode);
        Assert.Equal(0m, sown.FarmSurvival.ReservedSeedUnits);

        var carePreview = session.PreviewFarmWork(Preview(sown.Revision,
            cultivation.CultivationUnitStableId, SimulationFarmSurvivalCodes.CropCare,
            PyeongchangSimulation공간StableIds.대관령Farm재배관리공간));
        Assert.True(carePreview.CanConfirm);
        Assert.Equal(1m, carePreview.WaterCost);
        var cared = RunWork(session, "prepare-care", cultivation.CultivationUnitStableId,
            SimulationFarmSurvivalCodes.CropCare,
            PyeongchangSimulation공간StableIds.대관령Farm재배관리공간);
        Assert.Equal(Simulation재배단위상태Codes.HarvestReady,
            cared.FarmSurvival!.CultivationUnits.Single(value =>
                value.CultivationUnitStableId == cultivation.CultivationUnitStableId).StateCode);
        Assert.Equal(0m, cared.FarmSurvival.ReservedWaterUnits);
        Assert.All(cared.Tasks.Where(value => value.ActionCode ==
                SimulationFarmSurvivalCodes.Tilling
                || value.ActionCode == SimulationFarmSurvivalCodes.Sowing
                || value.ActionCode == SimulationFarmSurvivalCodes.CropCare),
            value => Assert.Equal(SimulationTaskStateCodes.Completed, value.StateCode));
    }

    [Fact]
    public async Task WI_FARM_01_02_03은_Scenario대체없이_Graph근거공간에서완료한다()
    {
        var graphSpatialWorld = await SimulationWorld상호작용GraphTests
            .CreateP1GraphSpatialWorldAsync();
        var graphDefinition = Assert.Single(graphSpatialWorld.Definitions);
        var request = CreateRequest(graphSpatialWorld);
        request.FarmSurvival!.FarmBuildingStableId = graphDefinition.FacilityStableId;
        request.Settlement!.Facilities.Single(value => value.FacilityStableId == FarmFacility)
            .FacilityStableId = graphDefinition.FacilityStableId;
        var session = new 경영SimulationSessionAggregate(request);
        var spatialStableId = graphDefinition.SpatialStableId;

        var current = RunWork(session, "graph-till", PreparationSoil,
            SimulationFarmSurvivalCodes.Tilling, spatialStableId);
        var sowingConfirmed = session.ConfirmFarmWork(Confirm(
            "command:wi-farm:graph-sow", current.Revision, PreparationSoil,
            SimulationFarmSurvivalCodes.Sowing, spatialStableId));
        current = session.Advance(Tick("command:wi-farm:graph-sow:tick",
            sowingConfirmed.WorldRevision));
        var cultivation = current.FarmSurvival!.CultivationUnits.Single(value =>
            value.TileStableId == PreparationSoil);
        current = RunWork(session, "graph-care", cultivation.CultivationUnitStableId,
            SimulationFarmSurvivalCodes.CropCare, spatialStableId);

        Assert.Equal(Simulation재배단위상태Codes.HarvestReady,
            current.FarmSurvival!.CultivationUnits.Single(value =>
                value.CultivationUnitStableId == cultivation.CultivationUnitStableId).StateCode);
        var spatialDefinition = Assert.Single(current.SpatialDefinitions);
        Assert.Equal(Simulation공간근거종류Codes.LandscapeGraph,
            spatialDefinition.EvidenceKindCode);
        Assert.Equal("farm:700-1145", spatialDefinition.LandscapeNodeStableId);
        Assert.All(current.Tasks.Where(value => value.ActionCode ==
                SimulationFarmSurvivalCodes.Tilling
                || value.ActionCode == SimulationFarmSurvivalCodes.Sowing
                || value.ActionCode == SimulationFarmSurvivalCodes.CropCare),
            value => Assert.Equal(SimulationTaskStateCodes.Completed, value.StateCode));
        var saved = session.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:wi-farm:graph-spatial",
            ExpectedRevision = current.Revision,
        });
        var restored = SimulationSessionReplay.Restore(saved);
        var restoredSave = restored.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = saved.SaveStableId,
            ExpectedRevision = restored.Revision,
        });
        Assert.Equal(saved.ReplayHash, restoredSave.ReplayHash);
        Assert.Equal(Simulation공간근거종류Codes.LandscapeGraph,
            Assert.Single(restored.Snapshot().SpatialDefinitions).EvidenceKindCode);
    }

    [Fact]
    public void 다섯_E4공간모판은_13개_WI_300kg공급선을_SaveReplay까지다시실행한다()
    {
        var spatialWorld = SimulationWorldInteractionSpatialSeedbedTestFixture
            .CreateSpatialWorld();
        var session = new 경영SimulationSessionAggregate(CreateRequest(spatialWorld));

        var tilled = RunWork(session, "seedbed-till", PreparationSoil,
            SimulationFarmSurvivalCodes.Tilling,
            SimulationWorldInteractionSpatialSeedbedTestFixture.ProductionPlot);
        var sown = RunWork(session, "seedbed-sow", PreparationSoil,
            SimulationFarmSurvivalCodes.Sowing,
            SimulationWorldInteractionSpatialSeedbedTestFixture.ProductionPlot);
        var growing = sown.FarmSurvival!.CultivationUnits.Single(value =>
            value.TileStableId == PreparationSoil);
        var cared = RunWork(session, "seedbed-care", growing.CultivationUnitStableId,
            SimulationFarmSurvivalCodes.CropCare,
            SimulationWorldInteractionSpatialSeedbedTestFixture.ProductionPlot);
        Assert.Equal(Simulation재배단위상태Codes.HarvestReady,
            cared.FarmSurvival!.CultivationUnits.Single(value =>
                value.CultivationUnitStableId == growing.CultivationUnitStableId).StateCode);

        var harvested = RunWork(session, "seedbed-harvest", CultivationUnit,
            SimulationFarmSurvivalCodes.Harvesting,
            SimulationWorldInteractionSpatialSeedbedTestFixture.ProductionPlot);
        var harvestLot = Assert.Single(harvested.FarmSurvival!.HarvestLots);
        RunWork(session, "seedbed-collect", harvestLot.HarvestLotStableId,
            SimulationFarmSurvivalCodes.HarvestCollection,
            SimulationWorldInteractionSpatialSeedbedTestFixture.CollectionArea);
        var packed = RunWork(session, "seedbed-pack", harvestLot.HarvestLotStableId,
            SimulationFarmSurvivalCodes.OutboundPacking,
            SimulationWorldInteractionSpatialSeedbedTestFixture.PackingArea);
        var packageLot = Assert.Single(packed.FarmSurvival!.PackageLots);

        var freightRequest = Freight(packageLot);
        freightRequest.Movement.PreferredOriginSpatialStableId =
            SimulationWorldInteractionSpatialSeedbedTestFixture.LoadingArea;
        freightRequest.Movement.PreferredRouteSpatialStableId =
            SimulationWorldInteractionSpatialSeedbedTestFixture.FarmHubCorridor;
        freightRequest.Movement.PreferredDestinationSpatialStableId =
            SimulationWorldInteractionSpatialSeedbedTestFixture.HubUnloading;
        var freightPreview = session.PreviewFreightTransport(freightRequest);
        Assert.Empty(freightPreview.BlockReasonCodes);
        Assert.Equal(packed.Revision, session.Snapshot().Revision);
        var dispatched = session.ConfirmFreightTransport(
            new SimulationFreightTransportConfirmRequest
            {
                CommandId = "command:wi-seedbed:farm-hub",
                ExpectedRevision = packed.Revision,
                Freight = freightRequest,
            });
        var departed = session.Advance(Tick("command:wi-seedbed:depart",
            dispatched.Revision));
        var inTransit = session.Advance(Tick("command:wi-seedbed:route",
            departed.Revision));
        var arrived = session.Advance(Tick("command:wi-seedbed:arrive",
            inTransit.Revision));
        var freight = Assert.Single(arrived.FreightTransports);
        Assert.Equal(화물운송상태코드.하차지도착, freight.StateCode);

        var receipt = new SimulationFreightReceiptPreviewRequest
        {
            TransportRequestStableId = freight.TransportRequestStableId,
            TransportRevision = freight.Revision,
            ActorStableId = PyeongchangSimulationNpcStableIds.진부입고검수담당,
            PreferredSpatialStableId =
                SimulationWorldInteractionSpatialSeedbedTestFixture.HubInspection,
            ReceiptDurationTicks = 1,
            SourceStableIds = new[] { packageLot.CargoStableId },
        };
        Assert.Empty(session.PreviewFreightReceipt(receipt).Decision.BlockReasonCodes);
        var receiptScheduled = session.ConfirmFreightReceipt(
            new SimulationFreightReceiptConfirmRequest
            {
                CommandId = "command:wi-seedbed:receipt",
                ExpectedRevision = arrived.Revision,
                Receipt = receipt,
            });
        var receiptCompleted = AdvanceTicks(session, receiptScheduled, 3,
            "command:wi-seedbed:receipt-tick");
        var inventory = Assert.Single(receiptCompleted.NpcFacilityInventories);
        Assert.Equal(SimulationNpcInventoryStateCodes.StorageEligible, inventory.StateCode);

        var putAway = new SimulationWarehousePutAwayPreviewRequest
        {
            InventoryStableId = inventory.InventoryStableId,
            InventoryRevision = inventory.Revision,
            ActorStableId = PyeongchangSimulationNpcStableIds.진부적재담당,
            PreferredSpatialStableId =
                SimulationWorldInteractionSpatialSeedbedTestFixture.HubStorage,
            PutAwayDurationTicks = 2,
            SourceStableIds = new[] { inventory.InventoryStableId },
        };
        Assert.Empty(session.PreviewWarehousePutAway(putAway).Decision.BlockReasonCodes);
        var putAwayScheduled = session.ConfirmWarehousePutAway(
            new SimulationWarehousePutAwayConfirmRequest
            {
                CommandId = "command:wi-seedbed:put-away",
                ExpectedRevision = receiptCompleted.Revision,
                PutAway = putAway,
            });
        var completed = AdvanceTicks(session, putAwayScheduled, 3,
            "command:wi-seedbed:put-away-tick");

        Assert.Equal(300m, completed.SpatialRuntimeStates.Single(value =>
                value.SpatialStableId ==
                    SimulationWorldInteractionSpatialSeedbedTestFixture.HubStorage)
            .OccupiedCapacities.Single(value =>
                value.CapacityCode == Simulation공간용량Codes.StorageCapacity).Quantity);
        Assert.All(completed.SpatialDefinitions, definition =>
        {
            Assert.Equal(Simulation공간근거종류Codes.Scenario,
                definition.EvidenceKindCode);
            Assert.Contains(definition.SourceStableIds, value =>
                value.StartsWith("wi-spatial-seedbed:", StringComparison.Ordinal));
        });

        var saved = session.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:wi-seedbed:farm-hub",
            ExpectedRevision = completed.Revision,
        });
        var restored = SimulationSessionReplay.Restore(saved);
        var restoredSave = restored.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = saved.SaveStableId,
            ExpectedRevision = restored.Revision,
        });
        Assert.Equal(saved.ReplayHash, restoredSave.ReplayHash);
        Assert.Equal(300m, restored.Snapshot().SpatialRuntimeStates.Single(value =>
                value.SpatialStableId ==
                    SimulationWorldInteractionSpatialSeedbedTestFixture.HubStorage)
            .OccupiedCapacities.Single(value =>
                value.CapacityCode == Simulation공간용량Codes.StorageCapacity).Quantity);
    }

    [Fact]
    public void WI_WORLD_04_시설수리는_수리공간과자재를예약하고_완료후내구도를갱신한다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var before = session.Snapshot();
        var preview = session.PreviewFarmWork(Preview(before.Revision, FarmFence,
            SimulationFarmSurvivalCodes.FenceRepair,
            PyeongchangSimulation공간StableIds.대관령Farm수리공간));

        Assert.True(preview.CanConfirm);
        Assert.Equal(1m, preview.MaterialCost);
        Assert.Contains(Simulation공간능력Codes.RepairWorkArea,
            preview.SpatialInteraction!.RequiredCapabilityCodes);
        Assert.Equal(before.Revision, session.Snapshot().Revision);

        session.ConfirmFarmWork(Confirm(
            "command:wi-world:facility-repair", before.Revision, FarmFence,
            SimulationFarmSurvivalCodes.FenceRepair,
            PyeongchangSimulation공간StableIds.대관령Farm수리공간));
        var confirmed = session.Snapshot();
        Assert.Equal(Simulation공간예약상태Codes.Reserved,
            Assert.Single(confirmed.SpatialReservations).StatusCode);

        var repaired = session.Advance(Tick("command:wi-world:facility-repair:tick",
            confirmed.Revision));
        Assert.Equal(85m, repaired.FarmSurvival!.Defenses.Single(value =>
            value.DefenseStableId == FarmFence).Durability);
        Assert.Equal(Simulation공간예약상태Codes.Released,
            Assert.Single(repaired.SpatialReservations).StatusCode);
        Assert.Contains(repaired.Effects, value =>
            value.EffectTypeCode == "FacilityRepaired");
    }

    [Fact]
    public void WI_FARM_04_05_06은_300kg수확부터_Cargo포장까지_같은계보로완료한다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var before = session.Snapshot();

        var harvestPreview = session.PreviewFarmWork(Preview(
            before.Revision, CultivationUnit, SimulationFarmSurvivalCodes.Harvesting,
            PyeongchangSimulation공간StableIds.대관령Farm수확공간));

        Assert.True(harvestPreview.CanConfirm);
        Assert.Equal(300m, harvestPreview.ProjectedQuantity);
        Assert.Equal("KGM", harvestPreview.ProjectedQuantityUnitCode);
        Assert.Equal(PyeongchangSimulation공간StableIds.대관령Farm수확공간,
            harvestPreview.SpatialInteraction!.SelectedSpatialStableId);
        Assert.Equal(before.Revision, session.Snapshot().Revision);
        Assert.Empty(session.Snapshot().SpatialReservations);

        var harvestConfirmed = session.ConfirmFarmWork(Confirm(
            "command:wi-farm:harvest", before.Revision, CultivationUnit,
            SimulationFarmSurvivalCodes.Harvesting,
            PyeongchangSimulation공간StableIds.대관령Farm수확공간));
        Assert.Equal(Simulation공간예약상태Codes.Reserved,
            Assert.Single(session.Snapshot().SpatialReservations).StatusCode);

        var harvested = session.Advance(Tick("command:wi-farm:harvest:tick",
            harvestConfirmed.WorldRevision));
        var harvestLot = Assert.Single(harvested.FarmSurvival!.HarvestLots);
        Assert.Equal(300m, harvestLot.Quantity);
        Assert.Equal(Simulation수확Lot상태Codes.HarvestedAtField, harvestLot.StateCode);
        Assert.Equal(Simulation재배단위상태Codes.Harvested,
            Assert.Single(harvested.FarmSurvival.CultivationUnits).StateCode);

        var collectionConfirmed = session.ConfirmFarmWork(Confirm(
            "command:wi-farm:collect", harvested.Revision, harvestLot.HarvestLotStableId,
            SimulationFarmSurvivalCodes.HarvestCollection,
            PyeongchangSimulation공간StableIds.대관령Farm집하공간));
        var collected = session.Advance(Tick("command:wi-farm:collect:tick",
            collectionConfirmed.WorldRevision));
        Assert.Equal(Simulation수확Lot상태Codes.CollectedAtYard,
            Assert.Single(collected.FarmSurvival!.HarvestLots).StateCode);

        var packingConfirmed = session.ConfirmFarmWork(Confirm(
            "command:wi-farm:pack", collected.Revision, harvestLot.HarvestLotStableId,
            SimulationFarmSurvivalCodes.OutboundPacking,
            PyeongchangSimulation공간StableIds.대관령Farm포장공간));
        var packed = session.Advance(Tick("command:wi-farm:pack:tick",
            packingConfirmed.WorldRevision));

        var finalHarvestLot = Assert.Single(packed.FarmSurvival!.HarvestLots);
        var packageLot = Assert.Single(packed.FarmSurvival.PackageLots);
        Assert.Equal(Simulation수확Lot상태Codes.PackedForShipment,
            finalHarvestLot.StateCode);
        Assert.Equal(Simulation포장Lot상태Codes.PreparedForShipment,
            packageLot.StateCode);
        Assert.Equal(finalHarvestLot.HarvestLotStableId, packageLot.HarvestLotStableId);
        Assert.Equal(300m, packageLot.Quantity);
        Assert.Equal("allocation:harvest-lot:" + finalHarvestLot.HarvestLotStableId,
            packageLot.SourceAllocationStableId);
        Assert.StartsWith("cargo:", packageLot.CargoStableId, StringComparison.Ordinal);
        Assert.All(packed.Tasks.Where(value => value.TaskTypeCode == "FarmSupplyWork"),
            value => Assert.Equal(SimulationTaskStateCodes.Completed, value.StateCode));
        Assert.All(packed.SpatialReservations,
            value => Assert.Equal(Simulation공간예약상태Codes.Released, value.StatusCode));
    }

    [Fact]
    public void 감자300kg은_Hub출고와마트진열까지_공간예약과상태계보를보존한다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var harvested = RunWork(session, "chain-harvest", CultivationUnit,
            SimulationFarmSurvivalCodes.Harvesting,
            PyeongchangSimulation공간StableIds.대관령Farm수확공간);
        var harvestLotId = Assert.Single(harvested.FarmSurvival!.HarvestLots)
            .HarvestLotStableId;
        RunWork(session, "chain-collect", harvestLotId,
            SimulationFarmSurvivalCodes.HarvestCollection,
            PyeongchangSimulation공간StableIds.대관령Farm집하공간);
        var packed = RunWork(session, "chain-pack", harvestLotId,
            SimulationFarmSurvivalCodes.OutboundPacking,
            PyeongchangSimulation공간StableIds.대관령Farm포장공간);
        var packageLot = Assert.Single(packed.FarmSurvival!.PackageLots);

        var freightRequest = Freight(packageLot);
        var freightPreview = session.PreviewFreightTransport(freightRequest);
        Assert.Empty(freightPreview.BlockReasonCodes);
        Assert.Equal(3, freightPreview.LogisticsMovement.CommonDecisionPreview
            .SpatialInteraction!.RoleBindings.Length);
        Assert.Equal(packed.Revision, session.Snapshot().Revision);

        var freightConfirm = new SimulationFreightTransportConfirmRequest
        {
            CommandId = "command:wi-log:farm-hub",
            ExpectedRevision = packed.Revision,
            Freight = freightRequest,
        };
        var dispatched = session.ConfirmFreightTransport(freightConfirm);
        var dispatchedRetry = session.ConfirmFreightTransport(freightConfirm);
        Assert.Equal(dispatched.Revision, dispatchedRetry.Revision);
        var movementTask = dispatched.Tasks.Single(value =>
            value.TaskTypeCode == "CargoRouteMovement");
        Assert.Equal(3, movementTask.SpatialRoleBindings.Length);
        var movementReservations = dispatched.SpatialReservations.Where(value =>
            value.TaskStableId == movementTask.TaskStableId).ToArray();
        Assert.Equal(2, movementReservations.Length);
        Assert.Contains(movementReservations, value => value.RoleCode ==
            Simulation공간역할Codes.OriginLoading);
        Assert.Contains(movementReservations, value => value.RoleCode ==
            Simulation공간역할Codes.DestinationUnloading);

        var departed = session.Advance(Tick("command:wi-log:depart", dispatched.Revision));
        Assert.Equal(SimulationLogisticsMovementStateCodes.InTransit,
            Assert.Single(departed.LogisticsMovements).StateCode);
        Assert.Equal(Simulation공간예약상태Codes.Released,
            departed.SpatialReservations.Single(value => value.TaskStableId ==
                movementTask.TaskStableId && value.RoleCode ==
                Simulation공간역할Codes.OriginLoading).StatusCode);
        Assert.Equal(Simulation공간예약상태Codes.Reserved,
            departed.SpatialReservations.Single(value => value.TaskStableId ==
                movementTask.TaskStableId && value.RoleCode ==
                Simulation공간역할Codes.DestinationUnloading).StatusCode);

        var inTransit = session.Advance(Tick("command:wi-log:route", departed.Revision));
        var arrived = session.Advance(Tick("command:wi-log:arrive", inTransit.Revision));
        var freight = Assert.Single(arrived.FreightTransports);
        Assert.Equal(화물운송상태코드.하차지도착, freight.StateCode);
        Assert.All(arrived.SpatialReservations.Where(value => value.TaskStableId ==
            movementTask.TaskStableId), value =>
            Assert.Equal(Simulation공간예약상태Codes.Released, value.StatusCode));

        var receipt = new SimulationFreightReceiptPreviewRequest
        {
            TransportRequestStableId = freight.TransportRequestStableId,
            TransportRevision = freight.Revision,
            ActorStableId = PyeongchangSimulationNpcStableIds.진부입고검수담당,
            PreferredSpatialStableId = PyeongchangSimulation공간StableIds.진부Hub검수공간,
            ReceiptDurationTicks = 1,
            SourceStableIds = new[] { packageLot.CargoStableId },
        };
        var receiptPreview = session.PreviewFreightReceipt(receipt);
        Assert.Empty(receiptPreview.Decision.BlockReasonCodes);
        Assert.Equal(PyeongchangSimulation공간StableIds.진부Hub검수공간,
            receiptPreview.SpatialInteraction!.SelectedSpatialStableId);
        var receiptScheduled = session.ConfirmFreightReceipt(
            new SimulationFreightReceiptConfirmRequest
            {
                CommandId = "command:wi-hub:receipt",
                ExpectedRevision = arrived.Revision,
                Receipt = receipt,
            });
        var receiptCompleted = AdvanceTicks(session, receiptScheduled, 3,
            "command:wi-hub:receipt-tick");
        var inventory = Assert.Single(receiptCompleted.NpcFacilityInventories);
        Assert.Equal(SimulationNpcInventoryStateCodes.StorageEligible, inventory.StateCode);
        Assert.Equal(300m, inventory.Quantity);

        var putAway = new SimulationWarehousePutAwayPreviewRequest
        {
            InventoryStableId = inventory.InventoryStableId,
            InventoryRevision = inventory.Revision,
            ActorStableId = PyeongchangSimulationNpcStableIds.진부적재담당,
            PreferredSpatialStableId = PyeongchangSimulation공간StableIds.진부Hub창고공간,
            PutAwayDurationTicks = 2,
            SourceStableIds = new[] { inventory.InventoryStableId },
        };
        var putAwayPreview = session.PreviewWarehousePutAway(putAway);
        Assert.Empty(putAwayPreview.Decision.BlockReasonCodes);
        Assert.Equal(PyeongchangSimulation공간StableIds.진부Hub창고공간,
            putAwayPreview.SpatialInteraction!.SelectedSpatialStableId);
        var putAwayScheduled = session.ConfirmWarehousePutAway(
            new SimulationWarehousePutAwayConfirmRequest
            {
                CommandId = "command:wi-hub:put-away",
                ExpectedRevision = receiptCompleted.Revision,
                PutAway = putAway,
            });
        var completed = AdvanceTicks(session, putAwayScheduled, 3,
            "command:wi-hub:put-away-tick");

        Assert.Equal(SimulationNpcInventoryStateCodes.PutAwayCompleted,
            Assert.Single(completed.NpcFacilityInventories).StateCode);
        var warehouse = completed.SpatialRuntimeStates.Single(value =>
            value.SpatialStableId == PyeongchangSimulation공간StableIds.진부Hub창고공간);
        Assert.Equal(300m, warehouse.OccupiedCapacities.Single(value =>
            value.CapacityCode == Simulation공간용량Codes.StorageCapacity).Quantity);
        Assert.Equal(0m, warehouse.ReservedCapacities.Single(value =>
            value.CapacityCode == Simulation공간용량Codes.StorageCapacity).Quantity);

        inventory = completed.NpcFacilityInventories.Single(value =>
            value.InventoryStableId == inventory.InventoryStableId);
        var outbound = SupplyChainWork(inventory,
            SimulationSupplyChainActionCodes.WarehouseOutboundFlow,
            PyeongchangSimulation공간StableIds.진부Hub피킹공간, 2);
        var outboundPreview = session.PreviewSupplyChainWork(outbound);
        Assert.Empty(outboundPreview.Decision.BlockReasonCodes);
        Assert.Equal(completed.Revision, session.Snapshot().Revision);
        var outboundScheduled = session.ConfirmSupplyChainWork(
            new SimulationSupplyChainWorkConfirmRequest
            {
                CommandId = "command:wi-hub:outbound",
                ExpectedRevision = completed.Revision,
                Work = outbound,
            });
        Assert.Equal(SimulationNpcInventoryStateCodes.OutboundRequested,
            outboundScheduled.NpcFacilityInventories.Single(value =>
                value.InventoryStableId == inventory.InventoryStableId).StateCode);
        var picked = session.Advance(Tick("command:wi-hub:picking-tick",
            outboundScheduled.Revision));
        Assert.Equal(SimulationNpcInventoryStateCodes.Picked,
            picked.NpcFacilityInventories.Single(value =>
                value.InventoryStableId == inventory.InventoryStableId).StateCode);
        var outboundReady = session.Advance(Tick("command:wi-hub:outbound-ready-tick",
            picked.Revision));
        var hubInventory = outboundReady.NpcFacilityInventories.Single(value =>
            value.InventoryStableId == inventory.InventoryStableId);
        Assert.Equal(SimulationNpcInventoryStateCodes.OutboundReady,
            hubInventory.StateCode);
        var outboundAllocation = outboundReady.Settlement!.HarvestLotAllocations
            .Single(value => value.AllocationStableId ==
                "allocation:warehouse-outbound:" + inventory.InventoryStableId);
        Assert.Equal(300m, outboundAllocation.AvailableQuantity);

        var marketFreightRequest = HubMarketFreight(hubInventory, outboundAllocation);
        Assert.Empty(session.PreviewFreightTransport(marketFreightRequest).BlockReasonCodes);
        var marketDispatched = session.ConfirmFreightTransport(
            new SimulationFreightTransportConfirmRequest
            {
                CommandId = "command:wi-market:transport",
                ExpectedRevision = outboundReady.Revision,
                Freight = marketFreightRequest,
            });
        var marketArrived = AdvanceTicks(session, marketDispatched, 3,
            "command:wi-market:transport-tick");
        var marketFreight = marketArrived.FreightTransports.Single(value =>
            value.TransportRequestStableId ==
                marketFreightRequest.Transport.TransportRequestStableId);
        Assert.Equal(화물운송상태코드.하차지도착, marketFreight.StateCode);

        var marketReceipt = new SimulationFreightReceiptPreviewRequest
        {
            TransportRequestStableId = marketFreight.TransportRequestStableId,
            TransportRevision = marketFreight.Revision,
            ActorStableId = PyeongchangSimulationNpcStableIds.진부입고검수담당,
            PreferredSpatialStableId =
                PyeongchangSimulation공간StableIds.평창Town마트하차공간,
            ReceiptDurationTicks = 1,
            SourceStableIds = new[] { marketFreight.CargoStableId },
        };
        Assert.Empty(session.PreviewFreightReceipt(marketReceipt).Decision.BlockReasonCodes);
        var marketReceiptScheduled = session.ConfirmFreightReceipt(
            new SimulationFreightReceiptConfirmRequest
            {
                CommandId = "command:wi-market:receipt",
                ExpectedRevision = marketArrived.Revision,
                Receipt = marketReceipt,
            });
        var marketReceived = session.Advance(Tick("command:wi-market:receipt-tick",
            marketReceiptScheduled.Revision));
        var marketInventory = marketReceived.NpcFacilityInventories.Single(value =>
            value.FacilityStableId == MarketFacility);
        Assert.Equal(SimulationNpcInventoryStateCodes.MarketReceived,
            marketInventory.StateCode);
        Assert.Equal("product:potato", marketInventory.ProductStableId);

        var inspected = RunSupplyChainWork(session, marketInventory,
            SimulationSupplyChainActionCodes.MarketInspection,
            PyeongchangSimulation공간StableIds.평창Town마트검수공간,
            "market-inspection");
        marketInventory = inspected.NpcFacilityInventories.Single(value =>
            value.FacilityStableId == MarketFacility);
        Assert.Equal(SimulationNpcInventoryStateCodes.MarketStorageEligible,
            marketInventory.StateCode);
        var backroom = RunSupplyChainWork(session, marketInventory,
            SimulationSupplyChainActionCodes.MarketBackroomPutAway,
            PyeongchangSimulation공간StableIds.평창Town마트후방공간,
            "market-backroom");
        marketInventory = backroom.NpcFacilityInventories.Single(value =>
            value.FacilityStableId == MarketFacility);
        Assert.Equal(SimulationNpcInventoryStateCodes.MarketBackroomStored,
            marketInventory.StateCode);
        var displayed = RunSupplyChainWork(session, marketInventory,
            SimulationSupplyChainActionCodes.MarketDisplayReplenishment,
            PyeongchangSimulation공간StableIds.평창Town마트진열공간,
            "market-display");
        Assert.Equal(SimulationNpcInventoryStateCodes.Displayed,
            displayed.NpcFacilityInventories.Single(value =>
                value.FacilityStableId == MarketFacility).StateCode);

        var saved = session.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:wi:farm-hub-chain",
            ExpectedRevision = displayed.Revision,
        });
        var restored = SimulationSessionReplay.Restore(saved);
        var restoredSave = restored.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = saved.SaveStableId,
            ExpectedRevision = restored.Revision,
        });
        Assert.Equal(saved.ReplayHash, restoredSave.ReplayHash);
        Assert.Equal(300m, restored.Snapshot().SpatialRuntimeStates.Single(value =>
            value.SpatialStableId == PyeongchangSimulation공간StableIds.진부Hub창고공간)
            .OccupiedCapacities.Single(value => value.CapacityCode ==
                Simulation공간용량Codes.StorageCapacity).Quantity);
    }

    [Fact]
    public async System.Threading.Tasks.Task 감자생산부터_Hub보관까지_시험HTTP경계를왕복한다()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();
        var current = await Post<경영SimulationSessionSnapshot>(client,
            "/api/simulation/v1/sessions", CreateRequest(), HttpStatusCode.Created);

        current = await RunFarmWork(client, current, "http-harvest", CultivationUnit,
            SimulationFarmSurvivalCodes.Harvesting,
            PyeongchangSimulation공간StableIds.대관령Farm수확공간);
        var harvestLotId = Assert.Single(current.FarmSurvival!.HarvestLots)
            .HarvestLotStableId;
        current = await RunFarmWork(client, current, "http-collect", harvestLotId,
            SimulationFarmSurvivalCodes.HarvestCollection,
            PyeongchangSimulation공간StableIds.대관령Farm집하공간);
        current = await RunFarmWork(client, current, "http-pack", harvestLotId,
            SimulationFarmSurvivalCodes.OutboundPacking,
            PyeongchangSimulation공간StableIds.대관령Farm포장공간);
        var packageLot = Assert.Single(current.FarmSurvival!.PackageLots);

        var freight = Freight(packageLot);
        var freightPreview = await Post<SimulationFreightTransportPreviewSnapshot>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}/freight-transport-previews",
            freight);
        Assert.Empty(freightPreview.BlockReasonCodes);
        Assert.Equal(3, freightPreview.LogisticsMovement.CommonDecisionPreview
            .SpatialInteraction!.RoleBindings.Length);
        current = await Post<경영SimulationSessionSnapshot>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}/freight-transports/confirm",
            new SimulationFreightTransportConfirmRequest
            {
                CommandId = "command:wi-http:freight",
                ExpectedRevision = current.Revision,
                Freight = freight,
            });
        current = await TickHttp(client, current, "command:wi-http:freight-tick", 3);
        var arrivedFreight = Assert.Single(current.FreightTransports);

        var receipt = new SimulationFreightReceiptPreviewRequest
        {
            TransportRequestStableId = arrivedFreight.TransportRequestStableId,
            TransportRevision = arrivedFreight.Revision,
            ActorStableId = PyeongchangSimulationNpcStableIds.진부입고검수담당,
            PreferredSpatialStableId = PyeongchangSimulation공간StableIds.진부Hub검수공간,
            ReceiptDurationTicks = 1,
            SourceStableIds = new[] { packageLot.CargoStableId },
        };
        var receiptPreview = await Post<SimulationDecisionPreviewSnapshot>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}/freight-receipt-previews",
            receipt);
        Assert.Empty(receiptPreview.Decision.BlockReasonCodes);
        current = await Post<경영SimulationSessionSnapshot>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}/freight-receipts/confirm",
            new SimulationFreightReceiptConfirmRequest
            {
                CommandId = "command:wi-http:receipt",
                ExpectedRevision = current.Revision,
                Receipt = receipt,
            });
        current = await TickHttp(client, current, "command:wi-http:receipt-tick", 3);
        var inventory = Assert.Single(current.NpcFacilityInventories);

        var putAway = new SimulationWarehousePutAwayPreviewRequest
        {
            InventoryStableId = inventory.InventoryStableId,
            InventoryRevision = inventory.Revision,
            ActorStableId = PyeongchangSimulationNpcStableIds.진부적재담당,
            PreferredSpatialStableId = PyeongchangSimulation공간StableIds.진부Hub창고공간,
            PutAwayDurationTicks = 2,
            SourceStableIds = new[] { inventory.InventoryStableId },
        };
        var putAwayPreview = await Post<SimulationDecisionPreviewSnapshot>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}/warehouse-put-away-previews",
            putAway);
        Assert.Empty(putAwayPreview.Decision.BlockReasonCodes);
        current = await Post<경영SimulationSessionSnapshot>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}/warehouse-put-aways/confirm",
            new SimulationWarehousePutAwayConfirmRequest
            {
                CommandId = "command:wi-http:put-away",
                ExpectedRevision = current.Revision,
                PutAway = putAway,
            });
        current = await TickHttp(client, current, "command:wi-http:put-away-tick", 3);

        Assert.Equal(SimulationNpcInventoryStateCodes.PutAwayCompleted,
            Assert.Single(current.NpcFacilityInventories).StateCode);
        Assert.Equal(300m, current.SpatialRuntimeStates.Single(value =>
            value.SpatialStableId == PyeongchangSimulation공간StableIds.진부Hub창고공간)
            .OccupiedCapacities.Single(value => value.CapacityCode ==
                Simulation공간용량Codes.StorageCapacity).Quantity);

        inventory = current.NpcFacilityInventories.Single(value =>
            value.InventoryStableId == inventory.InventoryStableId);
        var outbound = SupplyChainWork(inventory,
            SimulationSupplyChainActionCodes.WarehouseOutboundFlow,
            PyeongchangSimulation공간StableIds.진부Hub피킹공간, 2);
        var outboundPreview = await Post<SimulationDecisionPreviewSnapshot>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}/supply-chain-work-previews",
            outbound);
        Assert.Empty(outboundPreview.Decision.BlockReasonCodes);
        current = await Post<경영SimulationSessionSnapshot>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}/supply-chain-works/confirm",
            new SimulationSupplyChainWorkConfirmRequest
            {
                CommandId = "command:wi-http:hub-outbound",
                ExpectedRevision = current.Revision,
                Work = outbound,
            });
        current = await TickHttp(client, current,
            "command:wi-http:hub-outbound-tick", 2);
        Assert.Equal(SimulationNpcInventoryStateCodes.OutboundReady,
            current.NpcFacilityInventories.Single(value =>
                value.InventoryStableId == inventory.InventoryStableId).StateCode);

        var save = await Post<SimulationSessionSavePackage>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}/saves",
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:wi:http:farm-hub",
                ExpectedRevision = current.Revision,
            });
        Assert.False(string.IsNullOrWhiteSpace(save.ReplayHash));
        Assert.Equal(current.Revision, save.Snapshot.Revision);
    }

    [Fact]
    public void 수확Preview는_행위자공간능력과_선호공간을_임의대체하지않는다()
    {
        var request = CreateRequest();
        request.FarmSurvival!.Actors[0].CapabilityCodes = Array.Empty<string>();
        var missingActorCapability = new 경영SimulationSessionAggregate(request);

        var actorBlocked = missingActorCapability.PreviewFarmWork(Preview(
            0, CultivationUnit, SimulationFarmSurvivalCodes.Harvesting,
            PyeongchangSimulation공간StableIds.대관령Farm수확공간));

        Assert.False(actorBlocked.CanConfirm);
        Assert.Contains("SimulationFarmActorCapabilityMissing",
            actorBlocked.BlockingReasonCodes);

        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var wrongPreferred = session.PreviewFarmWork(Preview(
            0, CultivationUnit, SimulationFarmSurvivalCodes.Harvesting,
            PyeongchangSimulation공간StableIds.진부Hub검수공간));

        Assert.False(wrongPreferred.CanConfirm);
        Assert.Contains(Simulation공간차단Codes.DefinitionUnavailable,
            wrongPreferred.BlockingReasonCodes);
        Assert.Equal(string.Empty,
            wrongPreferred.SpatialInteraction!.SelectedSpatialStableId);
    }

    [Fact]
    public void 수확작업취소는_자기공간예약과행위자배정만_반환한다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var confirmed = session.ConfirmFarmWork(Confirm(
            "command:wi-farm:harvest-cancel", 0, CultivationUnit,
            SimulationFarmSurvivalCodes.Harvesting,
            PyeongchangSimulation공간StableIds.대관령Farm수확공간));
        var task = confirmed.WorkOrders.Single().WorkOrderStableId;

        var cancelled = session.CancelTask(task, new SimulationTaskCancelRequest
        {
            CommandId = "command:wi-farm:harvest-cancel:cancel",
            ExpectedRevision = confirmed.WorldRevision,
            ReasonCode = "PlayerChangedPlan",
        });

        Assert.Equal(SimulationTaskStateCodes.Cancelled,
            cancelled.Tasks.Single(value => value.TaskStableId == task).StateCode);
        Assert.Equal(SimulationTaskStateCodes.Cancelled,
            cancelled.FarmSurvival!.WorkOrders.Single().StatusCode);
        Assert.Equal(string.Empty, cancelled.FarmSurvival.Actors.Single().ActiveWorkOrderStableId);
        Assert.Equal(Simulation공간예약상태Codes.Cancelled,
            Assert.Single(cancelled.SpatialReservations).StatusCode);
        Assert.Equal(0m, cancelled.SpatialRuntimeStates.Single(value =>
            value.SpatialStableId == PyeongchangSimulation공간StableIds.대관령Farm수확공간)
            .ReservedCapacities.Single().Quantity);
    }

    [Fact]
    public void 수확확정은_명령멱등성과_예상개정충돌을보존한다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var request = Confirm("command:wi-farm:idempotent", 0, CultivationUnit,
            SimulationFarmSurvivalCodes.Harvesting,
            PyeongchangSimulation공간StableIds.대관령Farm수확공간);

        var first = session.ConfirmFarmWork(request);
        var retried = session.ConfirmFarmWork(request);
        Assert.Equal(first.WorldRevision, retried.WorldRevision);
        Assert.Single(session.Snapshot().Tasks);
        Assert.Single(session.Snapshot().SpatialReservations);

        var payloadConflict = Assert.Throws<SimulationConflictException>(() =>
            session.ConfirmFarmWork(Confirm("command:wi-farm:idempotent", 0,
                CultivationUnit, SimulationFarmSurvivalCodes.Harvesting,
                PyeongchangSimulation공간StableIds.대관령Farm집하공간)));
        Assert.Equal("SimulationCommandPayloadConflict", payloadConflict.ErrorCode);

        var revisionConflict = Assert.Throws<SimulationConflictException>(() =>
            session.ConfirmFarmWork(Confirm("command:wi-farm:stale", 0,
                CultivationUnit, SimulationFarmSurvivalCodes.Harvesting,
                PyeongchangSimulation공간StableIds.대관령Farm수확공간)));
        Assert.Equal("SimulationExpectedRevisionMismatch", revisionConflict.ErrorCode);
    }

    [Fact]
    public void 수확집하포장_SaveReplay는_같은상태와hash를재현한다()
    {
        var session = new 경영SimulationSessionAggregate(CreateRequest());
        var harvest = RunWork(session, "save-harvest", CultivationUnit,
            SimulationFarmSurvivalCodes.Harvesting,
            PyeongchangSimulation공간StableIds.대관령Farm수확공간);
        var harvestLotId = Assert.Single(harvest.FarmSurvival!.HarvestLots).HarvestLotStableId;
        RunWork(session, "save-collect", harvestLotId,
            SimulationFarmSurvivalCodes.HarvestCollection,
            PyeongchangSimulation공간StableIds.대관령Farm집하공간);
        var completed = RunWork(session, "save-pack", harvestLotId,
            SimulationFarmSurvivalCodes.OutboundPacking,
            PyeongchangSimulation공간StableIds.대관령Farm포장공간);
        var saved = session.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = "save:wi-farm:supply",
            ExpectedRevision = completed.Revision,
        });

        var restored = SimulationSessionReplay.Restore(saved);
        var restoredSave = restored.CreateSavePackage(new SimulationSessionSaveRequest
        {
            SaveStableId = saved.SaveStableId,
            ExpectedRevision = restored.Revision,
        });

        Assert.Equal(saved.ReplayHash, restoredSave.ReplayHash);
        Assert.Equal(300m,
            Assert.Single(restored.Snapshot().FarmSurvival!.PackageLots).Quantity);
    }

    private static 경영SimulationSessionSnapshot RunWork(
        경영SimulationSessionAggregate session,
        string commandSuffix,
        string targetStableId,
        string actionCode,
        string spatialStableId)
    {
        var current = session.Snapshot();
        var confirmed = session.ConfirmFarmWork(Confirm(
            "command:wi-farm:" + commandSuffix, current.Revision, targetStableId,
            actionCode, spatialStableId));
        return session.Advance(Tick("command:wi-farm:" + commandSuffix + ":tick",
            confirmed.WorldRevision));
    }

    private static 경영SimulationSessionSnapshot AdvanceTicks(
        경영SimulationSessionAggregate session,
        경영SimulationSessionSnapshot current,
        int count,
        string commandPrefix)
    {
        for (var index = 1; index <= count; index++)
        {
            current = session.Advance(Tick(commandPrefix + ":" + index,
                current.Revision));
        }
        return current;
    }

    private static async System.Threading.Tasks.Task<경영SimulationSessionSnapshot> RunFarmWork(
        HttpClient client,
        경영SimulationSessionSnapshot current,
        string commandSuffix,
        string targetStableId,
        string actionCode,
        string spatialStableId)
    {
        var preview = Preview(current.Revision, targetStableId, actionCode,
            spatialStableId);
        var previewResult = await Post<SimulationFarmWorkPreviewSnapshot>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}/farm-survival/work/preview",
            preview);
        Assert.True(previewResult.CanConfirm);
        await Post<SimulationFarmSurvivalStateSnapshot>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}/farm-survival/work/confirm",
            Confirm("command:wi-http:" + commandSuffix, current.Revision, targetStableId,
                actionCode, spatialStableId));
        current = await Get<경영SimulationSessionSnapshot>(client,
            $"/api/simulation/v1/sessions/{current.SessionStableId}");
        return await TickHttp(client, current,
            "command:wi-http:" + commandSuffix + ":tick", 1);
    }

    private static async System.Threading.Tasks.Task<경영SimulationSessionSnapshot> TickHttp(
        HttpClient client,
        경영SimulationSessionSnapshot current,
        string commandPrefix,
        int count)
    {
        for (var index = 1; index <= count; index++)
        {
            current = await Post<경영SimulationSessionSnapshot>(client,
                $"/api/simulation/v1/sessions/{current.SessionStableId}/ticks",
                Tick(commandPrefix + ":" + index, current.Revision));
        }
        return current;
    }

    private static async System.Threading.Tasks.Task<T> Post<T>(
        HttpClient client,
        string route,
        object request,
        HttpStatusCode expectedStatus = HttpStatusCode.OK)
    {
        using var response = await client.PostAsJsonAsync(route, request);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == expectedStatus,
            $"Expected {expectedStatus} but received {response.StatusCode}: {body}");
        var result = JsonSerializer.Deserialize<T>(body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return Assert.IsType<T>(result);
    }

    private static async System.Threading.Tasks.Task<T> Get<T>(
        HttpClient client,
        string route)
    {
        using var response = await client.GetAsync(route);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode,
            $"GET {route} failed with {response.StatusCode}: {body}");
        var result = JsonSerializer.Deserialize<T>(body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        return Assert.IsType<T>(result);
    }

    private static WebApplicationFactory<Program> CreateFactory()
        => new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["SsalddelExecution:Mode"] = "Simulation",
                            ["SimulationServer:Enabled"] = "true",
                            ["SimulationSharedPublicData:Enabled"] = "false",
                        });
                });
            });

    private static SimulationFreightTransportPreviewRequest Freight(
        Simulation포장LotSnapshot packageLot)
        => new()
        {
            Transport = new SimulationFreightTransportBindingRequest
            {
                TransportRequestStableId = "freight-transport:wi:farm-hub:potato-1",
                DispatchOfferStableId = "dispatch-offer:wi:farm-hub:potato-1",
                CarrierCandidateStableId = "carrier-candidate:wi:cooperative-1",
                VehicleStableId = "vehicle:wi:truck-1",
                VehicleCapacity = 400m,
                VehicleCapacityUnitCode = "KGM",
            },
            Movement = new SimulationLogisticsMovementPreviewRequest
            {
                CargoStableId = packageLot.CargoStableId,
                CargoRevision = 1,
                SourceAllocationStableId = packageLot.SourceAllocationStableId,
                HarvestLotStableId = packageLot.HarvestLotStableId,
                PackageLotStableId = packageLot.PackageLotStableId,
                ProductStableId = "product:potato",
                Quantity = packageLot.Quantity,
                UnitCode = packageLot.UnitCode,
                RouteStableId = "route:wi:farm-hub",
                OriginFacilityStableId = FarmFacility,
                DestinationFacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                ActorStableId = Player,
                PreferredOriginSpatialStableId =
                    PyeongchangSimulation공간StableIds.대관령Farm상차공간,
                PreferredRouteSpatialStableId =
                    PyeongchangSimulation공간StableIds.FarmHub운송회랑,
                PreferredDestinationSpatialStableId =
                    PyeongchangSimulation공간StableIds.진부Hub하차공간,
                RequiredRouteTicks = 3,
                SourceStableIds = new[] { packageLot.PackageLotStableId },
            },
        };

    private static SimulationSupplyChainWorkPreviewRequest SupplyChainWork(
        SimulationNpcFacilityInventorySnapshot inventory,
        string actionCode,
        string spatialStableId,
        int durationTicks = 1)
        => new()
        {
            InventoryStableId = inventory.InventoryStableId,
            InventoryRevision = inventory.Revision,
            ActionCode = actionCode,
            ActorStableId = PyeongchangSimulationNpcStableIds.진부적재담당,
            PreferredSpatialStableId = spatialStableId,
            DurationTicks = durationTicks,
            SourceStableIds = new[] { inventory.InventoryStableId },
        };

    private static 경영SimulationSessionSnapshot RunSupplyChainWork(
        경영SimulationSessionAggregate session,
        SimulationNpcFacilityInventorySnapshot inventory,
        string actionCode,
        string spatialStableId,
        string commandSuffix)
    {
        var current = session.Snapshot();
        var request = SupplyChainWork(inventory, actionCode, spatialStableId);
        Assert.Empty(session.PreviewSupplyChainWork(request).Decision.BlockReasonCodes);
        var confirmed = session.ConfirmSupplyChainWork(
            new SimulationSupplyChainWorkConfirmRequest
            {
                CommandId = "command:wi:" + commandSuffix,
                ExpectedRevision = current.Revision,
                Work = request,
            });
        return session.Advance(Tick("command:wi:" + commandSuffix + ":tick",
            confirmed.Revision));
    }

    private static SimulationFreightTransportPreviewRequest HubMarketFreight(
        SimulationNpcFacilityInventorySnapshot inventory,
        SimulationHarvestLotAllocationSnapshot allocation)
        => new()
        {
            Transport = new SimulationFreightTransportBindingRequest
            {
                TransportRequestStableId = "freight-transport:wi:hub-market:potato-1",
                DispatchOfferStableId = "dispatch-offer:wi:hub-market:potato-1",
                CarrierCandidateStableId = "carrier-candidate:wi:market-1",
                VehicleStableId = "vehicle:wi:market-truck-1",
                VehicleCapacity = 400m,
                VehicleCapacityUnitCode = inventory.UnitCode,
            },
            Movement = new SimulationLogisticsMovementPreviewRequest
            {
                CargoStableId = "cargo:wi:hub-market:potato-1",
                CargoRevision = 1,
                SourceAllocationStableId = allocation.AllocationStableId,
                HarvestLotStableId = allocation.HarvestLotStableId,
                PackageLotStableId = "package-lot:wi:hub-market:potato-1",
                ProductStableId = inventory.ProductStableId,
                Quantity = inventory.Quantity,
                UnitCode = inventory.UnitCode,
                RouteStableId = "route:wi:hub-market",
                OriginFacilityStableId = inventory.FacilityStableId,
                DestinationFacilityStableId = MarketFacility,
                ActorStableId = Player,
                PreferredOriginSpatialStableId =
                    PyeongchangSimulation공간StableIds.진부Hub출고상차공간,
                PreferredRouteSpatialStableId =
                    PyeongchangSimulation공간StableIds.HubTown운송회랑,
                PreferredDestinationSpatialStableId =
                    PyeongchangSimulation공간StableIds.평창Town마트하차공간,
                RequiredRouteTicks = 3,
                SourceStableIds = new[] { inventory.InventoryStableId },
            },
        };

    private static SimulationFarmWorkPreviewRequest Preview(
        long revision,
        string targetStableId,
        string actionCode,
        string spatialStableId)
        => new()
        {
            ExpectedRevision = revision,
            ActorStableId = Player,
            TargetStableId = targetStableId,
            ActionCode = actionCode,
            AssignmentKindCode = SimulationFarmSurvivalCodes.PlayerDirect,
            PreferredSpatialStableId = spatialStableId,
        };

    private static SimulationFarmWorkConfirmRequest Confirm(
        string commandId,
        long revision,
        string targetStableId,
        string actionCode,
        string spatialStableId)
        => new()
        {
            CommandId = commandId,
            ExpectedRevision = revision,
            ActorStableId = Player,
            TargetStableId = targetStableId,
            ActionCode = actionCode,
            AssignmentKindCode = SimulationFarmSurvivalCodes.PlayerDirect,
            PreferredSpatialStableId = spatialStableId,
        };

    private static 경영SimulationTick진행Request Tick(string commandId, long revision)
        => new()
        {
            CommandId = commandId,
            ExpectedRevision = revision,
            TickCount = 1,
        };

    private static 경영SimulationSession생성Request CreateRequest(
        Simulation공간세계InitialStateRequest? spatialWorld = null)
        => new()
        {
            ClientRequestId = Guid.Parse("4D73AB1E-7E22-4BE5-B638-807CB45C2AA1"),
            ScenarioStableId = "scenario:wi-farm-supply",
            ScenarioDataRevision = "fixture.r1",
            ScenarioSeed = 300,
            RuleRevision = "world-interaction.farm-supply.r1",
            DurationTicks = 30,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:wi-farm",
                TerritoryStableId = "territory:pyeongchang",
                SettlementStableId = "settlement:pyeongchang",
                GameDateStartsOn = new DateTimeOffset(2026, 8, 17, 0, 0, 0,
                    TimeSpan.Zero),
            },
            Settlement = new SimulationSettlementInitialStateRequest
            {
                TreasuryBalance = 1_000_000m,
                CurrencyCode = "KRW",
                LaborCapacityTotal = 100m,
                StorageCapacity = 20_000m,
                StorageUnitCode = "KGM",
                PopulationCount = 100,
                PopulationFoodDemandPerTick = 100m,
                FoodEquivalentUnitCode = "KGM",
                FoodEquivalentRuleRevision = "food-equivalent:wi.r1",
                Districts = new[]
                {
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:wi-farm",
                        DistrictTypeCode = "Farm",
                        SourceStableIds = new[] { "source:scenario.wi-farm" },
                    },
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:wi-hub",
                        DistrictTypeCode = "Logistics",
                        SourceStableIds = new[] { "source:scenario.wi-farm" },
                    },
                },
                Facilities = new[]
                {
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = FarmFacility,
                        FacilityTypeCode = "FarmPacking",
                        DistrictStableId = "district:wi-farm",
                        SourceStableIds = new[] { "source:scenario.wi-farm" },
                    },
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = PyeongchangSimulationWorldStableIds.진부Hub시설,
                        FacilityTypeCode = "LogisticsHub",
                        DistrictStableId = "district:wi-hub",
                        SourceStableIds = new[] { "source:scenario.wi-farm" },
                    },
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:wi-farm:storage",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Storage,
                        DistrictStableId = "district:wi-hub",
                        SourceStableIds = new[] { "source:scenario.wi-farm" },
                    },
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:wi-farm:market",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Market,
                        DistrictStableId = "district:wi-hub",
                        SourceStableIds = new[] { "source:scenario.wi-farm" },
                    },
                },
                SourceStableIds = new[] { "source:scenario.wi-farm" },
            },
            SpatialWorld = spatialWorld
                ?? PyeongchangSimulation공간상호작용Fixture.CreateFarmHubSupply(
                    FarmFacility, MarketFacility),
            NpcWorkforce = PyeongchangSimulationNpcWorkforceFixture.Create(),
            FarmSurvival = new SimulationFarmSurvivalInitialStateRequest
            {
                RuleRevision = SimulationFarmSurvivalCodes.ScenicSeasonRuleRevision,
                RegionStableId = "region:legal-dong:5176031000",
                AreaStableId = "area:pyeongchang:daegwallyeong-farm",
                TileKey = "kr5186:l2:700:1145",
                FarmBuildingStableId = FarmFacility,
                SupplyUnits = 8m,
                RepairMaterialUnits = 4m,
                SeedUnits = 2m,
                WaterUnits = 2m,
                Actors = new[]
                {
                    new SimulationFarmActorInitialStateRequest
                    {
                        ActorStableId = Player,
                        ActorKindCode = SimulationFarmSurvivalCodes.Player,
                        KoreanName = "공급선 농장 작업자",
                        CapabilityCodes = new[]
                        {
                            SimulationFarmActorCapabilityCodes.FarmHarvest,
                            SimulationFarmActorCapabilityCodes.FarmCollection,
                            SimulationFarmActorCapabilityCodes.FarmPacking,
                            SimulationFarmActorCapabilityCodes.FarmTilling,
                            SimulationFarmActorCapabilityCodes.FarmSowing,
                            SimulationFarmActorCapabilityCodes.FarmCropCare,
                        },
                    },
                },
                SoilTiles = new[]
                {
                    new SimulationFarmSoilTileInitialStateRequest
                    {
                        SoilTileStableId = PreparationSoil,
                        GridX = 0,
                        GridY = 0,
                        StateCode = SimulationFarmSurvivalCodes.Untilled,
                        PhysicalAreaSquareMeters = 100m,
                    },
                },
                CultivationUnits = new[]
                {
                    new Simulation재배단위Snapshot
                    {
                        CultivationUnitStableId = CultivationUnit,
                        Revision = 1,
                        TileStableId = "tile:wi-farm:potato-1",
                        CultivationStableId = "cultivation:wi-farm:potato",
                        ProductStableId = "product:potato",
                        CropVariantStableId = "crop-variant:potato.fixture",
                        StateCode = Simulation재배단위상태Codes.HarvestReady,
                        PhysicalAreaSquareMeters = 100m,
                        EffectiveCultivationAreaRatio = 1m,
                        SourceStableIds = new[] { "source:scenario.cultivation-unit" },
                    },
                },
                Defenses = new[]
                {
                    new SimulationFarmDefenseInitialStateRequest
                    {
                        DefenseStableId = FarmFence,
                        DefenseKindCode = SimulationFarmSurvivalCodes.Fence,
                        Durability = 60m,
                    },
                },
                PotatoProductionRule = new Simulation감자생산RuleSnapshot
                {
                    RuleStableId = "rule:potato-production.fixture.v1",
                    RuleRevision = 1,
                    SourceTypeCode = Simulation생산규칙SourceTypeCodes.Fixture,
                    ProductStableId = "product:potato",
                    CropVariantStableId = "crop-variant:potato.fixture",
                    BaseYieldKilogramsPerSquareMeter = 3m,
                    MinimumEnvironmentFactor = 0.5m,
                    MaximumEnvironmentFactor = 1m,
                    MinimumInputFactor = 0.8m,
                    MaximumInputFactor = 1.2m,
                    MinimumFacilityFactor = 0.8m,
                    MaximumFacilityFactor = 1.2m,
                    MinimumLossFactor = 0.1m,
                    MaximumLossFactor = 1m,
                    SourceStableIds = new[] { "source:fixture.potato-yield-rule" },
                    Limitations = new[] { "실제 생산량 또는 운영 수확량으로 사용하지 않는다." },
                },
            },
        };
}
