using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationLogisticsMovementTests
{
    [Fact]
    public void Preview는_Cargo와Lot계보를보존하지만_재고를예약하지않는다()
    {
        var context = ReadySource();

        var preview = context.Service.PreviewLogisticsMovement(
            context.Session.SessionStableId,
            Movement());
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.True(preview.IsCandidateOnly);
        Assert.True(preview.DoesNotApplySettlementState);
        Assert.Equal("cargo:sim.potato-1", preview.CargoStableId);
        Assert.Equal(3, preview.RequiredRouteTicks);
        Assert.Contains("VehicleAnimationIsPresentationOnly", preview.BoundaryCodes);
        Assert.Empty(current.LogisticsMovements);
        Assert.Equal(300m, Assert.Single(current.Settlement!.HarvestLotAllocations).AvailableQuantity);
    }

    [Fact]
    public void Confirm은_같은Cargo를예약하고_공통Task를생성한다()
    {
        var context = ReadySource();

        var confirmed = context.Service.ConfirmLogisticsMovement(
            context.Session.SessionStableId,
            Confirm(context.Session.Revision));

        var movement = Assert.Single(confirmed.LogisticsMovements);
        var allocation = Assert.Single(confirmed.Settlement!.HarvestLotAllocations);
        Assert.Equal(SimulationLogisticsMovementStateCodes.Reserved, movement.StateCode);
        Assert.Equal("harvest-lot:potato-1", movement.HarvestLotStableId);
        Assert.Equal("package-lot:potato-1", movement.PackageLotStableId);
        Assert.Equal(300m, movement.ReservedQuantity);
        Assert.Equal(300m, allocation.OutboundReservedQuantity);
        Assert.Equal(0m, allocation.AvailableQuantity);
        Assert.Equal(
            SimulationTaskStateCodes.Scheduled,
            confirmed.Tasks.Single(value => value.TaskStableId == movement.TaskStableId).StateCode);
    }

    [Fact]
    public void WorldTick은_같은Cargo를_출발_진행_도착시키고_예약량을보존한다()
    {
        var context = ReadySource();
        var confirmed = context.Service.ConfirmLogisticsMovement(
            context.Session.SessionStableId,
            Confirm(context.Session.Revision));

        var departed = Advance(context, confirmed, "command:tick.logistics-1", 1);
        var inTransit = Assert.Single(departed.LogisticsMovements);
        Assert.Equal("cargo:sim.potato-1", inTransit.CargoStableId);
        Assert.Equal(SimulationLogisticsMovementStateCodes.InTransit, inTransit.StateCode);
        Assert.Equal(1, inTransit.CompletedRouteTicks);
        Assert.Equal(300m, inTransit.ReservedQuantity);

        var progressing = Advance(context, departed, "command:tick.logistics-2", 1);
        var progress = Assert.Single(progressing.LogisticsMovements);
        Assert.Equal("cargo:sim.potato-1", progress.CargoStableId);
        Assert.Equal(2, progress.CompletedRouteTicks);

        var arrived = Advance(context, progressing, "command:tick.logistics-3", 1);
        var destination = Assert.Single(arrived.LogisticsMovements);
        Assert.Equal("cargo:sim.potato-1", destination.CargoStableId);
        Assert.Equal(SimulationLogisticsMovementStateCodes.ArrivedAtDestination, destination.StateCode);
        Assert.Equal(3, destination.CompletedRouteTicks);
        Assert.Equal(300m, destination.ReservedQuantity);
        Assert.Equal("stock-candidate:arrival:cargo:sim.potato-1", destination.DestinationStockCandidateStableId);
        Assert.Equal(
            SimulationTaskStateCodes.Completed,
            arrived.Tasks.Single(value => value.TaskStableId == destination.TaskStableId).StateCode);
    }

    [Fact]
    public void 다른Lot계보는_Preview에서차단되고_Confirm할수없다()
    {
        var context = ReadySource();
        var request = Movement();
        request.HarvestLotStableId = "harvest-lot:potato-other";

        var preview = context.Service.PreviewLogisticsMovement(
            context.Session.SessionStableId,
            request);
        var error = Assert.Throws<SimulationConflictException>(() =>
            context.Service.ConfirmLogisticsMovement(
                context.Session.SessionStableId,
                new SimulationLogisticsMovementConfirmRequest
                {
                    CommandId = "command:logistics.confirm-mismatch",
                    ExpectedRevision = context.Session.Revision,
                    Movement = request,
                }));

        Assert.Contains("SourceAllocationLineageMismatch", preview.CommonDecisionPreview.Decision.BlockReasonCodes);
        Assert.Equal("SimulationDecisionPreviewBlocked", error.ErrorCode);
        Assert.Empty(context.Service.Get(context.Session.SessionStableId).LogisticsMovements);
    }

    [Fact]
    public void SaveReplay는_이동중Cargo와_재고예약을동일하게복원한다()
    {
        var context = ReadySource();
        var confirmed = context.Service.ConfirmLogisticsMovement(
            context.Session.SessionStableId,
            Confirm(context.Session.Revision));
        var moving = Advance(context, confirmed, "command:tick.logistics-save", 1);
        var package = context.Service.Save(
            context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.logistics-1",
                ExpectedRevision = moving.Revision,
            });

        var restoreService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            context.SaveStore);
        var restored = restoreService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        var movement = Assert.Single(restored.Session.LogisticsMovements);
        Assert.Equal(SimulationLogisticsMovementStateCodes.InTransit, movement.StateCode);
        Assert.Equal(1, movement.CompletedRouteTicks);
        Assert.Equal(300m, movement.ReservedQuantity);
        Assert.Equal(0m, Assert.Single(restored.Session.Settlement!.HarvestLotAllocations).AvailableQuantity);
    }

    private static 경영SimulationSessionSnapshot Advance(
        Context context,
        경영SimulationSessionSnapshot current,
        string commandId,
        int ticks)
        => context.Service.Advance(
            context.Session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = commandId,
                ExpectedRevision = current.Revision,
                TickCount = ticks,
            });

    private static Context ReadySource()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            saveStore);
        var created = service.Create(CreateRequest());
        var impact = service.ConfirmHarvestDispositionImpact(
            created.SessionStableId,
            new SimulationHarvestDispositionImpactConfirmRequest
            {
                CommandId = "command:harvest.coop-1",
                ExpectedRevision = created.Revision,
                Impact = HarvestImpact(),
            });
        var ready = service.Advance(
            created.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:tick.harvest.coop-ready",
                ExpectedRevision = impact.Revision,
                TickCount = 2,
            });
        return new Context(service, saveStore, ready);
    }

    private static SimulationLogisticsMovementConfirmRequest Confirm(long revision)
        => new()
        {
            CommandId = "command:logistics.confirm-potato-1",
            ExpectedRevision = revision,
            Movement = Movement(),
        };

    private static SimulationLogisticsMovementPreviewRequest Movement()
        => new()
        {
            CargoStableId = "cargo:sim.potato-1",
            CargoRevision = 1,
            SourceAllocationStableId = "allocation:harvest-lot:harvest-lot:potato-1",
            HarvestLotStableId = "harvest-lot:potato-1",
            PackageLotStableId = "package-lot:potato-1",
            ProductStableId = "product:potato",
            Quantity = 300m,
            UnitCode = "KGM",
            RouteStableId = "route:sim.farm-hub-1",
            OriginFacilityStableId = "facility:sim.farm-packing-1",
            DestinationFacilityStableId = "facility:sim.regional-hub-1",
            ActorStableId = "actor:sim.farmer-1",
            RequiredRouteTicks = 3,
            SourceStableIds = new[]
            {
                "harvest-lot:potato-1",
                "package-lot:potato-1",
                "source:fixture.cargo-1",
            },
        };

    private static SimulationHarvestDispositionImpactPreviewRequest HarvestImpact()
        => new()
        {
            DispositionDecisionStableId = "decision:harvest.coop-1",
            DispositionDecisionRevision = 1,
            HarvestLotStableId = "harvest-lot:potato-1",
            HarvestLotRevision = 1,
            ProductStableId = "product:potato",
            Quantity = 300m,
            UnitCode = "KGM",
            ChoiceCode = SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
            NextWorkflowCode = SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate,
            ActorStableId = "actor:sim.farmer-1",
            SourceStableIds = new[] { "harvest-lot:potato-1", "source:fixture.harvest-1" },
        };

    private static 경영SimulationSession생성Request CreateRequest()
        => new()
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:sim.logistics-movement-1",
            ScenarioDataRevision = "scenario-data:r1",
            ScenarioSeed = 20260811,
            RuleRevision = "rule:r1",
            DurationTicks = 28,
            WorldContext = new SimulationWorldContext생성Request
            {
                FactionStableId = "faction:sim.farmers-1",
                TerritoryStableId = "territory:sim.farm-region-1",
                SettlementStableId = "settlement:sim.farm-town-1",
                GameDateStartsOn = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero),
            },
            Settlement = new SimulationSettlementInitialStateRequest
            {
                TreasuryBalance = 1_000_000m,
                CurrencyCode = "KRW",
                LaborCapacityTotal = 100m,
                StorageCapacity = 2_000m,
                StorageUnitCode = "KGM",
                PopulationCount = 100,
                PopulationFoodDemandPerTick = 100m,
                FoodEquivalentUnitCode = "KGM",
                FoodEquivalentRuleRevision = "food-equivalent:r1",
                Districts = new[]
                {
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.farm-1",
                        DistrictTypeCode = "Farm",
                        SourceStableIds = new[] { "source:fixture.settlement-1" },
                    },
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.market-1",
                        DistrictTypeCode = "Market",
                        SourceStableIds = new[] { "source:fixture.settlement-1" },
                    },
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.storage-1",
                        DistrictTypeCode = "Storage",
                        SourceStableIds = new[] { "source:fixture.settlement-1" },
                    },
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.logistics-1",
                        DistrictTypeCode = "Logistics",
                        SourceStableIds = new[] { "source:fixture.settlement-1" },
                    },
                },
                Facilities = new[]
                {
                    Facility("facility:sim.farm-packing-1", "FarmPacking", "district:sim.farm-1"),
                    Facility("facility:sim.regional-hub-1", "LogisticsHub", "district:sim.logistics-1"),
                    Facility("facility:sim.market-1", SimulationSettlementFacilityTypeCodes.Market, "district:sim.market-1"),
                    Facility("facility:sim.storage-1", SimulationSettlementFacilityTypeCodes.Storage, "district:sim.storage-1"),
                },
                SourceStableIds = new[] { "source:fixture.settlement-1" },
            },
        };

    private static SimulationSettlementFacilityRequest Facility(
        string id,
        string type,
        string district)
        => new()
        {
            FacilityStableId = id,
            FacilityTypeCode = type,
            DistrictStableId = district,
            SourceStableIds = new[] { "source:fixture.settlement-1" },
        };

    private sealed record Context(
        경영SimulationSessionService Service,
        InMemorySimulationSessionSaveStore SaveStore,
        경영SimulationSessionSnapshot Session);
}
