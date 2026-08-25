using System;
using System.Collections.Generic;
using Ssalddel.Interior.Contracts;

namespace Ssalddel.Interior.Domain
{
    /// <summary>
    /// Solo Local과 Hosted가 같은 의미로 사용할 수 있는 불변 Plan 대장이다.
    /// 영속 Adapter는 이 계약의 hash 충돌 거부 규칙을 그대로 유지해야 한다.
    /// </summary>
    public sealed class InteriorPlacementPlanCatalog
    {
        private readonly Dictionary<string, InteriorPlacementPlan> plans =
            new(StringComparer.Ordinal);

        public InteriorPlanHandle Pin(InteriorPlacementPlan plan)
        {
            if (plan is null) throw new ArgumentNullException(nameof(plan));
            var expectedHash = InteriorLayoutHash.ComputePlanHash(plan);
            if (!string.Equals(
                    expectedHash,
                    plan.InteriorPlacementPlanHashSha256,
                    StringComparison.Ordinal))
                throw new ArgumentException("InteriorPlacementPlan hash가 내용과 일치하지 않습니다.", nameof(plan));
            if (plans.TryGetValue(expectedHash, out var existing)
                && !string.Equals(
                    existing.BuildingPlacementStableId,
                    plan.BuildingPlacementStableId,
                    StringComparison.Ordinal))
                throw new InvalidOperationException("InteriorPlacementPlan hash 충돌이 감지되었습니다.");
            plans[expectedHash] = plan;
            return new InteriorPlanHandle
            {
                SchemaVersion = plan.SchemaVersion,
                BuildingPlacementStableId = plan.BuildingPlacementStableId,
                H1StableId = plan.H1StableId,
                InteriorDefinitionRevision = plan.InteriorDefinitionRevision,
                ReferenceCatalogRevision = plan.ReferenceCatalogRevision,
                ReferenceCatalogHashSha256 = plan.ReferenceCatalogHashSha256,
                PlacementControlRuleRevision =
                    plan.PlacementControlRuleRevision,
                VisualMetricCatalogRevision =
                    plan.VisualMetricCatalogRevision,
                VisualMetricCatalogHashSha256 =
                    plan.VisualMetricCatalogHashSha256,
                AdjustmentRevision = plan.AdjustmentRevision,
                InteriorPlacementPlanHashSha256 = expectedHash,
            };
        }

        public InteriorPlacementPlan GetRequired(string planHashSha256)
        {
            if (string.IsNullOrWhiteSpace(planHashSha256))
                throw new ArgumentException("Plan hash가 필요합니다.", nameof(planHashSha256));
            return plans.TryGetValue(planHashSha256.Trim(), out var plan)
                ? plan
                : throw new KeyNotFoundException("고정된 InteriorPlacementPlan을 찾을 수 없습니다.");
        }
    }
}
