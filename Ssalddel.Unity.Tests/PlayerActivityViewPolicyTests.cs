using Ssalddel.Unity.PlayerActivities;

namespace Ssalddel.Unity.Tests;

public sealed class PlayerActivityViewPolicyTests
{
    [Fact]
    public void 농장경영은_전술3인칭을기본으로하고_1인칭전환을허용한다()
    {
        var catalog = PlayerActivityViewPolicyCatalog.CreateDefault();

        var defaults = catalog.Resolve(PlayerActivityCodes.FarmManagement);
        var firstPerson = catalog.Resolve(
            PlayerActivityCodes.FarmManagement,
            PlayerActivityViewModeCodes.FirstPerson);

        Assert.Equal(PlayerActivityViewModeCodes.TacticalThirdPerson,
            defaults.ViewModeCode);
        Assert.True(defaults.UsedActivityDefault);
        Assert.Contains(PlayerActivityViewCapabilityCodes.MultiTargetSelection,
            defaults.AdvantageCapabilityCodes);
        Assert.Contains(PlayerActivityViewCapabilityCodes.BatchWorkPlanning,
            defaults.AdvantageCapabilityCodes);
        Assert.Equal(PlayerActivityViewModeCodes.FirstPerson,
            firstPerson.ViewModeCode);
        Assert.True(firstPerson.ManualOverrideApplied);
        Assert.Empty(firstPerson.AdvantageCapabilityCodes);
        Assert.False(firstPerson.ChangesWorldState);
        Assert.True(firstPerson.PresentationOnly);
    }

    [Fact]
    public void 탐험은_1인칭을기본으로하고_시야기반로딩능력을표시한다()
    {
        var decision = PlayerActivityViewPolicyCatalog.CreateDefault()
            .Resolve(PlayerActivityCodes.Exploration);

        Assert.Equal(PlayerActivityViewModeCodes.FirstPerson,
            decision.ViewModeCode);
        Assert.Contains(PlayerActivityViewCapabilityCodes.DirectMovement,
            decision.AdvantageCapabilityCodes);
        Assert.Contains(PlayerActivityViewCapabilityCodes.VisibilityDrivenStreaming,
            decision.AdvantageCapabilityCodes);
        Assert.False(decision.ChangesWorldState);
        Assert.True(decision.PresentationOnly);
    }

    [Fact]
    public void 허용되지않은시점은_활동기본값으로위장하지않고거부한다()
    {
        var catalog = PlayerActivityViewPolicyCatalog.CreateDefault();

        var error = Assert.Throws<InvalidOperationException>(() => catalog.Resolve(
            PlayerActivityCodes.FarmManagement,
            PlayerActivityViewModeCodes.Strategy));

        Assert.Equal("PlayerActivityViewOverrideNotAllowed", error.Message);
    }

    [Fact]
    public void 전투는_일인칭을권장하지만_전술삼인칭수동전환도허용한다()
    {
        var catalog = PlayerActivityViewPolicyCatalog.CreateDefault();

        var defaults = catalog.Resolve(PlayerActivityCodes.Combat);
        var awareness = catalog.Resolve(PlayerActivityCodes.Combat,
            PlayerActivityViewModeCodes.TacticalThirdPerson);

        Assert.Equal(PlayerActivityViewModeCodes.FirstPerson,
            defaults.ViewModeCode);
        Assert.Contains(PlayerActivityViewCapabilityCodes.WiderReactionWindow,
            defaults.AdvantageCapabilityCodes);
        Assert.Equal(PlayerActivityViewModeCodes.TacticalThirdPerson,
            awareness.ViewModeCode);
        Assert.True(awareness.ManualOverrideApplied);
        Assert.True(awareness.PresentationOnly);
        Assert.False(awareness.ChangesWorldState);
    }
}
