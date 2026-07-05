using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.통관;

[Table("관세사프로필")]
public class 관세사프로필
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("참여자_id")]
    [MaxLength(450)]
    public string 참여자Id { get; set; } = string.Empty;

    [Column("사무소명")]
    [MaxLength(200)]
    public string 사무소명 { get; set; } = string.Empty;

    [Column("관세사등록번호")]
    [MaxLength(100)]
    public string? 관세사등록번호 { get; set; }

    [Column("담당지역")]
    [MaxLength(200)]
    public string? 담당지역 { get; set; }

    [Column("전문품목메모")]
    [MaxLength(1000)]
    public string? 전문품목메모 { get; set; }

    [Column("수입전문여부")]
    public bool 수입전문여부 { get; set; } = true;

    [Column("수출전문여부")]
    public bool 수출전문여부 { get; set; } = true;

    [Column("수임가능여부")]
    public bool 수임가능여부 { get; set; } = true;

    [Column("관리자승인여부")]
    public bool 관리자승인여부 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
