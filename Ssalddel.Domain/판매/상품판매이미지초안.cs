using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 살뜰.도메인.판매;

[Table("상품판매이미지초안")]
public class 상품판매이미지초안
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("상품_id")]
    public long 상품Id { get; set; }

    [Column("생성작업_id")]
    public long 생성작업Id { get; set; }

    [Column("작성자_id")]
    [MaxLength(450)]
    public string 작성자Id { get; set; } = string.Empty;

    [Column("대표이미지url", TypeName = "varchar(1000)")]
    [MaxLength(1000)]
    public string? 대표이미지Url { get; set; }

    [Column("이미지목록json", TypeName = "longtext")]
    public string 이미지목록Json { get; set; } = "[]";

    [Column("원본자산참조json", TypeName = "longtext")]
    public string 원본자산참조Json { get; set; } = "[]";

    [Column("생성근거요약")]
    [MaxLength(1000)]
    public string 생성근거요약 { get; set; } = string.Empty;

    [Column("판매채널전송가능여부")]
    public bool 판매채널전송가능여부 { get; set; }

    [Column("생성시각")]
    public DateTimeOffset 생성시각 { get; set; } = DateTimeOffset.UtcNow;
}
