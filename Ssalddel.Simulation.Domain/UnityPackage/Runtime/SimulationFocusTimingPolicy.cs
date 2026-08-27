using System;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
        "집중 판정의 결정적 정수 정책과 접근성 모드 계약을 정의한다.",
        Boundary = "판정 정책은 Task 완료나 Unity 표현을 확정하지 않는다.")]
    public static class Simulation집중판정Policy
    {
        public static Simulation집중판정PolicySnapshot Create(
            string accessibilityModeCode)
        {
            var mode = string.IsNullOrWhiteSpace(accessibilityModeCode)
                ? Simulation집중판정Codes.Standard
                : accessibilityModeCode.Trim();
            return mode switch
            {
                Simulation집중판정Codes.Standard =>
                    new Simulation집중판정PolicySnapshot(),
                Simulation집중판정Codes.Assisted =>
                    new Simulation집중판정PolicySnapshot
                    {
                        AccessibilityModeCode = Simulation집중판정Codes.Assisted,
                        DurationMillis = 3_000,
                        PerfectDistanceMicro = 90_000,
                        GoodDistanceMicro = 270_000,
                    },
                Simulation집중판정Codes.NeutralSkip =>
                    new Simulation집중판정PolicySnapshot
                    {
                        AccessibilityModeCode = Simulation집중판정Codes.NeutralSkip,
                        DurationMillis = 0,
                        PerfectDistanceMicro = 0,
                        GoodDistanceMicro = 0,
                    },
                _ => throw new SimulationContractException(
                    "SimulationFocusTimingAccessibilityModeInvalid"),
            };
        }

        public static Simulation집중판정ResultSnapshot Evaluate(
            Simulation집중판정ChallengeSnapshot challenge,
            int? inputOffsetMillis)
        {
            if (challenge == null || challenge.Policy == null
                || challenge.Policy.TargetPositionMicro < 0
                || challenge.Policy.TargetPositionMicro > 1_000_000)
                throw new SimulationContractException(
                    "SimulationFocusTimingChallengeInvalid");
            var policy = challenge.Policy;
            if (policy.AccessibilityModeCode ==
                Simulation집중판정Codes.NeutralSkip)
                return Result(challenge,
                    Simulation집중판정Codes.AssistedNeutral, 0, 0);

            if (policy.DurationMillis <= 0
                || policy.PerfectDistanceMicro < 0
                || policy.GoodDistanceMicro < policy.PerfectDistanceMicro)
                throw new SimulationContractException(
                    "SimulationFocusTimingPolicyInvalid");
            if (!inputOffsetMillis.HasValue)
                return Result(challenge, Simulation집중판정Codes.NoInput,
                    0, 0);
            if (inputOffsetMillis.Value < 0
                || inputOffsetMillis.Value >= policy.DurationMillis)
                throw new SimulationConflictException(
                    "SimulationFocusTimingInputOffsetInvalid");

            var phase = (int)((long)inputOffsetMillis.Value * 1_000_000L
                              / policy.DurationMillis);
            var position = phase <= 500_000
                ? phase * 2
                : (1_000_000 - phase) * 2;
            var distance = Math.Abs(position - policy.TargetPositionMicro);
            if (distance <= policy.PerfectDistanceMicro)
                return Result(challenge, Simulation집중판정Codes.Perfect,
                    position, distance);
            if (distance <= policy.GoodDistanceMicro)
                return Result(challenge, Simulation집중판정Codes.Good,
                    position, distance);
            return Result(challenge, Simulation집중판정Codes.Miss,
                position, distance);
        }

        private static Simulation집중판정ResultSnapshot Result(
            Simulation집중판정ChallengeSnapshot challenge, string code,
            int position, int distance)
        {
            var meditation = code == Simulation집중판정Codes.Perfect
                ? Simulation집중판정Codes.PerfectRewardMilli
                : code == Simulation집중판정Codes.Good
                    ? Simulation집중판정Codes.GoodRewardMilli
                    : 0L;
            return new Simulation집중판정ResultSnapshot
            {
                ChallengeStableId = challenge.ChallengeStableId,
                StateCode = Simulation집중판정Codes.Manifested,
                ResultCode = code,
                PositionMicro = position,
                DistanceMicro = distance,
                명상경험증가Milli = meditation,
                회복증가Milli = meditation,
                분야StableId = challenge.분야StableId,
                세부숙련StableId = challenge.세부숙련StableId,
                SourceStableId = BuildSourceStableId(challenge),
                RuleRevision = PolicyRevision(challenge),
            };
        }

        private static string BuildSourceStableId(
            Simulation집중판정ChallengeSnapshot challenge)
            => "Focus:" + challenge.분야StableId + ":" +
               challenge.세부숙련StableId + ":" +
               challenge.WorldInteractionId;

        private static string PolicyRevision(
            Simulation집중판정ChallengeSnapshot challenge)
            => challenge.Policy?.RuleRevision ??
               Simulation집중판정Codes.RuleRevision;
    }
}
