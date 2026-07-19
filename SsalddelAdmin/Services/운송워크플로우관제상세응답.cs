using Ssalddel.Contracts.Common.Workflow;

namespace SsalddelAdmin.Services;

public sealed class 운송워크플로우관제상세응답
{
    public string 의뢰Id { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 화주Id { get; set; } = string.Empty;
    public string 운송방식 { get; set; } = string.Empty;
    public string 경로표시 { get; set; } = string.Empty;
    public string 현재상태라벨 { get; set; } = string.Empty;
    public string 현재상태색상 { get; set; } = "secondary";
    public string 관리자다음행동 { get; set; } = string.Empty;
    public 화주운송의뢰응답? 의뢰 { get; set; }
    public 결제목록응답? 결제 { get; set; }
    public 배차대기응답? 배차대기 { get; set; }
    public 운송진행응답? 운송 { get; set; }
    public IReadOnlyList<운송워크플로우단계응답> 단계목록 { get; set; } = [];
    public IReadOnlyList<운송워크플로우운영확인응답> 운영확인목록 { get; set; } = [];
    public IReadOnlyList<운송이벤트로그응답> 이벤트목록 { get; set; } = [];
    public IReadOnlyList<파일POD응답> 증빙목록 { get; set; } = [];
    public IReadOnlyList<기사월정산관리응답> 정산후보목록 { get; set; } = [];
}
