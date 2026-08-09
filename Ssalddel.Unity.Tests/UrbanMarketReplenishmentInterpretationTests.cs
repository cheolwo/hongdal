using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Tests.UnityData;

public sealed class UrbanMarketReplenishmentInterpretationTests
{
    [Fact]
    public async Task 감자진열대는_목표수량까지_6상자보충후보를_만든다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();

        var result = Interpret(data);
        var potato = Assert.Single(
            result.Replenishments,
            value => value.ShelfWorldId.Value == "market-shelf:potato");

        Assert.Equal(도심마트ReplenishmentNeedCodes.ReplenishmentCandidate, potato.NeedCode);
        Assert.Equal(2, potato.DisplayQuantity);
        Assert.Equal(10, potato.DisplayCapacity);
        Assert.Equal(8, potato.TargetQuantity);
        Assert.Equal(20, potato.BackroomOnHandQuantity);
        Assert.Equal(0, potato.BackroomAllocatedQuantity);
        Assert.Equal(20, potato.BackroomAvailableQuantity);
        Assert.Equal(6, potato.CandidateQuantity);
        Assert.True(potato.IsSourcePlanComplete);
        var source = Assert.Single(potato.SourcePlan);
        Assert.Equal("market-inventory:potato-backroom", source.InventoryWorldId.Value);
        Assert.Equal(6, source.Quantity);
        Assert.True(potato.CanPreviewRequest);
        Assert.Empty(potato.BlockReasonCodes);
        Assert.Contains(
            potato.SourceWorldIds,
            value => value.Value == "market-inventory:potato-backroom");
    }

    [Fact]
    public async Task 여러후방위치의가용재고를_결정적SourcePlan으로_나누어배정한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.재고목록.Single(value =>
            value.StableId == "market-inventory:potato-backroom").Quantity = 3;
        AddBackroomInventory(
            data,
            "market-location:backroom-b",
            "market-inventory:potato-backroom-b",
            "market-product:potato",
            5);

        var result = Interpret(data);
        var potato = Assert.Single(
            result.Replenishments,
            value => value.ShelfWorldId.Value == "market-shelf:potato");

        Assert.Equal(8, potato.BackroomOnHandQuantity);
        Assert.Equal(6, potato.CandidateQuantity);
        Assert.True(potato.IsSourcePlanComplete);
        Assert.Collection(
            potato.SourcePlan,
            first =>
            {
                Assert.Equal("market-inventory:potato-backroom", first.InventoryWorldId.Value);
                Assert.Equal("market-location:backroom-a", first.LocationWorldId.Value);
                Assert.Equal(3, first.Quantity);
            },
            second =>
            {
                Assert.Equal("market-inventory:potato-backroom-b", second.InventoryWorldId.Value);
                Assert.Equal("market-location:backroom-b", second.LocationWorldId.Value);
                Assert.Equal(3, second.Quantity);
            });
        Assert.True(potato.CanPreviewRequest);
    }

    [Fact]
    public async Task 명시적다중할당은_legacy단일Source를대체해_원천별로집계한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        AddBackroomInventory(
            data,
            "market-location:backroom-b",
            "market-inventory:onion-backroom-b",
            "market-product:onion",
            4);
        data.작업재고할당목록 = new[]
        {
            Allocation(
                "market-allocation:onion-a",
                "market-task:replenishment-onion",
                "market-inventory:onion-backroom",
                2),
            Allocation(
                "market-allocation:onion-b",
                "market-task:replenishment-onion",
                "market-inventory:onion-backroom-b",
                2),
        };

        var result = Interpret(data);
        var first = Assert.Single(
            result.InventoryAvailabilities,
            value => value.InventoryWorldId.Value == "market-inventory:onion-backroom");
        var second = Assert.Single(
            result.InventoryAvailabilities,
            value => value.InventoryWorldId.Value == "market-inventory:onion-backroom-b");

        Assert.Equal(2, first.AllocatedQuantity);
        Assert.Equal(6, first.AvailableQuantity);
        Assert.Equal(2, second.AllocatedQuantity);
        Assert.Equal(2, second.AvailableQuantity);
        Assert.Single(first.AllocatingAllocationWorldIds);
        Assert.Single(second.AllocatingAllocationWorldIds);
        var taskId = new Ssalddel.Unity.InterpretationContracts.WorldStableId(
            "market-task:replenishment-onion");
        Assert.DoesNotContain(
            result.SharedWorld.Graph.GetOutgoing(taskId),
            relation => relation.Kind == Ssalddel.Unity.InterpretationContracts.WorldRelationKind.Targets
                        && relation.To.Value == "market-inventory:onion-backroom");
        Assert.Equal(2, result.SharedWorld.Nodes
            .OfType<도심마트작업재고할당WorldNode>()
            .Count());
    }

    [Fact]
    public async Task 완료작업의legacy할당은_가용수량에서차감하지않는다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.재고목록.Single(value =>
            value.StableId == "market-inventory:potato-backroom").Quantity = 10;
        AddSecondPotatoShelfAndTask(data, 8);
        data.작업목록.Single(value =>
            value.StableId == "market-task:replenishment-potato-b").StateCode =
            도심마트TaskStateCodes.Completed;

        var result = Interpret(data);
        var availability = Assert.Single(
            result.InventoryAvailabilities,
            value => value.InventoryWorldId.Value == "market-inventory:potato-backroom");

        Assert.Equal(0, availability.AllocatedQuantity);
        Assert.Equal(10, availability.AvailableQuantity);
        Assert.Empty(availability.AllocatingTaskWorldIds);
    }

    [Fact]
    public async Task 명시적활성할당합이_작업수량과다르면_Data계약을거부한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.작업재고할당목록 = new[]
        {
            Allocation(
                "market-allocation:onion-partial",
                "market-task:replenishment-onion",
                "market-inventory:onion-backroom",
                3),
        };

        var errors = new 도심마트운영DataSnapshotValidator().Validate(data);

        Assert.Contains(
            "TaskAllocationQuantityMismatch:market-task:replenishment-onion",
            errors);
    }

    [Fact]
    public async Task 할당단위가재고단위와다르면_Data계약을거부한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        var allocation = Allocation(
            "market-allocation:onion-unit-mismatch",
            "market-task:replenishment-onion",
            "market-inventory:onion-backroom",
            4);
        allocation.QuantityUnit = "kg";
        data.작업재고할당목록 = new[] { allocation };

        var errors = new 도심마트운영DataSnapshotValidator().Validate(data);

        Assert.Contains(
            "TaskAllocationUnitMismatch:market-allocation:onion-unit-mismatch",
            errors);
        Assert.Contains(
            "TaskAllocationTaskUnitMismatch:market-allocation:onion-unit-mismatch",
            errors);
    }

    [Fact]
    public async Task 해제된할당은_가용수량에서차감하지않는다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        var active = Allocation(
            "market-allocation:onion-active",
            "market-task:replenishment-onion",
            "market-inventory:onion-backroom",
            4);
        var released = Allocation(
            "market-allocation:onion-released",
            "market-task:replenishment-onion",
            "market-inventory:onion-backroom",
            2);
        released.StateCode = 도심마트AllocationStateCodes.Released;
        data.작업재고할당목록 = new[] { active, released };

        var result = Interpret(data);
        var availability = Assert.Single(
            result.InventoryAvailabilities,
            value => value.InventoryWorldId.Value == "market-inventory:onion-backroom");

        Assert.Equal(4, availability.AllocatedQuantity);
        Assert.Equal(4, availability.AvailableQuantity);
        Assert.Single(availability.AllocatingAllocationWorldIds);
        Assert.Equal(
            "market-allocation:onion-active",
            availability.AllocatingAllocationWorldIds[0].Value);
    }

    [Fact]
    public async Task 존재하지않는원천재고할당은_Data계약을거부한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.작업재고할당목록 = new[]
        {
            Allocation(
                "market-allocation:onion-missing",
                "market-task:replenishment-onion",
                "market-inventory:missing",
                4),
        };

        var errors = new 도심마트운영DataSnapshotValidator().Validate(data);

        Assert.Contains(
            "TaskAllocationInventoryUnknown:market-allocation:onion-missing",
            errors);
    }

    [Fact]
    public async Task 중복할당StableId는_Data계약을거부한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.작업재고할당목록 = new[]
        {
            Allocation(
                "market-allocation:onion-duplicate",
                "market-task:replenishment-onion",
                "market-inventory:onion-backroom",
                2),
            Allocation(
                "market-allocation:onion-duplicate",
                "market-task:replenishment-onion",
                "market-inventory:onion-backroom",
                2),
        };

        var errors = new 도심마트운영DataSnapshotValidator().Validate(data);

        Assert.Contains(
            "DuplicateTaskAllocationStableId:market-allocation:onion-duplicate",
            errors);
    }

    [Fact]
    public async Task 활성진열보충작업이있으면_중복후보를_만들지않는다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();

        var result = Interpret(data);
        var onion = Assert.Single(
            result.Replenishments,
            value => value.ShelfWorldId.Value == "market-shelf:onion");

        Assert.Equal(도심마트ReplenishmentNeedCodes.TaskAlreadyActive, onion.NeedCode);
        Assert.Equal(8, onion.BackroomOnHandQuantity);
        Assert.Equal(4, onion.BackroomAllocatedQuantity);
        Assert.Equal(4, onion.BackroomAvailableQuantity);
        Assert.Equal(4, onion.ActiveTaskQuantity);
        Assert.Equal(0, onion.CandidateQuantity);
        Assert.False(onion.CanPreviewRequest);
        Assert.Contains(
            도심마트ReplenishmentBlockReasonCodes.ActiveTaskExists,
            onion.BlockReasonCodes);
    }

    [Fact]
    public async Task 다른진열대의활성작업도_같은원천재고의가용수량에서_차감한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.재고목록.Single(value =>
            value.StableId == "market-inventory:potato-backroom").Quantity = 10;
        AddSecondPotatoShelfAndTask(data, 8);

        var result = Interpret(data);
        var potato = Assert.Single(
            result.Replenishments,
            value => value.ShelfWorldId.Value == "market-shelf:potato");
        var availability = Assert.Single(
            result.InventoryAvailabilities,
            value => value.InventoryWorldId.Value == "market-inventory:potato-backroom");

        Assert.Equal(10, availability.OnHandQuantity);
        Assert.Equal(8, availability.AllocatedQuantity);
        Assert.Equal(2, availability.AvailableQuantity);
        Assert.False(availability.IsOversubscribed);
        Assert.Equal(10, potato.BackroomOnHandQuantity);
        Assert.Equal(8, potato.BackroomAllocatedQuantity);
        Assert.Equal(2, potato.BackroomAvailableQuantity);
        Assert.Equal(2, potato.CandidateQuantity);
        Assert.Equal(도심마트ReplenishmentNeedCodes.ReplenishmentCandidate, potato.NeedCode);
    }

    [Fact]
    public async Task 원천재고보다활성할당이많으면_실행후보가아닌_판단불가로_해석한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.재고목록.Single(value =>
            value.StableId == "market-inventory:potato-backroom").Quantity = 5;
        AddSecondPotatoShelfAndTask(data, 8);

        var result = Interpret(data);
        var potato = Assert.Single(
            result.Replenishments,
            value => value.ShelfWorldId.Value == "market-shelf:potato");
        var availability = Assert.Single(
            result.InventoryAvailabilities,
            value => value.InventoryWorldId.Value == "market-inventory:potato-backroom");

        Assert.True(availability.IsOversubscribed);
        Assert.Equal(0, availability.AvailableQuantity);
        Assert.Equal(도심마트ReplenishmentNeedCodes.DataInsufficient, potato.NeedCode);
        Assert.Equal(0, potato.CandidateQuantity);
        Assert.False(potato.CanPreviewRequest);
        Assert.Contains(
            도심마트ReplenishmentBlockReasonCodes.InventoryOversubscribed,
            potato.BlockReasonCodes);
    }

    [Fact]
    public async Task 후방재고가모두다른작업에할당되면_재고없음이아닌_가용수량부족이다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.재고목록.Single(value =>
            value.StableId == "market-inventory:potato-backroom").Quantity = 8;
        AddSecondPotatoShelfAndTask(data, 8);

        var result = Interpret(data);
        var potato = Assert.Single(
            result.Replenishments,
            value => value.ShelfWorldId.Value == "market-shelf:potato");

        Assert.Equal(8, potato.BackroomOnHandQuantity);
        Assert.Equal(8, potato.BackroomAllocatedQuantity);
        Assert.Equal(0, potato.BackroomAvailableQuantity);
        Assert.Equal(도심마트ReplenishmentNeedCodes.InboundRequired, potato.NeedCode);
        Assert.Contains(
            도심마트ReplenishmentBlockReasonCodes.AvailableQuantityInsufficient,
            potato.BlockReasonCodes);
        Assert.DoesNotContain(
            도심마트ReplenishmentBlockReasonCodes.BackroomInventoryMissing,
            potato.BlockReasonCodes);
    }

    [Fact]
    public async Task 보관재고가없으면_입고필요로_해석한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.재고목록.Single(value =>
            value.StableId == "market-inventory:potato-backroom").Quantity = 0;

        var result = Interpret(data);
        var potato = Assert.Single(
            result.Replenishments,
            value => value.ShelfWorldId.Value == "market-shelf:potato");

        Assert.Equal(도심마트ReplenishmentNeedCodes.InboundRequired, potato.NeedCode);
        Assert.Equal(0, potato.CandidateQuantity);
        Assert.Contains(
            도심마트ReplenishmentBlockReasonCodes.BackroomInventoryMissing,
            potato.BlockReasonCodes);
    }

    [Fact]
    public async Task 서버Capability가없으면_후보는보존하지만_preview를_허용하지않는다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.ServerCapabilityCodes = Array.Empty<string>();

        var result = Interpret(data);
        var potato = Assert.Single(
            result.Replenishments,
            value => value.ShelfWorldId.Value == "market-shelf:potato");

        Assert.Equal(도심마트ReplenishmentNeedCodes.ReplenishmentCandidate, potato.NeedCode);
        Assert.Equal(6, potato.CandidateQuantity);
        Assert.False(potato.CanPreviewRequest);
        Assert.Contains(
            도심마트ReplenishmentBlockReasonCodes.ServerCapabilityMissing,
            potato.BlockReasonCodes);
    }

    [Fact]
    public async Task 진열재고관계가없으면_판단불가로_해석한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.재고목록 = data.재고목록
            .Where(value => value.StableId != "market-inventory:potato-display")
            .ToArray();

        var result = Interpret(data);
        var potato = Assert.Single(
            result.Replenishments,
            value => value.ShelfWorldId.Value == "market-shelf:potato");

        Assert.Equal(도심마트ReplenishmentNeedCodes.DataInsufficient, potato.NeedCode);
        Assert.Equal(0, potato.CandidateQuantity);
        Assert.Contains(
            도심마트ReplenishmentBlockReasonCodes.DisplayInventoryMissing,
            potato.BlockReasonCodes);
    }

    [Fact]
    public async Task RuleRevision과목표율은_업무InterpretationRevision을_변경한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        var baseWorld = new 도심마트운영SharedWorldInterpreter().Interpret(
            data,
            도심마트SharedInterpretationContext.Operations());
        var interpreter = new 도심마트진열보충Interpreter();

        var first = interpreter.Interpret(
            baseWorld,
            new 도심마트ReplenishmentRuleSet(80, "rule-v1"));
        var second = interpreter.Interpret(
            baseWorld,
            new 도심마트ReplenishmentRuleSet(90, "rule-v2"));

        Assert.NotEqual(first.Lineage.InterpretationRevision, second.Lineage.InterpretationRevision);
        Assert.Equal(8, first.Replenishments.Single(value =>
            value.ShelfWorldId.Value == "market-shelf:potato").TargetQuantity);
        Assert.Equal(9, second.Replenishments.Single(value =>
            value.ShelfWorldId.Value == "market-shelf:potato").TargetQuantity);
    }

    private static 도심마트운영업무WorldState Interpret(도심마트운영DataSnapshot data)
    {
        var world = new 도심마트운영SharedWorldInterpreter().Interpret(
            data,
            도심마트SharedInterpretationContext.Operations());
        return new 도심마트진열보충Interpreter().Interpret(
            world,
            도심마트ReplenishmentRuleSet.SimulationDefault());
    }

    private static void AddSecondPotatoShelfAndTask(
        도심마트운영DataSnapshot data,
        int taskQuantity)
    {
        data.위치목록 = data.위치목록.Append(new 도심마트운영위치Data
        {
            StableId = "market-location:sales-floor-b",
            이름 = "판매장 B",
            KindCode = 도심마트LocationKindCodes.SalesFloor,
        }).ToArray();
        data.재고목록 = data.재고목록.Append(new 도심마트운영재고Data
        {
            StableId = "market-inventory:potato-display-b",
            ProductStableId = "market-product:potato",
            LocationStableId = "market-location:sales-floor-b",
            Quantity = 1,
            QuantityUnit = "상자",
        }).ToArray();
        data.진열대목록 = data.진열대목록.Append(new 도심마트운영진열대Data
        {
            StableId = "market-shelf:potato-b",
            ProductStableId = "market-product:potato",
            LocationStableId = "market-location:sales-floor-b",
            Capacity = 10,
            QuantityUnit = "상자",
        }).ToArray();
        data.작업목록 = data.작업목록.Append(new 도심마트운영작업Data
        {
            StableId = "market-task:replenishment-potato-b",
            KindCode = 도심마트TaskKindCodes.ShelfReplenishment,
            StateCode = 도심마트TaskStateCodes.Assigned,
            ProductStableId = "market-product:potato",
            SourceInventoryStableId = "market-inventory:potato-backroom",
            TargetShelfStableId = "market-shelf:potato-b",
            Quantity = taskQuantity,
            QuantityUnit = "상자",
        }).ToArray();
    }

    private static void AddBackroomInventory(
        도심마트운영DataSnapshot data,
        string locationStableId,
        string inventoryStableId,
        string productStableId,
        int quantity)
    {
        data.위치목록 = data.위치목록.Append(new 도심마트운영위치Data
        {
            StableId = locationStableId,
            이름 = "후방 보관 추가",
            KindCode = 도심마트LocationKindCodes.Backroom,
        }).ToArray();
        data.재고목록 = data.재고목록.Append(new 도심마트운영재고Data
        {
            StableId = inventoryStableId,
            ProductStableId = productStableId,
            LocationStableId = locationStableId,
            Quantity = quantity,
            QuantityUnit = "상자",
        }).ToArray();
    }

    private static 도심마트운영작업재고할당Data Allocation(
        string stableId,
        string taskStableId,
        string inventoryStableId,
        int quantity)
        => new 도심마트운영작업재고할당Data
        {
            StableId = stableId,
            TaskStableId = taskStableId,
            InventoryStableId = inventoryStableId,
            Quantity = quantity,
            QuantityUnit = "상자",
            StateCode = 도심마트AllocationStateCodes.Active,
            Revision = "simulation:allocation:1",
        };
}
