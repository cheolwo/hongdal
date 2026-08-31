using System.Globalization;
using System.Text.Json;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Unity.Cards;
using Ssalddel.Unity.Farm;
using Ssalddel.Unity.PresentationContracts;
using Xunit.Abstractions;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "연결 준비 관측의 미확보·결손·판본·소유/해제·비적용 및 두 분야 소비를 독립 시험한다.",
    Boundary = "합성 관측은 실제 AssetDatabase/Scene/논리E5 증거가 아니며 실제 상태를 주입하지 않는다.")]
public sealed class 표현연결PreflightTests(ITestOutputHelper output)
{
    private const string FixtureSha = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

    [Fact]
    public void 모든필수관측이일치해도_Ready는_E5완료가아니다()
    {
        var plan = Plan();
        var result = 표현연결Preflight.Review(plan, Observe(plan));
        Assert.Equal(표현연결Readiness.Ready, result.Readiness);
        Assert.All(result.Checks, x => Assert.Equal(표현연결Readiness.Ready, x.Readiness));
        Assert.False(result.IsE5Completion);
        Assert.Contains("NotEditor", result.EvidenceBoundary);
        Assert.Equal("crop:fixture", result.Target);
        Assert.Equal(64, result.ResultFingerprint.Length);
    }

    [Theory]
    [InlineData(표현연결항목.CandidatePath)]
    [InlineData(표현연결항목.VisualKey)]
    [InlineData(표현연결항목.CandidateFingerprint)]
    [InlineData(표현연결항목.Target)]
    [InlineData(표현연결항목.Session)]
    [InlineData(표현연결항목.StateRevision)]
    [InlineData(표현연결항목.PresentationRevision)]
    [InlineData(표현연결항목.Parent)]
    [InlineData(표현연결항목.Anchor)]
    [InlineData(표현연결항목.InteractionTarget)]
    [InlineData(표현연결항목.CreationOwnership)]
    [InlineData(표현연결항목.DisplayOwnership)]
    [InlineData(표현연결항목.SubscriptionOwnership)]
    [InlineData(표현연결항목.ReleaseCoverage)]
    public void 확인된_후보상태배치소유의_불일치는차단한다(표현연결항목 item)
    {
        var plan = Plan();
        var result = 표현연결Preflight.Review(plan, Observe(plan, item, "other"));
        Assert.Equal(표현연결Readiness.Blocked, result.Readiness);
        Assert.Contains(result.Checks, x => x.Item == item && x.Code == "ObservedMismatch");
        Assert.All(result.Checks, x => { Assert.NotEmpty(x.NextOwner); Assert.NotEmpty(x.EarliestReopenStage); });
    }

    [Theory]
    [InlineData(표현연결항목.CandidatePath)]
    [InlineData(표현연결항목.Component)]
    [InlineData(표현연결항목.Renderer)]
    [InlineData(표현연결항목.ReleaseCoverage)]
    public void 실제조회실패와_미확보는_다르게반환한다(표현연결항목 item)
    {
        var plan = Plan();
        var absent = 표현연결Preflight.Review(plan, Observe(plan, item, status: 표현연결ObservationStatus.Missing));
        var unknown = 표현연결Preflight.Review(plan, Observe(plan, item, status: 표현연결ObservationStatus.Unobserved));
        Assert.Equal(표현연결Readiness.Blocked, absent.Readiness);
        Assert.Equal(표현연결Readiness.Conditional, unknown.Readiness);
    }

    [Theory]
    [InlineData(표현연결항목.Component)]
    [InlineData(표현연결항목.Renderer)]
    [InlineData(표현연결항목.Collider)]
    [InlineData(표현연결항목.Bounds)]
    [InlineData(표현연결항목.Position)]
    [InlineData(표현연결항목.Parent)]
    [InlineData(표현연결항목.Anchor)]
    [InlineData(표현연결항목.InteractionTarget)]
    [InlineData(표현연결항목.CreationOwnership)]
    [InlineData(표현연결항목.DisplayOwnership)]
    [InlineData(표현연결항목.SubscriptionOwnership)]
    [InlineData(표현연결항목.ReleaseCoverage)]
    public void 건전성은_명칭일치만으로_확인하지않는다(표현연결항목 item)
    {
        var plan = Plan();
        Assert.Equal(표현연결Readiness.Blocked, 표현연결Preflight.Review(plan, Observe(plan, item, validity: false)).Readiness);
        Assert.Equal(표현연결Readiness.Conditional, 표현연결Preflight.Review(plan, Observe(plan, item, validity: null)).Readiness);
    }

    [Fact]
    public void 비적용은_사유가필요하고_논리E5를생략할수없다()
    {
        var valid = Replace(Plan(), 표현연결항목.Collider, "", false, "정적 카드이며 물리 충돌을 사용하지 않음");
        Assert.Equal(표현연결Readiness.Ready, 표현연결Preflight.Review(valid, Observe(valid)).Readiness);
        foreach (var invalid in new[] { Replace(Plan(), 표현연결항목.Collider, "", false),
            Replace(Plan(), 표현연결항목.LogicE5, "", false, "시험이라 생략"),
            Replace(Plan(), 표현연결항목.LogicE5, "E3") })
            Assert.Equal(표현연결Readiness.Blocked, 표현연결Preflight.Review(invalid, Observe(invalid)).Readiness);
        Assert.Equal(표현연결Readiness.Blocked, 표현연결Preflight.Review(Plan(), Observe(Plan(), 표현연결항목.Collider,
            status: 표현연결ObservationStatus.NotApplicable)).Readiness);
    }

    [Fact]
    public void 근거와관측문맥_미확보는Conditional이며_확인값을자동인증하지않는다()
    {
        var plan = Plan(); var rows = Observe(plan).Observations.ToArray();
        rows[0] = new(rows[0].Item, rows[0].Key, 표현연결ObservationStatus.Confirmed, rows[0].Value);
        Assert.Equal(표현연결Readiness.Conditional, 표현연결Preflight.Review(plan, new(plan.ContextFingerprint, rows)).Readiness);
        Assert.Equal(표현연결Readiness.Conditional, 표현연결Preflight.Review(plan, new("", Observe(plan).Observations)).Readiness);
        Assert.Equal(표현연결Readiness.Conditional, 표현연결Preflight.Review(null, null).Readiness);
        Assert.Equal(표현연결Readiness.Conditional, 표현연결Preflight.Review(new("r1", Array.Empty<표현연결Requirement>()), null).Readiness);
    }

    [Fact]
    public void 적용직전_준비판본이나위치가달라지면_이전관측은재검사한다()
    {
        var original = Plan(); var snapshot = Observe(original);
        foreach (var changed in new[] { new 표현연결Plan("fixture:r2", original.Requirements),
            Replace(original, 표현연결항목.CandidateFingerprint, "new-fingerprint"),
            Replace(original, 표현연결항목.StateRevision, "8"), Replace(original, 표현연결항목.Position, "other-pose") })
        {
            var result = 표현연결Preflight.Review(changed, snapshot);
            Assert.Equal(표현연결Readiness.Blocked, result.Readiness);
            Assert.Contains(result.Checks, x => x.Code == "ObservationContextChanged_RecheckRequired");
            Assert.NotEqual(original.ContextFingerprint, result.ContextFingerprint);
        }
    }

    [Fact]
    public void 입력과기존표현을_바꾸지않으며_결과도불변이고_순서문화권에결정적이다()
    {
        var requirements = Plan().Requirements.ToArray();
        var plan = new 표현연결Plan("fixture:r1", requirements);
        var rows = Observe(plan).Observations.ToArray(); var snapshot = new 표현연결관측Snapshot(plan.ContextFingerprint, rows);
        var before = JsonSerializer.Serialize(new { plan, snapshot });
        var existingPresentation = new object(); var original = existingPresentation;
        var result = 표현연결Preflight.Review(plan, snapshot);
        Assert.Equal(before, JsonSerializer.Serialize(new { plan, snapshot }));
        requirements[0] = new(표현연결항목.CandidatePath, "changed", "changed"); rows[0] = new(rows[0].Item, "changed", 표현연결ObservationStatus.Missing);
        Assert.Equal(before, JsonSerializer.Serialize(new { plan, snapshot }));
        Assert.Throws<NotSupportedException>(() => ((IList<표현연결Check>)result.Checks).Clear());
        var previousCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            var reversed = new 표현연결Plan("fixture:r1", plan.Requirements.Reverse());
            Assert.Equal(result.ResultFingerprint, 표현연결Preflight.Review(reversed,
                new(reversed.ContextFingerprint, snapshot.Observations.Reverse())).ResultFingerprint);
            표현연결Preflight.Review(plan, Observe(plan, 표현연결항목.CandidatePath, status: 표현연결ObservationStatus.Missing));
            Assert.Same(original, existingPresentation);
        }
        finally { CultureInfo.CurrentCulture = previousCulture; }
    }

    [Fact]
    public void 중복항목과_범위밖관측을_성공으로합치지않는다()
    {
        var plan = Plan(); var rows = Observe(plan).Observations;
        Assert.Equal(표현연결Readiness.Blocked, 표현연결Preflight.Review(plan, new(plan.ContextFingerprint, rows.Append(rows[0]))).Readiness);
        var duplicate = new 표현연결Plan("fixture:r1", plan.Requirements.Append(plan.Requirements[0]));
        Assert.Equal(표현연결Readiness.Blocked, 표현연결Preflight.Review(duplicate, Observe(plan)).Readiness);
        Assert.Equal(표현연결Readiness.Blocked, 표현연결Preflight.Review(plan,
            new(plan.ContextFingerprint, rows.Append(new(표현연결항목.Component, "unknown", 표현연결ObservationStatus.Missing)))).Readiness);
    }

    [Fact]
    public void Farm_실제연결미확보와_기존보고결손을모사한입력을_구분한다()
    {
        var missing = Farm수확표현연결Preflight.Review(null, new("D388-file-survey-only", Array.Empty<표현연결Requirement>()), null);
        Assert.Equal(표현연결Readiness.Conditional, missing.Readiness);
        Assert.Contains(missing.Checks, x => x.Code == "FarmSnapshotMissing_E5Unlinked");
        Assert.Contains(missing.Checks, x => x.Code == "FarmLogicE5EvidenceMissing");
        // 아래는 r99 결손과 현재 Logic E3를 모사한 독립 Fixture. 실제 r99 Scene/Session을 복원하지 않는다.
        var preparation = FarmPreparation(out var state);
        var plan = FarmPlan(state);
        var observations = Observe(plan).Observations.Select(x => x.Item == 표현연결항목.Component
            ? new 표현연결Observation(x.Item, x.Key, 표현연결ObservationStatus.Missing, "null-slot-fixture", "fixture:r99-reported-gap", FixtureSha)
            : x.Item == 표현연결항목.LogicE5 ? new(x.Item, x.Key, 표현연결ObservationStatus.Confirmed, "E3", "fixture:logic-gap", FixtureSha) : x).ToArray();
        var blocked = Farm수확표현연결Preflight.Review(state, plan, new(plan.ContextFingerprint, observations));
        Assert.Equal(표현연결Readiness.Blocked, blocked.Readiness);
        Assert.Contains(blocked.Checks, x => x.Item == 표현연결항목.Component && x.Code == "ObservedMissing");
        Assert.Contains(blocked.Checks, x => x.Item == 표현연결항목.LogicE5 && x.Code == "ObservedMismatch");
        Assert.Same(state, preparation.Current);
        output.WriteLine(JsonSerializer.Serialize(new { evidence="PureFixture_NotLiveEditor", missing, blocked }));
    }

    [Fact]
    public void Farm_기존준비소비자는_원본과직전표현을보존하고_다른Session을거부한다()
    {
        var preparation = FarmPreparation(out var state); var plan = FarmPlan(state);
        var stateBefore = JsonSerializer.Serialize(state);
        Assert.Equal(표현연결Readiness.Ready, Farm수확표현연결Preflight.Review(state, plan, Observe(plan)).Readiness);
        var mismatch = Replace(plan, 표현연결항목.Session, "session:other");
        var blocked = Farm수확표현연결Preflight.Review(state, mismatch, Observe(mismatch));
        Assert.Contains(blocked.Checks, x => x.Code == "FarmPreparedStateMismatch");
        Assert.Same(state, preparation.Current);
        Assert.Equal(stateBefore, JsonSerializer.Serialize(state));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Farm_상태가있어도_미준비기대값을_확인된불일치로바꾸지않는다(int missingKind)
    {
        var consumer = FarmPreparation(out var state);
        var complete = FarmPlan(state);
        표현연결Plan? plan = missingKind == 0 ? null : missingKind == 1
            ? new(complete.PreparationRevision, complete.Requirements.Where(x => x.Item != 표현연결항목.Target))
            : Replace(complete, 표현연결항목.Target, "");
        var result = Farm수확표현연결Preflight.Review(state, plan, plan == null ? null : Observe(plan));
        Assert.Equal(표현연결Readiness.Conditional, result.Readiness);
        Assert.DoesNotContain(result.Checks, x => x.Code == "FarmPreparedStateMismatch");
        Assert.Same(state, consumer.Current);
    }

    [Fact]
    public void 방문자_기존준비계약도_농장규칙없이_두번째시험입력으로소비한다()
    {
        var source = new Simulation공동체방문자응대CardSnapshot { CardStableId="card:visitor", VisitorStableId="visitor:one",
            SourceWorldRevision=7, StatusCode=Simulation공동체방문자체류Codes.결정대기, RemainingGuestCapacity=1 };
        var binding = new 방문자체류VisualBinding { StatusCode=source.StatusCode, VisualKey=방문자체류PresentationCodes.WaitingVisualKey,
            PrimaryAssetCandidateRef="Assets/Synty/PolygonStarter/Prefabs/Characters/SM_Chr_Male_01.prefab",
            AlternativeAssetCandidateRef="fixture:alternative", FallbackVisualKey="fixture:fallback", CandidateRevisionOrFingerprint="fixture:visitor.r1",
            AnimationRoleCode="VisitorArrival", ActionCueCode="Visitor.Waiting.Greet", PrimaryAnimationClipRef="fixture:clip", FallbackActionCueCode="fixture:idle" };
        var prepared = new 방문자체류PresentationPreparationProjector().Project("world:fixture", new[] {source}, new[] {binding});
        var visitor = Assert.Single(prepared.Visitors); var before = JsonSerializer.Serialize(prepared);
        var plan = Replace(Plan(), 표현연결항목.Target, visitor.VisitorStableId);
        plan = Replace(plan, 표현연결항목.VisualKey, visitor.VisualKey);
        plan = Replace(plan, 표현연결항목.CandidatePath, visitor.PrimaryAssetCandidateRef);
        plan = Replace(plan, 표현연결항목.CandidateFingerprint, visitor.CandidateRevisionOrFingerprint);
        plan = Replace(plan, 표현연결항목.StateRevision, prepared.SourceRevision.ToString(CultureInfo.InvariantCulture));
        plan = Replace(plan, 표현연결항목.PresentationRevision, prepared.PlanHashSha256);
        plan = Replace(plan, 표현연결항목.Anchor, visitor.RequiredHCapability);
        Assert.Equal(표현연결Readiness.Ready, 표현연결Preflight.Review(plan, Observe(plan)).Readiness);
        Assert.Equal(before, JsonSerializer.Serialize(prepared));
        Assert.False(visitor.CanConfirmAuthority);
    }

    private static 표현연결Plan Plan()
    {
        return new("fixture:r1", Enum.GetValues<표현연결항목>().Select(item => new 표현연결Requirement(item,
            item == 표현연결항목.Component ? "농장경영선택대상View" : "main",
            item switch { 표현연결항목.LogicE5 => "E5", 표현연결항목.Target => "crop:fixture",
                표현연결항목.Session => "session:fixture", 표현연결항목.StateRevision => "7",
                표현연결항목.CandidatePath => "Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Box_Potato_01.prefab",
                표현연결항목.CandidateFingerprint => "A128993CF0644A5988A537A0196DC69DCD36619538FC499E01ED7F5A3377C583",
                _ => "fixture:" + item })));
    }
    private static 표현연결Plan Replace(표현연결Plan plan, 표현연결항목 item, string value, bool required=true, string reason="")
        => new(plan.PreparationRevision, plan.Requirements.Select(x => x.Item == item ? new(item, x.Key, value, required, reason) : x));
    private static 표현연결관측Snapshot Observe(표현연결Plan plan, 표현연결항목? change=null, string? value=null,
        표현연결ObservationStatus status=표현연결ObservationStatus.Confirmed, bool? validity=true)
        => new(plan.ContextFingerprint, plan.Requirements.Where(x => x.Required).Select(x => new 표현연결Observation(x.Item, x.Key,
            x.Item == change ? status : 표현연결ObservationStatus.Confirmed, x.Item == change ? value ?? x.ExpectedValue : x.ExpectedValue,
            "fixture:observations", FixtureSha, x.Item == change ? validity : true)));
    private static Farm수확상태PresentationPreparation FarmPreparation(out Farm수확상태PresentationState state)
    {
        var source = new SimulationFarmSurvivalStateSnapshot { SessionStableId="session:fixture", RuleRevision="rule.r1", WorldRevision=7,
            SoilTiles=new[] {new SimulationFarmSoilTileSnapshot {SoilTileStableId="soil:one", StateCode="Tilled"}},
            CultivationUnits=new[] {new Simulation재배단위Snapshot {CultivationUnitStableId="crop:fixture", TileStableId="soil:one",
                Revision=1, ProductStableId="product:potato", StateCode="HarvestReady"}}, HarvestLots=Array.Empty<Simulation수확LotSnapshot>() };
        var consumer = new Farm수확상태PresentationPreparation("session:fixture", "rule.r1", "soil:one", "crop:fixture");
        Assert.True(consumer.TryPrepare(source, out var prepared, out _)); state=prepared!; return consumer;
    }
    private static 표현연결Plan FarmPlan(Farm수확상태PresentationState state)
    {
        var plan = Plan(); plan = Replace(plan, 표현연결항목.Target, state.CultivationUnitStableId);
        // 이 시험의 HarvestReady 후보는 D388의 큰 식물이다. 실제 VisualKey 선택/카탈로그 연결 승인은 아니다.
        plan = Replace(plan, 표현연결항목.CandidatePath, "Assets/Synty/PolygonFarm/Prefabs/Plants/SM_Prop_Plant_Potato_01_L.prefab");
        plan = Replace(plan, 표현연결항목.CandidateFingerprint, "FC01F89A96545D8FBA023FCAE7BE54F4EAE5330306A46519D52D6F3C945FF627");
        plan = Replace(plan, 표현연결항목.Session, state.SessionStableId);
        plan = Replace(plan, 표현연결항목.StateRevision, state.SourceWorldRevision.ToString(CultureInfo.InvariantCulture));
        plan = Replace(plan, 표현연결항목.PresentationRevision, state.PresentationRevision);
        plan = Replace(plan, 표현연결항목.PresentationSlot, state.PresentationSlot);
        return Replace(plan, 표현연결항목.StateCode, state.StateCode);
    }
}
