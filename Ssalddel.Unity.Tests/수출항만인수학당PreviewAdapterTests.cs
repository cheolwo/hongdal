using System.Text.Json;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Unity.Learning;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class 수출항만인수학당PreviewAdapterTests
{
    private readonly 수출항만인수학당PreviewAdapter adapter = new();
    private readonly 저녁학당업무Preview보강Projector projector = new();

    [Fact]
    public void CARD_BIZ_1B_서버Route를정확히보존한다()
    {
        Assert.Equal(
            "api/simulation/v1/sessions/simulation-session:sim.potato-1/export-port-receipt-previews",
            수출항만인수ApiRoutes.Preview("simulation-session:sim.potato-1"));
    }

    [Fact]
    public void CARD_BIZ_1B_UnityRuntime은서버ContractAssembly를직접참조하지않는다()
    {
        var runtimeReferences = typeof(수출항만인수학당PreviewAdapter).Assembly
            .GetReferencedAssemblies();

        Assert.DoesNotContain(runtimeReferences,
            value => value.Name == "Ssalddel.Simulation.Contracts");
    }

    [Fact]
    public void CARD_BIZ_1B_실제서버WirePreview를바보의미확인질문으로연결한다()
    {
        var contract = ContractPreview();
        var api = RoundTrip(contract);
        var inner = Inner(내면규칙Codes.BeginnerMind);

        var input = adapter.Map(api, 42, inner, 내면규칙Codes.BeginnerMind);
        var result = projector.Project(input);

        Assert.Equal(contract.ProductStableId, result.ProductStableId);
        Assert.Equal(300m, result.Quantity);
        Assert.Equal("KGM", result.UnitCode);
        Assert.Contains(contract.SourceExportCargoHandoffStableId,
            result.CanonicalSourceStableIds);
        Assert.Contains(contract.PackageLotStableId, result.CanonicalSourceStableIds);
        Assert.Equal(2, result.RevealedUnknowns.Length);
        Assert.Contains(result.RevealedUnknowns, value =>
            value.QuestionText.Contains("통관", StringComparison.Ordinal));
        Assert.Empty(result.MilestoneEvidence);
    }

    [Fact]
    public void CARD_BIZ_1B_실제서버WirePreview를전차의Milestone으로연결한다()
    {
        var contract = ContractPreview();
        var input = adapter.Map(RoundTrip(contract), 42,
            Inner(내면규칙Codes.IntegratedProgress),
            내면규칙Codes.IntegratedProgress);

        var result = projector.Project(input);

        Assert.Equal(new[]
        {
            "ExportAllocationVerified",
            "HandedOffInSimulation",
            "ArrivedAtDestination",
            "Previewed",
        }, result.MilestoneEvidence.Select(value => value.StateCode));
        Assert.Equal(contract.ReceiptStableId,
            result.MilestoneEvidence[^1].SourceStableIds[^1]);
        Assert.Equal(300m, result.Quantity);
        Assert.False(result.MayMutateCanonicalState);
        Assert.False(result.MayChangeAllowedIntents);
    }

    [Fact]
    public void CARD_BIZ_1B_차단된서버Preview는학습효과로우회하지않는다()
    {
        var contract = ContractPreview();
        contract.CommonDecisionPreview.Decision.BlockReasonCodes =
            new[] { "ExportPortCargoNotArrived" };

        var error = Assert.Throws<InvalidOperationException>(() => adapter.Map(
            RoundTrip(contract), 42, Inner(내면규칙Codes.BeginnerMind),
            내면규칙Codes.BeginnerMind));

        Assert.Equal(
            "EveningExportPortPreviewBlocked:ExportPortCargoNotArrived",
            error.Message);
    }

    [Fact]
    public void CARD_BIZ_1B_운영경계가빠진WirePreview를거부한다()
    {
        var contract = ContractPreview();
        contract.BoundaryCodes = contract.BoundaryCodes
            .Where(value => value != "NoCustomsClearance").ToArray();

        var error = Assert.Throws<InvalidOperationException>(() => adapter.Map(
            RoundTrip(contract), 42, Inner(내면규칙Codes.BeginnerMind),
            내면규칙Codes.BeginnerMind));

        Assert.Equal(
            "EveningExportPortPreviewBoundaryInvalid:NoCustomsClearance",
            error.Message);
    }

    private static 수출항만인수PreviewApiModel RoundTrip(
        Simulation수출항만인수PreviewSnapshot contract)
        => JsonSerializer.Deserialize<수출항만인수PreviewApiModel>(
            JsonSerializer.Serialize(contract))
            ?? throw new InvalidOperationException("ExportPortPreviewWireRoundTripFailed");

    private static 플레이어내면상태Snapshot Inner(string activeRuleCode)
        => new()
        {
            알아차림 = 1,
            의지 = 1,
            ActiveRuleCodes = new[] { activeRuleCode },
        };

    private static Simulation수출항만인수PreviewSnapshot ContractPreview()
        => new()
        {
            ReceiptStableId = "export-port-receipt:potato-1",
            CargoStableId = "cargo:sim.export-potato-1",
            SourceExportCargoHandoffStableId = "export-cargo-handoff:potato-1",
            SourceAllocationStableId = "allocation:harvest-lot:harvest-lot:potato-1",
            HarvestLotStableId = "harvest-lot:potato-1",
            PackageLotStableId =
                "package-lot-candidate:export:export-preparation:potato-1",
            ProductStableId = "product:potato",
            Quantity = 300m,
            UnitCode = "KGM",
            ReceivingFacilityStableId = "facility:sim.port-staging-1",
            IsCandidateOnly = true,
            DoesNotCreateCustomsOperation = true,
            BoundaryCodes = new[]
            {
                "SimulationOnly",
                "PortStagingReceiptOnly",
                "NoExportDeclaration",
                "NoOfficialInspection",
                "NoCustomsClearance",
                "NoVesselLoading",
                "ExportReadinessRequiresSeparateDecision",
            },
            CommonDecisionPreview = new SimulationDecisionPreviewSnapshot
            {
                Decision = new SimulationDecisionSnapshot
                {
                    DecisionStableId =
                        "decision:export-port-receiving:export-port-receipt:potato-1",
                    DecisionTypeCode = "ExportPortReceiving",
                    StateCode = SimulationDecisionStateCodes.Previewed,
                    Revision = 0,
                    SessionStableId = "simulation-session:sim.potato-1",
                    ActorStableId = "actor:sim.port-receiver-1",
                    TargetStableIds = new[]
                    {
                        "export-port-receipt:potato-1",
                        "cargo:sim.export-potato-1",
                        "facility:sim.port-staging-1",
                    },
                    Uncertainties = new[]
                    {
                        "Port staging receipt does not create an export operation.",
                    },
                    BlockReasonCodes = Array.Empty<string>(),
                    SourceStableIds = new[]
                    {
                        "source:fixture.export-port-receipt-1",
                        "cargo:sim.export-potato-1",
                        "export-cargo-handoff:potato-1",
                    },
                },
                TaskPlan = new SimulationTaskPlanSnapshot
                {
                    TaskStableId =
                        "task:export-port-receiving:export-port-receipt:potato-1",
                    TaskTypeCode = "ExportPortReceiving",
                    FacilityStableId = "facility:sim.port-staging-1",
                    AssignedCapacity = 300m,
                    AssignedCapacityUnitCode = "KGM",
                    DurationTicks = 1,
                    InputLotStableIds = new[] { "cargo:sim.export-potato-1" },
                    OutputCandidateCodes = new[]
                    {
                        "export-readiness-review-required",
                    },
                    SourceStableIds = new[]
                    {
                        "source:fixture.export-port-receipt-1",
                        "cargo:sim.export-potato-1",
                        "export-cargo-handoff:potato-1",
                    },
                },
            },
        };
}
