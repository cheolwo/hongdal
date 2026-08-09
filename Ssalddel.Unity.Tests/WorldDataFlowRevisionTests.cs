using Ssalddel.Unity.Data;

namespace Ssalddel.Tests.UnityData;

public sealed class WorldDataFlowRevisionTests
{
    [Fact]
    public void InterpretationRevision은_입력순서와무관하고_Rule변경을추적한다()
    {
        var population = new DataRevisionReference(
            "region-population:jungnang",
            "population-2026-08",
            DateTimeOffset.Parse("2026-08-01T00:00:00Z"));
        var orders = new DataRevisionReference(
            "regional-orders:jungnang",
            "orders-2026-08",
            DateTimeOffset.Parse("2026-08-08T00:00:00Z"));

        var forward = new DataRevisionSet(new[] { population, orders });
        var reverse = new DataRevisionSet(new[] { orders, population });

        var first = WorldDataFlowRevisionCalculator.CalculateInterpretation(
            forward, "regional-demand-interpreter-v1", "demand-rule-v1");
        var same = WorldDataFlowRevisionCalculator.CalculateInterpretation(
            reverse, "regional-demand-interpreter-v1", "demand-rule-v1");
        var changed = WorldDataFlowRevisionCalculator.CalculateInterpretation(
            forward, "regional-demand-interpreter-v1", "demand-rule-v2");

        Assert.Equal(first, same);
        Assert.NotEqual(first, changed);
    }

    [Fact]
    public void PresentationRevision은_Interpretation을보존하고_표현관점변경을추적한다()
    {
        const string interpretation = "interpretation:warehouse-v1";

        var manager = WorldDataFlowRevisionCalculator.CalculatePresentation(
            interpretation, "WarehouseManager", "warehouse-visual-v1", "warehouse-presentation-v1");
        var observer = WorldDataFlowRevisionCalculator.CalculatePresentation(
            interpretation, "PublicObserver", "warehouse-visual-v1", "warehouse-presentation-v1");

        Assert.NotEqual(manager, observer);
        var reference = new PresentationRevisionReference(
            interpretation,
            "WarehouseManager",
            "warehouse-visual-v1",
            "warehouse-presentation-v1",
            manager);
        Assert.Equal(interpretation, reference.InterpretationRevision);
    }

    [Fact]
    public void DataRevisionSet은_같은Source의중복Revision을거부한다()
    {
        var error = Assert.Throws<InvalidOperationException>(() => new DataRevisionSet(new[]
        {
            new DataRevisionReference("warehouse-zone:7", "revision-1"),
            new DataRevisionReference("warehouse-zone:7", "revision-2"),
        }));

        Assert.Equal("DuplicateDataRevisionSource:warehouse-zone:7", error.Message);
    }
}
