using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// 4개 실제 E5 AreaSet과 AreaSet Network 위에서 64개 WI의 공간 참여 방식을
    /// 직접 42, 문맥 6, 비공간 9, E5 대기 7로 분리해 검증한다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "WI와 공간 능력·예약·연결 책임을 결속한다.",
        Boundary = "H 포함 깊이와 E 증거 성숙도를 서로 대신하지 않는다.")]
    public sealed class SimulationWorld상호작용NetworkService
    {
        private readonly ISimulationWorldActualE5SpatialCatalogReader reader;

        public SimulationWorld상호작용NetworkService(
            ISimulationWorldActualE5SpatialCatalogReader reader) =>
            this.reader = reader ?? throw new ArgumentNullException(nameof(reader));

        public Task<SimulationWorld상호작용Network준비도Response> EvaluateAsync(
            string networkStableId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!reader.TryRead(out var spatial, out var errorCode))
                throw new InvalidOperationException(errorCode);
            if (!string.Equals(spatial.Network.NetworkStableId, networkStableId,
                    StringComparison.Ordinal))
                throw new InvalidOperationException("SimulationWorldAreaSetNetworkNotFound");

            var catalog = spatial.InteractionSpatialCatalog;
            ValidateCatalog(catalog, spatial);
            var direct = catalog.Bindings
                .OrderBy(item => item.WorldInteractionId, StringComparer.Ordinal)
                .Select(item => EvaluateBinding(item, catalog, spatial.Graphs))
                .ToArray();
            var directByWi = direct.ToDictionary(
                item => item.WorldInteractionId, StringComparer.Ordinal);
            var contextByWi = catalog.ContextualBindings.ToDictionary(
                item => item.WorldInteractionId, StringComparer.Ordinal);
            var nonSpatial = catalog.NonSpatialWiIds.ToHashSet(StringComparer.Ordinal);
            var pendingE5 = catalog.PendingE5WiIds.ToHashSet(StringComparer.Ordinal);
            var transitions = catalog.Transitions
                .OrderBy(item => item.TransitionStableId, StringComparer.Ordinal)
                .Select(item => EvaluateTransition(
                    item, directByWi, contextByWi, nonSpatial, spatial))
                .ToArray();

            foreach (var binding in direct)
            {
                var related = transitions.Where(item =>
                    item.FromWorldInteractionId == binding.WorldInteractionId
                    || item.ToWorldInteractionId == binding.WorldInteractionId).ToArray();
                // 미래 E5 대기 WI와의 인계는 Network 전체 준비도에는 남기되,
                // 이미 승인된 직접 WI의 현재 공간 폐루프를 소급해 무효화하지 않는다.
                var blockingRelated = related.Where(item =>
                    !pendingE5.Contains(item.FromWorldInteractionId)
                    && !pendingE5.Contains(item.ToWorldInteractionId)).ToArray();
                binding.SpatialClosedLoop = binding.StatusCode ==
                    SimulationWorld상호작용Graph상태Codes.Ready
                    && blockingRelated.All(IsReadyTransition);
                if (!binding.SpatialClosedLoop
                    && blockingRelated.Any(item => !IsReadyTransition(item)))
                    binding.Limitations = binding.Limitations
                        .Concat(new[] { "선행 또는 후속 WI와의 공간 인계가 닫히지 않았습니다." })
                        .Distinct(StringComparer.Ordinal).ToArray();
            }

            var contextual = catalog.ContextualBindings
                .OrderBy(item => item.WorldInteractionId, StringComparer.Ordinal)
                .Select(item => new SimulationWorld상호작용ContextBindingResponse
                {
                    WorldInteractionId = item.WorldInteractionId,
                    ParticipationCode = item.ParticipationCode,
                    ContextStableId = item.ContextStableId,
                    StatusCode = item.ContextBindingStateCode,
                    SourceStableIds = item.SourceStableIds,
                }).ToArray();
            var nonSpatialBindings = catalog.NonSpatialWiIds
                .OrderBy(item => item, StringComparer.Ordinal)
                .Select(item => new SimulationWorld상호작용NonSpatialResponse
                {
                    WorldInteractionId = item,
                }).ToArray();
            var allReady = direct.All(item => item.SpatialClosedLoop)
                           && contextual.All(item => item.StatusCode ==
                               SimulationWorld상호작용Graph상태Codes.ContextBound)
                           && nonSpatialBindings.All(item => item.StatusCode ==
                               SimulationWorld상호작용Graph상태Codes.NotSpatiallyApplicable)
                           && transitions.All(IsReadyTransition)
                           && catalog.PendingE5WiIds.Length == 0;

            return Task.FromResult(new SimulationWorld상호작용Network준비도Response
            {
                NetworkStableId = spatial.Network.NetworkStableId,
                NetworkRevision = spatial.Network.Revision,
                NetworkDefinitionHashSha256 = spatial.Network.DefinitionHashSha256,
                BindingCatalogRevision = catalog.CatalogRevision,
                BindingCatalogHashSha256 = catalog.CatalogHashSha256,
                OverallStatusCode = allReady
                    ? SimulationWorld상호작용Graph상태Codes.Ready
                    : SimulationWorld상호작용Graph상태Codes.Partial,
                GraphAudits = spatial.Graphs.Values
                    .OrderBy(item => item.LandscapeGraphStableId, StringComparer.Ordinal)
                    .Select(Audit).ToArray(),
                DirectBindings = direct,
                ContextualBindings = contextual,
                NonSpatialBindings = nonSpatialBindings,
                PendingE5WiIds = catalog.PendingE5WiIds
                    .OrderBy(item => item, StringComparer.Ordinal).ToArray(),
                Transitions = transitions,
                TotalWorldInteractionCount = direct.Length + contextual.Length
                                             + nonSpatialBindings.Length
                                             + catalog.PendingE5WiIds.Length,
                PresentationOnly = true,
                IsOperationalState = false,
            });
        }

        /// <summary>
        /// 실제 E5 대장에서 공간 폐루프가 닫힌 WI만 세션 초기 공간으로 해석한다.
        /// 해석 실패를 Scenario 공간으로 대체하지 않는다.
        /// </summary>
        public async Task<Simulation공간세계InitialStateRequest> ResolveSpatialWorldAsync(
            string networkStableId,
            string areaSetStableId,
            IEnumerable<string> worldInteractionIds,
            CancellationToken cancellationToken = default)
        {
            if (worldInteractionIds == null)
                throw new ArgumentNullException(nameof(worldInteractionIds));
            var requested = worldInteractionIds
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(item => item, StringComparer.Ordinal)
                .ToArray();
            if (requested.Length == 0)
                throw new InvalidOperationException("SimulationActualE5WorldInteractionRequired");

            var readiness = await EvaluateAsync(networkStableId, cancellationToken)
                .ConfigureAwait(false);
            var definitions = new List<Simulation공간정의InitialRequest>();
            foreach (var wiId in requested)
            {
                var binding = readiness.DirectBindings.SingleOrDefault(item =>
                    string.Equals(item.WorldInteractionId, wiId, StringComparison.Ordinal)
                    && string.Equals(item.AreaSetStableId, areaSetStableId,
                        StringComparison.Ordinal));
                if (binding == null || !binding.SpatialClosedLoop
                    || binding.SpatialDefinition == null)
                    throw new InvalidOperationException(
                        "SimulationActualE5SpatialClosedLoopUnavailable:" + wiId);
                definitions.Add(binding.SpatialDefinition);
            }

            return new Simulation공간세계InitialStateRequest
            {
                Definitions = definitions
                    .OrderBy(item => item.SpatialStableId, StringComparer.Ordinal)
                    .ToArray(),
            };
        }

        private static void ValidateCatalog(
            SimulationWorld상호작용NetworkBindingCatalog catalog,
            SimulationWorldActualE5SpatialCatalog spatial)
        {
            Require(catalog.SchemaVersion == "simulation-world-interaction-graph-binding.v2",
                "SimulationWorldInteractionNetworkCatalogSchemaInvalid");
            Require(catalog.NetworkStableId == spatial.Network.NetworkStableId,
                "SimulationWorldInteractionNetworkCatalogMismatch");
            Require(catalog.CatalogHashSha256.Length == 64,
                "SimulationWorldInteractionNetworkCatalogHashInvalid");
            Require(catalog.Bindings.Length == 42
                    && catalog.ContextualBindings.Length == 6
                    && catalog.NonSpatialWiIds.Length == 9
                    && catalog.PendingE5WiIds.Length == 7,
                "SimulationWorldInteractionNetworkPartitionInvalid");
            var ids = catalog.Bindings.Select(item => item.WorldInteractionId)
                .Concat(catalog.ContextualBindings.Select(item => item.WorldInteractionId))
                .Concat(catalog.NonSpatialWiIds)
                .Concat(catalog.PendingE5WiIds).ToArray();
            Require(ids.Length == 64 && ids.Distinct(StringComparer.Ordinal).Count() == 64,
                "SimulationWorldInteractionNetworkCoverageInvalid");
            Require(catalog.Bindings.All(item =>
                    item.ParticipationCode == SimulationWorld상호작용공간참여Codes.Required
                    && item.H1Ref.Length > 0 && item.H2Ref.Length > 0 && item.H3Ref.Length > 0),
                "SimulationWorldInteractionNetworkHierarchyInvalid");
            Require(catalog.ContextualBindings.All(item =>
                    item.ParticipationCode == SimulationWorld상호작용공간참여Codes.Contextual
                    && item.ContextBindingStateCode ==
                    SimulationWorld상호작용Graph상태Codes.ContextBound),
                "SimulationWorldInteractionNetworkContextInvalid");
        }

        private static SimulationWorld상호작용GraphBindingResponse EvaluateBinding(
            SimulationWorld상호작용GraphBindingPlan plan,
            SimulationWorld상호작용NetworkBindingCatalog catalog,
            IReadOnlyDictionary<string, SimulationWorldLandscapeGraphResponse> graphs)
        {
            var response = new SimulationWorld상호작용GraphBindingResponse
            {
                BindingStableId = plan.BindingStableId,
                WorldInteractionId = plan.WorldInteractionId,
                ParticipationCode = plan.ParticipationCode,
                AreaSetStableId = plan.AreaSetStableId,
                SpatialOwnerKindCode = plan.SpatialOwnerKindCode,
                SpatialOwnerStableId = plan.SpatialOwnerStableId,
                LandscapeGraphStableId = plan.LandscapeGraphStableId,
                RequiredNodeSemanticCode = plan.RequiredNodeSemanticCode,
                SpatialRoleCode = plan.SpatialRoleCode,
                SpatialStableId = plan.SpatialStableId,
                FacilityStableId = plan.FacilityStableId,
                AreaStableId = plan.AreaStableId,
                CapabilityCodes = plan.CapabilityCodes,
                BaseCapacities = plan.BaseCapacities,
                ReviewStatusCode = plan.ReviewStatusCode,
                H1Ref = plan.H1Ref,
                H2Ref = plan.H2Ref,
                H3Ref = plan.H3Ref,
                SourceStableIds = plan.SourceStableIds,
            };
            if (!graphs.TryGetValue(plan.LandscapeGraphStableId, out var graph))
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.WaitingForGraph;
                response.Limitations = new[] { "실제 E5 경관 Graph가 없습니다." };
                return response;
            }
            response.LandscapeGraphRevision = graph.GraphRevision;
            response.LandscapeGraphHashSha256 = graph.GraphHashSha256;
            if (graph.GraphRevision != plan.RequiredGraphRevision
                || graph.GraphHashSha256 != plan.RequiredGraphHashSha256)
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.GraphRevisionMismatch;
                response.Limitations = new[] { "승인된 WI 결속과 실제 E5 Graph 판본이 다릅니다." };
                return response;
            }
            if (plan.ReviewStatusCode != SimulationWorld상호작용Graph검토Codes.Approved)
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.ReviewRequired;
                response.Limitations = new[] { "공간 결속 검토가 승인되지 않았습니다." };
                return response;
            }
            var node = graph.Nodes
                .Where(item => item.SemanticCode == plan.RequiredNodeSemanticCode)
                .OrderBy(item => item.NodeStableId, StringComparer.Ordinal)
                .FirstOrDefault();
            if (node == null)
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.WaitingForNode;
                response.Limitations = new[] { "H1 실행 공간 Node가 없습니다." };
                return response;
            }
            response.MatchedLandscapeNodeStableId = node.NodeStableId;
            response.MatchedNodeEvidenceKindCode = node.EvidenceKindCode;
            response.StatusCode = SimulationWorld상호작용Graph상태Codes.Ready;
            response.SpatialDefinition = new Simulation공간정의InitialRequest
            {
                SpatialStableId = plan.SpatialStableId,
                FacilityStableId = plan.FacilityStableId,
                AreaStableId = plan.AreaStableId,
                AreaSetStableId = plan.AreaSetStableId,
                LandscapeGraphStableId = graph.LandscapeGraphStableId,
                LandscapeNodeStableId = node.NodeStableId,
                EvidenceKindCode = Simulation공간근거종류Codes.LandscapeGraph,
                CapabilityCodes = plan.CapabilityCodes,
                BaseCapacities = plan.BaseCapacities.Select(item =>
                    new Simulation공간용량Snapshot
                    {
                        CapacityCode = item.CapacityCode,
                        Quantity = item.Quantity,
                        UnitCode = item.UnitCode,
                    }).ToArray(),
                DefinitionRevision = "graph:" + graph.GraphRevision
                                     + ";binding:" + catalog.CatalogRevision,
                DefinitionHashSha256 = HashText(string.Join("|", new[]
                {
                    graph.GraphHashSha256, catalog.CatalogHashSha256,
                    plan.BindingStableId, node.NodeStableId,
                })),
                SourceStableIds = plan.SourceStableIds.Concat(new[]
                {
                    graph.LandscapeGraphStableId,
                    node.NodeStableId,
                    "graph-sha256:" + graph.GraphHashSha256,
                    "binding-sha256:" + catalog.CatalogHashSha256,
                }).Distinct(StringComparer.Ordinal)
                    .OrderBy(item => item, StringComparer.Ordinal).ToArray(),
            };
            return response;
        }

        private static SimulationWorld상호작용GraphTransitionResponse EvaluateTransition(
            SimulationWorld상호작용GraphTransitionPlan plan,
            IReadOnlyDictionary<string, SimulationWorld상호작용GraphBindingResponse> direct,
            IReadOnlyDictionary<string, SimulationWorld상호작용ContextBindingPlan> contextual,
            ISet<string> nonSpatial,
            SimulationWorldActualE5SpatialCatalog spatial)
        {
            var response = new SimulationWorld상호작용GraphTransitionResponse
            {
                TransitionStableId = plan.TransitionStableId,
                FromWorldInteractionId = plan.FromWorldInteractionId,
                ToWorldInteractionId = plan.ToWorldInteractionId,
            };
            var fromClassified = direct.ContainsKey(plan.FromWorldInteractionId)
                                 || contextual.ContainsKey(plan.FromWorldInteractionId)
                                 || nonSpatial.Contains(plan.FromWorldInteractionId);
            var toClassified = direct.ContainsKey(plan.ToWorldInteractionId)
                               || contextual.ContainsKey(plan.ToWorldInteractionId)
                               || nonSpatial.Contains(plan.ToWorldInteractionId);
            if (!fromClassified || !toClassified)
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.PathUnresolved;
                response.Limitations = new[] { "WI 공간 참여 분류가 없습니다." };
                return response;
            }
            if (!direct.TryGetValue(plan.FromWorldInteractionId, out var from)
                || !direct.TryGetValue(plan.ToWorldInteractionId, out var to))
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.Ready;
                response.Limitations = new[] { "문맥 또는 비공간 WI 인계이므로 직접 경로를 요구하지 않습니다." };
                return response;
            }
            if (from.StatusCode != SimulationWorld상호작용Graph상태Codes.Ready
                || to.StatusCode != SimulationWorld상호작용Graph상태Codes.Ready)
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.PathUnresolved;
                response.Limitations = new[] { "선행 또는 후속 WI의 H1 Node 결속이 준비되지 않았습니다." };
                return response;
            }
            response.FromLandscapeNodeStableId = from.MatchedLandscapeNodeStableId;
            response.ToLandscapeNodeStableId = to.MatchedLandscapeNodeStableId;
            if (from.LandscapeGraphStableId == to.LandscapeGraphStableId)
            {
                var graph = spatial.Graphs[from.LandscapeGraphStableId];
                if (from.MatchedLandscapeNodeStableId != to.MatchedLandscapeNodeStableId)
                {
                    response.EdgeStableIds = FindPath(graph,
                        from.MatchedLandscapeNodeStableId,
                        to.MatchedLandscapeNodeStableId);
                    if (response.EdgeStableIds.Length == 0)
                    {
                        response.StatusCode = SimulationWorld상호작용Graph상태Codes.PathUnresolved;
                        response.Limitations = new[] { "같은 Graph의 H1 Node 사이 Edge 경로가 없습니다." };
                        return response;
                    }
                }
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.Ready;
                return response;
            }
            if (from.AreaSetStableId == to.AreaSetStableId)
            {
                var fromGraph = spatial.Graphs[from.LandscapeGraphStableId];
                var toGraph = spatial.Graphs[to.LandscapeGraphStableId];
                var fromStub = fromGraph.ExternalConnectorStubs
                    .OrderBy(item => item.StubStableId, StringComparer.Ordinal).FirstOrDefault();
                var toStub = toGraph.ExternalConnectorStubs
                    .OrderBy(item => item.StubStableId, StringComparer.Ordinal).FirstOrDefault();
                if (fromStub == null || toStub == null)
                {
                    response.StatusCode = SimulationWorld상호작용Graph상태Codes.PathUnresolved;
                    response.Limitations = new[] { "AreaSet 내부 Graph 인계 연결점이 없습니다." };
                    return response;
                }
                response.ExternalConnectorStableId = fromStub.StubStableId;
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.Ready;
                return response;
            }
            var relation = spatial.Network.Relations.FirstOrDefault(item =>
                item.FromAreaSetStableId == from.AreaSetStableId
                && item.ToAreaSetStableId == to.AreaSetStableId);
            if (relation == null)
            {
                response.StatusCode = SimulationWorld상호작용Graph상태Codes.PathUnresolved;
                response.Limitations = new[] { "AreaSet Network에 이 방향의 인계 관계가 없습니다." };
                return response;
            }
            response.NetworkRelationStableId = relation.RelationStableId;
            response.RouteGraphStableId = relation.RouteGraphStableId;
            response.ExternalConnectorStableId = relation.FromConnectorStableId;
            response.StatusCode = SimulationWorld상호작용Graph상태Codes.NetworkRelationReady;
            return response;
        }

        private static bool IsReadyTransition(
            SimulationWorld상호작용GraphTransitionResponse transition) =>
            transition.StatusCode == SimulationWorld상호작용Graph상태Codes.Ready
            || transition.StatusCode ==
            SimulationWorld상호작용Graph상태Codes.NetworkRelationReady;

        private static SimulationWorld상호작용GraphAuditResponse Audit(
            SimulationWorldLandscapeGraphResponse graph) => new()
        {
            LandscapeGraphStableId = graph.LandscapeGraphStableId,
            GraphRevision = graph.GraphRevision,
            GraphHashSha256 = graph.GraphHashSha256,
            StatusCode = graph.StatusCode,
            NodeCount = graph.Nodes.Length,
            EdgeCount = graph.Edges.Length,
            ExternalConnectorCount = graph.ExternalConnectorStubs.Length,
            UnresolvedCount = graph.Unresolved.Length,
        };

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
                             item.FromNodeStableId == current || item.ToNodeStableId == current)
                         .OrderBy(item => item.EdgeStableId, StringComparer.Ordinal))
                {
                    var next = edge.FromNodeStableId == current
                        ? edge.ToNodeStableId : edge.FromNodeStableId;
                    if (!visited.Add(next)) continue;
                    previous[next] = Tuple.Create(current, edge.EdgeStableId);
                    if (next != target)
                    {
                        queue.Enqueue(next);
                        continue;
                    }
                    var result = new List<string>();
                    var cursor = target;
                    while (cursor != start)
                    {
                        var step = previous[cursor];
                        result.Add(step.Item2);
                        cursor = step.Item1;
                    }
                    result.Reverse();
                    return result.ToArray();
                }
            }
            return Array.Empty<string>();
        }

        private static string HashText(string value)
        {
            using (var algorithm = SHA256.Create())
            {
                var bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(value));
                var text = new StringBuilder(bytes.Length * 2);
                foreach (var item in bytes) text.Append(item.ToString("x2"));
                return text.ToString();
            }
        }

        private static void Require(bool condition, string errorCode)
        {
            if (!condition) throw new InvalidOperationException(errorCode);
        }
    }
}
