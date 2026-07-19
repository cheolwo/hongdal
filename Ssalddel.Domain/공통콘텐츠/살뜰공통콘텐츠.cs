using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.공통콘텐츠;

public enum 살뜰콘텐츠유형
{
    이미지 = 1,
    영상링크 = 2,
    외부링크 = 3
}

[Flags]
public enum 살뜰노출위치
{
    없음 = 0,
    홈화면위젯 = 1,
    잠금화면위젯 = 2,
    결제전혜택 = 4,
    앱공지 = 8
}

public enum 살뜰보상유형
{
    없음 = 0,
    포인트 = 1,
    할인율 = 2,
    할인금액 = 3
}

[Table("살뜰_공통콘텐츠")]
public sealed class 살뜰공통콘텐츠
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("title")]
    [MaxLength(200)]
    public string 제목 { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(1000)]
    public string 설명 { get; set; } = string.Empty;

    [Column("content_type")]
    public 살뜰콘텐츠유형 콘텐츠유형 { get; set; }

    [Column("image_url")]
    [MaxLength(2000)]
    public string? 이미지Url { get; set; }

    [Column("video_url")]
    [MaxLength(2000)]
    public string? 영상Url { get; set; }

    [Column("external_link_url")]
    [MaxLength(2000)]
    public string? 외부링크Url { get; set; }

    [Column("placement_flags")]
    public 살뜰노출위치 노출위치 { get; set; }

    [Column("show_to_driver")]
    public bool 기사노출 { get; set; }

    [Column("show_to_shipper")]
    public bool 화주노출 { get; set; }

    [Column("show_to_admin")]
    public bool 운영자노출 { get; set; }

    [Column("is_active")]
    public bool 활성화여부 { get; set; } = true;

    [Column("start_at")]
    public DateTimeOffset? 노출시작시각 { get; set; }

    [Column("end_at")]
    public DateTimeOffset? 노출종료시각 { get; set; }

    [Column("reward_policy_id")]
    public long? 보상정책Id { get; set; }

    public 살뜰콘텐츠보상정책? 보상정책 { get; set; }

    [Column("created_at")]
    public DateTimeOffset 생성시각 { get; set; } = DateTimeOffset.UtcNow;
}

[Table("살뜰_콘텐츠보상정책")]
public sealed class 살뜰콘텐츠보상정책
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("reward_type")]
    public 살뜰보상유형 보상유형 { get; set; }

    [Column("point_amount")]
    public int 지급포인트 { get; set; }

    [Column("discount_rate")]
    public decimal 할인율 { get; set; }

    [Column("discount_amount")]
    public int 할인금액 { get; set; }

    [Column("minimum_watch_seconds")]
    public int 최소시청초 { get; set; }

    [Column("required_watch_ratio")]
    public decimal 필요시청비율 { get; set; } = 0.8m;

    [Column("one_time_per_user")]
    public bool 사용자당1회만지급 { get; set; } = true;

    [Column("max_discount_amount")]
    public int? 최대할인금액 { get; set; }
}