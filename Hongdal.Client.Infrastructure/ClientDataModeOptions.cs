namespace Hongdal.Client.Infrastructure;

public sealed class ClientDataModeOptions
{
    public const string SectionName = "ClientDataMode";

    public bool AllowSampleFallback { get; set; }

    public bool AllowDevelopmentSnapshotFallback { get; set; }

    public bool RequireServerLedgerForV1Smoke { get; set; }

    public bool CanUseSampleFallback => AllowSampleFallback && !RequireServerLedgerForV1Smoke;

    public bool CanUseDevelopmentSnapshotFallback => AllowDevelopmentSnapshotFallback && !RequireServerLedgerForV1Smoke;
}
