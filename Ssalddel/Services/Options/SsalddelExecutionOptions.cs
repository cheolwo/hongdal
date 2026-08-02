using Microsoft.Extensions.Options;

namespace 살뜰.Services.Options;

public enum SsalddelExecutionMode
{
    Simulation,
    Operational
}

public sealed class SsalddelExecutionOptions
{
    public const string SectionName = "SsalddelExecution";

    public SsalddelExecutionMode Mode { get; set; } = SsalddelExecutionMode.Operational;

    public bool DevelopmentReadOnly { get; set; }
}

public interface ISsalddelExecutionModePolicy
{
    SsalddelExecutionMode Mode { get; }
    bool IsSimulation { get; }
    bool IsOperational { get; }
}

public sealed class SsalddelExecutionModePolicy : ISsalddelExecutionModePolicy
{
    public SsalddelExecutionModePolicy(IOptions<SsalddelExecutionOptions> options)
    {
        Mode = options.Value.Mode;
    }

    public SsalddelExecutionMode Mode { get; }
    public bool IsSimulation => Mode == SsalddelExecutionMode.Simulation;
    public bool IsOperational => Mode == SsalddelExecutionMode.Operational;
}
