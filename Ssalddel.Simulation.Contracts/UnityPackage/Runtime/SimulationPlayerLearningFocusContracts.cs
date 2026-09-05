using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation학습중점Codes
    {
        public const string SchemaVersion = "simulation-player-learning-focus.v1";
        public const string RuleRevision = "player-npc-learning-focus.r1";
        public const string NpcSource = "Npc";
        public const string PrimarySlot = "Primary";
        public const string Early = "Early";
        public const string Middle = "Middle";
        public const string Late = "Late";
        public const string Wood = "Wood";
        public const string Metal = "Metal";
        public const string HansActorStableId = "npc:hans";
        public const string HansFarmingCardStableId =
            "learning-card:npc:hans:farming.r1";
        public const string HansAxeCardStableId =
            "learning-card:npc:hans:axe.r1";
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "NPC 학습 카드와 절기 학습 구간의 불변 계약을 정의한다.",
        Boundary = "관계 해금·온라인 멘토 공유·Unity UI를 포함하지 않는다.")]
    public sealed class Simulation학습중점InitialState
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } =
            Simulation학습중점Codes.RuleRevision;
        public string ScheduleRevision { get; set; } = string.Empty;
        public Simulation학습구간Snapshot[] Segments { get; set; }
            = Array.Empty<Simulation학습구간Snapshot>();
        public Simulation학습카드DefinitionSnapshot[] Cards { get; set; }
            = Array.Empty<Simulation학습카드DefinitionSnapshot>();
        public string[] OwnedCardStableIds { get; set; } = Array.Empty<string>();
        public string ActiveCardStableId { get; set; } = string.Empty;
        public string ActiveFromSegmentStableId { get; set; } = string.Empty;
    }

    public sealed class Simulation학습구간Snapshot
    {
        public string SegmentStableId { get; set; } = string.Empty;
        public string SolarTermStableId { get; set; } = string.Empty;
        public string SolarTermRevision { get; set; } = string.Empty;
        public string PhaseCode { get; set; } = string.Empty;
        public int StartWorldTickInclusive { get; set; }
        public int EndWorldTickExclusive { get; set; }
    }

    public sealed class Simulation학습카드BindingSnapshot
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string DomainStableId { get; set; } = string.Empty;
        public string SkillStableId { get; set; } = string.Empty;
    }

    public sealed class Simulation학습카드DefinitionSnapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public string CardRevision { get; set; } = string.Empty;
        public string DefinitionHashSha256 { get; set; } = string.Empty;
        public string SourceKindCode { get; set; }
            = Simulation학습중점Codes.NpcSource;
        public string SourceActorStableId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string PrimaryFiveElementCode { get; set; } = string.Empty;
        public string[] SupportingFiveElementCodes { get; set; }
            = Array.Empty<string>();
        public Simulation학습카드BindingSnapshot[] Bindings { get; set; }
            = Array.Empty<Simulation학습카드BindingSnapshot>();
        public int UnderstandingDelta { get; set; } = 1;
        public string EffectRuleRevision { get; set; } = string.Empty;
    }

    public sealed class Simulation학습중점ChangeRequest
    {
        public Guid ClientRequestId { get; set; }
        public long ExpectedRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string CardStableId { get; set; } = string.Empty;
    }

    public sealed class Simulation학습중점PreviewSnapshot
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public long CurrentRevision { get; set; }
        public string CurrentSegmentStableId { get; set; } = string.Empty;
        public string ActiveCardStableId { get; set; } = string.Empty;
        public string RequestedCardStableId { get; set; } = string.Empty;
        public string EffectiveSegmentStableId { get; set; } = string.Empty;
        public int EffectiveWorldTick { get; set; }
        public bool AppliesAtCurrentBoundary { get; set; }
        public bool WouldReplacePendingChange { get; set; }
    }

    public sealed class Simulation학습중점PendingChangeSnapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public string EffectiveSegmentStableId { get; set; } = string.Empty;
        public int EffectiveWorldTick { get; set; }
        public Guid SourceClientRequestId { get; set; }
    }

    public sealed class Simulation학습중점ChangeReceiptSnapshot
    {
        public Guid ClientRequestId { get; set; }
        public long ExpectedRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string CardStableId { get; set; } = string.Empty;
        public string EffectiveSegmentStableId { get; set; } = string.Empty;
        public int EffectiveWorldTick { get; set; }
        public long ResultingRevision { get; set; }
    }

    public sealed class Simulation학습중점ActivationSnapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public string SegmentStableId { get; set; } = string.Empty;
        public int ActivatedWorldTick { get; set; }
        public Guid SourceClientRequestId { get; set; }
        public long ResultingRevision { get; set; }
    }

    public sealed class Simulation학습효과ReceiptSnapshot
    {
        public string ReceiptStableId { get; set; } = string.Empty;
        public string CardStableId { get; set; } = string.Empty;
        public string CardRevision { get; set; } = string.Empty;
        public string SourceActorStableId { get; set; } = string.Empty;
        public string SourceActionRecordStableId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string DomainStableId { get; set; } = string.Empty;
        public string SkillStableId { get; set; } = string.Empty;
        public string ResultCode { get; set; } = string.Empty;
        public int UnderstandingDelta { get; set; }
        public long AppliedWorldRevision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "한 플레이어의 보유 카드·주 슬롯·다음 학습 구간 예약 상태를 제공한다.",
        Boundary = "표현 사본은 Unity 배치나 실제 화면 증거가 아니다.")]
    public sealed class Simulation학습중점StateSnapshot
    {
        public string SchemaVersion { get; set; }
            = Simulation학습중점Codes.SchemaVersion;
        public string SessionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string RuleRevision { get; set; } = string.Empty;
        public string ScheduleRevision { get; set; } = string.Empty;
        public Simulation학습구간Snapshot[] Segments { get; set; }
            = Array.Empty<Simulation학습구간Snapshot>();
        public Simulation학습카드DefinitionSnapshot[] Cards { get; set; }
            = Array.Empty<Simulation학습카드DefinitionSnapshot>();
        public string[] OwnedCardStableIds { get; set; } = Array.Empty<string>();
        public string ActiveCardStableId { get; set; } = string.Empty;
        public string ActiveFromSegmentStableId { get; set; } = string.Empty;
        public Simulation학습중점PendingChangeSnapshot? PendingChange { get; set; }
        public Simulation학습중점ChangeReceiptSnapshot[] ChangeReceipts { get; set; }
            = Array.Empty<Simulation학습중점ChangeReceiptSnapshot>();
        public Simulation학습중점ActivationSnapshot[] ActivationHistory { get; set; }
            = Array.Empty<Simulation학습중점ActivationSnapshot>();
        public Simulation학습효과ReceiptSnapshot[] EffectReceipts { get; set; }
            = Array.Empty<Simulation학습효과ReceiptSnapshot>();
        public string StateHashSha256 { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class Simulation학습중점ProjectionSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string CurrentSegmentStableId { get; set; } = string.Empty;
        public string ActiveCardStableId { get; set; } = string.Empty;
        public string PendingCardStableId { get; set; } = string.Empty;
        public string PendingEffectiveSegmentStableId { get; set; } = string.Empty;
        public int PendingEffectiveWorldTick { get; set; } = -1;
        public Simulation학습카드DefinitionSnapshot[] OwnedCards { get; set; }
            = Array.Empty<Simulation학습카드DefinitionSnapshot>();
        public Simulation학습효과ReceiptSnapshot? LastEffectReceipt { get; set; }
        public string StateHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationNpc학습중점기여Request
    {
        public string PlayerStableId { get; set; } = string.Empty;
        public string CardStableId { get; set; } = string.Empty;
        public string CardRevision { get; set; } = string.Empty;
        public string CardDefinitionHashSha256 { get; set; } = string.Empty;
        public string SourceActorStableId { get; set; } = string.Empty;
        public string EffectReceiptStableId { get; set; } = string.Empty;
        public Simulation행위발현Record ActionRecord { get; set; }
            = new Simulation행위발현Record();
        public Simulation분야이해효과선Snapshot EffectLine { get; set; }
            = new Simulation분야이해효과선Snapshot();
    }
}
