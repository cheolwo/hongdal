using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldUiPlanningTests
{
    private static readonly SimulationWorld업무규칙집결원장 Rules =
        PyeongchangSimulationWorld업무규칙CatalogFactory.Create(
            "world-build:test", new string('a', 64), "area-set:test");

    [Fact]
    public void Figma_역할과_업무단계를_규칙연결_UI기획으로_구성한다()
    {
        var plan = PyeongchangSimulationWorldUI기획Factory.Create(Rules);

        SimulationWorldUI기획Validator.Validate(plan, Rules);
        var first = SimulationWorldUI기획Validator.ComputeHash(plan, Rules);
        var second = SimulationWorldUI기획Validator.ComputeHash(plan, Rules);

        Assert.Equal(3, plan.DesignEvidence.Count);
        Assert.Equal(6, plan.Surfaces.Count);
        Assert.Equal(36, plan.InformationItems.Count);
        Assert.Equal(48, plan.StatePresentations.Count);
        Assert.Equal(18, plan.ActionCandidates.Count);
        Assert.Equal(11, plan.RuleBindings.Count);
        Assert.Equal(SimulationWorldUI기획Validator.CurrentSchemaVersion, plan.SchemaVersion);
        Assert.Equal(first, second);
        Assert.All(plan.InformationItems, x => Assert.Equal(nameof(SimulationWorldUIProjectionItem), x.SourceContractKey));
        Assert.Contains(plan.Surfaces, x => x.RoleCode == SimulationWorldUI역할Codes.주문자 && x.WorkflowStageCode == "DiscoverCompareParticipate");
        Assert.Contains(plan.Surfaces, x => x.RoleCode == SimulationWorldUI역할Codes.기사 && x.WorkflowStageCode == "AcceptLoadTransportUnload");
        Assert.All(plan.RuleBindings, uiBinding =>
        {
            var source = Assert.Single(Rules.Bindings, x => x.StableId == uiBinding.BusinessRuleBindingStableId);
            var surface = Assert.Single(plan.Surfaces, x => x.StableId == uiBinding.SurfaceStableId);
            Assert.Equal(source.FacilityStableId, surface.FacilityStableId);
            Assert.Equal(source.CapabilityCode, uiBinding.FacilityCapabilityCode);
        });
    }

    [Fact]
    public void 확정행동이_Preview와_명시적확인_기대개정을_누락하면_거부한다()
    {
        var plan = PyeongchangSimulationWorldUI기획Factory.Create(Rules);
        var confirm = plan.ActionCandidates.First(x => x.ActionKindCode == SimulationWorldUI행동종류Codes.확정);
        confirm.RequiresExpectedRevision = false;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            SimulationWorldUI기획Validator.Validate(plan, Rules));

        Assert.StartsWith(SimulationWorldUI기획Validator.InvalidCode, exception.Message);
    }

    [Fact]
    public void Figma_설계근거가_없는_화면영역은_거부한다()
    {
        var plan = PyeongchangSimulationWorldUI기획Factory.Create(Rules);
        plan.Surfaces[0].DesignEvidenceStableId = "missing";

        Assert.Throws<InvalidOperationException>(() =>
            SimulationWorldUI기획Validator.Validate(plan, Rules));
    }

    [Fact]
    public void UI와_원본업무규칙연결의_시설기능이_다르면_거부한다()
    {
        var plan = PyeongchangSimulationWorldUI기획Factory.Create(Rules);
        plan.RuleBindings[0].FacilityCapabilityCode = SimulationWorld시설기능Codes.소비;

        Assert.Throws<InvalidOperationException>(() =>
            SimulationWorldUI기획Validator.Validate(plan, Rules));
    }

    [Fact]
    public void 활성업무규칙연결이_UI에서_누락되면_거부한다()
    {
        var plan = PyeongchangSimulationWorldUI기획Factory.Create(Rules);
        plan.RuleBindings = plan.RuleBindings.Skip(1).ToArray();

        Assert.Throws<InvalidOperationException>(() =>
            SimulationWorldUI기획Validator.Validate(plan, Rules));
    }

    [Fact]
    public async Task JobShell은_업무규칙대장을_읽어_UI기획을_저장한다()
    {
        var store = new RecordingStore();
        var shell = new SimulationWorldUI기획JobShell(
            new FixedReader(),
            new PyeongchangSimulationWorldUI기획Assembler(),
            store);

        var result = await shell.실행Async(Rules.CatalogRevision, CancellationToken.None);

        Assert.True(result.Inserted);
        Assert.Equal(6, result.SurfaceCount);
        Assert.NotNull(store.Plan);
    }

    private sealed class FixedReader : ISimulationWorld업무규칙집결Reader
    {
        public Task<SimulationWorld업무규칙집결원장?> 조회Async(string revision, CancellationToken cancellationToken) =>
            Task.FromResult<SimulationWorld업무규칙집결원장?>(revision == Rules.CatalogRevision ? Rules : null);
    }

    private sealed class RecordingStore : ISimulationWorldUI기획Store
    {
        public SimulationWorldUI기획원장? Plan { get; private set; }
        public Task<SimulationWorldUI기획저장결과> 저장Async(SimulationWorldUI기획원장 plan, SimulationWorld업무규칙집결원장 rules, CancellationToken cancellationToken)
        {
            Plan = plan;
            return Task.FromResult(new SimulationWorldUI기획저장결과 { Inserted=true, CatalogRevision=plan.CatalogRevision, CatalogHashSha256=SimulationWorldUI기획Validator.ComputeHash(plan,rules), SurfaceCount=plan.Surfaces.Count, InformationItemCount=plan.InformationItems.Count, StatePresentationCount=plan.StatePresentations.Count, ActionCandidateCount=plan.ActionCandidates.Count, RuleBindingCount=plan.RuleBindings.Count });
        }
    }
}
