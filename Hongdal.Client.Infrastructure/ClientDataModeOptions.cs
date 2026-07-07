namespace Hongdal.Client.Infrastructure;

public sealed class ClientDataModeOptions
{
    public const string SectionName = "ClientDataMode";

    public bool AllowSampleFallback { get; set; }

    public bool AllowDevelopmentSnapshotFallback { get; set; }
}
