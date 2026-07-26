using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssalddel.Domain.Content;

[Table("regional_culture_public_institution_sources")]
public sealed class 지역문화공공기관Source
{
    [Key]
    [Column("source_key")]
    [MaxLength(100)]
    public string SourceKey { get; set; } = string.Empty;

    [Column("country_code")]
    [MaxLength(2)]
    public string CountryCode { get; set; } = string.Empty;

    [Column("jurisdiction_level_code")]
    [MaxLength(40)]
    public string JurisdictionLevelCode { get; set; } = string.Empty;

    [Column("source_kind_code")]
    [MaxLength(40)]
    public string SourceKindCode { get; set; } = string.Empty;

    [Column("institution_name_ko")]
    [MaxLength(200)]
    public string InstitutionNameKo { get; set; } = string.Empty;

    [Column("institution_name_en")]
    [MaxLength(200)]
    public string InstitutionNameEn { get; set; } = string.Empty;

    [Column("supervising_institution_name_ko")]
    [MaxLength(200)]
    public string SupervisingInstitutionNameKo { get; set; } = string.Empty;

    [Column("responsibility_summary_ko", TypeName = "varchar(2000)")]
    public string ResponsibilitySummaryKo { get; set; } = string.Empty;

    [Column("region_key_pattern")]
    [MaxLength(120)]
    public string RegionKeyPattern { get; set; } = string.Empty;

    [Column("geographic_identifier_scheme")]
    [MaxLength(80)]
    public string GeographicIdentifierScheme { get; set; } = string.Empty;

    [Column("official_page_url", TypeName = "varchar(1000)")]
    public string OfficialPageUrl { get; set; } = string.Empty;

    [Column("data_url", TypeName = "varchar(1000)")]
    public string DataUrl { get; set; } = string.Empty;

    [Column("data_format_code")]
    [MaxLength(40)]
    public string DataFormatCode { get; set; } = string.Empty;

    [Column("is_machine_readable")]
    public bool IsMachineReadable { get; set; }

    [Column("refresh_cycle_code")]
    [MaxLength(40)]
    public string RefreshCycleCode { get; set; } = string.Empty;

    [Column("requires_regional_verification")]
    public bool RequiresRegionalVerification { get; set; } = true;

    [Column("limitations_ko", TypeName = "varchar(2000)")]
    public string LimitationsKo { get; set; } = string.Empty;

    [Column("evidence_checked_at_utc")]
    public DateTime EvidenceCheckedAtUtc { get; set; }

    [Column("source_version")]
    public int SourceVersion { get; set; } = 1;

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }
}
