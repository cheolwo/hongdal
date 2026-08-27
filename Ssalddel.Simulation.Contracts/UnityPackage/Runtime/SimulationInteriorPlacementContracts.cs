using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>
    /// Simulation 저장 경계가 원본 계획 전체 대신 동결하는 Interior 배치 계획 손잡이다.
    /// 외부 자료 원문, 자산 내부 식별자와 운영 원장은 포함하지 않는다.
    /// </summary>
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "세션이 동결하는 Interior 배치 계획의 최소 계약과 hash 경계를 정의한다.",
        Boundary = "상태 사본 계약은 실제 Unity 배치·통행 또는 E5 증거를 만들지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1저장재생계약)]
    public sealed class SimulationInteriorPlanHandleSnapshot
    {
        public string SchemaVersion { get; set; } = string.Empty;
        public string BuildingPlacementStableId { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public string InteriorDefinitionRevision { get; set; } = string.Empty;
        public string ReferenceCatalogRevision { get; set; } = string.Empty;
        public string ReferenceCatalogHashSha256 { get; set; } = string.Empty;
        public string PlacementControlRuleRevision { get; set; } = string.Empty;
        public string VisualMetricCatalogRevision { get; set; } = string.Empty;
        public string VisualMetricCatalogHashSha256 { get; set; } = string.Empty;
        public string AdjustmentRevision { get; set; } = string.Empty;
        public string InteriorPlacementPlanHashSha256 { get; set; } = string.Empty;
    }

    public static class SimulationInteriorPlanHandleCodes
    {
        public const string SchemaVersion = "interior-placement-plan.v2";
        public const string PlacementControlRuleRevision =
            "placement-control-hierarchy.v2";
    }
}
