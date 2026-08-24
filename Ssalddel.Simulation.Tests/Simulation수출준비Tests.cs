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

    [Fact]
    public void Cargo인계Preview는_준비된Cargo계보를보이지만_인계원장을만들지않는다()
    {
        var context = ReadyCargoPreparation();

        var preview = context.Service.Preview수출Cargo인계(
            context.Session.SessionStableId, CargoHandoff());
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.True(preview.IsCandidateOnly);
        Assert.True(preview.DoesNotCreateLogisticsMovement);
        Assert.Equal("cargo:sim.export-potato-1", preview.CargoStableId);
        Assert.Equal(300m, preview.Quantity);
        Assert.Contains("LogisticsMovementRequiresSeparateDecision", preview.BoundaryCodes);
        Assert.Empty(current.ExportCargoHandoffs);
        Assert.Empty(current.LogisticsMovements);
    }

    [Fact]
    public void Cargo준비완료전에는_배송대행지인계를Confirm할수없다()
    {
        var context = ScheduledCargoPreparation();

        var preview = context.Service.Preview수출Cargo인계(
            context.Session.SessionStableId, CargoHandoff());

        Assert.Contains("SourceExportCargoPreparationNotReady",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void Cargo준비목적지와다른시설의인계는차단한다()
    {
        var context = ReadyCargoPreparation();
        var request = CargoHandoff();
        request.ReceivingFacilityStableId = "facility:sim.storage-1";

        var preview = context.Service.Preview수출Cargo인계(
            context.Session.SessionStableId, request);

        Assert.Contains("ExportCargoHandoffFacilityMismatch",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void Cargo인계Confirm은_Simulation인계Task만예약하고_배차하지않는다()
    {
        var context = ReadyCargoPreparation();

        var confirmed = ConfirmCargoHandoff(context, CargoHandoff());
        var handoff = Assert.Single(confirmed.ExportCargoHandoffs);
        var cargo = Assert.Single(confirmed.ExportCargoPreparations);

        Assert.Equal(Simulation수출Cargo인계상태Codes.Scheduled, handoff.StateCode);
        Assert.Equal(Simulation수출Cargo준비상태Codes.HandoffScheduled, cargo.StateCode);
        Assert.Equal(handoff.HandoffStableId, cargo.HandoffStableId);
        Assert.Equal(300m,
            Assert.Single(confirmed.Settlement!.HarvestLotAllocations).OutboundReservedQuantity);
        Assert.Empty(confirmed.LogisticsMovements);
        Assert.Empty(confirmed.FreightTransports);
    }

    [Fact]
    public void Cargo인계완료Tick도_차량출발이나물류이동을만들지않는다()
    {
        var context = ReadyCargoPreparation();
        var confirmed = ConfirmCargoHandoff(context, CargoHandoff());

        var completed = Advance(context, confirmed, "command:tick.export-handoff-complete", 1);
        var handoff = Assert.Single(completed.ExportCargoHandoffs);
        var cargo = Assert.Single(completed.ExportCargoPreparations);

        Assert.Equal(Simulation수출Cargo인계상태Codes.HandedOffInSimulation, handoff.StateCode);
        Assert.Equal(Simulation수출Cargo준비상태Codes.HandedOffInSimulation, cargo.StateCode);
        Assert.NotNull(handoff.CompletedTick);
        Assert.Equal(handoff.CompletedTick, cargo.HandoffCompletedTick);
        Assert.Empty(completed.LogisticsMovements);
        Assert.Empty(completed.FreightTransports);
    }

    [Fact]
    public void SaveReplay는_Cargo인계완료와_물류미생성을동일하게복원한다()
    {
        var context = ReadyCargoPreparation();
        var confirmed = ConfirmCargoHandoff(context, CargoHandoff());
        var completed = Advance(context, confirmed, "command:tick.export-handoff-save", 1);
        var package = context.Service.Save(context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.export-cargo-handoff-1",
                ExpectedRevision = completed.Revision,
            });

        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        Assert.Equal(Simulation수출Cargo인계상태Codes.HandedOffInSimulation,
            Assert.Single(restored.Session.ExportCargoHandoffs).StateCode);
        Assert.Equal(Simulation수출Cargo준비상태Codes.HandedOffInSimulation,
            Assert.Single(restored.Session.ExportCargoPreparations).StateCode);
        Assert.Empty(restored.Session.LogisticsMovements);
    }

    [Fact]
    public void 수출물류Preview는_인계계보와기존예약을재사용하지만_상태를바꾸지않는다()
    {
        var context = HandedOffCargo();

        var preview = context.Service.PreviewLogisticsMovement(
            context.Session.SessionStableId, ExportMovement());
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.True(preview.ReusesExistingOutboundReservation);
        Assert.Equal("export-cargo-handoff:potato-1", preview.SourceExportCargoHandoffStableId);
        Assert.Contains("ExportHandoffLineageVerified", preview.BoundaryCodes);
        Assert.Contains("ExistingOutboundReservationReused", preview.BoundaryCodes);
        Assert.Empty(preview.CommonDecisionPreview.Decision.BlockReasonCodes);
        Assert.Empty(current.LogisticsMovements);
        Assert.Equal(300m,
            Assert.Single(current.Settlement!.HarvestLotAllocations).OutboundReservedQuantity);
    }

    [Fact]
    public void 배송대행지인계완료전에는_수출물류를시작할수없다()
    {
        var context = ScheduledCargoHandoff();

        var preview = context.Service.PreviewLogisticsMovement(
            context.Session.SessionStableId, ExportMovement());

        Assert.Contains("SourceExportCargoHandoffNotCompleted",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void 수출CargoLot계보가다르면_물류Preview에서차단한다()
    {
        var context = HandedOffCargo();
        var request = ExportMovement();
        request.PackageLotStableId = "package-lot-candidate:export:other";

        var preview = context.Service.PreviewLogisticsMovement(
            context.Session.SessionStableId, request);

        Assert.Contains("SourceExportCargoHandoffLineageMismatch",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void 수출물류Confirm은_기존300kg예약을유지하고_같은Cargo이동을예약한다()
    {
        var context = HandedOffCargo();

        var confirmed = ConfirmExportMovement(context, ExportMovement());
        var movement = Assert.Single(confirmed.LogisticsMovements);
        var handoff = Assert.Single(confirmed.ExportCargoHandoffs);
        var allocation = Assert.Single(confirmed.Settlement!.HarvestLotAllocations);

        Assert.Equal(SimulationLogisticsMovementStateCodes.Reserved, movement.StateCode);
        Assert.Equal(handoff.HandoffStableId, movement.SourceExportCargoHandoffStableId);
        Assert.Equal(movement.CargoStableId, handoff.LogisticsMovementCargoStableId);
        Assert.Equal(movement.TaskStableId, handoff.LogisticsMovementTaskStableId);
        Assert.Equal(300m, allocation.OutboundReservedQuantity);
        Assert.Equal(0m, allocation.AvailableQuantity);
        Assert.Empty(confirmed.FreightTransports);
    }

    [Fact]
    public void 수출물류WorldTick은_명시적Confirm뒤에만_출발하고도착한다()
    {
        var context = HandedOffCargo();
        var confirmed = ConfirmExportMovement(context, ExportMovement());

        var moving = Advance(context, confirmed, "command:tick.export-logistics-moving", 1);
        Assert.Equal(SimulationLogisticsMovementStateCodes.InTransit,
            Assert.Single(moving.LogisticsMovements).StateCode);

        var arrived = Advance(context, moving, "command:tick.export-logistics-arrived", 1);
        var movement = Assert.Single(arrived.LogisticsMovements);
        Assert.Equal(SimulationLogisticsMovementStateCodes.ArrivedAtDestination, movement.StateCode);
        Assert.Equal("facility:sim.port-staging-1", movement.DestinationFacilityStableId);
        Assert.Equal(300m, movement.ReservedQuantity);
        Assert.Empty(arrived.FreightTransports);
    }

    [Fact]
    public void SaveReplay는_수출인계계보와_이동중Cargo를동일하게복원한다()
    {
        var context = HandedOffCargo();
        var confirmed = ConfirmExportMovement(context, ExportMovement());
        var moving = Advance(context, confirmed, "command:tick.export-logistics-save", 1);
        var package = context.Service.Save(context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.export-logistics-1",
                ExpectedRevision = moving.Revision,
            });

        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        var movement = Assert.Single(restored.Session.LogisticsMovements);
        Assert.Equal(SimulationLogisticsMovementStateCodes.InTransit, movement.StateCode);
        Assert.Equal("export-cargo-handoff:potato-1", movement.SourceExportCargoHandoffStableId);
        Assert.Equal(movement.TaskStableId,
            Assert.Single(restored.Session.ExportCargoHandoffs).LogisticsMovementTaskStableId);
        Assert.Equal(300m,
            Assert.Single(restored.Session.Settlement!.HarvestLotAllocations).OutboundReservedQuantity);
    }

    [Fact]
    public void 항만인수Preview는_도착한Cargo계보를보이지만_인수원장을만들지않는다()
    {
        var context = ArrivedExportCargo();

        var preview = context.Service.Preview수출항만인수(
            context.Session.SessionStableId, PortReceipt());
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.Equal("export-cargo-handoff:potato-1", preview.SourceExportCargoHandoffStableId);
        Assert.Equal("harvest-lot:potato-1", preview.HarvestLotStableId);
        Assert.Equal(300m, preview.Quantity);
        Assert.Contains("PortStagingReceiptOnly", preview.BoundaryCodes);
        Assert.Contains("NoCustomsClearance", preview.BoundaryCodes);
        Assert.Empty(preview.CommonDecisionPreview.Decision.BlockReasonCodes);
        Assert.Empty(current.ExportPortReceipts);
    }

    [Fact]
    public void 항만에도착하기전Cargo는_인수할수없다()
    {
        var context = MovingExportCargo();

        var preview = context.Service.Preview수출항만인수(
            context.Session.SessionStableId, PortReceipt());

        Assert.Contains("ExportPortCargoNotArrived",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void Cargo도착지와다른시설의_항만인수는차단된다()
    {
        var context = ArrivedExportCargo();
        var request = PortReceipt();
        request.ReceivingFacilityStableId = "facility:sim.export-staging-1";

        var preview = context.Service.Preview수출항만인수(
            context.Session.SessionStableId, request);

        Assert.Contains("ExportPortReceivingFacilityMismatch",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void 항만인수Confirm은_기존300kg예약을유지하고_별도Task만예약한다()
    {
        var context = ArrivedExportCargo();

        var confirmed = ConfirmPortReceipt(context, PortReceipt());
        var receipt = Assert.Single(confirmed.ExportPortReceipts);
        var movement = Assert.Single(confirmed.LogisticsMovements);

        Assert.Equal(Simulation수출항만인수상태Codes.Scheduled, receipt.StateCode);
        Assert.Equal(receipt.ReceiptStableId, movement.DestinationReceiptStableId);
        Assert.Equal(300m,
            Assert.Single(confirmed.Settlement!.HarvestLotAllocations).OutboundReservedQuantity);
        Assert.Empty(confirmed.FreightTransports);
    }

    [Fact]
    public void 항만인수완료Tick은_준비시설인수만완료하고_통관이나선적을만들지않는다()
    {
        var context = ArrivedExportCargo();
        var confirmed = ConfirmPortReceipt(context, PortReceipt());

        var completed = Advance(context, confirmed, "command:tick.export-port-received", 1);
        var receipt = Assert.Single(completed.ExportPortReceipts);
        var movement = Assert.Single(completed.LogisticsMovements);

        Assert.Equal(Simulation수출항만인수상태Codes.ReceivedAtPortStaging, receipt.StateCode);
        Assert.NotNull(receipt.CompletedTick);
        Assert.Equal(receipt.CompletedTick, movement.DestinationReceiptCompletedTick);
        Assert.Contains("NoExportDeclaration", receipt.BoundaryCodes);
        Assert.Contains("NoOfficialInspection", receipt.BoundaryCodes);
        Assert.Contains("NoVesselLoading", receipt.BoundaryCodes);
    }

    [Fact]
    public void SaveReplay는_항만인수완료와_Cargo계보를동일하게복원한다()
    {
        var context = ArrivedExportCargo();
        var confirmed = ConfirmPortReceipt(context, PortReceipt());
        var completed = Advance(context, confirmed, "command:tick.export-port-save", 1);
        var package = context.Service.Save(context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.export-port-receipt-1",
                ExpectedRevision = completed.Revision,
            });

        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        var receipt = Assert.Single(restored.Session.ExportPortReceipts);
        Assert.Equal(Simulation수출항만인수상태Codes.ReceivedAtPortStaging, receipt.StateCode);
        Assert.Equal(receipt.ReceiptStableId,
            Assert.Single(restored.Session.LogisticsMovements).DestinationReceiptStableId);
        Assert.Equal(300m,
            Assert.Single(restored.Session.Settlement!.HarvestLotAllocations).OutboundReservedQuantity);
    }

    [Fact]
    public void 수출준비성Preview는_항만인수계보와준비후보를보이지만_원장을만들지않는다()
    {
        var context = ReceivedAtPortStaging();

        var preview = context.Service.Preview수출준비성검토(
            context.Session.SessionStableId, ReadinessReview(true, true));
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.Equal(Simulation수출준비성검토결과Codes.ReadyCandidate, preview.OutcomeCode);
        Assert.Equal("export-port-receipt:potato-1", preview.SourcePortReceiptStableId);
        Assert.Equal("harvest-lot:potato-1", preview.HarvestLotStableId);
        Assert.Equal(300m, preview.Quantity);
        Assert.Empty(preview.MissingRequirementCodes);
        Assert.Contains("SelfAssertedInputsOnly", preview.BoundaryCodes);
        Assert.Contains("NoQuarantineApproval", preview.BoundaryCodes);
        Assert.Empty(preview.CommonDecisionPreview.Decision.BlockReasonCodes);
        Assert.Empty(current.ExportReadinessReviews);
    }

    [Fact]
    public void 항만인수완료전에는_수출준비성을검토할수없다()
    {
        var context = ScheduledPortReceipt();

        var preview = context.Service.Preview수출준비성검토(
            context.Session.SessionStableId, ReadinessReview(true, true));

        Assert.Contains("ExportPortReceiptNotCompleted",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void 항만인수시설과다른곳의_수출준비성검토는차단된다()
    {
        var context = ReceivedAtPortStaging();
        var request = ReadinessReview(true, true);
        request.ReviewingFacilityStableId = "facility:sim.export-staging-1";

        var preview = context.Service.Preview수출준비성검토(
            context.Session.SessionStableId, request);

        Assert.Contains("ExportReadinessReviewFacilityMismatch",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void 서류나검사준비가빠진검토는_보완필요로끝나고_예약량을유지한다()
    {
        var context = ReceivedAtPortStaging();
        var confirmed = ConfirmReadinessReview(context, ReadinessReview(false, false));

        var completed = Advance(context, confirmed, "command:tick.export-readiness-action", 1);
        var review = Assert.Single(completed.ExportReadinessReviews);

        Assert.Equal(Simulation수출준비성검토상태Codes.ActionRequired, review.StateCode);
        Assert.Equal(Simulation수출준비성검토결과Codes.ActionRequired, review.OutcomeCode);
        Assert.Contains(Simulation수출준비성보완Codes.DocumentsNotPrepared,
            review.MissingRequirementCodes);
        Assert.Contains(Simulation수출준비성보완Codes.InspectionPreparationNotReady,
            review.MissingRequirementCodes);
        Assert.Equal(300m,
            Assert.Single(completed.Settlement!.HarvestLotAllocations).OutboundReservedQuantity);
    }

    [Fact]
    public void 보완필요뒤에는_새stableId로재검토하여_준비후보가될수있다()
    {
        var context = ActionRequiredReadinessReview();
        var retry = ReadinessReview(true, true, "export-readiness-review:potato-2");

        var preview = context.Service.Preview수출준비성검토(
            context.Session.SessionStableId, retry);
        Assert.Equal("export-readiness-review:potato-1", preview.ParentReviewStableId);
        Assert.Equal(2, preview.AttemptNumber);
        Assert.Empty(preview.CommonDecisionPreview.Decision.BlockReasonCodes);

        var confirmed = ConfirmReadinessReview(context, retry, "command:export-readiness.retry-1");
        var completed = Advance(context, confirmed, "command:tick.export-readiness-ready", 1);
        var reviews = completed.ExportReadinessReviews
            .OrderBy(value => value.AttemptNumber).ToArray();

        Assert.Equal(Simulation수출준비성검토상태Codes.ActionRequired, reviews[0].StateCode);
        Assert.Equal(Simulation수출준비성검토상태Codes.ReadyCandidate, reviews[1].StateCode);
        Assert.Equal(reviews[0].ReviewStableId, reviews[1].ParentReviewStableId);
    }

    [Fact]
    public void 준비후보완료뒤에는_같은항만인수의중복검토를차단한다()
    {
        var context = ReadyCandidateReadinessReview();
        var request = ReadinessReview(true, true, "export-readiness-review:potato-2");

        var preview = context.Service.Preview수출준비성검토(
            context.Session.SessionStableId, request);

        Assert.Contains("ExportReadinessAlreadyReady",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void SaveReplay는_보완과재검토계보를_동일하게복원한다()
    {
        var context = ActionRequiredReadinessReview();
        var retry = ReadinessReview(true, true, "export-readiness-review:potato-2");
        var confirmed = ConfirmReadinessReview(
            context, retry, "command:export-readiness.save-retry");
        var completed = Advance(context, confirmed, "command:tick.export-readiness-save", 1);
        var package = context.Service.Save(context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.export-readiness-1",
                ExpectedRevision = completed.Revision,
            });

        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        var reviews = restored.Session.ExportReadinessReviews
            .OrderBy(value => value.AttemptNumber).ToArray();
        Assert.Equal(2, reviews.Length);
        Assert.Equal(Simulation수출준비성검토상태Codes.ActionRequired, reviews[0].StateCode);
        Assert.Equal(Simulation수출준비성검토상태Codes.ReadyCandidate, reviews[1].StateCode);
        Assert.Equal(reviews[0].ReviewStableId, reviews[1].ParentReviewStableId);
        Assert.Equal(300m,
            Assert.Single(restored.Session.Settlement!.HarvestLotAllocations)
                .OutboundReservedQuantity);
    }

    [Fact]
    public void 선적계획Preview는_목적시장별수익비용기간위험을비교하지만_원장을바꾸지않는다()
    {
        var context = ReadyCandidateReadinessReview();
        var oceanRequest = ShipmentPlan();
        var airRequest = ShipmentPlan();
        airRequest.PlanStableId = "export-shipment-plan:potato-air-1";
        airRequest.DestinationMarketStableId = "market:sim.jp-osaka-1";
        airRequest.TransportModeCode = Simulation수출운송방식Codes.Air;
        airRequest.ExpectedGrossRevenue = 2_000_000m;
        airRequest.ExpectedInternationalLogisticsCost = 900_000m;
        airRequest.EstimatedTransitTicks = 2;
        airRequest.RiskScore = 25;

        var ocean = context.Service.Preview수출선적계획(
            context.Session.SessionStableId, oceanRequest);
        var air = context.Service.Preview수출선적계획(
            context.Session.SessionStableId, airRequest);
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.Equal(1_100_000m, ocean.ExpectedNetRevenue);
        Assert.Equal(Simulation수출위험수준Codes.Medium, ocean.RiskLevelCode);
        Assert.Equal(800_000m, air.ExpectedNetRevenue);
        Assert.Equal(Simulation수출위험수준Codes.Low, air.RiskLevelCode);
        Assert.True(ocean.DoesNotChangeTreasury);
        Assert.True(air.DoesNotCreateOperationalShipment);
        Assert.Empty(ocean.CommonDecisionPreview.Decision.BlockReasonCodes);
        Assert.Empty(air.CommonDecisionPreview.Decision.BlockReasonCodes);
        Assert.Empty(current.ExportShipmentPlans);
        Assert.Equal(context.Session.Settlement!.TreasuryBalance,
            current.Settlement!.TreasuryBalance);
    }

    [Fact]
    public void 보완필요인준비성검토로는_선적계획을확정할수없다()
    {
        var context = ActionRequiredReadinessReview();

        var preview = context.Service.Preview수출선적계획(
            context.Session.SessionStableId, ShipmentPlan());

        Assert.Contains("ExportReadinessReviewNotReady",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void 준비성검토시설과다른곳의_선적계획은차단된다()
    {
        var context = ReadyCandidateReadinessReview();
        var request = ShipmentPlan();
        request.PlanningFacilityStableId = "facility:sim.export-staging-1";

        var preview = context.Service.Preview수출선적계획(
            context.Session.SessionStableId, request);

        Assert.Contains("ExportShipmentPlanningFacilityMismatch",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void 선적계획Confirm은_계획Task만예약하고_재정이나예약량을바꾸지않는다()
    {
        var context = ReadyCandidateReadinessReview();

        var confirmed = ConfirmShipmentPlan(context, ShipmentPlan());
        var plan = Assert.Single(confirmed.ExportShipmentPlans);

        Assert.Equal(Simulation수출선적계획상태Codes.Scheduled, plan.StateCode);
        Assert.Equal(1_800_000m, plan.ExpectedGrossRevenue);
        Assert.Equal(700_000m, plan.ExpectedTotalCost);
        Assert.Equal(1_100_000m, plan.ExpectedNetRevenue);
        Assert.Equal(context.Session.Settlement!.TreasuryBalance,
            confirmed.Settlement!.TreasuryBalance);
        Assert.Equal(300m,
            Assert.Single(confirmed.Settlement.HarvestLotAllocations).OutboundReservedQuantity);
        Assert.Empty(confirmed.FreightTransports);
    }

    [Fact]
    public void 선적계획완료Tick은_계획후보만만들고_예약신고통관선적을수행하지않는다()
    {
        var context = ReadyCandidateReadinessReview();
        var confirmed = ConfirmShipmentPlan(context, ShipmentPlan());

        var completed = Advance(context, confirmed, "command:tick.export-shipment-planned", 1);
        var plan = Assert.Single(completed.ExportShipmentPlans);

        Assert.Equal(Simulation수출선적계획상태Codes.PlannedCandidate, plan.StateCode);
        Assert.NotNull(plan.CompletedTick);
        Assert.Contains("NoCarrierBooking", plan.BoundaryCodes);
        Assert.Contains("NoExportDeclaration", plan.BoundaryCodes);
        Assert.Contains("NoCustomsClearance", plan.BoundaryCodes);
        Assert.Contains("NoVesselLoading", plan.BoundaryCodes);
        Assert.Equal(context.Session.Settlement!.TreasuryBalance,
            completed.Settlement!.TreasuryBalance);
    }

    [Fact]
    public void 선택한선적계획뒤에는_같은준비성검토의중복계획을차단한다()
    {
        var context = PlannedShipmentCandidate();
        var request = ShipmentPlan();
        request.PlanStableId = "export-shipment-plan:potato-2";
        request.DestinationMarketStableId = "market:sim.jp-osaka-1";

        var preview = context.Service.Preview수출선적계획(
            context.Session.SessionStableId, request);

        Assert.Contains("ExportShipmentPlanAlreadyScheduled",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void SaveReplay는_선택한목적시장과예상조건을_동일하게복원한다()
    {
        var context = PlannedShipmentCandidate();
        var package = context.Service.Save(context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.export-shipment-plan-1",
                ExpectedRevision = context.Session.Revision,
            });

        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        var plan = Assert.Single(restored.Session.ExportShipmentPlans);
        Assert.Equal(Simulation수출선적계획상태Codes.PlannedCandidate, plan.StateCode);
        Assert.Equal("JP", plan.DestinationCountryCode);
        Assert.Equal("market:sim.jp-tokyo-1", plan.DestinationMarketStableId);
        Assert.Equal(Simulation수출운송방식Codes.Ocean, plan.TransportModeCode);
        Assert.Equal(1_100_000m, plan.ExpectedNetRevenue);
        Assert.Equal(42, plan.RiskScore);
        Assert.Equal(context.Session.Settlement!.TreasuryBalance,
            restored.Session.Settlement!.TreasuryBalance);
    }

    [Fact]
    public void 선적실행Preview는_성공과손실재정후보를보이지만_결과와상태를만들지않는다()
    {
        var context = PlannedShipmentCandidate();

        var preview = context.Service.Preview수출선적실행(
            context.Session.SessionStableId, ShipmentExecution());
        var current = context.Service.Get(context.Session.SessionStableId);

        Assert.Equal(58m, preview.SuccessProbabilityPercent);
        Assert.Equal(450_000m, preview.PreviouslyRecognizedProjectedRevenue);
        Assert.Equal(650_000m, preview.SuccessTreasuryDeltaCandidate);
        Assert.Equal(-1_150_000m, preview.LossTreasuryDeltaCandidate);
        Assert.Equal(1_150_000m, preview.RequiredLossCapacityReservation);
        Assert.True(preview.OutcomeHiddenUntilCompletion);
        Assert.Contains("DeterministicScenarioOutcomeOnly", preview.BoundaryCodes);
        Assert.Empty(preview.CommonDecisionPreview.Decision.BlockReasonCodes);
        Assert.Empty(current.ExportShipmentExecutions);
        Assert.Equal(context.Session.Settlement!.TreasuryBalance,
            current.Settlement!.TreasuryBalance);
    }

    [Fact]
    public void 계획Task완료전에는_선적Simulation을시작할수없다()
    {
        var context = ScheduledShipmentPlan();

        var preview = context.Service.Preview수출선적실행(
            context.Session.SessionStableId, ShipmentExecution());

        Assert.Contains("ExportShipmentPlanNotCompleted",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void 선적실행Confirm은_최악손실여력만예약하고_결과를미리공개하지않는다()
    {
        var context = PlannedShipmentCandidate();

        var confirmed = ConfirmShipmentExecution(context, ShipmentExecution());
        var execution = Assert.Single(confirmed.ExportShipmentExecutions);
        var plan = Assert.Single(confirmed.ExportShipmentPlans);

        Assert.Equal(Simulation수출선적실행상태Codes.Scheduled, execution.StateCode);
        Assert.Equal(Simulation수출선적결과Codes.Pending, execution.OutcomeCode);
        Assert.Null(execution.OutcomeRoll);
        Assert.Equal(execution.ExecutionStableId, plan.ExecutionStableId);
        Assert.Equal(context.Session.Settlement!.TreasuryBalance,
            confirmed.Settlement!.TreasuryBalance);
        Assert.Equal(confirmed.Settlement.TreasuryBalance - 1_150_000m,
            confirmed.Settlement.TreasuryAvailable);
        Assert.Equal(300m,
            Assert.Single(confirmed.Settlement.HarvestLotAllocations).OutboundReservedQuantity);
    }

    [Fact]
    public void 선적실행은_첫WorldTick에운송중이되지만_재정과수량은아직확정하지않는다()
    {
        var context = ScheduledShipmentExecution();

        var moving = Advance(context, context.Session, "command:tick.export-shipment-moving", 1);
        var execution = Assert.Single(moving.ExportShipmentExecutions);

        Assert.Equal(Simulation수출선적실행상태Codes.InTransit, execution.StateCode);
        Assert.NotNull(execution.DepartedTick);
        Assert.Null(execution.OutcomeRoll);
        Assert.Equal(context.Session.Settlement!.TreasuryBalance,
            moving.Settlement!.TreasuryBalance);
        Assert.Equal(300m,
            Assert.Single(moving.Settlement.HarvestLotAllocations).OutboundReservedQuantity);
    }

    [Fact]
    public void 성공결과는_기존예상매출을중복하지않고_순수익차액과도착수량을한번반영한다()
    {
        var context = ScheduledShipmentExecution();

        var completed = Advance(context, context.Session, "command:tick.export-shipment-success", 7);
        var execution = Assert.Single(completed.ExportShipmentExecutions);
        var allocation = Assert.Single(completed.Settlement!.HarvestLotAllocations);

        Assert.Equal(Simulation수출선적실행상태Codes.DeliveredInSimulation,
            execution.StateCode);
        Assert.Equal(Simulation수출선적결과Codes.Delivered, execution.OutcomeCode);
        Assert.True(execution.OutcomeRoll > execution.RiskScore);
        Assert.Equal(300m, execution.DeliveredQuantity);
        Assert.Equal(0m, execution.LostQuantity);
        Assert.Equal(650_000m, execution.AppliedTreasuryDelta);
        Assert.Equal(context.Session.Settlement!.TreasuryBalance + 650_000m,
            completed.Settlement.TreasuryBalance);
        Assert.Equal(completed.Settlement.TreasuryBalance,
            completed.Settlement.TreasuryAvailable);
        Assert.Equal(0m, allocation.OutboundReservedQuantity);
        Assert.Contains(completed.Effects, value =>
            value.EffectTypeCode == "ExportShipmentDeliveredTreasuryReconciliation"
                && value.Delta == 650_000m);
    }

    [Fact]
    public void 손실결과는_기존예상매출을되돌리고_비용과손실수량을한번반영한다()
    {
        var context = PlannedShipmentCandidate();
        var request = ShipmentExecution("export-shipment-execution:potato-3");
        var confirmed = ConfirmShipmentExecution(
            context, request, "command:export-shipment-execution.loss");

        var completed = Advance(context, confirmed, "command:tick.export-shipment-loss", 7);
        var execution = Assert.Single(completed.ExportShipmentExecutions);

        Assert.Equal(Simulation수출선적실행상태Codes.DisruptedWithLossInSimulation,
            execution.StateCode);
        Assert.Equal(Simulation수출선적결과Codes.DisruptedWithLoss, execution.OutcomeCode);
        Assert.True(execution.OutcomeRoll <= execution.RiskScore);
        Assert.Equal(0m, execution.DeliveredQuantity);
        Assert.Equal(300m, execution.LostQuantity);
        Assert.Equal(-1_150_000m, execution.AppliedTreasuryDelta);
        Assert.Equal(context.Session.Settlement!.TreasuryBalance - 1_150_000m,
            completed.Settlement!.TreasuryBalance);
        Assert.Equal(0m,
            Assert.Single(completed.Settlement.HarvestLotAllocations).OutboundReservedQuantity);
        Assert.Contains(completed.Effects, value =>
            value.EffectTypeCode == "ExportShipmentLossTreasuryReconciliation"
                && value.Delta == -1_150_000m);
    }

    [Fact]
    public void 실행을예약한계획은_두번실행할수없다()
    {
        var context = ScheduledShipmentExecution();
        var request = ShipmentExecution("export-shipment-execution:potato-2");

        var preview = context.Service.Preview수출선적실행(
            context.Session.SessionStableId, request);

        Assert.Contains("ExportShipmentPlanAlreadyExecuted",
            preview.CommonDecisionPreview.Decision.BlockReasonCodes);
    }

    [Fact]
    public void SaveReplay는_결정적성공결과와재정수량을_동일하게복원한다()
    {
        var context = ScheduledShipmentExecution();
        var completed = Advance(context, context.Session, "command:tick.export-shipment-save", 7);
        var package = context.Service.Save(context.Session.SessionStableId,
            new SimulationSessionSaveRequest
            {
                SaveStableId = "save:sim.export-shipment-execution-1",
                ExpectedRevision = completed.Revision,
            });

        var restoredService = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), context.SaveStore);
        var restored = restoredService.Restore(new SimulationSessionRestoreRequest
        {
            SaveStableId = package.SaveStableId,
        });

        Assert.Equal(package.ReplayHash, restored.ReplayHash);
        var execution = Assert.Single(restored.Session.ExportShipmentExecutions);
        Assert.Equal(Simulation수출선적실행상태Codes.DeliveredInSimulation,
            execution.StateCode);
        Assert.Equal(650_000m, execution.AppliedTreasuryDelta);
        Assert.Equal(execution.ExecutionStableId,
            Assert.Single(restored.Session.ExportShipmentPlans).ExecutionStableId);
        Assert.Equal(0m,
            Assert.Single(restored.Session.Settlement!.HarvestLotAllocations)
                .OutboundReservedQuantity);
        Assert.Contains(restored.Session.Effects, value =>
            value.EffectTypeCode == "ExportShipmentDeliveredTreasuryReconciliation");
    }

    [Fact]
    public void 수확판로결과는_성공한외부교역의전체계보와최종효과를_한카드로투영한다()
    {
        var context = DeliveredShipmentOutcome();
        var before = context.Service.Get(context.Session.SessionStableId);

        var outcome = context.Service.Get수확판로결과(
            context.Session.SessionStableId, "harvest-lot:potato-1");
        var after = context.Service.Get(context.Session.SessionStableId);
        var selected = Assert.Single(outcome.Routes, value => value.IsSelected);

        Assert.Equal(4, outcome.Routes.Length);
        Assert.Equal(SimulationHarvestDispositionChoiceCodes.ExportAgent,
            outcome.SelectedChoiceCode);
        Assert.Equal(Simulation수확판로단계Codes.ExportDelivered,
            selected.CurrentStageCode);
        Assert.Equal(300m, selected.ResolvedQuantity);
        Assert.Equal(300m, selected.ExportDeliveredQuantity);
        Assert.Equal(0m, selected.ExportLostQuantity);
        Assert.Equal(1_010_000m, selected.RecognizedTreasuryDelta);
        Assert.Equal(Simulation수출선적결과Codes.Delivered, selected.RiskResultCode);
        Assert.Contains("export-shipment-execution:potato-1", selected.RelatedStableIds);
        Assert.All(outcome.Routes.Where(value => !value.IsSelected), value =>
            Assert.Equal(Simulation수확판로단계Codes.NotSelected, value.CurrentStageCode));
        Assert.Equal(before.Revision, after.Revision);
        Assert.Equal(before.Settlement!.TreasuryBalance, after.Settlement!.TreasuryBalance);
        Assert.Contains("ProjectionOnly", outcome.BoundaryCodes);
        Assert.Contains("NoStateMutation", outcome.BoundaryCodes);
    }

    [Fact]
    public void 수확판로결과는_외부교역손실의수량과누적재정효과를구분한다()
    {
        var context = LostShipmentOutcome();

        var outcome = context.Service.Get수확판로결과(
            context.Session.SessionStableId, "harvest-lot:potato-1");
        var selected = Assert.Single(outcome.Routes, value => value.IsSelected);

        Assert.Equal(Simulation수확판로단계Codes.ExportDisruptedWithLoss,
            selected.CurrentStageCode);
        Assert.Equal(300m, selected.ResolvedQuantity);
        Assert.Equal(0m, selected.ExportDeliveredQuantity);
        Assert.Equal(300m, selected.ExportLostQuantity);
        Assert.Equal(-790_000m, selected.RecognizedTreasuryDelta);
        Assert.Equal(Simulation수출선적결과Codes.DisruptedWithLoss,
            selected.RiskResultCode);
        Assert.Contains("ShipmentOutcome:DisruptedWithLoss", selected.RiskCodes);
    }

    [Fact]
    public void 수확판로결과는_직판의시장공급과재정효과를선택경로에만표시한다()
    {
        var context = ReadyAllocation(
            SimulationHarvestDispositionChoiceCodes.DirectOnlineSale,
            SimulationHarvestDispositionWorkflowCodes.ProducerPackingCandidate, 3);

        var outcome = context.Service.Get수확판로결과(
            context.Session.SessionStableId, "harvest-lot:potato-1");
        var selected = Assert.Single(outcome.Routes, value => value.IsSelected);

        Assert.Equal(SimulationHarvestDispositionChoiceCodes.DirectOnlineSale,
            selected.ChoiceCode);
        Assert.Equal(Simulation수확판로단계Codes.DirectMarketSupplyAvailable,
            selected.CurrentStageCode);
        Assert.Equal(300m, selected.MarketSuppliedQuantity);
        Assert.Equal(300m, selected.ResolvedQuantity);
        Assert.Equal(300_000m, selected.RecognizedTreasuryDelta);
        Assert.Equal(300m, outcome.CurrentProductMarketSupplyQuantity);
        Assert.All(outcome.Routes.Where(value => !value.IsSelected), value =>
            Assert.Equal(0m, value.RecognizedTreasuryDelta));
    }

    [Fact]
    public void 수확판로결과는_보관Lot과비축수량을같은수확물에연결한다()
    {
        var context = ReadyAllocation(
            SimulationHarvestDispositionChoiceCodes.ReserveStorage,
            SimulationHarvestDispositionWorkflowCodes.ReserveStockLotCandidate,
            1,
            request =>
            {
                request.Settlement!.StorageOccupied = 1000m;
                request.Settlement.ReserveStockLots = new[]
                {
                    new SimulationReserveStockLotRequest
                    {
                        StockLotStableId = "stock-lot:sim.potato-basis-1",
                        ProductStableId = "product:potato",
                        StorageFacilityStableId = "facility:sim.storage-1",
                        Quantity = 1000m,
                        UnitCode = "KGM",
                        FoodEquivalentQuantity = 1200m,
                        SourceStableIds = new[] { "source:fixture.reserve-basis-1" },
                    },
                };
            });

        var outcome = context.Service.Get수확판로결과(
            context.Session.SessionStableId, "harvest-lot:potato-1");
        var selected = Assert.Single(outcome.Routes, value => value.IsSelected);

        Assert.Equal(Simulation수확판로단계Codes.ReserveStored, selected.CurrentStageCode);
        Assert.Equal(294m, selected.StoredQuantity);
        Assert.Equal(294m, selected.ResolvedQuantity);
        Assert.Equal(6m, selected.RemainingQuantity);
        Assert.Equal(-15_000m, selected.RecognizedTreasuryDelta);
        Assert.Equal(1294m, outcome.CurrentProductReserveQuantity);
        Assert.Contains("stock-lot:candidate:harvest-lot:potato-1:reserve-storage",
            selected.RelatedStableIds);
    }

    [Fact]
    public void 수확판로결과는_조합선택후아직인수전인상태를완료로과장하지않는다()
    {
        var context = ReadyAllocation(
            SimulationHarvestDispositionChoiceCodes.CooperativeShipment,
            SimulationHarvestDispositionWorkflowCodes.CooperativeIntakeCandidate, 2);

        var outcome = context.Service.Get수확판로결과(
            context.Session.SessionStableId, "harvest-lot:potato-1");
        var selected = Assert.Single(outcome.Routes, value => value.IsSelected);

        Assert.Equal(Simulation수확판로단계Codes.CooperativeIntakeCandidate,
            selected.CurrentStageCode);
        Assert.Equal(0m, selected.ResolvedQuantity);
        Assert.Equal(300m, selected.RemainingQuantity);
        Assert.Equal(210_000m, selected.RecognizedTreasuryDelta);
        Assert.Contains("CooperativeSettlementDelay", selected.RiskCodes);
    }

    [Fact]
    public void 수확판로결과목록은_현재Session의배분Lot만반환한다()
    {
        var context = DeliveredShipmentOutcome();

        var outcomes = context.Service.Get수확판로결과목록(context.Session.SessionStableId);

        var outcome = Assert.Single(outcomes);
        Assert.Equal("harvest-lot:potato-1", outcome.HarvestLotStableId);
        Assert.Equal(context.Session.Revision, outcome.WorldRevision);
    }

    [Fact]
    public void 배분되지않은HarvestLot은_판로결과조회에서찾을수없다()
    {
        var context = DeliveredShipmentOutcome();

        var error = Assert.Throws<SimulationNotFoundException>(() =>
            context.Service.Get수확판로결과(
                context.Session.SessionStableId, "harvest-lot:missing"));

        Assert.Equal("SimulationHarvestRouteOutcomeNotFound", error.ErrorCode);
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

    private static Context ScheduledCargoPreparation()
    {
        var context = PassedPreparation();
        var scheduled = ConfirmCargoPreparation(context, CargoPreparation());
        return new Context(context.Service, context.SaveStore, scheduled);
    }

    private static Context ReadyCargoPreparation()
    {
        var context = ScheduledCargoPreparation();
        var ready = Advance(context, context.Session, "command:tick.export-cargo-prepared", 1);
        return new Context(context.Service, context.SaveStore, ready);
    }

    private static Context ScheduledCargoHandoff()
    {
        var context = ReadyCargoPreparation();
        var scheduled = ConfirmCargoHandoff(context, CargoHandoff());
        return new Context(context.Service, context.SaveStore, scheduled);
    }

    private static Context HandedOffCargo()
    {
        var context = ScheduledCargoHandoff();
        var handedOff = Advance(context, context.Session, "command:tick.export-cargo-handed-off", 1);
        return new Context(context.Service, context.SaveStore, handedOff);
    }

    private static Context MovingExportCargo()
    {
        var context = HandedOffCargo();
        var confirmed = ConfirmExportMovement(context, ExportMovement());
        var moving = Advance(context, confirmed, "command:tick.export-port-moving", 1);
        return new Context(context.Service, context.SaveStore, moving);
    }

    private static Context ArrivedExportCargo()
    {
        var context = HandedOffCargo();
        var confirmed = ConfirmExportMovement(context, ExportMovement());
        var arrived = Advance(context, confirmed, "command:tick.export-port-arrived", 2);
        return new Context(context.Service, context.SaveStore, arrived);
    }

    private static Context ScheduledPortReceipt()
    {
        var context = ArrivedExportCargo();
        var scheduled = ConfirmPortReceipt(context, PortReceipt());
        return new Context(context.Service, context.SaveStore, scheduled);
    }

    private static Context ReceivedAtPortStaging()
    {
        var context = ScheduledPortReceipt();
        var received = Advance(context, context.Session, "command:tick.export-port-received-ready", 1);
        return new Context(context.Service, context.SaveStore, received);
    }

    private static Context ActionRequiredReadinessReview()
    {
        var context = ReceivedAtPortStaging();
        var confirmed = ConfirmReadinessReview(context, ReadinessReview(false, true));
        var completed = Advance(
            context, confirmed, "command:tick.export-readiness-action-required", 1);
        return new Context(context.Service, context.SaveStore, completed);
    }

    private static Context ReadyCandidateReadinessReview()
    {
        var context = ReceivedAtPortStaging();
        var confirmed = ConfirmReadinessReview(context, ReadinessReview(true, true));
        var completed = Advance(
            context, confirmed, "command:tick.export-readiness-candidate", 1);
        return new Context(context.Service, context.SaveStore, completed);
    }

    private static Context PlannedShipmentCandidate()
    {
        var context = ReadyCandidateReadinessReview();
        var confirmed = ConfirmShipmentPlan(context, ShipmentPlan());
        var completed = Advance(
            context, confirmed, "command:tick.export-shipment-candidate", 1);
        return new Context(context.Service, context.SaveStore, completed);
    }

    private static Context ScheduledShipmentPlan()
    {
        var context = ReadyCandidateReadinessReview();
        var scheduled = ConfirmShipmentPlan(context, ShipmentPlan());
        return new Context(context.Service, context.SaveStore, scheduled);
    }

    private static Context ScheduledShipmentExecution()
    {
        var context = PlannedShipmentCandidate();
        var scheduled = ConfirmShipmentExecution(context, ShipmentExecution());
        return new Context(context.Service, context.SaveStore, scheduled);
    }

    private static Context DeliveredShipmentOutcome()
    {
        var context = ScheduledShipmentExecution();
        var completed = Advance(
            context, context.Session, "command:tick.export-shipment-outcome-delivered", 7);
        return new Context(context.Service, context.SaveStore, completed);
    }

    private static Context LostShipmentOutcome()
    {
        var context = PlannedShipmentCandidate();
        var confirmed = ConfirmShipmentExecution(
            context,
            ShipmentExecution("export-shipment-execution:potato-3"),
            "command:export-shipment-execution.outcome-loss");
        var completed = Advance(
            context, confirmed, "command:tick.export-shipment-outcome-loss", 7);
        return new Context(context.Service, context.SaveStore, completed);
    }

    private static Context ReadyAllocation(
        string choiceCode,
        string workflowCode,
        int ticks,
        Action<경영SimulationSession생성Request>? configure = null)
    {
        var saveStore = new InMemorySimulationSessionSaveStore();
        var service = new 경영SimulationSessionService(
            new InMemory경영SimulationSessionStore(), saveStore);
        var createRequest = CreateRequest();
        configure?.Invoke(createRequest);
        var created = service.Create(createRequest);
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

    private static 경영SimulationSessionSnapshot ConfirmCargoHandoff(
        Context context,
        Simulation수출Cargo인계PreviewRequest handoff)
        => context.Service.Confirm수출Cargo인계(
            context.Session.SessionStableId,
            new Simulation수출Cargo인계ConfirmRequest
            {
                CommandId = "command:export-cargo-handoff.confirm-1",
                ExpectedRevision = context.Session.Revision,
                Handoff = handoff,
            });

    private static 경영SimulationSessionSnapshot ConfirmExportMovement(
        Context context,
        SimulationLogisticsMovementPreviewRequest movement)
        => context.Service.ConfirmLogisticsMovement(
            context.Session.SessionStableId,
            new SimulationLogisticsMovementConfirmRequest
            {
                CommandId = "command:export-logistics.confirm-1",
                ExpectedRevision = context.Session.Revision,
                Movement = movement,
            });

    private static 경영SimulationSessionSnapshot ConfirmPortReceipt(
        Context context,
        Simulation수출항만인수PreviewRequest receipt)
        => context.Service.Confirm수출항만인수(
            context.Session.SessionStableId,
            new Simulation수출항만인수ConfirmRequest
            {
                CommandId = "command:export-port-receipt.confirm-1",
                ExpectedRevision = context.Session.Revision,
                Receipt = receipt,
            });

    private static 경영SimulationSessionSnapshot ConfirmReadinessReview(
        Context context,
        Simulation수출준비성검토PreviewRequest review,
        string commandId = "command:export-readiness.confirm-1")
        => context.Service.Confirm수출준비성검토(
            context.Session.SessionStableId,
            new Simulation수출준비성검토ConfirmRequest
            {
                CommandId = commandId,
                ExpectedRevision = context.Session.Revision,
                Review = review,
            });

    private static 경영SimulationSessionSnapshot ConfirmShipmentPlan(
        Context context,
        Simulation수출선적계획PreviewRequest plan)
        => context.Service.Confirm수출선적계획(
            context.Session.SessionStableId,
            new Simulation수출선적계획ConfirmRequest
            {
                CommandId = "command:export-shipment-plan.confirm-1",
                ExpectedRevision = context.Session.Revision,
                Plan = plan,
            });

    private static 경영SimulationSessionSnapshot ConfirmShipmentExecution(
        Context context,
        Simulation수출선적실행PreviewRequest execution,
        string commandId = "command:export-shipment-execution.confirm-1")
        => context.Service.Confirm수출선적실행(
            context.Session.SessionStableId,
            new Simulation수출선적실행ConfirmRequest
            {
                CommandId = commandId,
                ExpectedRevision = context.Session.Revision,
                Execution = execution,
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

    private static Simulation수출Cargo인계PreviewRequest CargoHandoff()
        => new()
        {
            HandoffStableId = "export-cargo-handoff:potato-1",
            SourceCargoPreparationStableId = "export-cargo-preparation:potato-1",
            ReceivingFacilityStableId = "facility:sim.export-staging-1",
            ActorStableId = "actor:sim.export-agent-1",
            RequiredHandoffTicks = 1,
            SourceStableIds = new[] { "source:fixture.export-handoff-1" },
        };

    private static SimulationLogisticsMovementPreviewRequest ExportMovement()
        => new()
        {
            CargoStableId = "cargo:sim.export-potato-1",
            CargoRevision = 1,
            SourceExportCargoHandoffStableId = "export-cargo-handoff:potato-1",
            SourceAllocationStableId = "allocation:harvest-lot:harvest-lot:potato-1",
            HarvestLotStableId = "harvest-lot:potato-1",
            PackageLotStableId = "package-lot-candidate:export:export-preparation:potato-1",
            ProductStableId = "product:potato",
            Quantity = 300m,
            UnitCode = "KGM",
            RouteStableId = "route:sim.export-staging-port-1",
            OriginFacilityStableId = "facility:sim.export-staging-1",
            DestinationFacilityStableId = "facility:sim.port-staging-1",
            ActorStableId = "actor:sim.export-agent-1",
            RequiredRouteTicks = 2,
            SourceStableIds = new[] { "source:fixture.export-logistics-1" },
        };

    private static Simulation수출항만인수PreviewRequest PortReceipt()
        => new()
        {
            ReceiptStableId = "export-port-receipt:potato-1",
            CargoStableId = "cargo:sim.export-potato-1",
            ReceivingFacilityStableId = "facility:sim.port-staging-1",
            ActorStableId = "actor:sim.port-receiver-1",
            RequiredReceivingTicks = 1,
            SourceStableIds = new[] { "source:fixture.export-port-receipt-1" },
        };

    private static Simulation수출준비성검토PreviewRequest ReadinessReview(
        bool documentsPrepared,
        bool inspectionPreparationReady,
        string reviewStableId = "export-readiness-review:potato-1")
        => new()
        {
            ReviewStableId = reviewStableId,
            SourcePortReceiptStableId = "export-port-receipt:potato-1",
            ReviewingFacilityStableId = "facility:sim.port-staging-1",
            ActorStableId = "actor:sim.export-readiness-reviewer-1",
            DocumentsPrepared = documentsPrepared,
            InspectionPreparationReady = inspectionPreparationReady,
            RequiredReviewTicks = 1,
            SourceStableIds = new[] { "source:fixture.export-readiness-1" },
        };

    private static Simulation수출선적계획PreviewRequest ShipmentPlan()
        => new()
        {
            PlanStableId = "export-shipment-plan:potato-1",
            SourceReadinessReviewStableId = "export-readiness-review:potato-1",
            DestinationCountryCode = "JP",
            DestinationMarketStableId = "market:sim.jp-tokyo-1",
            TransportModeCode = Simulation수출운송방식Codes.Ocean,
            PlanningFacilityStableId = "facility:sim.port-staging-1",
            ActorStableId = "actor:sim.export-planner-1",
            ExpectedGrossRevenue = 1_800_000m,
            ExpectedInternationalLogisticsCost = 400_000m,
            ExpectedHandlingCost = 200_000m,
            ExpectedOtherCost = 100_000m,
            CurrencyCode = "KRW",
            EstimatedTransitTicks = 7,
            RiskScore = 42,
            RequiredPlanningTicks = 1,
            SourceStableIds = new[] { "source:fixture.export-shipment-plan-1" },
        };

    private static Simulation수출선적실행PreviewRequest ShipmentExecution(
        string executionStableId = "export-shipment-execution:potato-1")
        => new()
        {
            ExecutionStableId = executionStableId,
            SourceShipmentPlanStableId = "export-shipment-plan:potato-1",
            ExecutionFacilityStableId = "facility:sim.port-staging-1",
            ActorStableId = "actor:sim.export-shipment-operator-1",
            SourceStableIds = new[] { "source:fixture.export-shipment-execution-1" },
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
                    Facility("facility:sim.port-staging-1", "PortStaging", "district:sim.logistics-1"),
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
