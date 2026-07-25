using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Execution;

public sealed class SsalddelExecutionModePolicyTests
{
    [Fact]
    public void 기본_실행_모드는_Operational이다()
    {
        var options = new SsalddelExecutionOptions();
        var policy = new SsalddelExecutionModePolicy(Options.Create(options));

        Assert.Equal(SsalddelExecutionMode.Operational, policy.Mode);
        Assert.True(policy.IsOperational);
        Assert.False(policy.IsSimulation);
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
