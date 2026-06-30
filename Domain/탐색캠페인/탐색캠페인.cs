using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.탐색캠페인;

[Table("탐색캠페인")]
public class 탐색캠페인
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("개시자UserId")]
    public string 개시자UserId { get; set; } = string.Empty;

    [Column("개시자역할")]
    public string 개시자역할 { get; set; } = string.Empty;

    [Column("대상역할")]
    public string 대상역할 { get; set; } = string.Empty;

    [Column("탐색유형")]
    public string 탐색유형 { get; set; } = string.Empty;

    [Column("탐색명")]
    public string 탐색명 { get; set; } = string.Empty;

    [Column("운행예정일")]
    public DateTime 운행예정일 { get; set; }

    [Column("출발권역")]
    public string 출발권역 { get; set; } = string.Empty;

    [Column("희망도착권역")]
    public string? 희망도착권역 { get; set; }

    [Column("경유권역Json")]
    public string? 경유권역Json { get; set; }

    [Column("차량종류")]
    public string 차량종류 { get; set; } = string.Empty;

    [Column("최대적재중량Kg")]
    public decimal? 최대적재중량Kg { get; set; }

    [Column("최대적재부피Cbm")]
    public decimal? 최대적재부피Cbm { get; set; }

    [Column("모집대상수")]
    public int 모집대상수 { get; set; }

    [Column("탐색상태")]
    public string 탐색상태 { get; set; } = 상태값.탐색캠페인상태.초안;

    [Column("응답요약")]
    public string 응답요약 { get; set; } = string.Empty;

    [Column("실행판단사유")]
    public string 실행판단사유 { get; set; } = string.Empty;

    [Column("메모")]
    public string 메모 { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
