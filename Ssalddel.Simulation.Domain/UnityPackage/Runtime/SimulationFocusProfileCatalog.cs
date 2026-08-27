using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    /// <summary>
    /// 모든 WI를 집중 적용, 프로필 대기, NPC 전용, 자동 또는 제외로
    /// 분류한다. 실제 Challenge가 없는 WI에 보상을 추정하지 않는다.
    /// </summary>
    public static class Simulation기본집중ProfileCatalog
    {
        public static Simulation집중ProfileCatalogSnapshot Create()
        {
            var domains = Simulation기본플레이어분야Catalog.Create();
            return new Simulation집중ProfileCatalogSnapshot
            {
                Profiles = domains.Wi결속들
                    .OrderBy(value => value.WorldInteractionId,
                        StringComparer.Ordinal)
                    .Select(CreateProfile).ToArray(),
            };
        }

        private static Simulation집중ProfileDefinition CreateProfile(
            SimulationWI분야결속Definition binding)
        {
            var line = binding.결속선들.FirstOrDefault();
            if (string.Equals(binding.WorldInteractionId,
                    SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
                    StringComparison.Ordinal))
                return Profile(binding, line,
                    Simulation집중판정Codes.ProfileApplied,
                    Simulation집중판정Codes.FocusTiming,
                    "FirstVerticalSlice");
            if (binding.기여방식Code ==
                Simulation분야기여방식Codes.OperationOnly)
                return Profile(binding, line,
                    Simulation집중판정Codes.ProfileNpcOnly,
                    string.Empty, "NpcOrDelegatedOperationDoesNotMeditateForPlayer");
            if (binding.기여방식Code == Simulation분야기여방식Codes.None)
                return Profile(binding, line,
                    Simulation집중판정Codes.ProfileExcluded,
                    string.Empty, string.IsNullOrWhiteSpace(
                        binding.NoPlayerProgressReason)
                        ? "NoMeaningfulPlayerAction" : binding.NoPlayerProgressReason);
            return Profile(binding, line,
                Simulation집중판정Codes.ProfilePending,
                string.Empty, "ActionSpecificFocusProfileRequired");
        }

        private static Simulation집중ProfileDefinition Profile(
            SimulationWI분야결속Definition binding,
            Simulation분야숙련결속선Definition? line, string status,
            string challengeKind, string reason)
            => new Simulation집중ProfileDefinition
            {
                WorldInteractionId = binding.WorldInteractionId,
                적용상태Code = status,
                ChallengeKindCode = challengeKind,
                분야StableId = line?.분야StableId ?? string.Empty,
                세부숙련StableId = line?.세부숙련StableId ?? string.Empty,
                사유Code = reason,
                RuleRevision = Simulation집중판정Codes.MeditationRuleRevision,
            };
    }
}
