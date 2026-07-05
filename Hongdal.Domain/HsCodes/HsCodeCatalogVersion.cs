namespace Hongdal.Domain.HsCodes;

public sealed class HsCodeCatalogVersion
{
    public long Id { get; set; }

    public string StandardCode { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public int CodeDigits { get; set; }

    public string Revision { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public DateTime EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;

    public string Notes { get; set; } = string.Empty;

    public ICollection<HsCodeEntry> Entries { get; set; } = new List<HsCodeEntry>();
}
