using Microsoft.Extensions.Options;

namespace 홍달.Services.Options;

public enum HongdalExecutionMode
{
    Simulation,
    Operational
}

public sealed class HongdalExecutionOptions
{
    public const string SectionName = "HongdalExecution";

    public HongdalExecutionMode Mode { get; set; } = HongdalExecutionMode.Simulation;
}

public interface IHongdalExecutionModePolicy
{
    HongdalExecutionMode Mode { get; }
    bool IsSimulation { get; }
    bool IsOperational { get; }
}

public sealed class HongdalExecutionModePolicy : IHongdalExecutionModePolicy
{
    public HongdalExecutionModePolicy(IOptions<HongdalExecutionOptions> options)
    {
        Mode = options.Value.Mode;
    }

    public HongdalExecutionMode Mode { get; }
    public bool IsSimulation => Mode == HongdalExecutionMode.Simulation;
    public bool IsOperational => Mode == HongdalExecutionMode.Operational;
}
