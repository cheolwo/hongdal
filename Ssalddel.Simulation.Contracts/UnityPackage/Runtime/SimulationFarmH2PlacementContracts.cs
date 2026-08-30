using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    /// <summary>Farm 후보를 현행 배치 형식에 연결하는 비권위 입력. Prefab 경로/GUID는 포함하지 않는다.</summary>
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "Farm H2 후보·셀·소유·시각 측정 입력을 분리한다.", SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1공간계약,
        Boundary = "입력 계약은 실제 Prefab 측정·Scene 배치 또는 E5 발현을 증명하지 않는다.")]
    public sealed class SimulationFarmH2PlacementRequest
    {
        public string CandidateJson { get; set; } = string.Empty;
        public string ExpectedCandidateHashSha256 { get; set; } = string.Empty;
        public Simulation지도구성Plan MapPlan { get; set; } = new();
        public string OwnerCellStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string H2StableId { get; set; } = string.Empty;
        public string UnitCode { get; set; } = "Meters";
        public string AxisCode { get; set; } = "XRightYUpZForward";
        public double CellSizeMeters { get; set; }
        public double CellWorldOriginXMeters { get; set; }
        public double CellWorldOriginYMeters { get; set; }
        public double CellWorldOriginZMeters { get; set; }
        public double LocalOriginXMeters { get; set; }
        public double LocalOriginYMeters { get; set; }
        public double LocalOriginZMeters { get; set; }
        public double RotationDegrees { get; set; }
        public double UniformScale { get; set; } = 1d;
        public string ResolverRevision { get; set; } = string.Empty;
        public string ResolverHashSha256 { get; set; } = string.Empty;
        public string[] ResolvedCompositionKeys { get; set; } = Array.Empty<string>();
        public SimulationFarmH2PlacementBinding[] Bindings { get; set; } = Array.Empty<SimulationFarmH2PlacementBinding>();
        public SimulationFarmH2MeasurementPolicy Policy { get; set; } = new();
        public string SurfaceRevision { get; set; } = string.Empty;
        public string SurfaceHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmH2PlacementBinding
    {
        public string SourcePlacementStableId { get; set; } = string.Empty;
        public string PlacementStableId { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string AssetFamilyId { get; set; } = string.Empty;
        public string CompositionKey { get; set; } = string.Empty;
        // 생산 표시 외곽과 별개인, 통행을 포함한 H1 작업공간의 실제 미터 치수.
        public double WorkAreaWidthMeters { get; set; }
        public double WorkAreaDepthMeters { get; set; }
        public string WorkAreaEvidenceRef { get; set; } = string.Empty;
        public SimulationFarmH2AssetMeasurement Measurement { get; set; } = new();
    }

    /// <summary>측정 당시 회전 0/Scale 1에서 pivot 기준 활성 외곽. 실제/합성 근거를 구분한다.</summary>
    public sealed class SimulationFarmH2AssetMeasurement
    {
        public string Revision { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty; // SyntheticFixture / MeasuredWrapper
        public string EvidenceRef { get; set; } = string.Empty;
        public string AssetFingerprintSha256 { get; set; } = string.Empty;
        public string MeasurementHashSha256 { get; set; } = string.Empty;
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double CenterZ { get; set; }
        public double SizeX { get; set; }
        public double SizeY { get; set; }
        public double SizeZ { get; set; }
        public double UniformScale { get; set; } = 1d;
        public bool ActiveRenderer { get; set; }
        public bool ActiveCollider { get; set; }
    }

    /// <summary>값은 측정/기존 설정의 출처와 함께 호출자가 제공한다. 기본 게임 한계는 없다.</summary>
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "배치 측정 허용값과 시험/실측 출처를 명시하는 입력 계약이다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1공간계약,
        Boundary = "이 입력은 플레이어 이동 한계나 새 게임 규칙을 확정하지 않는다.")]
    public sealed class SimulationFarmH2MeasurementPolicy
    {
        public string Revision { get; set; } = string.Empty;
        public string EvidenceRef { get; set; } = string.Empty;
        public bool TrialOnly { get; set; } = true;
        public double MaximumSlopeDegrees { get; set; }
        public double MaximumHeightSpreadMeters { get; set; }
        public double GroundClearanceMeters { get; set; }
        public double BottomToleranceMeters { get; set; }
        public double MinimumSpacingMeters { get; set; }
        public double MinimumRouteWidthMeters { get; set; }
        public double RouteSampleStepMeters { get; set; }
        public double MaximumRouteSlopeDegrees { get; set; }
        public double MaximumRouteStepMeters { get; set; }
    }

    public sealed class SimulationFarmH2SurfaceSample
    {
        public bool Supported { get; set; }
        public bool PlacementAllowed { get; set; }
        public double HeightMeters { get; set; }
        public double SlopeDegrees { get; set; }
    }

    // 지면 공급자는 읽기 전용 조회이며 후보나 Terrain을 보정하지 않는다.
    public interface ISimulationFarmH2SurfaceReader
    {
        string Revision { get; }
        string HashSha256 { get; }
        SimulationFarmH2SurfaceSample Read(double worldX, double worldZ);
    }

    public sealed class SimulationFarmH2AnchorSnapshot
    {
        public string SourceAnchorStableId { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string OwnerPlacementStableId { get; set; } = string.Empty;
        public string H2StableId { get; set; } = string.Empty;
        public double LocalXMeters { get; set; }
        public double LocalZMeters { get; set; }
        public string FacingCode { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmH2RouteSnapshot
    {
        public string SourceRouteStableId { get; set; } = string.Empty;
        public string FromSourceAnchorStableId { get; set; } = string.Empty;
        public string ToSourceAnchorStableId { get; set; } = string.Empty;
        public double WidthMeters { get; set; }
    }

    public sealed class SimulationFarmH2ReservedAreaSnapshot
    {
        public string SourceStableId { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public double MinX { get; set; }
        public double MinZ { get; set; }
        public double MaxX { get; set; }
        public double MaxZ { get; set; }
    }

    public sealed class SimulationFarmH2PlacementResult
    {
        public string AdapterRevision { get; set; } = string.Empty;
        public string CandidateHashSha256 { get; set; } = string.Empty;
        public string CandidateInputHashSha256 { get; set; } = string.Empty;
        public string CandidateSurfaceHashSha256 { get; set; } = string.Empty;
        public string CandidateSeed { get; set; } = string.Empty;
        public string CandidatePatternRevision { get; set; } = string.Empty;
        public string ConversionInputHashSha256 { get; set; } = string.Empty;
        public string SurfaceSamplesHashSha256 { get; set; } = string.Empty;
        public string ConversionOutputHashSha256 { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string H2StableId { get; set; } = string.Empty;
        public bool ContainsSyntheticMeasurements { get; set; }
        public bool PolicyIsTrial { get; set; }
        public string PatternStatusCode { get; set; } = "UnapprovedCandidate";
        public bool ActualTraversalVerified { get; set; } // 변환기는 true로 설정하지 않는다.
        public Simulation세계자산배치Plan Plan { get; set; } = new();
        public SimulationFarmH2AnchorSnapshot[] Anchors { get; set; } = Array.Empty<SimulationFarmH2AnchorSnapshot>();
        public SimulationFarmH2RouteSnapshot[] Routes { get; set; } = Array.Empty<SimulationFarmH2RouteSnapshot>();
        public SimulationFarmH2ReservedAreaSnapshot[] ReservedAreas { get; set; } = Array.Empty<SimulationFarmH2ReservedAreaSnapshot>();
    }
}
