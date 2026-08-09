using System;
using System.Collections.Generic;
using System.Linq;

namespace Ssalddel.Unity.InterpretationContracts
{
    public enum WorldRelationKind
    {
        Contains,
        AssignedTo,
        Carries,
        LocatedAt,
        Targets,
        HandoffTo,
        DerivedFrom,
        HasActivity,
        ContributesTo,
    }

    public interface IWorldNode
    {
        WorldStableId StableId { get; }
    }

    public sealed class WorldRelation : IEquatable<WorldRelation>
    {
        public WorldRelation(
            WorldStableId from,
            WorldStableId to,
            WorldRelationKind kind)
        {
            if (!from.IsDefined) throw new ArgumentException("WorldRelationSourceMissing", nameof(from));
            if (!to.IsDefined) throw new ArgumentException("WorldRelationTargetMissing", nameof(to));
            if (from == to) throw new ArgumentException("WorldRelationSelfReference");
            From = from;
            To = to;
            Kind = kind;
        }

        public WorldStableId From { get; }
        public WorldStableId To { get; }
        public WorldRelationKind Kind { get; }

        public bool Equals(WorldRelation? other)
            => other != null && From == other.From && To == other.To && Kind == other.Kind;

        public override bool Equals(object? obj) => Equals(obj as WorldRelation);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = From.GetHashCode();
                hash = (hash * 397) ^ To.GetHashCode();
                return (hash * 397) ^ Kind.GetHashCode();
            }
        }
    }

    public sealed class WorldGraphContractException : InvalidOperationException
    {
        public WorldGraphContractException(string errorCode, string stableId = "")
            : base(string.IsNullOrWhiteSpace(stableId) ? errorCode : errorCode + ":" + stableId)
        {
            ErrorCode = errorCode;
            StableId = stableId;
        }

        public string ErrorCode { get; }
        public string StableId { get; }
    }

    /// <summary>
    /// Interpretation 단계에서 한 번 생성하는 engine-independent 관계 인덱스입니다.
    /// node와 relation의 무결성을 검증하고 정방향·역방향 탐색을 제공합니다.
    /// </summary>
    public sealed class WorldGraphIndex<TNode> where TNode : IWorldNode
    {
        private readonly Dictionary<WorldStableId, WorldRelation[]> outgoing;
        private readonly Dictionary<WorldStableId, WorldRelation[]> incoming;

        public WorldGraphIndex(
            IEnumerable<TNode> nodes,
            IEnumerable<WorldRelation> relations)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (relations == null) throw new ArgumentNullException(nameof(relations));

            var nodeIndex = new Dictionary<WorldStableId, TNode>();
            foreach (var node in nodes)
            {
                if (node == null) throw new WorldGraphContractException("WorldNodeMissing");
                if (!node.StableId.IsDefined)
                    throw new WorldGraphContractException("WorldNodeStableIdMissing");
                if (!nodeIndex.TryAdd(node.StableId, node))
                    throw new WorldGraphContractException("DuplicateWorldNode", node.StableId.Value);
            }

            var relationArray = relations
                .Select(value => value ?? throw new WorldGraphContractException("WorldRelationMissing"))
                .OrderBy(value => value.From)
                .ThenBy(value => value.To)
                .ThenBy(value => value.Kind)
                .ToArray();

            var duplicate = relationArray.GroupBy(value => value).FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new WorldGraphContractException("DuplicateWorldRelation", duplicate.Key.From.Value);

            foreach (var relation in relationArray)
            {
                if (!nodeIndex.ContainsKey(relation.From))
                    throw new WorldGraphContractException("WorldRelationSourceUnknown", relation.From.Value);
                if (!nodeIndex.ContainsKey(relation.To))
                    throw new WorldGraphContractException("WorldRelationTargetUnknown", relation.To.Value);
            }

            NodesById = nodeIndex;
            Relations = relationArray;
            outgoing = relationArray.GroupBy(value => value.From)
                .ToDictionary(group => group.Key, group => group.ToArray());
            incoming = relationArray.GroupBy(value => value.To)
                .ToDictionary(group => group.Key, group => group.ToArray());
        }

        public IReadOnlyDictionary<WorldStableId, TNode> NodesById { get; }
        public WorldRelation[] Relations { get; }

        public WorldRelation[] GetOutgoing(WorldStableId stableId)
            => outgoing.TryGetValue(stableId, out var values)
                ? values
                : Array.Empty<WorldRelation>();

        public WorldRelation[] GetIncoming(WorldStableId stableId)
            => incoming.TryGetValue(stableId, out var values)
                ? values
                : Array.Empty<WorldRelation>();

        public TNode GetRequiredNode(WorldStableId stableId)
            => NodesById.TryGetValue(stableId, out var node)
                ? node
                : throw new WorldGraphContractException("WorldNodeUnknown", stableId.Value);
    }
}
