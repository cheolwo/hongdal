namespace Ssalddel.WorkflowRules.Contracts
{
    public static class NatureSurvivalProfileCodes
    {
        public const string RealtimeR1 = "nature-survival.realtime.r1";
    }

    public static class NatureSurvivalClockPhaseCodes
    {
        public const string Daylight = "Daylight";
        public const string Dusk = "Dusk";
        public const string Night = "Night";
        public const string Dawn = "Dawn";
    }

    public sealed class NatureSurvivalClockProjection
    {
        public int CycleIndex { get; set; }
        public int ElapsedSecondsInCycle { get; set; }
        public string PhaseCode { get; set; } = NatureSurvivalClockPhaseCodes.Daylight;
        public int CompletedCycleCount { get; set; }
    }
}
