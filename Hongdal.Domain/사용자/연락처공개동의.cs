using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.사용자;

[Table("연락처공개동의")]
public class 연락처공개동의
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("인연연결요청_id")]
    public long 인연연결요청Id { get; set; }

    [Column("동의자_참여자_id")]
    [MaxLength(450)]
    public string 동의자참여자Id { get; set; } = string.Empty;

    [Column("프로필공개")]
    public bool 프로필공개 { get; set; }

    [Column("업체명공개")]
    public bool 업체명공개 { get; set; }

    [Column("이메일공개")]
    public bool 이메일공개 { get; set; }

    [Column("전화번호공개")]
    public bool 전화번호공개 { get; set; }

    [Column("카카오채널공개")]
    public bool 카카오채널공개 { get; set; }

    [Column("판매채널공개")]
    public bool 판매채널공개 { get; set; }

    [Column("제공목적")]
    [MaxLength(500)]
    public string 제공목적 { get; set; } = string.Empty;

    [Column("동의일시")]
    public DateTimeOffset 동의일시 { get; set; } = DateTimeOffset.UtcNow;

    [Column("철회일시")]
    public DateTimeOffset? 철회일시 { get; set; }
}
