using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation공동체방문자체류Codes
    {
        public const string RuleRevision = "community-visitor-stay.r1";
        public const string PlayableLoopStableId =
            "playable-loop:nature-camp-visitor-stay.v1";
        public const string WorldInteractionId = "WI-COMMUNITY-VISITOR-STAY";
        public const string 결정대기 = "AwaitingDecision";
        public const string 임시체류 = "TemporaryStay";
        public const string 거절 = "Rejected";
        public const string 임시체류수용 = "AcceptTemporaryStay";
        public const string 거절선택 = "Reject";
        public const string 환대확인 = "HospitalityAffirmed";
        public const string 경계보호 = "BoundaryProtected";
        public const string ExpectedRevisionMismatch =
            "CommunityVisitorStayExpectedRevisionMismatch";
        public const string VisitorUnknown = "CommunityVisitorUnknown";
        public const string VisitorAlreadyDecided = "CommunityVisitorAlreadyDecided";
        public const string CapacityUnavailable = "CommunityVisitorCapacityUnavailable";
        public const string DecisionInvalid = "CommunityVisitorDecisionInvalid";
        public const string CommandPayloadConflict =
            "CommunityVisitorStayCommandPayloadConflict";
    }

    public sealed class Simulation공동체방문자Definition
    {
        public string VisitorStableId { get; set; } = string.Empty;
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Nature 야영지 방문자 임시 체류 결정의 초기 권위 계약을 정의한다.",
        Boundary = "체류 기간·정식 편입·공간 배치를 포함하지 않는 첫 방문자 WI 계약이다.",
        WorldInteractionIds = new[] { Simulation공동체방문자체류Codes.WorldInteractionId })]
    public sealed class Simulation공동체방문자체류InitialStateRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string HostPlayerStableId { get; set; } = string.Empty;
        public long InitialWorldRevision { get; set; }
        public int GuestCapacity { get; set; }
        public int OccupiedGuestCapacity { get; set; }
        public Simulation공동체방문자Definition[] Visitors { get; set; }
            = Array.Empty<Simulation공동체방문자Definition>();
    }

    public sealed class Simulation공동체방문자Snapshot
    {
        public string VisitorStableId { get; set; } = string.Empty;
        public string StatusCode { get; set; } = Simulation공동체방문자체류Codes.결정대기;
        public string MindTraceCode { get; set; } = string.Empty;
    }

    public sealed class Simulation공동체방문자체류LedgerSnapshot
    {
        public string RuleRevision { get; set; } = Simulation공동체방문자체류Codes.RuleRevision;
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string HostPlayerStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public int GuestCapacity { get; set; }
        public int OccupiedGuestCapacity { get; set; }
        public Simulation공동체방문자Snapshot[] Visitors { get; set; }
            = Array.Empty<Simulation공동체방문자Snapshot>();
        public Simulation행위기록LedgerSnapshot ActionLedger { get; set; }
            = new Simulation행위기록LedgerSnapshot();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation공동체방문자체류PreviewRequest
    {
        public long ObservedWorldRevision { get; set; }
        public string VisitorStableId { get; set; } = string.Empty;
        public string DecisionCode { get; set; } = string.Empty;
    }

    public sealed class Simulation공동체방문자체류PreviewSnapshot
    {
        public long ObservedWorldRevision { get; set; }
        public string VisitorStableId { get; set; } = string.Empty;
        public string DecisionCode { get; set; } = string.Empty;
        public string ProjectedStatusCode { get; set; } = string.Empty;
        public string ProjectedMindTraceCode { get; set; } = string.Empty;
        public int RemainingGuestCapacity { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation공동체방문자체류ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public string VisitorStableId { get; set; } = string.Empty;
        public string DecisionCode { get; set; } = string.Empty;
    }

    public sealed class Simulation공동체방문자체류ConfirmResult
    {
        public Simulation공동체방문자체류LedgerSnapshot Ledger { get; set; }
            = new Simulation공동체방문자체류LedgerSnapshot();
        public Simulation행위발현Record ActionRecord { get; set; }
            = new Simulation행위발현Record();
        public bool Reused { get; set; }
    }

    public sealed class Simulation공동체방문자응대CardSnapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public long SourceWorldRevision { get; set; }
        public string VisitorStableId { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string MindTraceCode { get; set; } = string.Empty;
        public int RemainingGuestCapacity { get; set; }
    }
}
