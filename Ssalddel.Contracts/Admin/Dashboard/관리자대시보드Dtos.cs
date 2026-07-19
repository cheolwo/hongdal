namespace Ssalddel.Contracts.Admin.Dashboard;

public sealed class 관리자대시보드요약응답
{
    public int 오늘의뢰수 { get; set; }
    public int 결제대기수 { get; set; }
    public int 결제완료수 { get; set; }
    public int 배차대기수 { get; set; }
    public int 배차확정수 { get; set; }
    public int 운송중수 { get; set; }
    public int 완료수 { get; set; }
    public int 취소환불수 { get; set; }
    public int 운송예외수 { get; set; }
    public int 관리자확인필요수 { get; set; }
}
