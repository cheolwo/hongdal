using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.사용자;

[Table("주문자프로필")]
public class 주문자프로필
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    [Column("표시명")]
    [MaxLength(100)]
    public string 표시명 { get; set; } = string.Empty;

    [Column("연락처")]
    [MaxLength(50)]
    public string 연락처 { get; set; } = string.Empty;

    [Column("기본주소")]
    [MaxLength(500)]
    public string 기본주소 { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
