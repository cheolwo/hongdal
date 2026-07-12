using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.마트;

[Table("마트주문상품")]
public class 마트주문상품
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("마트주문_id")]
    public long 마트주문Id { get; set; }

    [Column("출고예정_id")]
    public long? 출고예정Id { get; set; }

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
    public string 상태 { get; set; } = "출고 예정";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public 마트주문? 마트주문 { get; set; }
}
