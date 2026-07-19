using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Tests.Services.Community;

public sealed class PlatformDiagramNodeStackOrderTests
{
    [Fact]
    public void MoveToFrontAndBack_ChangesOnlyTheStackOrder()
    {
        var stackOrder = new PlatformDiagramNodeStackOrder();
        stackOrder.Synchronize(["입고", "운송", "검수"]);

        Assert.True(stackOrder.MoveToFront("입고"));
        Assert.Equal(["운송", "검수", "입고"], stackOrder.NodeTitles);
        Assert.True(stackOrder.MoveToBack("검수"));
        Assert.Equal(["검수", "운송", "입고"], stackOrder.NodeTitles);
    }

    [Fact]
    public void Synchronize_PreservesExistingLayersAndAppendsNewNodesToFront()
    {
        var stackOrder = new PlatformDiagramNodeStackOrder();
        stackOrder.Synchronize(["A", "B", "C"]);
        stackOrder.MoveToFront("A");

        stackOrder.Synchronize(["A", "C", "D"]);

        Assert.Equal(["C", "A", "D"], stackOrder.NodeTitles);
        Assert.True(stackOrder.CanMoveToBack("D"));
        Assert.False(stackOrder.CanMoveToFront("D"));
        Assert.Equal(0, stackOrder.GetLayerIndex("C"));
        Assert.Equal(2, stackOrder.GetLayerIndex("D"));
    }
}
