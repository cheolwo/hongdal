using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class SimulationHarvestDispositionImpactTests
{
    [Theory]
    [InlineData(
        SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
        SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate,
        8, 30_000, 240_000, 2)]
    [InlineData(
        SimulationHarvestDispositionChoiceCodes.DirectOnlineSale,
        SimulationHarvestDispositionWorkflowCodes.ProducerPackingCandidate,
        18, 60_000, 360_000, 3)]
    [InlineData(
        SimulationHarvestDispositionChoiceCodes.ExportAgent,
        SimulationHarvestDispositionWorkflowCodes.ExportReadinessCandidate,
        24, 90_000, 450_000, 4)]
    [InlineData(
        SimulationHarvestDispositionChoiceCodes.ReserveStorage,
        SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate,
        6, 15_000, null, 1)]
    public void 네판로는_같은선택원장을유지하며_서버정책영향을각각Preview한다(
        string choiceCode,
        string workflowCode,
        int requiredLabor,
        int simulationCost,
        int? projectedRevenue,
        int durationTicks)
    {
        var service = Service();
        var session = service.Create(CreateSessionRequest());

        var preview = service.PreviewHarvestDispositionImpact(
            session.SessionStableId,
            ImpactRequest(choiceCode, workflowCode));

        Assert.Equal("harvest-disposition:sim.potato.20260407.r1", preview.DispositionDecisionStableId);
        Assert.Equal(1, preview.DispositionDecisionRevision);
        Assert.Equal(choiceCode, preview.ChoiceCode);
        Assert.Equal(workflowCode, preview.NextWorkflowCode);
        Assert.Equal(requiredLabor, preview.RequiredLabor);
        Assert.Equal(simulationCost, preview.SimulationCost);
        Assert.Equal(projectedRevenue.HasValue ? projectedRevenue.Value : null, preview.ProjectedRevenue);
        Assert.Equal(durationTicks, preview.DurationTicks);
        Assert.True(preview.IsCandidateOnly);
        Assert.True(preview.DoesNotApplySettlementState);
        Assert.Equal("harvest-impact:fixture-r1", preview.PolicyRevision);
        Assert.Equal(preview.DispositionDecisionStableId, preview.CommonDecisionPreview.Decision.DecisionStableId);
        Assert.Equal(SimulationDecisionStateCodes.Previewed, preview.CommonDecisionPreview.Decision.StateCode);
        Assert.Equal(new[] { workflowCode }, preview.CommonDecisionPreview.TaskPlan.OutputCandidateCodes);
        Assert.Contains(
            "source-revision:harvest-lot:sim.potato.20260407.r1:r1",
            preview.CommonDecisionPreview.Decision.SourceStableIds);
        Assert.Contains(
            "source-revision:harvest-disposition:sim.potato.20260407.r1:r1",
            preview.CommonDecisionPreview.Decision.SourceStableIds);
    }

    [Fact]
    public void Preview는_session과정착지원장을변경하지않는다()
    {
        var service = Service();
        var created = service.Create(CreateSessionRequest());

        var preview = service.PreviewHarvestDispositionImpact(
            created.SessionStableId,
            ImpactRequest(
                SimulationHarvestDispositionChoiceCodes.ReserveStorage,
                SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate));
        var stored = service.Get(created.SessionStableId);

        Assert.Equal(0, stored.Revision);
        Assert.Empty(stored.Decisions);
        Assert.Empty(stored.Tasks);
        Assert.Empty(stored.Effects);
        Assert.Equal(1_000_000m, stored.Settlement!.TreasuryBalance);
        Assert.Equal(1200m, stored.Settlement.StorageOccupied);
        Assert.Equal(1200m, stored.Settlement.FoodReserveEquivalent);
        Assert.Equal(10m, stored.Settlement.FoodSecurityDays);
        Assert.Equal(12.94m, preview.FoodSecurityDaysCandidate);
    }

    [Fact]
    public void 비축Preview는_감모와FoodEquivalent와식량안보후보를계산한다()
    {
        var service = Service();
        var session = service.Create(CreateSessionRequest());

        var preview = service.PreviewHarvestDispositionImpact(
            session.SessionStableId,
            ImpactRequest(
                SimulationHarvestDispositionChoiceCodes.ReserveStorage,
                SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate));
        var storage = Assert.IsType<SimulationReserveStorageCandidateSnapshot>(preview.StorageCandidate);

        Assert.Equal("facility:sim.storage", storage.StorageFacilityStableId);
        Assert.Equal(2000m, storage.StorageCapacity);
        Assert.Equal(1200m, storage.StorageOccupiedBefore);
        Assert.Equal(800m, storage.StorageAvailableBefore);
        Assert.Equal(300m, storage.RequestedQuantity);
        Assert.Equal(0.02m, storage.ShrinkageRate);
        Assert.Equal(6m, storage.ExpectedShrinkageQuantity);
        Assert.Equal(294m, storage.ExpectedStoredQuantity);
        Assert.Equal(352.8m, storage.FoodEquivalentAddedCandidate);
        Assert.Equal(1552.8m, storage.FoodReserveEquivalentCandidate);
        Assert.Equal(12.94m, storage.FoodSecurityDaysCandidate);
        Assert.Equal("food-equivalent:fixture-r1", storage.FoodEquivalentRuleRevision);
        Assert.Equal(
            "stock-lot:candidate:harvest-lot:sim.potato.20260407.r1:reserve-storage",
            storage.CandidateStockLotStableId);
        Assert.Contains(
            preview.CommonDecisionPreview.Decision.ExpectedEffects,
            value => value.ValueTypeCode == "FoodSecurityDaysCandidate"
                && value.BeforeValue == 10m
                && value.Delta == 2.94m
                && value.AfterValue == 12.94m);
    }

    [Fact]
    public void 창고용량이부족하면_Preview에block을표시하고Confirm을거부한다()
    {
        var service = Service();
        var request = CreateSessionRequest();
        request.Settlement!.StorageCapacity = 1400m;
        var session = service.Create(request);
        var impact = ImpactRequest(
            SimulationHarvestDispositionChoiceCodes.ReserveStorage,
            SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate);

        var preview = service.PreviewHarvestDispositionImpact(session.SessionStableId, impact);
        var error = Assert.Throws<SimulationConflictException>(() =>
            service.ConfirmHarvestDispositionImpact(
                session.SessionStableId,
                ConfirmRequest(session.Revision, impact)));

        Assert.Contains("InsufficientStorageCapacity", preview.CommonDecisionPreview.Decision.BlockReasonCodes);
        Assert.Equal("SimulationDecisionPreviewBlocked", error.ErrorCode);
        Assert.Equal(0, service.Get(session.SessionStableId).Revision);
    }

    [Theory]
    [InlineData(
        SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
        SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate,
        2)]
    [InlineData(
        SimulationHarvestDispositionChoiceCodes.DirectOnlineSale,
        SimulationHarvestDispositionWorkflowCodes.ProducerPackingCandidate,
        2)]
    [InlineData(
        SimulationHarvestDispositionChoiceCodes.ExportAgent,
        SimulationHarvestDispositionWorkflowCodes.ExportReadinessCandidate,
        2)]
    [InlineData(
        SimulationHarvestDispositionChoiceCodes.ReserveStorage,
        SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate,
        4)]
    public void 판로Confirm은_작업효과와수확Lot자원예약을생성한다(
        string choiceCode,
        string workflowCode,
        int expectedEffectCount)
    {
        var service = Service();
        var session = service.Create(CreateSessionRequest());
        var impact = ImpactRequest(choiceCode, workflowCode);

        var confirmed = service.ConfirmHarvestDispositionImpact(
            session.SessionStableId,
            ConfirmRequest(session.Revision, impact));

        var decision = Assert.Single(confirmed.Decisions);
        var task = Assert.Single(confirmed.Tasks);
        Assert.Equal("harvest-disposition:sim.potato.20260407.r1", decision.DecisionStableId);
        Assert.Equal(SimulationDecisionStateCodes.Confirmed, decision.StateCode);
        Assert.Equal(SimulationTaskStateCodes.Scheduled, task.StateCode);
        Assert.Equal(choiceCode + "Work", task.TaskTypeCode);
        Assert.Equal(new[] { workflowCode }, task.OutputCandidateCodes);
        Assert.Equal(expectedEffectCount + 2, confirmed.Effects.Length);
        Assert.All(
            confirmed.Effects,
            effect => Assert.Equal(SimulationEffectStateCodes.Pending, effect.StateCode));
        Assert.Equal(1_000_000m, confirmed.Settlement!.TreasuryBalance);
        Assert.True(confirmed.Settlement.TreasuryReserved > 0m);
        Assert.True(confirmed.Settlement.LaborReserved > 25m);
        Assert.Equal(1200m, confirmed.Settlement.StorageOccupied);
        Assert.Equal(
            choiceCode == SimulationHarvestDispositionChoiceCodes.ReserveStorage ? 294m : 0m,
            confirmed.Settlement.StorageReserved);
        Assert.Equal(10m, confirmed.Settlement.FoodSecurityDays);
        var allocation = Assert.Single(confirmed.Settlement.HarvestLotAllocations);
        Assert.Equal(SimulationHarvestLotAllocationStateCodes.Reserved, allocation.StateCode);
        Assert.Equal("harvest-lot:sim.potato.20260407.r1", allocation.HarvestLotStableId);
    }

    [Fact]
    public void 비축작업완료Tick은_현금노동재고와식량안보를원자적으로반영한다()
    {
        var service = Service();
        var session = service.Create(CreateSessionRequest());
        var impact = ImpactRequest(
            SimulationHarvestDispositionChoiceCodes.ReserveStorage,
            SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate);
        var confirmed = service.ConfirmHarvestDispositionImpact(
            session.SessionStableId,
            ConfirmRequest(session.Revision, impact));

        var completed = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:harvest-impact.tick-storage-1",
                ExpectedRevision = confirmed.Revision,
                TickCount = 1,
            });

        Assert.Equal(SimulationTaskStateCodes.Completed, Assert.Single(completed.Tasks).StateCode);
        Assert.All(
            completed.Effects,
            effect => Assert.Equal(SimulationEffectStateCodes.Applied, effect.StateCode));
        Assert.Empty(completed.Settlement!.ActiveTaskStableIds);
        Assert.Equal(985_000m, completed.Settlement.TreasuryBalance);
        Assert.Equal(0m, completed.Settlement.TreasuryReserved);
        Assert.Equal(25m, completed.Settlement.LaborReserved);
        Assert.Equal(1494m, completed.Settlement.StorageOccupied);
        Assert.Equal(0m, completed.Settlement.StorageReserved);
        Assert.Equal(1552.8m, completed.Settlement.FoodReserveEquivalent);
        Assert.Equal(12.94m, completed.Settlement.FoodSecurityDays);
        Assert.Contains(
            completed.Settlement.ReserveStockLots,
            value => value.StockLotStableId.StartsWith("stock-lot:candidate:", StringComparison.Ordinal)
                && value.Quantity == 294m);
        var allocation = Assert.Single(completed.Settlement.HarvestLotAllocations);
        Assert.Equal(SimulationHarvestLotAllocationStateCodes.Applied, allocation.StateCode);
        Assert.Equal(1, allocation.AppliedTick);
    }

    [Fact]
    public void 온라인직판Preview는_시장공급과예상수입을분리해보여준다()
    {
        var service = Service();
        var session = service.Create(CreateSessionRequest());

        var preview = service.PreviewHarvestDispositionImpact(
            session.SessionStableId,
            ImpactRequest(
                SimulationHarvestDispositionChoiceCodes.DirectOnlineSale,
                SimulationHarvestDispositionWorkflowCodes.ProducerPackingCandidate));

        Assert.Collection(
            preview.CommonDecisionPreview.Decision.ExpectedEffects,
            market =>
            {
                Assert.Equal("MarketSupplyCandidate", market.ValueTypeCode);
                Assert.Equal(300m, market.BeforeValue);
                Assert.Equal(300m, market.Delta);
                Assert.Equal(600m, market.AfterValue);
                Assert.Equal("KGM", market.UnitCode);
            },
            revenue =>
            {
                Assert.Equal("ProjectedTreasuryIncomeCandidate", revenue.ValueTypeCode);
                Assert.Equal(1_000_000m, revenue.BeforeValue);
                Assert.Equal(360_000m, revenue.Delta);
                Assert.Equal(1_360_000m, revenue.AfterValue);
                Assert.Equal("KRW", revenue.UnitCode);
            });
        Assert.Contains("UnsoldInventory", preview.RiskCodes);
    }

    [Fact]
    public void 비축Confirm과Tick은_saveReplay에서같은hash와후보효과를복원한다()
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var sourceService = Service(saveStore);
        var session = sourceService.Create(CreateSessionRequest());
        var impact = ImpactRequest(
            SimulationHarvestDispositionChoiceCodes.ReserveStorage,
            SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate);
        var confirmed = sourceService.ConfirmHarvestDispositionImpact(
            session.SessionStableId,
            ConfirmRequest(session.Revision, impact));
        var completed = sourceService.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:harvest-impact.tick-replay-1",
                ExpectedRevision = confirmed.Revision,
                TickCount = 1,
            });
        var saved = sourceService.Save(
            session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.harvest-impact-storage-1",
                ExpectedRevision = completed.Revision,
            });

        var restoredService = Service(saveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = saved.SaveStableId,
        });
        var resaved = restoredService.Save(
            restored.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.harvest-impact-storage-check-1",
                ExpectedRevision = restored.Session.Revision,
            });

        Assert.Equal(saved.ReplayHash, restored.ReplayHash);
        Assert.Equal(saved.ReplayHash, resaved.ReplayHash);
        Assert.Equal(2, restored.ReplayedCommandCount);
        Assert.Equal(
            SimulationCommandTypeCodes.HarvestDispositionImpactConfirm,
            saved.CommandLog[0].CommandTypeCode);
        Assert.NotNull(saved.CommandLog[0].HarvestDispositionImpactConfirmRequest);
        Assert.Equal(
            "harvest-disposition:sim.potato.20260407.r1",
            Assert.Single(restored.Session.Decisions).DecisionStableId);
        Assert.Equal(SimulationTaskStateCodes.Completed, Assert.Single(restored.Session.Tasks).StateCode);
        Assert.Equal(6, restored.Session.Effects.Length);
        Assert.All(
            restored.Session.Effects,
            effect => Assert.Equal(SimulationEffectStateCodes.Applied, effect.StateCode));
        Assert.Equal(12.94m, restored.Session.Settlement!.FoodSecurityDays);
        Assert.Equal(985_000m, restored.Session.Settlement.TreasuryBalance);
        Assert.Equal(
            SimulationHarvestLotAllocationStateCodes.Applied,
            Assert.Single(restored.Session.Settlement.HarvestLotAllocations).StateCode);
    }

    [Fact]
    public void 같은수확Lot은_다른판로로중복배정할수없다()
    {
        var service = Service();
        var session = service.Create(CreateSessionRequest());
        var first = service.ConfirmHarvestDispositionImpact(
            session.SessionStableId,
            ConfirmRequest(
                session.Revision,
                ImpactRequest(
                    SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
                    SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate)));
        var secondImpact = ImpactRequest(
            SimulationHarvestDispositionChoiceCodes.DirectOnlineSale,
            SimulationHarvestDispositionWorkflowCodes.ProducerPackingCandidate);
        secondImpact.DispositionDecisionStableId = "harvest-disposition:sim.potato.second";

        var secondConfirm = ConfirmRequest(first.Revision, secondImpact);
        secondConfirm.CommandId = "command:harvest-impact.confirm-2";
        var error = Assert.Throws<SimulationConflictException>(() =>
            service.ConfirmHarvestDispositionImpact(session.SessionStableId, secondConfirm));

        Assert.Equal("SimulationHarvestLotAlreadyAllocated", error.ErrorCode);
        Assert.Single(service.Get(session.SessionStableId).Settlement!.HarvestLotAllocations);
    }

    [Fact]
    public void 같은판로ConfirmCommand재시도는_예약을중복생성하지않는다()
    {
        var service = Service();
        var session = service.Create(CreateSessionRequest());
        var request = ConfirmRequest(
            session.Revision,
            ImpactRequest(
                SimulationHarvestDispositionChoiceCodes.ReserveStorage,
                SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate));

        var first = service.ConfirmHarvestDispositionImpact(session.SessionStableId, request);
        var retried = service.ConfirmHarvestDispositionImpact(session.SessionStableId, request);

        Assert.Equal(first.Revision, retried.Revision);
        Assert.Equal(first.Settlement!.TreasuryReserved, retried.Settlement!.TreasuryReserved);
        Assert.Equal(first.Settlement.StorageReserved, retried.Settlement.StorageReserved);
        Assert.Single(retried.Settlement.HarvestLotAllocations);
    }

    [Fact]
    public void 온라인직판완료Tick은_시장공급과순현금을반영한다()
    {
        var service = Service();
        var session = service.Create(CreateSessionRequest());
        var confirmed = service.ConfirmHarvestDispositionImpact(
            session.SessionStableId,
            ConfirmRequest(
                session.Revision,
                ImpactRequest(
                    SimulationHarvestDispositionChoiceCodes.DirectOnlineSale,
                    SimulationHarvestDispositionWorkflowCodes.ProducerPackingCandidate)));

        var completed = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:harvest-impact.tick-direct-1",
                ExpectedRevision = confirmed.Revision,
                TickCount = 3,
            });

        Assert.Equal(1_300_000m, completed.Settlement!.TreasuryBalance);
        Assert.Equal(600m, Assert.Single(completed.Settlement.MarketSupplyByProduct).Quantity);
        Assert.Equal(25m, completed.Settlement.LaborReserved);
        Assert.Equal(0m, completed.Settlement.TreasuryReserved);
    }

    [Theory]
    [InlineData(
        SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
        SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate,
        2,
        1210000)]
    [InlineData(
        SimulationHarvestDispositionChoiceCodes.ExportAgent,
        SimulationHarvestDispositionWorkflowCodes.ExportReadinessCandidate,
        4,
        1360000)]
    public void 조합출하와수출대행완료Tick은_각비용과Simulation수입을반영한다(
        string choiceCode,
        string workflowCode,
        int durationTicks,
        decimal expectedTreasury)
    {
        var service = Service();
        var session = service.Create(CreateSessionRequest());
        var confirmed = service.ConfirmHarvestDispositionImpact(
            session.SessionStableId,
            ConfirmRequest(session.Revision, ImpactRequest(choiceCode, workflowCode)));

        var completed = service.Advance(
            session.SessionStableId,
            new 경영SimulationTick진행Request
            {
                CommandId = "command:harvest-impact.tick-cash-1",
                ExpectedRevision = confirmed.Revision,
                TickCount = durationTicks,
            });

        Assert.Equal(expectedTreasury, completed.Settlement!.TreasuryBalance);
        Assert.Equal(300m, Assert.Single(completed.Settlement.MarketSupplyByProduct).Quantity);
        Assert.Equal(1200m, completed.Settlement.StorageOccupied);
        Assert.Equal(25m, completed.Settlement.LaborReserved);
        Assert.Equal(
            SimulationHarvestLotAllocationStateCodes.Applied,
            Assert.Single(completed.Settlement.HarvestLotAllocations).StateCode);
    }

    [Theory]
    [InlineData("UnknownChoice", "UnknownWorkflow", "SimulationHarvestDispositionChoiceUnknown")]
    [InlineData(
        SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
        SimulationHarvestDispositionWorkflowCodes.ProducerPackingCandidate,
        "SimulationHarvestDispositionWorkflowMismatch")]
    public void 선택과다음workflow의허용목록을검증한다(
        string choiceCode,
        string workflowCode,
        string expectedErrorCode)
    {
        var service = Service();
        var session = service.Create(CreateSessionRequest());

        var error = Assert.Throws<SimulationContractException>(() =>
            service.PreviewHarvestDispositionImpact(
                session.SessionStableId,
                ImpactRequest(choiceCode, workflowCode)));

        Assert.Equal(expectedErrorCode, error.ErrorCode);
    }

    [Fact]
    public void HarvestLot출처와canonical수량단위를요구한다()
    {
        var service = Service();
        var first = service.Create(CreateSessionRequest());
        var missingSource = ImpactRequest(
            SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
            SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate);
        missingSource.SourceStableIds = new[] { "source:unrelated" };
        var sourceError = Assert.Throws<SimulationContractException>(() =>
            service.PreviewHarvestDispositionImpact(first.SessionStableId, missingSource));

        var second = service.Create(CreateSessionRequest());
        var invalidUnit = ImpactRequest(
            SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
            SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate);
        invalidUnit.UnitCode = "box";
        var unitError = Assert.Throws<SimulationContractException>(() =>
            service.PreviewHarvestDispositionImpact(second.SessionStableId, invalidUnit));

        Assert.Equal("SimulationHarvestLotSourceMissing", sourceError.ErrorCode);
        Assert.Equal("SimulationHarvestQuantityUnitInvalid", unitError.ErrorCode);
    }

    [Fact]
    public void 정착지가없는Session의판로Preview를명시적으로거부한다()
    {
        var service = Service();
        var noSettlementRequest = CreateSessionRequest();
        noSettlementRequest.Settlement = null;
        var noSettlement = service.Create(noSettlementRequest);
        var noSettlementError = Assert.Throws<SimulationContractException>(() =>
            service.PreviewHarvestDispositionImpact(
                noSettlement.SessionStableId,
                ImpactRequest(
                    SimulationHarvestDispositionChoiceCodes.ReserveStorage,
                    SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate)));

        Assert.Equal("SimulationSettlementRequiredForHarvestImpact", noSettlementError.ErrorCode);
    }

    private static 경영SimulationSessionService Service(
        ISimulationSessionSaveStore? saveStore = null)
        => new(
            new InMemory경영SimulationSessionStore(),
            saveStore ?? new InMemorySimulationSessionSaveStore());

    private static SimulationHarvestDispositionImpactConfirmRequest ConfirmRequest(
        long expectedRevision,
        SimulationHarvestDispositionImpactPreviewRequest impact)
        => new()
        {
            CommandId = "command:harvest-impact.confirm-1",
            ExpectedRevision = expectedRevision,
            Impact = impact,
        };

    private static SimulationHarvestDispositionImpactPreviewRequest ImpactRequest(
        string choiceCode,
        string workflowCode)
        => new()
        {
            DispositionDecisionStableId = "harvest-disposition:sim.potato.20260407.r1",
            DispositionDecisionRevision = 1,
            HarvestLotStableId = "harvest-lot:sim.potato.20260407.r1",
            HarvestLotRevision = 1,
            ProductStableId = "product:potato",
            Quantity = 300m,
            UnitCode = "kg",
            ChoiceCode = choiceCode,
            NextWorkflowCode = workflowCode,
            ActorStableId = "actor:sim.farmer-1",
            SourceStableIds = new[] { "harvest-lot:sim.potato.20260407.r1" },
        };

    private static 경영SimulationSession생성Request CreateSessionRequest()
        => new()
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:sim.harvest-impact-storage-1",
            ScenarioDataRevision = "scenario-data:r1",
            ScenarioSeed = 20260810,
            RuleRevision = "rule:harvest-impact-storage-r1",
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
                LaborReserved = 25m,
                StorageCapacity = 2000m,
                StorageOccupied = 1200m,
                StorageUnitCode = "KGM",
                PopulationCount = 100,
                PopulationFoodDemandPerTick = 100m,
                GarrisonCount = 20,
                GarrisonFoodDemandPerTick = 20m,
                FoodEquivalentUnitCode = "FoodEquivalentUnit",
                FoodEquivalentRuleRevision = "food-equivalent:fixture-r1",
                Districts = new[]
                {
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.farm",
                        DistrictTypeCode = "FarmDistrict",
                        SourceStableIds = new[] { "source:scenario-harvest-impact-r1" },
                    },
                    new SimulationSettlementDistrictRequest
                    {
                        DistrictStableId = "district:sim.central",
                        DistrictTypeCode = "CentralDistrict",
                        SourceStableIds = new[] { "source:scenario-harvest-impact-r1" },
                    },
                },
                Facilities = new[]
                {
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:sim.storage",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Storage,
                        DistrictStableId = "district:sim.farm",
                        SourceStableIds = new[] { "source:scenario-harvest-impact-r1" },
                    },
                    new SimulationSettlementFacilityRequest
                    {
                        FacilityStableId = "facility:sim.market",
                        FacilityTypeCode = SimulationSettlementFacilityTypeCodes.Market,
                        DistrictStableId = "district:sim.central",
                        SourceStableIds = new[] { "source:scenario-harvest-impact-r1" },
                    },
                },
                MarketSupplyByProduct = new[]
                {
                    new SimulationMarketSupplyRequest
                    {
                        ProductStableId = "product:potato",
                        Quantity = 300m,
                        UnitCode = "KGM",
                        SourceStableIds = new[] { "source:scenario-harvest-impact-r1" },
                    },
                },
                ReserveStockLots = new[]
                {
                    new SimulationReserveStockLotRequest
                    {
                        StockLotStableId = "stock-lot:sim.potato-1",
                        ProductStableId = "product:potato",
                        StorageFacilityStableId = "facility:sim.storage",
                        Quantity = 1000m,
                        OutboundReservedQuantity = 0m,
                        UnitCode = "KGM",
                        FoodEquivalentQuantity = 1200m,
                        OutboundReservedFoodEquivalentQuantity = 0m,
                        SourceStableIds = new[] { "source:scenario-harvest-impact-r1" },
                    },
                },
                SourceStableIds = new[] { "source:scenario-harvest-impact-r1" },
            },
        };
}
