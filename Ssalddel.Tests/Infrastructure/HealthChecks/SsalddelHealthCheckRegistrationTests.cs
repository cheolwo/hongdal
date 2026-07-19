using Ssalddel.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Infrastructure.HealthChecks;

public sealed class SsalddelHealthCheckRegistrationTests
{
    [Fact]
    public void AddSsalddelHealthChecks_RegistersSeparateLivenessAndReadinessChecks()
    {
        var services = new ServiceCollection();

        services.AddSsalddelHealthChecks();

        using var provider = services.BuildServiceProvider();
        var registrations = provider
            .GetRequiredService<IOptions<HealthCheckServiceOptions>>()
            .Value
            .Registrations
            .ToDictionary(registration => registration.Name);

        Assert.Equal(["live"], registrations["self"].Tags);
        Assert.Equal(["ready"], registrations["persistence"].Tags);
        Assert.Equal(HealthStatus.Unhealthy, registrations["persistence"].FailureStatus);
        Assert.Equal(TimeSpan.FromSeconds(15), registrations["persistence"].Timeout);
    }

    [Fact]
    public void DatabaseInitializationOptions_AreSafeForMultiInstanceProductionByDefault()
    {
        var options = new DatabaseInitializationOptions();

        Assert.False(options.RunAtStartup);
        Assert.True(options.FailOnError);
    }
}
