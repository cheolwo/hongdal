namespace 살뜰.Services.Options;

public sealed class GroupImportReadinessOsOptions
{
    public const string SectionName = "GroupImportReadinessOS";

    public bool Enabled { get; set; } = true;

    public int ScanIntervalSeconds { get; set; } = 300;

    public int BatchSize { get; set; } = 100;

    public int EvidenceFreshnessDays { get; set; } = 30;

    public int MaxCommandHistory { get; set; } = 50;
}
