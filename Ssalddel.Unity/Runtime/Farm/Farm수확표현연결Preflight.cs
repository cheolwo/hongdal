using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Unity.PresentationContracts;

namespace Ssalddel.Unity.Farm
{
    /// <summary>
    /// D386의 이미 준비된 불변 상태만 읽는다. TryPrepare/Resolve/lease/Scene setter를 실행하지 않는다.
    /// Slot은 VisualKey/정확 후보와 다르므로 D388의 후보 선택 공백을 임의 대응표로 메우지 않는다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "한 재배의 기존 준비상태와 E4 후보/대상/소유 관측을 공통 연결 사전검사에 소비시킨다.",
        WorkOrderIds = new[] { "E7-WO-FARM-CROP-CYCLE" },
        WorldInteractionIds = new[] { "WI-FARM-04" },
        Boundary = "상태 사본 미확보·과거 관측을 실제 수확/Scene/논리E5 성공으로 대체하지 않는다.")]
    public static class Farm수확표현연결Preflight
    {
        /// <summary>기존 Review의 관측·E5 차단을 보존하며 별도 E4 후보 준비의 동일 상태/후보 결속을 대조한다.</summary>
        public static 표현연결Result ReviewVisualCandidate(Farm수확상태PresentationState? state,
            Farm수확시각후보State? candidate, 표현연결Plan? plan, 표현연결관측Snapshot? observations)
        {
            var baseline = Review(state, plan, observations);
            var checks = new List<표현연결Check>(baseline.Checks);
            if (candidate == null)
                checks.Add(표현연결Preflight.Check(표현연결항목.CandidatePath, "farm-visual-candidate",
                    표현연결Readiness.Conditional, "FarmVisualCandidateNotPrepared"));
            else
            {
                if (state != null)
                {
                    Same(표현연결항목.Session, state.SessionStableId, candidate.Source.SessionStableId);
                    Same(표현연결항목.Target, state.CultivationUnitStableId, candidate.Source.CultivationUnitStableId);
                    Same(표현연결항목.StateRevision, state.SourceWorldRevision.ToString(CultureInfo.InvariantCulture),
                        candidate.SourceWorldRevision.ToString(CultureInfo.InvariantCulture));
                    Same(표현연결항목.PresentationRevision, state.PresentationRevision, candidate.SourcePresentationRevision);
                    Same(표현연결항목.StateCode, state.StateCode, candidate.Source.StateCode);
                }
                MatchPlan(표현연결항목.CandidatePath, candidate.Candidate.AssetPath);
                MatchPlan(표현연결항목.CandidateFingerprint, candidate.CandidateFingerprint);
                MatchPlan(표현연결항목.VisualKey, candidate.VisualKey);
            }
            return new 표현연결Result(plan, checks);

            void Same(표현연결항목 item, string expected, string actual)
            {
                if (expected != actual)
                    checks.Add(표현연결Preflight.Check(item, "farm-visual-candidate", 표현연결Readiness.Blocked,
                        "FarmVisualCandidateBindingMismatch", expected, actual));
            }
            void MatchPlan(표현연결항목 item, string actual)
            {
                var required = plan?.Requirements.Where(x => x.Item == item).ToArray()
                    ?? Array.Empty<표현연결Requirement>();
                if (required.Length == 1 && !string.IsNullOrWhiteSpace(required[0].ExpectedValue))
                    Same(item, required[0].ExpectedValue, actual);
            }
        }

        public static 표현연결Result Review(Farm수확상태PresentationState? state,
            표현연결Plan? plan, 표현연결관측Snapshot? observations)
        {
            var common = 표현연결Preflight.Review(plan, observations);
            var checks = new List<표현연결Check>(common.Checks);
            if (observations == null || !observations.Observations.Any(x => x.Item == 표현연결항목.LogicE5
                && x.Status != 표현연결ObservationStatus.Unobserved))
                checks.Add(표현연결Preflight.Check(표현연결항목.LogicE5, "farm-logic-evidence",
                    표현연결Readiness.Conditional, "FarmLogicE5EvidenceMissing"));
            if (state == null)
                checks.Add(표현연결Preflight.Check(표현연결항목.StateRevision, "farm-state",
                    표현연결Readiness.Conditional, "FarmSnapshotMissing_E5Unlinked"));
            else
            {
                Match(표현연결항목.Target, state.CultivationUnitStableId);
                Match(표현연결항목.Session, state.SessionStableId);
                Match(표현연결항목.StateRevision, state.SourceWorldRevision.ToString(CultureInfo.InvariantCulture));
                Match(표현연결항목.PresentationRevision, state.PresentationRevision);
                Match(표현연결항목.PresentationSlot, state.PresentationSlot);
                Match(표현연결항목.StateCode, state.StateCode);
            }
            return new 표현연결Result(plan, checks);

            void Match(표현연결항목 item, string actual)
            {
                var required = plan?.Requirements.Where(x => x.Item == item).ToArray()
                    ?? Array.Empty<표현연결Requirement>();
                // 미준비와 확인된 불일치를 섞지 않는다. 누락/중복 판정은 공통 검사를 보존한다.
                if (required.Length != 1 || string.IsNullOrWhiteSpace(required[0].ExpectedValue)) return;
                if (required[0].ExpectedValue != actual)
                    checks.Add(표현연결Preflight.Check(item, "farm-state", 표현연결Readiness.Blocked,
                        "FarmPreparedStateMismatch", required.FirstOrDefault()?.ExpectedValue ?? "", actual));
            }
        }
    }
}
