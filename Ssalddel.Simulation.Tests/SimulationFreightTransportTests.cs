using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Ssalddel.WorkflowRules.Contracts;
using Xunit;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationFreightTransportTests
{
    [Fact]
    public void Preview는_운송의뢰와배차후보를보여주지만_원장을변경하지않는다()
    {
        var context = ReadySource();

        var preview = context.Service.PreviewFreightTransport(
            context.Session.SessionStableId,
            Freight());
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.Equal("freight-transport:sim.potato-1", preview.TransportRequestStableId);
        Assert.Equal(화물운송상태코드.배차대기, preview.RequestStateCode);
        Assert.Equal(화물운송상태코드.배차확정, preview.DispatchStateCode);
        Assert.Equal("workflow-rules.v1", preview.RuleRevision);
        Assert.Contains("RealDriverAssignment", preview.ExcludedOperationalEffectCodes);
        Assert.Empty(preview.BlockReasonCodes);
        Assert.Empty(current.FreightTransports);
        Assert.Empty(current.LogisticsMovements);
    }

    [Fact]
    public void Confirm은_같은Cargo에_가상운송의뢰와배차이력을연결한다()
    {
        var context = ReadySource();

        var confirmed = context.Service.ConfirmFreightTransport(
            context.Session.SessionStableId,
            Confirm(context.Session.Revision));

        var freight = Assert.Single(confirmed.FreightTransports);
        var movement = Assert.Single(confirmed.LogisticsMovements);
        Assert.Equal(movement.CargoStableId, freight.CargoStableId);
        Assert.Equal(화물운송상태코드.배차확정, freight.StateCode);
        Assert.Equal(400m, freight.VehicleCapacity);
        Assert.Equal(300m, freight.Quantity);
        Assert.Equal(3, freight.StateHistory.Length);
        Assert.Equal(화물운송상태코드.배차대기, freight.StateHistory[0].ToStateCode);
        Assert.Equal(화물운송상태코드.배차확정, freight.StateHistory[2].ToStateCode);
        Assert.Equal(0m, Assert.Single(confirmed.Settlement!.HarvestLotAllocations).AvailableQuantity);
    }

    [Fact]
    public void WorldTick은_상차와운송_하차도착의인과이력을남긴다()
    {
        var context = ReadySource();
        var confirmed = context.Service.ConfirmFreightTransport(
            context.Session.SessionStableId,
            Confirm(context.Session.Revision));

        var departed = Advance(context, confirmed, "command:tick.freight-1");
        var moving = Assert.Single(departed.FreightTransports);
        Assert.Equal(화물운송상태코드.운송중, moving.StateCode);
        Assert.Equal(departed.WorldContext.WorldTick, moving.PickedUpTick);
        Assert.Contains(moving.StateHistory, value => value.ToStateCode == 화물운송상태코드.상차지도착);
        Assert.Contains(moving.StateHistory, value => value.ToStateCode == 화물운송상태코드.상차완료);

        var progress = Advance(context, departed, "command:tick.freight-2");
        var arrived = Advance(context, progress, "command:tick.freight-3");
        var dropoff = Assert.Single(arrived.FreightTransports);
        Assert.Equal(화물운송상태코드.하차지도착, dropoff.StateCode);
        Assert.Equal(arrived.WorldContext.WorldTick, dropoff.ArrivedAtDropoffTick);
        Assert.Null(dropoff.ReceivedTick);
    }

    [Fact]
    public void 인수는_도착뒤별도Confirm과Tick에서만완료된다()
    {
        var context = ReadySource();
        var confirmed = context.Service.ConfirmFreightTransport(
            context.Session.SessionStableId,
            Confirm(context.Session.Revision));
        var first = Advance(context, confirmed, "command:tick.freight-receipt-1");
        var second = Advance(context, first, "command:tick.freight-receipt-2");
        var arrived = Advance(context, second, "command:tick.freight-receipt-3");
        var freight = Assert.Single(arrived.FreightTransports);
        var receipt = Receipt(freight.Revision);

        var preview = context.Service.PreviewFreightReceipt(
            context.Session.SessionStableId,
            receipt);
        var scheduled = context.Service.ConfirmFreightReceipt(
            context.Session.SessionStableId,
            new SimulationFreightReceiptConfirmRequest
            {
                CommandId = "command:freight.receipt-confirm-1",
                ExpectedRevision = arrived.Revision,
                Receipt = receipt,
            });

        Assert.Empty(preview.Decision.BlockReasonCodes);
        Assert.Equal(화물운송상태코드.하차지도착, Assert.Single(scheduled.FreightTransports).StateCode);
        var completed = Advance(context, scheduled, "command:tick.freight-receipt-complete");
        var received = Assert.Single(completed.FreightTransports);
        Assert.Equal(화물운송상태코드.인수완료, received.StateCode);
        Assert.Equal(completed.WorldContext.WorldTick, received.ReceivedTick);
        Assert.Equal(
            SimulationTaskStateCodes.Completed,
            completed.Tasks.Single(value => value.TaskStableId == received.ReceiptTaskStableId).StateCode);
    }

    [Fact]
    public void 차량용량이화물보다작으면_Preview가차단하고Confirm할수없다()
    {
        var context = ReadySource();
        var request = Freight();
        request.Transport.VehicleCapacity = 299m;

        var preview = context.Service.PreviewFreightTransport(context.Session.SessionStableId, request);
        var error = Assert.Throws<SimulationConflictException>(() =>
            context.Service.ConfirmFreightTransport(
                context.Session.SessionStableId,
                new SimulationFreightTransportConfirmRequest
                {
                    CommandId = "command:freight.capacity-blocked",
                    ExpectedRevision = context.Session.Revision,
                    Freight = request,
                }));

        Assert.Contains("FreightVehicleCapacityExceeded", preview.BlockReasonCodes);
        Assert.Equal("SimulationDecisionPreviewBlocked", error.ErrorCode);
        Assert.Empty(context.Service.Get(context.Session.SessionStableId).FreightTransports);
    }

    [Fact]
    public void SaveReplay는_인수완료상태와전체전이이력을동일하게복원한다()
    {
        var context = ReadySource();
        var confirmed = context.Service.ConfirmFreightTransport(
            context.Session.SessionStableId,
            Confirm(context.Session.Revision));
        var first = Advance(context, confirmed, "command:tick.freight-save-1");
        var second = Advance(context, first, "command:tick.freight-save-2");
        var arrived = Advance(context, second, "command:tick.freight-save-3");
        var freight = Assert.Single(arrived.FreightTransports);
        var scheduled = context.Service.ConfirmFreightReceipt(
            context.Session.SessionStableId,
            new SimulationFreightReceiptConfirmRequest
            {
                CommandId = "command:freight.receipt-save",
                ExpectedRevision = arrived.Revision,
                Receipt = Receipt(freight.Revision),
            });
        var completed = Advance(context, scheduled, "command:tick.freight-save-4");
        var package = context.Service.Save(
            context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.freight-transport-1",
                ExpectedRevision = completed.Revision,
            });

        var restoreService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            context.SaveStore);
        var restored = restoreService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        var restoredFreight = Assert.Single(restored.Session.FreightTransports);
        Assert.Equal(화물운송상태코드.인수완료, restoredFreight.StateCode);
        Assert.Equal(8, restoredFreight.StateHistory.Length);
        Assert.Equal(300m, restoredFreight.Quantity);
        Assert.Equal(300m, Assert.Single(restored.Session.LogisticsMovements).ReservedQuantity);
    }

    [Fact]
    public void 배차Preview는_용량부족과위치노후를차단하고_대기기사를추천한다()
    {
        var context = ReadySource();

        var preview = context.Service.PreviewFreightDispatch(
            context.Session.SessionStableId,
            Dispatch());
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.Equal(context.Session.Revision, preview.ObservedRevision);
        Assert.Equal("carrier-candidate:sim.waiting-truck", preview.RecommendedCarrierCandidateStableId);
        Assert.Equal("freight-dispatch-candidate.v1", preview.RuleRevision);
        Assert.Empty(preview.BlockReasonCodes);
        Assert.Equal(3, preview.CandidateEvaluations.Length);
        Assert.Contains(
            "VehicleCapacityExceeded",
            preview.CandidateEvaluations.Single(value =>
                value.CarrierCandidateStableId == "carrier-candidate:sim.small-van").BlockReasonCodes);
        Assert.Contains(
            "CandidateLocationStale",
            preview.CandidateEvaluations.Single(value =>
                value.CarrierCandidateStableId == "carrier-candidate:sim.stale-truck").BlockReasonCodes);
        var recommended = preview.CandidateEvaluations.Single(value => value.IsRecommended);
        Assert.True(recommended.IsEligible);
        Assert.Equal(9m, recommended.Score.DriverWaitingScore);
        Assert.Empty(current.FreightTransports);
        Assert.Empty(current.LogisticsMovements);
    }

    [Fact]
    public void 배차Confirm은_선택근거와후보평가를_가상운송에보존한다()
    {
        var context = ReadySource();

        var confirmed = context.Service.ConfirmFreightDispatch(
            context.Session.SessionStableId,
            DispatchConfirm(context.Session.Revision, "carrier-candidate:sim.waiting-truck"));

        var freight = Assert.Single(confirmed.FreightTransports);
        Assert.Equal("carrier-candidate:sim.waiting-truck", freight.CarrierCandidateStableId);
        Assert.Equal("vehicle:sim.truck-fresh", freight.VehicleStableId);
        var decision = Assert.IsType<SimulationFreightDispatchDecisionSnapshot>(freight.DispatchDecision);
        Assert.Equal("carrier-candidate:sim.waiting-truck", decision.RecommendedCarrierCandidateStableId);
        Assert.Equal("carrier-candidate:sim.waiting-truck", decision.SelectedCarrierCandidateStableId);
        Assert.Equal(3, decision.CandidateEvaluations.Length);
        Assert.True(Assert.Single(decision.CandidateEvaluations, value => value.IsSelected).IsEligible);
        Assert.Equal(화물운송상태코드.배차확정, freight.StateCode);
    }

    [Fact]
    public void 배차Confirm은_차단된후보를선택하면_원장을변경하지않는다()
    {
        var context = ReadySource();

        var error = Assert.Throws<SimulationConflictException>(() =>
            context.Service.ConfirmFreightDispatch(
                context.Session.SessionStableId,
                DispatchConfirm(context.Session.Revision, "carrier-candidate:sim.small-van")));

        Assert.Equal("SimulationFreightDispatchCandidateIneligible", error.ErrorCode);
        var current = context.Service.Get(context.Session.SessionStableId);
        Assert.Empty(current.FreightTransports);
        Assert.Empty(current.LogisticsMovements);
    }

    [Fact]
    public void 배차SaveReplay는_추천과선택근거를동일하게복원한다()
    {
        var context = ReadySource();
        var confirmed = context.Service.ConfirmFreightDispatch(
            context.Session.SessionStableId,
            DispatchConfirm(context.Session.Revision, "carrier-candidate:sim.waiting-truck"));
        var package = context.Service.Save(
            context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.freight-dispatch-1",
                ExpectedRevision = confirmed.Revision,
            });
        var restoreService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(),
            context.SaveStore);

        var restored = restoreService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        var decision = Assert.Single(restored.Session.FreightTransports).DispatchDecision;
        Assert.NotNull(decision);
        Assert.Equal("carrier-candidate:sim.waiting-truck", decision!.SelectedCarrierCandidateStableId);
        Assert.Equal(3, decision.CandidateEvaluations.Length);
        Assert.Single(decision.CandidateEvaluations, value => value.IsSelected);
    }

    [Fact]
    public void 배차Confirm은_같은Command재시도에_같은Snapshot을돌려준다()
    {
        var context = ReadySource();
        var command = DispatchConfirm(
            context.Session.Revision,
            "carrier-candidate:sim.waiting-truck");

        var first = context.Service.ConfirmFreightDispatch(
            context.Session.SessionStableId,
            command);
        var retried = context.Service.ConfirmFreightDispatch(
            context.Session.SessionStableId,
            command);

        Assert.Equal(first.Revision, retried.Revision);
        Assert.Equal(first.WorldContext.WorldTick, retried.WorldContext.WorldTick);
        Assert.Equal(
            Assert.Single(first.FreightTransports).DispatchDecision!.SelectedCarrierCandidateStableId,
            Assert.Single(retried.FreightTransports).DispatchDecision!.SelectedCarrierCandidateStableId);
    }

    private static 경영SimulationSessionSnapshot Advance(
        Context context,
        경영SimulationSessionSnapshot current,
        string commandId)
        => context.Service.Advance(
            context.Session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = commandId,
                ExpectedRevision = current.Revision,
                TickCount = 1,
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
                CommandId = "command:harvest.freight-source",
                ExpectedRevision = created.Revision,
                Impact = HarvestImpact(),
            });
        var ready = service.Advance(
            created.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:tick.harvest.freight-ready",
                ExpectedRevision = impact.Revision,
                TickCount = 2,
            });
        return new Context(service, saveStore, ready);
    }

    private static SimulationFreightTransportConfirmRequest Confirm(long revision)
        => new()
        {
            CommandId = "command:freight.confirm-potato-1",
            ExpectedRevision = revision,
            Freight = Freight(),
        };

    private static SimulationFreightDispatchConfirmRequest DispatchConfirm(
        long revision,
        string selectedCarrierCandidateStableId)
        => new()
        {
            CommandId = "command:freight.dispatch-potato-1",
            ExpectedRevision = revision,
            SelectedCarrierCandidateStableId = selectedCarrierCandidateStableId,
            FreightDispatch = Dispatch(),
        };

    private static SimulationFreightDispatchPreviewRequest Dispatch()
        => new()
        {
            Dispatch = new SimulationFreightDispatchRequest
            {
                TransportRequestStableId = "freight-transport:sim.potato-1",
                LocationFreshnessMinutes = 10m,
                BasePickupRadiusKm = 5m,
                MaximumRemotePickupRadiusKm = 30m,
                RemotePickupAverageSpeedKmH = 40m,
                RemotePickupArrivalBufferMinutes = 10m,
                PickupWindowRemainingMinutes = 60m,
                SourceStableIds = new[] { "source:fixture.freight-dispatch-1" },
                Candidates = new[]
                {
                    Candidate(
                        "carrier-candidate:sim.small-van",
                        "vehicle:sim.van-small",
                        200m,
                        2m,
                        1m,
                        10m,
                        "가까운 소형 밴"),
                    Candidate(
                        "carrier-candidate:sim.stale-truck",
                        "vehicle:sim.truck-stale",
                        400m,
                        3m,
                        30m,
                        30m,
                        "위치 확인이 필요한 트럭"),
                    Candidate(
                        "carrier-candidate:sim.waiting-truck",
                        "vehicle:sim.truck-fresh",
                        400m,
                        6m,
                        2m,
                        90m,
                        "대기 중인 지역 트럭"),
                },
            },
            Movement = Freight().Movement,
        };

    private static SimulationFreightDispatchCandidateRequest Candidate(
        string candidateId,
        string vehicleId,
        decimal capacity,
        decimal distanceKm,
        decimal locationAgeMinutes,
        decimal waitingMinutes,
        string reason)
        => new()
        {
            CarrierCandidateStableId = candidateId,
            VehicleStableId = vehicleId,
            IsFreightApp = true,
            IsVehicleActive = true,
            IsDriverOperating = true,
            LocationAgeMinutes = locationAgeMinutes,
            PickupDistanceKm = distanceKm,
            PickupAllowedRadiusKm = 10m,
            VehicleCapacity = capacity,
            VehicleCapacityUnitCode = "KGM",
            IsVehicleCompatible = true,
            DriverWaitingMinutes = waitingMinutes,
            CanCompleteSchedule = true,
            CanInsertSchedule = true,
            EstimatedExtraProfit = 4_000m,
            AdditionalDelayMinutes = 5m,
            RecommendationTypeCode = "single",
            BaseReason = reason,
        };

    private static SimulationFreightTransportPreviewRequest Freight()
        => new()
        {
            Transport = new SimulationFreightTransportBindingRequest
            {
                TransportRequestStableId = "freight-transport:sim.potato-1",
                DispatchOfferStableId = "dispatch-offer:sim.potato-1",
                CarrierCandidateStableId = "carrier-candidate:sim.coop-1",
                VehicleStableId = "vehicle:sim.truck-1",
                VehicleCapacity = 400m,
                VehicleCapacityUnitCode = "KGM",
            },
            Movement = new SimulationLogisticsMovementPreviewRequest
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
                    "source:fixture.freight-1",
                },
            },
        };

    private static SimulationFreightReceiptPreviewRequest Receipt(long revision)
        => new()
        {
            TransportRequestStableId = "freight-transport:sim.potato-1",
            TransportRevision = revision,
            ActorStableId = "actor:sim.hub-receiver-1",
            ReceiptDurationTicks = 1,
            SourceStableIds = new[] { "source:fixture.freight-receipt-1" },
        };

    private static SimulationHarvestDispositionImpactPreviewRequest HarvestImpact()
        => new()
        {
            DispositionDecisionStableId = "decision:harvest.freight-source",
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
            ScenarioStableId = "scenario:sim.freight-transport-1",
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
                    District("district:sim.farm-1", "Farm"),
                    District("district:sim.logistics-1", "Logistics"),
                    District("district:sim.market-1", "Market"),
                    District("district:sim.storage-1", "Storage"),
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

    private static SimulationSettlementDistrictRequest District(string id, string type)
        => new()
        {
            DistrictStableId = id,
            DistrictTypeCode = type,
            SourceStableIds = new[] { "source:fixture.settlement-1" },
        };

    private static SimulationSettlementFacilityRequest Facility(string id, string type, string district)
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
