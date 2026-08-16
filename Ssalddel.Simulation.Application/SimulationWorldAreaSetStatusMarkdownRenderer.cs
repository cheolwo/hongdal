using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    public static class SimulationWorldAreaSetStatusMarkdownRenderer
    {
        public static string Render(
            SimulationWorldAreaSetDefinitionResponse areaSet,
            IReadOnlyList<SimulationWorldLandscapeGraphResponse> graphs)
        {
            if (areaSet == null) throw new ArgumentNullException(nameof(areaSet));
            if (graphs == null) throw new ArgumentNullException(nameof(graphs));
            var byId = graphs.ToDictionary(
                item => item.LandscapeGraphStableId, StringComparer.Ordinal);
            var builder = new StringBuilder();
            builder.AppendLine("<!-- 이 문서는 AreaSet 상태 renderer가 다시 생성하는 산출물입니다. 직접 수정하지 마십시오. -->");
            builder.AppendLine();
            builder.AppendLine("# " + areaSet.Title + " 상태");
            builder.AppendLine();
            builder.AppendLine("- 고유 식별자: `" + areaSet.AreaSetStableId + "`");
            builder.AppendLine("- 정의 개정: `" + areaSet.Revision + "`");
            builder.AppendLine("- 정의 SHA-256: `" + areaSet.DefinitionHashSha256 + "`");
            builder.AppendLine("- 사람 문서 SHA-256: `" + areaSet.DocumentHashSha256 + "`");
            builder.AppendLine("- Area / ScenarioRoute / Graph / 관계: `"
                               + areaSet.AreaRefs.Length + " / "
                               + areaSet.ScenarioRouteRefs.Length + " / "
                               + areaSet.LandscapeGraphs.Length + " / "
                               + areaSet.GraphRelations.Length + "`");
            builder.AppendLine();
            builder.AppendLine("```text");
            builder.AppendLine("AreaSet");
            for (var index = 0; index < areaSet.LandscapeGraphs.Length; index++)
            {
                var descriptor = areaSet.LandscapeGraphs[index];
                if (!byId.TryGetValue(descriptor.LandscapeGraphStableId, out var graph))
                    throw new InvalidOperationException(
                        "AreaSetStatusGraphMissing:" + descriptor.LandscapeGraphStableId);
                var branch = index == areaSet.LandscapeGraphs.Length - 1 ? "└─" : "├─";
                builder.AppendLine(branch + " " + KoreanRole(graph.GraphRoleCode)
                                   + " [" + graph.StatusCode + "]"
                                   + " Tile=" + graph.TileRefs.Length
                                   + " Placement=" + graph.Placements.Length
                                   + " Unresolved=" + graph.Unresolved.Length);
            }
            builder.AppendLine("```");
            builder.AppendLine();
            builder.AppendLine("## Graph 실행 상태");
            builder.AppendLine();
            builder.AppendLine("| 경관 Graph | 상태 | Tile | Node | Edge | 배치 | 외부 연결 | 미해결 | Graph SHA-256 |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |");
            foreach (var descriptor in areaSet.LandscapeGraphs)
            {
                var graph = byId[descriptor.LandscapeGraphStableId];
                builder.AppendLine("| " + KoreanRole(graph.GraphRoleCode)
                                   + "<br>`" + graph.LandscapeGraphStableId + "` | `"
                                   + graph.StatusCode + "` | " + graph.TileRefs.Length
                                   + " | " + graph.Nodes.Length
                                   + " | " + graph.Edges.Length
                                   + " | " + graph.Placements.Length
                                   + " | " + graph.ExternalConnectorStubs.Length
                                   + " | " + graph.Unresolved.Length
                                   + " | `" + graph.GraphHashSha256 + "` |");
            }
            builder.AppendLine();
            builder.AppendLine("`Declared`와 `PartialUnresolved`는 자료 부족을 꾸며내지 않고 남긴 상태다. Unity의 플레이어별 `Prepared / Active / Cached`는 이 서버 빌드 상태와 별도로 관리한다.");
            return builder.ToString().Replace("\r\n", "\n");
        }

        private static string KoreanRole(string value) => value switch
        {
            "FarmCore" => "대관령면 Farm",
            "FarmHubCorridor" => "Farm–Hub 회랑",
            "HubCore" => "진부면 Hub",
            "HubTownCorridor" => "Hub–Town 회랑",
            "TownCore" => "평창읍 Town",
            _ => value,
        };
    }
}
