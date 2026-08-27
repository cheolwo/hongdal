namespace Ssalddel.WorkflowRules.Contracts
{
    public static class WorldAtmosphereRuleRevisions
    {
        public const string R1 = "world-atmosphere.r1";
    }

    public static class WorldAtmosphereProfileCodes
    {
        public const string NatureNightDay2FixtureR1 =
            "world-atmosphere:nature-night-day2.fixture.r1";
    }

    public static class WorldAtmosphereClockSourceCodes
    {
        public const string NatureCycleClock = "NatureCycleClock";
    }

    public static class WorldAtmosphereScopeCodes
    {
        public const string World = "World";
    }

    public static class WorldWeatherCodes
    {
        public const string Clear = "Clear";
        public const string Cloudy = "Cloudy";
        public const string Rain = "Rain";
        public const string Thunderstorm = "Thunderstorm";

        public static bool IsKnown(string value)
            => value == Clear || value == Cloudy || value == Rain
                || value == Thunderstorm;
    }

    public sealed class WorldAtmosphereProjection
    {
        public string WeatherCode { get; set; } = WorldWeatherCodes.Clear;
        public string NextWeatherCode { get; set; } = WorldWeatherCodes.Clear;
        public int TransitionProgressPermille { get; set; }
        public int CloudCoverPermille { get; set; }
        public int PrecipitationPermille { get; set; }
        public int WindIntensityPermille { get; set; }
        public int LightningSequenceIndex { get; set; }
    }
}
