using System;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    /// <summary>
    /// 모든 WI를 비실행 명상 WI군 관점에서 전수 분류한다.
    /// 이 Catalog는 자식 WI의 실행 계약을 감싸거나 다시 실행하지 않는다.
    /// </summary>
    public static class Simulation기본명상WiFamilyCatalog
    {
        public static Simulation명상WiFamilyCatalogSnapshot Create()
        {
            var playerDomains = Simulation기본플레이어분야Catalog.Create();
            return new Simulation명상WiFamilyCatalogSnapshot
            {
                Families = new[]
                {
                    new Simulation명상WiFamilyDefinition(),
                },
                Bindings = playerDomains.Wi결속들
                    .OrderBy(value => value.WorldInteractionId,
                        StringComparer.Ordinal)
                    .Select(Bind).ToArray(),
            };
        }

        public static Simulation명상WiFamilyBindingDefinition Resolve(
            string worldInteractionId)
        {
            if (string.IsNullOrWhiteSpace(worldInteractionId))
                throw new SimulationContractException(
                    "SimulationMeditationWiFamilyWorldInteractionIdInvalid");
            return Create().Bindings.SingleOrDefault(value => string.Equals(
                       value.WorldInteractionId, worldInteractionId,
                       StringComparison.Ordinal))
                   ?? throw new SimulationContractException(
                       "SimulationMeditationWiFamilyBindingMissing");
        }

        private static Simulation명상WiFamilyBindingDefinition Bind(
            SimulationWI분야결속Definition binding)
        {
            var isPlayerAction = binding.기여방식Code ==
                                 Simulation분야기여방식Codes.PlayerDirect
                                 || binding.기여방식Code ==
                                 Simulation분야기여방식Codes.PlayerOrOperation
                                 || binding.기여방식Code ==
                                 Simulation분야기여방식Codes.LearningOnly;
            if (isPlayerAction)
                return Definition(binding, Simulation명상WiFamilyCodes.Bound,
                    Simulation명상WiFamilyCodes.PlayerAction, string.Empty,
                    Simulation명상WiFamilyCodes.FamilyStableId);
            if (binding.기여방식Code ==
                Simulation분야기여방식Codes.OperationOnly)
                return Definition(binding,
                    Simulation명상WiFamilyCodes.NotApplicable,
                    Simulation명상WiFamilyCodes.NpcOrDelegatedOnly,
                    "NpcOrDelegatedOperationDoesNotMeditateForPlayer");
            return Definition(binding,
                Simulation명상WiFamilyCodes.NotApplicable,
                Simulation명상WiFamilyCodes.NoMeaningfulPlayerAction,
                string.IsNullOrWhiteSpace(binding.NoPlayerProgressReason)
                    ? "NoMeaningfulPlayerAction"
                    : binding.NoPlayerProgressReason);
        }

        private static Simulation명상WiFamilyBindingDefinition Definition(
            SimulationWI분야결속Definition binding, string status,
            string actionKind, string reason, params string[] familyIds)
            => new Simulation명상WiFamilyBindingDefinition
            {
                WorldInteractionId = binding.WorldInteractionId,
                상위WiFamilyStableIds = familyIds,
                결속상태Code = status,
                행위분류Code = actionKind,
                사유Code = reason,
            };
    }
}
