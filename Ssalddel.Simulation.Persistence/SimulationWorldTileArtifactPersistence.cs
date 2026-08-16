using Microsoft.EntityFrameworkCore;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Persistence;

/// <summary>
/// 파생 실행본 가운데 가장 최근에 저장된 완료 타일 산출물을 조회한다.
/// 원본 파일 본문은 객체 저장소 경계에 남겨 두고 DB에는 계보와 위치만 보관한다.
/// </summary>
public sealed class SimulationWorldTileArtifactReader(
    SimulationWorld파생DbContext dbContext) : ISimulationWorldTileArtifactReader
{
    public bool TryRead(
        string tileKey,
        string layerCode,
        out SimulationWorldTileArtifactSnapshot value)
    {
        var result = (
            from artifact in dbContext.UnityArtifacts.AsNoTracking()
            join tile in dbContext.UnityTileManifests.AsNoTracking()
                on new { artifact.RunId, StableId = artifact.TileManifestStableId }
                equals new { tile.RunId, tile.StableId }
            join run in dbContext.Runs.AsNoTracking() on artifact.RunId equals run.Id
            where tile.TileKey == tileKey
                && artifact.ArtifactKindCode == layerCode
                && artifact.StatusCode == SimulationWorldUnity산출물상태Codes.완료
                && artifact.StorageObjectKey != null
                && artifact.ArtifactHashSha256 != null
                && artifact.SourceRevision != null
                && artifact.SourceHashSha256 != null
                && artifact.HorizontalCrsCode != null
                && artifact.ResolutionMeters != null
                && artifact.ArtifactFormatCode != null
                && artifact.ArtifactByteLength != null
                && artifact.SampleWidth != null
                && artifact.SampleHeight != null
            orderby run.StoredAtUtc descending, artifact.Id descending
            select new
            {
                tile.TileKey,
                LayerCode = artifact.ArtifactKindCode,
                artifact.SourceRevision,
                artifact.ArtifactHashSha256,
                artifact.SourceHashSha256,
                artifact.SourceReferenceDate,
                artifact.HorizontalCrsCode,
                artifact.VerticalDatumCode,
                artifact.ResolutionMeters,
                artifact.NoDataValue,
                artifact.ArtifactFormatCode,
                ArtifactRelativePath = artifact.StorageObjectKey,
                artifact.ArtifactByteLength,
                artifact.SampleWidth,
                artifact.SampleHeight,
            }).FirstOrDefault();

        if (result == null)
        {
            value = new SimulationWorldTileArtifactSnapshot();
            return false;
        }

        value = new SimulationWorldTileArtifactSnapshot
        {
            TileKey = result.TileKey,
            LayerCode = result.LayerCode,
            SourceRevision = result.SourceRevision!,
            ArtifactHashSha256 = result.ArtifactHashSha256!,
            SourceHashSha256 = result.SourceHashSha256!,
            SourceReferenceDate = result.SourceReferenceDate,
            HorizontalCrsCode = result.HorizontalCrsCode!,
            VerticalDatumCode = result.VerticalDatumCode,
            ResolutionMeters = result.ResolutionMeters!.Value,
            NoDataValue = result.NoDataValue,
            ArtifactFormatCode = result.ArtifactFormatCode!,
            ArtifactRelativePath = result.ArtifactRelativePath!,
            ArtifactByteLength = result.ArtifactByteLength!.Value,
            SampleWidth = result.SampleWidth!.Value,
            SampleHeight = result.SampleHeight!.Value,
        };
        return true;
    }
}
