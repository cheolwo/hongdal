using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Tests.UnityData;

public sealed class WorldIdentityGraphTests
{
    [Fact]
    public void 세Identity는_같은문자열이어도_명시적인Lineage로연결한다()
    {
        var source = new SourceStableId("inventory:item-17");
        var world = new WorldStableId("inventory:item-17");
        var presentation = new PresentationStableId("warehouse:inventory:item-17");

        var worldLineage = new WorldIdentityLineage(world, new[] { source });
        var presentationLineage = new PresentationIdentityLineage(presentation, new[] { world });

        Assert.Equal(source, Assert.Single(worldLineage.SourceIds));
        Assert.Equal(world, Assert.Single(presentationLineage.SourceWorldIds));
        Assert.Equal("warehouse:inventory:item-17", presentationLineage.PresentationId.Value);
    }

    [Fact]
    public void Identity는_StableId규칙을위반하면_생성을거부한다()
    {
        Assert.Throws<ArgumentException>(() => new SourceStableId("Invalid ID"));
        Assert.Throws<ArgumentException>(() => new WorldStableId("missing-colon"));
        Assert.Throws<ArgumentException>(() => new PresentationStableId("Upper:Case"));
    }

    [Fact]
    public void GraphIndex는_정방향과역방향Typed관계를제공한다()
    {
        var inventory = Node("inventory:item-17");
        var task = Node("task:putaway-31");
        var npc = Node("npc:dock-worker-4");
        var index = new WorldGraphIndex<TestNode>(
            new[] { inventory, task, npc },
            new[]
            {
                new WorldRelation(task.StableId, inventory.StableId, WorldRelationKind.Targets),
                new WorldRelation(task.StableId, npc.StableId, WorldRelationKind.AssignedTo),
            });

        Assert.Equal(2, index.GetOutgoing(task.StableId).Length);
        Assert.Equal(WorldRelationKind.Targets, Assert.Single(index.GetIncoming(inventory.StableId)).Kind);
        Assert.Same(npc, index.GetRequiredNode(npc.StableId));
    }

    [Fact]
    public void GraphIndex는_중복Node를거부한다()
    {
        var first = Node("inventory:item-17");
        var error = Assert.Throws<WorldGraphContractException>(() =>
            new WorldGraphIndex<TestNode>(new[] { first, Node("inventory:item-17") }, Array.Empty<WorldRelation>()));

        Assert.Equal("DuplicateWorldNode", error.ErrorCode);
    }

    [Fact]
    public void GraphIndex는_존재하지않는Relation대상을거부한다()
    {
        var task = Node("task:putaway-31");
        var missing = new WorldStableId("inventory:missing-1");
        var error = Assert.Throws<WorldGraphContractException>(() =>
            new WorldGraphIndex<TestNode>(
                new[] { task },
                new[] { new WorldRelation(task.StableId, missing, WorldRelationKind.Targets) }));

        Assert.Equal("WorldRelationTargetUnknown", error.ErrorCode);
        Assert.Equal(missing.Value, error.StableId);
    }

    [Fact]
    public void GraphIndex는_같은TypedRelation중복을거부한다()
    {
        var task = Node("task:putaway-31");
        var inventory = Node("inventory:item-17");
        var relation = new WorldRelation(task.StableId, inventory.StableId, WorldRelationKind.Targets);
        var error = Assert.Throws<WorldGraphContractException>(() =>
            new WorldGraphIndex<TestNode>(new[] { task, inventory }, new[] { relation, relation }));

        Assert.Equal("DuplicateWorldRelation", error.ErrorCode);
    }

    private static TestNode Node(string id) => new(new WorldStableId(id));

    private sealed class TestNode : IWorldNode
    {
        public TestNode(WorldStableId stableId) => StableId = stableId;
        public WorldStableId StableId { get; }
    }
}
