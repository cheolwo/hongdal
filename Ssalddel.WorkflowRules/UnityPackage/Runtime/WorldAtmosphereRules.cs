using System;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.WorkflowRules
{
    /// <summary>
    /// 첫 Nature 폐루프에서 사용할 결정적 세계 공통 기상 Fixture다.
    /// 현실 기상 관측이나 Unity frame time을 입력으로 사용하지 않는다.
    /// </summary>
    public static class WorldAtmosphereRules
    {
        public const int CycleSeconds = 1_200;
        private const int TransitionSeconds = 30;

        public static WorldAtmosphereProjection Evaluate(
            string profileStableId,
            int scenarioSeed,
            int cycleIndex,
            int elapsedSecondsInCycle)
        {
            if (!string.Equals(profileStableId,
                    WorldAtmosphereProfileCodes.NatureNightDay2FixtureR1,
                    StringComparison.Ordinal))
                throw new ArgumentException("WorldAtmosphereProfileUnknown",
                    nameof(profileStableId));
            if (cycleIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(cycleIndex));
            if (elapsedSecondsInCycle < 0
                || elapsedSecondsInCycle >= CycleSeconds)
                throw new ArgumentOutOfRangeException(nameof(elapsedSecondsInCycle));

            var segment = SegmentAt(elapsedSecondsInCycle);
            var next = SegmentAt(segment.EndExclusive % CycleSeconds);
            var transitionStartsAt = Math.Max(segment.Start,
                segment.EndExclusive - TransitionSeconds);
            var transition = elapsedSecondsInCycle < transitionStartsAt
                ? 0
                : Math.Min(1_000, (elapsedSecondsInCycle - transitionStartsAt)
                    * 1_000 / Math.Max(1,
                        segment.EndExclusive - transitionStartsAt));

            var cloud = Blend(CloudCover(segment.WeatherCode),
                CloudCover(next.WeatherCode), transition);
            var precipitation = Blend(Precipitation(segment.WeatherCode),
                Precipitation(next.WeatherCode), transition);
            var wind = Blend(Wind(segment.WeatherCode),
                Wind(next.WeatherCode), transition);
            var lightningSequenceIndex = 0;
            if (string.Equals(segment.WeatherCode,
                    WorldWeatherCodes.Thunderstorm, StringComparison.Ordinal))
            {
                var seedOffset = PositiveModulo(scenarioSeed + cycleIndex * 17, 23);
                lightningSequenceIndex = 1 + Math.Max(0,
                    elapsedSecondsInCycle - segment.Start + seedOffset) / 37;
            }

            return new WorldAtmosphereProjection
            {
                WeatherCode = segment.WeatherCode,
                NextWeatherCode = next.WeatherCode,
                TransitionProgressPermille = transition,
                CloudCoverPermille = cloud,
                PrecipitationPermille = precipitation,
                WindIntensityPermille = wind,
                LightningSequenceIndex = lightningSequenceIndex,
            };
        }

        private static Segment SegmentAt(int second)
        {
            if (second < 450) return new Segment(0, 450, WorldWeatherCodes.Clear);
            if (second < 600) return new Segment(450, 600, WorldWeatherCodes.Cloudy);
            if (second < 820) return new Segment(600, 820, WorldWeatherCodes.Rain);
            if (second < 990) return new Segment(820, 990,
                WorldWeatherCodes.Thunderstorm);
            if (second < 1_110) return new Segment(990, 1_110,
                WorldWeatherCodes.Rain);
            return new Segment(1_110, CycleSeconds, WorldWeatherCodes.Clear);
        }

        private static int CloudCover(string weatherCode)
            => weatherCode == WorldWeatherCodes.Clear ? 80
                : weatherCode == WorldWeatherCodes.Cloudy ? 760
                : weatherCode == WorldWeatherCodes.Rain ? 930 : 1_000;

        private static int Precipitation(string weatherCode)
            => weatherCode == WorldWeatherCodes.Rain ? 720
                : weatherCode == WorldWeatherCodes.Thunderstorm ? 1_000 : 0;

        private static int Wind(string weatherCode)
            => weatherCode == WorldWeatherCodes.Clear ? 100
                : weatherCode == WorldWeatherCodes.Cloudy ? 260
                : weatherCode == WorldWeatherCodes.Rain ? 480 : 820;

        private static int Blend(int from, int to, int permille)
            => from + (to - from) * permille / 1_000;

        private static int PositiveModulo(int value, int divisor)
        {
            var result = value % divisor;
            return result < 0 ? result + divisor : result;
        }

        private readonly struct Segment
        {
            public Segment(int start, int endExclusive, string weatherCode)
            {
                Start = start;
                EndExclusive = endExclusive;
                WeatherCode = weatherCode;
            }

            public int Start { get; }
            public int EndExclusive { get; }
            public string WeatherCode { get; }
        }
    }
}
