using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.판매;

[Table("상품상세이미지생성작업")]
public class 상품상세이미지생성작업
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

    [Column("요청자_id")]
    [MaxLength(450)]
    public string 요청자Id { get; set; } = string.Empty;

    [Column("상태")]
    public 상세이미지생성상태 상태 { get; set; } = 상세이미지생성상태.대기;

    [Column("생성프롬프트", TypeName = "longtext")]
    public string? 생성프롬프트 { get; set; }

    [Column("원본자산참조json", TypeName = "longtext")]
    public string? 원본자산참조Json { get; set; }

    [Column("오류내용", TypeName = "longtext")]
    public string? 오류내용 { get; set; }

    [Column("관련생성이미지작업_id")]
    public long? 관련생성이미지작업Id { get; set; }

    [Column("생성시각")]
    public DateTimeOffset 생성시각 { get; set; } = DateTimeOffset.UtcNow;

    [Column("완료시각")]
    public DateTimeOffset? 완료시각 { get; set; }
}

public enum 상세이미지생성상태
{
    대기 = 1,
    원본수집중 = 2,
    프롬프트생성완료 = 3,
    이미지생성요청중 = 4,
    이미지생성완료 = 5,
    실패 = 6,
    취소 = 7
}
