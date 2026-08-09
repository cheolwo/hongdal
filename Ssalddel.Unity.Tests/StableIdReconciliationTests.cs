using Ssalddel.Unity.PresentationContracts.Reconciliation;

namespace Ssalddel.Tests.UnityData;

public sealed class StableIdReconciliationTests
{
    [Fact]
    public void StableId기준_변경집합은_입력순서와무관하게_결정적이다()
    {
        var currentKeep = Item("object:keep", 1, "presentation:keep");
        var reconciler = Reconciler();

        var changes = reconciler.Reconcile(
            new[]
            {
                Item("object:remove", 1, "presentation:remove"),
                Item("object:update", 1, "presentation:update-v1"),
                currentKeep,
            },
            new[]
            {
                Item("object:update", 2, "presentation:update-v2"),
                Item("object:keep", 2, "presentation:keep"),
                Item("object:add", 1, "presentation:add"),
            });

        Assert.Equal("object:add", Assert.Single(changes.Added).StableId);
        Assert.Equal("object:update", Assert.Single(changes.Updated).StableId);
        Assert.Equal("object:remove", Assert.Single(changes.Removed).StableId);
        Assert.Same(currentKeep, Assert.Single(changes.Unchanged));
    }

    [Fact]
    public void DataRevision이달라도_PresentationRevision이같으면_View대상을유지한다()
    {
        var current = Item("object:one", 1, "presentation:same");
        var incoming = Item("object:one", 2, "presentation:same");

        var changes = Reconciler().Reconcile(new[] { current }, new[] { incoming });

        Assert.Empty(changes.Updated);
        Assert.Same(current, Assert.Single(changes.Unchanged));
    }

    [Fact]
    public void 낮은DataRevision과_중복StableId를_거부한다()
    {
        var reconciler = Reconciler();

        var lower = Assert.Throws<StableIdReconciliationException>(() =>
            reconciler.Reconcile(
                new[] { Item("object:one", 2, "presentation:v2") },
                new[] { Item("object:one", 1, "presentation:v1") }));
        Assert.Equal("LowerDataRevision", lower.ErrorCode);
        Assert.Equal("object:one", lower.StableId);

        var duplicate = Assert.Throws<StableIdReconciliationException>(() =>
            reconciler.Reconcile(
                Array.Empty<ReconcileItem>(),
                new[]
                {
                    Item("object:same", 1, "presentation:v1"),
                    Item("object:same", 2, "presentation:v2"),
                }));
        Assert.Equal("DuplicateStableId", duplicate.ErrorCode);
        Assert.Equal("incoming", duplicate.CollectionName);
    }

    private static StableIdReconciler<ReconcileItem> Reconciler()
        => new(new StableIdReconciliationPolicy<ReconcileItem>(
            item => item.StableId,
            presentationRevision: item => item.PresentationRevision,
            dataRevisionComparison: (incoming, current) => incoming.DataRevision.CompareTo(current.DataRevision)));

    private static ReconcileItem Item(
        string stableId,
        long dataRevision,
        string presentationRevision)
        => new()
        {
            StableId = stableId,
            DataRevision = dataRevision,
            PresentationRevision = presentationRevision,
        };

    private sealed class ReconcileItem
    {
        public string StableId { get; init; } = string.Empty;
        public long DataRevision { get; init; }
        public string PresentationRevision { get; init; } = string.Empty;
    }
}
