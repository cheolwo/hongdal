using Ssalddel.Unity.Cards;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "처방 기록의 상태·H 능력·VisualKey·fallback과 읽기 전용 경계를 검증한다.",
    WorkOrderIds = new[] { "E7-WO-NATURE-BASIC-HERBAL-RECOVERY" },
    Boundary = "자동 시험은 실제 Synty Prefab·Scene·Collider·입력·Game View를 대신하지 않는다.")]
public sealed class Simulation처방기록PresentationPreparationTests
{
    [Fact]
    public void 읽을수있는_열린책은_H능력과_PreviewAnchor로_결속된다()
    {
        var preparation = Project(Card(처방지식CardStateCodes.Readable,
            "source:nature:abandoned-camp-book"), Binding(
            "source:nature:abandoned-camp-book",
            처방기록PresentationCodes.OpenBookVisualKey));

        var source = Assert.Single(preparation.Sources);
        Assert.Equal(처방기록PresentationCodes.RequiredHCapability,
            source.RequiredHCapability);
        Assert.Equal(처방기록PresentationCodes.OpenBookVisualKey,
            source.VisualKey);
        Assert.Equal(처방기록PresentationCodes.InteractionAnchorCode,
            source.InteractionAnchorCode);
        Assert.True(source.CanOpenInformation);
        Assert.True(source.CanRequestPreview);
        Assert.False(source.CanConfirmAuthority);
        Assert.True(source.PresentationOnly);
        Assert.True(preparation.PresentationOnly);
        Assert.False(preparation.MutatesCanonicalState);
        Assert.Equal(64, preparation.PlanHashSha256.Length);
    }

    [Theory]
    [InlineData(처방지식CardStateCodes.Known, true, false)]
    [InlineData(처방지식CardStateCodes.Blocked, false, false)]
    public void 카드상태는_정보열기와_Preview가능성만_결정한다(
        string stateCode, bool canOpen, bool canPreview)
    {
        var preparation = Project(Card(stateCode, "source:a"),
            Binding("source:a",
                처방기록PresentationCodes.LoosePaperVisualKey));

        var source = Assert.Single(preparation.Sources);
        Assert.Equal(canOpen, source.CanOpenInformation);
        Assert.Equal(canPreview, source.CanRequestPreview);
        Assert.False(source.CanConfirmAuthority);
    }

    [Fact]
    public void 승인Binding이_없으면_명시적인_PrimitiveFallback을_사용한다()
    {
        var preparation = Project(Card(처방지식CardStateCodes.Readable,
            "source:unapproved"));

        var source = Assert.Single(preparation.Sources);
        Assert.Equal(처방기록PresentationCodes.FallbackVisualKey,
            source.VisualKey);
        Assert.Equal(처방기록PresentationCodes.FallbackVisualKey,
            source.FallbackVisualKey);
        Assert.StartsWith("fallback:",
            source.CandidateRevisionOrFingerprint);
    }

    [Fact]
    public void 입력순서가_달라도_Source순서와_Hash가_같다()
    {
        var family = Family(
            Card(처방지식CardStateCodes.Readable, "source:z"),
            Card(처방지식CardStateCodes.Blocked, "source:a"));
        var bindings = new[]
        {
            Binding("source:z", 처방기록PresentationCodes.OpenBookVisualKey),
            Binding("source:a", 처방기록PresentationCodes.LoosePaperVisualKey),
        };
        var projector = new 처방기록PresentationPreparationProjector();

        var first = projector.Project(family, bindings);
        family.Cards = family.Cards.Reverse().ToArray();
        var second = projector.Project(family, bindings.Reverse());

        Assert.Equal(new[] { "source:a", "source:z" },
            first.Sources.Select(value => value.KnowledgeSourceStableId));
        Assert.Equal(first.Sources.Select(value => value.PresentationStableId),
            second.Sources.Select(value => value.PresentationStableId));
        Assert.Equal(first.PlanHashSha256, second.PlanHashSha256);
    }

    [Fact]
    public void 중복_SourceBinding은_거부한다()
    {
        var family = Family(Card(처방지식CardStateCodes.Readable,
            "source:a"));
        var binding = Binding("source:a",
            처방기록PresentationCodes.OpenBookVisualKey);

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 처방기록PresentationPreparationProjector().Project(family,
                new[] { binding, binding }));

        Assert.Equal("RecipeKnowledgeVisualBindingInvalid", error.Message);
    }

    [Theory]
    [InlineData(처방기록PresentationCodes.OpenBookVisualKey)]
    [InlineData(처방기록PresentationCodes.LoosePaperVisualKey)]
    [InlineData(처방기록PresentationCodes.FallbackVisualKey)]
    public void 기존_세가지_주표현키와_임의형식의_판본값은_그대로_보존한다(
        string visualKey)
    {
        var binding = Binding("source:a", visualKey);
        binding.CandidateRevisionOrFingerprint = "opaque-candidate-reference";

        var preparation = Project(Card(처방지식CardStateCodes.Readable,
            "source:a"), binding);

        var source = Assert.Single(preparation.Sources);
        Assert.Equal(visualKey, source.VisualKey);
        Assert.Equal(처방기록PresentationCodes.FallbackVisualKey,
            source.FallbackVisualKey);
        Assert.Equal(binding.CandidateRevisionOrFingerprint,
            source.CandidateRevisionOrFingerprint);
        Assert.Equal("recipe-knowledge-source-candidates.r1",
            preparation.CandidateRevision);
        Assert.True(preparation.PresentationOnly);
        Assert.False(preparation.MutatesCanonicalState);
        Assert.False(source.CanConfirmAuthority);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Knowledge.Recipe.Record.Unapproved")]
    [InlineData("knowledge.recipe.record.openbook")]
    [InlineData("KNOWLEDGE.RECIPE.RECORD.OPENBOOK")]
    [InlineData(" " + 처방기록PresentationCodes.OpenBookVisualKey)]
    [InlineData(처방기록PresentationCodes.OpenBookVisualKey + " ")]
    [InlineData("Knowledge.Recipe.Record.loosePaper")]
    [InlineData("primitive.ReadableKnowledgeSourceMarker")]
    [InlineData(처방기록PresentationCodes.FallbackVisualKey + "\t")]
    public void 미허용_주표현키와_대소문자_공백변형은_정규화하지_않고_거부한다(
        string? visualKey)
    {
        var binding = Binding("source:a", visualKey!);

        var error = Assert.Throws<InvalidOperationException>(() =>
            Project(Card(처방지식CardStateCodes.Readable, "source:a"),
                binding));

        Assert.Equal("RecipeKnowledgeVisualBindingInvalid", error.Message);
        Assert.Equal(visualKey, binding.VisualKey);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Primitive.Unapproved")]
    [InlineData(처방기록PresentationCodes.OpenBookVisualKey)]
    [InlineData(처방기록PresentationCodes.LoosePaperVisualKey)]
    [InlineData("primitive.readableknowledgesourcemarker")]
    [InlineData(" " + 처방기록PresentationCodes.FallbackVisualKey)]
    [InlineData(처방기록PresentationCodes.FallbackVisualKey + " ")]
    public void 대체표현키는_기존_명시Primitive만_허용한다(string? fallbackKey)
    {
        var binding = Binding("source:a",
            처방기록PresentationCodes.OpenBookVisualKey);
        binding.FallbackVisualKey = fallbackKey!;

        var error = Assert.Throws<InvalidOperationException>(() =>
            Project(Card(처방지식CardStateCodes.Readable, "source:a"),
                binding));

        Assert.Equal("RecipeKnowledgeVisualBindingInvalid", error.Message);
        Assert.Equal(fallbackKey, binding.FallbackVisualKey);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void 잘못된_키의_실패는_기존결과와_입력을_변경하지_않는다(
        bool fallbackInvalid)
    {
        var family = Family(
            Card(처방지식CardStateCodes.Known, "source:z"),
            Card(처방지식CardStateCodes.Readable, "source:a"));
        var bindings = new[]
        {
            Binding("source:z", 처방기록PresentationCodes.LoosePaperVisualKey),
            Binding("source:a", 처방기록PresentationCodes.OpenBookVisualKey),
        };
        var invalid = Binding("source:a", fallbackInvalid
            ? 처방기록PresentationCodes.OpenBookVisualKey
            : "Knowledge.Recipe.Record.Unknown");
        if (fallbackInvalid)
            invalid.FallbackVisualKey = "Primitive.Unapproved";
        var projector = new 처방기록PresentationPreparationProjector();
        var familyBefore = System.Text.Json.JsonSerializer.Serialize(family);
        var bindingsBefore = System.Text.Json.JsonSerializer.Serialize(bindings);
        var invalidBefore = System.Text.Json.JsonSerializer.Serialize(invalid);
        var previous = projector.Project(family, bindings);
        var previousBefore = System.Text.Json.JsonSerializer.Serialize(previous);

        var error = Assert.Throws<InvalidOperationException>(() =>
            projector.Project(family, new[] { bindings[0], invalid }));
        var after = projector.Project(family, bindings.Reverse());

        Assert.Equal("RecipeKnowledgeVisualBindingInvalid", error.Message);
        Assert.Equal(familyBefore, System.Text.Json.JsonSerializer.Serialize(family));
        Assert.Equal(bindingsBefore, System.Text.Json.JsonSerializer.Serialize(bindings));
        Assert.Equal(invalidBefore, System.Text.Json.JsonSerializer.Serialize(invalid));
        Assert.Equal(previousBefore, System.Text.Json.JsonSerializer.Serialize(previous));
        Assert.Equal(previousBefore, System.Text.Json.JsonSerializer.Serialize(after));
        Assert.Equal(previous.PlanHashSha256, after.PlanHashSha256);
        Assert.Equal(new[] { "source:a", "source:z" },
            after.Sources.Select(value => value.KnowledgeSourceStableId));
    }

    private static 처방기록PresentationPreparation Project(
        처방지식CardProjection card,
        params 처방기록VisualBinding[] bindings)
        => new 처방기록PresentationPreparationProjector().Project(
            Family(card), bindings);

    private static 처방지식CardFamilyProjection Family(
        params 처방지식CardProjection[] cards)
        => new()
        {
            WorldStableId = "world:nature:test",
            PlayerStableId = "player:solo",
            WorldRevision = 7,
            Cards = cards,
            PresentationOnly = true,
        };

    private static 처방지식CardProjection Card(string stateCode,
        params string[] sourceStableIds)
        => new()
        {
            RecipeStableId = "recipe:nature:basic-herbal-tea.v1",
            StateCode = stateCode,
            KnowledgeSourceStableIds = sourceStableIds,
            BlockReasonCodes = stateCode == 처방지식CardStateCodes.Blocked
                ? new[] { "KnowledgeSourceNotAccessible" }
                : Array.Empty<string>(),
            WorkspaceItem = new CardWorkspaceItem(),
        };

    private static 처방기록VisualBinding Binding(string sourceStableId,
        string visualKey)
        => new()
        {
            KnowledgeSourceStableId = sourceStableId,
            VisualKey = visualKey,
            FallbackVisualKey = 처방기록PresentationCodes.FallbackVisualKey,
            CandidateRevisionOrFingerprint =
                "fixture:candidate-fingerprint.r1",
        };
}
