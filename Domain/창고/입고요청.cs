using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.창고;

[Table("입고요청")]
public class 입고요청
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("창고_id")]
    public long 창고Id { get; set; }

    [Column("주문자_user_id")]
    [MaxLength(450)]
    public string 주문자UserId { get; set; } = string.Empty;

    [Column("공급처명")]
    [MaxLength(200)]
    public string 공급처명 { get; set; } = string.Empty;

    [Column("원주문참조번호")]
    [MaxLength(100)]
    public string 원주문참조번호 { get; set; } = string.Empty;

    [Column("상태")]
    [MaxLength(50)]
    public string 상태 { get; set; } = "입고예정";

    [Column("예정도착일")]
    public DateTime? 예정도착일 { get; set; }

    [Column("비고")]
    [MaxLength(1000)]
    public string 비고 { get; set; } = string.Empty;

    [Column("입고완료일시")]
    public DateTime? 입고완료일시 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
