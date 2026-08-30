using System.Globalization;
using System.Text.Json;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3,
    "경관 소비 사전검사의 누락·중복·계보·정규형 봉인·원본 보존과 결정성을 검증한다.",
    SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3결정성검증,
    Boundary = "합성 공통 계획 시험이다. 실제 LS01 B 변환·실측 지형·Unity 조립·Play Mode·Game View 증거가 아니다.")]
public sealed class Simulation경관배치소비검증Tests
{
    [Fact]
    public void 결속만통과하고_H1없는환경장식을_실제조립기대목록으로반환한다()
    {
        var request = Request();
        var result = Verify(request);
        Assert.Equal("BindingVerified", result.StatusCode);
        Assert.Equal(new[] { "fixture:decoration:rock", "fixture:decoration:tree" }, result.ExpectedPlacements.Select(value => value.PlacementStableId));
        Assert.Equal(request.SourcePlan.AssetPlacementPlanHashSha256, result.SourcePlanHashSha256);
        Assert.Equal(request.BaselinePlan.AssetPlacementPlanHashSha256, result.BaselinePlanHashSha256);
        Assert.Equal(request.OwnerCellStableId, result.OwnerCellStableId);
        Assert.Equal(request.H2StableId, result.H2StableId);
        Assert.Equal(request.AreaSetStableId, result.AreaSetStableId);
        Assert.All(request.SourcePlan.Placements.Where(value => value.PlacementStableId.StartsWith("fixture:decoration:")), value => Assert.Empty(value.H1StableId));
    }

    [Fact]
    public void 입력전체와_기준A객체를_변경하지않는다()
    {
        var request = Request();
        var before = JsonSerializer.Serialize(request);
        Verify(request);
        Verify(request);
        Assert.Equal(before, JsonSerializer.Serialize(request));
        Assert.Equal(3, request.BaselinePlan.Placements.Length);
        Assert.Equal(5, request.SourcePlan.Placements.Length);
    }

    [Fact]
    public void 결과기대목록은_요청배열이나_원본계획변경과분리된다()
    {
        var request = Request();
        var result = Verify(request);
        request.ExpectedDecorations[0] = new("replaced", "replaced");
        request.SourcePlan.Placements[3].CompositionKey = "replaced";
        Assert.Equal("fixture:decoration:tree", result.ExpectedPlacements[1].PlacementStableId);
        Assert.Equal("fixture:composition:tree", result.ExpectedPlacements[1].CompositionKey);
        var collection = Assert.IsAssignableFrom<IList<Simulation경관추가배치Binding>>(result.ExpectedPlacements);
        Assert.Throws<NotSupportedException>(() => collection.Add(new("new", "new")));
    }

    [Fact]
    public void 배열순서와문화권을바꿔도_같은정규형과결과다()
    {
        var request = Request();
        var expected = JsonSerializer.Serialize(Verify(request));
        Array.Reverse(request.SourcePlan.Placements);
        Array.Reverse(request.BaselinePlan.Placements);
        Array.Reverse(request.MapPlan.HBindings);
        Array.Reverse(request.ResolvedCompositionKeys);
        Array.Reverse(request.ExpectedDecorations);
        var culture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.Equal(expected, JsonSerializer.Serialize(Verify(request)));
        }
        finally { CultureInfo.CurrentCulture = culture; }
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void 기대hash불일치는_거부한다(bool source)
    {
        var request = Request();
        if (source) request.ExpectedSourcePlanHashSha256 = Hash("wrong");
        else request.ExpectedBaselinePlanHashSha256 = Hash("wrong");
        Reject(request, source ? "SourcePlanHashMismatch" : "BaselinePlanHashMismatch");
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void 저장된hash가같아도_원본내용손상은_거부한다(bool source)
    {
        var request = Request();
        (source ? request.SourcePlan : request.BaselinePlan).Placements[0].LocalXMeters += 1;
        Reject(request, source ? "SourcePlanHashMismatch" : "BaselinePlanHashMismatch");
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void 계획내중복ID는_거부한다(bool source)
    {
        var request = Request();
        var plan = source ? request.SourcePlan : request.BaselinePlan;
        plan.Placements = plan.Placements.Append(Clone(plan.Placements[0])).ToArray();
        Reject(request, source ? "SourcePlacementDuplicate" : "BaselinePlacementDuplicate");
    }

    [Fact]
    public void 기대목록의중복ID는_거부한다()
    {
        var request = Request();
        request.ExpectedDecorations = request.ExpectedDecorations.Append(request.ExpectedDecorations[0]).ToArray();
        Reject(request, "ExpectedDecorationDuplicate");
    }

    [Fact]
    public void Ambient가하나누락되어도_전체성공으로오인하지않는다()
    {
        var request = Request();
        request.SourcePlan.Placements = request.SourcePlan.Placements.Where(value => value.PlacementStableId != "fixture:decoration:tree").ToArray();
        SealSource(request);
        Reject(request, "ExpectedDecorationMissing");
    }

    [Fact]
    public void 원A환경객체를_추가장식으로오인하면거부한다()
    {
        var request = Request();
        var original = request.BaselinePlan.Placements[2];
        request.ExpectedDecorations = request.ExpectedDecorations.Append(new(original.PlacementStableId, original.CompositionKey)).ToArray();
        Reject(request, "DecorationAlreadyInBaseline");
    }

    [Fact]
    public void 기대하지않은추가장식이있으면_거부한다()
    {
        var request = Request();
        request.SourcePlan.Placements = request.SourcePlan.Placements.Append(Decoration("extra", "tree")).ToArray();
        SealSource(request);
        Reject(request, "UnexpectedDecoration");
    }

    [Theory]
    [InlineData("placement")] [InlineData("removed")] [InlineData("metadata")]
    [InlineData("handle")] [InlineData("body")]
    public void Source를다시봉인해도_A객체와메타데이터와실내목록변경은거부한다(string mutation)
    {
        var request = Request();
        if (mutation == "placement") request.SourcePlan.Placements[0].LocalXMeters += 1;
        if (mutation == "removed") request.SourcePlan.Placements = request.SourcePlan.Placements.Skip(1).ToArray();
        if (mutation == "metadata") request.SourcePlan.RuleRevision = "different";
        if (mutation == "handle") request.SourcePlan.InteriorPlanHandles = new[] { new SimulationInteriorPlanHandleSnapshot { BuildingPlacementStableId = "extra" } };
        if (mutation == "body")
        {
            var body = new SimulationInteriorPlacementPlanBodySnapshot { BuildingPlacementStableId = "extra" };
            body.BodyHashSha256 = Simulation세계자산CanonicalHash.ComputeInteriorBodyHash(body);
            request.SourcePlan.InteriorPlanBodies = new[] { body };
        }
        SealSource(request);
        Reject(request, "BaselineChanged");
    }

    [Theory]
    [InlineData("request")] [InlineData("plan")] [InlineData("decoration")]
    public void 셀소유불일치는_거부한다(string mutation)
    {
        var request = Request();
        if (mutation == "request") request.OwnerCellStableId = "other";
        if (mutation == "plan") request.SourcePlan.CellStableId = "other";
        if (mutation == "decoration") request.SourcePlan.Placements[3].OwnerCellStableId = "other";
        SealSource(request);
        Reject(request, mutation == "decoration" ? "DecorationOwnerCellMismatch" : "OwnerCellMismatch");
    }

    [Theory]
    [InlineData("H2")] [InlineData("H4")] [InlineData("ambiguous")]
    public void H소유누락과중복계보는_거부한다(string mutation)
    {
        var request = Request();
        if (mutation == "H2") request.H2StableId = "unknown";
        if (mutation == "H4") request.AreaSetStableId = "unknown";
        if (mutation == "ambiguous")
        {
            request.MapPlan.HBindings = request.MapPlan.HBindings.Append(Clone(request.MapPlan.HBindings[0])).ToArray();
            SealMapAndPlans(request);
        }
        Reject(request, "HOwnershipMissingOrAmbiguous");
    }

    [Fact]
    public void 잘못된지도hash는_거부한다()
    {
        var request = Request();
        request.MapPlan.WorldSeed = "changed";
        Reject(request, "MapPlanHashMismatch");
    }

    [Theory]
    [InlineData(true)] [InlineData(false)]
    public void 배치계획의지도hash와revision계보불일치는_거부한다(bool hash)
    {
        var request = Request();
        if (hash) request.SourcePlan.MapPlanHashSha256 = Hash("other-map");
        else request.SourcePlan.SourceWorldRevision += 1;
        SealSource(request);
        Reject(request, "MapLineageMismatch");
    }

    [Theory]
    [InlineData("kind")] [InlineData("authority")] [InlineData("presentation")]
    public void 장식이환경과Ambient표현경계를벗어나면_거부한다(string mutation)
    {
        var request = Request();
        var placement = request.SourcePlan.Placements[3];
        if (mutation == "kind") placement.PlacementKindCode = Simulation세계자산배치Codes.MapAnchor;
        if (mutation == "authority") placement.AuthorityKindCode = Simulation세계자산배치Codes.SimulationEntity;
        if (mutation == "presentation") placement.PresentationOnly = false;
        SealSource(request);
        Reject(request, "DecorationPresentationBoundaryInvalid");
    }

    [Fact]
    public void 등록된키라도_기대단일구성키와다르면거부한다()
    {
        var request = Request();
        request.SourcePlan.Placements[3].CompositionKey = "fixture:composition:rock";
        SealSource(request);
        Reject(request, "DecorationCompositionMismatch");
    }

    [Theory]
    [InlineData("NatureResourceNode")] [InlineData("NatureCabin")]
    [InlineData("NatureWorkbench")] [InlineData("NatureDroppedTimber")]
    public void Ambient라도_Unity행위표식분류를_장식으로위장하면거부한다(string category)
    {
        var request = Request();
        request.SourcePlan.Placements[3].CategoryCode = category;
        request.SourcePlan.Placements[3].StateCode = Simulation세계자산배치Codes.Standing;
        SealSource(request);
        Reject(request, "DecorationInteractionCategoryForbidden");
    }

    [Theory]
    [InlineData("change")] [InlineData("spawn")] [InlineData("parent")]
    public void 추가장식의권위계보와부모위장은_거부한다(string mutation)
    {
        var request = Request();
        var placement = request.SourcePlan.Placements[3];
        if (mutation == "change") placement.SourceChangeStableIds = new[] { "invented-change" };
        if (mutation == "spawn") placement.SourceSpawnDecisionStableId = "invented-decision";
        if (mutation == "parent") placement.ParentPlacementStableId = "fixture:barn";
        SealSource(request);
        Reject(request, mutation == "parent" ? "DecorationParentForbidden" : "DecorationAuthorityLineageForbidden");
    }

    [Fact]
    public void Nature이름만으로거부하거나_충돌가능플래그를접지증거로오인하지않는다()
    {
        var request = Request();
        request.SourcePlan.Placements[3].CategoryCode = "NatureScenery";
        request.SourcePlan.Placements[3].CollisionEligible = true;
        request.SourcePlan.Placements[3].FixedAnchor = true;
        SealSource(request);
        Assert.Equal("BindingVerified", Verify(request).StatusCode);
    }

    [Fact]
    public void 이름이비슷해도_실제해결기등록키가아니면거부한다()
    {
        var request = Request();
        request.ResolvedCompositionKeys = new[] { "FIXTURE:composition:tree", "fixture:composition:rock" };
        Reject(request, "DecorationCompositionUnresolved");
    }

    [Theory]
    [InlineData(0)] [InlineData(-1)] [InlineData(.999999)] [InlineData(2)]
    public void nativeScale1이아니면_거부한다(double scale)
    {
        var request = Request();
        request.SourcePlan.Placements[3].UniformScale = scale;
        SealSource(request);
        Reject(request, "DecorationNativeScaleRequired");
    }

    [Theory]
    [InlineData("x", double.NaN)] [InlineData("y", double.PositiveInfinity)]
    [InlineData("z", double.NegativeInfinity)] [InlineData("rotation", double.NaN)] [InlineData("scale", double.PositiveInfinity)]
    public void 유한하지않은TRS는_거부한다(string field, double value)
    {
        var request = Request();
        var placement = request.SourcePlan.Placements[3];
        if (field == "x") placement.LocalXMeters = value;
        if (field == "y") placement.LocalYMeters = value;
        if (field == "z") placement.LocalZMeters = value;
        if (field == "rotation") placement.RotationDegrees = value;
        if (field == "scale") placement.UniformScale = value;
        SealSource(request);
        Reject(request, "DecorationTransformNotFinite");
    }

    [Fact]
    public void 존재하는H1은허용하지만_가짜H1은거부한다()
    {
        var request = Request();
        request.SourcePlan.Placements[3].H1StableId = "fixture:h1";
        SealSource(request);
        Assert.Equal("BindingVerified", Verify(request).StatusCode);
        request.SourcePlan.Placements[3].H1StableId = "fabricated";
        SealSource(request);
        Reject(request, "DecorationH1Unknown");
    }

    [Fact]
    public void 검증실패도_입력을변경하지않는다()
    {
        var request = Request();
        request.SourcePlan.Placements[3].UniformScale = 2;
        SealSource(request);
        var before = JsonSerializer.Serialize(request);
        Reject(request, "DecorationNativeScaleRequired");
        Assert.Equal(before, JsonSerializer.Serialize(request));
    }

    [Fact]
    public void 실내본문을바꾸고_상위hash만유지해도거부한다()
    {
        var request = Request();
        var body = new SimulationInteriorPlacementPlanBodySnapshot { BuildingPlacementStableId = "fixture:barn" };
        body.BodyHashSha256 = Simulation세계자산CanonicalHash.ComputeInteriorBodyHash(body);
        request.BaselinePlan.InteriorPlanBodies = new[] { Clone(body) };
        request.SourcePlan.InteriorPlanBodies = new[] { Clone(body) };
        SealMapAndPlans(request);
        Assert.Equal("BindingVerified", Verify(request).StatusCode);
        request.SourcePlan.InteriorPlanBodies[0].BuildingPlacementStableId = "changed";
        Reject(request, "SourceInteriorBodyHashMismatch");
    }

    [Theory]
    [InlineData("expected")] [InlineData("resolver")] [InlineData("placement")] [InlineData("map")]
    public void 누락입력은_명시적으로거부한다(string missing)
    {
        var request = Request();
        if (missing == "expected") request.ExpectedDecorations = Array.Empty<Simulation경관추가배치Binding>();
        if (missing == "resolver") request.ResolvedCompositionKeys = Array.Empty<string>();
        if (missing == "placement") request.SourcePlan.Placements = new Simulation세계자산PlacementSnapshot[] { null! };
        if (missing == "map") request.MapPlan.HBindings = null!;
        Assert.Throws<ArgumentException>(() => Verify(request));
    }

    private static Simulation경관배치소비검증Result Verify(Simulation경관배치소비검증Request request)
        => new Simulation경관배치소비검증Service().Verify(request);
    private static void Reject(Simulation경관배치소비검증Request request, string code)
        => Assert.Equal("SimulationLandscapeBinding:" + code, Assert.Throws<ArgumentException>(() => Verify(request)).Message);
    private static string Hash(string value) => Simulation세계자산CanonicalHash.Hash(value);
    private static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
    private static void SealSource(Simulation경관배치소비검증Request request)
        => request.ExpectedSourcePlanHashSha256 = request.SourcePlan.AssetPlacementPlanHashSha256 = Simulation세계자산CanonicalHash.ComputeAssetPlacementPlanHash(request.SourcePlan);
    private static void SealMapAndPlans(Simulation경관배치소비검증Request request)
    {
        request.MapPlan.MapPlanHashSha256 = Simulation세계자산CanonicalHash.ComputeMapPlanHash(request.MapPlan);
        request.SourcePlan.MapPlanHashSha256 = request.BaselinePlan.MapPlanHashSha256 = request.MapPlan.MapPlanHashSha256;
        request.ExpectedBaselinePlanHashSha256 = request.BaselinePlan.AssetPlacementPlanHashSha256 = Simulation세계자산CanonicalHash.ComputeAssetPlacementPlanHash(request.BaselinePlan);
        SealSource(request);
    }

    private static Simulation세계자산PlacementSnapshot Decoration(string id, string key) => new()
    {
        PlacementStableId = "fixture:decoration:" + id, OwnerCellStableId = "fixture:cell",
        CompositionKey = "fixture:composition:" + key, PlacementKindCode = Simulation세계자산배치Codes.Environment,
        AuthorityKindCode = Simulation세계자산배치Codes.AmbientPresentation, PresentationOnly = true,
        LocalXMeters = 3.25, LocalYMeters = .125, LocalZMeters = -4.5, RotationDegrees = 90, UniformScale = 1
    };

    // 실제 LS01 B.delta.json을 변환하지 않는 독립 합성 표본이다.
    private static Simulation경관배치소비검증Request Request()
    {
        var request = new Simulation경관배치소비검증Request
        {
            OwnerCellStableId = "fixture:cell", H2StableId = "fixture:h2", AreaSetStableId = "fixture:h4",
            MapPlan = new Simulation지도구성Plan
            {
                CellStableId = "fixture:cell", SourceWorldRevision = 37, WorldSeed = "synthetic-only",
                HBindings = new[]
                {
                    new Simulation지도H결속Snapshot { HLevelCode = "H2", SpatialStableId = "fixture:h2" },
                    new Simulation지도H결속Snapshot { HLevelCode = "H4", SpatialStableId = "fixture:h4" },
                    new Simulation지도H결속Snapshot { HLevelCode = "H1", SpatialStableId = "fixture:h1" }
                }
            },
            BaselinePlan = new Simulation세계자산배치Plan
            {
                CellStableId = "fixture:cell", SourceWorldRevision = 37,
                Placements = new[]
                {
                    new Simulation세계자산PlacementSnapshot { PlacementStableId = "fixture:barn", PlacementKindCode = Simulation세계자산배치Codes.Building, UniformScale = 2 },
                    new Simulation세계자산PlacementSnapshot { PlacementStableId = "fixture:plot", PlacementKindCode = Simulation세계자산배치Codes.MapAnchor, PresentationOnly = false },
                    Decoration("existing-ambient", "grass")
                }
            },
            ExpectedDecorations = new[]
            {
                new Simulation경관추가배치Binding("fixture:decoration:tree", "fixture:composition:tree"),
                new Simulation경관추가배치Binding("fixture:decoration:rock", "fixture:composition:rock")
            },
            ResolvedCompositionKeys = new[] { "fixture:composition:tree", "fixture:composition:rock" }
        };
        request.SourcePlan = Clone(request.BaselinePlan);
        request.SourcePlan.Placements = request.SourcePlan.Placements.Concat(new[] { Decoration("tree", "tree"), Decoration("rock", "rock") }).ToArray();
        SealMapAndPlans(request);
        return request;
    }
}
