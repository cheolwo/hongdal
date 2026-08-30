using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    [SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
        "원후보·실측·Player 접근 근거를 가진 비권위 Farm 부지 개정 입력이다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1공간계약,
        Boundary = "부지 개정은 생산100㎡·WI 상태·실제 통행·E 승격을 변경하지 않는다.")]
    public sealed class SimulationFarmH2부지확장Request
    {
        public SimulationFarmH2PlacementRequest ParentRequest { get; set; } = new();
        public string StudyHashSha256 { get; set; } = string.Empty;
        public string MeasurementSourceHashSha256 { get; set; } = string.Empty;
        public string BarnCode { get; set; } = "Barn01";
        public SimulationFarmH2접근측정 Player { get; set; } = new();
        // identity Wrapper에서 renderer 중심 대비 보수적인 Renderer+Collider 수평 합집합.
        public SimulationFarmH2외곽측정 BarnSolidBounds { get; set; } = new();
        public string BarnSolidMeasurementHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationFarmH2접근측정
    {
        public string EvidenceRef { get; set; } = string.Empty;
        public string EvidenceHashSha256 { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public double RadiusMeters { get; set; }
        public double SkinWidthMeters { get; set; }
        public double ClickStopDistanceMeters { get; set; }
        public double HeightMeters { get; set; }
        public double StepOffsetMeters { get; set; }
        public double SlopeLimitDegrees { get; set; }
        public double UniformScale { get; set; } = 1;
    }

    public sealed class SimulationFarmH2외곽측정
    {
        public double MinX { get; set; }
        public double MinZ { get; set; }
        public double MaxX { get; set; }
        public double MaxZ { get; set; }
    }

    public sealed class SimulationFarmH2부지확장Result
    {
        public string Revision { get; set; } = string.Empty;
        public string ParentCandidateHashSha256 { get; set; } = string.Empty;
        public string StudyHashSha256 { get; set; } = string.Empty;
        public string MeasurementSourceHashSha256 { get; set; } = string.Empty;
        public double PlayerCenterClearanceMeters { get; set; }
        public double AccessLaneWidthMeters { get; set; }
        public double ReservationWidthMeters { get; set; }
        public double ReservationDepthMeters { get; set; }
        public SimulationFarmH2PlacementRequest CandidateRequest { get; set; } = new();
        public SimulationFarmH2PlacementResult? ValidatedPlacement { get; set; }
        public bool ActualTraversalVerified { get; set; }
    }
}
