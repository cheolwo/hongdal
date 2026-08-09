using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Unity.Warehouse
{
    public sealed class WarehouseWorldGraphNode : IWorldNode
    {
        public WarehouseWorldGraphNode(WarehouseWorldObject value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            StableId = new WorldStableId(value.StableId);
        }

        public WorldStableId StableId { get; }
        public WarehouseWorldObject Value { get; }
    }

    /// <summary>
    /// 기존 WarehouseWorldObject의 명시적 참조를 typed World relation으로 변환하는
    /// compatibility adapter입니다. 새로운 해석 코드는 이 graph를 직접 생성해야 합니다.
    /// </summary>
    public sealed class WarehouseWorldGraphBuilder
    {
        public WorldGraphIndex<WarehouseWorldGraphNode> Build(WarehouseWorldSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var objects = snapshot.Objects ?? throw new InvalidOperationException("WarehouseWorldObjectsMissing");
            var nodes = objects.Select(value => new WarehouseWorldGraphNode(value)).ToArray();
            var byId = nodes.ToDictionary(value => value.Value.StableId, StringComparer.Ordinal);
            var relations = new HashSet<WorldRelation>();

            foreach (var node in nodes)
            {
                var item = node.Value;
                if (item.Kind == "Task" && TryGet(byId, item.SourceStableId, out var inventory))
                    relations.Add(Relation(node, inventory, WorldRelationKind.Targets));
                if (item.Kind == "Npc" && TryGet(byId, item.SourceStableId, out var task))
                    relations.Add(Relation(task, node, WorldRelationKind.AssignedTo));
            }

            foreach (var group in nodes
                .Where(value => !string.IsNullOrWhiteSpace(value.Value.CanonicalRelationStableId))
                .GroupBy(value => value.Value.CanonicalRelationStableId, StringComparer.Ordinal))
            {
                var inboundTask = group.FirstOrDefault(value => value.Value.Kind == "InboundTask");
                var handoff = group.FirstOrDefault(value => value.Value.Kind == "Handoff");
                var cargo = group.FirstOrDefault(value => value.Value.Kind == "Cargo");
                var vehicle = group.FirstOrDefault(value => value.Value.Kind == "Vehicle");

                Add(relations, handoff, inboundTask, WorldRelationKind.HandoffTo);
                Add(relations, handoff, cargo, WorldRelationKind.Targets);
                Add(relations, inboundTask, cargo, WorldRelationKind.Targets);
                Add(relations, vehicle, cargo, WorldRelationKind.Carries);

                foreach (var task in group.Where(value => value.Value.Kind == "Task"))
                    Add(relations, task, inboundTask, WorldRelationKind.DerivedFrom);
                foreach (var npc in group.Where(value => value.Value.Kind == "Npc"))
                    Add(relations, inboundTask, npc, WorldRelationKind.AssignedTo);
            }

            return new WorldGraphIndex<WarehouseWorldGraphNode>(nodes, relations);
        }

        private static bool TryGet(
            IReadOnlyDictionary<string, WarehouseWorldGraphNode> values,
            string stableId,
            out WarehouseWorldGraphNode value)
        {
            if (!string.IsNullOrWhiteSpace(stableId) && values.TryGetValue(stableId, out var found))
            {
                value = found;
                return true;
            }

            value = null!;
            return false;
        }

        private static void Add(
            ISet<WorldRelation> relations,
            WarehouseWorldGraphNode? from,
            WarehouseWorldGraphNode? to,
            WorldRelationKind kind)
        {
            if (from != null && to != null && from.StableId != to.StableId)
                relations.Add(Relation(from, to, kind));
        }

        private static WorldRelation Relation(
            WarehouseWorldGraphNode from,
            WarehouseWorldGraphNode to,
            WorldRelationKind kind)
            => new WorldRelation(from.StableId, to.StableId, kind);
    }
}
