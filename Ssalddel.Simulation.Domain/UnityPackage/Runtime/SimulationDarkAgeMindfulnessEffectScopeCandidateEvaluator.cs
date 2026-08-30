using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "암흑기 정신 차림 접근 후보와 판본화된 Effect Profile을 읽어 개인 효과만 허용하고 세계 효과를 거부한다.",
        StepKey = "domain.dark-age-mindfulness-effect-scope-candidate",
        DependsOnStepKeys = new[]
        {
            "contract.dark-age-mindfulness-access-candidate",
            "domain.dark-age-mindfulness-access-candidate",
            "contract.dark-age-mindfulness-effect-scope-candidate",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 39,
        Boundary = "Profile 허용 여부만 판정하며 Effect·전투·관찰·제작·세계 상태를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q021 암흑기 제한 접근에서 개인 정신 차림 Effect만 허용하는 Profile을 읽기 전용으로 판정한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "후보 판정이며 실제 Effect 적용·분야별 수치·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class SimulationDarkAgeMindfulnessEffectScopeCandidateEvaluator
    {
        public static Simulation암흑기정신차림EffectScopeProfileDefinition[]
            DefaultProfiles() => new[]
            {
                Profile(Simulation암흑기정신차림EffectScopeCandidateCodes
                    .PersonalCombatFocus, true),
                Profile(Simulation암흑기정신차림EffectScopeCandidateCodes
                    .PersonalDeepObservation, true),
                Profile(Simulation암흑기정신차림EffectScopeCandidateCodes
                    .PersonalPrecisionCrafting, true),
                Profile(Simulation암흑기정신차림EffectScopeCandidateCodes
                    .RegionalRestoration, false),
                Profile(Simulation암흑기정신차림EffectScopeCandidateCodes
                    .SpatialExpansion, false),
                Profile(Simulation암흑기정신차림EffectScopeCandidateCodes
                    .CommunityProduction, false),
            };

        public Simulation암흑기정신차림EffectScopeCandidateSnapshot Evaluate(
            Simulation암흑기정신차림EffectScopeCandidateRequest request)
        {
            var reasons = new List<string>();
            var access = request?.AccessCandidate;
            if (access?.ReadinessCode !=
                    Simulation암흑기정신차림접근CandidateCodes.Ready ||
                access.LimitedGwangbokEffectAccessCandidate != true)
                reasons.Add(Simulation암흑기정신차림EffectScopeCandidateCodes
                    .DarkAgeAccessCandidateRequired);
            if (request?.ProfileRevision !=
                Simulation암흑기정신차림EffectScopeCandidateCodes
                    .ProfileRevision)
                reasons.Add(Simulation암흑기정신차림EffectScopeCandidateCodes
                    .ProfileRevisionRequired);
            if (string.IsNullOrWhiteSpace(request?.EffectCode))
                reasons.Add(Simulation암흑기정신차림EffectScopeCandidateCodes
                    .EffectCodeRequired);

            var profile = (request?.Profiles ??
                    Array.Empty<Simulation암흑기정신차림EffectScopeProfileDefinition>())
                .FirstOrDefault(value => string.Equals(value.EffectCode,
                    request?.EffectCode, StringComparison.Ordinal));
            if (profile == null)
                reasons.Add(Simulation암흑기정신차림EffectScopeCandidateCodes
                    .EffectProfileMissing);
            var allowed = reasons.Count == 0 && profile!.AllowedInDarkAge &&
                profile.ConsumerScopeCode ==
                    Simulation암흑기정신차림EffectScopeCandidateCodes
                        .PersonalMindfulnessConsumer;

            return new Simulation암흑기정신차림EffectScopeCandidateSnapshot
            {
                AccessDecisionCode = allowed
                    ? Simulation암흑기정신차림EffectScopeCandidateCodes.Allowed
                    : Simulation암흑기정신차림EffectScopeCandidateCodes.Denied,
                PlayerStableId = access?.PlayerStableId ?? string.Empty,
                EffectCode = request?.EffectCode?.Trim() ?? string.Empty,
                ConsumerScopeCode = profile?.ConsumerScopeCode ?? string.Empty,
                ProfileRevision = request?.ProfileRevision?.Trim()
                    ?? string.Empty,
                ReasonCodes = reasons.ToArray(),
                PersonalMindfulnessEffect = profile?.ConsumerScopeCode ==
                    Simulation암흑기정신차림EffectScopeCandidateCodes
                        .PersonalMindfulnessConsumer,
                WorldOrCommunityEffect = profile?.ConsumerScopeCode ==
                    Simulation암흑기정신차림EffectScopeCandidateCodes
                        .WorldOrCommunityConsumer,
                AppliesEffect = false,
                ChangesWorldState = false,
            };
        }

        private static Simulation암흑기정신차림EffectScopeProfileDefinition
            Profile(string effectCode, bool allowed)
        {
            return new Simulation암흑기정신차림EffectScopeProfileDefinition
            {
                EffectCode = effectCode,
                ConsumerScopeCode = allowed
                    ? Simulation암흑기정신차림EffectScopeCandidateCodes
                        .PersonalMindfulnessConsumer
                    : Simulation암흑기정신차림EffectScopeCandidateCodes
                        .WorldOrCommunityConsumer,
                AllowedInDarkAge = allowed,
            };
        }
    }
}
