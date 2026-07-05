using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.창고;

[Table("창고사용자")]
public class 창고사용자
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("창고_id")]
    public long 창고Id { get; set; }

    [Column("user_id")]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Column("역할명")]
    [MaxLength(100)]
    public string 역할명 { get; set; } = string.Empty;

    [Column("is_primary")]
    public bool IsPrimary { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
