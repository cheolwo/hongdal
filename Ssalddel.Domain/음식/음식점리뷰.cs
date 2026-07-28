using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.음식;

[Table("음식점리뷰")]
public sealed class 음식점리뷰
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("음식점_id")]
    public long 음식점Id { get; set; }

    [Column("주문자_user_id")]
    [MaxLength(450)]
    public string 주문자UserId { get; set; } = string.Empty;

    [Column("주문번호")]
    [MaxLength(100)]
    public string 주문번호 { get; set; } = string.Empty;

    [Column("별점")]
    public int 별점 { get; set; }

    [Column("내용")]
    [MaxLength(2000)]
    public string 내용 { get; set; } = string.Empty;

    [Column("사진_urls_json")]
    public string 사진UrlsJson { get; set; } = "[]";

    [Column("같은음식점_저평점3회연속")]
    public bool 같은음식점기준저평점3회연속여부 { get; set; }

    [Column("사장노출허용")]
    public bool 사장노출허용여부 { get; set; }

    [Column("관리자검토필요")]
    public bool 관리자검토필요여부 { get; set; }

    [Column("관리자게시강제")]
    public bool 관리자게시강제여부 { get; set; }

    [Column("현재노출")]
    public bool 현재노출여부 { get; set; }

    [Column("게시종료일시_utc")]
    public DateTime? 게시종료일시Utc { get; set; }

    [Column("최근조치사유")]
    [MaxLength(1000)]
    public string? 최근조치사유 { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
