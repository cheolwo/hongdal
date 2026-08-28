using System.Text.Json;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Unity.Cards;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "처방 지식 카드 가족의 상태 구분, 결정성, Revision 경계를 검증한다.",
    Boundary = "자동 시험은 실제 Unity Scene·Game View 증거를 대신하지 않는다.")]
public sealed class Simulation처방지식CardFamilyTests
{
    [Fact]
    public async Task 카드가_Known_Readable_Blocked로_결정적으로_정렬되고_서랍에_연결된다()
    {
        var ledger = Ledger("recipe:z-known.v1");
        var previews = new[]
        {
            Preview("recipe:m-blocked.v1", "source:z", false, false,
                "BlockedB", "BlockedA"),
            Preview("recipe:a-readable.v1", "source:b", false, true),
            Preview("recipe:z-known.v1", "source:known", true, false),
            Preview("recipe:a-readable.v1", "source:a", false, true),
        };
        var projector = new 처방지식CardFamilyProjector();

        var projection = projector.Project(ledger, previews.Reverse());
        var workspace = await new CardWorkspaceCoordinator(new ICardFamilySource[]
        {
            new 처방지식CardFamilySource(projection),
        }).LoadAsync();

        Assert.True(projection.PresentationOnly);
        Assert.Equal(7, projection.WorldRevision);
        Assert.Equal(new[]
        {
            "recipe:a-readable.v1",
            "recipe:m-blocked.v1",
            "recipe:z-known.v1",
        }, projection.Cards.Select(value => value.RecipeStableId));
        Assert.Equal(new[]
        {
            처방지식CardStateCodes.Readable,
            처방지식CardStateCodes.Blocked,
            처방지식CardStateCodes.Known,
        }, projection.Cards.Select(value => value.StateCode));
        Assert.Equal(new[] { "source:a", "source:b" },
            projection.Cards[0].KnowledgeSourceStableIds);
        Assert.Equal(new[] { "BlockedA", "BlockedB" },
            projection.Cards[1].BlockReasonCodes);
        Assert.True(projection.Cards[0].WorkspaceItem.IsAvailable);
        Assert.True(projection.Cards[1].WorkspaceItem.IsLocked);
        Assert.True(projection.Cards[2].WorkspaceItem.IsAvailable);
        Assert.All(workspace.Items, item =>
            Assert.Equal(CardAuthorityCodes.ProjectionReadOnly,
                item.AuthorityCode));
        Assert.Equal(CardFamilyCodes.RecipeKnowledge,
            Assert.Single(workspace.LoadedFamilyCodes));
        Assert.True(workspace.PresentationOnly);
    }

    [Fact]
    public void 입력순서가_달라도_카드와_사유의_순서는_같다()
    {
        var ledger = Ledger();
        var first = new[]
        {
            Preview("recipe:b.v1", "source:b", false, false, "ReasonZ"),
            Preview("recipe:a.v1", "source:c", false, false, "ReasonB"),
            Preview("recipe:a.v1", "source:a", false, false, "ReasonA"),
        };
        var projector = new 처방지식CardFamilyProjector();

        var left = projector.Project(ledger, first);
        var right = projector.Project(ledger, first.Reverse());

        Assert.Equal(left.Cards.Select(CardIdentity),
            right.Cards.Select(CardIdentity));
    }

    [Theory]
    [InlineData(8, "player:solo", false)]
    [InlineData(7, "player:other", false)]
    [InlineData(7, "player:solo", true)]
    public void 다른_Revision_Player_AlreadyKnown_상태는_거부한다(
        long previewRevision, string previewPlayer, bool alreadyKnown)
    {
        var ledger = Ledger();
        var preview = Preview("recipe:a.v1", "source:a", alreadyKnown, false);
        preview.ObservedWorldRevision = previewRevision;
        preview.PlayerStableId = previewPlayer;

        var error = Assert.Throws<InvalidOperationException>(() =>
            new 처방지식CardFamilyProjector().Project(ledger,
                new[] { preview }));

        Assert.Equal("RecipeKnowledgePreviewSnapshotMismatch", error.Message);
    }

    [Fact]
    public void 투영은_원장과_Preview를_변경하지_않는다()
    {
        var ledger = Ledger();
        var preview = Preview("recipe:a.v1", "source:a", false, true);
        var knownBefore = ledger.KnownRecipeStableIds.ToArray();
        var reasonsBefore = preview.BlockReasonCodes.ToArray();

        _ = new 처방지식CardFamilyProjector().Project(ledger,
            new[] { preview });

        Assert.Equal(7, ledger.WorldRevision);
        Assert.Equal(knownBefore, ledger.KnownRecipeStableIds);
        Assert.Equal(7, preview.ObservedWorldRevision);
        Assert.Equal(reasonsBefore, preview.BlockReasonCodes);
    }

    private static string CardIdentity(처방지식CardProjection value)
        => string.Join("|", value.RecipeStableId, value.StateCode,
            string.Join(",", value.KnowledgeSourceStableIds),
            string.Join(",", value.BlockReasonCodes),
            value.WorkspaceItem.CardStableId,
            value.WorkspaceItem.CardCopyStableId);

    private static 플레이어지식LedgerApiModel Ledger(
        params string[] knownRecipeStableIds)
        => RoundTrip<플레이어지식LedgerApiModel>(
            new Simulation플레이어지식LedgerSnapshot
            {
                WorldStableId = "world:nature:test",
                SessionStableId = "session:nature:test",
                PlayerStableId = "player:solo",
                WorldRevision = 7,
                KnownRecipeStableIds = knownRecipeStableIds,
                StateHashSha256 = "fixture-hash",
            });

    private static 지식습득PreviewApiModel Preview(string recipeId,
        string sourceId, bool alreadyKnown, bool canConfirm,
        params string[] blockReasonCodes)
        => RoundTrip<지식습득PreviewApiModel>(
            new Simulation지식습득PreviewSnapshot
            {
                ObservedWorldRevision = 7,
                PlayerStableId = "player:solo",
                RecipeStableId = recipeId,
                KnowledgeSourceStableId = sourceId,
                AlreadyKnown = alreadyKnown,
                CanConfirm = canConfirm,
                BlockReasonCodes = blockReasonCodes,
            });

    private static TApi RoundTrip<TApi>(object contract)
        => JsonSerializer.Deserialize<TApi>(JsonSerializer.Serialize(contract))
            ?? throw new InvalidOperationException(
                "RecipeKnowledgeWireRoundTripFailed");
}
