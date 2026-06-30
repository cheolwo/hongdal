using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.탐색캠페인;

[Table("탐색캠페인대상자")]
public class 탐색캠페인대상자
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("탐색캠페인Id")]
    public long 탐색캠페인Id { get; set; }

    [Column("대상UserId")]
    public string 대상UserId { get; set; } = string.Empty;

    [Column("대상역할")]
    public string 대상역할 { get; set; } = string.Empty;

    [Column("관계점수Snapshot")]
    public decimal 관계점수Snapshot { get; set; }

    [Column("반응가능성점수Snapshot")]
    public decimal 반응가능성점수Snapshot { get; set; }

    [Column("선정사유")]
    public string 선정사유 { get; set; } = string.Empty;

    [Column("대상상태")]
    public string 대상상태 { get; set; } = 상태값.탐색캠페인대상상태.선정됨;

    [Column("발송메시지")]
    public string 발송메시지 { get; set; } = string.Empty;

    [Column("발송일시")]
    public DateTime? 발송일시 { get; set; }

    [Column("마지막응답일시")]
    public DateTime? 마지막응답일시 { get; set; }

    [Column("예상정보요약")]
    public string 예상정보요약 { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
