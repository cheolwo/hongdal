using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.운송;

[Table("운송의뢰상품연결")]
public class 운송의뢰상품연결
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("운송의뢰_id")]
    [MaxLength(100)]
    public string 운송의뢰Id { get; set; } = string.Empty;

    [Column("입고상품_id")]
    public long 입고상품Id { get; set; }

    [Column("할당수량")]
    public int 할당수량 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
