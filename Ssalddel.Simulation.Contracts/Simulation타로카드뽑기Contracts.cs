using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation타로DeckCodes
    {
        public const string StarterDeckStableId = "tarot-deck:starter-12";
        public const string StarterDeckRevision = "tarot-deck:starter-12.r1";
        public const string DrawRuleRevision = "tarot-draw-rule:r1";
    }

    public sealed class Simulation타로DeckCardCopySnapshot
    {
        public string CardCopyStableId { get; set; } = string.Empty;
        public int CopyNumber { get; set; }
        public SimulationTurnCardSnapshot Card { get; set; }
            = new SimulationTurnCardSnapshot();
    }

    public sealed class Simulation타로CardOfferSnapshot
    {
        public string OfferStableId { get; set; } = string.Empty;
        public int OfferSlotNumber { get; set; }
        public string CardCopyStableId { get; set; } = string.Empty;
        public string OrientationCode { get; set; } = string.Empty;
        public SimulationTurnCardSnapshot Card { get; set; }
            = new SimulationTurnCardSnapshot();
    }

    public sealed class Simulation타로DrawSnapshot
    {
        public string DrawStableId { get; set; } = string.Empty;
        public string DeckStableId { get; set; } = string.Empty;
        public string DeckRevision { get; set; } = string.Empty;
        public string DrawRuleRevision { get; set; } = string.Empty;
        public int TurnNumber { get; set; }
        public string TurnHistoryHash { get; set; } = string.Empty;
        public Simulation타로CardOfferSnapshot[] Offers { get; set; }
            = Array.Empty<Simulation타로CardOfferSnapshot>();
    }

    public sealed class Simulation타로CardSelectionRequest
    {
        public string OfferStableId { get; set; } = string.Empty;
        public string CardStableId { get; set; } = string.Empty;
        public string OrientationCode { get; set; } = string.Empty;
    }
}
