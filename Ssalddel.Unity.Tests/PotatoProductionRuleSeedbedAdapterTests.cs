using Ssalddel.Unity.Exhibition;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class PotatoProductionRuleSeedbedAdapterTests
{
    private readonly PotatoProductionRuleSeedbedAdapter adapter = new();

    [Fact]
    public void 감자생산서버사본을_모판기준상태와300kg예상효과로변환한다()
    {
        var envelope = adapter.MapPreview(PreviewSource());

        Assert.Equal("감자 생산 규칙 실험대", envelope.Scenario.DisplayName);
        Assert.Equal(PotatoProductionRuleSeedbedCodes.ScenarioStableId,
            envelope.Preview.ScenarioStableId);
        Assert.Equal(100m, envelope.EffectiveCultivationAreaSquareMeters);
        Assert.Equal(300m, envelope.BaseHarvestQuantityKilograms);
        Assert.Equal(300m, envelope.ExpectedHarvestQuantityKilograms);
        Assert.Equal(0m, Assert.Single(envelope.Baseline.Values).Value);
        var effect = Assert.Single(envelope.Preview.Effects);
        Assert.Equal(0m, effect.BeforeValue);
        Assert.Equal(300m, effect.DeltaValue);
        Assert.Equal(300m, effect.AfterValue);
        Assert.True(effect.IsCanonicalResourceEffect);
    }

    [Fact]
    public void 생산량은Unity에서재계산하지않고_서버효과값을그대로보존한다()
    {
        var source = PreviewSource();
        source.EffectiveCultivationAreaSquareMeters = 80m;
        source.BaseHarvestQuantityKilograms = 240m;
        source.ExpectedHarvestQuantityKilograms = 187.11m;
        source.EffectLines[0].DeltaValue = 187.11m;
        source.EffectLines[0].AfterValue = 187.11m;

        var envelope = adapter.MapPreview(source);

        Assert.Equal(187.11m, envelope.ExpectedHarvestQuantityKilograms);
        Assert.Equal(187.11m, Assert.Single(envelope.Preview.Effects).AfterValue);
    }

    [Fact]
    public void 감자생산실험은_확정뒤적용효과가있는서버재조회값과대조한다()
    {
        var envelope = adapter.MapPreview(PreviewSource());
        var coordinator = new RuleSeedbedCoordinator();
        var session = coordinator.Begin(envelope.Scenario, envelope.Baseline);
        coordinator.LoadPreview(session, envelope.Preview);
        coordinator.RequestSimulationConfirm(session, "simulation-command:potato-production.fixture-1");

        var refreshed = adapter.MapCanonicalRefresh(
            CanonicalSource(),
            envelope.EffectBundleStableId);
        coordinator.ApplyCanonicalRefresh(session, refreshed);

        Assert.Equal(RuleSeedbedPhaseCodes.Reconciled, session.PhaseCode);
        var comparison = Assert.Single(session.Comparisons);
        Assert.Equal(300m, comparison.RefreshedValue);
        Assert.True(comparison.MatchesPreview);
        Assert.False(session.CanonicalStateMutatedByPresentation);
    }

    [Fact]
    public void 서버권위가아닌생산사본은거부한다()
    {
        var source = PreviewSource();
        source.IsServerAuthoritative = false;

        var error = Assert.Throws<InvalidOperationException>(() => adapter.MapPreview(source));

        Assert.Equal("RuleSeedbedPotatoAuthorityInvalid", error.Message);
    }

    [Fact]
    public void 기준개정번호와미리보기개정번호가다르면거부한다()
    {
        var source = PreviewSource();
        source.BasedOnRevision = 4;

        var error = Assert.Throws<InvalidOperationException>(() => adapter.MapPreview(source));

        Assert.Equal("RuleSeedbedPotatoRevisionInvalid", error.Message);
    }

    [Fact]
    public void 실운영Mode의생산효과는모판에들일수없다()
    {
        var source = PreviewSource();
        source.ModeCode = "Operational";

        var error = Assert.Throws<InvalidOperationException>(() => adapter.MapPreview(source));

        Assert.Equal("RuleSeedbedPotatoBoundaryInvalid", error.Message);
    }

    [Fact]
    public void 서버예상수확량과효과선수량이다르면거부한다()
    {
        var source = PreviewSource();
        source.ExpectedHarvestQuantityKilograms = 299m;

        var error = Assert.Throws<InvalidOperationException>(() => adapter.MapPreview(source));

        Assert.Equal("RuleSeedbedPotatoEffectInvalid", error.Message);
    }

    [Fact]
    public void 시나리오대장과다른규칙고유식별자는거부한다()
    {
        var source = PreviewSource();
        source.RuleStableId = "rule:potato-production.unknown";

        var error = Assert.Throws<InvalidOperationException>(() => adapter.MapPreview(source));

        Assert.Equal("RuleSeedbedPotatoRuleMismatch", error.Message);
    }

    [Fact]
    public void 서버재조회에예상효과적용기록이없으면대조하지않는다()
    {
        var source = CanonicalSource();
        source.AppliedEffectBundleStableIds = Array.Empty<string>();

        var error = Assert.Throws<InvalidOperationException>(() =>
            adapter.MapCanonicalRefresh(
                source,
                "effect-bundle:potato-production.fixture-1"));

        Assert.Equal("RuleSeedbedPotatoEffectBundleNotApplied", error.Message);
    }

    [Fact]
    public void 차단사유는보존되어Simulation확정을막는다()
    {
        var source = PreviewSource();
        source.BlockingReasonCodes = new[] { "HarvestTaskNotCompleted" };
        var envelope = adapter.MapPreview(source);
        var coordinator = new RuleSeedbedCoordinator();
        var session = coordinator.Begin(envelope.Scenario, envelope.Baseline);
        coordinator.LoadPreview(session, envelope.Preview);

        Assert.Throws<InvalidOperationException>(() =>
            coordinator.RequestSimulationConfirm(
                session,
                "simulation-command:potato-production.fixture-1"));
        Assert.Equal(RuleSeedbedPhaseCodes.PreviewLoaded, session.PhaseCode);
    }

    private static PotatoProductionRuleSeedbedPreviewApiSnapshot PreviewSource()
        => new()
        {
            SnapshotStableId = "snapshot:potato-production.before-1",
            Revision = 5,
            WorldTick = 11,
            IsServerAuthoritative = true,
            PreviewStableId = "preview:potato-production.fixture-1",
            BasedOnRevision = 5,
            EffectBundleStableId = "effect-bundle:potato-production.fixture-1",
            RuleStableId = "rule:potato-production.fixture.v1",
            RuleRevision = 1,
            RuleDomainCode = "Production",
            ModeCode = "Simulation",
            EffectStateCode = "Pending",
            CultivationUnitStableId = "cultivation-unit:potato.tile-1",
            TileStableId = "tile:farm.potato-1",
            HarvestLotStableId = "harvest-lot:potato.fixture-1",
            EffectiveCultivationAreaSquareMeters = 100m,
            BaseHarvestQuantityKilograms = 300m,
            ExpectedHarvestQuantityKilograms = 300m,
            EffectLines = new[]
            {
                new PotatoProductionRuleSeedbedEffectLineApiSnapshot
                {
                    EffectLineStableId = "effect-line:potato-production.output.fixture-1",
                    MutationKindCode = "Production",
                    RoleCode = "Output",
                    TargetLedgerStableId = "harvest-stock:potato.fixture-1",
                    BeforeValue = 0m,
                    DeltaValue = 300m,
                    AfterValue = 300m,
                    UnitCode = "kg",
                    SourceStableIds = new[]
                    {
                        "source:fixture.potato-production",
                        "environment-snapshot:potato.day-90",
                    },
                },
            },
            SourceStableIds = new[]
            {
                "source:simulation-server",
                "source:fixture.potato-production",
            },
            Limitations = new[]
            {
                "실제 농업 생산성 또는 운영 수확량으로 사용하지 않는다.",
            },
        };

    private static PotatoProductionRuleSeedbedCanonicalApiSnapshot CanonicalSource()
        => new()
        {
            SnapshotStableId = "snapshot:potato-production.after-1",
            Revision = 6,
            WorldTick = 12,
            IsServerAuthoritative = true,
            Ledgers = new[]
            {
                new PotatoProductionRuleSeedbedLedgerApiSnapshot
                {
                    LedgerStableId = "harvest-stock:potato.fixture-1",
                    Value = 300m,
                    UnitCode = "kg",
                },
            },
            AppliedEffectBundleStableIds = new[]
            {
                "effect-bundle:potato-production.fixture-1",
            },
            SourceStableIds = new[]
            {
                "source:simulation-server",
                "effect-bundle:potato-production.fixture-1",
            },
        };
}
