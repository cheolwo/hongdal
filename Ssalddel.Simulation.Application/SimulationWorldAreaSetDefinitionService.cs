using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{

public static class PyeongchangAreaSetStableIds
{
    public const string AreaSet = "area-set:sim:pyeongchang:farm-hub-town.v1";
    public const string LegacyAreaSetAlias = "pyeongchang-farm-hub-town-v1";
    public const string FarmGraph = "landscape-graph:sim:pyeongchang:daegwallyeong-farm.v1";
    public const string FarmHubCorridorGraph = "landscape-graph:sim:pyeongchang:farm-hub-corridor.v1";
    public const string HubGraph = "landscape-graph:sim:pyeongchang:jinbu-hub.v1";
    public const string HubTownCorridorGraph = "landscape-graph:sim:pyeongchang:hub-town-corridor.v1";
    public const string TownGraph = "landscape-graph:sim:pyeongchang:pyeongchang-town.v1";

    public static string NormalizeAreaSet(string stableId) =>
        string.Equals(stableId, LegacyAreaSetAlias, StringComparison.Ordinal)
            ? AreaSet
            : stableId;
}

public sealed class SimulationWorldAreaSetDefinitionCatalog
{
    public SimulationWorldAreaSetDefinitionResponse AreaSet { get; init; } = new();
    public IReadOnlyDictionary<string, SimulationWorldLandscapeGraphDescriptorResponse> Graphs { get; init; } =
        new Dictionary<string, SimulationWorldLandscapeGraphDescriptorResponse>(StringComparer.Ordinal);
}

public interface ISimulationWorldAreaSetDefinitionReader
{
    bool TryRead(out SimulationWorldAreaSetDefinitionCatalog catalog, out string errorCode);
}

public sealed class DisabledSimulationWorldAreaSetDefinitionReader : ISimulationWorldAreaSetDefinitionReader
{
    public bool TryRead(out SimulationWorldAreaSetDefinitionCatalog catalog, out string errorCode)
    {
        catalog = new SimulationWorldAreaSetDefinitionCatalog();
        errorCode = "AreaSetDefinitionUnavailable";
        return false;
    }
}

public sealed class FileSimulationWorldAreaSetDefinitionReader
    : ISimulationWorldAreaSetDefinitionReader
{
    private readonly string _areaSetDefinitionPath;

    public FileSimulationWorldAreaSetDefinitionReader(string areaSetDefinitionPath) =>
        _areaSetDefinitionPath = areaSetDefinitionPath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public bool TryRead(out SimulationWorldAreaSetDefinitionCatalog catalog, out string errorCode)
    {
        catalog = new SimulationWorldAreaSetDefinitionCatalog();
        var rootPath = ResolvePath(_areaSetDefinitionPath);
        if (rootPath == null)
        {
            errorCode = "AreaSetDefinitionUnavailable";
            return false;
        }

        try
        {
            var rootDirectory = Path.GetDirectoryName(rootPath)!;
            var root = Deserialize<AreaSetDocument>(rootPath);
            Require(root.SchemaVersion == SimulationWorldLandscapeCompositionCodes.AreaSetSchemaVersion,
                "AreaSetSchemaVersionMismatch");
            root.AreaSetStableId = PyeongchangAreaSetStableIds.NormalizeAreaSet(root.AreaSetStableId);
            Require(root.AreaSetStableId.StartsWith("area-set:sim:", StringComparison.Ordinal),
                "AreaSetStableIdInvalid");
            Require(root.Revision > 0 && !string.IsNullOrWhiteSpace(root.Title),
                "AreaSetDefinitionInvalid");
            Require(root.PresentationOnly, "AreaSetMustBePresentationOnly");
            RequireDistinct(root.AreaRefs, "AreaSetAreaRefDuplicate");
            RequireDistinct(root.LandscapeGraphRefs, "AreaSetGraphRefDuplicate");
            RequireDistinct(root.GraphRelations.Select(item => item.RelationStableId),
                "AreaSetGraphRelationDuplicate");

            var areas = Directory.GetFiles(Path.Combine(rootDirectory, "areas"), "*.json")
                .Select(Deserialize<AreaDocument>).ToArray();
            RequireSet(root.AreaRefs, areas.Select(item => item.AreaStableId), "AreaSetAreaRefMismatch");

            var graphDocuments = Directory.GetFiles(
                    Path.Combine(rootDirectory, "landscape-graphs"), "*.json")
                .Select(path => (Path: path, Document: Deserialize<GraphDocument>(path)))
                .ToArray();
            RequireSet(root.LandscapeGraphRefs,
                graphDocuments.Select(item => item.Document.LandscapeGraphStableId),
                "AreaSetGraphRefMismatch");

            var areaSet = new SimulationWorldAreaSetDefinitionResponse
            {
                AreaSetStableId = root.AreaSetStableId,
                Revision = root.Revision,
                Title = root.Title,
                Summary = root.Summary,
                AreaRefs = root.AreaRefs.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                ScenarioRouteRefs = root.ScenarioRouteRefs.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                CompletionAreaRefs = root.CompletionAreaRefs.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                DefinitionStatusCode = SimulationWorldLandscapeCompositionCodes.Available,
                PresentationOnly = true,
                IsOperationalState = false,
            };

            var descriptors = graphDocuments.Select(item => ToDescriptor(item.Document, root)).ToArray();
            var descriptorById = descriptors.ToDictionary(
                item => item.LandscapeGraphStableId, item => item, StringComparer.Ordinal);
            foreach (var descriptor in descriptors)
            {
                Require(descriptor.PresentationSafe(), "LandscapeGraphDefinitionInvalid");
                Require(descriptor.AreaRefs.All(root.AreaRefs.Contains), "LandscapeGraphAreaRefInvalid");
                Require(descriptor.ScenarioRouteRefs.All(root.ScenarioRouteRefs.Contains),
                    "LandscapeGraphRouteRefInvalid");
            }

            var relations = root.GraphRelations.Select(item => ToRelation(item, descriptorById)).ToArray();
            ValidateAuthoredDocuments(rootDirectory, root, areas);
            var definitionFiles = new[] { rootPath }
                .Concat(Directory.GetFiles(Path.Combine(rootDirectory, "areas"), "*.json"))
                .Concat(graphDocuments.Select(item => item.Path));
            areaSet.DefinitionHashSha256 = HashCanonicalJsonFiles(definitionFiles);
            areaSet.DocumentHashSha256 = HashTextFiles(
                Directory.GetFiles(Path.Combine(rootDirectory, "authored"), "*.md", SearchOption.AllDirectories));
            foreach (var descriptor in descriptors)
                descriptor.DefinitionHashSha256 = HashGraphDefinition(
                    graphDocuments.Single(item =>
                        item.Document.LandscapeGraphStableId == descriptor.LandscapeGraphStableId).Path);
            // AreaSet의 배열 순서는 단순 정렬이 아니라 사람이 설계한 이동·서술 순서다.
            areaSet.LandscapeGraphs = root.LandscapeGraphRefs.Select(
                graphId => descriptorById[graphId]).ToArray();
            areaSet.GraphRelations = relations;
            catalog = new SimulationWorldAreaSetDefinitionCatalog
            {
                AreaSet = areaSet,
                Graphs = descriptorById,
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

    private static SimulationWorldLandscapeGraphDescriptorResponse ToDescriptor(
        GraphDocument graph, AreaSetDocument root)
    {
        Require(root.LandscapeGraphRefs.Contains(graph.LandscapeGraphStableId),
            "LandscapeGraphNotDeclared");
        Require(graph.GraphRevision > 0 && graph.PresentationOnly,
            "LandscapeGraphDefinitionInvalid");
        RequireDistinct(graph.AreaRefs, "LandscapeGraphAreaRefDuplicate");
        RequireDistinct(graph.TileRefs, "LandscapeGraphTileRefDuplicate");
        return new SimulationWorldLandscapeGraphDescriptorResponse
        {
            LandscapeGraphStableId = graph.LandscapeGraphStableId,
            GraphRoleCode = graph.GraphRoleCode,
            GraphRevision = graph.GraphRevision,
            BuildStatusCode = SimulationWorldLandscapeCompositionCodes.Declared,
            Bounds = graph.Bounds == null ? new SimulationWorldLandscapeBoundsResponse() : new()
            {
                MinEastingMeters = graph.Bounds.MinEastingMeters,
                MinNorthingMeters = graph.Bounds.MinNorthingMeters,
                MaxEastingMeters = graph.Bounds.MaxEastingMeters,
                MaxNorthingMeters = graph.Bounds.MaxNorthingMeters,
            },
            AreaRefs = graph.AreaRefs.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            TileRefs = graph.TileRefs.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            ScenarioRouteRefs = graph.ScenarioRouteRefs.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
        };
    }

    private static SimulationWorldLandscapeGraphRelationResponse ToRelation(
        GraphRelationDocument relation,
        IReadOnlyDictionary<string, SimulationWorldLandscapeGraphDescriptorResponse> graphs)
    {
        Require(graphs.ContainsKey(relation.FromGraphStableId)
                && graphs.ContainsKey(relation.ToGraphStableId)
                && relation.FromGraphStableId != relation.ToGraphStableId,
            "LandscapeGraphRelationInvalid");
        Require(relation.RelationCode is
            SimulationWorldLandscapeCompositionCodes.GraphAdjacent or
            SimulationWorldLandscapeCompositionCodes.GraphConnected or
            SimulationWorldLandscapeCompositionCodes.GraphTransition,
            "LandscapeGraphRelationCodeInvalid");
        Require(!string.IsNullOrWhiteSpace(relation.FromConnectorStableId)
                && !string.IsNullOrWhiteSpace(relation.ToConnectorStableId)
                && !string.IsNullOrWhiteSpace(relation.ConnectorTypeCode)
                && !string.IsNullOrWhiteSpace(relation.RouteSignature),
            "LandscapeGraphConnectorPairInvalid");
        return new SimulationWorldLandscapeGraphRelationResponse
        {
            RelationStableId = relation.RelationStableId,
            FromGraphStableId = relation.FromGraphStableId,
            ToGraphStableId = relation.ToGraphStableId,
            RelationCode = relation.RelationCode,
            ConnectorPair = new SimulationWorldLandscapeConnectorPairResponse
            {
                FromConnectorStableId = relation.FromConnectorStableId,
                ToConnectorStableId = relation.ToConnectorStableId,
                ConnectorTypeCode = relation.ConnectorTypeCode,
                RouteSignature = relation.RouteSignature,
            },
        };
    }

    private static void ValidateAuthoredDocuments(
        string rootDirectory, AreaSetDocument root, IEnumerable<AreaDocument> areas)
    {
        var areaSetMarkdown = Path.GetFullPath(Path.Combine(rootDirectory, root.AuthoredDocument));
        var directives = ReadDirectives(areaSetMarkdown);
        RequireSet(new[] { root.AreaSetStableId }, directives.AreaSets, "AreaSetMarkdownRefMismatch");
        RequireSet(root.AreaRefs, directives.Areas, "AreaMarkdownRefMismatch");
        RequireSet(root.LandscapeGraphRefs, directives.Graphs, "LandscapeGraphMarkdownRefMismatch");
        foreach (var area in areas)
        {
            var markdownPath = Path.GetFullPath(Path.Combine(rootDirectory, "areas", area.AuthoredDocument));
            var areaDirectives = ReadDirectives(markdownPath);
            RequireSet(new[] { area.AreaStableId }, areaDirectives.Areas, "AreaDocumentRefMismatch");
        }
    }

    private static (string[] AreaSets, string[] Areas, string[] Graphs) ReadDirectives(string path)
    {
        Require(File.Exists(path), "AuthoredDocumentMissing");
        var areaSets = new List<string>();
        var areas = new List<string>();
        var graphs = new List<string>();
        foreach (var line in File.ReadLines(path, Encoding.UTF8).Select(value => value.Trim()))
        {
            if (line.StartsWith("@areaset ", StringComparison.Ordinal)) areaSets.Add(line[9..].Trim());
            if (line.StartsWith("@area ", StringComparison.Ordinal)) areas.Add(line[6..].Trim());
            if (line.StartsWith("@landscape-graph ", StringComparison.Ordinal)) graphs.Add(line[17..].Trim());
        }
        return (areaSets.ToArray(), areas.ToArray(), graphs.ToArray());
    }

    private static T Deserialize<T>(string path)
    {
        var json = File.ReadAllText(path, Encoding.UTF8);
        Require(!json.Contains("Assets/", StringComparison.OrdinalIgnoreCase)
                && !json.Contains(".prefab", StringComparison.OrdinalIgnoreCase)
                && !json.Contains("\"guid\"", StringComparison.OrdinalIgnoreCase),
            "PaidAssetReferenceForbidden");
        return JsonSerializer.Deserialize<T>(json, JsonOptions)
               ?? throw new InvalidOperationException("AreaSetJsonInvalid");
    }

    private static string HashCanonicalJsonFiles(IEnumerable<string> paths)
    {
        using var sha = SHA256.Create();
        var canonical = new StringBuilder();
        foreach (var path in paths.OrderBy(Path.GetFileName, StringComparer.Ordinal))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            canonical.Append(Path.GetFileName(path)).Append(':');
            WriteCanonical(document.RootElement, canonical);
            canonical.AppendLine();
        }
        return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString())))
            .Replace("-", string.Empty).ToLowerInvariant();
    }

    private static string HashGraphDefinition(string path) => HashCanonicalJsonFiles(new[] { path });

    private static string HashTextFiles(IEnumerable<string> paths)
    {
        using var sha = SHA256.Create();
        var text = string.Join("\n", paths.OrderBy(value => value, StringComparer.Ordinal)
            .Select(path => Path.GetRelativePath(Path.GetDirectoryName(Path.GetDirectoryName(path))!, path)
                            + ":" + File.ReadAllText(path, Encoding.UTF8).Replace("\r\n", "\n")));
        return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)))
            .Replace("-", string.Empty).ToLowerInvariant();
    }

    private static void WriteCanonical(JsonElement value, StringBuilder output)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                output.Append('{');
                var firstProperty = true;
                foreach (var property in value.EnumerateObject().OrderBy(item => item.Name, StringComparer.Ordinal))
                {
                    if (!firstProperty) output.Append(',');
                    firstProperty = false;
                    output.Append(JsonSerializer.Serialize(property.Name)).Append(':');
                    WriteCanonical(property.Value, output);
                }
                output.Append('}');
                break;
            case JsonValueKind.Array:
                output.Append('[');
                var firstItem = true;
                foreach (var item in value.EnumerateArray())
                {
                    if (!firstItem) output.Append(',');
                    firstItem = false;
                    WriteCanonical(item, output);
                }
                output.Append(']');
                break;
            default:
                output.Append(value.GetRawText());
                break;
        }
    }

    private static void RequireSet(IEnumerable<string> expected, IEnumerable<string> actual, string error)
    {
        var left = expected.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        var right = actual.OrderBy(value => value, StringComparer.Ordinal).ToArray();
        Require(left.SequenceEqual(right, StringComparer.Ordinal), error);
    }

    private static void RequireDistinct(IEnumerable<string> values, string error)
    {
        var array = values.ToArray();
        Require(array.Length == array.Distinct(StringComparer.Ordinal).Count(), error);
    }

    private static void Require(bool condition, string error)
    {
        if (!condition) throw new InvalidOperationException(error);
    }

    private static string? ResolvePath(string path)
    {
        if (Path.IsPathRooted(path)) return File.Exists(path) ? path : null;
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            var candidate = Path.GetFullPath(Path.Combine(current.FullName, path));
            if (File.Exists(candidate)) return candidate;
            current = current.Parent;
        }
        return File.Exists(path) ? Path.GetFullPath(path) : null;
    }

    private sealed class AreaSetDocument
    {
        public string SchemaVersion { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public int Revision { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string AuthoredDocument { get; set; } = string.Empty;
        public string[] AreaRefs { get; set; } = Array.Empty<string>();
        public string[] ScenarioRouteRefs { get; set; } = Array.Empty<string>();
        public string[] CompletionAreaRefs { get; set; } = Array.Empty<string>();
        public string[] LandscapeGraphRefs { get; set; } = Array.Empty<string>();
        public GraphRelationDocument[] GraphRelations { get; set; } = Array.Empty<GraphRelationDocument>();
        public bool PresentationOnly { get; set; }
    }

    private sealed class AreaDocument
    {
        public string AreaStableId { get; set; } = string.Empty;
        public string AuthoredDocument { get; set; } = string.Empty;
    }

    private sealed class GraphDocument
    {
        public string LandscapeGraphStableId { get; set; } = string.Empty;
        public string GraphRoleCode { get; set; } = string.Empty;
        public int GraphRevision { get; set; }
        public BoundsDocument? Bounds { get; set; }
        public string[] AreaRefs { get; set; } = Array.Empty<string>();
        public string[] TileRefs { get; set; } = Array.Empty<string>();
        public string[] ScenarioRouteRefs { get; set; } = Array.Empty<string>();
        public bool PresentationOnly { get; set; }
    }

    private sealed class BoundsDocument
    {
        public double MinEastingMeters { get; set; }
        public double MinNorthingMeters { get; set; }
        public double MaxEastingMeters { get; set; }
        public double MaxNorthingMeters { get; set; }
    }

    private sealed class GraphRelationDocument
    {
        public string RelationStableId { get; set; } = string.Empty;
        public string FromGraphStableId { get; set; } = string.Empty;
        public string ToGraphStableId { get; set; } = string.Empty;
        public string RelationCode { get; set; } = string.Empty;
        public string FromConnectorStableId { get; set; } = string.Empty;
        public string ToConnectorStableId { get; set; } = string.Empty;
        public string ConnectorTypeCode { get; set; } = string.Empty;
        public string RouteSignature { get; set; } = string.Empty;
    }
}

internal static class SimulationWorldLandscapeGraphDescriptorExtensions
{
    public static bool PresentationSafe(this SimulationWorldLandscapeGraphDescriptorResponse value) =>
        value.LandscapeGraphStableId.StartsWith("landscape-graph:sim:", StringComparison.Ordinal)
        && !string.IsNullOrWhiteSpace(value.GraphRoleCode)
        && value.GraphRevision > 0;
}
}
