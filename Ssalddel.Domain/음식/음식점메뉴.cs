using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.음식;

[Table("음식점메뉴")]
public sealed class 음식점메뉴
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("음식점공개프로필_id")]
    public long 음식점공개프로필Id { get; set; }

    [Column("메뉴명")]
    [MaxLength(200)]
    public string 메뉴명 { get; set; } = string.Empty;

    [Column("설명")]
    [MaxLength(1000)]
    public string 설명 { get; set; } = string.Empty;

    [Column("판매가", TypeName = "decimal(18,2)")]
    public decimal 판매가 { get; set; }

    [Column("대표이미지_url")]
    [MaxLength(1000)]
    public string? 대표이미지Url { get; set; }

    [Column("공개여부")]
    public bool 공개여부 { get; set; }

    [Column("품절여부")]
    public bool 품절여부 { get; set; }

    [Column("표시순서")]
    public int 표시순서 { get; set; }

    [Column("created_at_utc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Column("updated_at_utc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public 음식점공개프로필? 음식점공개프로필 { get; set; }
}
