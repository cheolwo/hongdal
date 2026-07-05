using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.설정;

[Table("사용자_Command_기능설정")]
public class 사용자Command기능설정
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    [MaxLength(450)]
    public string 사용자Id { get; set; } = string.Empty;

    [Column("command_name")]
    [MaxLength(200)]
    public string CommandName { get; set; } = string.Empty;

    [Column("feature_name")]
    [MaxLength(100)]
    public string FeatureName { get; set; } = string.Empty;

    [Column("is_enabled")]
    public bool IsEnabled { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
