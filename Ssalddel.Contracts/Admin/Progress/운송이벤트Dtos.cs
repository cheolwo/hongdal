namespace Ssalddel.Contracts.Admin.Progress;

public sealed class 운송이벤트요청
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 이벤트타입 { get; set; } = string.Empty;
    public DateTime 이벤트시각 { get; set; }
    public string 메타데이터 { get; set; } = string.Empty;
}