using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssalddel.Domain.Content;

[Table("regional_culture_image_prompts")]
public sealed class 지역문화이미지Prompt
{
    [Key]
    [Column("region_key")]
    [MaxLength(80)]
    public string RegionKey { get; set; } = string.Empty;

    [Column("country_code")]
    [MaxLength(2)]
    public string CountryCode { get; set; } = string.Empty;

    [Column("subdivision_code")]
    [MaxLength(16)]
    public string SubdivisionCode { get; set; } = string.Empty;

    [Column("region_name_ko")]
    [MaxLength(120)]
    public string RegionNameKo { get; set; } = string.Empty;

    [Column("region_name_en")]
    [MaxLength(120)]
    public string RegionNameEn { get; set; } = string.Empty;

    [Column("region_name_local")]
    [MaxLength(120)]
    public string RegionNameLocal { get; set; } = string.Empty;

    [Column("region_type_code")]
    [MaxLength(40)]
    public string RegionTypeCode { get; set; } = string.Empty;

    [Column("geography_summary_ko", TypeName = "varchar(1000)")]
    public string GeographySummaryKo { get; set; } = string.Empty;

    [Column("culture_summary_ko", TypeName = "varchar(2000)")]
    public string CultureSummaryKo { get; set; } = string.Empty;

    [Column("visual_anchors_json", TypeName = "longtext")]
    public string VisualAnchorsJson { get; set; } = "[]";

    [Column("avoid_expressions_json", TypeName = "longtext")]
    public string AvoidExpressionsJson { get; set; } = "[]";

    [Column("prompt_ko", TypeName = "longtext")]
    public string PromptKo { get; set; } = string.Empty;

    [Column("aspect_ratio")]
    [MaxLength(20)]
    public string AspectRatio { get; set; } = "16:9";

    [Column("safe_crop")]
    [MaxLength(40)]
    public string SafeCrop { get; set; } = "center-4:3";

    [Column("review_status_code")]
    [MaxLength(40)]
    public string ReviewStatusCode { get; set; } = 지역문화이미지Prompt검토상태Codes.ResearchDraft;

    [Column("requires_evidence_review")]
    public bool RequiresEvidenceReview { get; set; } = true;

    [Column("evidence_notes_ko", TypeName = "longtext")]
    public string EvidenceNotesKo { get; set; } = string.Empty;

    [Column("prompt_version")]
    public int PromptVersion { get; set; } = 1;

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }
}

public static class 지역문화이미지Prompt검토상태Codes
{
    public const string ResearchDraft = "ResearchDraft";
    public const string EvidenceReviewed = "EvidenceReviewed";
    public const string ApprovedForGeneration = "ApprovedForGeneration";
    public const string Retired = "Retired";
}

public static class 지역문화행정구역유형Codes
{
    public const string KoreaSpecialCity = "KoreaSpecialCity";
    public const string KoreaMetropolitanCity = "KoreaMetropolitanCity";
    public const string KoreaSpecialSelfGoverningCity = "KoreaSpecialSelfGoverningCity";
    public const string KoreaProvince = "KoreaProvince";
    public const string KoreaSpecialSelfGoverningProvince = "KoreaSpecialSelfGoverningProvince";
    public const string UnitedStatesState = "UnitedStatesState";
    public const string ChinaProvince = "ChinaProvince";
    public const string ChinaAutonomousRegion = "ChinaAutonomousRegion";
    public const string ChinaMunicipality = "ChinaMunicipality";
}
