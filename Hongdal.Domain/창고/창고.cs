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

    [Column("소유자유형")]
    [MaxLength(50)]
    public string 소유자유형 { get; set; } = 창고소유자유형.주문자;

    [Column("창고유형")]
    [MaxLength(50)]
    public string 창고유형 { get; set; } = 홍달.도메인.창고.창고유형.가상창고;

    [Column("물류대행지분류")]
    [MaxLength(50)]
    public string 물류대행지분류 { get; set; } = "DeliveryAgency";

    [Column("창고명")]
    [MaxLength(200)]
    public string 창고명 { get; set; } = string.Empty;

    [Column("사업자번호")]
    [MaxLength(50)]
    public string 사업자번호 { get; set; } = string.Empty;

    [Column("주소")]
    [MaxLength(500)]
    public string 주소 { get; set; } = string.Empty;

    [Column("국가코드")]
    [MaxLength(10)]
    public string 국가코드 { get; set; } = "KR";

    [Column("담당자명")]
    [MaxLength(100)]
    public string 담당자명 { get; set; } = string.Empty;

    [Column("연락처")]
    [MaxLength(50)]
    public string 연락처 { get; set; } = string.Empty;

    [Column("위도", TypeName = "decimal(10,7)")]
    public decimal? 위도 { get; set; }

    [Column("경도", TypeName = "decimal(10,7)")]
    public decimal? 경도 { get; set; }

    [Column("기본창고여부")]
    public bool 기본창고여부 { get; set; } = true;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
