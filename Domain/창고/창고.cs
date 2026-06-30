using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.창고;

[Table("창고")]
public class 창고
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("소유자_user_id")]
    [MaxLength(450)]
    public string 소유자UserId { get; set; } = string.Empty;

    [Column("창고명")]
    [MaxLength(200)]
    public string 창고명 { get; set; } = string.Empty;

    [Column("사업자번호")]
    [MaxLength(50)]
    public string 사업자번호 { get; set; } = string.Empty;

    [Column("주소")]
    [MaxLength(500)]
    public string 주소 { get; set; } = string.Empty;

    [Column("담당자명")]
    [MaxLength(100)]
    public string 담당자명 { get; set; } = string.Empty;

    [Column("연락처")]
    [MaxLength(50)]
    public string 연락처 { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
