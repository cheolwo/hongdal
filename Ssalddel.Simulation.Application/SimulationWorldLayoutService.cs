using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{

public sealed class SimulationWorldLayoutCatalog
{
    public SimulationWorldLayoutDefinitionResponse Definition { get; init; } = new();
    public SimulationWorldGroundingBindingResponse GroundingBinding { get; init; } = new();
    public SimulationWorldGroundingReadinessResponse GroundingReadiness { get; init; } = new();
}

public interface ISimulationWorldLayoutCatalogReader
{
    bool TryRead(out SimulationWorldLayoutCatalog catalog, out string errorCode);
}

public sealed class FileSimulationWorldLayoutCatalogReader :
    ISimulationWorldLayoutCatalogReader
{
    private readonly string path;

    public FileSimulationWorldLayoutCatalogReader(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("H5 세계 배치 대장 경로가 필요합니다.", nameof(path));
        this.path = ResolvePath(path);
    }

    public bool TryRead(out SimulationWorldLayoutCatalog catalog, out string errorCode)
    {
        catalog = new SimulationWorldLayoutCatalog();
        if (!File.Exists(path))
        {
            errorCode = "WorldLayoutCatalogUnavailable";
            return false;
        }

        try
        {
            var document = JsonSerializer.Deserialize<WorldLayoutDocument>(
                File.ReadAllBytes(path),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("WorldLayoutCatalogInvalid");
            Validate(document);
            catalog = new SimulationWorldLayoutCatalog
            {
                Definition = document.WorldLayoutDefinition,
                GroundingBinding = document.WorldGroundingBinding,
                GroundingReadiness = document.GroundingReadiness,
            };
            errorCode = string.Empty;
            return true;
        }
        catch (Exception error) when (error is IOException or JsonException or InvalidOperationException)
        {
            errorCode = error.Message;
            return false;
        }
    }

    private static void Validate(WorldLayoutDocument document)
    {
        var definition = document.WorldLayoutDefinition;
        var binding = document.WorldGroundingBinding;
        var readiness = document.GroundingReadiness;
        Require(document.SchemaVersion == "simulation-world-h5-spatial-output.v1", "WorldLayoutOutputSchemaMismatch");
        Require(definition.SchemaVersion == SimulationWorldLayoutCodes.DefinitionSchemaVersion, "WorldLayoutDefinitionSchemaMismatch");
        Require(definition.CoordinateSpaceCode == SimulationWorldLayoutCodes.ScenarioLocalMeters, "WorldLayoutCoordinateSpaceInvalid");
        Require(definition.AreaSetInstances.Length == 4 && definition.CorridorInstances.Length == 3, "WorldLayoutInstanceCountInvalid");
        Require(definition.AreaSetInstances.All(item => item.PlacementTransform.CoordinateSpaceCode == SimulationWorldLayoutCodes.ScenarioLocalMeters), "WorldLayoutAreaCoordinateSpaceInvalid");
        Require(definition.CorridorInstances.All(item => item.PlacementTransform.CoordinateSpaceCode == SimulationWorldLayoutCodes.ScenarioLocalMeters), "WorldLayoutCorridorCoordinateSpaceInvalid");
        Require(definition.AreaSetInstances.SelectMany(item => item.GraphInstances).All(item =>
            item.PlacementTransform.CoordinateSpaceCode == SimulationWorldLayoutCodes.ParentLocalMeters
            && item.ExternalConnectors.All(connector => connector.CoordinateSpaceCode == SimulationWorldLayoutCodes.ParentLocalMeters)),
            "WorldLayoutChildCoordinateSpaceInvalid");
        Require(definition.Relations.Count(item => item.SpatialRealizationCode == SimulationWorldLayoutCodes.PhysicalCorridor) == 3
                && definition.Relations.Where(item => item.SpatialRealizationCode == SimulationWorldLayoutCodes.PhysicalCorridor)
                    .All(item => definition.CorridorInstances.Any(corridor => corridor.CorridorInstanceStableId == item.CorridorInstanceStableId))
                && definition.Relations.Where(item => item.SpatialRealizationCode == SimulationWorldLayoutCodes.AbstractTravel)
                    .All(item => string.IsNullOrEmpty(item.CorridorInstanceStableId)),
            "WorldLayoutRelationRealizationInvalid");
        Require(IsHash(definition.WorldLayoutHashSha256) && definition.PresentationOnly && !definition.IsOperationalState,
            "WorldLayoutAuthorityInvalid");
        Require(binding.SchemaVersion == SimulationWorldLayoutCodes.GroundingBindingSchemaVersion
                && binding.WorldLayoutStableId == definition.WorldLayoutStableId
                && binding.WorldLayoutRevision == definition.WorldLayoutRevision
                && binding.WorldLayoutHashSha256 == definition.WorldLayoutHashSha256,
            "WorldGroundingBindingLayoutMismatch");
        Require(binding.PlacementAuthorityCode == SimulationWorldLayoutCodes.ScenarioRelative
                && binding.WorldGroundingStateCode == SimulationWorldLayoutCodes.NotApplied
                && string.IsNullOrEmpty(binding.E6AnchorStableId)
                && string.IsNullOrEmpty(binding.GroundingEvidenceHashSha256),
            "WorldGroundingBindingAuthorityInvalid");
        Require(readiness.SchemaVersion == SimulationWorldLayoutCodes.GroundingReadinessSchemaVersion
                && readiness.WorldLayoutStableId == definition.WorldLayoutStableId
                && readiness.GroundingReadinessStateCode == SimulationWorldLayoutCodes.Partial
                && !readiness.AppliesAuthority,
            "WorldGroundingReadinessInvalid");
    }

    private static bool IsHash(string value) => value.Length == 64 && value.All(Uri.IsHexDigit);
    private static void Require(bool condition, string errorCode)
    {
        if (!condition) throw new InvalidOperationException(errorCode);
    }

    private static string ResolvePath(string value)
    {
        if (Path.IsPathRooted(value)) return Path.GetFullPath(value);
        var direct = Path.GetFullPath(value);
        if (File.Exists(direct)) return direct;
        for (var current = new DirectoryInfo(AppContext.BaseDirectory); current != null; current = current.Parent)
        {
            var candidate = Path.GetFullPath(Path.Combine(current.FullName, value));
            if (File.Exists(candidate)) return candidate;
        }
        return direct;
    }

    private sealed class WorldLayoutDocument
    {
        public string SchemaVersion { get; set; } = string.Empty;
        public SimulationWorldLayoutDefinitionResponse WorldLayoutDefinition { get; set; } = new();
        public SimulationWorldGroundingBindingResponse WorldGroundingBinding { get; set; } = new();
        public SimulationWorldGroundingReadinessResponse GroundingReadiness { get; set; } = new();
    }
}

public sealed class SimulationWorldLayoutService
{
    private readonly ISimulationWorldLayoutCatalogReader reader;

    public SimulationWorldLayoutService(ISimulationWorldLayoutCatalogReader reader) =>
        this.reader = reader ?? throw new ArgumentNullException(nameof(reader));

    public Task<SimulationWorldLayoutDefinitionResponse?> ReadDefinitionAsync(string stableId, CancellationToken cancellationToken = default)
        => ReadAsync(stableId, catalog => catalog.Definition.WorldLayoutStableId, catalog => catalog.Definition, cancellationToken);

    public Task<SimulationWorldGroundingBindingResponse?> ReadGroundingBindingAsync(string stableId, CancellationToken cancellationToken = default)
        => ReadAsync(stableId, catalog => catalog.GroundingBinding.WorldLayoutStableId, catalog => catalog.GroundingBinding, cancellationToken);

    public Task<SimulationWorldGroundingReadinessResponse?> ReadGroundingReadinessAsync(string stableId, CancellationToken cancellationToken = default)
        => ReadAsync(stableId, catalog => catalog.GroundingReadiness.WorldLayoutStableId, catalog => catalog.GroundingReadiness, cancellationToken);

    private Task<T?> ReadAsync<T>(string stableId, Func<SimulationWorldLayoutCatalog, string> id, Func<SimulationWorldLayoutCatalog, T> select, CancellationToken cancellationToken) where T : class
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(reader.TryRead(out var catalog, out _)
                               && string.Equals(id(catalog), stableId, StringComparison.Ordinal)
            ? select(catalog)
            : null);
    }
}
}
