namespace 살뜰.Services.Options;

public sealed class GroupPurchaseDemandOsOptions
{
    public const string SectionName = "GroupPurchaseDemandOS";

    public bool Enabled { get; set; } = true;

    public int ScanIntervalSeconds { get; set; } = 60;

    public int BatchSize { get; set; } = 100;

    public int AgingReviewHours { get; set; } = 24;
}
