namespace Ssalddel.Domain.TraditionalMarkets;

public sealed class TraditionalMarketSyncRun
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "Running";
    public string SourceDatasetKey { get; set; } = string.Empty;
    public DateOnly SourceReferenceDate { get; set; }
    public int FetchedCount { get; set; }
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int UnchangedCount { get; set; }
    public int DeactivatedCount { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}
