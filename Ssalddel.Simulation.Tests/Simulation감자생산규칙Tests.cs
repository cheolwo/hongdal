using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class Simulation감자생산규칙Tests
{
    private readonly Simulation감자생산규칙 rule = new();

    [Fact]
    public void 단일Tile재배군집은_명시면적과Fixture생산성으로_300kg수확후보를만든다()
    {
        var preview = rule.CreatePreview(Request());

        Assert.Equal(100m, preview.EffectiveCultivationAreaSquareMeters);
        Assert.Equal(300m, preview.BaseHarvestQuantityKilograms);
        Assert.Equal(300m, preview.ExpectedHarvestQuantityKilograms);
        Assert.Equal("tile:farm.potato-1", preview.TileStableId);
        Assert.Equal("harvest-lot:potato.fixture-1", preview.HarvestLotStableId);
        Assert.Equal(Simulation업무규칙영역Codes.Production,
            preview.PendingEffectBundle.RuleDomainCode);
        var line = Assert.Single(preview.PendingEffectBundle.Lines);
        Assert.Equal(Simulation자원변동유형Codes.Production, line.MutationKindCode);
        Assert.Equal(Simulation자원효과역할Codes.Output, line.RoleCode);
        Assert.Equal(300m, line.Delta);
        Assert.Equal("kg", line.UnitCode);
    }

    [Fact]
    public void 환경과투입시설손실계수는_규칙범위안에서만수확량에반영된다()
    {
        var request = Request();
        request.CultivationUnit.EffectiveCultivationAreaRatio = 0.8m;
        request.EnvironmentFactor = 0.75m;
        request.InputFactor = 1.1m;
        request.FacilityFactor = 1.05m;
        request.LossFactor = 0.9m;

        var preview = rule.CreatePreview(request);

        Assert.Equal(80m, preview.EffectiveCultivationAreaSquareMeters);
        Assert.Equal(240m, preview.BaseHarvestQuantityKilograms);
        Assert.Equal(187.11m, preview.ExpectedHarvestQuantityKilograms);
    }

    [Fact]
    public void 수확후보효과는_Task완료Tick에원자적으로적용할수있다()
    {
        var preview = rule.CreatePreview(Request());
        var state = new Simulation자원원장상태Snapshot
        {
            Revision = 4,
            WorldTick = 11,
            SourceStableIds = new[] { "session:potato-production" },
        };

        var result = new Simulation자원효과적용기().Apply(
            state,
            preview.PendingEffectBundle,
            preview.CompletedTick);

        var ledger = Assert.Single(result.State.Ledgers);
        Assert.Equal(300m, ledger.Value);
        Assert.Equal("harvest-lot:potato.fixture-1", ledger.LotStableId);
        Assert.Equal(12, result.AppliedEffectBundle.AppliedTick);
        Assert.Equal(SimulationEffectStateCodes.Applied, result.AppliedEffectBundle.StateCode);
    }

    [Fact]
    public void 수확준비가아닌재배단위는_생산효과를만들지않는다()
    {
        var request = Request();
        request.CultivationUnit.StateCode = Simulation재배단위상태Codes.Growing;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreatePreview(request));

        Assert.Equal("SimulationCultivationUnitNotHarvestReady", error.ErrorCode);
    }

    [Fact]
    public void 완료되지않은수확Task는_생산효과를만들지않는다()
    {
        var request = Request();
        request.TaskStateCode = SimulationTaskStateCodes.InProgress;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreatePreview(request));

        Assert.Equal("SimulationPotatoProductionTaskNotCompleted", error.ErrorCode);
    }

    [Fact]
    public void 물리면적이없는Tile은_생산량을추정하지않는다()
    {
        var request = Request();
        request.CultivationUnit.PhysicalAreaSquareMeters = 0m;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreatePreview(request));

        Assert.Equal("SimulationCultivationUnitNotHarvestReady", error.ErrorCode);
    }

    [Fact]
    public void 관측자료를검토없이생산규칙으로승격하지않는다()
    {
        var request = Request();
        request.Rule.SourceTypeCode = "PublicObservation";

        var error = Assert.Throws<SimulationContractException>(() => rule.CreatePreview(request));

        Assert.Equal("SimulationPotatoProductionRuleInvalid", error.ErrorCode);
    }

    [Fact]
    public void 규칙범위를넘는시설보너스는_차단한다()
    {
        var request = Request();
        request.FacilityFactor = 1.3m;

        var error = Assert.Throws<SimulationContractException>(() => rule.CreatePreview(request));

        Assert.Equal("SimulationPotatoProductionFacilityFactorInvalid", error.ErrorCode);
    }

    [Fact]
    public void 생산효과는_재배단위_환경_Decision_Task_출처를보존한다()
    {
        var preview = rule.CreatePreview(Request());
        var sources = preview.PendingEffectBundle.SourceStableIds;

        Assert.Contains("cultivation-unit:potato.tile-1", sources);
        Assert.Contains("environment-snapshot:potato.day-90", sources);
        Assert.Contains("decision:potato-harvest.fixture-1", sources);
        Assert.Contains("task:potato-harvest.fixture-1", sources);
        Assert.Contains("source:fixture.potato-production", sources);
        Assert.Contains("limitation:fixture-not-operational", sources);
    }

    private static Simulation감자생산Request Request()
        => new()
        {
            EffectBundleStableId = "effect-bundle:potato-production.fixture-1",
            EffectLineStableId = "effect-line:potato-production.output.fixture-1",
            DecisionStableId = "decision:potato-harvest.fixture-1",
            DecisionStateCode = SimulationDecisionStateCodes.Confirmed,
            CompletedTaskStableId = "task:potato-harvest.fixture-1",
            TaskStateCode = SimulationTaskStateCodes.Completed,
            CompletedTick = 12,
            HarvestLotStableId = "harvest-lot:potato.fixture-1",
            HarvestLedgerStableId = "harvest-stock:potato.fixture-1",
            EnvironmentSnapshotStableId = "environment-snapshot:potato.day-90",
            EnvironmentFactor = 1m,
            InputFactor = 1m,
            FacilityFactor = 1m,
            LossFactor = 1m,
            SourceStableIds = new[]
            {
                "source:fixture.potato-production",
                "limitation:fixture-not-operational",
            },
            CultivationUnit = new Simulation재배단위Snapshot
            {
                CultivationUnitStableId = "cultivation-unit:potato.tile-1",
                Revision = 3,
                TileStableId = "tile:farm.potato-1",
                CultivationStableId = "cultivation:potato.fixture-1",
                ProductStableId = "product:potato",
                CropVariantStableId = "crop-variant:potato.fixture",
                StateCode = Simulation재배단위상태Codes.HarvestReady,
                PhysicalAreaSquareMeters = 100m,
                EffectiveCultivationAreaRatio = 1m,
                SourceStableIds = new[] { "source:fixture.cultivation-unit" },
            },
            Rule = new Simulation감자생산RuleSnapshot
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
                Limitations = new[] { "실제 농업 생산성 또는 운영 수확량으로 사용하지 않는다." },
            },
        };
}
