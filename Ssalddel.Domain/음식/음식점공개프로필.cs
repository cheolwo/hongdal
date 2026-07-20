using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.음식;

/// <summary>
/// 연락처·정산 조건을 가진 업체 원장과 분리된 주문자 공개용 음식점 투영입니다.
/// </summary>
[Table("음식점공개프로필")]
public sealed class 음식점공개프로필
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("업체_id")]
    public long? 업체Id { get; set; }

    [Column("상호명")]
    [MaxLength(200)]
    public string 상호명 { get; set; } = string.Empty;

    [Column("카테고리")]
    [MaxLength(100)]
    public string 카테고리 { get; set; } = string.Empty;

    [Column("소개")]
    [MaxLength(1000)]
    public string 소개 { get; set; } = string.Empty;

    [Column("공개주소")]
    [MaxLength(500)]
    public string 공개주소 { get; set; } = string.Empty;

    [Column("위도", TypeName = "decimal(18,10)")]
    public decimal 위도 { get; set; }

    [Column("경도", TypeName = "decimal(18,10)")]
    public decimal 경도 { get; set; }

    [Column("대표이미지_url")]
    [MaxLength(1000)]
    public string? 대표이미지Url { get; set; }

    [Column("최소주문금액", TypeName = "decimal(18,2)")]
    public decimal 최소주문금액 { get; set; }

    [Column("예상조리분")]
    public int 예상조리분 { get; set; }

    [Column("공개여부")]
    public bool 공개여부 { get; set; }

    [Column("주문가능여부")]
    public bool 주문가능여부 { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<음식점메뉴> 메뉴목록 { get; set; } = [];
}
