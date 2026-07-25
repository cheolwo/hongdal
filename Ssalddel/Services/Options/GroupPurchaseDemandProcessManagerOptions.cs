namespace 살뜰.Services.Options;

public sealed class GroupPurchaseDemandProcessManagerOptions
{
    // 배포 설정 호환을 위해 기존 section key는 유지한다.
    public const string SectionName = "GroupPurchaseDemandOS";

    public bool Enabled { get; set; } = true;

    public int ScanIntervalSeconds { get; set; } = 60;

    public int BatchSize { get; set; } = 100;

    public int AgingReviewHours { get; set; } = 24;
}
