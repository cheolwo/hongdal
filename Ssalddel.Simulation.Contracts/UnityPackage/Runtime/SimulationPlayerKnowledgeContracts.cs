using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation플레이어지식Codes
    {
        public const string RuleRevision = "player-knowledge.r1";
        public const string PlayableLoopStableId =
            "playable-loop:nature-basic-herbal-recovery.v1";
        public const string 지식습득WorldInteractionId = "WI-ACTOR-03";
        public const string 기초약초차RecipeStableId =
            "recipe:nature:basic-herbal-tea.v1";

        public const string ExpectedRevisionMismatch =
            "PlayerKnowledgeExpectedRevisionMismatch";
        public const string PlayerMismatch = "PlayerKnowledgePlayerMismatch";
        public const string RecipeUnknown = "PlayerKnowledgeRecipeUnknown";
        public const string KnowledgeSourceUnavailable =
            "PlayerKnowledgeSourceUnavailable";
        public const string CommandPayloadConflict =
            "PlayerKnowledgeCommandPayloadConflict";
    }

    public sealed class Simulation처방지식SourceDefinition
    {
        public string KnowledgeSourceStableId { get; set; } = string.Empty;
        public bool IsAccessible { get; set; }
        public string[] ApprovedRecipeStableIds { get; set; }
            = Array.Empty<string>();
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "플레이어별 처방 지식 원장과 승인된 지식 출처의 초기 권위 계약을 정의한다.",
        Boundary = "저장·RemoteHost·Unity 표현을 포함하지 않는 Logic E1~E3 순수 계약이다.")]
    public sealed class Simulation플레이어지식InitialStateRequest
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public long InitialWorldRevision { get; set; }
        public string[] KnownRecipeStableIds { get; set; }
            = Array.Empty<string>();
        public Simulation처방지식SourceDefinition[] KnowledgeSources { get; set; }
            = Array.Empty<Simulation처방지식SourceDefinition>();
    }

    public sealed class Simulation플레이어지식LedgerSnapshot
    {
        public string RuleRevision { get; set; }
            = Simulation플레이어지식Codes.RuleRevision;
        public string WorldStableId { get; set; } = string.Empty;
        public string SessionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public string[] KnownRecipeStableIds { get; set; }
            = Array.Empty<string>();
        public Simulation행위기록LedgerSnapshot ActionLedger { get; set; }
            = new Simulation행위기록LedgerSnapshot();
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class Simulation지식습득PreviewRequest
    {
        public long ObservedWorldRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string RecipeStableId { get; set; } = string.Empty;
        public string KnowledgeSourceStableId { get; set; } = string.Empty;
    }

    public sealed class Simulation지식습득PreviewSnapshot
    {
        public string WorldInteractionId { get; set; }
            = Simulation플레이어지식Codes.지식습득WorldInteractionId;
        public long ObservedWorldRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string RecipeStableId { get; set; } = string.Empty;
        public string KnowledgeSourceStableId { get; set; } = string.Empty;
        public bool AlreadyKnown { get; set; }
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation지식습득ConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedWorldRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string RecipeStableId { get; set; } = string.Empty;
        public string KnowledgeSourceStableId { get; set; } = string.Empty;
    }

    public sealed class Simulation지식습득ConfirmResult
    {
        public Simulation플레이어지식LedgerSnapshot KnowledgeLedger { get; set; }
            = new Simulation플레이어지식LedgerSnapshot();
        public Simulation행위발현Record? ActionRecord { get; set; }
        public bool Added { get; set; }
        public bool Reused { get; set; }
    }
}
