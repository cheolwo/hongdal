using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Execution;

public sealed class SsalddelExecutionModePolicyTests
{
    [Fact]
    public void 기본_실행_모드는_Simulation이다()
    {
        var options = new SsalddelExecutionOptions();
        var policy = new SsalddelExecutionModePolicy(Options.Create(options));

        Assert.Equal(SsalddelExecutionMode.Simulation, policy.Mode);
        Assert.True(policy.IsSimulation);
        Assert.False(policy.IsOperational);
    }

    [Fact]
    public void Operational_설정을_운영_모드로_해석한다()
    {
        var options = new SsalddelExecutionOptions
        {
            Mode = SsalddelExecutionMode.Operational
        };
        var policy = new SsalddelExecutionModePolicy(Options.Create(options));

        Assert.Equal(SsalddelExecutionMode.Operational, policy.Mode);
        Assert.False(policy.IsSimulation);
        Assert.True(policy.IsOperational);
    }
}
