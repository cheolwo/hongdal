using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    // 이 값은 클라이언트의 회복 허용 주장으로 받지 않는다. Session의 실제 활동 작성자가 공급한다.
    [Flags]
    public enum Simulation행동체력활동
    {
        미확인 = 0,
        대기 = 1,
        걷기 = 2,
        노동 = 4,
        질주 = 8,
        전투 = 16
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "Farm의 기존 비용과 분리된 자연 회복 계산 단위·정밀도·시간 범위를 정의한다.",
        Boundary = "고정 시험 출발값이며 Host 시간·Actor 활동의 진실성을 증명하지 않는다. 휴식·마나·열원·성장을 포함하지 않는다.",
        WorldInteractionIds = new[] { "WI-FARM-01", "WI-FARM-02", "WI-FARM-03", "WI-FARM-04" })]
    public static class Simulation행동체력자연회복Policy
    {
        public const string Revision = "player-stamina-natural-recovery.rule.r1";
        public const decimal 최대체력 = 100m;
        public const long 초당회복Micro = 250000;
        public const long 체력당Micro = 1000000;
        public const long 초당Millis = 1000;
        public const long 최대구간Millis = 60000;
    }

    // 저장 계약이 아닌 계산 입력. 실제 Session/Save 결속 전에는 별도 권위 원장으로 사용하지 않는다.
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "동일 Actor의 마지막 정산 시각·현재 활동·정수 분할 잔여를 명시한다.",
        Boundary = "Stamina 원본은 기존 Farm Actor다. 미확인 활동은 회복 불허이며 현재 Snapshot과 이 계산 입력의 연결은 통합 담당 책임이다.",
        WorldInteractionIds = new[] { "WI-FARM-01", "WI-FARM-02", "WI-FARM-03", "WI-FARM-04" })]
    public sealed class Simulation행동체력회복Cursor
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = Simulation행동체력자연회복Policy.Revision;
        public long 정산시각Millis { get; set; }
        public long 활동Revision { get; set; }
        public Simulation행동체력활동 활동 { get; set; }
        // 1 Micro 미만 잔여의 분자(분모1000). 금지 구간에는 보존, 상한 도달 시 폐기.
        public long 회복잔여분자 { get; set; }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "Host가 확인한 연속 시간 구간과 끝 시점 활동 전이를 입력한다.",
        Boundary = "네트워크 요청 DTO가 아니다. 시간 공급자·Actor 작성자 검증을 대신하는 trusted bool이나 UI 경과초를 두지 않는다.",
        WorldInteractionIds = new[] { "WI-FARM-01", "WI-FARM-02", "WI-FARM-03", "WI-FARM-04" })]
    public sealed class Simulation행동체력회복구간
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public long 시작Millis { get; set; }
        public long 종료Millis { get; set; }
        public long Expected활동Revision { get; set; }
        public bool 끝에서활동변경 { get; set; }
        public long 다음활동Revision { get; set; }
        public Simulation행동체력활동 다음활동 { get; set; }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "회복량·기존 체력·새 체력·다음 cursor·차단 이유를 무변경 계산 결과로 반환한다.",
        Boundary = "계산 성공은 Confirm, ActionRecord, WorldRevision, Session 또는 Save 반영 성공이 아니다.",
        WorldInteractionIds = new[] { "WI-FARM-01", "WI-FARM-02", "WI-FARM-03", "WI-FARM-04" })]
    public sealed class Simulation행동체력회복계산Result
    {
        public decimal 이전체력 { get; set; }
        public decimal 다음체력 { get; set; }
        public decimal 회복량 { get; set; }
        public string 회복상태Code { get; set; } = string.Empty;
        public Simulation행동체력회복Cursor 다음Cursor { get; set; } = new Simulation행동체력회복Cursor();
    }
}
