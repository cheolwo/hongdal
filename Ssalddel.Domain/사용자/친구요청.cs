using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.사용자;

// 기존 운영 DB와 migration 호환을 위해 물리 테이블명은 유지한다.
[Table("인연연결요청")]
public class 친구요청
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("요청자_참여자_id")]
    [MaxLength(450)]
    public string 요청자참여자Id { get; set; } = string.Empty;

    [Column("요청자_역할")]
    public 살뜰역할유형 요청자역할 { get; set; }

    [Column("대상자_참여자_id")]
    [MaxLength(450)]
    public string 대상자참여자Id { get; set; } = string.Empty;

    [Column("대상자_역할")]
    public 살뜰역할유형 대상자역할 { get; set; }

    [Column("감사메시지_id")]
    public long? 감사메시지Id { get; set; }

    [Column("주문_id")]
    public long? 주문Id { get; set; }

    [Column("통관절차_id")]
    public long? 통관절차Id { get; set; }

    [Column("요청목적")]
    [MaxLength(300)]
    public string 요청목적 { get; set; } = string.Empty;

    [Column("요청메시지")]
    [MaxLength(1000)]
    public string 요청메시지 { get; set; } = string.Empty;

    [Column("상태")]
    public 친구요청상태 상태 { get; set; } = 친구요청상태.대기;

    [Column("요청일시")]
    public DateTimeOffset 요청일시 { get; set; } = DateTimeOffset.UtcNow;

    [Column("응답일시")]
    public DateTimeOffset? 응답일시 { get; set; }

    [Column("거절사유")]
    [MaxLength(500)]
    public string? 거절사유 { get; set; }
}

public enum 친구요청상태
{
    대기 = 1,
    수락 = 2,
    거절 = 3,
    취소 = 4
}
