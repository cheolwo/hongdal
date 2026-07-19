using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.창고;

[Table("재고이동")]
public class 재고이동
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("창고_id")]
    public long 창고Id { get; set; }

    [Column("입고상품_id")]
    public long? 입고상품Id { get; set; }

    [Column("판매상품_id")]
    public long? 판매상품Id { get; set; }

    [Column("상품명")]
    [MaxLength(200)]
    public string 상품명 { get; set; } = string.Empty;

    [Column("sku")]
    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;

    [Column("이동유형")]
    [MaxLength(50)]
    public string 이동유형 { get; set; } = string.Empty;

    [Column("수량")]
    public int 수량 { get; set; }

    [Column("주문_id")]
    public long? 주문Id { get; set; }

    [Column("주문참조번호")]
    [MaxLength(100)]
    public string? 주문참조번호 { get; set; }

    [Column("출고예정_id")]
    public long? 출고예정Id { get; set; }

    [Column("입고요청_id")]
    public long? 입고요청Id { get; set; }

    [Column("운송의뢰_id")]
    [MaxLength(100)]
    public string? 운송의뢰Id { get; set; }

    [Column("처리_user_id")]
    [MaxLength(450)]
    public string 처리UserId { get; set; } = string.Empty;

    [Column("메모")]
    [MaxLength(500)]
    public string 메모 { get; set; } = string.Empty;

    [Column("발생일시")]
    public DateTime 발생일시 { get; set; } = DateTime.UtcNow;
}
