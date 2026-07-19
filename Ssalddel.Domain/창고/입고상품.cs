using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.창고;

[Table("입고상품")]
public class 입고상품
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("입고요청_id")]
    public long 입고요청Id { get; set; }

    [Column("창고_id")]
    public long 창고Id { get; set; }

    [Column("소유자_user_id")]
    [MaxLength(450)]
    public string 소유자UserId { get; set; } = string.Empty;

    [Column("판매자_user_id")]
    [MaxLength(450)]
    public string 판매자UserId { get; set; } = string.Empty;

    [Column("상품명")]
    [MaxLength(200)]
    public string 상품명 { get; set; } = string.Empty;

    [Column("sku")]
    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;

    [Column("옵션명")]
    [MaxLength(200)]
    public string 옵션명 { get; set; } = string.Empty;

    [Column("입고수량")]
    public int 입고수량 { get; set; }

    [Column("가용수량")]
    public int 가용수량 { get; set; }

    [Column("예약수량")]
    public int 예약수량 { get; set; }

    [Column("불량수량")]
    public int 불량수량 { get; set; }

    [Column("보관위치")]
    [MaxLength(100)]
    public string 보관위치 { get; set; } = string.Empty;

    [Column("계약번호")]
    [MaxLength(100)]
    public string 계약번호 { get; set; } = string.Empty;

    [Column("계약유형")]
    [MaxLength(50)]
    public string 계약유형 { get; set; } = string.Empty;

    [Column("계약상대방명")]
    [MaxLength(200)]
    public string 계약상대방명 { get; set; } = string.Empty;

    [Column("정산방식")]
    [MaxLength(100)]
    public string 정산방식 { get; set; } = string.Empty;

    [Column("판매수수료율", TypeName = "decimal(9,2)")]
    public decimal 판매수수료율 { get; set; }

    [Column("보관료일단가", TypeName = "decimal(18,2)")]
    public decimal 보관료일단가 { get; set; }

    [Column("통관필요여부")]
    public bool 통관필요여부 { get; set; }

    [Column("계약시작일")]
    public DateTime? 계약시작일 { get; set; }

    [Column("계약종료일")]
    public DateTime? 계약종료일 { get; set; }

    [Column("계약메모")]
    [MaxLength(1000)]
    public string 계약메모 { get; set; } = string.Empty;

    [Column("상태")]
    [MaxLength(50)]
    public string 상태 { get; set; } = "보관중";

    [Column("입고완료일시")]
    public DateTime? 입고완료일시 { get; set; }

    [Column("community_ledger_id")]
    [MaxLength(120)]
    public string? 커뮤니티원장Id { get; set; }

    [Column("community_ledger_template_key")]
    [MaxLength(120)]
    public string? 커뮤니티원장템플릿Key { get; set; }

    [Column("community_ledger_state")]
    [MaxLength(80)]
    public string? 커뮤니티원장상태 { get; set; }

    [Column("community_ledger_synced_at_utc")]
    public DateTime? 커뮤니티원장동기화시각Utc { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
