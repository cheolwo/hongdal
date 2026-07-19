using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.통관;

[Table("통관절차")]
public class 통관절차
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("주문_id")]
    public long? 주문Id { get; set; }

    [Column("주문참조번호")]
    [MaxLength(100)]
    public string 주문참조번호 { get; set; } = string.Empty;

    [Column("출고예정_id")]
    public long? 출고예정Id { get; set; }

    [Column("입고요청_id")]
    public long? 입고요청Id { get; set; }

    [Column("출고창고_id")]
    public long 출고창고Id { get; set; }

    [Column("입고창고_id")]
    public long 입고창고Id { get; set; }

    [Column("물류거래방향")]
    public 물류거래방향 물류거래방향 { get; set; }

    [Column("대표상품명")]
    [MaxLength(200)]
    public string? 대표상품명 { get; set; }

    [Column("상태")]
    public 통관절차상태 상태 { get; set; } = 통관절차상태.관세사검토대기;

    [Column("확정관세사_참여자_id")]
    [MaxLength(450)]
    public string? 확정관세사참여자Id { get; set; }

    [Column("메모")]
    [MaxLength(1000)]
    public string? 메모 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
