using System;
using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "기존 Preview를 변경하지 않고 지정 1회 수확의 권한·용량·완료단위 후보를 판정한다.",
        Boundary = "WI-FARM-04/05 소비 지원 정책만 제공한다. Session·Confirm·실제 이동·Save 또는 전체 WI E3 증거가 아니다.",
        WorldInteractionIds = new[] { "WI-FARM-04", "WI-FARM-05" })]
    public static class Simulation농사수확위임Policy
    {
        public static Simulation농사수확위임Result Evaluate(Simulation농사수확위임Input 입력)
        {
            var 차단 = new List<string>();
            var 결과 = new Simulation농사수확위임Result();
            if (입력 == null)
            {
                결과.차단사유Codes = new[] { Simulation농사수확위임Codes.InputInvalid };
                return 결과;
            }
            결과.완료단위기준Revision = 입력.완료단위기준Revision ?? string.Empty;
            var 기존 = 입력.수확Preview;
            if (기존 == null) 차단.Add(Simulation농사수확위임Codes.PreviewInvalid);
            else
            {
                결과.작물잔량Kgm = Math.Max(0m, 기존.ProjectedQuantity);
                if (!기존.CanConfirm) 차단.Add(Simulation농사수확위임Codes.PreviewRejected);
                if (기존.BlockingReasonCodes == null) 차단.Add(Simulation농사수확위임Codes.PreviewInvalid);
                else foreach (var 사유 in 기존.BlockingReasonCodes)
                    if (string.IsNullOrWhiteSpace(사유)) 차단.Add(Simulation농사수확위임Codes.PreviewInvalid);
                    else 차단.Add(사유);
                if (기존.ActionCode != SimulationFarmSurvivalCodes.Harvesting
                    || 기존.AssignmentKindCode != SimulationFarmSurvivalCodes.NpcDelegated
                    || !기존.SimulationOnly || 기존.IsOperationalState || 기존.ProjectedQuantity < 0m)
                    차단.Add(Simulation농사수확위임Codes.PreviewInvalid);
                if (기존.ProjectedQuantityUnitCode != Simulation농사수확위임Codes.Kilograms)
                    차단.Add(Simulation농사수확위임Codes.UnitMismatch);
                if (!같은지정(입력.승인ActorStableId, 기존.ActorStableId)
                    || !같은지정(입력.승인재배단위StableId, 기존.TargetStableId))
                    차단.Add(Simulation농사수확위임Codes.ScopeMismatch);
            }
            if (!같은지정(입력.승인보관처StableId, 입력.대상보관처StableId))
                차단.Add(Simulation농사수확위임Codes.ScopeMismatch);
            if (!입력.기존위임자격확인 || !입력.보관처재고사용권한확인)
                차단.Add(Simulation농사수확위임Codes.AuthorityDenied);
            if (입력.이미실행횟수 != 0) 차단.Add(Simulation농사수확위임Codes.AlreadyExecuted);
            if (입력.안전상태Code != Simulation농사수확위임Codes.Safe)
                차단.Add(Simulation농사수확위임Codes.UnsafeOrUnknown);
            if (입력.완료단위Kgm <= 0m || string.IsNullOrWhiteSpace(입력.완료단위기준Revision))
                차단.Add(Simulation농사수확위임Codes.CompletionUnitInvalid);
            if (입력.승인최대수량Kgm <= 0m || 입력.운반여유수량 < 0m)
                차단.Add(Simulation농사수확위임Codes.InputInvalid);
            if (입력.운반여유단위Code != Simulation농사수확위임Codes.Kilograms)
                차단.Add(Simulation농사수확위임Codes.UnitMismatch);

            var 용량유효 = 용량검증(입력.보관용량, 차단);
            용량유효 &= 용량검증(입력.점유용량, 차단);
            용량유효 &= 용량검증(입력.예약용량, 차단);
            var 보관여유 = 0m;
            if (용량유효)
            {
                // 더하기 overflow와 음수 여유를 피한다. 다른 용량 단위를 환산하지 않는다.
                if (입력.점유용량.Quantity > 입력.보관용량.Quantity
                    || 입력.예약용량.Quantity > 입력.보관용량.Quantity - 입력.점유용량.Quantity)
                    차단.Add(Simulation농사수확위임Codes.CapacityInvalid);
                else 보관여유 = 입력.보관용량.Quantity - 입력.점유용량.Quantity - 입력.예약용량.Quantity;
            }
            if (차단.Count == 0)
            {
                var 상한 = Math.Min(Math.Min(기존!.ProjectedQuantity, 입력.승인최대수량Kgm),
                    Math.Min(보관여유, 입력.운반여유수량));
                // 나눗셈의 큰 몫 overflow 없이 완료단위로 내린다.
                var 후보 = 상한 - 상한 % 입력.완료단위Kgm;
                if (후보 == 0m) 차단.Add(Simulation농사수확위임Codes.NoCompleteUnit);
                else
                {
                    결과.후보허용 = true;
                    결과.수용가능후보수량Kgm = 후보;
                    결과.작물잔량Kgm = 기존.ProjectedQuantity - 후보;
                    결과.다음행동Code = Simulation농사수확위임Codes.AwaitAuthorityCommand;
                }
            }
            // 배열을 공유하지 않으며 입력의 차단 순서를 유지한다.
            var 고유차단 = new List<string>();
            foreach (var 사유 in 차단)
                if (!고유차단.Contains(사유)) 고유차단.Add(사유);
            결과.차단사유Codes = 고유차단.ToArray();
            return 결과;
        }

        private static bool 같은지정(string 승인, string 대상)
            => !string.IsNullOrWhiteSpace(승인) && string.Equals(승인, 대상, StringComparison.Ordinal);

        private static bool 용량검증(Simulation공간용량Snapshot 용량, List<string> 차단)
        {
            if (용량 == null || 용량.Quantity < 0m
                || 용량.CapacityCode != Simulation공간용량Codes.StorageCapacity)
            {
                차단.Add(Simulation농사수확위임Codes.CapacityInvalid);
                return false;
            }
            if (용량.UnitCode != Simulation농사수확위임Codes.Kilograms)
            {
                차단.Add(Simulation농사수확위임Codes.UnitMismatch);
                return false;
            }
            return true;
        }
    }
}
