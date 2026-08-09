using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.UrbanMarket;

namespace Ssalddel.Tests.UnityData;

public sealed class UrbanMarketSharedWorldTests
{
    [Fact]
    public void 공개상품SharedWorld는_물리재고와진열대노드를_만들지않는다()
    {
        var data = new 도심마트공개상품DataMapper().Map(PublicResponse());
        var world = new 도심마트공개상품SharedWorldInterpreter().Interpret(
            data,
            도심마트SharedInterpretationContext.PublicProducts());

        Assert.Contains(world.Nodes, node => node is 도심마트RootWorldNode);
        Assert.Single(world.Nodes.OfType<도심마트공개상품WorldNode>());
        Assert.Empty(world.Nodes.OfType<도심마트재고WorldNode>());
        Assert.Empty(world.Nodes.OfType<도심마트진열대WorldNode>());
        Assert.Contains(
            도심마트LimitationCodes.PhysicalInventoryNotProvided,
            world.Lineage.LimitationCodes);
        Assert.Single(world.Relations, relation => relation.Kind == WorldRelationKind.Contains);
    }

    [Fact]
    public async Task 관리자Simulation은_상품위치재고진열대작업을_typedGraph로_구성한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        var world = new 도심마트운영SharedWorldInterpreter().Interpret(
            data,
            도심마트SharedInterpretationContext.Operations());

        Assert.Equal(DataRuntimeMode.Simulation, world.Mode);
        Assert.Equal(2, world.Nodes.OfType<도심마트운영상품WorldNode>().Count());
        Assert.Equal(2, world.Nodes.OfType<도심마트위치WorldNode>().Count());
        Assert.Equal(4, world.Nodes.OfType<도심마트재고WorldNode>().Count());
        Assert.Equal(2, world.Nodes.OfType<도심마트진열대WorldNode>().Count());
        var task = Assert.Single(world.Nodes.OfType<도심마트작업WorldNode>());
        var allocation = Assert.Single(world.Nodes.OfType<도심마트작업재고할당WorldNode>());
        Assert.Equal(task.StableId, allocation.TaskWorldId);
        Assert.Equal("market-inventory:onion-backroom", allocation.InventoryWorldId.Value);

        var taskTargets = world.Graph.GetOutgoing(task.StableId)
            .Where(relation => relation.Kind == WorldRelationKind.Targets)
            .Select(relation => relation.To.Value)
            .ToArray();
        Assert.Equal(
            new[]
            {
                "market-inventory:onion-backroom",
                "market-product:onion",
                "market-shelf:onion",
            },
            taskTargets.OrderBy(value => value, StringComparer.Ordinal));
        Assert.Contains(
            world.Graph.GetOutgoing(allocation.StableId),
            relation => relation.Kind == WorldRelationKind.DerivedFrom
                        && relation.To == task.StableId);

        var inventory = world.Nodes.OfType<도심마트재고WorldNode>()
            .Single(node => node.StableId.Value == "market-inventory:potato-backroom");
        Assert.Contains(
            world.Graph.GetOutgoing(inventory.StableId),
            relation => relation.Kind == WorldRelationKind.LocatedAt
                        && relation.To.Value == "market-location:backroom-a");
        Assert.Contains(도심마트CapabilityCodes.CreateShelfReplenishment, world.ServerCapabilityCodes);
        Assert.Contains(도심마트LimitationCodes.SimulationOnly, world.Lineage.LimitationCodes);
    }

    [Fact]
    public async Task 운영DataValidator는_danglingTask관계를_거부한다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        data.작업목록[0].SourceInventoryStableId = "market-inventory:missing";

        var errors = new 도심마트운영DataSnapshotValidator().Validate(data);

        Assert.Contains("TaskInventoryUnknown:market-task:replenishment-onion", errors);
        Assert.Equal(
            "TaskInventoryUnknown:market-task:replenishment-onion",
            Assert.Throws<InvalidOperationException>(() =>
                new 도심마트운영SharedWorldInterpreter().Interpret(
                    data,
                    도심마트SharedInterpretationContext.Operations())).Message);
    }

    [Fact]
    public async Task 같은Data와Rule은_같은InterpretationRevision을_만든다()
    {
        var data = await new Simulated도심마트운영DataQuery().조회Async();
        var interpreter = new 도심마트운영SharedWorldInterpreter();
        var context = 도심마트SharedInterpretationContext.Operations();

        var first = interpreter.Interpret(data, context);
        var second = interpreter.Interpret(data, context);
        data.DataRevision = "simulation:market-operations:2";
        var changed = interpreter.Interpret(data, context);

        Assert.Equal(first.Lineage.InterpretationRevision, second.Lineage.InterpretationRevision);
        Assert.NotEqual(first.Lineage.InterpretationRevision, changed.Lineage.InterpretationRevision);
        Assert.StartsWith("interpretation:", first.Lineage.InterpretationRevision);
    }

    [Fact]
    public void SharedWorldNode는_Unity표현속성을_갖지않는다()
    {
        var propertyNames = typeof(도심마트WorldNode)
            .Assembly
            .GetTypes()
            .Where(type => type.IsSubclassOf(typeof(도심마트WorldNode)))
            .SelectMany(type => type.GetProperties())
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("Color", propertyNames);
        Assert.DoesNotContain("SocketKey", propertyNames);
        Assert.DoesNotContain("DetailText", propertyNames);
        Assert.DoesNotContain("Prefab", propertyNames);
    }

    private static 도심마트목록ApiModel PublicResponse()
        => new()
        {
            TotalCount = 1,
            재고기준안내 = "내부 재고가 아닌 판매 가능 수량 projection",
            Items =
            [
                new 도심마트상품ApiModel
                {
                    Id = 41,
                    상품명 = "감자",
                    판매단위 = "20kg",
                    판매가 = 35_000m,
                    판매가능수량 = 12,
                    판매가능여부 = true,
                    재고기준시각 = DateTimeOffset.Parse("2026-08-08T01:00:00Z"),
                    수정시각 = DateTimeOffset.Parse("2026-08-08T01:05:00Z"),
                },
            ],
        };
}
