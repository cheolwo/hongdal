using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hongdal.Contracts.Common.Exploration;

namespace 홍달.도메인.탐색캠페인;

[Table("탐색캠페인응답")]
public class 탐색캠페인응답
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("탐색캠페인Id")]
    public long 탐색캠페인Id { get; set; }

    [Column("응답자UserId")]
    public string 응답자UserId { get; set; } = string.Empty;

    [Column("응답자역할")]
    public string 응답자역할 { get; set; } = string.Empty;

    [Column("응답유형")]
    public 운행문의응답유형 응답유형 { get; set; }

    [Column("희망상차일시")]
    public DateTime? 희망상차일시 { get; set; }

    [Column("출발지요약")]
    public string 출발지요약 { get; set; } = string.Empty;

    [Column("도착지요약")]
    public string 도착지요약 { get; set; } = string.Empty;

    [Column("예상중량Kg")]
    public decimal? 예상중량Kg { get; set; }

    [Column("예상부피Cbm")]
    public decimal? 예상부피Cbm { get; set; }

    [Column("예상팔레트개수")]
    public int? 예상팔레트개수 { get; set; }

    [Column("메모요약")]
    public string 메모요약 { get; set; } = string.Empty;

    [Column("응답일시")]
    public DateTime 응답일시 { get; set; } = DateTime.UtcNow;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
