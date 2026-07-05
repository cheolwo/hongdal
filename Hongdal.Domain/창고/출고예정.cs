using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.창고;

[Table("출고예정")]
public class 출고예정
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("주문_id")]
    public long? 주문Id { get; set; }

    [Column("주문참조번호")]
    [MaxLength(100)]
    public string 주문참조번호 { get; set; } = string.Empty;

    [Column("판매상품_id")]
    public long? 판매상품Id { get; set; }

    [Column("입고상품_id")]
    public long? 입고상품Id { get; set; }

    [Column("판매자_user_id")]
    [MaxLength(450)]
    public string 판매자UserId { get; set; } = string.Empty;

    [Column("주문자_user_id")]
    [MaxLength(450)]
    public string 주문자UserId { get; set; } = string.Empty;

    [Column("출고창고_id")]
    public long 출고창고Id { get; set; }

    [Column("상품명")]
    [MaxLength(200)]
    public string 상품명 { get; set; } = string.Empty;

    [Column("sku")]
    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;

    [Column("수량")]
    public int 수량 { get; set; }

    [Column("상태")]
    [MaxLength(50)]
    public string 상태 { get; set; } = 출고상태.예정;

    [Column("운송의뢰_id")]
    [MaxLength(100)]
    public string? 운송의뢰Id { get; set; }

    [Column("입고요청_id")]
    public long? 입고요청Id { get; set; }

    [Column("출고처리일시")]
    public DateTime? 출고처리일시 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
