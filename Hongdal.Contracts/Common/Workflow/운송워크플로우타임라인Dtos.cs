namespace Hongdal.Contracts.Common.Workflow;

public sealed class 운송워크플로우단계응답
{
    public int 순번 { get; set; }
    public string 단계코드 { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public string 증빙 { get; set; } = string.Empty;
    public bool 완료됨 { get; set; }
    public bool 진행중 { get; set; }
    public bool 확인필요 { get; set; }
    public string 색상 { get; set; } = "secondary";
}

public sealed class 운송워크플로우운영확인응답
{
    public string 구분 { get; set; } = string.Empty;
    public string 우선도 { get; set; } = string.Empty;
    public string 조치안내 { get; set; } = string.Empty;
}
