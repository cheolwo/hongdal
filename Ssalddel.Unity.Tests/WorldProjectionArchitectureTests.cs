using Ssalddel.Unity.Evidence;
using Ssalddel.Unity.Sensors;
using Ssalddel.Unity.WorldProjection;

namespace Ssalddel.Tests.UnityData;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class WorldProjectionArchitectureTests
{
    [Fact]
    public void 대표_PageProjectionCatalog는_중복과_안전경계_오류가_없다()
    {
        var errors = PageWorldProjectionCatalog.Validate();

        Assert.Empty(errors);
        Assert.InRange(PageWorldProjectionCatalog.RepresentativeRoutes.Count, 15, 20);
    }

    [Fact]
    public void 운영_Command는_명시적_확인과_Canonical_재조회가_필수다()
    {
        var operational = PageWorldProjectionCatalog.RepresentativeRoutes
            .Where(item => item.InteractionEffectCode == WorldInteractionEffectCodes.ServerCommand)
            .ToArray();

        Assert.NotEmpty(operational);
        Assert.All(operational, item => Assert.True(item.RequiresExplicitConfirmation));
        Assert.All(operational, item => Assert.True(item.RequiresCanonicalStateRefresh));
    }

    [Fact]
    public void 운송_과업은_차고가_아니라_도심물류센터_Zone에_배치한다()
    {
        var transportRoutes = PageWorldProjectionCatalog.RepresentativeRoutes
            .Where(item => item.RoutePattern.StartsWith("/driver/transports/", StringComparison.Ordinal)
                           || string.Equals(item.RoutePattern, "/shipper/request", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(transportRoutes);
        Assert.All(
            transportRoutes,
            item => Assert.Equal(WorldZoneCodes.UrbanLogisticsCenter, item.WorldZoneCode));
    }

    [Fact]
    public void 민감한_계좌_Page는_Unity_World가_아니라_Web으로_인계한다()
    {
        var definition = PageWorldProjectionCatalog.Find("/driver/account/bank");

        Assert.NotNull(definition);
        Assert.Equal(PageProjectionStageCodes.KeepWeb, definition.ProjectionStageCode);
        Assert.Equal(WorldInteractionEffectCodes.WebHandoff, definition.InteractionEffectCode);
        Assert.Contains(PageProjectionTypeCodes.KeepWeb, definition.ProjectionTypeCodes);
    }

    [Theory]
    [InlineData(SensorConditionCodes.Normal, "Online", "Stable")]
    [InlineData(SensorConditionCodes.Dry, "Online", "Warning")]
    [InlineData(SensorConditionCodes.Waterlogged, "Online", "Warning")]
    [InlineData(SensorConditionCodes.Stale, "Stale", "Stale")]
    [InlineData(SensorConditionCodes.Offline, "Offline", "Off")]
    public void SensorState가_관측상태와_근거를_장비표현으로_Projection한다(
        string conditionCode,
        string expectedEquipment,
        string expectedIndicator)
    {
        var state = SensorState(conditionCode);

        var projection = new SensorProjectionResolver().Resolve(state);

        Assert.Equal(state.SensorId, projection.SensorId);
        Assert.Equal(state.Revision, projection.Revision);
        Assert.Equal(state.Interpretation.EvidenceCardIds, projection.EvidenceCardIds);
        Assert.Equal(expectedEquipment, projection.VisualState.EquipmentStateCode);
        Assert.Equal(expectedIndicator, projection.VisualState.IndicatorStateCode);
        Assert.False(string.IsNullOrWhiteSpace(projection.VisualState.MaterialStateCode));
    }

    [Fact]
    public void SensorProjection은_측정값을_재해석하지_않고_상위_판정상태만_표현한다()
    {
        var dry = SensorState(SensorConditionCodes.Dry);
        var normal = SensorState(SensorConditionCodes.Normal);
        dry.Value = 20m;
        normal.Value = 20m;

        var resolver = new SensorProjectionResolver();
        var dryProjection = resolver.Resolve(dry);
        var normalProjection = resolver.Resolve(normal);

        Assert.NotEqual(dryProjection.VisualState.MaterialStateCode, normalProjection.VisualState.MaterialStateCode);
        Assert.Equal(dry.Interpretation.EvidenceCardIds, dryProjection.EvidenceCardIds);
    }

    [Fact]
    public void WorldProjection은_StableId로_추가_갱신_제거_유지를_계산한다()
    {
        var current = new[]
        {
            Projection("observation:keep", 1, "Normal"),
            Projection("observation:update", 1, "Normal"),
            Projection("observation:remove", 1, "Normal"),
        };
        var incoming = new[]
        {
            Projection("observation:keep", 1, "Normal"),
            Projection("observation:update", 2, "Dry"),
            Projection("observation:add", 1, "Normal"),
        };

        var changes = new WorldProjectionReconciler().Reconcile(current, incoming);

        Assert.Equal("observation:add", Assert.Single(changes.Added).StableId);
        Assert.Equal("observation:update", Assert.Single(changes.Updated).StableId);
        Assert.Equal("observation:remove", Assert.Single(changes.Removed).StableId);
        Assert.Equal("observation:keep", Assert.Single(changes.Unchanged).StableId);
    }

    [Fact]
    public void WorldProjection은_중복_ID와_낮은_Revision을_거부한다()
    {
        var reconciler = new WorldProjectionReconciler();
        var duplicate = new[]
        {
            Projection("observation:same", 1, "Normal"),
            Projection("observation:same", 2, "Dry"),
        };

        var duplicateError = Assert.Throws<InvalidOperationException>(
            () => reconciler.Reconcile(Array.Empty<WorldObjectProjection>(), duplicate));
        Assert.StartsWith("DuplicateStableId:", duplicateError.Message);

        var lowerRevisionError = Assert.Throws<InvalidOperationException>(
            () => reconciler.Reconcile(
                new[] { Projection("observation:one", 2, "Normal") },
                new[] { Projection("observation:one", 1, "Normal") }));
        Assert.StartsWith("LowerRevision:", lowerRevisionError.Message);
    }

    [Fact]
    public void 연구근거Card는_주장_제품해석_Unity표현과_한계를_분리한다()
    {
        var card = new 연구근거Card
        {
            EvidenceCardId = "evidence:soil-water-001",
            Title = "특정 토양과 작물의 이용 가능 수분",
            SourceReferences = new[] { "source:soil-water-paper-001" },
            ResearchScope = "대표 토양과 작물의 특정 생육 단계",
            SupportedClaim = "자료가 직접 뒷받침하는 관측 범위",
            ProductInterpretation = "Normal, Dry, Critical 판정 규칙",
            UnityVisualTranslation = "토양 젖음과 균열 표현",
            Limitations = new[] { "다른 토양과 생육 단계에 자동 일반화하지 않음" },
            EvidenceVersion = "1.0.0",
            EffectiveAt = DateTimeOffset.Parse("2026-08-07T00:00:00+09:00"),
        };

        var errors = new 연구근거Validator().Validate(card);

        Assert.Empty(errors);
    }

    private static 농장SensorState SensorState(string conditionCode)
    {
        return new 농장SensorState
        {
            SensorId = "sensor:soil-moisture-001",
            Revision = 7,
            SourceTypeCode = SensorSourceTypeCodes.SimulatedFixture,
            MeasurementTypeCode = "volumetric-water-content",
            Value = 20m,
            Unit = "%",
            ObservedAt = DateTimeOffset.Parse("2026-08-07T08:30:00+09:00"),
            DataStatusCode = "Success",
            ConditionCode = conditionCode,
            Interpretation = new ProjectionRule근거Reference
            {
                RuleKey = "rule:soil-water-001",
                RuleVersion = "1.0.0",
                EvidenceCardIds = new[] { "evidence:soil-water-001" },
                ConfidenceCode = EvidenceConfidenceCodes.Medium,
                LimitationSummary = "대표 시나리오 범위에서만 적용",
            },
        };
    }

    private static WorldObjectProjection Projection(string stableId, long revision, string displayState)
    {
        return new WorldObjectProjection
        {
            StableId = stableId,
            Revision = revision,
            WorldZoneCode = WorldZoneCodes.PublicDataHall,
            WorldObjectKey = "observation-marker",
            DisplayStateCode = displayState,
            DataStatusCode = "Success",
            EvidenceCardIds = new[] { "evidence:soil-water-001" },
            ProjectedAt = DateTimeOffset.Parse("2026-08-07T08:30:00+09:00"),
        };
    }
}
