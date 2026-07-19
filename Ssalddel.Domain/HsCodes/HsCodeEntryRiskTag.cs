namespace Ssalddel.Domain.HsCodes;

public sealed class HsCodeEntryRiskTag
{
    public long Id { get; set; }

    public long HsCodeEntryId { get; set; }

    public HsCodeEntry? HsCodeEntry { get; set; }

    public HsCodeRiskTagType TagType { get; set; }

    public string Label { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public HsCodeRiskTagSource Source { get; set; } = HsCodeRiskTagSource.SystemRule;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
