using System.Reflection;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Server.Controllers;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationWorldGameplayBoundaryTests
{
    private static readonly string[] WorldGameplayMethods =
    {
        "GetNatureMindState",
        "GetNatureFarmInterpretation",
        "GetPlayerAreaAccess",
        "PreviewAreaTraversal",
        "ConfirmAreaTraversal",
        "GetHostedWorldState",
        "PreviewOpenHostedWorld",
        "ConfirmOpenHostedWorld",
        "PreviewJoinHostedWorld",
        "ConfirmJoinHostedWorld",
        "PreviewHostedGuestAction",
        "ConfirmHostedGuestAction",
        "GetCoopConstructionState",
        "PreviewCoopContribution",
        "ConfirmCoopContribution",
        "PreviewCoopDemolition",
        "ConfirmCoopDemolition",
        "PreviewCoopRestore",
        "ConfirmCoopRestore",
        "GetGameplayObservability",
    };

    [Fact]
    public void 세계게임플레이는_세션생명주기와_분리된ApplicationService가_소유한다()
    {
        var lifecycle = DeclaredPublicMethods(
            typeof(경영SimulationSession생명주기Service));
        var gameplay = DeclaredPublicMethods(
            typeof(경영SimulationWorldGameplayService));
        var compatibilityFacade = DeclaredPublicMethods(
            typeof(경영SimulationSessionService));

        foreach (var method in WorldGameplayMethods)
        {
            Assert.DoesNotContain(method, lifecycle);
            Assert.Contains(method, gameplay);
            Assert.Contains(method, compatibilityFacade);
        }
    }

    [Fact]
    public void 세계게임플레이Http는_전용Controller가_소유하고_기존Facade는_호환만_유지한다()
    {
        var lifecycleApi = DeclaredPublicMethods(
            typeof(경영SimulationSessionsController));
        var gameplayApi = DeclaredPublicMethods(
            typeof(경영SimulationWorldGameplayController));

        Assert.DoesNotContain("GetHostedWorld", lifecycleApi);
        Assert.DoesNotContain("PreviewCoopContribution", lifecycleApi);
        Assert.Contains("GetHostedWorld", gameplayApi);
        Assert.Contains("PreviewCoopContribution", gameplayApi);
        Assert.True(typeof(SimulationApiControllerBase)
            .IsAssignableFrom(typeof(경영SimulationSessionsController)));
        Assert.True(typeof(SimulationApiControllerBase)
            .IsAssignableFrom(typeof(경영SimulationWorldGameplayController)));
    }

    private static string[] DeclaredPublicMethods(Type type)
        => type.GetMethods(BindingFlags.Instance | BindingFlags.Public
                           | BindingFlags.DeclaredOnly)
            .Select(method => method.Name)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
}
