using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation세계자원재생Codes
    {
        public const string WorldInteractionId = "WI-WORLD-RESOURCE-REGENERATE";
        public const string RuleRevision = "resource-regeneration.r1";
        public const string PlayerProgressionNotApplicableReason = "WorldDerivedResourceRegeneration";
        public const string ResourceAvailabilityChanged = "ResourceAvailabilityChanged";
        public const string 식물 = "Plant", 환경묶음 = "Loose";
        public const string 자연 = "Natural", 평탄화 = "Flattened", 건설 = "Construction", 도로 = "Road";
    }

    public sealed class Simulation자원재생Profile
    {
        public string ProfileStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string 자원StableId { get; set; } = string.Empty;
        public string 종류Code { get; set; } = Simulation세계자원재생Codes.식물;
        public int 성숙단계 { get; set; }
        public long 단계간Tick { get; set; }
        public int 생성확률Micro { get; set; }
        public int Tick당생성상한 { get; set; }
    }

    public sealed class Simulation자원재생Cell
    {
        public string CellStableId { get; set; } = string.Empty;
        public string ProfileStableId { get; set; } = string.Empty;
        public string 토지용도Code { get; set; } = Simulation세계자원재생Codes.자연;
        public long 중심Xmm { get; set; }
        public long 중심Zmm { get; set; }
    }

    public sealed class Simulation자원재생Node
    {
        public string NodeStableId { get; set; } = string.Empty;
        public string CellStableId { get; set; } = string.Empty;
        public int 성장단계 { get; set; }
        public long 다음성장Tick { get; set; }
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1, "신뢰된 자원 정책·셀·노드와 권위 Tick 입력을 정의한다.",
        Boundary = "생산 공간 정책·기존 Session·Save·Unity에 연결하지 않는다.",
        WorldInteractionIds = new[] { Simulation세계자원재생Codes.WorldInteractionId })]
    public sealed class Simulation세계자원재생InitialState
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string 권위주체StableId { get; set; } = string.Empty;
        public long WorldSeed { get; set; }
        public long WorldRevision { get; set; }
        public long WorldTick { get; set; }
        public Simulation자원재생Profile[] Profiles { get; set; } = Array.Empty<Simulation자원재생Profile>();
        public Simulation자원재생Cell[] Cells { get; set; } = Array.Empty<Simulation자원재생Cell>();
        public Simulation자원재생Node[] Nodes { get; set; } = Array.Empty<Simulation자원재생Node>();
    }

    // 이 요청은 인증된 Host 내부 경계다. StableId 자체를 네트워크 인증 수단으로 사용하지 않는다.
    public sealed class Simulation자원재생TickRequest
    {
        public string 권위주체StableId { get; set; } = string.Empty;
        public string TransitionId { get; set; } = string.Empty;
        public long WorldTick { get; set; }
        public long ExpectedRevision { get; set; }
    }

    public sealed class Simulation자원재생Preview
    {
        public long WorldRevision { get; set; }
        public string[] 변경노드StableIds { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public bool CanApply => BlockReasonCodes.Length == 0;
    }

    public sealed class Simulation자원재생조회Node
    {
        public string NodeStableId { get; set; } = string.Empty;
        public string CellStableId { get; set; } = string.Empty;
        public string ProfileStableId { get; set; } = string.Empty;
        public string 자원StableId { get; set; } = string.Empty;
        public string 종류Code { get; set; } = string.Empty;
        public long Xmm { get; set; }
        public long Zmm { get; set; }
        public int 성장단계 { get; set; }
        public long 다음성장Tick { get; set; }
        public bool 채집가능 { get; set; }
    }

    public sealed class Simulation세계자원재생Snapshot
    {
        public long WorldRevision { get; set; }
        public long WorldTick { get; set; }
        public string 정책HashSha256 { get; set; } = string.Empty;
        public string StateHashSha256 { get; set; } = string.Empty;
        public Simulation자원재생조회Node[] Nodes { get; set; } = Array.Empty<Simulation자원재생조회Node>();
        public Simulation행위기록LedgerSnapshot ActionLedger { get; set; } = new Simulation행위기록LedgerSnapshot();
    }
    public sealed class Simulation자원재생TickResult
    {
        public bool Reused { get; set; }
        public Simulation세계자원재생Snapshot State { get; set; } = new Simulation세계자원재생Snapshot();
        public Simulation분야성장적용Snapshot 분야성장적용 { get; set; }
            = new Simulation분야성장적용Snapshot();
    }
}
