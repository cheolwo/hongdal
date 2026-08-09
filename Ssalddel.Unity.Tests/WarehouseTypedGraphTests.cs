using Ssalddel.Unity.InterpretationContracts;
using Ssalddel.Unity.Warehouse;

namespace Ssalddel.Tests.UnityData;

public sealed class WarehouseTypedGraphTests
{
    [Fact]
    public void WarehouseGraph는_재고작업Npc를_Targets와AssignedTo로관계화한다()
    {
        var snapshot = BasicSnapshot();

        var graph = new WarehouseWorldGraphBuilder().Build(snapshot);

        Assert.Contains(graph.Relations, relation =>
            relation.Kind == WorldRelationKind.Targets
            && relation.From.Value == "warehouse-task:1"
            && relation.To.Value == "warehouse-inventory:1");
        Assert.Contains(graph.Relations, relation =>
            relation.Kind == WorldRelationKind.AssignedTo
            && relation.From.Value == "warehouse-task:1"
            && relation.To.Value == "warehouse-npc:1");
    }

    [Fact]
    public void Selection은_문자열Kind분기대신_TypedGraph연결요소를사용한다()
    {
        var snapshot = BasicSnapshot();

        var selection = new WarehouseRelationResolver().Select(snapshot, "warehouse-inventory:1");

        Assert.Equal(
            new[] { "warehouse-task:1", "warehouse-npc:1" },
            selection.Related.Select(value => value.StableId));
    }

    private static WarehouseWorldSnapshot BasicSnapshot()
        => new()
        {
            StableId = "warehouse-zone:1",
            Revision = "revision-1",
            GeneratedAtUtc = DateTimeOffset.Parse("2026-08-08T09:00:00Z"),
            Objects =
            [
                new WarehouseWorldObject
                {
                    StableId = "warehouse-inventory:1",
                    Kind = "Inventory",
                    Title = "감자",
                },
                new WarehouseWorldObject
                {
                    StableId = "warehouse-task:1",
                    Kind = "Task",
                    Title = "적재",
                    SourceStableId = "warehouse-inventory:1",
                },
                new WarehouseWorldObject
                {
                    StableId = "warehouse-npc:1",
                    Kind = "Npc",
                    Title = "DockWorker",
                    SourceStableId = "warehouse-task:1",
                },
            ],
        };
}
