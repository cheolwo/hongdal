using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    public sealed class SimulationWorldActualE5SpatialCatalog
    {
        public SimulationWorldAreaSetNetworkResponse Network { get; init; } = new();
        public IReadOnlyDictionary<string, SimulationWorldAreaSetDefinitionResponse> AreaSets { get; init; } =
            new Dictionary<string, SimulationWorldAreaSetDefinitionResponse>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, SimulationWorldLandscapeGraphResponse> Graphs { get; init; } =
            new Dictionary<string, SimulationWorldLandscapeGraphResponse>(StringComparer.Ordinal);
        public SimulationWorld상호작용NetworkBindingCatalog InteractionSpatialCatalog { get; init; } =
            new();
    }

    public interface ISimulationWorldActualE5SpatialCatalogReader
    {
        bool TryRead(out SimulationWorldActualE5SpatialCatalog catalog, out string errorCode);
    }

    public sealed class DisabledSimulationWorldActualE5SpatialCatalogReader :
        ISimulationWorldActualE5SpatialCatalogReader
    {
        public bool TryRead(out SimulationWorldActualE5SpatialCatalog catalog, out string errorCode)
        {
            catalog = new SimulationWorldActualE5SpatialCatalog();
            errorCode = "ActualE5SpatialCatalogUnavailable";
            return false;
        }
    }

    public sealed class FileSimulationWorldActualE5SpatialCatalogReader :
        ISimulationWorldActualE5SpatialCatalogReader
    {
        private readonly string path;

        public FileSimulationWorldActualE5SpatialCatalogReader(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("실제 E5 공간 대장 경로가 필요합니다.", nameof(path));
            this.path = ResolvePath(path);
        }

        public bool TryRead(out SimulationWorldActualE5SpatialCatalog catalog, out string errorCode)
        {
            catalog = new SimulationWorldActualE5SpatialCatalog();
            if (!File.Exists(path))
            {
                errorCode = "ActualE5SpatialCatalogUnavailable";
                return false;
            }

            try
            {
                var root = JsonSerializer.Deserialize<ActualE5Document>(
                    File.ReadAllBytes(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("ActualE5SpatialCatalogInvalid");
                Validate(root);
                var areaSets = root.AreaSets.ToDictionary(
                    item => item.Definition.AreaSetStableId,
                    item => item.Definition,
                    StringComparer.Ordinal);
                var graphs = root.AreaSets.SelectMany(item => item.Graphs)
                    .Concat(root.RouteGraphs)
                    .ToDictionary(item => item.LandscapeGraphStableId, item => item,
                        StringComparer.Ordinal);
                catalog = new SimulationWorldActualE5SpatialCatalog
                {
                    Network = root.Network,
                    AreaSets = areaSets,
                    Graphs = graphs,
                    InteractionSpatialCatalog = root.InteractionSpatialCatalog,
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

        private static void Validate(ActualE5Document root)
        {
            Require(root.SchemaVersion == "simulation-world-actual-e5-spatial-output.v1",
                "ActualE5SpatialSchemaMismatch");
            Require(root.Network.NetworkStableId == PyeongchangAreaSetStableIds.ActualNetwork,
                "ActualE5NetworkStableIdMismatch");
            Require(root.AreaSets.Length == 4, "ActualE5AreaSetCountInvalid");
            Require(root.RouteGraphs.Length == 3, "ActualE5RouteGraphCountInvalid");
            Require(root.Network.Relations.Length == 8, "ActualE5NetworkRelationCountInvalid");
            Require(root.Network.PresentationOnly && !root.Network.IsOperationalState,
                "ActualE5NetworkAuthorityInvalid");
            var graphs = root.AreaSets.SelectMany(item => item.Graphs)
                .Concat(root.RouteGraphs).ToArray();
            Require(root.Counts.AreaSets == root.AreaSets.Length
                    && root.Counts.InternalGraphs == root.AreaSets.Sum(item => item.Graphs.Length)
                    && root.Counts.NetworkRouteGraphs == root.RouteGraphs.Length
                    && root.Counts.TotalGraphs == graphs.Length
                    && graphs.Length >= 13,
                "ActualE5GraphCountInvalid");
            Require(graphs.Select(item => item.LandscapeGraphStableId)
                    .Distinct(StringComparer.Ordinal).Count() == graphs.Length,
                "ActualE5GraphDuplicate");
            Require(graphs.All(item => item.StatusCode ==
                                      SimulationWorldLandscapeCompositionCodes.Available
                                      && item.Unresolved.Length == 0
                                      && item.PresentationOnly
                                      && !item.IsOperationalState),
                "ActualE5GraphUnavailable");
            Require(root.AreaSets.All(item =>
                    item.Definition.CanonicalNetworkStableId == root.Network.NetworkStableId
                    && item.Definition.CoordinateSpaceCode ==
                    SimulationWorldLandscapeCompositionCodes.ScenarioLocalMeters),
                "ActualE5AreaSetNetworkBindingInvalid");
            var interaction = root.InteractionSpatialCatalog;
            Require(interaction.SchemaVersion == "simulation-world-interaction-graph-binding.v2"
                    && interaction.NetworkStableId == root.Network.NetworkStableId,
                "ActualE5InteractionCatalogInvalid");
            Require(interaction.Bindings.Length == 30
                    && interaction.ContextualBindings.Length == 5
                    && interaction.NonSpatialWiIds.Length == 6,
                "ActualE5InteractionPartitionInvalid");
            Require(interaction.Bindings.Select(item => item.WorldInteractionId)
                    .Concat(interaction.ContextualBindings.Select(item => item.WorldInteractionId))
                    .Concat(interaction.NonSpatialWiIds)
                    .Distinct(StringComparer.Ordinal).Count() == 41,
                "ActualE5InteractionCoverageInvalid");
        }

        private static void Require(bool condition, string errorCode)
        {
            if (!condition) throw new InvalidOperationException(errorCode);
        }

        private static string ResolvePath(string value)
        {
            if (Path.IsPathRooted(value)) return Path.GetFullPath(value);
            var direct = Path.GetFullPath(value);
            if (File.Exists(direct)) return direct;
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                var candidate = Path.GetFullPath(Path.Combine(current.FullName, value));
                if (File.Exists(candidate)) return candidate;
                current = current.Parent;
            }
            return direct;
        }

        private sealed class ActualE5Document
        {
            public string SchemaVersion { get; set; } = string.Empty;
            public ActualE5Counts Counts { get; set; } = new();
            public SimulationWorldAreaSetNetworkResponse Network { get; set; } = new();
            public ActualE5AreaSetDocument[] AreaSets { get; set; } =
                Array.Empty<ActualE5AreaSetDocument>();
            public SimulationWorldLandscapeGraphResponse[] RouteGraphs { get; set; } =
                Array.Empty<SimulationWorldLandscapeGraphResponse>();
            public SimulationWorld상호작용NetworkBindingCatalog InteractionSpatialCatalog
                { get; set; } = new();
        }

        private sealed class ActualE5Counts
        {
            public int AreaSets { get; set; }
            public int InternalGraphs { get; set; }
            public int NetworkRouteGraphs { get; set; }
            public int TotalGraphs { get; set; }
        }

        private sealed class ActualE5AreaSetDocument
        {
            public SimulationWorldAreaSetDefinitionResponse Definition { get; set; } = new();
            public SimulationWorldLandscapeGraphResponse[] Graphs { get; set; } =
                Array.Empty<SimulationWorldLandscapeGraphResponse>();
        }
    }

    public sealed class SimulationWorldActualE5SpatialService
    {
        private readonly ISimulationWorldActualE5SpatialCatalogReader reader;

        public SimulationWorldActualE5SpatialService(
            ISimulationWorldActualE5SpatialCatalogReader reader) =>
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));

        public Task<SimulationWorldAreaSetNetworkResponse?> ReadNetworkAsync(
            string networkStableId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!reader.TryRead(out var catalog, out _)
                || !string.Equals(catalog.Network.NetworkStableId, networkStableId,
                    StringComparison.Ordinal))
                return Task.FromResult<SimulationWorldAreaSetNetworkResponse?>(null);
            return Task.FromResult<SimulationWorldAreaSetNetworkResponse?>(catalog.Network);
        }

        public Task<SimulationWorldAreaSetDefinitionResponse?> ReadAreaSetAsync(
            string areaSetStableId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!reader.TryRead(out var catalog, out _)
                || !catalog.AreaSets.TryGetValue(areaSetStableId, out var value))
                return Task.FromResult<SimulationWorldAreaSetDefinitionResponse?>(null);
            return Task.FromResult<SimulationWorldAreaSetDefinitionResponse?>(value);
        }

        public Task<SimulationWorldLandscapeGraphResponse?> ReadGraphAsync(
            string graphStableId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!reader.TryRead(out var catalog, out _)
                || !catalog.Graphs.TryGetValue(graphStableId, out var value))
                return Task.FromResult<SimulationWorldLandscapeGraphResponse?>(null);
            return Task.FromResult<SimulationWorldLandscapeGraphResponse?>(value);
        }

        public Task<SimulationWorldLandscapeGraphIndexResponse?> ReadGraphIndexAsync(
            string areaSetStableId,
            string? tileKey,
            int radiusTiles,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (radiusTiles is < 0 or > 12
                || !reader.TryRead(out var catalog, out _)
                || !catalog.AreaSets.TryGetValue(areaSetStableId, out var areaSet))
                return Task.FromResult<SimulationWorldLandscapeGraphIndexResponse?>(null);

            var coveredTileKeys = areaSet.LandscapeGraphs
                .SelectMany(item => item.TileRefs)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToArray();
            var normalizedTileKey = string.IsNullOrWhiteSpace(tileKey)
                ? coveredTileKeys.FirstOrDefault() ?? string.Empty
                : tileKey;
            if (normalizedTileKey.Length > 0
                && !normalizedTileKey.StartsWith("scenario-local:", StringComparison.Ordinal))
                return Task.FromResult<SimulationWorldLandscapeGraphIndexResponse?>(null);

            return Task.FromResult<SimulationWorldLandscapeGraphIndexResponse?>(new()
            {
                AreaSetStableId = areaSet.AreaSetStableId,
                CenterTileKey = normalizedTileKey,
                RadiusTiles = radiusTiles,
                CoveredTileKeys = coveredTileKeys,
                Graphs = areaSet.LandscapeGraphs
                    .OrderBy(item => item.LandscapeGraphStableId, StringComparer.Ordinal)
                    .ToArray(),
                PresentationOnly = true,
            });
        }
    }
}
