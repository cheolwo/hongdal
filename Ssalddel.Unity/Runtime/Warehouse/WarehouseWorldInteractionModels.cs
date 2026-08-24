using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Unity.Warehouse
{
    public static class WarehouseLocationSocketKeys
    {
        public const string InboundDock = "warehouse.inbound-dock";
        public const string StorageZone = "warehouse.storage-zone";
        public const string RackZone = "warehouse.rack-zone";
        public const string OutboundStaging = "warehouse.outbound-staging";
        public const string Approach = "warehouse.approach";
        public const string StaffEntry = "warehouse.staff-entry";
        public const string InspectionZone = "warehouse.inspection-zone";
        public const string VehicleExit = "warehouse.vehicle-exit";
        public const string UnassignedArea = "warehouse.unassigned-area";
    }

    public sealed class WarehouseLocationResolution
    {
        public string RequestedCode { get; set; } = string.Empty;
        public string SocketKey { get; set; } = WarehouseLocationSocketKeys.UnassignedArea;
        public bool IsAssigned { get; set; }
        public bool IsKnown { get; set; }
    }

    /// <summary>
    /// 서버 위치 코드를 Scene의 의미 소켓으로 변환합니다. 보관 위치 코드는 Rack 영역에만
    /// 투영하며 실제 랙 칸, 적재 용량 또는 물리 팔레트 수를 추론하지 않습니다.
    /// </summary>
    public sealed class WarehouseLocationResolver
    {
        private static readonly HashSet<string> SemanticKeys = new(StringComparer.Ordinal)
        {
            WarehouseLocationSocketKeys.InboundDock,
            WarehouseLocationSocketKeys.StorageZone,
            WarehouseLocationSocketKeys.RackZone,
            WarehouseLocationSocketKeys.OutboundStaging,
            WarehouseLocationSocketKeys.Approach,
            WarehouseLocationSocketKeys.StaffEntry,
            WarehouseLocationSocketKeys.InspectionZone,
            WarehouseLocationSocketKeys.VehicleExit,
            WarehouseLocationSocketKeys.UnassignedArea,
        };

        public WarehouseLocationResolution Resolve(string? locationCode)
        {
            var code = locationCode?.Trim() ?? string.Empty;
            if (code.Length == 0)
                return Resolution(code, WarehouseLocationSocketKeys.UnassignedArea, false, true);
            if (SemanticKeys.Contains(code))
                return Resolution(code, code, code != WarehouseLocationSocketKeys.UnassignedArea, true);
            if (LooksLikeStorageLocation(code))
                return Resolution(code, WarehouseLocationSocketKeys.RackZone, true, true);
            return Resolution(code, WarehouseLocationSocketKeys.UnassignedArea, false, false);
        }

        private static bool LooksLikeStorageLocation(string code)
            => code.Any(char.IsLetterOrDigit) && code.All(value => char.IsLetterOrDigit(value) || value is '-' or '_' or '.');

        private static WarehouseLocationResolution Resolution(string requested, string socket, bool assigned, bool known)
            => new() { RequestedCode = requested, SocketKey = socket, IsAssigned = assigned, IsKnown = known };
    }

    /// <summary>기존 W1 소비 코드와의 호환 facade입니다.</summary>
    public sealed class WarehouseLocationCatalog
    {
        private readonly WarehouseLocationResolver resolver;

        public WarehouseLocationCatalog()
            : this(new WarehouseLocationResolver())
        {
        }

        public WarehouseLocationCatalog(WarehouseLocationResolver resolver)
            => this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

        public WarehouseLocationResolution Resolve(string? locationCode)
            => resolver.Resolve(locationCode);
    }

    public sealed class WarehouseWorldSelection
    {
        public WarehouseWorldObject Selected { get; set; } = null!;
        public WarehouseWorldObject[] Related { get; set; } = Array.Empty<WarehouseWorldObject>();
    }

    /// <summary>명시적인 Stable ID 참조만 따라 재고-작업-NPC 관계를 계산합니다.</summary>
    public sealed class WarehouseRelationResolver
    {
        private readonly WarehouseWorldGraphBuilder graphBuilder;

        public WarehouseRelationResolver()
            : this(new WarehouseWorldGraphBuilder())
        {
        }

        public WarehouseRelationResolver(WarehouseWorldGraphBuilder graphBuilder)
            => this.graphBuilder = graphBuilder ?? throw new ArgumentNullException(nameof(graphBuilder));

        public WarehouseWorldSelection Select(WarehouseWorldSnapshot snapshot, string stableId)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var objects = snapshot.Objects ?? throw new InvalidOperationException("WarehouseWorldObjectsMissing");
            var graph = graphBuilder.Build(snapshot);
            var selectedId = new WorldStableId(stableId);
            if (!graph.NodesById.TryGetValue(selectedId, out var selectedNode))
                throw new InvalidOperationException("WarehouseWorldSelectionUnknown:" + stableId);

            var relatedIds = ConnectedComponent(graph, selectedId);

            return new WarehouseWorldSelection
            {
                Selected = selectedNode.Value,
                Related = objects.Where(item => relatedIds.Contains(item.StableId)).ToArray(),
            };
        }

        private static HashSet<string> ConnectedComponent(
            WorldGraphIndex<WarehouseWorldGraphNode> graph,
            WorldStableId selected)
        {
            var visited = new HashSet<WorldStableId> { selected };
            var pending = new Queue<WorldStableId>();
            pending.Enqueue(selected);
            while (pending.Count > 0)
            {
                var current = pending.Dequeue();
                foreach (var relation in graph.GetOutgoing(current))
                    Visit(relation.To, visited, pending);
                foreach (var relation in graph.GetIncoming(current))
                    Visit(relation.From, visited, pending);
            }

            visited.Remove(selected);
            return new HashSet<string>(visited.Select(value => value.Value), StringComparer.Ordinal);
        }

        private static void Visit(
            WorldStableId value,
            ISet<WorldStableId> visited,
            Queue<WorldStableId> pending)
        {
            if (visited.Add(value)) pending.Enqueue(value);
        }
    }

    /// <summary>기존 W1 소비 코드와의 호환 facade입니다.</summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "Unity WI 표현과 공간 능력·연결 책임을 결속한다.",
        Boundary = "Unity 배선이 H 또는 E 증거를 자동 승격하지 않는다.")]
    public sealed class WarehouseWorldSelectionService
    {
        private readonly WarehouseRelationResolver resolver;

        public WarehouseWorldSelectionService()
            : this(new WarehouseRelationResolver())
        {
        }

        public WarehouseWorldSelectionService(WarehouseRelationResolver resolver)
            => this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

        public WarehouseWorldSelection Select(WarehouseWorldSnapshot snapshot, string stableId)
            => resolver.Select(snapshot, stableId);
    }
}
