using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;
using Xunit;

namespace Ssalddel.Simulation.Tests;

public sealed class Simulation수출준비Tests
{
    [Fact]
    public void Preview는_운영수출경계를명시하고_배분수량을변경하지않는다()
    {
        var context = ReadyExportAllocation();

        var preview = context.Service.Preview수출준비(
            context.Session.SessionStableId,
            Preparation(Simulation수출준비검사결과Codes.Passed));
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.True(preview.IsCandidateOnly);
        Assert.True(preview.DoesNotCreateOperationalExport);
        Assert.Contains("NoExportDeclaration", preview.BoundaryCodes);
        Assert.Contains("NoCustomsClearance", preview.BoundaryCodes);
        Assert.Empty(current.ExportPreparations);
        Assert.Equal(300m, Assert.Single(current.Settlement!.HarvestLotAllocations).AvailableQuantity);
    }

    [Fact]
    public void Confirm과WorldTick은_포장과검사를거쳐_인계후보를만든다()
    {
        var context = ReadyExportAllocation();
        var confirmed = Confirm(context, Preparation(Simulation수출준비검사결과Codes.Passed));

        var scheduled = Assert.Single(confirmed.ExportPreparations);
        Assert.Equal(Simulation수출준비상태Codes.Scheduled, scheduled.StateCode);
        Assert.Equal(0m, Assert.Single(confirmed.Settlement!.HarvestLotAllocations).AvailableQuantity);

        var packaging = Advance(context, confirmed, "command:tick.export-packaging", 1);
        Assert.Equal(Simulation수출준비상태Codes.Packaging,
            Assert.Single(packaging.ExportPreparations).StateCode);

        var inspection = Advance(context, packaging, "command:tick.export-inspection", 1);
        Assert.Equal(Simulation수출준비상태Codes.Inspection,
            Assert.Single(inspection.ExportPreparations).StateCode);

        var ready = Advance(context, inspection, "command:tick.export-handoff", 1);
        var preparation = Assert.Single(ready.ExportPreparations);
        Assert.Equal(Simulation수출준비상태Codes.HandoffCandidateReady, preparation.StateCode);
        Assert.NotNull(preparation.HandoffCandidateReadyTick);
        Assert.False(preparation.CanRetry);
        Assert.Equal(300m,
            Assert.Single(ready.Settlement!.HarvestLotAllocations).OutboundReservedQuantity);
    }

    [Fact]
    public void 검사실패는_인계후보를만들지않고_수량을재작업가능하게반환한다()
    {
        var context = ReadyExportAllocation();
        var request = Preparation(Simulation수출준비검사결과Codes.Failed);
        request.FailureReasonCode = "PackagingDamage";
        request.PackagingTicks = 1;
        var confirmed = Confirm(context, request);

        var failed = Advance(context, confirmed, "command:tick.export-failed", 2);
        var preparation = Assert.Single(failed.ExportPreparations);

        Assert.Equal(Simulation수출준비상태Codes.ReworkRequired, preparation.StateCode);
        Assert.Equal("PackagingDamage", preparation.FailureReasonCode);
        Assert.True(preparation.CanRetry);
        Assert.Null(preparation.HandoffCandidateReadyTick);
        var allocation = Assert.Single(failed.Settlement!.HarvestLotAllocations);
        Assert.Equal(0m, allocation.OutboundReservedQuantity);
        Assert.Equal(300m, allocation.AvailableQuantity);
    }

    [Fact]
    public void 수출판로가아닌배분은_Preview에서차단된다()
    {
        var context = ReadyAllocation(SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
            SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate, 2);

        var preview = context.Service.Preview수출준비(
            context.Session.SessionStableId,
            Preparation(Simulation수출준비검사결과Codes.Passed));

        Assert.Contains("SourceAllocationNotExportReadiness",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void SaveReplay는_검사중원장과_배분예약을동일하게복원한다()
    {
        var context = ReadyExportAllocation();
        var confirmed = Confirm(context, Preparation(Simulation수출준비검사결과Codes.Passed));
        var inspection = Advance(context, confirmed, "command:tick.export-save", 2);
        var package = context.Service.Save(
            context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.export-preparation-1",
                ExpectedRevision = inspection.Revision,
            });

        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        Assert.Equal(Simulation수출준비상태Codes.Inspection,
            Assert.Single(restored.Session.ExportPreparations).StateCode);
        Assert.Equal(0m,
            Assert.Single(restored.Session.Settlement!.HarvestLotAllocations).AvailableQuantity);
    }

    [Fact]
    public void 재작업Preview는_실패원장을부모로보존하고_상태를바꾸지않는다()
    {
        var context = FailedPreparation();

        var preview = context.Service.Preview수출재작업(
            context.Session.SessionStableId,
            Rework(Simulation수출준비검사결과Codes.Passed));
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.True(preview.IsReworkAttempt);
        Assert.Equal("export-preparation:potato-1", preview.RootPreparationStableId);
        Assert.Equal("export-preparation:potato-1", preview.PreviousPreparationStableId);
        Assert.Equal(2, preview.AttemptNumber);
        Assert.Single(current.ExportPreparations);
        Assert.True(Assert.Single(current.ExportPreparations).CanRetry);
        Assert.Equal(300m, Assert.Single(current.Settlement!.HarvestLotAllocations).AvailableQuantity);
    }

    [Fact]
    public void 재작업Confirm과Tick은_원본실패를보존하고_두번째검사를통과시킨다()
    {
        var context = FailedPreparation();
        var confirmed = ConfirmRework(context, Rework(Simulation수출준비검사결과Codes.Passed));

        Assert.Equal(2, confirmed.ExportPreparations.Length);
        var original = confirmed.ExportPreparations.Single(value => value.AttemptNumber == 1);
        var retry = confirmed.ExportPreparations.Single(value => value.AttemptNumber == 2);
        Assert.Equal(Simulation수출준비상태Codes.ReworkRequired, original.StateCode);
        Assert.False(original.CanRetry);
        Assert.Equal(Simulation수출준비상태Codes.ReworkScheduled, retry.StateCode);
        Assert.Equal(original.PreparationStableId, retry.PreviousPreparationStableId);
        Assert.Equal(0m, Assert.Single(confirmed.Settlement!.HarvestLotAllocations).AvailableQuantity);

        var reworking = Advance(context, confirmed, "command:tick.export-reworking", 1);
        Assert.Equal(Simulation수출준비상태Codes.Reworking,
            reworking.ExportPreparations.Single(value => value.AttemptNumber == 2).StateCode);

        var inspection = Advance(context, reworking, "command:tick.export-reinspection", 1);
        Assert.Equal(Simulation수출준비상태Codes.Inspection,
            inspection.ExportPreparations.Single(value => value.AttemptNumber == 2).StateCode);

        var ready = Advance(context, inspection, "command:tick.export-retry-ready", 1);
        var completed = ready.ExportPreparations.Single(value => value.AttemptNumber == 2);
        Assert.Equal(Simulation수출준비상태Codes.HandoffCandidateReady, completed.StateCode);
        Assert.Equal("export-preparation:potato-1", completed.RootPreparationStableId);
    }

    [Fact]
    public void 같은실패원장에서_재작업을동시에두번예약할수없다()
    {
        var context = FailedPreparation();
        var confirmed = ConfirmRework(context, Rework(Simulation수출준비검사결과Codes.Passed));
        var second = Rework(Simulation수출준비검사결과Codes.Passed);
        second.RetryPreparationStableId = "export-preparation:potato-retry-2";

        var preview = context.Service.Preview수출재작업(
            context.Session.SessionStableId,
            second);

        Assert.Contains("PreviousExportPreparationNotRetryable",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
        Assert.Contains("ExportPreparationSourceAlreadyReserved",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
        Assert.Equal(2, confirmed.ExportPreparations.Length);
    }

    [Fact]
    public void 재검사실패는_두번째사유를남기고_다음재작업가능수량을반환한다()
    {
        var context = FailedPreparation();
        var request = Rework(Simulation수출준비검사결과Codes.Failed);
        request.FailureReasonCode = "TemperatureDeviation";
        request.ReworkTicks = 1;
        var confirmed = ConfirmRework(context, request);

        var failed = Advance(context, confirmed, "command:tick.export-retry-failed", 2);
        var retry = failed.ExportPreparations.Single(value => value.AttemptNumber == 2);

        Assert.Equal(Simulation수출준비상태Codes.ReworkRequired, retry.StateCode);
        Assert.Equal("TemperatureDeviation", retry.FailureReasonCode);
        Assert.True(retry.CanRetry);
        Assert.Equal(300m, Assert.Single(failed.Settlement!.HarvestLotAllocations).AvailableQuantity);
    }

    [Fact]
    public void SaveReplay는_재작업계보와_재검사중상태를동일하게복원한다()
    {
        var context = FailedPreparation();
        var confirmed = ConfirmRework(context, Rework(Simulation수출준비검사결과Codes.Passed));
        var inspection = Advance(context, confirmed, "command:tick.export-rework-save", 2);
        var package = context.Service.Save(context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.export-rework-1",
                ExpectedRevision = inspection.Revision,
            });

        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        Assert.Equal(2, restored.Session.ExportPreparations.Length);
        var retry = restored.Session.ExportPreparations.Single(value => value.AttemptNumber == 2);
        Assert.Equal(Simulation수출준비상태Codes.Inspection, retry.StateCode);
        Assert.Equal("export-preparation:potato-1", retry.PreviousPreparationStableId);
        Assert.Equal("export-preparation:potato-1", retry.RootPreparationStableId);
    }

    [Fact]
    public void Cargo준비Preview는_검사통과Lot계보를보이지만_Cargo원장을만들지않는다()
    {
        var context = PassedPreparation();

        var preview = context.Service.Preview수출Cargo준비(
            context.Session.SessionStableId, CargoPreparation());
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.True(preview.IsCandidateOnly);
        Assert.True(preview.DoesNotCreateOperationalHandoff);
        Assert.Equal("package-lot-candidate:export:export-preparation:potato-1",
            preview.PackageLotStableId);
        Assert.Equal(300m, preview.Quantity);
        Assert.Contains("NoOperationalCarrierHandoff", preview.BoundaryCodes);
        Assert.Contains("NoVehicleDeparture", preview.BoundaryCodes);
        Assert.Empty(current.ExportCargoPreparations);
        Assert.Empty(current.LogisticsMovements);
    }

    [Fact]
    public void 검사실패준비는_Cargo준비입력으로사용할수없다()
    {
        var context = FailedPreparation();

        var preview = context.Service.Preview수출Cargo준비(
            context.Session.SessionStableId, CargoPreparation());

        Assert.Contains("SourceExportPreparationNotPassed",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void Cargo준비Confirm은_기존예약량을중복예약하지않는다()
    {
        var context = PassedPreparation();

        var confirmed = ConfirmCargoPreparation(context, CargoPreparation());
        var cargo = Assert.Single(confirmed.ExportCargoPreparations);
        var allocation = Assert.Single(confirmed.Settlement!.HarvestLotAllocations);
        var preparation = Assert.Single(confirmed.ExportPreparations);

        Assert.Equal(Simulation수출Cargo준비상태Codes.Scheduled, cargo.StateCode);
        Assert.Equal(300m, allocation.OutboundReservedQuantity);
        Assert.Equal(0m, allocation.AvailableQuantity);
        Assert.Equal(cargo.CargoPreparationStableId, preparation.CargoPreparationStableId);
        Assert.Equal(cargo.CargoStableId, preparation.CargoStableId);
        Assert.Empty(confirmed.LogisticsMovements);
    }

    [Fact]
    public void Cargo준비완료Tick은_인계대기상태만만들고_출발시키지않는다()
    {
        var context = PassedPreparation();
        var confirmed = ConfirmCargoPreparation(context, CargoPreparation());

        var ready = Advance(context, confirmed, "command:tick.export-cargo-ready", 1);
        var cargo = Assert.Single(ready.ExportCargoPreparations);

        Assert.Equal(Simulation수출Cargo준비상태Codes.ReadyForHandoff, cargo.StateCode);
        Assert.NotNull(cargo.ReadyForHandoffTick);
        Assert.Equal("facility:sim.export-agent-1", cargo.OriginFacilityStableId);
        Assert.Equal("facility:sim.export-staging-1", cargo.DestinationFacilityStableId);
        Assert.Empty(ready.LogisticsMovements);
        Assert.DoesNotContain(ready.Tasks, value =>
            value.TaskTypeCode == "LogisticsMovement");
    }

    [Fact]
    public void 재검사통과한최신시도도_Cargo준비계보의원천이된다()
    {
        var context = PassedReworkPreparation();
        var request = CargoPreparation();
        request.SourceExportPreparationStableId = "export-preparation:potato-retry-1";
        request.CargoPreparationStableId = "export-cargo-preparation:potato-retry-1";
        request.CargoStableId = "cargo:sim.export-potato-retry-1";

        var preview = context.Service.Preview수출Cargo준비(
            context.Session.SessionStableId, request);

        Assert.Equal("export-preparation:potato-1", preview.RootExportPreparationStableId);
        Assert.Equal(2, preview.ExportPreparationAttemptNumber);
        Assert.Empty(preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void SaveReplay는_Cargo인계대기와_수출준비연결을동일하게복원한다()
    {
        var context = PassedPreparation();
        var confirmed = ConfirmCargoPreparation(context, CargoPreparation());
        var ready = Advance(context, confirmed, "command:tick.export-cargo-save", 1);
        var package = context.Service.Save(context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.export-cargo-preparation-1",
                ExpectedRevision = ready.Revision,
            });

        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        var cargo = Assert.Single(restored.Session.ExportCargoPreparations);
        Assert.Equal(Simulation수출Cargo준비상태Codes.ReadyForHandoff, cargo.StateCode);
        Assert.Equal(cargo.CargoPreparationStableId,
            Assert.Single(restored.Session.ExportPreparations).CargoPreparationStableId);
        Assert.Empty(restored.Session.LogisticsMovements);
    }

    private static Context ReadyExportAllocation()
        => ReadyAllocation(SimulationHarvestDispositionChoiceCodes.ExportAgent,
            SimulationHarvestDispositionWorkflowCodes.ExportReadinessCandidate, 4);

    private static Context FailedPreparation()
    {
        var context = ReadyExportAllocation();
        var request = Preparation(Simulation수출준비검사결과Codes.Failed);
        request.FailureReasonCode = "PackagingDamage";
        request.PackagingTicks = 1;
        var confirmed = Confirm(context, request);
        var failed = Advance(context, confirmed, "command:tick.export-initial-failed", 2);
        return new Context(context.Service, context.SaveStore, failed);
    }

    private static Context PassedPreparation()
    {
        var context = ReadyExportAllocation();
        var confirmed = Confirm(context, Preparation(Simulation수출준비검사결과Codes.Passed));
        var passed = Advance(context, confirmed, "command:tick.export-initial-passed", 3);
        return new Context(context.Service, context.SaveStore, passed);
    }

    private static Context PassedReworkPreparation()
    {
        var context = FailedPreparation();
        var confirmed = ConfirmRework(context, Rework(Simulation수출준비검사결과Codes.Passed));
        var passed = Advance(context, confirmed, "command:tick.export-rework-passed", 3);
        return new Context(context.Service, context.SaveStore, passed);
    }

    private static Context ReadyAllocation(string choiceCode, string workflowCode, int ticks)
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), saveStore);
        var created = service.Create(CreateRequest());
        var impact = service.ConfirmHarvestDispositionImpact(
            created.SessionStableId,
            new SimulationHarvestDispositionImpactConfirmRequest
            {
                CommandId = "command:harvest.export-choice-1",
                ExpectedRevision = created.Revision,
                Impact = new SimulationHarvestDispositionImpactPreviewRequest
                {
                    DispositionDecisionStableId = "decision:harvest.export-choice-1",
                    DispositionDecisionRevision = 1,
                    HarvestLotStableId = "harvest-lot:potato-1",
                    HarvestLotRevision = 1,
                    ProductStableId = "product:potato",
                    Quantity = 300m,
                    UnitCode = "KGM",
                    ChoiceCode = choiceCode,
                    NextWorkflowCode = workflowCode,
                    ActorStableId = "actor:sim.farmer-1",
                    SourceStableIds = new[] { "harvest-lot:potato-1", "source:fixture.harvest-1" },
                },
            });
        var ready = service.Advance(created.SessionStableId, new 경영SimulationTick진행Request
        {
            CommandId = "command:tick.harvest-export-ready",
            ExpectedRevision = impact.Revision,
            TickCount = ticks,
        });
        return new Context(service, saveStore, ready);
    }

    private static 경영SimulationSessionSnapshot Confirm(
        Context context,
        Simulation수출준비PreviewRequest preparation)
        => context.Service.Confirm수출준비(
            context.Session.SessionStableId,
            new Simulation수출준비ConfirmRequest
            {
                CommandId = "command:export-preparation.confirm-1",
                ExpectedRevision = context.Session.Revision,
                Preparation = preparation,
            });

    private static 경영SimulationSessionSnapshot ConfirmRework(
        Context context,
        Simulation수출재작업PreviewRequest rework)
        => context.Service.Confirm수출재작업(
            context.Session.SessionStableId,
            new Simulation수출재작업ConfirmRequest
            {
                CommandId = "command:export-rework.confirm-1",
                ExpectedRevision = context.Session.Revision,
                Rework = rework,
            });

    private static 경영SimulationSessionSnapshot ConfirmCargoPreparation(
        Context context,
        Simulation수출Cargo준비PreviewRequest cargoPreparation)
        => context.Service.Confirm수출Cargo준비(
            context.Session.SessionStableId,
            new Simulation수출Cargo준비ConfirmRequest
            {
                CommandId = "command:export-cargo-preparation.confirm-1",
                ExpectedRevision = context.Session.Revision,
                CargoPreparation = cargoPreparation,
            });

    private static 경영SimulationSessionSnapshot Advance(
        Context context,
        경영SimulationSessionSnapshot current,
        string commandId,
        int ticks)
        => context.Service.Advance(context.Session.SessionStableId, new 경영SimulationTick진행Request
        {
            CommandId = commandId,
            ExpectedRevision = current.Revision,
            TickCount = ticks,
        });

    private static Simulation수출준비PreviewRequest Preparation(string outcome)
        => new()
        {
            PreparationStableId = "export-preparation:potato-1",
            SourceAllocationStableId = "allocation:harvest-lot:harvest-lot:potato-1",
            Quantity = 300m,
            UnitCode = "KGM",
            PackingFacilityStableId = "facility:sim.farm-packing-1",
            HandoffFacilityStableId = "facility:sim.export-agent-1",
            ActorStableId = "actor:sim.farmer-1",
            PackagingTicks = 2,
            InspectionTicks = 1,
            InspectionOutcomeCode = outcome,
            SourceStableIds = new[] { "harvest-lot:potato-1", "source:fixture.export-1" },
        };

    private static Simulation수출재작업PreviewRequest Rework(string outcome)
        => new()
        {
            FailedPreparationStableId = "export-preparation:potato-1",
            RetryPreparationStableId = "export-preparation:potato-retry-1",
            ReworkFacilityStableId = "facility:sim.farm-packing-1",
            HandoffFacilityStableId = "facility:sim.export-agent-1",
            ActorStableId = "actor:sim.farmer-1",
            ReworkTicks = 2,
            InspectionTicks = 1,
            InspectionOutcomeCode = outcome,
            SourceStableIds = new[] { "source:fixture.export-rework-1" },
        };

    private static Simulation수출Cargo준비PreviewRequest CargoPreparation()
        => new()
        {
            CargoPreparationStableId = "export-cargo-preparation:potato-1",
            SourceExportPreparationStableId = "export-preparation:potato-1",
            CargoStableId = "cargo:sim.export-potato-1",
            CargoRevision = 1,
            RouteStableId = "route:sim.export-agent-staging-1",
            DestinationFacilityStableId = "facility:sim.export-staging-1",
            ActorStableId = "actor:sim.farmer-1",
            RequiredPreparationTicks = 1,
            SourceStableIds = new[] { "source:fixture.export-cargo-1" },
        };

    private static 경영SimulationSession생성Request CreateRequest()
        => new()
        {
            ClientRequestId = Guid.NewGuid(),
            ScenarioStableId = "scenario:sim.export-preparation-1",
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
                    Facility("facility:sim.export-agent-1", "ExportAgent", "district:sim.logistics-1"),
                    Facility("facility:sim.export-staging-1", "ExportStaging", "district:sim.logistics-1"),
                    Facility("facility:sim.market-1", SimulationSettlementFacilityTypeCodes.Market,
                        "district:sim.market-1"),
                    Facility("facility:sim.storage-1", SimulationSettlementFacilityTypeCodes.Storage,
                        "district:sim.storage-1"),
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
