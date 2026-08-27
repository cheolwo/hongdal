using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Nature 첫날 WI의 권위·표현 엔진 순서 계약을 공통 검증 Profile로 제공한다.",
        Boundary = "Profile은 실제 실행 증거나 별도 성숙도 축이 아니다.")]
    public static class SimulationNatureFirstDayEngineValidationProfiles
    {
        public const string ProfileRevision =
            "nature-night-day2.engine-interaction.r3";
        public const string PlayableLoopStableId =
            "playable-loop:nature-night-day2.v1";

        public static SimulationPlayableLoopEngineValidationProfile Get(
            string worldInteractionId)
        {
            var requirements = new List<
                SimulationPlayableLoopEngineRequirement>
            {
                Required(
                    SimulationEngineInteractionComponentCodes
                        .WorldInteractionPipeline,
                    SimulationEngineInteractionPhaseCodes.Preview),
                Required(
                    SimulationEngineInteractionComponentCodes
                        .WorldInteractionPipeline,
                    SimulationEngineInteractionPhaseCodes.Confirm),
                Required(
                    SimulationEngineInteractionComponentCodes
                        .WorldInteractionPipeline,
                    SimulationEngineInteractionPhaseCodes.FocusEvidenceCollect,
                    "Logic", "E4", allowsNotApplicable: true),
                Required(SimulationEngineInteractionComponentCodes.AuthorityCore,
                    SimulationEngineInteractionPhaseCodes.AuthorityCommit,
                    "Logic", "E5"),
                Required(SimulationEngineInteractionComponentCodes.ActionJournal,
                    SimulationEngineInteractionPhaseCodes.ActionRecordAppend,
                    "Logic", "E5"),
                Required(
                    SimulationEngineInteractionComponentCodes
                        .PlayerDomainProgression,
                    SimulationEngineInteractionPhaseCodes.PlayerProgressionApply,
                    "Logic", "E5", allowsNotApplicable: true),
                Required(
                    SimulationEngineInteractionComponentCodes
                        .PlayerMeditationProgression,
                    SimulationEngineInteractionPhaseCodes
                        .MeditationProgressionApply,
                    "Logic", "E5", allowsNotApplicable: true),
                Required(
                    SimulationEngineInteractionComponentCodes
                        .WorldInteractionPipeline,
                    SimulationEngineInteractionPhaseCodes.ReturnProjection,
                    "Logic", "E5"),
            };
            if (string.Equals(worldInteractionId, "WI-NATURE-14",
                    StringComparison.Ordinal))
            {
                requirements.Add(Required(
                    SimulationEngineInteractionComponentCodes.LhSurface,
                    SimulationEngineInteractionPhaseCodes.ActionRecordRead,
                    "Presentation", "E5"));
                requirements.Add(Required(
                    SimulationEngineInteractionComponentCodes.LhSurface,
                    SimulationEngineInteractionPhaseCodes.SurfacePreparation,
                    "Presentation", "E5"));
                requirements.Add(Required(
                    SimulationEngineInteractionComponentCodes.SkyPresentation,
                    SimulationEngineInteractionPhaseCodes.ActionRecordRead,
                    "Presentation", "E5"));
                requirements.Add(Required(
                    SimulationEngineInteractionComponentCodes.SkyPresentation,
                    SimulationEngineInteractionPhaseCodes.AtmosphereProjection,
                    "Presentation", "E5"));
                requirements.Add(Required(
                    SimulationEngineInteractionComponentCodes.ExteriorPlacement,
                    SimulationEngineInteractionPhaseCodes.ActionRecordRead,
                    "Presentation", "E5"));
                requirements.Add(Required(
                    SimulationEngineInteractionComponentCodes.ExteriorPlacement,
                    SimulationEngineInteractionPhaseCodes.ExteriorPlacement,
                    "Presentation", "E5"));
            }
            requirements.Add(Required(
                SimulationEngineInteractionComponentCodes.InteriorPlacement,
                SimulationEngineInteractionPhaseCodes.ActionRecordRead,
                "Presentation", "E5"));
            requirements.Add(Required(
                SimulationEngineInteractionComponentCodes.InteriorPlacement,
                SimulationEngineInteractionPhaseCodes.InteriorPlacement,
                "Presentation", "E5"));
            requirements.Add(Required(
                SimulationEngineInteractionComponentCodes.WorldPresentation,
                SimulationEngineInteractionPhaseCodes.ActionRecordRead,
                "Presentation", "E5"));
            requirements.Add(Required(
                SimulationEngineInteractionComponentCodes.WorldPresentation,
                SimulationEngineInteractionPhaseCodes.ReturnProjection,
                "Presentation", "E5"));

            if (worldInteractionId != "WI-NATURE-13"
                && worldInteractionId != "WI-NATURE-14"
                && worldInteractionId != "WI-NATURE-15")
                throw new InvalidOperationException(
                    "NatureFirstDayEngineValidationProfileMissing");
            return new SimulationPlayableLoopEngineValidationProfile
            {
                ProfileRevision = ProfileRevision,
                PlayableLoopStableId = PlayableLoopStableId,
                WorldInteractionId = worldInteractionId,
                ExpectedChangeSemanticCodes = worldInteractionId ==
                                              "WI-NATURE-14"
                    ? new[]
                    {
                        Simulation행위변화의미Codes.Actor상태변경,
                        Simulation행위변화의미Codes.시간상태변경,
                        Simulation행위변화의미Codes.대기변경,
                        Simulation행위변화의미Codes.실내설비변경,
                    }
                    : new[]
                    {
                        Simulation행위변화의미Codes.Actor상태변경,
                    },
                RequiredPresentationConsumerCodes = requirements
                    .Where(value => value.MaturityTrackCode == "Presentation")
                    .Select(value => value.ComponentCode)
                    .Distinct(StringComparer.Ordinal).ToArray(),
                Requirements = requirements.ToArray(),
            };
        }

        private static SimulationPlayableLoopEngineRequirement Required(
            string componentCode, string phaseCode,
            string trackCode = "Logic", string reopenStage = "E4",
            bool allowsNotApplicable = false) => new()
        {
            ComponentCode = componentCode,
            PhaseCode = phaseCode,
            MaturityTrackCode = trackCode,
            EarliestReopenEvidenceStageCode = reopenStage,
            AllowsReused = true,
            AllowsNotApplicable = allowsNotApplicable,
        };
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "엔진 추적을 사용하지 않는 Host가 기존 실행을 유지하는 Null Adapter를 제공한다.",
        Boundary = "Null Adapter는 권위 상태와 기존 WI 실행 결과를 바꾸지 않는다.")]
    public sealed class NullSimulationPlayableLoopEngineTraceSink
        : ISimulationPlayableLoopEngineTraceSink
    {
        public static readonly NullSimulationPlayableLoopEngineTraceSink Instance
            = new NullSimulationPlayableLoopEngineTraceSink();

        private NullSimulationPlayableLoopEngineTraceSink()
        {
        }

        public void Record(SimulationPlayableLoopEngineTraceEntry entry)
        {
        }

        public SimulationPlayableLoopEngineTraceEntry[] Snapshot(
            string playableLoopStableId, string worldInteractionId,
            string commandId) =>
            Array.Empty<SimulationPlayableLoopEngineTraceEntry>();
    }

    /// <summary>
    /// 시험과 로컬 진단을 위한 비권위 추적 수집기다. Session Save에 포함하지 않는다.
    /// </summary>
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E3,
        "로컬 진단과 시험에서 같은 WI 명령의 엔진 추적 순서를 결정적으로 수집한다.",
        Boundary = "수집 결과는 Session Save와 Replay canonical hash에 포함하지 않는다.")]
    public sealed class InMemorySimulationPlayableLoopEngineTraceSink
        : ISimulationPlayableLoopEngineTraceSink
    {
        private readonly object gate = new object();
        private readonly List<SimulationPlayableLoopEngineTraceEntry> entries
            = new List<SimulationPlayableLoopEngineTraceEntry>();

        public void Record(SimulationPlayableLoopEngineTraceEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));
            lock (gate)
            {
                var clone = Clone(entry);
                if (clone.Sequence <= 0)
                    clone.Sequence = entries.Count(value => SameCommand(
                        value, clone)) + 1;
                entries.Add(clone);
            }
        }

        public SimulationPlayableLoopEngineTraceEntry[] Snapshot(
            string playableLoopStableId, string worldInteractionId,
            string commandId)
        {
            lock (gate)
                return entries.Where(value =>
                        string.Equals(value.PlayableLoopStableId,
                            playableLoopStableId ?? string.Empty,
                            StringComparison.Ordinal)
                        && string.Equals(value.WorldInteractionId,
                            worldInteractionId ?? string.Empty,
                            StringComparison.Ordinal)
                        && string.Equals(value.CommandId,
                            commandId ?? string.Empty, StringComparison.Ordinal))
                    .OrderBy(value => value.Sequence)
                    .Select(Clone).ToArray();
        }

        private static bool SameCommand(
            SimulationPlayableLoopEngineTraceEntry left,
            SimulationPlayableLoopEngineTraceEntry right) =>
            string.Equals(left.PlayableLoopStableId,
                right.PlayableLoopStableId, StringComparison.Ordinal)
            && string.Equals(left.WorldInteractionId,
                right.WorldInteractionId, StringComparison.Ordinal)
            && string.Equals(left.CommandId, right.CommandId,
                StringComparison.Ordinal);

        private static SimulationPlayableLoopEngineTraceEntry Clone(
            SimulationPlayableLoopEngineTraceEntry value) => new()
        {
            PlayableLoopStableId = value.PlayableLoopStableId,
            WorldInteractionId = value.WorldInteractionId,
            CommandId = value.CommandId,
            AuthorityLocationCode = value.AuthorityLocationCode,
            ComponentCode = value.ComponentCode,
            ComponentKindCode = value.ComponentKindCode,
            ComponentRevision = value.ComponentRevision,
            PhaseCode = value.PhaseCode,
            Sequence = value.Sequence,
            InputHashSha256 = value.InputHashSha256,
            OutputHashSha256 = value.OutputHashSha256,
            StatusCode = value.StatusCode,
            BeforeAuthorityRevision = value.BeforeAuthorityRevision,
            AfterAuthorityRevision = value.AfterAuthorityRevision,
            ReasonCode = value.ReasonCode,
        };
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E3,
        "동일 WI 명령의 권위·LH·Sky·실내외 배치 순서와 비권위 경계를 결정적으로 검사한다.",
        Boundary = "엔진 추적은 Logic·Presentation 증거를 보조하며 별도 E 성숙도나 Save 권위 상태를 만들지 않는다.")]
    public sealed class SimulationPlayableLoopEngineInteractionValidator
    {
        public SimulationPlayableLoopEngineValidationSnapshot Validate(
            SimulationPlayableLoopEngineValidationProfile profile,
            IEnumerable<SimulationPlayableLoopEngineTraceEntry> source,
            string commandId)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            var trace = (source ?? Array.Empty<SimulationPlayableLoopEngineTraceEntry>())
                .Where(value => string.Equals(value.PlayableLoopStableId,
                                    profile.PlayableLoopStableId,
                                    StringComparison.Ordinal)
                                && string.Equals(value.WorldInteractionId,
                                    profile.WorldInteractionId,
                                    StringComparison.Ordinal)
                                && string.Equals(value.CommandId,
                                    commandId ?? string.Empty,
                                    StringComparison.Ordinal))
                .OrderBy(value => value.Sequence).ToArray();
            var failures = new List<string>();
            var reopenStages = new List<string>();
            var lastSequence = 0;

            foreach (var requirement in profile.Requirements)
            {
                var failureCountBefore = failures.Count;
                var match = trace.FirstOrDefault(value =>
                    value.Sequence > lastSequence
                    && string.Equals(value.ComponentCode,
                        requirement.ComponentCode, StringComparison.Ordinal)
                    && string.Equals(value.PhaseCode,
                        requirement.PhaseCode, StringComparison.Ordinal));
                if (match == null)
                {
                    failures.Add("EngineInteractionRequiredStepMissing:"
                                 + requirement.ComponentCode + ":"
                                 + requirement.PhaseCode);
                    reopenStages.Add(
                        requirement.EarliestReopenEvidenceStageCode);
                    continue;
                }
                if (string.Equals(match.StatusCode,
                        SimulationEngineInteractionStatusCodes.Blocked,
                        StringComparison.Ordinal))
                    failures.Add("EngineInteractionStepBlocked:"
                                 + requirement.ComponentCode);
                else if (string.Equals(match.StatusCode,
                             SimulationEngineInteractionStatusCodes.Reused,
                             StringComparison.Ordinal)
                         && !requirement.AllowsReused)
                    failures.Add("EngineInteractionReuseNotAllowed:"
                                 + requirement.ComponentCode);
                else if (string.Equals(match.StatusCode,
                             SimulationEngineInteractionStatusCodes.NotApplicable,
                             StringComparison.Ordinal)
                         && !requirement.AllowsNotApplicable)
                    failures.Add("EngineInteractionNotApplicable:"
                                 + requirement.ComponentCode);
                else if (string.Equals(match.StatusCode,
                             SimulationEngineInteractionStatusCodes.NotApplicable,
                             StringComparison.Ordinal)
                         && string.IsNullOrWhiteSpace(match.ReasonCode))
                    failures.Add("EngineInteractionNotApplicableReasonMissing:"
                                 + requirement.ComponentCode);
                if (failures.Count > failureCountBefore)
                    reopenStages.Add(
                        requirement.EarliestReopenEvidenceStageCode);
                lastSequence = match.Sequence;
            }

            foreach (var entry in trace.Where(value => string.Equals(
                         value.ComponentKindCode,
                         SimulationEngineInteractionComponentKinds.Presentation,
                         StringComparison.Ordinal)))
                if (entry.BeforeAuthorityRevision != entry.AfterAuthorityRevision)
                {
                    failures.Add("PresentationMutatedAuthorityRevision:"
                                 + entry.ComponentCode);
                    reopenStages.Add("E5");
                }

            return new SimulationPlayableLoopEngineValidationSnapshot
            {
                ProfileRevision = profile.ProfileRevision,
                PlayableLoopStableId = profile.PlayableLoopStableId,
                WorldInteractionId = profile.WorldInteractionId,
                CommandId = commandId ?? string.Empty,
                Passed = failures.Count == 0,
                EarliestReopenEvidenceStageCode = failures.Count == 0
                    ? string.Empty : EarliestStage(reopenStages),
                FailureCodes = failures.Distinct(StringComparer.Ordinal).ToArray(),
                TraceEntries = trace,
            };
        }

        private static string EarliestStage(IEnumerable<string> values)
            => values.Where(value => !string.IsNullOrWhiteSpace(value))
                .OrderBy(value => int.TryParse(value.TrimStart('E'), out var rank)
                    ? rank : int.MaxValue)
                .FirstOrDefault() ?? "E3";
    }

    internal static class SimulationPlayableLoopEngineTraceHash
    {
        public static string Compute(params object?[] values)
        {
            var canonical = string.Join("|", values.Select(value =>
                value == null ? string.Empty : value.ToString() ?? string.Empty));
            using var sha = SHA256.Create();
            return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical)))
                .Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
