using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.설정;

[Table("사용자_View_설정")]
public class 사용자View설정
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Column("app_key")]
    [MaxLength(100)]
    public string AppKey { get; set; } = string.Empty;

    [Column("view_key")]
    [MaxLength(200)]
    public string ViewKey { get; set; } = string.Empty;

    [Column("is_visible")]
    public bool IsVisible { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
