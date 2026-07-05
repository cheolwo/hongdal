namespace Hongdal.Domain.HsCodes;

public sealed class HsCodeClassificationCase
{
    public long Id { get; set; }

    public long? HsCodeEntryId { get; set; }

    public HsCodeEntry? HsCodeEntry { get; set; }

    public string HsCode { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string SourceReferenceNo { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public string IssuingAuthority { get; set; } = string.Empty;

    public DateTime? DecidedAt { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string GoodsDescription { get; set; } = string.Empty;

    public string DecisionReason { get; set; } = string.Empty;

    public bool IsPublicOfficialCase { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
