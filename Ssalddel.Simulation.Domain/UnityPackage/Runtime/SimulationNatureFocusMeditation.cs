using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        private Simulation집중판정ChallengeSnapshot? natureActiveFocusChallenge;
        private Simulation집중판정ResultSnapshot? natureLastFocusResult;

        public Simulation집중판정ChallengeSnapshot SubmitNatureFocusTiming(
            Simulation집중판정AttemptRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CommandId)
                || string.IsNullOrWhiteSpace(request.ChallengeStableId))
                throw new SimulationContractException(
                    "SimulationFocusTimingAttemptRequired");
            lock (gate)
            {
                EnsureNatureSurvivalEnabled();
                if (request.ExpectedWorldRevision != Revision)
                    throw new SimulationConflictException(
                        SimulationNatureSurvivalCodes.ExpectedRevisionMismatch);
                var challenge = natureActiveFocusChallenge
                    ?? throw new SimulationConflictException(
                        "SimulationFocusTimingChallengeNotActive");
                if (!string.Equals(challenge.ChallengeStableId,
                        request.ChallengeStableId.Trim(), StringComparison.Ordinal)
                    || challenge.ChallengeRevision !=
                        request.ExpectedChallengeRevision)
                    throw new SimulationConflictException(
                        "SimulationFocusTimingChallengeRevisionMismatch");
                if (challenge.StateCode != Simulation집중판정Codes.Offered)
                    throw new SimulationConflictException(
                        "SimulationFocusTimingAttemptAlreadySubmitted");

                var candidate = Simulation집중판정Policy.Evaluate(challenge,
                    request.InputOffsetMillis);
                challenge.InputOffsetMillis = request.InputOffsetMillis;
                challenge.CandidateResultCode = candidate.ResultCode;
                challenge.CandidatePositionMicro = candidate.PositionMicro;
                challenge.CandidateDistanceMicro = candidate.DistanceMicro;
                challenge.StateCode = Simulation집중판정Codes.AttemptEvaluated;
                challenge.ChallengeRevision++;
                Revision++;
                AppendNatureFocusTimingCommand(request);
                return CloneFocusChallenge(challenge)!;
            }
        }

        private void BeginNatureHarvestFocus(
            SimulationNatureSurvivalCommandRequest request)
        {
            var profile = Simulation기본집중ProfileCatalog.Create().Profiles
                .Single(value => string.Equals(value.WorldInteractionId,
                    SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
                    StringComparison.Ordinal));
            if (profile.적용상태Code !=
                Simulation집중판정Codes.ProfileApplied)
                throw new SimulationContractException(
                    "SimulationFocusProfileNotApplied");
            var policy = Simulation집중판정Policy.Create(
                natureSurvivalCreationState?.FocusAccessibilityModeCode
                ?? Simulation집중판정Codes.Standard);
            natureActiveFocusChallenge = new Simulation집중판정ChallengeSnapshot
            {
                ChallengeStableId = "focus:nature:logging:" +
                    HashFocusKey(request.CommandId.Trim()).Substring(0, 24),
                ChallengeRevision = 1,
                PlayerStableId = request.PlayerStableId.Trim(),
                WorldInteractionId = SimulationNatureSurvivalCodes
                    .BeginHarvestWorldInteractionId,
                OriginCommandId = request.CommandId.Trim(),
                TargetStableId = NormalizeOptional(request.TargetStableId),
                분야StableId = profile.분야StableId,
                세부숙련StableId = profile.세부숙련StableId,
                Policy = policy,
            };
        }

        private void CompleteNatureHarvestActionAndFocus(
            SimulationNatureActiveWorkSnapshot work, long completedRevision)
        {
            var start = FindActionManifestation(work.OriginCommandId).기록;
            var challenge = natureActiveFocusChallenge;
            var result = challenge == null
                         || !string.Equals(challenge.OriginCommandId,
                             work.OriginCommandId, StringComparison.Ordinal)
                ? null
                : Simulation집중판정Policy.Evaluate(challenge,
                    challenge.InputOffsetMillis);
            Simulation행위발현Record? completionRecord = null;
            if (start != null)
            {
                var completion = AppendActionManifestationAndProgression(
                new Simulation행위발현Record
                {
                    WorldStableId = start.WorldStableId,
                    SessionStableId = start.SessionStableId,
                    PlayableLoopStableId = start.PlayableLoopStableId,
                    WorldInteractionId = start.WorldInteractionId,
                    CommandId = work.OriginCommandId + ":completed",
                    TriggerSourceCode = start.TriggerSourceCode,
                    InitiatorStableId = start.InitiatorStableId,
                    ActorStableId = start.ActorStableId,
                    ActorKindCode = start.ActorKindCode,
                    TargetStableIds = start.TargetStableIds.ToArray(),
                    OutcomeStableId = "outcome:" + work.OriginCommandId +
                        ":completed",
                    PrimaryOutcomeCode = "HarvestCompleted",
                    결과분류Code = Simulation행위결과분류Codes.성공,
                    EffectBatchStableId = "effect-batch:" +
                        work.OriginCommandId + ":completed",
                    EffectReceiptStableIds = new[]
                    {
                        "effect-receipt:" + work.OriginCommandId +
                        ":harvest-completed",
                    },
                    변화의미Codes = new[]
                    {
                        Simulation행위변화의미Codes.세계객체생성,
                        Simulation행위변화의미Codes.재고변경,
                        Simulation행위변화의미Codes.실외배치변경,
                    },
                    영향공간StableIds = start.영향공간StableIds.ToArray(),
                    SourceReferenceIds = new[] { start.행위기록StableId },
                    BeforeWorldRevision = Revision,
                    AfterWorldRevision = completedRevision,
                    AppliedWorldTick = CurrentTick,
                    RuleRevision = RuleRevision,
                    SpatialRevision = start.SpatialRevision,
                    DataRevision = start.DataRevision,
                }, result);
                completionRecord = completion.기록;
            }
            else if (challenge != null && result != null
                     && result.회복증가Milli > 0)
            {
                // v29가 감싸는 v28 이하 Command 재생에서는 행위 원장을
                // 뒤에서 복원한다. 이 호환 경로는 저장된 개인 회복만
                // 재구성하고 명상 기여는 v28 Profile 복원에 맡긴다.
                ApplyNatureMindImpactForPlayer(challenge.PlayerStableId,
                    "mind-impact:focus:" + challenge.ChallengeStableId +
                    ":recovery",
                    SimulationNatureMindCodes.FocusTimingCompleted,
                    result.SourceStableId,
                    SimulationNatureMindCodes.RecoveryAxis,
                    result.회복증가Milli /
                    (decimal)Simulation집중판정Codes.MilliPerPoint,
                    CurrentTick);
            }

            if (challenge == null || result == null) return;
            result.SourceActionRecordStableId =
                completionRecord?.행위기록StableId ?? string.Empty;
            result.AppliedWorldRevision = completedRevision;
            natureLastFocusResult = CloneFocusResult(result);
            natureActiveFocusChallenge = null;
        }

        internal void RestoreNatureFocusState(
            SimulationNatureSurvivalStateSnapshot saved)
        {
            lock (gate)
            {
                natureActiveFocusChallenge = CloneFocusChallenge(
                    saved.ActiveFocusChallenge);
                natureLastFocusResult = CloneFocusResult(saved.LastFocusResult);
            }
        }

        private void VoidNatureHarvestFocus(string originCommandId)
        {
            if (natureActiveFocusChallenge == null
                || !string.Equals(natureActiveFocusChallenge.OriginCommandId,
                    originCommandId, StringComparison.Ordinal)) return;
            natureLastFocusResult = new Simulation집중판정ResultSnapshot
            {
                ChallengeStableId = natureActiveFocusChallenge.ChallengeStableId,
                StateCode = Simulation집중판정Codes.Voided,
                분야StableId = natureActiveFocusChallenge.분야StableId,
                세부숙련StableId = natureActiveFocusChallenge.세부숙련StableId,
                RuleRevision = natureActiveFocusChallenge.Policy.RuleRevision,
            };
            natureActiveFocusChallenge = null;
        }

        internal static string CalculateNatureFocusStateHash(
            SimulationNatureSurvivalStateSnapshot state)
        {
            var challenge = state.ActiveFocusChallenge;
            var result = state.LastFocusResult;
            var canonical = string.Join("|", new[]
            {
                challenge?.ChallengeStableId ?? string.Empty,
                challenge?.ChallengeRevision.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                challenge?.StateCode ?? string.Empty,
                challenge?.OriginCommandId ?? string.Empty,
                challenge?.InputOffsetMillis?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                challenge?.CandidateResultCode ?? string.Empty,
                challenge?.Policy.RuleRevision ?? string.Empty,
                result?.ChallengeStableId ?? string.Empty,
                result?.StateCode ?? string.Empty,
                result?.ResultCode ?? string.Empty,
                result?.명상경험증가Milli.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                result?.회복증가Milli.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                result?.SourceActionRecordStableId ?? string.Empty,
                result?.AppliedWorldRevision.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            });
            return HashFocusKey(canonical);
        }

        internal static Simulation집중판정ChallengeSnapshot? CloneFocusChallenge(
            Simulation집중판정ChallengeSnapshot? source)
            => source == null ? null : new Simulation집중판정ChallengeSnapshot
            {
                ChallengeStableId = source.ChallengeStableId,
                ChallengeRevision = source.ChallengeRevision,
                StateCode = source.StateCode,
                PlayerStableId = source.PlayerStableId,
                WorldInteractionId = source.WorldInteractionId,
                OriginCommandId = source.OriginCommandId,
                TargetStableId = source.TargetStableId,
                분야StableId = source.분야StableId,
                세부숙련StableId = source.세부숙련StableId,
                Policy = new Simulation집중판정PolicySnapshot
                {
                    ChallengeKindCode = source.Policy.ChallengeKindCode,
                    CyclePolicyCode = source.Policy.CyclePolicyCode,
                    AccessibilityModeCode = source.Policy.AccessibilityModeCode,
                    ChallengeStartOffsetMillis = source.Policy.ChallengeStartOffsetMillis,
                    DurationMillis = source.Policy.DurationMillis,
                    TargetPositionMicro = source.Policy.TargetPositionMicro,
                    PerfectDistanceMicro = source.Policy.PerfectDistanceMicro,
                    GoodDistanceMicro = source.Policy.GoodDistanceMicro,
                    RuleRevision = source.Policy.RuleRevision,
                },
                InputOffsetMillis = source.InputOffsetMillis,
                CandidateResultCode = source.CandidateResultCode,
                CandidatePositionMicro = source.CandidatePositionMicro,
                CandidateDistanceMicro = source.CandidateDistanceMicro,
            };

        internal static Simulation집중판정ResultSnapshot? CloneFocusResult(
            Simulation집중판정ResultSnapshot? source)
            => source == null ? null : new Simulation집중판정ResultSnapshot
            {
                ChallengeStableId = source.ChallengeStableId,
                StateCode = source.StateCode,
                ResultCode = source.ResultCode,
                PositionMicro = source.PositionMicro,
                DistanceMicro = source.DistanceMicro,
                명상경험증가Milli = source.명상경험증가Milli,
                회복증가Milli = source.회복증가Milli,
                분야StableId = source.분야StableId,
                세부숙련StableId = source.세부숙련StableId,
                SourceStableId = source.SourceStableId,
                SourceActionRecordStableId = source.SourceActionRecordStableId,
                AppliedWorldRevision = source.AppliedWorldRevision,
                RuleRevision = source.RuleRevision,
            };

        private static string HashFocusKey(string value)
        {
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(value)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
