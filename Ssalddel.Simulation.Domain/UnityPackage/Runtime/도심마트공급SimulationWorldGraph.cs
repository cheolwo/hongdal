using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public enum 도심마트공급WorldNodeKind
    {
        Product,
        Supplier,
        Offer,
        ContractDraft,
    }

    public enum 도심마트공급WorldRelationKind
    {
        Provides,
        Targets,
        ProvidedBy,
        Covers,
        DerivedFrom,
    }

    public sealed class 도심마트공급WorldNode
    {
        public 도심마트공급WorldNode(string stableId, 도심마트공급WorldNodeKind kind, string sourceStableId)
        {
            EnsureStableId(stableId, "SupplyWorldNodeStableIdInvalid");
            EnsureStableId(sourceStableId, "SupplyWorldNodeSourceStableIdInvalid");
            StableId = stableId;
            Kind = kind;
            SourceStableId = sourceStableId;
        }

        public string StableId { get; }
        public 도심마트공급WorldNodeKind Kind { get; }
        public string SourceStableId { get; }

        private static void EnsureStableId(string value, string errorCode)
        {
            if (string.IsNullOrWhiteSpace(value)
                || value.Length > 160
                || value.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
                throw new SimulationContractException(errorCode);
        }
    }

    public sealed class 도심마트공급WorldRelation : IEquatable<도심마트공급WorldRelation>
    {
        public 도심마트공급WorldRelation(
            string fromStableId,
            string toStableId,
            도심마트공급WorldRelationKind kind)
        {
            if (string.Equals(fromStableId, toStableId, StringComparison.Ordinal))
                throw new SimulationContractException("SupplyWorldRelationSelfReference");
            FromStableId = fromStableId;
            ToStableId = toStableId;
            Kind = kind;
        }

        public string FromStableId { get; }
        public string ToStableId { get; }
        public 도심마트공급WorldRelationKind Kind { get; }

        public bool Equals(도심마트공급WorldRelation? other)
            => other != null
                && string.Equals(FromStableId, other.FromStableId, StringComparison.Ordinal)
                && string.Equals(ToStableId, other.ToStableId, StringComparison.Ordinal)
                && Kind == other.Kind;

        public override bool Equals(object? obj) => Equals(obj as 도심마트공급WorldRelation);

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

    public sealed class 도심마트공급SimulationWorldGraph
    {
        private readonly Dictionary<string, 도심마트공급WorldRelation[]> outgoing;
        private readonly Dictionary<string, 도심마트공급WorldRelation[]> incoming;

        public 도심마트공급SimulationWorldGraph(
            IEnumerable<도심마트공급WorldNode> nodes,
            IEnumerable<도심마트공급WorldRelation> relations)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (relations == null) throw new ArgumentNullException(nameof(relations));

            var nodeById = new Dictionary<string, 도심마트공급WorldNode>(StringComparer.Ordinal);
            foreach (var node in nodes)
            {
                if (node == null) throw new SimulationContractException("SupplyWorldNodeMissing");
                if (!nodeById.TryAdd(node.StableId, node))
                    throw new SimulationContractException("SupplyWorldNodeDuplicate");
            }

            var relationArray = relations
                .Select(value => value ?? throw new SimulationContractException("SupplyWorldRelationMissing"))
                .OrderBy(value => value.FromStableId, StringComparer.Ordinal)
                .ThenBy(value => value.ToStableId, StringComparer.Ordinal)
                .ThenBy(value => value.Kind)
                .ToArray();
            if (relationArray.GroupBy(value => value).Any(group => group.Count() > 1))
                throw new SimulationContractException("SupplyWorldRelationDuplicate");

            foreach (var relation in relationArray)
            {
                if (!nodeById.TryGetValue(relation.FromStableId, out var from))
                    throw new SimulationContractException("SupplyWorldRelationSourceUnknown");
                if (!nodeById.TryGetValue(relation.ToStableId, out var to))
                    throw new SimulationContractException("SupplyWorldRelationTargetUnknown");
                ValidateRelationKinds(from.Kind, to.Kind, relation.Kind);
            }

            NodesById = nodeById;
            Relations = relationArray;
            outgoing = relationArray.GroupBy(value => value.FromStableId)
                .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
            incoming = relationArray.GroupBy(value => value.ToStableId)
                .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);
        }

        public IReadOnlyDictionary<string, 도심마트공급WorldNode> NodesById { get; }
        public 도심마트공급WorldRelation[] Relations { get; }

        public 도심마트공급WorldRelation[] GetOutgoing(string stableId)
            => outgoing.TryGetValue(stableId, out var values)
                ? values
                : Array.Empty<도심마트공급WorldRelation>();

        public 도심마트공급WorldRelation[] GetIncoming(string stableId)
            => incoming.TryGetValue(stableId, out var values)
                ? values
                : Array.Empty<도심마트공급WorldRelation>();

        private static void ValidateRelationKinds(
            도심마트공급WorldNodeKind from,
            도심마트공급WorldNodeKind to,
            도심마트공급WorldRelationKind relation)
        {
            var valid = relation switch
            {
                도심마트공급WorldRelationKind.Provides
                    => from == 도심마트공급WorldNodeKind.Supplier && to == 도심마트공급WorldNodeKind.Offer,
                도심마트공급WorldRelationKind.Targets
                    => from == 도심마트공급WorldNodeKind.Offer && to == 도심마트공급WorldNodeKind.Product,
                도심마트공급WorldRelationKind.ProvidedBy
                    => from == 도심마트공급WorldNodeKind.ContractDraft && to == 도심마트공급WorldNodeKind.Supplier,
                도심마트공급WorldRelationKind.Covers
                    => from == 도심마트공급WorldNodeKind.ContractDraft && to == 도심마트공급WorldNodeKind.Product,
                도심마트공급WorldRelationKind.DerivedFrom
                    => from == 도심마트공급WorldNodeKind.ContractDraft && to == 도심마트공급WorldNodeKind.Offer,
                _ => false,
            };
            if (!valid) throw new SimulationContractException("SupplyWorldRelationSemanticMismatch");
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
        "공간 WI의 실행 문맥·세계 발현 판정에 사용할 공간 조립 증거를 제공한다.",
        Boundary = "AreaSet·Graph·배치·통행은 조건부 입력이며 그 자체로 E4·E5를 완료하지 않는다.")]
    public sealed class 도심마트공급SimulationWorldGraphBuilder
    {
        public 도심마트공급SimulationWorldGraph Build(도심마트공급경영SimulationDataSnapshot snapshot)
        {
            도심마트공급경영SimulationDataValidator.Validate(snapshot);
            var nodes = new List<도심마트공급WorldNode>();
            var relations = new List<도심마트공급WorldRelation>();

            foreach (var productStableId in snapshot.Offers.Select(value => value.ProductStableId)
                .Concat(snapshot.ContractDrafts.Select(value => value.ProductStableId))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal))
            {
                nodes.Add(Node(productStableId, 도심마트공급WorldNodeKind.Product, productStableId));
            }

            foreach (var supplier in snapshot.Suppliers.OrderBy(value => value.SupplierStableId, StringComparer.Ordinal))
                nodes.Add(Node(supplier.SupplierStableId, 도심마트공급WorldNodeKind.Supplier, supplier.SupplierStableId));

            foreach (var offer in snapshot.Offers.OrderBy(value => value.OfferStableId, StringComparer.Ordinal))
            {
                nodes.Add(Node(offer.OfferStableId, 도심마트공급WorldNodeKind.Offer, offer.OfferStableId));
                relations.Add(Relation(offer.SupplierStableId, offer.OfferStableId, 도심마트공급WorldRelationKind.Provides));
                relations.Add(Relation(offer.OfferStableId, offer.ProductStableId, 도심마트공급WorldRelationKind.Targets));
            }

            foreach (var draft in snapshot.ContractDrafts.OrderBy(value => value.ContractDraftStableId, StringComparer.Ordinal))
            {
                nodes.Add(Node(draft.ContractDraftStableId, 도심마트공급WorldNodeKind.ContractDraft, draft.ContractDraftStableId));
                relations.Add(Relation(draft.ContractDraftStableId, draft.SupplierStableId, 도심마트공급WorldRelationKind.ProvidedBy));
                relations.Add(Relation(draft.ContractDraftStableId, draft.ProductStableId, 도심마트공급WorldRelationKind.Covers));
                relations.Add(Relation(draft.ContractDraftStableId, draft.SourceOfferStableId, 도심마트공급WorldRelationKind.DerivedFrom));
            }

            return new 도심마트공급SimulationWorldGraph(nodes, relations);
        }

        private static 도심마트공급WorldNode Node(
            string stableId,
            도심마트공급WorldNodeKind kind,
            string sourceStableId)
            => new 도심마트공급WorldNode(stableId, kind, sourceStableId);

        private static 도심마트공급WorldRelation Relation(
            string fromStableId,
            string toStableId,
            도심마트공급WorldRelationKind kind)
            => new 도심마트공급WorldRelation(fromStableId, toStableId, kind);
    }
}
