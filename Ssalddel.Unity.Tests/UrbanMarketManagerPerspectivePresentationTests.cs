using Ssalddel.Unity.Application;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Tests.UnityData;

public sealed class UrbanMarketManagerPerspectivePresentationTests
{
    [Fact]
    public async Task 기본운영World를_처리대기와진행중Queue로_분류한다()
    {
        var perspective = await Perspective();

        var pending = Assert.Single(perspective.PendingActions);
        Assert.Equal("market-shelf:potato", pending.ShelfWorldId.Value);
        Assert.Equal(200, pending.PriorityScore);
        Assert.Contains(마트관리자PriorityReasonCodes.ReplenishmentReady, pending.PriorityReasonCodes);
        Assert.Contains(마트관리자InteractionIntentCodes.PreviewShelfReplenishment, pending.AllowedInteractionIntentCodes);
        Assert.Single(perspective.InProgress);
        Assert.Empty(perspective.UrgentActions);
        Assert.Empty(perspective.DataAttention);
    }

    [Fact]
    public async Task 진열수량0은_SourcePlan이있어도_긴급Queue로분류한다()
    {
        var data = await Fixture();
        data.재고목록.Single(value => value.StableId == "market-inventory:potato-display").Quantity = 0;

        var perspective = Perspective(data);
        var urgent = Assert.Single(perspective.UrgentActions);

        Assert.Equal("market-shelf:potato", urgent.ShelfWorldId.Value);
        Assert.Contains(마트관리자PriorityReasonCodes.ShelfEmpty, urgent.PriorityReasonCodes);
        Assert.True(urgent.CanPreviewRequest);
        Assert.Contains(마트관리자InteractionIntentCodes.PreviewShelfReplenishment, urgent.AllowedInteractionIntentCodes);
    }

    [Fact]
    public async Task 무결성오류는_실행보다먼저_DataAttention으로격리한다()
    {
        var data = await Fixture();
        data.재고목록 = data.재고목록
            .Where(value => value.StableId != "market-inventory:potato-display")
            .ToArray();

        var perspective = Perspective(data);
        var attention = Assert.Single(perspective.DataAttention);

        Assert.Equal(400, attention.PriorityScore);
        Assert.Contains(마트관리자PriorityReasonCodes.DataIntegrityAttention, attention.PriorityReasonCodes);
        Assert.Contains(도심마트ReplenishmentBlockReasonCodes.DisplayInventoryMissing, attention.PriorityReasonCodes);
        Assert.False(attention.CanPreviewRequest);
        Assert.DoesNotContain(마트관리자InteractionIntentCodes.PreviewShelfReplenishment, attention.AllowedInteractionIntentCodes);
    }

    [Fact]
    public async Task 정상진열대는_30초ActionQueue에서숨긴다()
    {
        var data = await Fixture();
        data.재고목록.Single(value => value.StableId == "market-inventory:potato-display").Quantity = 8;

        var perspective = Perspective(data);

        Assert.Equal(1, perspective.NoActionNeededCount);
        Assert.DoesNotContain(perspective.ActionQueue, value => value.ShelfWorldId.Value == "market-shelf:potato");
    }

    [Fact]
    public async Task 같은우선순위에서는_StableId를결정적TieBreaker로사용한다()
    {
        var data = await Fixture();
        AddCandidateShelf(data, "market-shelf:apple", "market-product:apple", "market-inventory:apple-display", "market-inventory:apple-backroom");

        var perspective = Perspective(data);

        Assert.Equal(
            perspective.PendingActions.Select(value => value.ShelfWorldId.Value).OrderBy(value => value, StringComparer.Ordinal),
            perspective.PendingActions.Select(value => value.ShelfWorldId.Value));
        Assert.All(perspective.PendingActions, value => Assert.Equal(200, value.PriorityScore));
    }

    [Fact]
    public async Task Focus는_SharedGraph의연결관계와SourceLineage를보존한다()
    {
        var perspective = Perspective(await Fixture(), new WorldStableId("market-shelf:potato"));

        Assert.Contains(perspective.FocusWorldIds, value => value.Value == "market-product:potato");
        Assert.Contains(perspective.FocusWorldIds, value => value.Value == "market-inventory:potato-backroom");
        Assert.NotEmpty(perspective.FocusRelations);
        Assert.Contains(perspective.PendingActions.Single().SourceWorldIds, value => value.Value == "market-shelf:potato");
    }

    [Fact]
    public async Task Projector는_요약Queue진열대작업상세를_독립Surface로만든다()
    {
        var perspective = Perspective(await Fixture(), new WorldStableId("market-shelf:potato"));

        var surface = Project(perspective);

        Assert.Equal(30, surface.ManagerSummary.RefreshIntervalSeconds);
        Assert.Equal(1, surface.ManagerSummary.PendingCount);
        Assert.Contains("대기 1", surface.ManagerSummary.SummaryText, StringComparison.Ordinal);
        Assert.Equal(2, surface.PriorityQueue.Length);
        Assert.Equal(2, surface.Shelves.Length);
        Assert.All(surface.Shelves, value => Assert.True(value.ShelfWorldId.IsDefined));
        Assert.Single(surface.TaskMarkers);
        Assert.Single(surface.Details);
        Assert.True(surface.PriorityQueue.Single(value => value.QueueCode == 마트관리자QueueCodes.PendingActions).IsFocused);
        Assert.False(surface.PriorityQueue.Single(value => value.QueueCode == 마트관리자QueueCodes.InProgress).IsFocused);
    }

    [Fact]
    public async Task SourcePlanSurface합계는_보충후보수량과일치한다()
    {
        var perspective = await Perspective();

        var surface = Project(perspective);
        var pending = Assert.Single(perspective.PendingActions);

        Assert.Equal(pending.CandidateQuantity, surface.SourcePlans.Sum(value => int.Parse(value.QuantityText.Split(' ')[0])));
        Assert.All(surface.SourcePlans, value => Assert.NotEmpty(value.Identity.SourceWorldIds));
    }

    [Fact]
    public async Task Presentation은_판매속도없는품절예상시간을_만들지않는다()
    {
        var surface = Project(await Perspective());
        var text = string.Join("|",
            surface.PriorityQueue.Select(value => value.SummaryText)
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

    private static void AddCandidateShelf(
        도심마트운영DataSnapshot data,
        string shelfId,
        string productId,
        string displayInventoryId,
        string backroomInventoryId)
    {
        data.상품목록 = data.상품목록.Append(new 도심마트운영상품Data
        {
            StableId = productId,
            상품명 = "사과",
            판매단위 = "상자",
        }).ToArray();
        data.재고목록 = data.재고목록.Append(new 도심마트운영재고Data
        {
            StableId = displayInventoryId,
            ProductStableId = productId,
            LocationStableId = "market-location:sales-floor-a",
            Quantity = 1,
            QuantityUnit = "상자",
        }).Append(new 도심마트운영재고Data
        {
            StableId = backroomInventoryId,
            ProductStableId = productId,
            LocationStableId = "market-location:backroom-a",
            Quantity = 10,
            QuantityUnit = "상자",
        }).ToArray();
        data.진열대목록 = data.진열대목록.Append(new 도심마트운영진열대Data
        {
            StableId = shelfId,
            ProductStableId = productId,
            LocationStableId = "market-location:sales-floor-a",
            Capacity = 10,
            QuantityUnit = "상자",
        }).ToArray();
    }
}
