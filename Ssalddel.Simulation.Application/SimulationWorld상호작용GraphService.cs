using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E6,
        "세계 의미·인과·근거와 플레이 준비도 책임을 제공한다.",
        Boundary = "근거 자료와 Simulation 규칙 및 E 승격을 분리한다.")]
    public interface ISimulationWorld상호작용GraphReadinessStore
    {
        Task ReplaceAsync(
            SimulationWorld상호작용Graph준비도Response readiness,
            CancellationToken cancellationToken = default);

        Task<SimulationWorld상호작용Graph준비도Response?> ReadLatestAsync(
            string areaSetStableId,
            CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E6,
        "세계 의미·인과·근거와 플레이 준비도 책임을 제공한다.",
        Boundary = "근거 자료와 Simulation 규칙 및 E 승격을 분리한다.")]
    public sealed class DisabledSimulationWorld상호작용GraphReadinessStore :
        ISimulationWorld상호작용GraphReadinessStore
    {
        public Task ReplaceAsync(
            SimulationWorld상호작용Graph준비도Response readiness,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<SimulationWorld상호작용Graph준비도Response?> ReadLatestAsync(
            string areaSetStableId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<SimulationWorld상호작용Graph준비도Response?>(null);
    }

    public interface ISimulationWorld상호작용GraphCatalogReader
    {
        Task<SimulationWorld상호작용GraphBindingCatalog> ReadAsync(
            CancellationToken cancellationToken = default);
    }

    public sealed class FileSimulationWorld상호작용GraphCatalogReader :
        ISimulationWorld상호작용GraphCatalogReader
    {
        private readonly string path;

        public FileSimulationWorld상호작용GraphCatalogReader(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("상호작용 Graph 대장 경로가 필요합니다.", nameof(path));
            this.path = ResolvePath(path);
        }

        private static string ResolvePath(string value)
        {
            if (Path.IsPathRooted(value))
                return Path.GetFullPath(value);
            var direct = Path.GetFullPath(value);
            if (File.Exists(direct))
                return direct;
            var current = new DirectoryInfo(AppContext.BaseDirectory);
            while (current != null)
            {
                var candidate = Path.GetFullPath(Path.Combine(current.FullName, value));
                if (File.Exists(candidate))
                    return candidate;
                current = current.Parent;
            }
            return direct;
        }

        public async Task<SimulationWorld상호작용GraphBindingCatalog> ReadAsync(
            CancellationToken cancellationToken = default)
        {
            var bytes = await ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
            var value = JsonSerializer.Deserialize<SimulationWorld상호작용GraphBindingCatalog>(
                bytes,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("SimulationWorldInteractionGraphCatalogInvalid");
            Validate(value);
            value.CatalogHashSha256 = Sha256(bytes);
            return value;
        }

        private static async Task<byte[]> ReadAllBytesAsync(
            string filePath,
            CancellationToken cancellationToken)
        {
            using (var stream = new FileStream(
                filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, true))
            {
                var bytes = new byte[stream.Length];
                var read = 0;
                while (read < bytes.Length)
                {
                    var count = await stream.ReadAsync(
                        bytes, read, bytes.Length - read, cancellationToken).ConfigureAwait(false);
                    if (count == 0)
                        break;
                    read += count;
                }
                if (read != bytes.Length)
                    throw new EndOfStreamException(filePath);
                return bytes;
            }
        }

        private static void Validate(SimulationWorld상호작용GraphBindingCatalog value)
        {
            Require(value.AreaSetStableId, "SimulationWorldInteractionGraphAreaSetInvalid");
            Require(value.CatalogRevision, "SimulationWorldInteractionGraphCatalogRevisionInvalid");
            if (value.Bindings.Length == 0)
                throw new InvalidOperationException("SimulationWorldInteractionGraphBindingsMissing");
            if (value.Bindings.GroupBy(item => item.BindingStableId, StringComparer.Ordinal)
                .Any(group => group.Count() > 1))
                throw new InvalidOperationException("SimulationWorldInteractionGraphBindingDuplicate");
            if (value.Bindings.GroupBy(item => item.WorldInteractionId, StringComparer.Ordinal)
                .Any(group => group.Count() > 1))
                throw new InvalidOperationException("SimulationWorldInteractionGraphWiDuplicate");
            foreach (var binding in value.Bindings)
            {
                Require(binding.BindingStableId, "SimulationWorldInteractionGraphBindingIdInvalid");
                Require(binding.WorldInteractionId, "SimulationWorldInteractionGraphWiInvalid");
                Require(binding.LandscapeGraphStableId, "SimulationWorldInteractionGraphIdInvalid");
                Require(binding.RequiredNodeSemanticCode, "SimulationWorldInteractionGraphSemanticInvalid");
                Require(binding.SpatialRoleCode, "SimulationWorldInteractionGraphRoleInvalid");
                Require(binding.SpatialStableId, "SimulationWorldInteractionSpatialIdInvalid");
                Require(binding.ReviewStatusCode, "SimulationWorldInteractionGraphReviewInvalid");
                foreach (var capacity in binding.BaseCapacities)
                {
                    Require(capacity.CapacityCode, "SimulationWorldInteractionCapacityCodeInvalid");
                    Require(capacity.UnitCode, "SimulationWorldInteractionCapacityUnitInvalid");
                    Require(capacity.EvidenceKindCode, "SimulationWorldInteractionCapacityEvidenceInvalid");
                    Require(capacity.EvidenceReference, "SimulationWorldInteractionCapacityReferenceInvalid");
                    Require(capacity.CapacityRuleRevision, "SimulationWorldInteractionCapacityRuleInvalid");
                    if (capacity.Quantity <= 0m)
                        throw new InvalidOperationException("SimulationWorldInteractionCapacityQuantityInvalid");
                }
            }
        }

        private static void Require(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(errorCode);
        }

        private static string Sha256(byte[] bytes)
        {
            using (var algorithm = SHA256.Create())
                return ToHex(algorithm.ComputeHash(bytes));
        }

        private static string ToHex(byte[] value)
        {
            var text = new StringBuilder(value.Length * 2);
            foreach (var item in value)
                text.Append(item.ToString("x2"));
            return text.ToString();
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "WI와 공간 능력·예약·연결 책임을 결속한다.",
        Boundary = "H 포함 깊이와 E 증거 성숙도를 서로 대신하지 않는다.")]
    public sealed class SimulationWorld상호작용GraphService
    {
        private readonly ISimulationWorldAreaSetGraphStore graphStore;
        private readonly ISimulationWorld상호작용GraphCatalogReader catalogReader;

        public SimulationWorld상호작용GraphService(
            ISimulationWorldAreaSetGraphStore graphStore,
            ISimulationWorld상호작용GraphCatalogReader catalogReader)
        {
            this.graphStore = graphStore ?? throw new ArgumentNullException(nameof(graphStore));
            this.catalogReader = catalogReader ?? throw new ArgumentNullException(nameof(catalogReader));
        }

        public async Task<SimulationWorld상호작용Graph준비도Response> EvaluateAsync(
            string areaSetStableId,
            CancellationToken cancellationToken = default)
        {
            var catalog = await catalogReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (!string.Equals(catalog.AreaSetStableId, areaSetStableId, StringComparison.Ordinal))
                throw new InvalidOperationException("SimulationWorldInteractionGraphAreaSetMismatch");
            var areaSet = await graphStore.ReadAreaSetAsync(areaSetStableId, cancellationToken)
                .ConfigureAwait(false);
            if (areaSet == null)
                throw new InvalidOperationException("SimulationWorldAreaSetNotFound");

            var graphs = new Dictionary<string, SimulationWorldLandscapeGraphResponse?>(
                StringComparer.Ordinal);
            foreach (var graphId in catalog.Bindings.Select(item => item.LandscapeGraphStableId)
                .Distinct(StringComparer.Ordinal))
                graphs[graphId] = await graphStore.ReadGraphAsync(graphId, cancellationToken)
                    .ConfigureAwait(false);

            var bindings = catalog.Bindings
                .OrderBy(item => item.BindingStableId, StringComparer.Ordinal)
                .Select(item => EvaluateBinding(item, catalog, graphs[item.LandscapeGraphStableId]))
                .ToArray();
            var byWi = bindings.ToDictionary(item => item.WorldInteractionId, StringComparer.Ordinal);
            var transitions = catalog.Transitions
                .OrderBy(item => item.TransitionStableId, StringComparer.Ordinal)
                .Select(item => EvaluateTransition(item, byWi, graphs))
                .ToArray();

            foreach (var binding in bindings)
            {
                var related = transitions.Where(item =>
                    string.Equals(item.FromWorldInteractionId, binding.WorldInteractionId,
                        StringComparison.Ordinal)
                    || string.Equals(item.ToWorldInteractionId, binding.WorldInteractionId,
                        StringComparison.Ordinal)).ToArray();
                binding.SpatialClosedLoop = binding.StatusCode ==
                    SimulationWorld상호작용Graph상태Codes.Ready
                    && related.All(item => item.StatusCode ==
                        SimulationWorld상호작용Graph상태Codes.Ready);
                if (!binding.SpatialClosedLoop
                    && binding.StatusCode == SimulationWorld상호작용Graph상태Codes.Ready)
                {
                    binding.StatusCode = SimulationWorld상호작용Graph상태Codes.PathUnresolved;
                    binding.Limitations = binding.Limitations
                        .Concat(new[] { "선행 또는 후속 WI 공간 경로가 닫히지 않았습니다." })
                        .Distinct(StringComparer.Ordinal).ToArray();
                    binding.SpatialDefinition = null;
                }
            }

            var audits = graphs.Values.Where(item => item != null)
                .Cast<SimulationWorldLandscapeGraphResponse>()
                .OrderBy(item => item.LandscapeGraphStableId, StringComparer.Ordinal)
                .Select(item => new SimulationWorld상호작용GraphAuditResponse
                {
                    LandscapeGraphStableId = item.LandscapeGraphStableId,
                    GraphRevision = item.GraphRevision,
                    GraphHashSha256 = item.GraphHashSha256,
                    StatusCode = item.StatusCode,
                    NodeCount = item.Nodes.Length,
                    EdgeCount = item.Edges.Length,
                    ExternalConnectorCount = item.ExternalConnectorStubs.Length,
                    UnresolvedCount = item.Unresolved.Length,
                }).ToArray();

            return new SimulationWorld상호작용Graph준비도Response
            {
                AreaSetStableId = areaSet.AreaSetStableId,
                AreaSetRevision = areaSet.Revision,
                AreaSetDefinitionHashSha256 = areaSet.DefinitionHashSha256,
                BindingCatalogRevision = catalog.CatalogRevision,
                BindingCatalogHashSha256 = catalog.CatalogHashSha256,
                OverallStatusCode = bindings.All(item => item.SpatialClosedLoop)
                    ? SimulationWorld상호작용Graph상태Codes.Ready
                    : SimulationWorld상호작용Graph상태Codes.Partial,
                GraphAudits = audits,
                Bindings = bindings,
                Transitions = transitions,
            };
        }

        public async Task<Simulation공간세계InitialStateRequest> ResolveSpatialWorldAsync(
            string areaSetStableId,
            IEnumerable<string> worldInteractionIds,
            CancellationToken cancellationToken = default)
        {
            var requested = worldInteractionIds.Distinct(StringComparer.Ordinal).ToArray();
            var readiness = await EvaluateAsync(areaSetStableId, cancellationToken)
                .ConfigureAwait(false);
            var definitions = new List<Simulation공간정의InitialRequest>();
            foreach (var wiId in requested)
            {
                var binding = readiness.Bindings.SingleOrDefault(item =>
                    string.Equals(item.WorldInteractionId, wiId, StringComparison.Ordinal));
                if (binding == null || !binding.SpatialClosedLoop || binding.SpatialDefinition == null)
                    throw new InvalidOperationException("SimulationSpatialClosedLoopUnavailable:" + wiId);
                definitions.Add(binding.SpatialDefinition);
            }
            return new Simulation공간세계InitialStateRequest
            {
                Definitions = definitions.GroupBy(item => item.SpatialStableId, StringComparer.Ordinal)
                    .Select(MergeSpatialDefinitions)
                    .OrderBy(item => item.SpatialStableId, StringComparer.Ordinal).ToArray(),
            };
        }

        private static Simulation공간정의InitialRequest MergeSpatialDefinitions(
            IGrouping<string, Simulation공간정의InitialRequest> group)
        {
            var values = group.ToArray();
            var first = values[0];
            if (values.Any(item =>
                !string.Equals(item.LandscapeGraphStableId, first.LandscapeGraphStableId,
                    StringComparison.Ordinal)
                || !string.Equals(item.LandscapeNodeStableId, first.LandscapeNodeStableId,
                    StringComparison.Ordinal)
                || !string.Equals(item.DefinitionRevision, first.DefinitionRevision,
                    StringComparison.Ordinal)))
                throw new InvalidOperationException("SimulationSpatialDefinitionMergeConflict:" + group.Key);

            var capacities = values.SelectMany(item => item.BaseCapacities)
                .GroupBy(item => item.CapacityCode + "|" + item.UnitCode, StringComparer.Ordinal)
                .Select(item => new Simulation공간용량Snapshot
                {
                    CapacityCode = item.First().CapacityCode,
                    Quantity = item.Max(value => value.Quantity),
                    UnitCode = item.First().UnitCode,
                }).OrderBy(item => item.CapacityCode, StringComparer.Ordinal).ToArray();
            var capabilityCodes = values.SelectMany(item => item.CapabilityCodes)
                .Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToArray();
            var sources = values.SelectMany(item => item.SourceStableIds)
                .Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToArray();
            return new Simulation공간정의InitialRequest
            {
                SpatialStableId = first.SpatialStableId,
                FacilityStableId = first.FacilityStableId,
                AreaStableId = first.AreaStableId,
                AreaSetStableId = first.AreaSetStableId,
                LandscapeGraphStableId = first.LandscapeGraphStableId,
                LandscapeNodeStableId = first.LandscapeNodeStableId,
                EvidenceKindCode = first.EvidenceKindCode,
                AccessStateCode = first.AccessStateCode,
                CapabilityCodes = capabilityCodes,
                BaseCapacities = capacities,
                DefinitionRevision = first.DefinitionRevision,
                DefinitionHashSha256 = HashText(string.Join("|", values
                    .Select(item => item.DefinitionHashSha256)
                    .OrderBy(item => item, StringComparer.Ordinal))),
                SourceStableIds = sources,
            };
        }

        private static SimulationWorld상호작용GraphBindingResponse EvaluateBinding(
            SimulationWorld상호작용GraphBindingPlan plan,
            SimulationWorld상호작용GraphBindingCatalog catalog,
            SimulationWorldLandscapeGraphResponse? graph)
        {
            var response = new SimulationWorld상호작용GraphBindingResponse
            {
                BindingStableId = plan.BindingStableId,
                WorldInteractionId = plan.WorldInteractionId,
                LandscapeGraphStableId = plan.LandscapeGraphStableId,
                RequiredNodeSemanticCode = plan.RequiredNodeSemanticCode,
                SpatialRoleCode = plan.SpatialRoleCode,
                SpatialStableId = plan.SpatialStableId,
                FacilityStableId = plan.FacilityStableId,
                AreaStableId = plan.AreaStableId,
                CapabilityCodes = plan.CapabilityCodes.OrderBy(item => item, StringComparer.Ordinal).ToArray(),
                BaseCapacities = plan.BaseCapacities,
                ReviewStatusCode = plan.ReviewStatusCode,
                SourceStableIds = plan.SourceStableIds,
            };
            if (graph == null)
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.WaitingForGraph;
                response.Limitations = new[] { "경관 Graph가 파생 DB에 없습니다." };
                return response;
            }
            response.LandscapeGraphRevision = graph.GraphRevision;
            response.LandscapeGraphHashSha256 = graph.GraphHashSha256;
            if ((plan.RequiredGraphRevision > 0 && plan.RequiredGraphRevision != graph.GraphRevision)
                || (!string.IsNullOrWhiteSpace(plan.RequiredGraphHashSha256)
                    && !string.Equals(plan.RequiredGraphHashSha256, graph.GraphHashSha256,
                        StringComparison.Ordinal)))
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.GraphRevisionMismatch;
                response.Limitations = new[] { "승인한 Graph 개정 또는 해시와 현재 Graph가 다릅니다." };
                return response;
            }
            if (!string.Equals(plan.ReviewStatusCode,
                SimulationWorld상호작용Graph검토Codes.Approved, StringComparison.Ordinal))
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.ReviewRequired;
                response.Limitations = new[] { "공간 능력·용량 연결이 승인되지 않았습니다." };
                return response;
            }
            var nodes = graph.Nodes.Where(item => string.Equals(
                    item.SemanticCode, plan.RequiredNodeSemanticCode, StringComparison.Ordinal))
                .OrderBy(item => item.NodeStableId, StringComparer.Ordinal).ToArray();
            if (nodes.Length == 0)
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.WaitingForNode;
                response.Limitations = new[] { "필요한 공간 역할의 Node가 없습니다." };
                return response;
            }
            var selected = nodes[0];
            response.MatchedLandscapeNodeStableId = selected.NodeStableId;
            response.MatchedNodeEvidenceKindCode = selected.EvidenceKindCode;
            response.StatusCode = SimulationWorld상호작용Graph상태Codes.Ready;
            response.Limitations = nodes.Length > 1
                ? new[] { "같은 의미의 Node가 여러 개여서 고유 식별자 순으로 선택했습니다." }
                : Array.Empty<string>();
            var definitionHash = HashText(string.Join("|", new[]
            {
                graph.GraphHashSha256, catalog.CatalogHashSha256, plan.BindingStableId,
                selected.NodeStableId, string.Join(",", response.CapabilityCodes),
            }));
            response.SpatialDefinition = new Simulation공간정의InitialRequest
            {
                SpatialStableId = plan.SpatialStableId,
                FacilityStableId = plan.FacilityStableId,
                AreaStableId = plan.AreaStableId,
                AreaSetStableId = catalog.AreaSetStableId,
                LandscapeGraphStableId = graph.LandscapeGraphStableId,
                LandscapeNodeStableId = selected.NodeStableId,
                EvidenceKindCode = Simulation공간근거종류Codes.LandscapeGraph,
                CapabilityCodes = response.CapabilityCodes,
                BaseCapacities = plan.BaseCapacities.Select(item => new Simulation공간용량Snapshot
                {
                    CapacityCode = item.CapacityCode,
                    Quantity = item.Quantity,
                    UnitCode = item.UnitCode,
                }).ToArray(),
                DefinitionRevision = "graph:" + graph.GraphRevision + ";binding:" + catalog.CatalogRevision,
                DefinitionHashSha256 = definitionHash,
                SourceStableIds = plan.SourceStableIds.Concat(new[]
                {
                    graph.LandscapeGraphStableId,
                    selected.NodeStableId,
                    "graph-sha256:" + graph.GraphHashSha256,
                    "binding-sha256:" + catalog.CatalogHashSha256,
                }).Distinct(StringComparer.Ordinal).OrderBy(item => item, StringComparer.Ordinal).ToArray(),
            };
            return response;
        }

        private static SimulationWorld상호작용GraphTransitionResponse EvaluateTransition(
            SimulationWorld상호작용GraphTransitionPlan plan,
            IReadOnlyDictionary<string, SimulationWorld상호작용GraphBindingResponse> bindings,
            IReadOnlyDictionary<string, SimulationWorldLandscapeGraphResponse?> graphs)
        {
            var response = new SimulationWorld상호작용GraphTransitionResponse
            {
                TransitionStableId = plan.TransitionStableId,
                FromWorldInteractionId = plan.FromWorldInteractionId,
                ToWorldInteractionId = plan.ToWorldInteractionId,
            };
            if (!bindings.TryGetValue(plan.FromWorldInteractionId, out var from)
                || !bindings.TryGetValue(plan.ToWorldInteractionId, out var to)
                || string.IsNullOrWhiteSpace(from.MatchedLandscapeNodeStableId)
                || string.IsNullOrWhiteSpace(to.MatchedLandscapeNodeStableId))
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.PathUnresolved;
                response.Limitations = new[] { "선행 또는 후속 WI 공간 Node가 없습니다." };
                return response;
            }
            response.FromLandscapeNodeStableId = from.MatchedLandscapeNodeStableId;
            response.ToLandscapeNodeStableId = to.MatchedLandscapeNodeStableId;
            if (!string.Equals(from.LandscapeGraphStableId, to.LandscapeGraphStableId,
                StringComparison.Ordinal))
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.PathUnresolved;
                response.Limitations = new[] { "서로 다른 Graph의 연결은 양쪽 외부 연결점이 필요합니다." };
                return response;
            }
            var graph = graphs[from.LandscapeGraphStableId];
            if (graph == null)
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.WaitingForGraph;
                return response;
            }
            if (plan.ExternalConnectorRequired)
            {
                var connector = graph.ExternalConnectorStubs.FirstOrDefault(item =>
                    string.IsNullOrWhiteSpace(plan.RequiredConnectorTypeCode)
                    || string.Equals(item.ConnectorTypeCode, plan.RequiredConnectorTypeCode,
                        StringComparison.Ordinal));
                if (connector == null)
                {
                    response.StatusCode = SimulationWorld상호작용Graph상태Codes.PathUnresolved;
                    response.Limitations = new[] { "필요한 외부 연결점이 없습니다." };
                    return response;
                }
                response.ExternalConnectorStableId = connector.StubStableId;
            }
            if (string.Equals(from.MatchedLandscapeNodeStableId,
                to.MatchedLandscapeNodeStableId, StringComparison.Ordinal))
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.Ready;
                return response;
            }
            var path = FindPath(graph, from.MatchedLandscapeNodeStableId,
                to.MatchedLandscapeNodeStableId);
            if (path.Length == 0)
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.PathUnresolved;
                response.Limitations = new[] { "두 WI 공간 사이에 Graph Edge 경로가 없습니다." };
                return response;
            }
            response.EdgeStableIds = path;
            response.StatusCode = SimulationWorld상호작용Graph상태Codes.Ready;
            return response;
        }

        private static string[] FindPath(
            SimulationWorldLandscapeGraphResponse graph,
            string start,
            string target)
        {
            var queue = new Queue<string>();
            var visited = new HashSet<string>(StringComparer.Ordinal) { start };
            var previous = new Dictionary<string, Tuple<string, string>>(StringComparer.Ordinal);
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var edge in graph.Edges.Where(item =>
                    string.Equals(item.FromNodeStableId, current, StringComparison.Ordinal)
                    || string.Equals(item.ToNodeStableId, current, StringComparison.Ordinal))
                    .OrderBy(item => item.EdgeStableId, StringComparer.Ordinal))
                {
                    var next = string.Equals(edge.FromNodeStableId, current, StringComparison.Ordinal)
                        ? edge.ToNodeStableId : edge.FromNodeStableId;
                    if (!visited.Add(next))
                        continue;
                    previous[next] = Tuple.Create(current, edge.EdgeStableId);
                    if (string.Equals(next, target, StringComparison.Ordinal))
                    {
                        var result = new List<string>();
                        var cursor = target;
                        while (!string.Equals(cursor, start, StringComparison.Ordinal))
                        {
                            var step = previous[cursor];
                            result.Add(step.Item2);
                            cursor = step.Item1;
                        }
                        result.Reverse();
                        return result.ToArray();
                    }
                    queue.Enqueue(next);
                }
            }
            return Array.Empty<string>();
        }

        private static string HashText(string value)
        {
            using (var algorithm = SHA256.Create())
            {
                var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                var text = new StringBuilder(hash.Length * 2);
                foreach (var item in hash)
                    text.Append(item.ToString("x2"));
                return text.ToString();
            }
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "WI와 공간 능력·예약·연결 책임을 결속한다.",
        Boundary = "H 포함 깊이와 E 증거 성숙도를 서로 대신하지 않는다.")]
    public sealed class SimulationWorld상호작용GraphJobShell
    {
        private readonly SimulationWorld상호작용GraphService service;
        private readonly ISimulationWorld상호작용GraphReadinessStore store;

        public SimulationWorld상호작용GraphJobShell(
            SimulationWorld상호작용GraphService service,
            ISimulationWorld상호작용GraphReadinessStore store)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public async Task<SimulationWorld상호작용Graph준비도Response> BuildAsync(
            string areaSetStableId,
            CancellationToken cancellationToken = default)
        {
            var readiness = await service.EvaluateAsync(areaSetStableId, cancellationToken)
                .ConfigureAwait(false);
            await store.ReplaceAsync(readiness, cancellationToken).ConfigureAwait(false);
            return readiness;
        }
    }
}
