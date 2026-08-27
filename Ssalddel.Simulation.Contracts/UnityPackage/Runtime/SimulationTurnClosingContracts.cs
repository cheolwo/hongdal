using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationTurnCardKindCodes
    {
        public const string Tarot = "Tarot";
        public const string Philosophy = "Philosophy";
        public const string Culture = "Culture";
    }

    public static class SimulationTurnCardEffectTimingCodes
    {
        public const string NextTurn = "NextTurn";
    }

    public static class SimulationTurnCardEffectCodes
    {
        public const string EmpressProductionGrowth = "EmpressProductionGrowth";
        public const string ChariotFastTransport = "ChariotFastTransport";
        public const string JusticeTradeBalance = "JusticeTradeBalance";
        public const string TemperanceFlowBalance = "TemperanceFlowBalance";
        public const string BeginnerMind = "BeginnerMind";
        public const string IntegratedProgress = "IntegratedProgress";
        public const string LocalContextAwareness = "LocalContextAwareness";
    }

    public sealed class SimulationTurnCardSnapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public string CardRevision { get; set; } = string.Empty;
        public string CardKindCode { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string OfferStableId { get; set; } = string.Empty;
        public string OrientationCode { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string EffectTimingCode { get; set; } = SimulationTurnCardEffectTimingCodes.NextTurn;
        public string EffectCode { get; set; } = string.Empty;
        public string TargetStatCode { get; set; } = string.Empty;
        public int StatDelta { get; set; }
        public string SourceStableId { get; set; } = string.Empty;
        public string RegionKey { get; set; } = string.Empty;
        public DateTimeOffset? AvailableFromGameDate { get; set; }
        public DateTimeOffset? AvailableThroughGameDate { get; set; }
        public string CalendarRevision { get; set; } = string.Empty;
        public string EffectRuleRevision { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public DateTimeOffset? EvidenceCheckedAtUtc { get; set; }
    }

    public sealed class SimulationTurnClosingContextSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public int TurnNumber { get; set; }
        public DateTimeOffset GameDate { get; set; }
        public long Revision { get; set; }
        public int PendingTaskCount { get; set; }
        public bool CanCloseTurn { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public SimulationTurnCardSnapshot[] AvailableCards { get; set; }
            = Array.Empty<SimulationTurnCardSnapshot>();
        public Simulation타로DrawSnapshot TarotDraw { get; set; }
            = new Simulation타로DrawSnapshot();
        public SimulationTarotContextStateSnapshot TarotContext { get; set; }
            = new SimulationTarotContextStateSnapshot();
    }

    public sealed class SimulationTurnClosingPreviewRequest
    {
        public long ExpectedRevision { get; set; }
        public string[] SelectedCardStableIds { get; set; } = Array.Empty<string>();
        public Simulation타로CardSelectionRequest? SelectedTarotCard { get; set; }
            = null;
        public bool DeactivateActiveMajorArcana { get; set; }
            = false;
    }

    public sealed class SimulationTurnClosingPreviewSnapshot
    {
        public string PreviewStableId { get; set; } = string.Empty;
        public long BaseRevision { get; set; }
        public int ClosingTurnNumber { get; set; }
        public DateTimeOffset ClosingGameDate { get; set; }
        public int NextTurnNumber { get; set; }
        public DateTimeOffset NextGameDate { get; set; }
        public int PendingTaskCount { get; set; }
        public SimulationTurnCardSnapshot[] SelectedCards { get; set; }
            = Array.Empty<SimulationTurnCardSnapshot>();
        public Simulation메이저아르카나방향판정Snapshot? MajorArcanaDirectionDecision
            { get; set; }
        public bool DeactivatesActiveMajorArcana { get; set; }
            = false;
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationTurnClosingConfirmRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationTurnClosingPreviewRequest Preview { get; set; }
            = new SimulationTurnClosingPreviewRequest();
    }

    public sealed class SimulationTurnClosingSnapshot
    {
        public string TurnClosingStableId { get; set; } = string.Empty;
        public int ClosedTurnNumber { get; set; }
        public DateTimeOffset ClosedGameDate { get; set; }
        public int ResultingWorldTick { get; set; }
        public long ResultingRevision { get; set; }
        public SimulationTurnCardSnapshot[] SelectedCards { get; set; }
            = Array.Empty<SimulationTurnCardSnapshot>();
        public Simulation메이저아르카나방향판정Snapshot? MajorArcanaDirectionDecision
            { get; set; }
        public bool DeactivatedActiveMajorArcana { get; set; }
            = false;
    }

    public sealed class SimulationActiveTurnCardEffectSnapshot
    {
        public string CardStableId { get; set; } = string.Empty;
        public string CardRevision { get; set; } = string.Empty;
        public string CardKindCode { get; set; } = string.Empty;
        public string CardCopyStableId { get; set; } = string.Empty;
        public string OfferStableId { get; set; } = string.Empty;
        public string OrientationCode { get; set; } = string.Empty;
        public string EffectCode { get; set; } = string.Empty;
        public string TargetStatCode { get; set; } = string.Empty;
        public int StatDelta { get; set; }
        public int ActiveTurnNumber { get; set; }
        public string SourceTurnClosingStableId { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string RegionKey { get; set; } = string.Empty;
        public string CalendarRevision { get; set; } = string.Empty;
        public string EffectRuleRevision { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public DateTimeOffset? EvidenceCheckedAtUtc { get; set; }
    }
}
