namespace Hongdal.Contracts.Admin.Settlement;

public sealed class 기사월정산관리응답
{
    public string 기사Id { get; set; } = string.Empty;
    public int 년도 { get; set; }
    public int 월 { get; set; }
    public int 배차건수 { get; set; }
    public decimal 이용료 { get; set; }
    public bool 월상한적용여부 { get; set; }
    public bool 결제완료 { get; set; }
    public DateTime UpdatedAt { get; set; }
}