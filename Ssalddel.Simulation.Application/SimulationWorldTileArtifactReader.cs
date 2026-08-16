namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// 파생 World DB에 저장된 타일 산출물의 최신 완료 상태를 조회한다.
    /// 파일 본문을 읽거나 Unity 표현을 확정하는 책임은 갖지 않는다.
    /// </summary>
    public interface ISimulationWorldTileArtifactReader
    {
        bool TryRead(
            string tileKey,
            string layerCode,
            out SimulationWorldTileArtifactSnapshot value);
    }

    public sealed class SimulationWorldTileArtifactSnapshot
    {
        public string TileKey { get; set; } = string.Empty;
        public string LayerCode { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public string ArtifactHashSha256 { get; set; } = string.Empty;
        public string SourceHashSha256 { get; set; } = string.Empty;
        public string? SourceReferenceDate { get; set; }
        public string HorizontalCrsCode { get; set; } = string.Empty;
        public string? VerticalDatumCode { get; set; }
        public decimal ResolutionMeters { get; set; }
        public string? NoDataValue { get; set; }
        public string ArtifactFormatCode { get; set; } = string.Empty;
        public string ArtifactRelativePath { get; set; } = string.Empty;
        public long ArtifactByteLength { get; set; }
        public int SampleWidth { get; set; }
        public int SampleHeight { get; set; }
    }

    public sealed class DisabledSimulationWorldTileArtifactReader
        : ISimulationWorldTileArtifactReader
    {
        public bool TryRead(
            string tileKey,
            string layerCode,
            out SimulationWorldTileArtifactSnapshot value)
        {
            value = new SimulationWorldTileArtifactSnapshot();
            return false;
        }
    }
}
