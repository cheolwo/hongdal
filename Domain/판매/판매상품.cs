using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.판매;

[Table("판매상품")]
public class 판매상품
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("입고상품_id")]
    public long 입고상품Id { get; set; }

    [Column("소유자_user_id")]
    [MaxLength(450)]
    public string 소유자UserId { get; set; } = string.Empty;

    [Column("대표상품명")]
    [MaxLength(200)]
    public string 대표상품명 { get; set; } = string.Empty;

    [Column("판매sku")]
    [MaxLength(100)]
    public string 판매SKU { get; set; } = string.Empty;

    [Column("판매가", TypeName = "decimal(18,2)")]
    public decimal 판매가 { get; set; }

    [Column("상태")]
    [MaxLength(50)]
    public string 상태 { get; set; } = "준비";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
