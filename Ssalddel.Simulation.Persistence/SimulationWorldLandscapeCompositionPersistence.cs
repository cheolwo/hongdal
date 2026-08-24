using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssalddel.Simulation.Application;
using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Persistence;

public sealed class SimulationWorld경관조립실행Entity
{
    public long Id { get; set; }
    public string GraphBuildStableId { get; set; } = string.Empty;
    public string LandscapeGraphStableId { get; set; } = string.Empty;
    public string BuildScopeCode { get; set; } = string.Empty;
    public string GraphRoleCode { get; set; } = string.Empty;
    public int GraphRevision { get; set; }
    public string DefinitionHashSha256 { get; set; } = string.Empty;
    public string TileKey { get; set; } = string.Empty;
    public string AreaSetStableId { get; set; } = string.Empty;
    public string GrammarRevision { get; set; } = string.Empty;
    public string GrammarHashSha256 { get; set; } = string.Empty;
    public string GraphHashSha256 { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public bool PresentationOnly { get; set; }
    public DateTimeOffset StoredAtUtc { get; set; }
}

public sealed class SimulationWorld경관공간NodeEntity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string NodeStableId { get; set; } = string.Empty;
    public string ParentNodeStableId { get; set; } = string.Empty;
    public string NodeKindCode { get; set; } = string.Empty;
    public string SemanticCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public double CenterEastingMeters { get; set; }
    public double CenterNorthingMeters { get; set; }
    public double WidthMeters { get; set; }
    public double DepthMeters { get; set; }
    public SimulationWorld경관조립실행Entity? Run { get; set; }
}

public sealed class SimulationWorld경관공간EdgeEntity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string EdgeStableId { get; set; } = string.Empty;
    public string FromNodeStableId { get; set; } = string.Empty;
    public string RelationCode { get; set; } = string.Empty;
    public string ToNodeStableId { get; set; } = string.Empty;
    public string ConnectorTypeCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public bool IsExternalStub { get; set; }
    public string NeighborTileKey { get; set; } = string.Empty;
    public string PlacementStableId { get; set; } = string.Empty;
    public string RouteSignature { get; set; } = string.Empty;
    public string DirectionCode { get; set; } = string.Empty;
    public double WorldEastingMeters { get; set; }
    public double WorldNorthingMeters { get; set; }
    public double WidthMeters { get; set; }
    public SimulationWorld경관조립실행Entity? Run { get; set; }
}

public sealed class SimulationWorld경관모판배치Entity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string PlacementStableId { get; set; } = string.Empty;
    public string NodeStableId { get; set; } = string.Empty;
    public string OwnerTileKey { get; set; } = string.Empty;
    public string CompositionKey { get; set; } = string.Empty;
    public string TopologyCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public double EastingMeters { get; set; }
    public double NorthingMeters { get; set; }
    public double PhysicalElevationMeters { get; set; }
    public double RotationDegrees { get; set; }
    public bool Mirrored { get; set; }
    public int DeterministicSeed { get; set; }
    public double FootprintWidthMeters { get; set; }
    public double FootprintDepthMeters { get; set; }
    public bool PresentationOnly { get; set; }
    public SimulationWorld경관조립실행Entity? Run { get; set; }
}

public sealed class SimulationWorld경관조립미해결Entity
{
    public long Id { get; set; }
    public long RunId { get; set; }
    public string UnresolvedStableId { get; set; } = string.Empty;
    public string NodeStableId { get; set; } = string.Empty;
    public string ReasonCode { get; set; } = string.Empty;
    public string RequiredSemanticCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public SimulationWorld경관조립실행Entity? Run { get; set; }
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "공간 WI의 경관 조립 문맥과 계보를 저장한다.",
    Boundary = "경관 조립 저장만으로 권위 상태 전이와 결과 발현을 증명하지 않는다.")]
public sealed class SimulationWorldLandscapeCompositionStore(
    SimulationWorld파생DbContext dbContext)
    : ISimulationWorldLandscapeCompositionStore,
      ISimulationWorldLandscapeCompositionReader
{
    public async Task ReplaceBuildAsync(
        IReadOnlyList<SimulationWorldLandscapeCompositionTileResponse> tiles,
        CancellationToken cancellationToken = default)
    {
        var buildIds = tiles.Select(item => item.GraphBuildStableId).ToList();
        var oldRuns = await dbContext.LandscapeAssemblyRuns
            .Where(item => buildIds.Contains(item.GraphBuildStableId))
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        if (oldRuns.Length > 0)
        {
            dbContext.LandscapeAssemblyRuns.RemoveRange(oldRuns);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        foreach (var tile in tiles)
        {
            var run = new SimulationWorld경관조립실행Entity
            {
                GraphBuildStableId = tile.GraphBuildStableId,
                LandscapeGraphStableId = "legacy-landscape-graph:" + tile.TileKey,
                BuildScopeCode = SimulationWorldLandscapeCompositionCodes.LegacyTileBuildScope,
                GraphRoleCode = "LegacyTile",
                GraphRevision = 1,
                DefinitionHashSha256 = new string('0', 64),
                TileKey = tile.TileKey,
                AreaSetStableId = tile.AreaSetStableId,
                GrammarRevision = tile.GrammarRevision,
                GrammarHashSha256 = tile.GrammarHashSha256,
                GraphHashSha256 = tile.GraphHashSha256,
                StatusCode = tile.StatusCode,
                PresentationOnly = tile.PresentationOnly,
                StoredAtUtc = DateTimeOffset.UtcNow,
            };
            dbContext.LandscapeAssemblyRuns.Add(run);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            dbContext.LandscapeNodes.AddRange(tile.Nodes.Select(item => new SimulationWorld경관공간NodeEntity
            {
                RunId = run.Id,
                NodeStableId = item.NodeStableId,
                ParentNodeStableId = item.ParentNodeStableId,
                NodeKindCode = item.NodeKindCode,
                SemanticCode = item.SemanticCode,
                EvidenceKindCode = item.EvidenceKindCode,
                CenterEastingMeters = item.CenterEastingMeters,
                CenterNorthingMeters = item.CenterNorthingMeters,
                WidthMeters = item.WidthMeters,
                DepthMeters = item.DepthMeters,
            }));
            dbContext.LandscapeEdges.AddRange(tile.Edges.Select(item => new SimulationWorld경관공간EdgeEntity
            {
                RunId = run.Id,
                EdgeStableId = item.EdgeStableId,
                FromNodeStableId = item.FromNodeStableId,
                RelationCode = item.RelationCode,
                ToNodeStableId = item.ToNodeStableId,
                ConnectorTypeCode = item.ConnectorTypeCode,
                EvidenceKindCode = item.EvidenceKindCode,
            }));
            dbContext.LandscapeEdges.AddRange(tile.ExternalConnectorStubs.Select(item =>
                new SimulationWorld경관공간EdgeEntity
                {
                    RunId = run.Id,
                    EdgeStableId = item.StubStableId,
                    IsExternalStub = true,
                    NeighborTileKey = item.NeighborTileKey,
                    PlacementStableId = item.PlacementStableId,
                    ConnectorTypeCode = item.ConnectorTypeCode,
                    RouteSignature = item.RouteSignature,
                    DirectionCode = item.DirectionCode,
                    EvidenceKindCode = item.EvidenceKindCode,
                    WorldEastingMeters = item.WorldEastingMeters,
                    WorldNorthingMeters = item.WorldNorthingMeters,
                    WidthMeters = item.WidthMeters,
                }));
            dbContext.LandscapePlacements.AddRange(tile.Placements.Select(item =>
                new SimulationWorld경관모판배치Entity
                {
                    RunId = run.Id,
                    PlacementStableId = item.PlacementStableId,
                    NodeStableId = item.NodeStableId,
                    OwnerTileKey = item.OwnerTileKey,
                    CompositionKey = item.CompositionKey,
                    TopologyCode = item.TopologyCode,
                    EvidenceKindCode = item.EvidenceKindCode,
                    EastingMeters = item.EastingMeters,
                    NorthingMeters = item.NorthingMeters,
                    PhysicalElevationMeters = item.PhysicalElevationMeters,
                    RotationDegrees = item.RotationDegrees,
                    Mirrored = item.Mirrored,
                    DeterministicSeed = item.DeterministicSeed,
                    FootprintWidthMeters = item.FootprintWidthMeters,
                    FootprintDepthMeters = item.FootprintDepthMeters,
                    PresentationOnly = item.PresentationOnly,
                }));
            dbContext.LandscapeUnresolved.AddRange(tile.Unresolved.Select(item =>
                new SimulationWorld경관조립미해결Entity
                {
                    RunId = run.Id,
                    UnresolvedStableId = item.UnresolvedStableId,
                    NodeStableId = item.NodeStableId,
                    ReasonCode = item.ReasonCode,
                    RequiredSemanticCode = item.RequiredSemanticCode,
                    EvidenceKindCode = item.EvidenceKindCode,
                    Detail = item.Detail,
                }));
        }
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<SimulationWorldLandscapeCompositionTileResponse?> ReadLatestAsync(
        string tileKey,
        CancellationToken cancellationToken = default)
    {
        var run = await dbContext.LandscapeAssemblyRuns.AsNoTracking()
            .Where(item => item.TileKey == tileKey)
            .OrderByDescending(item => item.StoredAtUtc)
            .ThenByDescending(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        if (run == null) return null;

        var nodes = await dbContext.LandscapeNodes.AsNoTracking()
            .Where(item => item.RunId == run.Id).OrderBy(item => item.NodeStableId)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var edges = await dbContext.LandscapeEdges.AsNoTracking()
            .Where(item => item.RunId == run.Id).OrderBy(item => item.EdgeStableId)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var placements = await dbContext.LandscapePlacements.AsNoTracking()
            .Where(item => item.RunId == run.Id).OrderBy(item => item.PlacementStableId)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);
        var unresolved = await dbContext.LandscapeUnresolved.AsNoTracking()
            .Where(item => item.RunId == run.Id).OrderBy(item => item.UnresolvedStableId)
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        return new SimulationWorldLandscapeCompositionTileResponse
        {
            TileKey = run.TileKey,
            AreaSetStableId = run.AreaSetStableId,
            GraphBuildStableId = run.GraphBuildStableId,
            GraphHashSha256 = run.GraphHashSha256,
            GrammarRevision = run.GrammarRevision,
            GrammarHashSha256 = run.GrammarHashSha256,
            StatusCode = run.StatusCode,
            Nodes = nodes.Select(item => new SimulationWorldLandscapeNodeResponse
            {
                NodeStableId = item.NodeStableId,
                ParentNodeStableId = item.ParentNodeStableId,
                NodeKindCode = item.NodeKindCode,
                SemanticCode = item.SemanticCode,
                EvidenceKindCode = item.EvidenceKindCode,
                CenterEastingMeters = item.CenterEastingMeters,
                CenterNorthingMeters = item.CenterNorthingMeters,
                WidthMeters = item.WidthMeters,
                DepthMeters = item.DepthMeters,
            }).ToArray(),
            Edges = edges.Where(item => !item.IsExternalStub).Select(item =>
                new SimulationWorldLandscapeEdgeResponse
                {
                    EdgeStableId = item.EdgeStableId,
                    FromNodeStableId = item.FromNodeStableId,
                    RelationCode = item.RelationCode,
                    ToNodeStableId = item.ToNodeStableId,
                    ConnectorTypeCode = item.ConnectorTypeCode,
                    EvidenceKindCode = item.EvidenceKindCode,
                }).ToArray(),
            Placements = placements.Select(item => new SimulationWorldLandscapePlacementResponse
            {
                PlacementStableId = item.PlacementStableId,
                NodeStableId = item.NodeStableId,
                OwnerTileKey = item.OwnerTileKey,
                CompositionKey = item.CompositionKey,
                TopologyCode = item.TopologyCode,
                EvidenceKindCode = item.EvidenceKindCode,
                EastingMeters = item.EastingMeters,
                NorthingMeters = item.NorthingMeters,
                PhysicalElevationMeters = item.PhysicalElevationMeters,
                RotationDegrees = item.RotationDegrees,
                Mirrored = item.Mirrored,
                DeterministicSeed = item.DeterministicSeed,
                FootprintWidthMeters = item.FootprintWidthMeters,
                FootprintDepthMeters = item.FootprintDepthMeters,
                PresentationOnly = item.PresentationOnly,
            }).ToArray(),
            ExternalConnectorStubs = edges.Where(item => item.IsExternalStub).Select(item =>
                new SimulationWorldLandscapeExternalConnectorResponse
                {
                    StubStableId = item.EdgeStableId,
                    PlacementStableId = item.PlacementStableId,
                    NeighborTileKey = item.NeighborTileKey,
                    ConnectorTypeCode = item.ConnectorTypeCode,
                    RouteSignature = item.RouteSignature,
                    DirectionCode = item.DirectionCode,
                    EvidenceKindCode = item.EvidenceKindCode,
                    WorldEastingMeters = item.WorldEastingMeters,
                    WorldNorthingMeters = item.WorldNorthingMeters,
                    WidthMeters = item.WidthMeters,
                }).ToArray(),
            Unresolved = unresolved.Select(item => new SimulationWorldLandscapeUnresolvedResponse
            {
                UnresolvedStableId = item.UnresolvedStableId,
                NodeStableId = item.NodeStableId,
                ReasonCode = item.ReasonCode,
                RequiredSemanticCode = item.RequiredSemanticCode,
                EvidenceKindCode = item.EvidenceKindCode,
                Detail = item.Detail,
            }).ToArray(),
            PresentationOnly = run.PresentationOnly,
            IsOperationalState = false,
        };
    }
}

public sealed class SimulationWorldLandscapeGrammarManifestReader(string manifestPath)
    : ISimulationWorldLandscapeGrammarCatalogReader
{
    public bool TryRead(out SimulationWorldLandscapeGrammarCatalog catalog, out string errorCode)
    {
        catalog = new SimulationWorldLandscapeGrammarCatalog();
        var resolvedPath = ResolveManifestPath(manifestPath);
        if (resolvedPath == null)
        {
            errorCode = SimulationWorldLandscapeCompositionCodes.WaitingForGrammarManifest;
            return false;
        }

        var json = File.ReadAllText(resolvedPath, Encoding.UTF8);
        if (json.Contains("Assets/", StringComparison.OrdinalIgnoreCase)
            || json.Contains(".prefab", StringComparison.OrdinalIgnoreCase)
            || json.Contains("\"guid\"", StringComparison.OrdinalIgnoreCase))
        {
            errorCode = SimulationWorldLandscapeCompositionCodes.CatalogMismatch;
            return false;
        }

        var document = JsonSerializer.Deserialize<ManifestDocument>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (document == null)
        {
            errorCode = SimulationWorldLandscapeCompositionCodes.CatalogMismatch;
            return false;
        }
        if (!string.Equals(document.CatalogHashSha256,
                ComputeManifestCatalogHash(document), StringComparison.OrdinalIgnoreCase))
        {
            errorCode = SimulationWorldLandscapeCompositionCodes.CatalogMismatch;
            return false;
        }

        catalog = new SimulationWorldLandscapeGrammarCatalog
        {
            SchemaVersion = document.SchemaVersion,
            CatalogRevision = document.CatalogRevision,
            CatalogHashSha256 = document.CatalogHashSha256,
            PresentationOnly = document.PresentationOnly,
            Entries = document.Entries.Select(item => new SimulationWorldLandscapeGrammarEntry
            {
                CompositionKey = item.CompositionKey,
                SetName = item.SetName,
                VariantCode = item.VariantCode,
                FamilyCode = item.FamilyCode,
                TopologyCode = item.TopologyCode,
                FootprintX = item.FootprintX,
                FootprintY = item.FootprintY,
                MaxConsecutive = item.MaxConsecutive,
                RecentWindowSize = item.RecentWindowSize,
                MirrorAllowed = item.MirrorAllowed,
                RotationCodes = item.RotationCodes,
                Connectors = item.Connectors.Select(value => new SimulationWorldLandscapeGrammarConnector
                {
                    ConnectorTypeCode = value.ConnectorTypeCode,
                    DirectionCode = value.DirectionCode,
                    RouteSignature = value.RouteSignature,
                    LocalX = value.LocalX,
                    LocalZ = value.LocalZ,
                    Width = value.Width,
                }).ToArray(),
                PresentationOnly = item.PresentationOnly,
            }).ToArray(),
        };
        try
        {
            catalog.ValidateCanonicalCatalog();
            errorCode = string.Empty;
            return true;
        }
        catch (InvalidOperationException)
        {
            errorCode = SimulationWorldLandscapeCompositionCodes.CatalogMismatch;
            catalog = new SimulationWorldLandscapeGrammarCatalog();
            return false;
        }
    }

    private static string? ResolveManifestPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (Path.IsPathRooted(value)) return File.Exists(value) ? value : null;
        for (var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
             directory != null;
             directory = directory.Parent)
        {
            var candidate = Path.GetFullPath(Path.Combine(directory.FullName, value));
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }

    private static string ComputeManifestCatalogHash(ManifestDocument manifest)
    {
        var builder = new StringBuilder()
            .Append(manifest.SchemaVersion).Append('|')
            .Append(manifest.CatalogRevision).Append('|')
            .Append(manifest.PresentationOnly ? '1' : '0').AppendLine();
        foreach (var entry in manifest.Entries.OrderBy(
                     item => item.CompositionKey, StringComparer.Ordinal))
        {
            builder.Append(entry.CompositionKey).Append('|')
                .Append(entry.SourceCompositionKey).Append('|')
                .Append(entry.TopologyCode).Append('|')
                .Append(entry.AssemblyScaleCode).Append('|')
                .Append(entry.FootprintX.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append('|')
                .Append(entry.FootprintY.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append('|')
                .Append(entry.MinimumSlopeDegrees.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append('|')
                .Append(entry.MaximumSlopeDegrees.ToString("R", System.Globalization.CultureInfo.InvariantCulture)).Append('|')
                .Append(string.Join(",", entry.EdgeProfiles
                    .OrderBy(item => item.DirectionCode, StringComparer.Ordinal)
                    .Select(item => item.DirectionCode + ":" + item.ProfileCode + ":"
                        + (item.Required ? "1" : "0"))))
                .Append('|').Append(entry.MaxConsecutive).Append('|')
                .Append(entry.RecentWindowSize).Append('|')
                .Append(string.Join(",", entry.RotationCodes)).Append('|')
                .Append(entry.SeedVersion).Append('|')
                .Append(entry.DetailGeneratorRevision).Append('|')
                .Append(entry.TriangleCount).Append('|')
                .Append(entry.MaterialSlotCount).AppendLine();
        }
        using var sha = SHA256.Create();
        return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(builder.ToString()))
            .Select(value => value.ToString("x2")));
    }

    private sealed class ManifestDocument
    {
        public int SchemaVersion { get; set; }
        public string CatalogRevision { get; set; } = string.Empty;
        public string CatalogHashSha256 { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; }
        public ManifestEntry[] Entries { get; set; } = Array.Empty<ManifestEntry>();
    }

    private sealed class ManifestEntry
    {
        public string CompositionKey { get; set; } = string.Empty;
        public string SourceCompositionKey { get; set; } = string.Empty;
        public string SetName { get; set; } = string.Empty;
        public string VariantCode { get; set; } = string.Empty;
        public string FamilyCode { get; set; } = string.Empty;
        public string TopologyCode { get; set; } = string.Empty;
        public string AssemblyScaleCode { get; set; } = string.Empty;
        public float FootprintX { get; set; }
        public float FootprintY { get; set; }
        public float MinimumSlopeDegrees { get; set; }
        public float MaximumSlopeDegrees { get; set; }
        public ManifestEdge[] EdgeProfiles { get; set; } = Array.Empty<ManifestEdge>();
        public int MaxConsecutive { get; set; }
        public int RecentWindowSize { get; set; }
        public bool MirrorAllowed { get; set; }
        public string[] RotationCodes { get; set; } = Array.Empty<string>();
        public ManifestConnector[] Connectors { get; set; } = Array.Empty<ManifestConnector>();
        public string SeedVersion { get; set; } = string.Empty;
        public string DetailGeneratorRevision { get; set; } = string.Empty;
        public int TriangleCount { get; set; }
        public int MaterialSlotCount { get; set; }
        public bool PresentationOnly { get; set; }
    }

    private sealed class ManifestEdge
    {
        public string DirectionCode { get; set; } = string.Empty;
        public string ProfileCode { get; set; } = string.Empty;
        public bool Required { get; set; }
    }

    private sealed class ManifestConnector
    {
        public string ConnectorTypeCode { get; set; } = string.Empty;
        public string DirectionCode { get; set; } = string.Empty;
        public string RouteSignature { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public double Width { get; set; }
    }
}

internal abstract class SimulationWorldLandscapeEntityConfiguration
{
    protected static void Id<T>(PropertyBuilder<T> property, string name) =>
        property.HasColumnName(name);
    protected static void Text(PropertyBuilder<string> property, string name, int length) =>
        property.HasColumnName(name).HasMaxLength(length);
}

internal sealed class SimulationWorld경관조립실행Configuration
    : SimulationWorldLandscapeEntityConfiguration,
      IEntityTypeConfiguration<SimulationWorld경관조립실행Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld경관조립실행Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_경관조립실행");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => item.GraphBuildStableId).IsUnique();
        builder.HasIndex(item => new { item.TileKey, item.StoredAtUtc });
        Id(builder.Property(item => item.Id), "식별번호");
        Text(builder.Property(item => item.GraphBuildStableId), "경관Graph생성고유식별자", 240);
        Text(builder.Property(item => item.LandscapeGraphStableId), "경관Graph고유식별자", 240);
        Text(builder.Property(item => item.BuildScopeCode), "생성범위코드", 32);
        Text(builder.Property(item => item.GraphRoleCode), "경관Graph역할코드", 80);
        Id(builder.Property(item => item.GraphRevision), "경관Graph개정번호");
        Text(builder.Property(item => item.DefinitionHashSha256), "경관Graph정의SHA256", 64);
        Text(builder.Property(item => item.TileKey), "타일키", 120);
        Text(builder.Property(item => item.AreaSetStableId), "영역묶음고유식별자", 200);
        Text(builder.Property(item => item.GrammarRevision), "경관문법개정번호", 160);
        Text(builder.Property(item => item.GrammarHashSha256), "경관문법SHA256", 64);
        Text(builder.Property(item => item.GraphHashSha256), "경관GraphSHA256", 64);
        Text(builder.Property(item => item.StatusCode), "생성상태코드", 64);
        Id(builder.Property(item => item.PresentationOnly), "표현전용여부");
        Id(builder.Property(item => item.StoredAtUtc), "저장시각UTC");
    }
}

internal sealed class SimulationWorld경관공간NodeConfiguration
    : SimulationWorldLandscapeEntityConfiguration,
      IEntityTypeConfiguration<SimulationWorld경관공간NodeEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld경관공간NodeEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_경관공간Node");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.NodeStableId }).IsUnique();
        Id(builder.Property(item => item.Id), "식별번호");
        Id(builder.Property(item => item.RunId), "경관조립실행식별번호");
        Text(builder.Property(item => item.NodeStableId), "공간Node고유식별자", 220);
        Text(builder.Property(item => item.ParentNodeStableId), "상위공간Node고유식별자", 220);
        Text(builder.Property(item => item.NodeKindCode), "공간위상코드", 40);
        Text(builder.Property(item => item.SemanticCode), "공간의미코드", 120);
        Text(builder.Property(item => item.EvidenceKindCode), "근거종류코드", 40);
        Id(builder.Property(item => item.CenterEastingMeters), "중심동쪽좌표미터");
        Id(builder.Property(item => item.CenterNorthingMeters), "중심북쪽좌표미터");
        Id(builder.Property(item => item.WidthMeters), "너비미터");
        Id(builder.Property(item => item.DepthMeters), "깊이미터");
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorld경관공간EdgeConfiguration
    : SimulationWorldLandscapeEntityConfiguration,
      IEntityTypeConfiguration<SimulationWorld경관공간EdgeEntity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld경관공간EdgeEntity> builder)
    {
        builder.ToTable("시뮬레이션월드_경관공간Edge");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.EdgeStableId }).IsUnique();
        Id(builder.Property(item => item.Id), "식별번호");
        Id(builder.Property(item => item.RunId), "경관조립실행식별번호");
        Text(builder.Property(item => item.EdgeStableId), "공간Edge고유식별자", 220);
        Text(builder.Property(item => item.FromNodeStableId), "출발공간Node고유식별자", 220);
        Text(builder.Property(item => item.RelationCode), "관계코드", 60);
        Text(builder.Property(item => item.ToNodeStableId), "도착공간Node고유식별자", 220);
        Text(builder.Property(item => item.ConnectorTypeCode), "연결자종류코드", 100);
        Text(builder.Property(item => item.EvidenceKindCode), "근거종류코드", 40);
        Id(builder.Property(item => item.IsExternalStub), "외부연결Stub여부");
        Text(builder.Property(item => item.NeighborTileKey), "인접타일키", 120);
        Text(builder.Property(item => item.PlacementStableId), "모판배치고유식별자", 220);
        Text(builder.Property(item => item.RouteSignature), "경로서명", 160);
        Text(builder.Property(item => item.DirectionCode), "방향코드", 20);
        Id(builder.Property(item => item.WorldEastingMeters), "세계동쪽좌표미터");
        Id(builder.Property(item => item.WorldNorthingMeters), "세계북쪽좌표미터");
        Id(builder.Property(item => item.WidthMeters), "연결너비미터");
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorld경관모판배치Configuration
    : SimulationWorldLandscapeEntityConfiguration,
      IEntityTypeConfiguration<SimulationWorld경관모판배치Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld경관모판배치Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_경관모판배치");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.PlacementStableId }).IsUnique();
        Id(builder.Property(item => item.Id), "식별번호");
        Id(builder.Property(item => item.RunId), "경관조립실행식별번호");
        Text(builder.Property(item => item.PlacementStableId), "모판배치고유식별자", 220);
        Text(builder.Property(item => item.NodeStableId), "공간Node고유식별자", 220);
        Text(builder.Property(item => item.OwnerTileKey), "소유타일키", 120);
        Text(builder.Property(item => item.CompositionKey), "CompositionKey", 240);
        Text(builder.Property(item => item.TopologyCode), "공간위상코드", 40);
        Text(builder.Property(item => item.EvidenceKindCode), "근거종류코드", 40);
        Id(builder.Property(item => item.EastingMeters), "동쪽좌표미터");
        Id(builder.Property(item => item.NorthingMeters), "북쪽좌표미터");
        Id(builder.Property(item => item.PhysicalElevationMeters), "물리표고미터");
        Id(builder.Property(item => item.RotationDegrees), "회전각도");
        Id(builder.Property(item => item.Mirrored), "대칭여부");
        Id(builder.Property(item => item.DeterministicSeed), "결정시드");
        Id(builder.Property(item => item.FootprintWidthMeters), "점유너비미터");
        Id(builder.Property(item => item.FootprintDepthMeters), "점유깊이미터");
        Id(builder.Property(item => item.PresentationOnly), "표현전용여부");
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class SimulationWorld경관조립미해결Configuration
    : SimulationWorldLandscapeEntityConfiguration,
      IEntityTypeConfiguration<SimulationWorld경관조립미해결Entity>
{
    public void Configure(EntityTypeBuilder<SimulationWorld경관조립미해결Entity> builder)
    {
        builder.ToTable("시뮬레이션월드_경관조립미해결");
        builder.HasKey(item => item.Id);
        builder.HasIndex(item => new { item.RunId, item.UnresolvedStableId }).IsUnique();
        Id(builder.Property(item => item.Id), "식별번호");
        Id(builder.Property(item => item.RunId), "경관조립실행식별번호");
        Text(builder.Property(item => item.UnresolvedStableId), "미해결고유식별자", 240);
        Text(builder.Property(item => item.NodeStableId), "공간Node고유식별자", 220);
        Text(builder.Property(item => item.ReasonCode), "미해결사유코드", 100);
        Text(builder.Property(item => item.RequiredSemanticCode), "필요공간의미코드", 160);
        Text(builder.Property(item => item.EvidenceKindCode), "근거종류코드", 40);
        builder.Property(item => item.Detail).HasColumnName("상세설명").HasMaxLength(1000);
        builder.HasOne(item => item.Run).WithMany().HasForeignKey(item => item.RunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
