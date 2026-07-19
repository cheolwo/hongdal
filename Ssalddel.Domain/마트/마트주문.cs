using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.마트;

[Table("마트주문")]
public class 마트주문
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("주문참조번호")]
    [MaxLength(100)]
    public string 주문참조번호 { get; set; } = string.Empty;

    [Column("주문_id")]
    public long? 주문Id { get; set; }

    [Column("주문자_user_id")]
    [MaxLength(450)]
    public string 주문자UserId { get; set; } = string.Empty;

    [Column("판매자_user_id")]
    [MaxLength(450)]
    public string 판매자UserId { get; set; } = string.Empty;

    [Column("상태")]
    [MaxLength(50)]
    public string 상태 { get; set; } = "출고 예정";

    [Column("현재단계")]
    [MaxLength(80)]
    public string? 현재단계 { get; set; }

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

    public List<마트주문상품> 상품목록 { get; set; } = [];
}
