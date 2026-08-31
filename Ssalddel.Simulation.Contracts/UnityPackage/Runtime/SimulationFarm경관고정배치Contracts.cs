using System;
using System.Collections.Generic;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1, "LS01 고정 장식과 단일 자산·제공 지면의 비권위 입력을 분리한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1공간계약, Boundary = "결속 후보는 실제 Unity Resolver 설치나 지면 검증 증거가 아니다.")]
    public sealed class SimulationFarm경관고정배치Request
    {
        public SimulationFarmH2PlacementRequest BaseRequest { get; set; } = new();
        public string DeltaJson { get; set; } = string.Empty;
        public string MeasurementsJson { get; set; } = string.Empty;
        public string BindingRevision { get; set; } = string.Empty;
        public string BindingHashSha256 { get; set; } = string.Empty;
        public SimulationFarm경관단일자산Binding[] Bindings { get; set; } = Array.Empty<SimulationFarm경관단일자산Binding>();
        public string SurfaceEvidenceKindCode { get; set; } = string.Empty;
        public string SurfaceEvidenceRef { get; set; } = string.Empty;
        // 추가 현장 점유/보호 입력. 좌표는 ownerCell local 미터이며 빈 집합도 명시적으로 봉인한다.
        public string ContextRevision { get; set; } = string.Empty;
        public string ContextHashSha256 { get; set; } = string.Empty;
        public SimulationFarmH2ReservedAreaSnapshot[] ExistingObstacles { get; set; } = Array.Empty<SimulationFarmH2ReservedAreaSnapshot>();
        public SimulationFarmH2ReservedAreaSnapshot[] AdditionalProtectedAreas { get; set; } = Array.Empty<SimulationFarmH2ReservedAreaSnapshot>();
    }

    public sealed class SimulationFarm경관단일자산Binding
    {
        public string VisualKey { get; set; } = string.Empty;
        public string CompositionKey { get; set; } = string.Empty;
        public string PrefabGuid { get; set; } = string.Empty;
        public string PrefabHashSha256 { get; set; } = string.Empty;
        public string MetaHashSha256 { get; set; } = string.Empty;
        public int SourceObjectCount { get; set; }
    }

    public sealed class SimulationFarm경관외곽Snapshot
    {
        public string PlacementStableId { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string PrefabGuid { get; set; } = string.Empty;
        public double RendererBottomMeters { get; set; }
        public SimulationFarmH2ReservedAreaSnapshot AllLodRendererBounds { get; set; } = new();
        public SimulationFarmH2ReservedAreaSnapshot ConservativeBounds { get; set; } = new();
        public int ActiveSolidColliderCount { get; set; }
        public string ActiveLodStatusCode { get; set; } = "NotEvaluatedAtRuntime";
    }

    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1, "A와 B의 별도 봉인 및 전체 표본·계획 계보를 반환한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1공간계약, Boundary = "제공 표면 검사는 실제 통행·Scene·E5와 다르다.")]
    public sealed class SimulationFarm경관고정배치Result
    {
        public string ServiceRevision { get; set; } = string.Empty;
        public string StatusCode { get; set; } = "UnapprovedCandidate";
        public string ValidationCode { get; set; } = "ValidatedAgainstProvidedSurface";
        public bool ActualTraversalVerified { get; set; }
        public bool ActualResolverVerified { get; set; }
        public string SurfaceEvidenceKindCode { get; set; } = string.Empty;
        public string SurfaceEvidenceRef { get; set; } = string.Empty;
        public string DeltaFileHashSha256 { get; set; } = string.Empty;
        public string DeltaInputHashSha256 { get; set; } = string.Empty;
        public string DeltaOutputHashSha256 { get; set; } = string.Empty;
        public string MeasurementFileHashSha256 { get; set; } = string.Empty;
        public string BindingHashSha256 { get; set; } = string.Empty;
        public string ConversionInputHashSha256 { get; set; } = string.Empty;
        public string SurfaceSamplesHashSha256 { get; set; } = string.Empty;
        public string ResultHashSha256 { get; set; } = string.Empty;
        public SimulationFarmH2PlacementResult BaseResult { get; set; } = new();
        public Simulation세계자산배치Plan Plan { get; set; } = new();
        public SimulationFarm경관외곽Snapshot[] Envelopes { get; set; } = Array.Empty<SimulationFarm경관외곽Snapshot>();
        public SortedDictionary<string,string> SurfaceSamples { get; set; } = new(StringComparer.Ordinal);
    }
}
