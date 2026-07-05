using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.판매;

[Table("감사메시지")]
public class 감사메시지
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("상품_id")]
    public long 상품Id { get; set; }

    [Column("주문_id")]
    public long? 주문Id { get; set; }

    [Column("통관절차_id")]
    public long? 통관절차Id { get; set; }

    [Column("발신자구분")]
    [MaxLength(50)]
    public string 발신자구분 { get; set; } = "익명구매자";

    [Column("발신참여자_id")]
    [MaxLength(450)]
    public string? 발신참여자Id { get; set; }

    [Column("대상역할")]
    [MaxLength(100)]
    public string 대상역할 { get; set; } = string.Empty;

    [Column("대상참여자_id")]
    [MaxLength(450)]
    public string? 대상참여자Id { get; set; }

    [Column("대상표시명")]
    [MaxLength(200)]
    public string 대상표시명 { get; set; } = string.Empty;

    [Column("메시지내용")]
    [MaxLength(1000)]
    public string 메시지내용 { get; set; } = string.Empty;

    [Column("공개가능여부")]
    public bool 공개가능여부 { get; set; }

    [Column("수신자에게전달여부")]
    public bool 수신자에게전달여부 { get; set; }

    [Column("검수상태")]
    public 감사메시지검수상태 검수상태 { get; set; } = 감사메시지검수상태.대기;

    [Column("작성일시")]
    public DateTimeOffset 작성일시 { get; set; } = DateTimeOffset.UtcNow;
}

public enum 감사메시지검수상태
{
    대기 = 1,
    승인 = 2,
    반려 = 3,
    차단 = 4
}
