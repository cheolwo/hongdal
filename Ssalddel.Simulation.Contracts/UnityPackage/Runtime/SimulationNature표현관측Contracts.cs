using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>하나의 LocalRuntime 읽기 구간에서 확보한 표현용 사본. 저장 형식이 아니다.</summary>
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "Nature 상태와 완료 행위 원장을 동일 권위 읽기 구간으로 묶는다.",
        Boundary = "읽기 사본이며 권위 명령·Save 형식·Remote API·실제 표현 완료를 추가하지 않는다.")]
    public sealed class SimulationNature표현관측Snapshot
    {
        public 경영SimulationSessionSnapshot Session { get; set; } = new();
        public SimulationNatureSurvivalStateSnapshot Nature { get; set; } = new();
        public Simulation영역건물발전Snapshot BuildingProgression { get; set; } = new();
        // null은 과거 상태 등에 원장이 미제공됨을 뜻한다. 완료 기록을 합성하지 않는다.
        public Simulation행위기록LedgerSnapshot? ActionLedger { get; set; }
    }
}
