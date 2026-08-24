using System;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.WorkflowRules
{
    /// <summary>
    /// 서버와 Solo Unity가 같이 사용하는 Nature 생존 규칙이다.
    /// 외부 시각이나 난수 상태를 읽지 않고 같은 입력에 같은 결과를 낸다.
    /// </summary>
    public static class NatureSurvivalRules
    {
        public const int CycleSeconds = 1_200;
        public const int DaylightEndsAtSecond = 600;
        public const int DuskEndsAtSecond = 750;
        public const int NightEndsAtSecond = 1_110;
        public const int HarvestWorkSeconds = 4;
        public const int HarvestTimberQuantity = 2;
        public const int CabinTimberCost = 6;
        public const int CabinWorkSeconds = 30;
        public const int TreeRegrowthCycleCount = 3;
        public const int CabinStorageCapacity = 20;
        public const int FirstDuskEncounterChancePermille = 650;

        public static NatureSurvivalClockProjection AdvanceClock(
            int cycleIndex,
            int elapsedSecondsInCycle,
            int elapsedSeconds)
        {
            if (cycleIndex < 0) throw new ArgumentOutOfRangeException(nameof(cycleIndex));
            if (elapsedSecondsInCycle < 0 || elapsedSecondsInCycle >= CycleSeconds)
                throw new ArgumentOutOfRangeException(nameof(elapsedSecondsInCycle));
            if (elapsedSeconds < 0) throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));

            var total = elapsedSecondsInCycle + elapsedSeconds;
            var completed = total / CycleSeconds;
            var nextElapsed = total % CycleSeconds;
            return new NatureSurvivalClockProjection
            {
                CycleIndex = checked(cycleIndex + completed),
                ElapsedSecondsInCycle = nextElapsed,
                PhaseCode = PhaseAt(nextElapsed),
                CompletedCycleCount = completed,
            };
        }

        public static string PhaseAt(int elapsedSecondsInCycle)
        {
            if (elapsedSecondsInCycle < 0 || elapsedSecondsInCycle >= CycleSeconds)
                throw new ArgumentOutOfRangeException(nameof(elapsedSecondsInCycle));
            if (elapsedSecondsInCycle < DaylightEndsAtSecond)
                return NatureSurvivalClockPhaseCodes.Daylight;
            if (elapsedSecondsInCycle < DuskEndsAtSecond)
                return NatureSurvivalClockPhaseCodes.Dusk;
            if (elapsedSecondsInCycle < NightEndsAtSecond)
                return NatureSurvivalClockPhaseCodes.Night;
            return NatureSurvivalClockPhaseCodes.Dawn;
        }

        public static bool CrossedIntoDusk(int previousSecond, int elapsedSeconds)
        {
            if (previousSecond < 0 || previousSecond >= CycleSeconds)
                throw new ArgumentOutOfRangeException(nameof(previousSecond));
            if (elapsedSeconds < 0) throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
            if (elapsedSeconds == 0) return false;

            var absoluteEnd = previousSecond + elapsedSeconds;
            for (var dusk = DaylightEndsAtSecond; dusk <= absoluteEnd; dusk += CycleSeconds)
            {
                if (dusk > previousSecond) return true;
            }
            return false;
        }

        public static bool RollFirstDuskEncounter(
            int scenarioSeed,
            string sessionStableId,
            int cycleIndex,
            int noiseEventCount)
        {
            if (string.IsNullOrWhiteSpace(sessionStableId))
                throw new ArgumentException("Session stable id is required.", nameof(sessionStableId));
            if (cycleIndex < 0) throw new ArgumentOutOfRangeException(nameof(cycleIndex));
            if (noiseEventCount <= 0) return false;

            var input = scenarioSeed + "|" + sessionStableId.Trim() + "|"
                + cycleIndex + "|" + noiseEventCount + "|first-dusk";
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                var roll = ((hash[0] << 8) | hash[1]) % 1_000;
                return roll < FirstDuskEncounterChancePermille;
            }
        }
    }
}
