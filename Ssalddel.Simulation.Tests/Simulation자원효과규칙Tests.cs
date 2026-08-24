using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class Simulation자원효과규칙Tests
{
    private readonly Simulation자원효과묶음Validator validator = new();
    private readonly Simulation자원효과적용기 applicator = new();

    [Fact]
    public void 생산은_출력원장을증가시키고_규칙과원인을보존한다()
    {
        var bundle = Bundle(Line(
            "effect-line:harvest.output",
            Simulation자원변동유형Codes.Production,
            Simulation자원효과역할Codes.Output,
            "harvest-stock:potato",
            0m,
            300m,
            300m));

        validator.Validate(bundle);

        Assert.Equal("rule:resource.potato-harvest.v1", bundle.RuleStableId);
        Assert.Equal("decision:potato-harvest", bundle.CausedByDecisionStableId);
        Assert.Equal("task:potato-harvest", bundle.CausedByTaskStableId);
    }

    [Fact]
    public void 예약은_전체량을바꾸지않고_가용량과예약량사이에서이동한다()
    {
        var available = Line(
            "effect-line:order.available",
            Simulation자원변동유형Codes.Reservation,
            Simulation자원효과역할Codes.Available,
            "market-stock:potato.available",
            300m,
            -20m,
            280m,
            "conservation:order.potato-20",
            -20m);
        var reserved = Line(
            "effect-line:order.reserved",
            Simulation자원변동유형Codes.Reservation,
            Simulation자원효과역할Codes.Reserved,
            "market-stock:potato.reserved",
            0m,
            20m,
            20m,
            "conservation:order.potato-20",
            20m);

        validator.Validate(Bundle(available, reserved));
    }

    [Fact]
    public void 이동은_원천과대상과명시적손실의합을보존한다()
    {
        var source = Line(
            "effect-line:transfer.source",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Source,
            "farm-stock:potato",
            100m,
            -100m,
            0m,
            "conservation:transfer.potato",
            -100m);
        var target = Line(
            "effect-line:transfer.target",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Target,
            "hub-stock:potato",
            0m,
            96m,
            96m,
            "conservation:transfer.potato",
            96m);
        var loss = Line(
            "effect-line:transfer.loss",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Loss,
            "loss-ledger:potato",
            0m,
            4m,
            4m,
            "conservation:transfer.potato",
            4m);

        validator.Validate(Bundle(source, target, loss));
    }

    [Fact]
    public void 변환은_서로다른원장단위를_공통보존단위로검증한다()
    {
        var input = Line(
            "effect-line:packing.input",
            Simulation자원변동유형Codes.Transformation,
            Simulation자원효과역할Codes.Input,
            "harvest-stock:potato",
            100m,
            -100m,
            0m,
            "conservation:packing.potato",
            -100m);
        var output = Line(
            "effect-line:packing.output",
            Simulation자원변동유형Codes.Transformation,
            Simulation자원효과역할Codes.Output,
            "package-stock:potato.box",
            0m,
            5m,
            5m,
            "conservation:packing.potato",
            100m,
            unitCode: "box");

        validator.Validate(Bundle(input, output));
    }

    [Fact]
    public void 이전값과변화량과이후값이맞지않으면거부한다()
    {
        var line = Line(
            "effect-line:invalid.arithmetic",
            Simulation자원변동유형Codes.Production,
            Simulation자원효과역할Codes.Output,
            "harvest-stock:potato",
            0m,
            300m,
            299m);

        var error = Assert.Throws<SimulationContractException>(() => validator.Validate(Bundle(line)));

        Assert.Equal("SimulationResourceValueConservationInvalid", error.ErrorCode);
    }

    [Fact]
    public void 생산을감소효과로기록하면거부한다()
    {
        var line = Line(
            "effect-line:invalid.production-sign",
            Simulation자원변동유형Codes.Production,
            Simulation자원효과역할Codes.Output,
            "harvest-stock:potato",
            300m,
            -20m,
            280m);

        var error = Assert.Throws<SimulationContractException>(() => validator.Validate(Bundle(line)));

        Assert.Equal("SimulationResourceEffectRoleSignMismatch", error.ErrorCode);
    }

    [Fact]
    public void 이동보존량이맞지않으면거부한다()
    {
        var source = Line(
            "effect-line:invalid-transfer.source",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Source,
            "farm-stock:potato",
            100m,
            -100m,
            0m,
            "conservation:invalid-transfer",
            -100m);
        var target = Line(
            "effect-line:invalid-transfer.target",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Target,
            "hub-stock:potato",
            0m,
            99m,
            99m,
            "conservation:invalid-transfer",
            99m);

        var error = Assert.Throws<SimulationContractException>(() =>
            validator.Validate(Bundle(source, target)));

        Assert.Equal("SimulationResourceConservationImbalance", error.ErrorCode);
    }

    [Fact]
    public void 이동에대상역할이없으면거부한다()
    {
        var source = Line(
            "effect-line:missing-target.source",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Source,
            "farm-stock:potato",
            100m,
            -100m,
            0m,
            "conservation:missing-target",
            -100m);
        var loss = Line(
            "effect-line:missing-target.loss",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Loss,
            "loss-ledger:potato",
            0m,
            100m,
            100m,
            "conservation:missing-target",
            100m);

        var error = Assert.Throws<SimulationContractException>(() =>
            validator.Validate(Bundle(source, loss)));

        Assert.Equal("SimulationResourceConservationRolesMissing", error.ErrorCode);
    }

    [Fact]
    public void 같은효과선고유식별자를두번사용하면거부한다()
    {
        var first = Line(
            "effect-line:duplicate",
            Simulation자원변동유형Codes.Production,
            Simulation자원효과역할Codes.Output,
            "harvest-stock:potato",
            0m,
            100m,
            100m);
        var second = Line(
            "effect-line:duplicate",
            Simulation자원변동유형Codes.Production,
            Simulation자원효과역할Codes.Output,
            "harvest-stock:potato-second",
            0m,
            100m,
            100m);

        var error = Assert.Throws<SimulationContractException>(() =>
            validator.Validate(Bundle(first, second)));

        Assert.Equal("SimulationResourceEffectLineDuplicate", error.ErrorCode);
    }

    [Fact]
    public void Applied상태는적용Tick이필수이고_Pending에는허용하지않는다()
    {
        var line = Line(
            "effect-line:state",
            Simulation자원변동유형Codes.Reconciliation,
            Simulation자원효과역할Codes.Record,
            "market-stock:potato",
            280m,
            0m,
            280m);
        var appliedWithoutTick = Bundle(line);
        appliedWithoutTick.StateCode = SimulationEffectStateCodes.Applied;

        var appliedError = Assert.Throws<SimulationContractException>(() =>
            validator.Validate(appliedWithoutTick));

        var pendingWithTick = Bundle(line);
        pendingWithTick.AppliedTick = 2;
        var pendingError = Assert.Throws<SimulationContractException>(() =>
            validator.Validate(pendingWithTick));

        Assert.Equal("SimulationResourceEffectAppliedTickInvalid", appliedError.ErrorCode);
        Assert.Equal("SimulationResourceEffectAppliedTickInvalid", pendingError.ErrorCode);
    }

    [Theory]
    [InlineData(Simulation업무규칙영역Codes.Production)]
    [InlineData(Simulation업무규칙영역Codes.Consumption)]
    [InlineData(Simulation업무규칙영역Codes.Transport)]
    [InlineData(Simulation업무규칙영역Codes.Warehouse)]
    [InlineData(Simulation업무규칙영역Codes.Market)]
    [InlineData(Simulation업무규칙영역Codes.Facility)]
    [InlineData(Simulation업무규칙영역Codes.Time)]
    public void 업무자원효과묶음은_허용된규칙영역을명시한다(string ruleDomainCode)
    {
        var bundle = Bundle(Line(
            "effect-line:domain.reconciliation",
            Simulation자원변동유형Codes.Reconciliation,
            Simulation자원효과역할Codes.Record,
            "ledger:domain-check",
            0m,
            0m,
            0m));
        bundle.RuleDomainCode = ruleDomainCode;

        validator.Validate(bundle);
    }

    [Fact]
    public void 그래픽과카메라같은표현규칙은_자원효과영역으로허용하지않는다()
    {
        var bundle = Bundle(Line(
            "effect-line:presentation-domain.reconciliation",
            Simulation자원변동유형Codes.Reconciliation,
            Simulation자원효과역할Codes.Record,
            "ledger:presentation-domain-check",
            0m,
            0m,
            0m));
        bundle.RuleDomainCode = "Presentation.Camera";

        var error = Assert.Throws<SimulationContractException>(() => validator.Validate(bundle));

        Assert.Equal("SimulationResourceRuleDomainInvalid", error.ErrorCode);
    }

    [Fact]
    public void 효과묶음은_모든원장에원자적으로적용되고_입력상태를바꾸지않는다()
    {
        var state = LedgerState(
            Ledger("farm-stock:potato", 100m),
            Ledger("hub-stock:potato", 0m),
            Ledger("loss-ledger:potato", 0m));
        var source = Line(
            "effect-line:atomic.source",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Source,
            "farm-stock:potato",
            100m,
            -100m,
            0m,
            "conservation:atomic-transfer",
            -100m);
        var target = Line(
            "effect-line:atomic.target",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Target,
            "hub-stock:potato",
            0m,
            96m,
            96m,
            "conservation:atomic-transfer",
            96m);
        var loss = Line(
            "effect-line:atomic.loss",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Loss,
            "loss-ledger:potato",
            0m,
            4m,
            4m,
            "conservation:atomic-transfer",
            4m);

        var result = applicator.Apply(state, Bundle(source, target, loss), 3);

        Assert.Equal(100m, state.Ledgers.Single(value =>
            value.LedgerStableId == "farm-stock:potato").Value);
        Assert.Equal(0m, result.State.Ledgers.Single(value =>
            value.LedgerStableId == "farm-stock:potato").Value);
        Assert.Equal(96m, result.State.Ledgers.Single(value =>
            value.LedgerStableId == "hub-stock:potato").Value);
        Assert.Equal(4m, result.State.Ledgers.Single(value =>
            value.LedgerStableId == "loss-ledger:potato").Value);
        Assert.Equal(1, result.State.Revision);
        Assert.Equal(3, result.State.WorldTick);
        Assert.Equal(SimulationEffectStateCodes.Applied, result.AppliedEffectBundle.StateCode);
        Assert.Equal(3, result.AppliedEffectBundle.AppliedTick);
    }

    [Fact]
    public void 한원장의이전값이맞지않으면_전체효과를거부하고입력상태를보존한다()
    {
        var state = LedgerState(
            Ledger("farm-stock:potato", 100m),
            Ledger("hub-stock:potato", 1m));
        var source = Line(
            "effect-line:rollback.source",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Source,
            "farm-stock:potato",
            100m,
            -100m,
            0m,
            "conservation:rollback-transfer",
            -100m);
        var target = Line(
            "effect-line:rollback.target",
            Simulation자원변동유형Codes.Transfer,
            Simulation자원효과역할Codes.Target,
            "hub-stock:potato",
            0m,
            100m,
            100m,
            "conservation:rollback-transfer",
            100m);

        var error = Assert.Throws<SimulationContractException>(() =>
            applicator.Apply(state, Bundle(source, target), 2));

        Assert.Equal("SimulationResourceLedgerBeforeValueMismatch", error.ErrorCode);
        Assert.Equal(100m, state.Ledgers.Single(value =>
            value.LedgerStableId == "farm-stock:potato").Value);
        Assert.Equal(1m, state.Ledgers.Single(value =>
            value.LedgerStableId == "hub-stock:potato").Value);
        Assert.Empty(state.AppliedEffectBundleStableIds);
    }

    [Fact]
    public void 이미적용된효과묶음은_중복적용을거부한다()
    {
        var state = LedgerState(Ledger("harvest-stock:potato", 0m));
        state.AppliedEffectBundleStableIds = new[] { "effect-bundle:resource.potato-1" };
        var production = Line(
            "effect-line:duplicate-bundle.production",
            Simulation자원변동유형Codes.Production,
            Simulation자원효과역할Codes.Output,
            "harvest-stock:potato",
            0m,
            300m,
            300m);

        var error = Assert.Throws<SimulationContractException>(() =>
            applicator.Apply(state, Bundle(production), 1));

        Assert.Equal("SimulationResourceEffectBundleAlreadyApplied", error.ErrorCode);
        Assert.Equal(0m, state.Ledgers[0].Value);
    }

    [Fact]
    public void 원장의자원이나단위가효과선과다르면거부한다()
    {
        var state = LedgerState(Ledger("harvest-stock:potato", 0m));
        state.Ledgers[0].UnitCode = "box";
        var production = Line(
            "effect-line:binding.production",
            Simulation자원변동유형Codes.Production,
            Simulation자원효과역할Codes.Output,
            "harvest-stock:potato",
            0m,
            300m,
            300m);

        var error = Assert.Throws<SimulationContractException>(() =>
            applicator.Apply(state, Bundle(production), 1));

        Assert.Equal("SimulationResourceLedgerBindingMismatch", error.ErrorCode);
    }

    private static Simulation자원효과묶음Snapshot Bundle(
        params Simulation자원효과선Snapshot[] lines)
        => new()
        {
            EffectBundleStableId = "effect-bundle:resource.potato-1",
            RuleStableId = "rule:resource.potato-harvest.v1",
            RuleRevision = 1,
            RuleDomainCode = Simulation업무규칙영역Codes.Production,
            ModeCode = "Simulation",
            StateCode = SimulationEffectStateCodes.Pending,
            CausedByDecisionStableId = "decision:potato-harvest",
            CausedByTaskStableId = "task:potato-harvest",
            Lines = lines,
            SourceStableIds = new[] { "source:fixture.resource-rule" },
        };

    private static Simulation자원효과선Snapshot Line(
        string lineStableId,
        string mutationKindCode,
        string roleCode,
        string ledgerStableId,
        decimal before,
        decimal delta,
        decimal after,
        string? conservationGroupStableId = null,
        decimal conservationQuantity = 0m,
        string unitCode = "kg")
        => new()
        {
            EffectLineStableId = lineStableId,
            MutationKindCode = mutationKindCode,
            RoleCode = roleCode,
            ResourceTypeCode = "resource:potato",
            TargetLedgerStableId = ledgerStableId,
            ProductStableId = "product:potato",
            LotStableId = "harvest-lot:sim.potato",
            BeforeValue = before,
            Delta = delta,
            AfterValue = after,
            UnitCode = unitCode,
            ConservationGroupStableId = conservationGroupStableId,
            ConservationQuantity = conservationQuantity,
            ConservationUnitCode = conservationGroupStableId == null ? null : "kg",
            SourceStableIds = new[] { "source:fixture.resource-rule" },
        };

    private static Simulation자원원장상태Snapshot LedgerState(
        params Simulation자원원장항목Snapshot[] ledgers)
        => new()
        {
            Revision = 0,
            WorldTick = 0,
            Ledgers = ledgers,
            AppliedEffectBundleStableIds = Array.Empty<string>(),
            SourceStableIds = new[] { "source:fixture.resource-ledger" },
        };

    private static Simulation자원원장항목Snapshot Ledger(string stableId, decimal value)
        => new()
        {
            LedgerStableId = stableId,
            ResourceTypeCode = "resource:potato",
            ProductStableId = "product:potato",
            LotStableId = "harvest-lot:sim.potato",
            Value = value,
            UnitCode = "kg",
            SourceStableIds = new[] { "source:fixture.resource-ledger" },
        };
}
