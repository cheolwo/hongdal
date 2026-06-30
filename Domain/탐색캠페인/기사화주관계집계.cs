using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace 홍달.도메인.탐색캠페인;

[Table("기사화주관계집계")]
public class 기사화주관계집계
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("기사Id")]
    public string 기사Id { get; set; } = string.Empty;

    [Column("화주UserId")]
    public string 화주UserId { get; set; } = string.Empty;

    [Column("최근거래일시")]
    public DateTime? 최근거래일시 { get; set; }

    [Column("누적운송건수")]
    public int 누적운송건수 { get; set; }

    [Column("기사발신응답률")]
    public decimal 기사발신응답률 { get; set; }

    [Column("화주발신응답률")]
    public decimal 화주발신응답률 { get; set; }

    [Column("최근30일접점수")]
    public int 최근30일접점수 { get; set; }

    [Column("취소율")]
    public decimal 취소율 { get; set; }

    [Column("양방향관계점수")]
    public decimal 양방향관계점수 { get; set; }

    [Column("기사발신최근접촉일시")]
    public DateTime? 기사발신최근접촉일시 { get; set; }

    [Column("화주발신최근접촉일시")]
    public DateTime? 화주발신최근접촉일시 { get; set; }

    [Column("선호출발권역")]
    public string 선호출발권역 { get; set; } = string.Empty;

    [Column("선호도착권역")]
    public string 선호도착권역 { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
