namespace Hongdal.Contracts.Admin.Management;

public sealed class 운임구성요청
{
    public string 의뢰Id { get; set; } = string.Empty;
    public decimal 기본운임 { get; set; }
    public decimal 거리운임 { get; set; }
    public decimal 할증 { get; set; }
    public decimal 대기료 { get; set; }
    public decimal 수작업비 { get; set; }
    public decimal 최종운임 { get; set; }
}