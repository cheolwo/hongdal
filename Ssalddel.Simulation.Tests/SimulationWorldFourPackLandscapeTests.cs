using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldFourPackLandscapeTests
{
    [Fact]
    public void 평창세영역은_Nature기반v2그래픽Profile을만든다()
    {
        var planner = new SimulationWorld기본Synty경관Planner();
        var request = new SimulationWorldSynty경관Job요청
        {
            TargetPlatformCode = SimulationWorldSynty대상플랫폼Codes.PC,
            QualityTierCode = "PC-High",
        };
        var targets = new[]
        {
            Area("area:daegwallyeong", "5176038000"),
            Area("area:jinbu", "5176036000"),
            Area("area:pyeongchang", "5176025000"),
        };

        var result = planner.계획(
            request,
            new SimulationWorld공간실행Snapshot { UnityArtifactCount = 1 },
            targets);

        Assert.Equal(SimulationWorldSynty작업상태Codes.일부완료, result.StatusCode);
        Assert.Equal(3, result.GraphicsPlans.Count);
        Assert.All(result.GraphicsPlans, plan =>
        {
            Assert.EndsWith(":v2", plan.StableId, StringComparison.Ordinal);
            Assert.EndsWith(".v2", plan.TextureSetKey, StringComparison.Ordinal);
            Assert.EndsWith(".v2", plan.MaterialVariantKey, StringComparison.Ordinal);
            Assert.Contains("nature", plan.ColorPaletteKey, StringComparison.Ordinal);
            Assert.Contains("nature", plan.BackgroundProfileKey, StringComparison.Ordinal);
            Assert.True(plan.PresentationOnly);
            Assert.DoesNotContain("/", plan.ColorPaletteKey, StringComparison.Ordinal);
            Assert.DoesNotContain("\\", plan.ColorPaletteKey, StringComparison.Ordinal);
        });
        Assert.Empty(result.VisualPlacements);
        Assert.Equal(3, result.Rejections.Count);
    }

    private static SimulationWorld파생Node Area(string stableId, string regionCode) => new()
    {
        StableId = stableId,
        NodeKindCode = "Area",
        RegionCode = regionCode,
    };
}
