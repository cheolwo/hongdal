using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Domain,
        "승인된 플레이어 분야 진척을 정확한 수치 없이 정성적 성장 낌새로 투영한다.",
        StepKey = "domain.player-growth-hint-projection",
        DependsOnStepKeys = new[]
        {
            "contract.player-growth-hint-projection",
        },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 27,
        Boundary = "기존 분야 Profile을 읽지만 기여 기록·정확한 수치·해금·인벤토리를 결과에 복사하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Q009 승인된 성장 분야를 순위·주 성장 방식·정성 단계의 낌새로 투영한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        Boundary = "읽기 전용 Projection이며 권한을 생성하거나 플레이어 Profile·WorldRevision을 변경하지 않는다.")]
    public sealed class SimulationPlayerGrowthHintProjection
    {
        public Simulation성장낌새ProjectionSnapshot Project(
            Simulation플레이어분야ProfileSnapshot profile,
            Simulation플레이어분야CatalogSnapshot catalog,
            Simulation성장낌새ProjectionRequest request)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (request == null) throw new ArgumentNullException(nameof(request));
            var result = Base(profile, request);
            if (!string.Equals(profile.PlayerStableId,
                    request.TargetPlayerStableId, StringComparison.Ordinal))
            {
                result.ReasonCode =
                    Simulation성장낌새ProjectionCodes.TargetProfileMismatch;
                return result;
            }
            if (string.IsNullOrWhiteSpace(
                    request.AuthorizationPolicyRevision))
            {
                result.ReasonCode = Simulation성장낌새ProjectionCodes
                    .PolicyRevisionRequired;
                return result;
            }
            var known = new HashSet<string>(catalog.분야들.Select(value =>
                value.분야StableId), StringComparer.Ordinal);
            var authorized = new HashSet<string>((request.AuthorizedDomainCodes
                    ?? Array.Empty<string>())
                .Where(known.Contains), StringComparer.Ordinal);
            if (authorized.Count == 0)
            {
                result.ReasonCode = Simulation성장낌새ProjectionCodes
                    .NoAuthorizedDomain;
                return result;
            }
            var names = catalog.분야들.ToDictionary(value =>
                value.분야StableId, value => value.한국어명,
                StringComparer.Ordinal);
            var maximum = Math.Max(1, Math.Min(5,
                request.MaximumHintCount));
            result.Hints = profile.분야진척들
                .Where(value => authorized.Contains(value.분야StableId))
                .Where(value => Total(value) > 0)
                .OrderByDescending(Total)
                .ThenBy(value => value.분야StableId, StringComparer.Ordinal)
                .Take(maximum)
                .Select((value, index) => Hint(value, names, index + 1))
                .ToArray();
            result.Allowed = true;
            result.ReasonCode = Simulation성장낌새ProjectionCodes.Allowed;
            return result;
        }

        private static Simulation성장낌새ProjectionSnapshot Base(
            Simulation플레이어분야ProfileSnapshot profile,
            Simulation성장낌새ProjectionRequest request)
            => new Simulation성장낌새ProjectionSnapshot
            {
                ObserverPlayerStableId = request.ObserverPlayerStableId,
                TargetPlayerStableId = request.TargetPlayerStableId,
                SourceProfileRevision = profile.Revision.ToString(
                    CultureInfo.InvariantCulture),
                AuthorizationPolicyRevision =
                    request.AuthorizationPolicyRevision ?? string.Empty,
                ContainsExactProgressValues = false,
                ContainsContributionRecords = false,
                ContainsUnlockCodes = false,
                ContainsInventory = false,
                ChangesWorldState = false,
            };

        private static Simulation성장낌새HintSnapshot Hint(
            Simulation분야진척Snapshot value,
            IReadOnlyDictionary<string, string> names, int rank)
        {
            var candidates = new[]
            {
                (Simulation분야진척종류Codes.이해도, value.이해도,
                    value.이해도단계Code, 0),
                (Simulation분야진척종류Codes.현장숙련도,
                    value.현장숙련도, value.현장숙련도단계Code, 1),
                (Simulation분야진척종류Codes.운영숙련도,
                    value.운영숙련도, value.운영숙련도단계Code, 2),
            };
            var dominant = candidates.OrderByDescending(item => item.Item2)
                .ThenBy(item => item.Item4).First();
            return new Simulation성장낌새HintSnapshot
            {
                Rank = rank,
                DomainStableId = value.분야StableId,
                DomainNameKo = names.TryGetValue(value.분야StableId,
                    out var name) ? name : value.분야StableId,
                DominantProgressKindCode = dominant.Item1,
                QualitativeStageCode = dominant.Item3,
            };
        }

        private static int Total(Simulation분야진척Snapshot value)
            => value.이해도 + value.현장숙련도 + value.운영숙련도;
    }
}
