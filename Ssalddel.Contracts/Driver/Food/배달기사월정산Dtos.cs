namespace Ssalddel.Contracts.Driver.Food;

public sealed class 배달기사월정산응답
{
    public string 기사Id { get; set; } = string.Empty;
    public int 년도 { get; set; }
    public int 월 { get; set; }
    public int 배차건수 { get; set; }
    public decimal 이용료 { get; set; }
    public bool 결제완료 { get; set; }
}

public sealed class 배달기사월정산결제완료응답
{
    public string 기사Id { get; set; } = string.Empty;
    public int 년도 { get; set; }
    public int 월 { get; set; }
    public int 배차건수 { get; set; }
    public decimal 차감이용료 { get; set; }
    public bool 결제완료 { get; set; }
    public DateTime 처리일시Utc { get; set; }
}