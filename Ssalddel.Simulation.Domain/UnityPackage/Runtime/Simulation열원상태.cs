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
        "점화·연료 추가·소화를 단일 열원 상태 변경으로 판정한다.",
        Boundary = "독립 실행 수명 원장이다. Session·Save·Tick·표현 엔진은 연결하지 않는다.",
        WorldInteractionIds = new[] { Simulation열원상태Codes.WorldInteractionId })]
    public sealed class Simulation열원상태Aggregate
    {
        private readonly object 동기화 = new object();
        private Simulation열원InitialState 상태;
        private Simulation행위발현Ledger 행위원장;
        private readonly Dictionary<string, (string 입력, Simulation열원LedgerSnapshot 결과)> 확정명령
            = new Dictionary<string, (string, Simulation열원LedgerSnapshot)>(StringComparer.Ordinal);

        public Simulation열원상태Aggregate(Simulation열원InitialState 초기상태)
        {
            if (초기상태 == null) throw new SimulationContractException("HeatInitialStateRequired");
            상태 = 복사(초기상태);
            foreach (var 식별자 in new[] { 상태.WorldStableId, 상태.SessionStableId,
                상태.PlayerStableId, 상태.HeatSourceStableId, 상태.PolicyRevision }) 필수(식별자);
            if (상태.WorldRevision < 0 || 상태.Capacity <= 0 || 상태.Energy < 0 || 상태.Energy > 상태.Capacity
                || !new[] { "Off", "Smoldering", "Burning" }.Contains(상태.StatusCode)
                || (상태.StatusCode == "Off" && 상태.Energy != 0)
                || (상태.StatusCode == "Burning" && 상태.Energy == 0))
                throw new SimulationContractException("HeatInitialStateInvalid");
            var 식별자집합 = new HashSet<string>(StringComparer.Ordinal);
            foreach (var 연료 in 상태.Fuels)
            {
                필수(연료.FuelStableId);
                if (!식별자집합.Add(연료.FuelStableId) || 연료.UnitEnergy <= 0 || 연료.Quantity < 0)
                    throw new SimulationContractException("HeatFuelPolicyInvalid");
            }
            행위원장 = new Simulation행위발현Ledger(상태.WorldStableId);
        }

        public Simulation열원LedgerSnapshot Snapshot()
        {
            lock (동기화) return 상태사본();
        }

        public Simulation열원PreviewSnapshot Preview(Simulation열원PreviewRequest 요청)
        {
            if (요청 == null) throw new SimulationContractException("HeatRequestRequired");
            lock (동기화) return 판정(요청);
        }

        public Simulation열원ConfirmResult Confirm(Simulation열원ConfirmRequest 요청)
        {
            if (요청 == null) throw new SimulationContractException("HeatRequestRequired");
            필수(요청.CommandId);
            lock (동기화)
            {
                var 입력 = 정규형(요청.ActorStableId, 요청.HeatSourceStableId, 요청.OperationCode,
                    요청.FuelStableId, 요청.Quantity, 요청.ExpectedRevision);
                if (확정명령.TryGetValue(요청.CommandId, out var 기존))
                {
                    if (기존.입력 != 입력) throw new SimulationConflictException("HeatCommandPayloadConflict");
                    return new Simulation열원ConfirmResult { Ledger = 복사(기존.결과), Reused = true };
                }
                var 미리보기 = 판정(요청);
                if (!미리보기.CanConfirm) throw new SimulationConflictException(미리보기.BlockReasonCodes[0]);

                // 모든 검사와 후보 계산을 끝낸 뒤 한 번에 교체한다. Append 실패도 기존 상태를 남긴다.
                var 다음상태 = 복사(상태);
                다음상태.StatusCode = 미리보기.ProjectedStatusCode;
                다음상태.Energy = 미리보기.ProjectedEnergy;
                다음상태.WorldRevision++;
                if (요청.OperationCode != Simulation열원상태Codes.소화)
                    다음상태.Fuels.Single(x => x.FuelStableId == 요청.FuelStableId).Quantity -= 요청.Quantity;
                var 다음원장 = Simulation행위발현Ledger.Restore(행위원장.Snapshot());
                다음원장.Append(new Simulation행위발현Record
                {
                    WorldStableId = 상태.WorldStableId, SessionStableId = 상태.SessionStableId,
                    PlayableLoopStableId = Simulation열원상태Codes.PlayableLoopStableId,
                    WorldInteractionId = Simulation열원상태Codes.WorldInteractionId,
                    CommandId = 요청.CommandId, TriggerSourceCode = "PlayerDriven",
                    InitiatorStableId = 요청.ActorStableId, ActorStableId = 요청.ActorStableId,
                    ActorKindCode = "Player", TargetStableIds = new[] { 상태.HeatSourceStableId },
                    OutcomeStableId = "outcome:heat-source:" + 요청.CommandId,
                    PrimaryOutcomeCode = "HeatSourceStateChanged",
                    결과분류Code = Simulation행위결과분류Codes.성공,
                    변화의미Codes = 요청.OperationCode == Simulation열원상태Codes.소화
                        ? new[] { "HeatSourceStateChanged" }
                        : new[] { "HeatSourceStateChanged", Simulation행위변화의미Codes.재고변경 },
                    SourceReferenceIds = new[] { 상태.PolicyRevision, "operation:" + 요청.OperationCode,
                        "fuel:" + 요청.FuelStableId, "quantity:" + 요청.Quantity.ToString(CultureInfo.InvariantCulture) },
                    BeforeWorldRevision = 상태.WorldRevision, AfterWorldRevision = 다음상태.WorldRevision,
                    RuleRevision = Simulation열원상태Codes.RuleRevision + "/" + 상태.PolicyRevision,
                });
                상태 = 다음상태;
                행위원장 = 다음원장;
                var 결과 = 상태사본();
                확정명령.Add(요청.CommandId, (입력, 결과));
                return new Simulation열원ConfirmResult { Ledger = 복사(결과) };
            }
        }

        private Simulation열원PreviewSnapshot 판정(Simulation열원PreviewRequest 요청)
        {
            var 차단 = new List<string>();
            if (요청.ExpectedRevision != 상태.WorldRevision) 차단.Add("HeatExpectedRevisionMismatch");
            if (요청.ActorStableId != 상태.PlayerStableId) 차단.Add("HeatActorNotAuthorized");
            if (요청.HeatSourceStableId != 상태.HeatSourceStableId) 차단.Add("HeatSourceUnknown");
            if (!상태.Accessible) 차단.Add("HeatSourceInaccessible");
            if (상태.WorldRevision == long.MaxValue) 차단.Add("HeatRevisionExhausted");
            var 다음상태 = 상태.StatusCode;
            var 다음에너지 = 상태.Energy;
            if (요청.OperationCode == Simulation열원상태Codes.소화)
            {
                if (상태.StatusCode == "Off") 차단.Add("HeatAlreadyOff");
                if (!string.IsNullOrEmpty(요청.FuelStableId) || 요청.Quantity != 0) 차단.Add("HeatExtinguishPayloadInvalid");
                다음상태 = "Off";
                다음에너지 = 0;
            }
            else if (요청.OperationCode == Simulation열원상태Codes.점화 || 요청.OperationCode == Simulation열원상태Codes.연료추가)
            {
                if (요청.OperationCode == Simulation열원상태Codes.점화)
                {
                    if (상태.StatusCode == "Burning") 차단.Add("HeatAlreadyBurning");
                    if (!상태.HasBasicSurvivalAbility) 차단.Add("HeatBasicSurvivalRequired");
                }
                else if (상태.StatusCode != "Burning") 차단.Add("HeatIgnitionRequired");
                var 연료 = 상태.Fuels.FirstOrDefault(x => x.FuelStableId == 요청.FuelStableId);
                if (연료 == null) 차단.Add("HeatFuelNotApproved");
                if (요청.Quantity <= 0) 차단.Add("HeatFuelQuantityInvalid");
                else if (연료 != null)
                {
                    if (요청.Quantity > 연료.Quantity) 차단.Add("HeatFuelInsufficient");
                    // 나눗셈으로 먼저 용량을 검사해 곱셈·덧셈 overflow를 막는다.
                    if (요청.Quantity > (상태.Capacity - 상태.Energy) / 연료.UnitEnergy)
                        차단.Add("HeatCapacityExceeded");
                    else 다음에너지 += 요청.Quantity * 연료.UnitEnergy;
                }
                다음상태 = "Burning";
            }
            else 차단.Add("HeatOperationInvalid");
            return new Simulation열원PreviewSnapshot
            {
                WorldRevision = 상태.WorldRevision, BlockReasonCodes = 차단.ToArray(),
                ProjectedStatusCode = 차단.Count == 0 ? 다음상태 : 상태.StatusCode,
                ProjectedEnergy = 차단.Count == 0 ? 다음에너지 : 상태.Energy,
            };
        }

        private Simulation열원LedgerSnapshot 상태사본()
        {
            var 원장 = 행위원장.Snapshot();
            var 정렬연료 = 상태.Fuels.OrderBy(x => x.FuelStableId, StringComparer.Ordinal);
            var 원문 = 정규형(Simulation열원상태Codes.RuleRevision, 상태.WorldStableId, 상태.SessionStableId,
                상태.PlayerStableId, 상태.HeatSourceStableId, 상태.PolicyRevision, 상태.WorldRevision,
                상태.StatusCode, 상태.Energy, 상태.Capacity, 상태.Accessible, 상태.HasBasicSurvivalAbility,
                string.Concat(정렬연료.Select(x => 정규형(x.FuelStableId, x.UnitEnergy, x.Quantity))), 원장.StateHashSha256);
            using var 해시 = SHA256.Create();
            return new Simulation열원LedgerSnapshot
            {
                State = 복사(상태), ActionLedger = 원장,
                StateHashSha256 = BitConverter.ToString(해시.ComputeHash(Encoding.UTF8.GetBytes(원문)))
                    .Replace("-", string.Empty).ToLowerInvariant(),
            };
        }

        private static string 정규형(params object[] 값들) => string.Concat(값들.Select(값 =>
        {
            var 문자열 = Convert.ToString(값, CultureInfo.InvariantCulture) ?? string.Empty;
            return 문자열.Length.ToString(CultureInfo.InvariantCulture) + ":" + 문자열;
        }));

        private static void 필수(string 값)
        {
            if (string.IsNullOrWhiteSpace(값) || 값 != 값.Trim()) throw new SimulationContractException("HeatIdentifierInvalid");
        }

        private static Simulation열원InitialState 복사(Simulation열원InitialState 원본) => new Simulation열원InitialState
        {
            WorldStableId = 원본.WorldStableId, SessionStableId = 원본.SessionStableId,
            PlayerStableId = 원본.PlayerStableId, HeatSourceStableId = 원본.HeatSourceStableId,
            PolicyRevision = 원본.PolicyRevision, StatusCode = 원본.StatusCode, WorldRevision = 원본.WorldRevision,
            Energy = 원본.Energy, Capacity = 원본.Capacity, Accessible = 원본.Accessible,
            HasBasicSurvivalAbility = 원본.HasBasicSurvivalAbility,
            Fuels = (원본.Fuels ?? throw new SimulationContractException("HeatFuelPolicyRequired"))
                .Select(x => x == null ? throw new SimulationContractException("HeatFuelPolicyInvalid")
                    : new Simulation연료Definition { FuelStableId = x.FuelStableId, UnitEnergy = x.UnitEnergy, Quantity = x.Quantity })
                .OrderBy(x => x.FuelStableId, StringComparer.Ordinal).ToArray(),
        };

        private static Simulation열원LedgerSnapshot 복사(Simulation열원LedgerSnapshot 원본) => new Simulation열원LedgerSnapshot
        {
            State = 복사(원본.State), StateHashSha256 = 원본.StateHashSha256,
            ActionLedger = Simulation행위발현Ledger.Restore(원본.ActionLedger).Snapshot(),
        };
    }
}
