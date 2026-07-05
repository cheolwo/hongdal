using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.설정;

[Table("플랫폼_View_정책")]
public class 플랫폼View정책
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("app_key")]
    [MaxLength(100)]
    public string AppKey { get; set; } = string.Empty;

    [Column("view_key")]
    [MaxLength(200)]
    public string ViewKey { get; set; } = string.Empty;

    [Column("display_name")]
    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [Column("route")]
    [MaxLength(300)]
    public string Route { get; set; } = string.Empty;

    [Column("icon_key")]
    [MaxLength(200)]
    public string IconKey { get; set; } = string.Empty;

    [Column("role_name")]
    [MaxLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [Column("is_required")]
    public bool IsRequired { get; set; }

    [Column("policy_enabled")]
    public bool PolicyEnabled { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
