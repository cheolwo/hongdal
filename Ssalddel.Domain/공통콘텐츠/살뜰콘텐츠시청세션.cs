using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.공통콘텐츠;

[Table("살뜰_콘텐츠시청세션")]
public sealed class 살뜰콘텐츠시청세션
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("user_id")]
    [MaxLength(64)]
    public string 사용자Id { get; set; } = string.Empty;

    [Column("content_id")]
    public long 콘텐츠Id { get; set; }

    public 살뜰공통콘텐츠 콘텐츠 { get; set; } = default!;

    [Column("video_total_seconds")]
    public int 영상전체초 { get; set; }

    [Column("watched_seconds")]
    public int 누적시청초 { get; set; }

    [Column("is_completed")]
    public bool 완료여부 { get; set; }

    [Column("is_reward_granted")]
    public bool 보상지급여부 { get; set; }

    [Column("started_at")]
    public DateTimeOffset 시작시각 { get; set; } = DateTimeOffset.UtcNow;

    [Column("last_progress_at")]
    public DateTimeOffset? 마지막진행시각 { get; set; }

    [Column("completed_at")]
    public DateTimeOffset? 완료시각 { get; set; }
}