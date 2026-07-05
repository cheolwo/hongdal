using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.판매;

[Table("상품식별코드맵")]
public class 상품식별코드맵
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("코드값")]
    [MaxLength(300)]
    public string 코드값 { get; set; } = string.Empty;

    [Column("코드유형")]
    public 상품식별코드유형 코드유형 { get; set; } = 상품식별코드유형.QR;

    [Column("상품_id")]
    public long 상품Id { get; set; }

    [Column("활성여부")]
    public bool 활성여부 { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum 상품식별코드유형
{
    Barcode = 1,
    QR = 2
}
