using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.판매;

[Table("판매상품")]
public class 판매상품
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("입고상품_id")]
    public long 입고상품Id { get; set; }

    [Column("소유자_user_id")]
    [MaxLength(450)]
    public string 소유자UserId { get; set; } = string.Empty;

    [Column("대표상품명")]
    [MaxLength(200)]
    public string 대표상품명 { get; set; } = string.Empty;

    [Column("판매sku")]
    [MaxLength(100)]
    public string 판매SKU { get; set; } = string.Empty;

    [Column("판매가", TypeName = "decimal(18,2)")]
    public decimal 판매가 { get; set; }

    [Column("상태")]
    [MaxLength(50)]
    public string 상태 { get; set; } = "준비";

    [Column("샘플데이터여부")]
    public bool 샘플데이터여부 { get; set; }

    [Column("샘플데이터코드")]
    [MaxLength(100)]
    public string? 샘플데이터코드 { get; set; }

    [Column("Image_Url", TypeName = "varchar(1000)")]
    public string? 이미지Url { get; set; }

    [Column("이미지생성상태")]
    [MaxLength(50)]
    public string 이미지생성상태 { get; set; } = 판매상품이미지생성상태.미생성;

    [Column("이미지생성요청시각")]
    public DateTime? 이미지생성요청시각 { get; set; }

    [Column("이미지생성완료시각")]
    public DateTime? 이미지생성완료시각 { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public static class 판매상품이미지생성상태
{
    public const string 미생성 = "미생성";
    public const string 생성대기 = "생성대기";
    public const string 생성중 = "생성중";
    public const string 완료 = "완료";
    public const string 실패 = "실패";
}
