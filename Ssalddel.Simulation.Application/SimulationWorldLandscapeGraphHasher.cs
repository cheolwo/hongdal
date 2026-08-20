using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    internal static class SimulationWorldLandscapeGraphHasher
    {
        public static void Finalize(SimulationWorldLandscapeGraphResponse graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            graph.GraphBuildStableId = string.Empty;
            graph.GraphHashSha256 = string.Empty;
            using var sha = SHA256.Create();
            graph.GraphHashSha256 = BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(SerializeForHash(graph))))
                .Replace("-", string.Empty).ToLowerInvariant();
            graph.GraphBuildStableId = graph.LandscapeGraphStableId
                                       + ":build:" + graph.GraphHashSha256[..24];
        }

        private static string SerializeForHash(SimulationWorldLandscapeGraphResponse graph)
        {
            if (!string.IsNullOrWhiteSpace(graph.SpatialOwnerKindCode)
                || !string.IsNullOrWhiteSpace(graph.SpatialOwnerStableId)
                || !string.IsNullOrWhiteSpace(graph.CoordinateSpaceCode))
                return JsonSerializer.Serialize(graph);

            // 기존 E4 Graph의 승인 hash는 optional E5 소유자 필드를 추가하기 전의
            // canonical JSON을 기준으로 한다. 빈 신규 필드가 기존 승인 근거를 무효화하지 않는다.
            return JsonSerializer.Serialize(new
            {
                graph.SchemaVersion,
                graph.AreaSetStableId,
                graph.LandscapeGraphStableId,
                graph.GraphBuildStableId,
                graph.GraphRoleCode,
                graph.GraphRevision,
                graph.DefinitionHashSha256,
                graph.GraphHashSha256,
                graph.GrammarRevision,
                graph.GrammarHashSha256,
                graph.StatusCode,
                graph.Bounds,
                graph.AreaRefs,
                graph.TileRefs,
                graph.ScenarioRouteRefs,
                graph.Nodes,
                graph.Edges,
                graph.Placements,
                graph.ExternalConnectorStubs,
                graph.Unresolved,
                graph.PresentationOnly,
                graph.IsOperationalState,
            });
        }
    }
}
