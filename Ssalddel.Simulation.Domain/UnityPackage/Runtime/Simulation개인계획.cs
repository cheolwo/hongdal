using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
        "개인 계획 설정·수정과 최초 안정 근거의 중복 방지를 판정한다.",
        Boundary = "별도 메모리 원장. 회복·명상 수치·진척·수면·기존 Day2 원장을 변경하지 않는다.",
        WorldInteractionIds = new[] { Simulation개인계획Codes.WorldInteractionId })]
    public sealed class Simulation개인계획Aggregate
    {
        private readonly object 동기화 = new object();
        private readonly string 세계, 세션, 플레이어, 계획, 정책;
        private readonly int 문구상한;
        private readonly HashSet<string> 허용목표;
        private long 개정;
        private string 목표 = string.Empty, 문구 = string.Empty, 최초안정근거 = string.Empty;
        private Simulation행위발현Ledger 행위;
        private readonly Dictionary<string, (string 입력, Simulation개인계획Snapshot 결과)> 명령들 =
            new Dictionary<string, (string, Simulation개인계획Snapshot)>(StringComparer.Ordinal);

        public Simulation개인계획Aggregate(Simulation개인계획InitialState 초기)
        {
            if (초기 == null) throw new SimulationContractException("PersonalPlanInitialStateRequired");
            세계 = 필수(초기.WorldStableId); 세션 = 필수(초기.SessionStableId);
            플레이어 = 필수(초기.PlayerStableId); 계획 = 필수(초기.PlanStableId); 정책 = 필수(초기.PolicyRevision);
            if (초기.WorldRevision < 0 || 초기.MaxDescriptionLength <= 0 || 초기.AllowedObjectiveStableIds == null)
                throw new SimulationContractException("PersonalPlanPolicyInvalid");
            허용목표 = new HashSet<string>(초기.AllowedObjectiveStableIds.Select(필수), StringComparer.Ordinal);
            if (허용목표.Count == 0 || 허용목표.Count != 초기.AllowedObjectiveStableIds.Length)
                throw new SimulationContractException("PersonalPlanObjectivesInvalid");
            문구상한 = 초기.MaxDescriptionLength; 개정 = 초기.WorldRevision;
            행위 = new Simulation행위발현Ledger(세계);
        }

        public Simulation개인계획Snapshot Snapshot() { lock (동기화) return 사본(); }
        public Simulation개인계획PreviewSnapshot Preview(Simulation개인계획PreviewRequest 요청)
        {
            if (요청 == null) throw new SimulationContractException("PersonalPlanRequestRequired");
            lock (동기화) return 판정(요청);
        }

        public Simulation개인계획ConfirmResult Confirm(Simulation개인계획ConfirmRequest 요청)
        {
            if (요청 == null) throw new SimulationContractException("PersonalPlanRequestRequired");
            var 명령 = 필수(요청.CommandId);
            lock (동기화)
            {
                var 정규문구 = (요청.Description ?? string.Empty).Trim();
                var 입력 = 정규형(요청.ActorStableId, 요청.PlanStableId, 요청.ObjectiveStableId, 정규문구, 요청.ExpectedRevision);
                if (명령들.TryGetValue(명령, out var 이전))
                {
                    if (입력 != 이전.입력) throw new SimulationConflictException("PersonalPlanCommandPayloadConflict");
                    return new Simulation개인계획ConfirmResult { Reused = true, State = 복사(이전.결과) };
                }
                var 확인 = 판정(요청);
                if (!확인.CanConfirm) throw new SimulationConflictException(확인.BlockReasonCodes[0]);
                var 다음근거 = 최초안정근거.Length == 0 ? "plan-stability-eligibility:" + 해시(정규형(세계, 세션, 플레이어, 계획)) : 최초안정근거;
                var 다음행위 = Simulation행위발현Ledger.Restore(행위.Snapshot());
                다음행위.Append(new Simulation행위발현Record
                {
                    WorldStableId = 세계, SessionStableId = 세션,
                    PlayableLoopStableId = "playable-loop:nature-night-day2.v1",
                    WorldInteractionId = Simulation개인계획Codes.WorldInteractionId,
                    CommandId = 명령, ActorStableId = 플레이어, InitiatorStableId = 플레이어,
                    ActorKindCode = "Player", TriggerSourceCode = "PlayerDriven", TargetStableIds = new[] { 계획 },
                    OutcomeStableId = "outcome:personal-plan:" + 명령, PrimaryOutcomeCode = "PersonalPlanSet",
                    결과분류Code = Simulation행위결과분류Codes.성공,
                    변화의미Codes = new[] { Simulation행위변화의미Codes.플레이어진척변경 },
                    BeforeWorldRevision = 개정, AfterWorldRevision = 개정 + 1,
                    RuleRevision = Simulation개인계획Codes.RuleRevision + "/" + 정책,
                    SourceReferenceIds = 최초안정근거.Length == 0
                        ? new[] { 요청.ObjectiveStableId, 다음근거, "description-sha256:" + 해시(정규문구) }
                        : new[] { 요청.ObjectiveStableId, "description-sha256:" + 해시(정규문구) },
                });
                목표 = 요청.ObjectiveStableId; 문구 = 정규문구; 최초안정근거 = 다음근거;
                개정++; 행위 = 다음행위;
                var 결과 = 사본(); 명령들.Add(명령, (입력, 결과));
                return new Simulation개인계획ConfirmResult { State = 복사(결과) };
            }
        }

        private Simulation개인계획PreviewSnapshot 판정(Simulation개인계획PreviewRequest 요청)
        {
            var 차단 = new List<string>(); var 정규문구 = (요청.Description ?? string.Empty).Trim();
            if (요청.ActorStableId != 플레이어) 차단.Add("PersonalPlanActorNotAuthorized");
            if (요청.PlanStableId != 계획) 차단.Add("PersonalPlanUnknown");
            if (요청.ExpectedRevision != 개정) 차단.Add("PersonalPlanExpectedRevisionMismatch");
            if (개정 == long.MaxValue) 차단.Add("PersonalPlanRevisionExhausted");
            if (요청.ObjectiveStableId == null || !허용목표.Contains(요청.ObjectiveStableId)) 차단.Add("PersonalPlanObjectiveUnknown");
            if (정규문구.Length == 0 || 정규문구.Length > 문구상한 || 정규문구.Any(char.IsControl)) 차단.Add("PersonalPlanDescriptionInvalid");
            if (목표 == 요청.ObjectiveStableId && 문구 == 정규문구) 차단.Add("PersonalPlanUnchanged");
            return new Simulation개인계획PreviewSnapshot
            {
                WorldRevision = 개정, NormalizedDescription = 정규문구,
                BlockReasonCodes = 차단.ToArray(),
                InitialStabilityEligibilityWouldBeRecorded = 차단.Count == 0 && 최초안정근거.Length == 0,
            };
        }

        private Simulation개인계획Snapshot 사본()
        {
            var 원장 = 행위.Snapshot();
            return new Simulation개인계획Snapshot
            {
                PlanStableId = 계획, ObjectiveStableId = 목표, Description = 문구, WorldRevision = 개정,
                InitialStabilityEligibilityStableId = 최초안정근거, ActionLedger = 원장,
                StateHashSha256 = 해시(정규형(Simulation개인계획Codes.RuleRevision, 세계, 세션, 플레이어, 계획, 정책,
                    문구상한, 정규형(허용목표.OrderBy(x => x, StringComparer.Ordinal).Cast<object>().ToArray()),
                    개정, 목표, 문구, 최초안정근거, 원장.StateHashSha256)),
            };
        }
        private static Simulation개인계획Snapshot 복사(Simulation개인계획Snapshot 원본) => new Simulation개인계획Snapshot
        {
            PlanStableId = 원본.PlanStableId, ObjectiveStableId = 원본.ObjectiveStableId, Description = 원본.Description,
            WorldRevision = 원본.WorldRevision, InitialStabilityEligibilityStableId = 원본.InitialStabilityEligibilityStableId,
            StateHashSha256 = 원본.StateHashSha256, ActionLedger = Simulation행위발현Ledger.Restore(원본.ActionLedger).Snapshot(),
        };
        private static string 필수(string 값)
        {
            if (string.IsNullOrWhiteSpace(값) || 값 != 값.Trim()) throw new SimulationContractException("PersonalPlanIdentifierInvalid");
            return 값;
        }
        private static string 정규형(params object[] 값들) => string.Concat(값들.Select(값 =>
        {
            var 문자열 = Convert.ToString(값, CultureInfo.InvariantCulture) ?? string.Empty;
            return 문자열.Length.ToString(CultureInfo.InvariantCulture) + ":" + 문자열;
        }));
        private static string 해시(string 값)
        {
            using var 계산기 = SHA256.Create();
            return BitConverter.ToString(계산기.ComputeHash(Encoding.UTF8.GetBytes(값))).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
