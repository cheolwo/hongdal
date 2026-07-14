using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Tests.Services.Execution;

public sealed class HongdalExecutionModePolicyTests
{
    [Fact]
    public void 기본_실행_모드는_Simulation이다()
    {
        var options = new HongdalExecutionOptions();
        var policy = new HongdalExecutionModePolicy(Options.Create(options));

        Assert.Equal(HongdalExecutionMode.Simulation, policy.Mode);
        Assert.True(policy.IsSimulation);
        Assert.False(policy.IsOperational);
    }

    [Fact]
    public void Operational_설정을_운영_모드로_해석한다()
    {
        var options = new HongdalExecutionOptions
        {
            Mode = HongdalExecutionMode.Operational
        };
        var policy = new HongdalExecutionModePolicy(Options.Create(options));

        Assert.Equal(HongdalExecutionMode.Operational, policy.Mode);
        Assert.False(policy.IsSimulation);
        Assert.True(policy.IsOperational);
    }
}
