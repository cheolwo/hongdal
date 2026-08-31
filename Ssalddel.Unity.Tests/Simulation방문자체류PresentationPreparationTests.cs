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
    [Theory]
    [InlineData(Simulation공동체방문자체류Codes.결정대기, 방문자체류PresentationCodes.WaitingVisualKey,
        "Visitor.Waiting.Greet", 방문자체류PresentationCodes.VisitorWaitingAnchor)]
    [InlineData(Simulation공동체방문자체류Codes.임시체류, 방문자체류PresentationCodes.AcceptedVisualKey,
        "Visitor.State.IdleOrDepart", 방문자체류PresentationCodes.GuestRestAnchor)]
    [InlineData(Simulation공동체방문자체류Codes.거절, 방문자체류PresentationCodes.RejectedVisualKey,
        "Visitor.State.IdleOrDepart", 방문자체류PresentationCodes.VisitorDepartureAnchor)]
    public void 상태검사진입점은_세상태의_기존key_role_cue_기준점을_결속한다(
        string status, string key, string cue, string anchor)
    {
        var card = Card("visitor:state", status);
        var binding = StateBinding(status);
        var result = new 방문자체류PresentationPreparationProjector().ProjectWithStateBindingValidation(
            "world:fixture", new[] { card }, new[] { binding });
        var visitor = Assert.Single(result.Visitors);
        Assert.Equal(key, visitor.VisualKey);
        Assert.Equal(cue, visitor.ActionCueCode);
        Assert.Equal("VisitorArrival", visitor.AnimationRoleCode);
        Assert.Equal(anchor, visitor.RequiredHCapability);
        Assert.Equal(status == Simulation공동체방문자체류Codes.결정대기, visitor.CanRequestPreview);
        Assert.False(visitor.UsesRootMotion);
        Assert.False(visitor.CanConfirmAuthority);
        Assert.True(visitor.PresentationOnly);
        Assert.False(result.MutatesCanonicalState);
    }

    [Theory]
    [InlineData("key", "CommunityVisitorBindingVisualKeyMismatch")]
    [InlineData("role", "CommunityVisitorBindingAnimationRoleMismatch")]
    [InlineData("cue", "CommunityVisitorBindingActionCueMismatch")]
    [InlineData("unknown-state", "CommunityVisitorBindingStateUnsupported")]
    [InlineData("case", "CommunityVisitorBindingVisualKeyMismatch")]
    public void 제공Binding의_확인된불일치는_fallback으로_숨기지않는다(string fault, string expected)
    {
        var card = Card("visitor:waiting", Simulation공동체방문자체류Codes.결정대기);
        var binding = StateBinding(card.StatusCode);
        switch (fault)
        {
            case "key": binding.VisualKey = 방문자체류PresentationCodes.RejectedVisualKey; break;
            case "role": binding.AnimationRoleCode = "DifferentRole"; break;
            case "cue": binding.ActionCueCode = "Visitor.State.IdleOrDepart"; break;
            case "unknown-state": binding.StatusCode = "UnknownState"; break;
            case "case": binding.VisualKey = binding.VisualKey.ToLowerInvariant(); break;
        }
        var before = System.Text.Json.JsonSerializer.Serialize(new { card, binding });
        var error = Assert.Throws<InvalidOperationException>(() =>
            new 방문자체류PresentationPreparationProjector().ProjectWithStateBindingValidation(
                "world:fixture", new[] { card }, new[] { binding }));
        Assert.Equal(expected, error.Message);
        Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(new { card, binding }));
    }

    [Theory]
    [InlineData(Simulation공동체방문자체류Codes.임시체류)]
    [InlineData(Simulation공동체방문자체류Codes.거절)]
    public void 완료상태의_대기Greet는_새검사에서만_거부되고_기존Project는_호환된다(string status)
    {
        var card = Card("visitor:completed", status);
        var binding = StateBinding(status);
        binding.ActionCueCode = "Visitor.Waiting.Greet";
        var projector = new 방문자체류PresentationPreparationProjector();
        Assert.Equal("Visitor.Waiting.Greet", Assert.Single(projector.Project(
            "world:fixture", new[] { card }, new[] { binding }).Visitors).ActionCueCode);
        var error = Assert.Throws<InvalidOperationException>(() => projector.ProjectWithStateBindingValidation(
            "world:fixture", new[] { card }, new[] { binding }));
        Assert.Equal("CommunityVisitorBindingActionCueMismatch", error.Message);
    }

    [Fact]
    public void Binding미제공은_기존_명시적primitive_fallback을_유지한다()
    {
        var card = Card("visitor:missing", Simulation공동체방문자체류Codes.거절);
        var projector = new 방문자체류PresentationPreparationProjector();
        var previous = projector.Project("world:fixture", new[] { card }, Array.Empty<방문자체류VisualBinding>());
        var result = projector.ProjectWithStateBindingValidation("world:fixture", new[] { card },
            Array.Empty<방문자체류VisualBinding>());
        Assert.Equal(previous.PlanHashSha256, result.PlanHashSha256);
        var visitor = Assert.Single(result.Visitors);
        Assert.Equal(방문자체류PresentationCodes.FallbackVisualKey, visitor.VisualKey);
        Assert.Empty(visitor.PrimaryAssetCandidateRef);
        Assert.Equal("Visitor.State.Static", visitor.ActionCueCode);
    }

    [Theory]
    [InlineData("duplicate-binding", "CommunityVisitorVisualBindingInvalid")]
    [InlineData("mixed-revision", "CommunityVisitorPresentationRevisionMixed")]
    public void 새검사진입점도_기존중복과_혼합revision을_거부한다(string fault, string expected)
    {
        var first = Card("visitor:a", Simulation공동체방문자체류Codes.결정대기);
        var second = Card("visitor:b", Simulation공동체방문자체류Codes.결정대기);
        var binding = StateBinding(first.StatusCode);
        if (fault == "mixed-revision") second.SourceWorldRevision++;
        var bindings = fault == "duplicate-binding" ? new[] { binding, binding } : new[] { binding };
        var error = Assert.Throws<InvalidOperationException>(() =>
            new 방문자체류PresentationPreparationProjector().ProjectWithStateBindingValidation(
                "world:fixture", new[] { first, second }, bindings));
        Assert.Equal(expected, error.Message);
    }

    [Fact]
    public void 올바른입력은_기존결과와같고_입력순서와_입력을_보존한다()
    {
        var cards = new[] { Card("visitor:z", Simulation공동체방문자체류Codes.거절),
            Card("visitor:a", Simulation공동체방문자체류Codes.결정대기) };
        var bindings = new[] { StateBinding(cards[0].StatusCode), StateBinding(cards[1].StatusCode) };
        var before = System.Text.Json.JsonSerializer.Serialize(new { cards, bindings });
        var projector = new 방문자체류PresentationPreparationProjector();
        var original = projector.Project("world:fixture", cards, bindings);
        var first = projector.ProjectWithStateBindingValidation("world:fixture", cards, bindings);
        var second = projector.ProjectWithStateBindingValidation("world:fixture", cards.Reverse(), bindings.Reverse());
        Assert.Equal(System.Text.Json.JsonSerializer.Serialize(original), System.Text.Json.JsonSerializer.Serialize(first));
        Assert.Equal(first.PlanHashSha256, second.PlanHashSha256);
        Assert.Equal(new[] { "visitor:a", "visitor:z" }, first.Visitors.Select(x => x.VisitorStableId));
        Assert.Equal(before, System.Text.Json.JsonSerializer.Serialize(new { cards, bindings }));
    }

    private static 방문자체류VisualBinding StateBinding(string status)
    {
        var key = status == Simulation공동체방문자체류Codes.결정대기 ? 방문자체류PresentationCodes.WaitingVisualKey
            : status == Simulation공동체방문자체류Codes.임시체류 ? 방문자체류PresentationCodes.AcceptedVisualKey
            : 방문자체류PresentationCodes.RejectedVisualKey;
        var binding = Binding(status, key);
        binding.ActionCueCode = status == Simulation공동체방문자체류Codes.결정대기
            ? "Visitor.Waiting.Greet" : "Visitor.State.IdleOrDepart";
        return binding;
    }

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
