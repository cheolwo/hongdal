using System;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "검증된 동일 Actor의 연속 구간을 이전 활동으로 정산한 뒤 끝 시점 활동으로 전환한다.",
        Boundary = "순수 준비 계산. Host/Session/Command 원자성·실제 이동 신뢰·Save·Replay는 별도 결속 전이며 세계 상태를 직접 쓰지 않는다.",
        WorldInteractionIds = new[] { "WI-FARM-01", "WI-FARM-02", "WI-FARM-03", "WI-FARM-04" })]
    public static class Simulation행동체력자연회복Calculator
    {
        private const Simulation행동체력활동 허용표시 = Simulation행동체력활동.대기 | Simulation행동체력활동.걷기;
        private const Simulation행동체력활동 금지표시 = Simulation행동체력활동.노동 | Simulation행동체력활동.질주 | Simulation행동체력활동.전투;

        public static Simulation행동체력회복계산Result Prepare(
            decimal 현재체력, Simulation행동체력회복Cursor 현재, Simulation행동체력회복구간 구간)
        {
            if (현재 == null || 구간 == null)
                throw new SimulationContractException("StaminaRecoveryInputRequired");
            if (string.IsNullOrWhiteSpace(현재.SessionStableId) || string.IsNullOrWhiteSpace(현재.ActorStableId)
                || 현재.RuleRevision != Simulation행동체력자연회복Policy.Revision
                || 현재.정산시각Millis < 0 || 현재.활동Revision < 0
                || 현재.회복잔여분자 < 0 || 현재.회복잔여분자 >= Simulation행동체력자연회복Policy.초당Millis)
                throw new SimulationContractException("StaminaRecoveryCursorInvalid");
            if (현재체력 < 0 || 현재체력 > Simulation행동체력자연회복Policy.최대체력)
                throw new SimulationContractException("StaminaRecoveryValueInvalid");
            활동검증(현재.활동);
            if (!string.Equals(현재.SessionStableId, 구간.SessionStableId, StringComparison.Ordinal)
                || !string.Equals(현재.ActorStableId, 구간.ActorStableId, StringComparison.Ordinal))
                throw new SimulationConflictException("StaminaRecoveryActorBindingMismatch");
            if (현재.활동Revision != 구간.Expected활동Revision)
                throw new SimulationConflictException("StaminaRecoveryActivityRevisionMismatch");
            if (구간.시작Millis != 현재.정산시각Millis)
                throw new SimulationConflictException("StaminaRecoveryIntervalNotContiguous");
            if (구간.시작Millis < 0 || 구간.종료Millis < 구간.시작Millis
                || 구간.종료Millis - 구간.시작Millis > Simulation행동체력자연회복Policy.최대구간Millis)
                throw new SimulationContractException("StaminaRecoveryIntervalInvalid");
            if (구간.끝에서활동변경)
            {
                활동검증(구간.다음활동);
                if (현재.활동Revision == long.MaxValue || 구간.다음활동Revision != 현재.활동Revision + 1)
                    throw new SimulationConflictException("StaminaRecoveryNextActivityRevisionInvalid");
            }
            else if (구간.다음활동Revision != 0 || 구간.다음활동 != Simulation행동체력활동.미확인)
                throw new SimulationContractException("StaminaRecoveryUnexpectedActivityTransition");

            // 새 사본만 반환한다. 호출자는 같은 Session lock에서 실제 Actor 체력과 함께 적용해야 한다.
            var 다음 = new Simulation행동체력회복Cursor
            {
                SessionStableId = 현재.SessionStableId, ActorStableId = 현재.ActorStableId,
                RuleRevision = 현재.RuleRevision, 정산시각Millis = 구간.종료Millis,
                활동 = 구간.끝에서활동변경 ? 구간.다음활동 : 현재.활동,
                활동Revision = 구간.끝에서활동변경 ? 구간.다음활동Revision : 현재.활동Revision,
                회복잔여분자 = 현재.회복잔여분자
            };
            var 상태 = (현재.활동 & 금지표시) != 0 ? "BlockedByActivity"
                : (현재.활동 & 허용표시) == 0 ? "ActivityUnverified" : "Recovering";
            decimal 다음체력 = 현재체력;
            if (현재체력 == Simulation행동체력자연회복Policy.최대체력)
            {
                다음.회복잔여분자 = 0;
                상태 = "Full";
            }
            else if (상태 == "Recovering")
            {
                var 경과 = 구간.종료Millis - 구간.시작Millis;
                var 분자 = checked(경과 * Simulation행동체력자연회복Policy.초당회복Micro + 현재.회복잔여분자);
                var 회복Micro = 분자 / Simulation행동체력자연회복Policy.초당Millis;
                다음.회복잔여분자 = 분자 % Simulation행동체력자연회복Policy.초당Millis;
                다음체력 = Math.Min(Simulation행동체력자연회복Policy.최대체력,
                    현재체력 + (decimal)회복Micro / Simulation행동체력자연회복Policy.체력당Micro);
                if (다음체력 == Simulation행동체력자연회복Policy.최대체력)
                {
                    다음.회복잔여분자 = 0; // 상한에서 버린 회복량을 다음 작업 뒤 꺼내 쓰지 않는다.
                    상태 = "Full";
                }
                else if (경과 == 0) 상태 = "NoElapsedTime";
            }
            return new Simulation행동체력회복계산Result
            {
                이전체력 = 현재체력, 다음체력 = 다음체력, 회복량 = 다음체력 - 현재체력,
                회복상태Code = 상태, 다음Cursor = 다음
            };
        }

        private static void 활동검증(Simulation행동체력활동 활동)
        {
            if ((활동 & ~(허용표시 | 금지표시)) != 0 || (활동 & 허용표시) == 허용표시)
                throw new SimulationContractException("StaminaRecoveryActivityInvalid");
        }
    }
}
