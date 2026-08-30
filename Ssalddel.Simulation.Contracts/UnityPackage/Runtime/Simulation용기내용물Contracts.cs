using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation용기내용물Codes
    {
        public const string RuleRevision = "container-contents.hb01.r1";
        public const string 물 = "Water", 차 = "Tea";
        public const long 냄비시험최대량Ml = 1000, 병시험최대량Ml = 500, 컵시험최대량Ml = 200;
    }

    /// <summary>출처별 실제 잔량. 입력 검증은 공통 계산 경계에서 수행한다.</summary>
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "내용물 제조 출처별 정수 mL 계약이다.", Boundary = "미래 소비자 지원 계약이며 WI 실행·저장·효과 증거가 아니다.",
        WorldInteractionIds = new[] { "WI-ACTOR-CONSUME", "WI-CRAFT-BREW" })]
    public sealed class Simulation내용물출처Snapshot
    {
        public string 제조출처StableId { get; }
        public long 양Ml { get; }
        public Simulation내용물출처Snapshot(string 제조출처StableId, long 양Ml)
        { this.제조출처StableId = 제조출처StableId; this.양Ml = 양Ml; }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "용기 능력·내용물 양·처방 판본·온도·출처의 불변 상태 사본이다.",
        Boundary = "온도 단위는 게임용 milli°C. 실제 재질 안전·약효·Session 권위 계약이 아니다.",
        WorldInteractionIds = new[] { "WI-ACTOR-CONSUME", "WI-CRAFT-BREW" })]
    public sealed class Simulation용기내용물Snapshot
    {
        private readonly Simulation내용물출처Snapshot[] 출처;
        public string 용기StableId { get; }
        public bool 물보관가능 { get; }
        public long 최대량Ml { get; }
        public long 현재량Ml { get; }
        public string 종류Code { get; }
        public string 처방StableId { get; }
        public string 효과Revision { get; }
        public long 온도MilliCelsius { get; }
        public Simulation내용물출처Snapshot[] 제조출처 => (Simulation내용물출처Snapshot[])출처.Clone();

        public Simulation용기내용물Snapshot(string 용기StableId, bool 물보관가능,
            long 최대량Ml, long 현재량Ml, string 종류Code, string 처방StableId,
            string 효과Revision, long 온도MilliCelsius, Simulation내용물출처Snapshot[] 제조출처)
        {
            this.용기StableId = 용기StableId; this.물보관가능 = 물보관가능;
            this.최대량Ml = 최대량Ml; this.현재량Ml = 현재량Ml;
            this.종류Code = 종류Code; this.처방StableId = 처방StableId;
            this.효과Revision = 효과Revision; this.온도MilliCelsius = 온도MilliCelsius;
            출처 = 제조출처 == null ? throw new ArgumentNullException(nameof(제조출처))
                : (Simulation내용물출처Snapshot[])제조출처.Clone();
        }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "동종 전송 후보의 두 상태 사본과 실제 이동량을 반환한다.",
        Boundary = "권위 Confirm·행위 기록·성장 적용이 없는 순수 계산 결과다.",
        WorldInteractionIds = new[] { "WI-ACTOR-CONSUME", "WI-CRAFT-BREW" })]
    public sealed class Simulation내용물전송Result
    {
        public Simulation용기내용물Snapshot 원천 { get; }
        public Simulation용기내용물Snapshot 대상 { get; }
        public long 이동량Ml { get; }
        public Simulation내용물전송Result(Simulation용기내용물Snapshot 원천,
            Simulation용기내용물Snapshot 대상, long 이동량Ml)
        { this.원천 = 원천; this.대상 = 대상; this.이동량Ml = 이동량Ml; }
    }
}
