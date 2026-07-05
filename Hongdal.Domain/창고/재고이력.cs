using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.창고;

[Table("재고이력")]
public class 재고이력
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("입고상품_id")]
    public long 입고상품Id { get; set; }

    [Column("이력유형")]
    [MaxLength(50)]
    public string 이력유형 { get; set; } = string.Empty;

    [Column("변경수량")]
    public int 변경수량 { get; set; }

    [Column("변경후수량")]
    public int 변경후수량 { get; set; }

    [Column("원인유형")]
    [MaxLength(50)]
    public string 원인유형 { get; set; } = string.Empty;

    [Column("원인_id")]
    public long? 원인Id { get; set; }

    [Column("처리_user_id")]
    [MaxLength(450)]
    public string 처리UserId { get; set; } = string.Empty;

    [Column("메모")]
    [MaxLength(500)]
    public string 메모 { get; set; } = string.Empty;

    [Column("처리일시")]
    public DateTime 처리일시 { get; set; } = DateTime.UtcNow;
}
