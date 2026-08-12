using System.Reflection;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;
using Ssalddel.Simulation.Infrastructure;

namespace Ssalddel.Simulation.Tests;

public sealed class SimulationLayerBoundaryTests
{
    [Fact]
    public void Domain은_Application과_Infrastructure를_참조하지_않는다()
    {
        var references = ReferenceNames(typeof(SimulationSessionReplay).Assembly);

        Assert.DoesNotContain("Ssalddel.Simulation.Application", references);
        Assert.DoesNotContain("Ssalddel.Simulation.Infrastructure", references);
        Assert.DoesNotContain("Ssalddel.Simulation.Server", references);
    }

    [Fact]
    public void Application은_Domain을_참조하되_Infrastructure와_Server를_참조하지_않는다()
    {
        var references = ReferenceNames(typeof(경영SimulationSessionService).Assembly);

        Assert.Contains("Ssalddel.Simulation.Domain", references);
        Assert.DoesNotContain("Ssalddel.Simulation.Infrastructure", references);
        Assert.DoesNotContain("Ssalddel.Simulation.Server", references);
    }

    [Fact]
    public void Infrastructure는_Application의_저장계약을_구현한다()
    {
        Assert.IsAssignableFrom<I경영SimulationSessionStore>(
            new InMemory경영SimulationSessionStore());
        Assert.IsAssignableFrom<ISimulationSessionSaveStore>(
            new InMemorySimulationSessionSaveStore());
    }

    private static IReadOnlySet<string> ReferenceNames(Assembly assembly)
        => assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToHashSet(StringComparer.Ordinal);
}
