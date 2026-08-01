using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssalddel.Domain.Geography;

[Table("regional_agricultural_map_regions")]
public sealed class 지역농수산Map행정구역
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("public_region_key")]
    [MaxLength(80)]
    public string PublicRegionKey { get; set; } = string.Empty;

    [Column("country_code")]
    [MaxLength(2)]
    public string CountryCode { get; set; } = string.Empty;

    [Column("region_type_code")]
    [MaxLength(40)]
    public string RegionTypeCode { get; set; } = string.Empty;

    [Column("parent_region_id")]
    public Guid? ParentRegionId { get; set; }

    public 지역농수산Map행정구역? ParentRegion { get; set; }

    [Column("display_name_ko")]
    [MaxLength(200)]
    public string DisplayNameKo { get; set; } = string.Empty;

    [Column("display_name_en")]
    [MaxLength(200)]
    public string DisplayNameEn { get; set; } = string.Empty;

    [Column("display_name_local")]
    [MaxLength(200)]
    public string DisplayNameLocal { get; set; } = string.Empty;

    [Column("valid_from")]
    public DateOnly? ValidFrom { get; set; }

    [Column("valid_to")]
    public DateOnly? ValidTo { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }

    public ICollection<지역농수산Map행정구역CodeAssignment> CodeAssignments { get; set; } =
        new List<지역농수산Map행정구역CodeAssignment>();

    public ICollection<지역농수산Map행정구역Boundary> Boundaries { get; set; } =
        new List<지역농수산Map행정구역Boundary>();

    public ICollection<지역농수산Map지역Crosswalk> IncomingCrosswalks { get; set; } =
        new List<지역농수산Map지역Crosswalk>();

    public ICollection<지역농수산Map행정구역> ChildRegions { get; set; } =
        new List<지역농수산Map행정구역>();
}

[Table("regional_agricultural_map_region_codes")]
public sealed class 지역농수산Map행정구역CodeAssignment
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("region_id")]
    public Guid RegionId { get; set; }

    public 지역농수산Map행정구역 Region { get; set; } = null!;

    [Column("scheme_code")]
    [MaxLength(80)]
    public string SchemeCode { get; set; } = string.Empty;

    [Column("external_code")]
    [MaxLength(200)]
    public string ExternalCode { get; set; } = string.Empty;

    [Column("source_vintage")]
    [MaxLength(40)]
    public string SourceVintage { get; set; } = string.Empty;

    [Column("valid_from")]
    public DateOnly? ValidFrom { get; set; }

    [Column("valid_to")]
    public DateOnly? ValidTo { get; set; }

    [Column("source_url", TypeName = "varchar(1000)")]
    public string SourceUrl { get; set; } = string.Empty;

    [Column("verified_at_utc")]
    public DateTime VerifiedAtUtc { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }
}

[Table("regional_agricultural_map_region_boundaries")]
public sealed class 지역농수산Map행정구역Boundary
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("region_id")]
    public Guid RegionId { get; set; }

    public 지역농수산Map행정구역 Region { get; set; } = null!;

    [Column("boundary_source_code")]
    [MaxLength(80)]
    public string BoundarySourceCode { get; set; } = string.Empty;

    [Column("boundary_vintage")]
    [MaxLength(40)]
    public string BoundaryVintage { get; set; } = string.Empty;

    [Column("geometry_reference", TypeName = "varchar(1000)")]
    public string GeometryReference { get; set; } = string.Empty;

    [Column("anchor_latitude", TypeName = "decimal(10,7)")]
    public decimal AnchorLatitude { get; set; }

    [Column("anchor_longitude", TypeName = "decimal(10,7)")]
    public decimal AnchorLongitude { get; set; }

    [Column("simplification_level")]
    public int SimplificationLevel { get; set; }

    [Column("source_url", TypeName = "varchar(1000)")]
    public string SourceUrl { get; set; } = string.Empty;

    [Column("verified_at_utc")]
    public DateTime VerifiedAtUtc { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }
}

[Table("regional_agricultural_map_region_crosswalks")]
public sealed class 지역농수산Map지역Crosswalk
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("source_scheme_code")]
    [MaxLength(80)]
    public string SourceSchemeCode { get; set; } = string.Empty;

    [Column("source_code")]
    [MaxLength(200)]
    public string SourceCode { get; set; } = string.Empty;

    [Column("source_name_raw")]
    [MaxLength(300)]
    public string SourceNameRaw { get; set; } = string.Empty;

    [Column("source_vintage")]
    [MaxLength(40)]
    public string SourceVintage { get; set; } = string.Empty;

    [Column("target_region_id")]
    public Guid? TargetRegionId { get; set; }

    public 지역농수산Map행정구역? TargetRegion { get; set; }

    [Column("match_method_code")]
    [MaxLength(40)]
    public string MatchMethodCode { get; set; } = string.Empty;

    [Column("confidence_code")]
    [MaxLength(40)]
    public string ConfidenceCode { get; set; } = string.Empty;

    [Column("valid_from")]
    public DateOnly? ValidFrom { get; set; }

    [Column("valid_to")]
    public DateOnly? ValidTo { get; set; }

    [Column("reviewed_at_utc")]
    public DateTime? ReviewedAtUtc { get; set; }

    [Column("evidence_url", TypeName = "varchar(1000)")]
    public string EvidenceUrl { get; set; } = string.Empty;

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; }

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; }
}
