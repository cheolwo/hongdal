using Ssalddel.Simulation.Contracts;
using Ssalddel.Unity.Cards;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "방문자 체류 카드의 상태별 H 기준점·VisualKey·자산 후보·fallback과 읽기 전용 경계를 검증한다.",
    WorkOrderIds = new[] { "E7-WO-NATURE-CAMP-VISITOR-STAY" },
    WorldInteractionIds = new[] { "WI-COMMUNITY-VISITOR-STAY" },
    Boundary = "자동 시험은 실제 Synty Prefab·Scene·위치·Renderer·Collider·입력·Game View를 대신하지 않는다.")]
public sealed class Simulation방문자체류PresentationPreparationTests
{
    [Fact]
    public void 결정대기_방문자는_입구대기_기준점과_Preview에_결속된다()
    {
        var preparation = Project(Card("visitor:b",
            Simulation공동체방문자체류Codes.결정대기),
            Binding(Simulation공동체방문자체류Codes.결정대기,
                방문자체류PresentationCodes.WaitingVisualKey));

        var visitor = Assert.Single(preparation.Visitors);
        Assert.Equal(방문자체류PresentationCodes.VisitorWaitingAnchor,
            visitor.RequiredHCapability);
        Assert.Equal(방문자체류PresentationCodes.WaitingVisualKey,
            visitor.VisualKey);
        Assert.True(visitor.CanRequestPreview);
        Assert.False(visitor.CanConfirmAuthority);
        Assert.Equal("VisitorArrival", visitor.AnimationRoleCode);
        Assert.Equal("Visitor.Waiting.Greet", visitor.ActionCueCode);
        Assert.False(visitor.UsesRootMotion);
        Assert.True(visitor.PresentationOnly);
        Assert.True(preparation.PresentationOnly);
        Assert.False(preparation.MutatesCanonicalState);
        Assert.Equal(64, preparation.PlanHashSha256.Length);
    }

    [Theory]
    [InlineData(Simulation공동체방문자체류Codes.임시체류,
        방문자체류PresentationCodes.GuestRestAnchor)]
    [InlineData(Simulation공동체방문자체류Codes.거절,
        방문자체류PresentationCodes.VisitorDepartureAnchor)]
    public void 결정완료_상태는_각각_휴식과_이탈_기준점으로_결속된다(
        string statusCode, string expectedCapability)
    {
        var preparation = Project(Card("visitor:a", statusCode),
            Binding(statusCode, "Community.Visitor.State"));

        var visitor = Assert.Single(preparation.Visitors);
        Assert.Equal(expectedCapability, visitor.RequiredHCapability);
        Assert.False(visitor.CanRequestPreview);
        Assert.False(visitor.CanConfirmAuthority);
    }

    [Fact]
    public void 승인Binding이_없으면_명시적인_PrimitiveFallback을_사용한다()
    {
        var preparation = Project(Card("visitor:a",
            Simulation공동체방문자체류Codes.결정대기));

        var visitor = Assert.Single(preparation.Visitors);
        Assert.Equal(방문자체류PresentationCodes.FallbackVisualKey,
            visitor.VisualKey);
        Assert.Empty(visitor.PrimaryAssetCandidateRef);
        Assert.Empty(visitor.AlternativeAssetCandidateRef);
        Assert.StartsWith("fallback:",
            visitor.CandidateRevisionOrFingerprint);
    }

    [Fact]
    public void 입력순서가_달라도_방문자순서와_Hash가_같다()
    {
        var cards = new[]
        {
            Card("visitor:z", Simulation공동체방문자체류Codes.거절),
            Card("visitor:a", Simulation공동체방문자체류Codes.결정대기),
        };
        var bindings = new[]
        {
            Binding(Simulation공동체방문자체류Codes.거절,
                방문자체류PresentationCodes.RejectedVisualKey),
            Binding(Simulation공동체방문자체류Codes.결정대기,
                방문자체류PresentationCodes.WaitingVisualKey),
        };
        var projector = new 방문자체류PresentationPreparationProjector();

        var first = projector.Project("world:nature:test", cards, bindings);
        var second = projector.Project("world:nature:test",
            cards.Reverse(), bindings.Reverse());

        Assert.Equal(new[] { "visitor:a", "visitor:z" },
            first.Visitors.Select(value => value.VisitorStableId));
        Assert.Equal(first.PlanHashSha256, second.PlanHashSha256);
    }

    [Fact]
    public void 서로_다른_revision의_카드는_한_표현계획으로_섞지_않는다()
    {
        var first = Card("visitor:a",
            Simulation공동체방문자체류Codes.결정대기);
        var second = Card("visitor:b",
            Simulation공동체방문자체류Codes.결정대기);
        second.SourceWorldRevision++;

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 방문자체류PresentationPreparationProjector().Project(
                "world:nature:test", new[] { first, second },
                Array.Empty<방문자체류VisualBinding>()));

        Assert.Equal("CommunityVisitorPresentationRevisionMixed",
            error.Message);
    }

    [Fact]
    public void 중복_상태Binding은_거부한다()
    {
        var binding = Binding(
            Simulation공동체방문자체류Codes.결정대기,
            방문자체류PresentationCodes.WaitingVisualKey);

        var error = Assert.Throws<InvalidOperationException>(() =>
            Project(Card("visitor:a",
                Simulation공동체방문자체류Codes.결정대기),
                binding, binding));

        Assert.Equal("CommunityVisitorVisualBindingInvalid", error.Message);
    }

    private static 방문자체류PresentationPreparation Project(
        Simulation공동체방문자응대CardSnapshot card,
        params 방문자체류VisualBinding[] bindings)
        => new 방문자체류PresentationPreparationProjector().Project(
            "world:nature:test", new[] { card }, bindings);

    private static Simulation공동체방문자응대CardSnapshot Card(
        string visitorStableId, string statusCode)
        => new()
        {
            CardStableId = "card:community-visitor:" + visitorStableId,
            SourceWorldRevision = 7,
            VisitorStableId = visitorStableId,
            StatusCode = statusCode,
            MindTraceCode = statusCode ==
                Simulation공동체방문자체류Codes.임시체류
                ? Simulation공동체방문자체류Codes.환대확인
                : statusCode == Simulation공동체방문자체류Codes.거절
                    ? Simulation공동체방문자체류Codes.경계보호
                    : string.Empty,
            RemainingGuestCapacity = 1,
        };

    private static 방문자체류VisualBinding Binding(string statusCode,
        string visualKey)
        => new()
        {
            StatusCode = statusCode,
            VisualKey = visualKey,
            PrimaryAssetCandidateRef =
                "Assets/Synty/PolygonStarter/Prefabs/Characters/SM_Chr_Male_01.prefab",
            AlternativeAssetCandidateRef =
                "Assets/Synty/PolygonStarter/Prefabs/Characters/SM_Chr_Female_01.prefab",
            FallbackVisualKey =
                방문자체류PresentationCodes.FallbackVisualKey,
            CandidateRevisionOrFingerprint = "fixture:visitor-candidate.r1",
            AnimationRoleCode = "VisitorArrival",
            ActionCueCode = "Visitor.Waiting.Greet",
            PrimaryAnimationClipRef =
                "Assets/Synty/AnimationEmotesAndTaunts/Animations/Polygon/Masculine/Greet/A_POLY_EMOT_Greet_Wave_Masc.fbx",
            FallbackActionCueCode = "Visitor.State.Static",
        };
}
