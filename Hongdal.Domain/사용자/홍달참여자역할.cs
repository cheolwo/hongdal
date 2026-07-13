using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.사용자;

public enum 홍달역할유형
{
    주문자 = 1,
    판매자 = 2,
    기사 = 3,
    창고관리자 = 4,
    운영자 = 5,
    교육참여자 = 6,
    모임참여자 = 7,
    관세사 = 8,
    선생님 = 9,
    현장체험지도자 = 10
}

[Table("홍달참여자역할")]
public class 홍달참여자역할
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("참여자_id")]
    [MaxLength(450)]
    public string 참여자Id { get; set; } = string.Empty;

    [Column("역할유형")]
    public 홍달역할유형 역할유형 { get; set; }

    [Column("활성화여부")]
    public bool 활성화여부 { get; set; } = true;

    [Column("부여시각")]
    public DateTimeOffset 부여시각 { get; set; } = DateTimeOffset.UtcNow;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public 홍달참여자? 참여자 { get; set; }
}
