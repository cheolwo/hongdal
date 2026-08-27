namespace Ssalddel.Simulation.Contracts
{
    /// <summary>
    /// 세계 공통 대기 상태를 켜는 선택형 세션 입력이다. 실제 기상 관측값을
    /// 제출하지 않고 서버가 승인한 결정적 프로필만 지정한다.
    /// </summary>
    public sealed class SimulationAtmosphereInitialStateRequest
    {
        public string ProfileStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string ClockSourceCode { get; set; } = string.Empty;
    }

    public sealed class SimulationAtmosphereStateSnapshot
    {
        public bool IsEnabled { get; set; }
        public string ProfileStableId { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
        public string ClockSourceCode { get; set; } = string.Empty;
        public string ScopeCode { get; set; } = "World";
        public string WeatherCode { get; set; } = "Clear";
        public string NextWeatherCode { get; set; } = "Clear";
        public int TransitionProgressPermille { get; set; }
        public int CloudCoverPermille { get; set; }
        public int PrecipitationPermille { get; set; }
        public int WindIntensityPermille { get; set; }
        public int LightningSequenceIndex { get; set; }
        public int CycleIndex { get; set; }
        public int ElapsedSecondsInCycle { get; set; }
    }
}
