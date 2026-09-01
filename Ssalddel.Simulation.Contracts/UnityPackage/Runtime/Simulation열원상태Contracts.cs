using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation열원상태Codes
    {
        public const string WorldInteractionId = "WI-HEAT-SOURCE-STATE-CHANGE";
        public const string PlayableLoopStableId = "playable-loop:nature-night-day2.v1";
        public const string RuleRevision = "heat-source-state.r1";
        public const string PlayerProgressionNotApplicableReason =
            "HeatSourceProgressPolicyUndefined";
        public const string 점화 = "Ignite", 연료추가 = "AddFuel", 소화 = "Extinguish";
        public const string 꺼짐 = "Off", 잔불 = "Smoldering", 연소중 = "Burning";
    }

    public sealed class Simulation연료Definition
    {
        public string FuelStableId { get; set; } = string.Empty;
        public long UnitEnergy { get; set; }
        public long Quantity { get; set; }
    }

    /// <summary>신뢰된 초기화 경계 전용. 클라이언트 명령으로 정책/재고를 받지 않는다.</summary>
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "열원·연료·접근 권한의 초기 상태 계약을 정의한다.",
        Boundary = "실행 수명 Fixture이며 Save·HTTP·실제 연소 시간 계약이 아니다.",
        WorldInteractionIds = new[] { Simulation열원상태Codes.WorldInteractionId })]
    public sealed class Simulation열원InitialState
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string HeatSourceStableId { get; set; } = string.Empty;
        public string PolicyRevision { get; set; } = string.Empty;
        public string StatusCode { get; set; } = Simulation열원상태Codes.꺼짐;
        public long WorldRevision { get; set; }
        public long Energy { get; set; }
        public long Capacity { get; set; }
        public bool Accessible { get; set; }
        public bool HasBasicSurvivalAbility { get; set; }
        public Simulation연료Definition[] Fuels { get; set; } = Array.Empty<Simulation연료Definition>();
    }

    public class Simulation열원PreviewRequest
    {
        public string ActorStableId { get; set; } = string.Empty;
        public string HeatSourceStableId { get; set; } = string.Empty;
        public string OperationCode { get; set; } = string.Empty;
        public string FuelStableId { get; set; } = string.Empty;
        public long Quantity { get; set; }
        public long ExpectedRevision { get; set; }
    }

    public sealed class Simulation열원ConfirmRequest : Simulation열원PreviewRequest
    {
        public string CommandId { get; set; } = string.Empty;
    }

    public sealed class Simulation열원PreviewSnapshot
    {
        public long WorldRevision { get; set; }
        public string ProjectedStatusCode { get; set; } = string.Empty;
        public long ProjectedEnergy { get; set; }
        public bool CanConfirm => BlockReasonCodes.Length == 0;
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation열원LedgerSnapshot
    {
        public Simulation열원InitialState State { get; set; } = new Simulation열원InitialState();
        public Simulation행위기록LedgerSnapshot ActionLedger { get; set; } = new Simulation행위기록LedgerSnapshot();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation열원ConfirmResult
    {
        public Simulation열원LedgerSnapshot Ledger { get; set; } = new Simulation열원LedgerSnapshot();
        public bool Reused { get; set; }
    }
}
