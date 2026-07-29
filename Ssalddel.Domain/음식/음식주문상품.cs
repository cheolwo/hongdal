using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.음식;

[Table("음식주문상품")]
public class 음식주문상품
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("음식주문_id")]
    public long 음식주문Id { get; set; }

    [Column("메뉴_id")]
    public long? 메뉴Id { get; set; }

    [Column("상품명")]
    [MaxLength(200)]
    public string 상품명 { get; set; } = string.Empty;

    [Column("수량")]
    public int 수량 { get; set; }

    [Column("단가", TypeName = "decimal(18,2)")]
    public decimal 단가 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public 음식주문? 음식주문 { get; set; }
}
