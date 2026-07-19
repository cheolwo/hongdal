using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.판매;

[Table("상품물류자산")]
public class 상품물류자산
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

    [Column("자산유형")]
    public 상품자산유형 자산유형 { get; set; }

    [Column("파일url", TypeName = "varchar(1000)")]
    [MaxLength(1000)]
    public string 파일Url { get; set; } = string.Empty;

    [Column("설명")]
    [MaxLength(1000)]
    public string? 설명 { get; set; }

    [Column("등록자_id")]
    [MaxLength(450)]
    public string 등록자Id { get; set; } = string.Empty;

    [Column("상세이미지사용가능여부")]
    public bool 상세이미지사용가능여부 { get; set; } = true;

    [Column("등록시각")]
    public DateTimeOffset 등록시각 { get; set; } = DateTimeOffset.UtcNow;
}

public enum 상품자산유형
{
    검품사진 = 1,
    포장사진 = 2,
    라벨사진 = 3,
    실측사진 = 4,
    구성품사진 = 5,
    손상확인사진 = 6,
    원산지자료 = 7,
    통관자료 = 8,
    성분표시 = 9,
    사용주의사항 = 10,
    물류메모 = 11,
    상세이미지생성원본 = 12,
    상세이미지생성이미지 = 13
}
