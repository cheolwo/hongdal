using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationWorldStreamCodes
    {
        public const string PyeongchangFarmRecipe =
            "world-stream:kr:51760:daegwallyeong-farm.v1";
        public const string ElevationLayer = "elevation";
        public const string LandCoverLayer = "land-cover";
        public const string PlacementMaskLayer = "placement-mask";
        public const string WaitingForSpatialArtifact = "WaitingForSpatialArtifact";
        public const string Available = "Available";
        public const string Observed = "Observed";
        public const string Derived = "Derived";
        public const string PresentationOnly = "PresentationOnly";
        public const string Scenario = "Scenario";
        public const string BuildingObject = "Building";
        public const string RegionSummaryWaitingForDerivedData = "WaitingForRegionSummary";
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldStreaming,
        SsalddelCodeLayer.Contract,
        "L2 타일 활성·준비 범위와 사전 적재 규칙을 전달한다.",
        StepKey = "contract.stream-recipe",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        FlowOrder = 10,
        Boundary = "Recipe는 제공 범위와 로드 정책이며 Unity가 전체 타일을 동시에 생성하라는 명령이 아니다.")]
    public sealed class SimulationWorldStreamRecipeResponse
    {
        public string RecipeStableId { get; set; } = string.Empty;
        public string RecipeRevision { get; set; } = string.Empty;
        public string RecipeHashSha256 { get; set; } = string.Empty;
        public string CoordinateReferenceSystem { get; set; } = string.Empty;
        public int TileLevel { get; set; }
        public int TileSizeMeters { get; set; }
        public int DetailRadius { get; set; }
        public int ActiveRadius { get; set; }
        public int PrefetchRadius { get; set; }
        public int MaxConcurrentTileLoads { get; set; }
        public double BoundaryPrefetchFraction { get; set; }
        public int CenterTileX { get; set; }
        public int CenterTileY { get; set; }
        public string[] CoverageTileKeys { get; set; } = Array.Empty<string>();
        public string[] LayerCodes { get; set; } = Array.Empty<string>();
        public string RegionSummaryProfileRevision { get; set; } = string.Empty;
        public string RegionSummaryProfileHashSha256 { get; set; } = string.Empty;
        public string[] SupportedSummaryLodCodes { get; set; } = Array.Empty<string>();
        public bool IsOperationalState { get; set; }
        public string EvidenceKindCode { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldTileStreamManifestResponse
    {
        public string RecipeStableId { get; set; } = string.Empty;
        public string TileKey { get; set; } = string.Empty;
        public int TileLevel { get; set; }
        public int TileX { get; set; }
        public int TileY { get; set; }
        public int HaloMeters { get; set; }
        public string ManifestRevision { get; set; } = string.Empty;
        public string ManifestHashSha256 { get; set; } = string.Empty;
        public string RegionSummaryProfileRevision { get; set; } = string.Empty;
        public string RegionSummaryStatusCode { get; set; } = string.Empty;
        public string? RegionSummaryHashSha256 { get; set; }
        public SimulationWorldTileLayerDescriptorResponse[] Layers { get; set; }
            = Array.Empty<SimulationWorldTileLayerDescriptorResponse>();
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldTileLayerDescriptorResponse
    {
        public string LayerCode { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public string? ArtifactHashSha256 { get; set; }
        public string? ArtifactRelativePath { get; set; }
        public string? ArtifactContentPath { get; set; }
        public string? SourceHashSha256 { get; set; }
        public string? SourceReferenceDate { get; set; }
        public string? HorizontalCrsCode { get; set; }
        public string? VerticalDatumCode { get; set; }
        public decimal? ResolutionMeters { get; set; }
        public string? NoDataValue { get; set; }
        public string? ArtifactFormatCode { get; set; }
        public long? ArtifactByteLength { get; set; }
        public int? SampleWidth { get; set; }
        public int? SampleHeight { get; set; }
        public bool PresentationOnly { get; set; }
    }

    public sealed class SimulationWorldTileArtifactDescriptorResponse
    {
        public string TileKey { get; set; } = string.Empty;
        public string LayerCode { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public string? ArtifactHashSha256 { get; set; }
        public string? ArtifactRelativePath { get; set; }
        public string? ArtifactContentPath { get; set; }
        public string? SourceHashSha256 { get; set; }
        public string? SourceReferenceDate { get; set; }
        public string? HorizontalCrsCode { get; set; }
        public string? VerticalDatumCode { get; set; }
        public decimal? ResolutionMeters { get; set; }
        public string? NoDataValue { get; set; }
        public string? ArtifactFormatCode { get; set; }
        public long? ArtifactByteLength { get; set; }
        public int? SampleWidth { get; set; }
        public int? SampleHeight { get; set; }
        public bool PresentationOnly { get; set; }
        public string KoreanStatusLabel { get; set; } = string.Empty;
    }

    public sealed class SimulationWorldTileActivityProjectionResponse
    {
        public string TileKey { get; set; } = string.Empty;
        public long ActivityRevision { get; set; }
        public int WorldTick { get; set; }
        public string[] ActivityStableIds { get; set; } = Array.Empty<string>();
        public bool PresentationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldTileObjectProjectionResponse
    {
        public string TileKey { get; set; } = string.Empty;
        public string PlacementRevision { get; set; } = string.Empty;
        public string PlacementHashSha256 { get; set; } = string.Empty;
        public SimulationWorldTileObjectPlacementResponse[] Objects { get; set; }
            = Array.Empty<SimulationWorldTileObjectPlacementResponse>();
        public bool PresentationOnly { get; set; }
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationWorldTileObjectPlacementResponse
    {
        public string ObjectStableId { get; set; } = string.Empty;
        public string ObjectTypeCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string LandCoverCode { get; set; } = string.Empty;
        public string RegionRoleCode { get; set; } = string.Empty;
        public double LocalOffsetXMeters { get; set; }
        public double LocalOffsetYMeters { get; set; }
        public double RotationDegrees { get; set; }
        public double FootprintWidthMeters { get; set; }
        public double FootprintDepthMeters { get; set; }
        public double HeightMeters { get; set; }
        public bool CollisionEligible { get; set; }
        public bool PresentationOnly { get; set; }
    }
}
