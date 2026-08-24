using Ssalddel.Unity.Application;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Tests.UnityData;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class UrbanMarketManagerPerspectivePresentationTests
{
    [Fact]
    public async Task 기본운영World는_우선순위없이_모든진열상태를보존한다()
    {
        var perspective = await Perspective();

        Assert.Equal(2, perspective.ShelfStates.Length);
        Assert.Equal(
            perspective.ShelfStates.Select(value => value.ShelfWorldId.Value)
                .OrderBy(value => value, StringComparer.Ordinal),
            perspective.ShelfStates.Select(value => value.ShelfWorldId.Value));
        var potato = Assert.Single(
            perspective.ShelfStates,
            value => value.ShelfWorldId.Value == "market-shelf:potato");
        var onion = Assert.Single(
            perspective.ShelfStates,
            value => value.ShelfWorldId.Value == "market-shelf:onion");
        Assert.Equal(도심마트ReplenishmentNeedCodes.ReplenishmentCandidate, potato.NeedCode);
        Assert.Contains(
            마트관리자InteractionIntentCodes.PreviewShelfReplenishment,
            potato.AllowedInteractionIntentCodes);
        Assert.Equal(도심마트ReplenishmentNeedCodes.TaskAlreadyActive, onion.NeedCode);
        Assert.Contains(
            마트관리자InteractionIntentCodes.ReviewTaskProgress,
            onion.AllowedInteractionIntentCodes);
    }

    [Fact]
    public async Task 진열수량0도_긴급점수없이_SharedNeed와SourcePlan을보존한다()
    {
        var data = await Fixture();
        data.재고목록.Single(value => value.StableId == "market-inventory:potato-display").Quantity = 0;

        var perspective = Perspective(data);
        var potato = Assert.Single(
            perspective.ShelfStates,
            value => value.ShelfWorldId.Value == "market-shelf:potato");

        Assert.Equal(0, potato.DisplayQuantity);
        Assert.Equal(도심마트ReplenishmentNeedCodes.ReplenishmentCandidate, potato.NeedCode);
        Assert.True(potato.IsSourcePlanComplete);
        Assert.True(potato.CanPreviewRequest);
    }

    [Fact]
    public async Task 무결성오류는_Queue분류없이_Need와차단사유를보존한다()
    {
        var data = await Fixture();
        data.재고목록 = data.재고목록
            .Where(value => value.StableId != "market-inventory:potato-display")
            .ToArray();

        var perspective = Perspective(data);
        var potato = Assert.Single(
            perspective.ShelfStates,
            value => value.ShelfWorldId.Value == "market-shelf:potato");

        Assert.Equal(도심마트ReplenishmentNeedCodes.DataInsufficient, potato.NeedCode);
        Assert.Contains(
            도심마트ReplenishmentBlockReasonCodes.DisplayInventoryMissing,
            potato.BlockReasonCodes);
        Assert.False(potato.CanPreviewRequest);
        Assert.DoesNotContain(
            마트관리자InteractionIntentCodes.PreviewShelfReplenishment,
            potato.AllowedInteractionIntentCodes);
    }

    [Fact]
    public async Task 정상진열대도_Perspective에서숨기지않는다()
    {
        var data = await Fixture();
        data.재고목록.Single(value => value.StableId == "market-inventory:potato-display").Quantity = 8;

        var perspective = Perspective(data);
        var potato = Assert.Single(
            perspective.ShelfStates,
            value => value.ShelfWorldId.Value == "market-shelf:potato");

        Assert.Equal(도심마트ReplenishmentNeedCodes.NoActionNeeded, potato.NeedCode);
    }

    [Fact]
    public async Task Focus는_SharedGraph의연결관계와SourceLineage를보존한다()
    {
        var perspective = Perspective(await Fixture(), new WorldStableId("market-shelf:potato"));

        Assert.Contains(perspective.FocusWorldIds, value => value.Value == "market-product:potato");
        Assert.Contains(perspective.FocusWorldIds, value => value.Value == "market-inventory:potato-backroom");
        Assert.NotEmpty(perspective.FocusRelations);
        Assert.Contains(
            perspective.ShelfStates.Single(value => value.ShelfWorldId.Value == "market-shelf:potato").SourceWorldIds,
            value => value.Value == "market-shelf:potato");
    }

    [Fact]
    public async Task Projector는_Queue없이_진열대작업계획상세Surface만만든다()
    {
        var perspective = Perspective(await Fixture(), new WorldStableId("market-shelf:potato"));

        var surface = Project(perspective);

        Assert.Equal(2, surface.Shelves.Length);
        Assert.All(surface.Shelves, value => Assert.True(value.ShelfWorldId.IsDefined));
        Assert.Single(surface.TaskMarkers);
        Assert.Single(surface.SourcePlans);
        Assert.Single(surface.Details);
        Assert.True(surface.Shelves.Single(value => value.ShelfWorldId.Value == "market-shelf:potato").IsHighlighted);
        Assert.False(surface.Shelves.Single(value => value.ShelfWorldId.Value == "market-shelf:onion").IsHighlighted);
    }

    [Fact]
    public async Task SourcePlanSurface합계는_보충후보수량과일치한다()
    {
        var perspective = await Perspective();

        var surface = Project(perspective);
        var potato = Assert.Single(
            perspective.ShelfStates,
            value => value.ShelfWorldId.Value == "market-shelf:potato");

        Assert.Equal(
            potato.CandidateQuantity,
            surface.SourcePlans.Sum(value => int.Parse(value.QuantityText.Split(' ')[0])));
        Assert.All(surface.SourcePlans, value => Assert.NotEmpty(value.Identity.SourceWorldIds));
    }

    [Fact]
    public async Task Presentation은_판매속도없는품절예상시간을_만들지않는다()
    {
        var surface = Project(await Perspective());
        var text = string.Join("|",
            surface.Shelves.Select(value => value.QuantityText)
                .Concat(surface.Details.Select(value => value.ReasonText)));

        Assert.DoesNotContain("곧 품절", text, StringComparison.Ordinal);
        Assert.DoesNotContain("시간 후", text, StringComparison.Ordinal);
        Assert.DoesNotContain("매출 영향", text, StringComparison.Ordinal);
    }

    private static async Task<마트관리자PerspectiveWorldState> Perspective()
        => Perspective(await Fixture());

    private static 마트관리자PerspectiveWorldState Perspective(
        도심마트운영DataSnapshot data,
        WorldStableId? focus = null)
    {
        var shared = new 도심마트운영업무SharedWorldInterpreter(
            new 도심마트운영SharedWorldInterpreter(),
            new 도심마트진열보충Interpreter(),
            도심마트ReplenishmentRuleSet.SimulationDefault())
            .Interpret(data, 도심마트SharedInterpretationContext.Operations());
        return new 마트관리자PerspectiveInterpreter().Interpret(
            shared,
            new InterpretationPerspectiveContext(
                마트관리자PerspectiveCodes.Role,
                마트관리자PerspectiveCodes.ReviewReplenishment,
                마트관리자PerspectiveCodes.Zone,
                WorldInterpretationMode.Simulation,
                focus));
    }

    private static 도심마트PresentationSnapshot Project(마트관리자PerspectiveWorldState perspective)
        => new 도심마트PresentationProjector(new 도심마트ManagerVisualPolicy())
            .Project(perspective, new 도심마트ManagerPresentationContext());

    private static Task<도심마트운영DataSnapshot> Fixture()
        => new Simulated도심마트운영DataQuery().조회Async();
}
