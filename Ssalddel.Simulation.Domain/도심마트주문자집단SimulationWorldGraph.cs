using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public enum 도심마트주문자집단WorldNodeKind
    {
        OrdererGroup,
        DemandRequest,
        Product,
        PickupPoint,
    }

    public enum 도심마트주문자집단WorldRelationKind
    {
        RaisesDemand,
        RequestsProduct,
        RequestsFulfillmentAt,
    }

    public sealed class 도심마트주문자집단WorldNode
    {
        public 도심마트주문자집단WorldNode(
            string stableId,
            도심마트주문자집단WorldNodeKind kind,
            string sourceStableId)
        {
            EnsureStableId(stableId, "OrdererGroupWorldNodeStableIdInvalid");
            EnsureStableId(sourceStableId, "OrdererGroupWorldNodeSourceStableIdInvalid");
            StableId = stableId;
            Kind = kind;
            SourceStableId = sourceStableId;
        }

        public string StableId { get; }
        public 도심마트주문자집단WorldNodeKind Kind { get; }
        public string SourceStableId { get; }

        private static void EnsureStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new SimulationContractException(errorCode);
        }
    }

    public sealed class 도심마트주문자집단WorldRelation :
        IEquatable<도심마트주문자집단WorldRelation>
    {
        public 도심마트주문자집단WorldRelation(
            string fromStableId,
            string toStableId,
            도심마트주문자집단WorldRelationKind kind)
        {
            if (string.Equals(fromStableId, toStableId, StringComparison.Ordinal))
                throw new SimulationContractException("OrdererGroupWorldRelationSelfReference");
            FromStableId = fromStableId;
            ToStableId = toStableId;
            Kind = kind;
        }

        public string FromStableId { get; }
        public string ToStableId { get; }
        public 도심마트주문자집단WorldRelationKind Kind { get; }

        public bool Equals(도심마트주문자집단WorldRelation? other)
            => other != null
                && string.Equals(FromStableId, other.FromStableId, StringComparison.Ordinal)
                && string.Equals(ToStableId, other.ToStableId, StringComparison.Ordinal)
                && Kind == other.Kind;

        public override bool Equals(object? obj)
            => Equals(obj as 도심마트주문자집단WorldRelation);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.Ordinal.GetHashCode(FromStableId ?? string.Empty);
                hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(ToStableId ?? string.Empty);
                return (hash * 397) ^ Kind.GetHashCode();
            }
        }
    }

    public sealed class 도심마트주문자집단SimulationWorldGraph
    {
        private readonly Dictionary<string, 도심마트주문자집단WorldRelation[]> outgoing;
        private readonly Dictionary<string, 도심마트주문자집단WorldRelation[]> incoming;

        public 도심마트주문자집단SimulationWorldGraph(
            IEnumerable<도심마트주문자집단WorldNode> nodes,
            IEnumerable<도심마트주문자집단WorldRelation> relations)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (relations == null) throw new ArgumentNullException(nameof(relations));

            var nodeById = new Dictionary<string, 도심마트주문자집단WorldNode>(StringComparer.Ordinal);
            foreach (var node in nodes
                .Select(value => value
                    ?? throw new SimulationContractException("OrdererGroupWorldNodeMissing"))
                .OrderBy(value => value.StableId, StringComparer.Ordinal))
            {
                if (!nodeById.TryAdd(node.StableId, node))
                    throw new SimulationContractException("OrdererGroupWorldNodeDuplicate");
            }

            var relationArray = relations
                .Select(value => value
                    ?? throw new SimulationContractException("OrdererGroupWorldRelationMissing"))
                .OrderBy(value => value.FromStableId, StringComparer.Ordinal)
                .ThenBy(value => value.ToStableId, StringComparer.Ordinal)
                .ThenBy(value => value.Kind)
                .ToArray();
            if (relationArray.GroupBy(value => value).Any(group => group.Count() > 1))
                throw new SimulationContractException("OrdererGroupWorldRelationDuplicate");

            foreach (var relation in relationArray)
            {
                if (!nodeById.TryGetValue(relation.FromStableId, out var from))
                    throw new SimulationContractException("OrdererGroupWorldRelationSourceUnknown");
                if (!nodeById.TryGetValue(relation.ToStableId, out var to))
                    throw new SimulationContractException("OrdererGroupWorldRelationTargetUnknown");
                ValidateRelationKinds(from.Kind, to.Kind, relation.Kind);
            }

            NodesById = nodeById;
            Relations = relationArray;
            outgoing = relationArray.GroupBy(value => value.FromStableId)
                .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
            incoming = relationArray.GroupBy(value => value.ToStableId)
                .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        }

        public IReadOnlyDictionary<string, 도심마트주문자집단WorldNode> NodesById { get; }
        public 도심마트주문자집단WorldRelation[] Relations { get; }

        public 도심마트주문자집단WorldRelation[] GetOutgoing(string stableId)
            => outgoing.TryGetValue(stableId, out var values)
                ? values
                : Array.Empty<도심마트주문자집단WorldRelation>();

        public 도심마트주문자집단WorldRelation[] GetIncoming(string stableId)
            => incoming.TryGetValue(stableId, out var values)
                ? values
                : Array.Empty<도심마트주문자집단WorldRelation>();

        private static void ValidateRelationKinds(
            도심마트주문자집단WorldNodeKind from,
            도심마트주문자집단WorldNodeKind to,
            도심마트주문자집단WorldRelationKind relation)
        {
            var valid = relation switch
            {
                도심마트주문자집단WorldRelationKind.RaisesDemand
                    => from == 도심마트주문자집단WorldNodeKind.OrdererGroup
                        && to == 도심마트주문자집단WorldNodeKind.DemandRequest,
                도심마트주문자집단WorldRelationKind.RequestsProduct
                    => from == 도심마트주문자집단WorldNodeKind.DemandRequest
                        && to == 도심마트주문자집단WorldNodeKind.Product,
                도심마트주문자집단WorldRelationKind.RequestsFulfillmentAt
                    => from == 도심마트주문자집단WorldNodeKind.DemandRequest
                        && to == 도심마트주문자집단WorldNodeKind.PickupPoint,
                _ => false,
            };
            if (!valid)
                throw new SimulationContractException("OrdererGroupWorldRelationSemanticMismatch");
        }
    }

    public sealed class 도심마트주문자집단SimulationWorldGraphBuilder
    {
        public 도심마트주문자집단SimulationWorldGraph Build(
            도심마트주문자집단수요SimulationDataSnapshot snapshot)
        {
            도심마트공급경영SimulationDataValidator.Validate(snapshot);
            var nodes = new List<도심마트주문자집단WorldNode>();
            var relations = new List<도심마트주문자집단WorldRelation>();

            foreach (var productStableId in snapshot.Groups
                .Select(value => value.ProductStableId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal))
                nodes.Add(Node(productStableId, 도심마트주문자집단WorldNodeKind.Product));

            foreach (var pickupStableId in snapshot.Groups
                .Select(value => value.RequestedPickupPointStableId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal))
                nodes.Add(Node(pickupStableId, 도심마트주문자집단WorldNodeKind.PickupPoint));

            foreach (var group in snapshot.Groups
                .OrderBy(value => value.OrdererGroupStableId, StringComparer.Ordinal))
            {
                nodes.Add(Node(group.OrdererGroupStableId, 도심마트주문자집단WorldNodeKind.OrdererGroup));
                nodes.Add(Node(group.DemandRequestStableId, 도심마트주문자집단WorldNodeKind.DemandRequest));
                relations.Add(Relation(group.OrdererGroupStableId, group.DemandRequestStableId,
                    도심마트주문자집단WorldRelationKind.RaisesDemand));
                relations.Add(Relation(group.DemandRequestStableId, group.ProductStableId,
                    도심마트주문자집단WorldRelationKind.RequestsProduct));
                relations.Add(Relation(group.DemandRequestStableId, group.RequestedPickupPointStableId,
                    도심마트주문자집단WorldRelationKind.RequestsFulfillmentAt));
            }

            return new 도심마트주문자집단SimulationWorldGraph(nodes, relations);
        }

        private static 도심마트주문자집단WorldNode Node(
            string stableId,
            도심마트주문자집단WorldNodeKind kind)
            => new 도심마트주문자집단WorldNode(stableId, kind, stableId);

        private static 도심마트주문자집단WorldRelation Relation(
            string fromStableId,
            string toStableId,
            도심마트주문자집단WorldRelationKind kind)
            => new 도심마트주문자집단WorldRelation(fromStableId, toStableId, kind);
    }
}
