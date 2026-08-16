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
                    Encoding.UTF8.GetBytes(JsonSerializer.Serialize(graph))))
                .Replace("-", string.Empty).ToLowerInvariant();
            graph.GraphBuildStableId = graph.LandscapeGraphStableId
                                       + ":build:" + graph.GraphHashSha256[..24];
        }
    }
}
