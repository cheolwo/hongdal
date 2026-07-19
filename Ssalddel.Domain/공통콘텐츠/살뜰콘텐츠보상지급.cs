using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.공통콘텐츠;

[Table("살뜰_콘텐츠보상지급")]
public sealed class 살뜰콘텐츠보상지급
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    [MaxLength(64)]
    public string 사용자Id { get; set; } = string.Empty;

    [Column("content_id")]
    public long 콘텐츠Id { get; set; }

    [Column("reward_type")]
    public 살뜰보상유형 보상유형 { get; set; }

    [Column("granted_points")]
    public int 지급포인트 { get; set; }

    [Column("discount_rate")]
    public decimal 할인율 { get; set; }

    [Column("discount_amount")]
    public int 할인금액 { get; set; }

    [Column("is_used_in_payment")]
    public bool 결제사용여부 { get; set; }

    [Column("granted_at")]
    public DateTimeOffset 지급시각 { get; set; } = DateTimeOffset.UtcNow;

    [Column("used_at")]
    public DateTimeOffset? 사용시각 { get; set; }
}