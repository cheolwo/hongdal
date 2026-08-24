using Ssalddel.Unity.Exhibition;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class RuleSeedbedExperimentTests
{
    private readonly RuleSeedbedCoordinator coordinator = new();

    [Fact]
    public void 통합대장은_생산_소비_운송_창고_표현다섯규칙영역을제공한다()
    {
        var scenarios = IntegratedRuleSeedbedCatalog.Create();

        Assert.Equal(5, scenarios.Length);
        Assert.Equal(
            new[]
            {
                RuleSeedbedDomainCodes.Consumption,
                RuleSeedbedDomainCodes.Presentation,
                RuleSeedbedDomainCodes.Production,
                RuleSeedbedDomainCodes.Transport,
                RuleSeedbedDomainCodes.Warehouse,
            },
            scenarios.Select(value => value.RuleDomainCode).OrderBy(value => value).ToArray());
        Assert.All(scenarios, value => Assert.NotEmpty(value.SeedbedObjectStableIds));
    }

    [Fact]
    public void 모든실험은_실운영API를호출하지않는다()
    {
        var scenarios = IntegratedRuleSeedbedCatalog.Create();

        Assert.All(scenarios, value => Assert.True(value.DoesNotCallOperationalApi));
    }

    [Fact]
    public void 실험시작은_서버기준상태를준비하되변경하지않는다()
    {
        var baseline = Baseline();
        var session = coordinator.Begin(ConsumptionScenario(), baseline);

        Assert.Equal(RuleSeedbedPhaseCodes.Ready, session.PhaseCode);
        Assert.Same(baseline, session.Baseline);
        Assert.Equal(300m, session.Baseline.Values.Single().Value);
        Assert.False(session.CanonicalStateMutatedByPresentation);
    }

    [Fact]
    public void 미리보기는_예상효과를보여주지만기준상태를변경하지않는다()
    {
        var session = coordinator.Begin(ConsumptionScenario(), Baseline());

        coordinator.LoadPreview(session, ConsumptionPreview());

        Assert.Equal(RuleSeedbedPhaseCodes.PreviewLoaded, session.PhaseCode);
        Assert.Equal(280m, session.Preview!.Effects.Single().AfterValue);
        Assert.Equal(300m, session.Baseline.Values.Single().Value);
        Assert.False(session.CanonicalStateMutatedByPresentation);
    }

    [Fact]
    public void Simulation확정요청만으로는_기준상태가바뀌지않는다()
    {
        var session = PreviewLoadedSession();

        coordinator.RequestSimulationConfirm(session, "simulation-command:consume-potato.1");

        Assert.Equal(RuleSeedbedPhaseCodes.AwaitingCanonicalRefresh, session.PhaseCode);
        Assert.Equal(300m, session.Baseline.Values.Single().Value);
        Assert.Null(session.Refreshed);
    }

    [Fact]
    public void 서버재조회후에만_미리보기와실제값을대조한다()
    {
        var session = PreviewLoadedSession();
        coordinator.RequestSimulationConfirm(session, "simulation-command:consume-potato.1");

        coordinator.ApplyCanonicalRefresh(session, Refreshed(280m));

        var comparison = Assert.Single(session.Comparisons);
        Assert.Equal(RuleSeedbedPhaseCodes.Reconciled, session.PhaseCode);
        Assert.Equal(300m, comparison.BeforeValue);
        Assert.Equal(-20m, comparison.ActualDeltaValue);
        Assert.Equal(280m, comparison.RefreshedValue);
        Assert.True(comparison.MatchesPreview);
    }

    [Fact]
    public void 서버재조회값이미리보기와다르면_조화완료로판정하지않는다()
    {
        var session = PreviewLoadedSession();
        coordinator.RequestSimulationConfirm(session, "simulation-command:consume-potato.1");

        Assert.Throws<InvalidOperationException>(() =>
            coordinator.ApplyCanonicalRefresh(session, Refreshed(285m)));
        Assert.Equal(RuleSeedbedPhaseCodes.AwaitingCanonicalRefresh, session.PhaseCode);
    }

    [Fact]
    public void 미리보기이전값이_서버기준상태와다르면거부한다()
    {
        var session = coordinator.Begin(ConsumptionScenario(), Baseline());
        var preview = ConsumptionPreview();
        preview.Effects.Single().BeforeValue = 301m;
        preview.Effects.Single().AfterValue = 281m;

        Assert.Throws<InvalidOperationException>(() => coordinator.LoadPreview(session, preview));
        Assert.Equal(RuleSeedbedPhaseCodes.Ready, session.PhaseCode);
    }

    [Fact]
    public void 표현규칙은_비권위효과만미리볼수있고Simulation확정은할수없다()
    {
        var scenario = IntegratedRuleSeedbedCatalog.Create()
            .Single(value => value.RuleDomainCode == RuleSeedbedDomainCodes.Presentation);
        var session = coordinator.Begin(scenario, Baseline());
        var preview = PresentationPreview(scenario, false);

        coordinator.LoadPreview(session, preview);

        Assert.Equal(RuleSeedbedPhaseCodes.PreviewLoaded, session.PhaseCode);
        Assert.False(session.Preview!.Effects.Single().IsCanonicalResourceEffect);
        Assert.Throws<InvalidOperationException>(() =>
            coordinator.RequestSimulationConfirm(session, "simulation-command:not-allowed"));
    }

    [Fact]
    public void 표현규칙이_기준원장효과를주장하면거부한다()
    {
        var scenario = IntegratedRuleSeedbedCatalog.Create()
            .Single(value => value.RuleDomainCode == RuleSeedbedDomainCodes.Presentation);
        var session = coordinator.Begin(scenario, Baseline());

        Assert.Throws<InvalidOperationException>(() =>
            coordinator.LoadPreview(session, PresentationPreview(scenario, true)));
    }

    [Fact]
    public void 초기화는_같은기준상태에서다시실험할수있게중간결과를지운다()
    {
        var session = PreviewLoadedSession();

        coordinator.Reset(session);

        Assert.Equal(RuleSeedbedPhaseCodes.Ready, session.PhaseCode);
        Assert.Null(session.Preview);
        Assert.Null(session.Refreshed);
        Assert.Empty(session.Comparisons);
        Assert.Equal(300m, session.Baseline.Values.Single().Value);
    }

    private RuleSeedbedSessionSnapshot PreviewLoadedSession()
    {
        var session = coordinator.Begin(ConsumptionScenario(), Baseline());
        return coordinator.LoadPreview(session, ConsumptionPreview());
    }

    private static RuleSeedbedScenarioDescriptor ConsumptionScenario()
        => IntegratedRuleSeedbedCatalog.Create()
            .Single(value => value.RuleDomainCode == RuleSeedbedDomainCodes.Consumption);

    private static RuleSeedbedCanonicalStateSnapshot Baseline()
        => new()
        {
            SnapshotStableId = "snapshot:market.1",
            Revision = 1,
            WorldTick = 10,
            IsServerAuthoritative = true,
            Values = new[]
            {
                new RuleSeedbedResourceValueSnapshot
                {
                    LedgerStableId = "market-stock:potato.available",
                    Value = 300m,
                    Unit = "kg",
                },
            },
            SourceStableIds = new[] { "source:simulation-server" },
        };

    private static RuleSeedbedCanonicalStateSnapshot Refreshed(decimal value)
        => new()
        {
            SnapshotStableId = "snapshot:market.2",
            Revision = 2,
            WorldTick = 11,
            IsServerAuthoritative = true,
            Values = new[]
            {
                new RuleSeedbedResourceValueSnapshot
                {
                    LedgerStableId = "market-stock:potato.available",
                    Value = value,
                    Unit = "kg",
                },
            },
            SourceStableIds = new[] { "source:simulation-server" },
        };

    private static RuleSeedbedPreviewSnapshot ConsumptionPreview()
        => new()
        {
            PreviewStableId = "preview:market-consumption.1",
            ScenarioStableId = "rule-seedbed:consumption.market-resident",
            BasedOnRevision = 1,
            RuleStableId = "rule:market-resident-consumption.resource.v1",
            Effects = new[]
            {
                new RuleSeedbedEffectSnapshot
                {
                    EffectStableId = "effect:market-stock-potato.1",
                    StepCode = "Consumption",
                    TargetStableId = "market-stock:potato.available",
                    BeforeValue = 300m,
                    DeltaValue = -20m,
                    AfterValue = 280m,
                    Unit = "kg",
                    IsCanonicalResourceEffect = true,
                    SourceStableIds = new[] { "source:consumption-rule-preview" },
                },
            },
            BlockingReasonCodes = Array.Empty<string>(),
            SourceStableIds = new[] { "source:simulation-server-preview" },
        };

    private static RuleSeedbedPreviewSnapshot PresentationPreview(
        RuleSeedbedScenarioDescriptor scenario,
        bool isCanonicalResourceEffect)
        => new()
        {
            PreviewStableId = "preview:presentation.1",
            ScenarioStableId = scenario.ScenarioStableId,
            BasedOnRevision = 1,
            RuleStableId = scenario.RuleStableId,
            Effects = new[]
            {
                new RuleSeedbedEffectSnapshot
                {
                    EffectStableId = "effect:camera-focus.1",
                    StepCode = "Camera",
                    TargetStableId = "presentation-output:camera.focus",
                    BeforeValue = 0m,
                    DeltaValue = 1m,
                    AfterValue = 1m,
                    Unit = "state",
                    IsCanonicalResourceEffect = isCanonicalResourceEffect,
                    SourceStableIds = new[] { "source:presentation-rule-preview" },
                },
            },
            BlockingReasonCodes = Array.Empty<string>(),
            SourceStableIds = new[] { "source:presentation-rule-catalog" },
        };
}
