using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.마트;

/// <summary>
/// 판매자·창고 내부 원장과 분리된 주문자 공개용 마트 상품·가용 수량 투영입니다.
/// </summary>
[Table("마트공개상품")]
public sealed class 마트공개상품
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("판매상품_id")]
    public long? 판매상품Id { get; set; }

    [Column("상품명")]
    [MaxLength(200)]
    public string 상품명 { get; set; } = string.Empty;

    [Column("카테고리")]
    [MaxLength(100)]
    public string 카테고리 { get; set; } = string.Empty;

    [Column("짧은설명")]
    [MaxLength(300)]
    public string 짧은설명 { get; set; } = string.Empty;

    [Column("설명")]
    [MaxLength(2000)]
    public string 설명 { get; set; } = string.Empty;

    [Column("판매단위")]
    [MaxLength(100)]
    public string 판매단위 { get; set; } = string.Empty;

    [Column("판매가", TypeName = "decimal(18,2)")]
    public decimal 판매가 { get; set; }

    [Column("대표이미지_url")]
    [MaxLength(1000)]
    public string? 대표이미지Url { get; set; }

    [Column("판매가능수량")]
    public int 판매가능수량 { get; set; }

    [Column("공개여부")]
    public bool 공개여부 { get; set; }

    [Column("판매허용여부")]
    public bool 판매허용여부 { get; set; }

    [Column("재고기준시각_utc")]
    public DateTime 재고기준시각Utc { get; set; } = DateTime.UtcNow;

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
