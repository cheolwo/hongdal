using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.PotatoJourney
{
    public static class PotatoProductionDistributionObjectKindCodes
    {
        public const string Product = "Product";
        public const string Cultivation = "Cultivation";
        public const string CargoVehicle = "CargoVehicle";
        public const string Warehouse = "Warehouse";
        public const string Market = "Market";
        public const string PublicMarketProduct = "PublicMarketProduct";
    }

    /// <summary>
    /// Unity Presentation catalog의 semantic key입니다. 서버 DTO와 Synty asset 경로에는 포함되지 않습니다.
    /// </summary>
    public static class PotatoProductionDistributionVisualKeys
    {
        public const string PotatoProduct = "farm.cargo.potato-box";
        public const string PotatoCultivation = "farm.crop.potato.large";
        public const string DeliveryVan = "urban.vehicle.van";
        public const string Warehouse = "urban.building.logistics";
        public const string Market = "urban.building.market";
        public const string MarketShelf = "urban.market.shelf";
    }

    public sealed class PotatoProductionDistributionMemoryNode
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string ObjectKindCode { get; set; } = string.Empty;
        public string ZoneCode { get; set; } = string.Empty;
        public string VisualKey { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string SourceModeCode { get; set; } = string.Empty;
    }

    public sealed class PotatoProductionDistributionMemoryRelation
    {
        public string FromStableId { get; set; } = string.Empty;
        public string RelationCode { get; set; } = string.Empty;
        public string ToStableId { get; set; } = string.Empty;
    }

    public sealed class PotatoProductionDistributionMemoryProjection
    {
        public string WorldStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAt { get; set; }
        public string SourceModeCode { get; set; } = string.Empty;
        public PotatoProductionDistributionMemoryNode[] Nodes { get; set; } =
            Array.Empty<PotatoProductionDistributionMemoryNode>();
        public PotatoProductionDistributionMemoryRelation[] Relations { get; set; } =
            Array.Empty<PotatoProductionDistributionMemoryRelation>();
    }

    public sealed class PotatoProductionDistributionMemoryChangeSet
    {
        public PotatoProductionDistributionMemoryNode[] Added { get; set; } =
            Array.Empty<PotatoProductionDistributionMemoryNode>();
        public PotatoProductionDistributionMemoryNode[] Updated { get; set; } =
            Array.Empty<PotatoProductionDistributionMemoryNode>();
        public PotatoProductionDistributionMemoryNode[] Removed { get; set; } =
            Array.Empty<PotatoProductionDistributionMemoryNode>();
        public PotatoProductionDistributionMemoryNode[] Unchanged { get; set; } =
            Array.Empty<PotatoProductionDistributionMemoryNode>();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PotatoProductionDistributionWorldMemoryProjector
    {
        public PotatoProductionDistributionMemoryProjection Project(PotatoJourneySnapshot source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            StableDataId.EnsureValid(source.StableId, nameof(source.StableId));
            if (string.IsNullOrWhiteSpace(source.Revision) || source.GeneratedAt == default)
                throw new InvalidOperationException("PotatoProductionDistributionMemorySnapshotInvalid");

            var nodes = new List<PotatoProductionDistributionMemoryNode>
            {
                Node(source.Product.ProductStableId, source.Revision,
                    PotatoProductionDistributionObjectKindCodes.Product, "farm",
                    PotatoProductionDistributionVisualKeys.PotatoProduct,
                    source.LinkageStatusCode, source.SourceModeCode),
            };
            var relations = new List<PotatoProductionDistributionMemoryRelation>();

            if (source.Farm != null)
            {
                nodes.Add(Node(source.Farm.CultivationStableId,
                    source.Farm.CultivationRevision.ToString(),
                    PotatoProductionDistributionObjectKindCodes.Cultivation, "farm",
                    PotatoProductionDistributionVisualKeys.PotatoCultivation,
                    source.Farm.GrowthStatusCode, source.SourceModeCode));
                relations.Add(Relation(source.Farm.CultivationStableId,
                    "Produces", source.Product.ProductStableId));
            }

            if (source.CargoJourney != null)
            {
                nodes.Add(Node(source.CargoJourney.CargoStableId, source.Revision,
                    PotatoProductionDistributionObjectKindCodes.CargoVehicle, "transport-network",
                    PotatoProductionDistributionVisualKeys.DeliveryVan,
                    source.CargoJourney.HandoffStateCode, source.SourceModeCode));
                relations.Add(Relation(source.Product.ProductStableId,
                    "TransportedAs", source.CargoJourney.CargoStableId));
            }

            if (source.Warehouse != null)
            {
                nodes.Add(Node(source.Warehouse.WarehouseStableId, source.Revision,
                    PotatoProductionDistributionObjectKindCodes.Warehouse, "warehouse",
                    PotatoProductionDistributionVisualKeys.Warehouse,
                    source.Warehouse.StatusCode, source.SourceModeCode));
                relations.Add(Relation(source.CargoJourney!.CargoStableId,
                    "DeliveredTo", source.Warehouse.WarehouseStableId));
            }

            if (source.Market != null)
            {
                nodes.Add(Node(source.Market.SourceStableId, source.Market.SourceRevision,
                    PotatoProductionDistributionObjectKindCodes.Market, "market-order",
                    PotatoProductionDistributionVisualKeys.Market,
                    source.Market.IsSaleAvailable ? "SaleAvailable" : "SaleUnavailable",
                    source.SourceModeCode));
                nodes.Add(Node(source.Market.PublicProductStableId, source.Market.SourceRevision,
                    PotatoProductionDistributionObjectKindCodes.PublicMarketProduct, "market-order",
                    PotatoProductionDistributionVisualKeys.MarketShelf,
                    source.Market.IsSaleAvailable ? "SaleAvailable" : "SaleUnavailable",
                    source.SourceModeCode));
                relations.Add(Relation(source.Market.SourceStableId,
                    "Publishes", source.Market.PublicProductStableId));
                relations.Add(Relation(source.Product.ProductStableId,
                    "PresentedAs", source.Market.PublicProductStableId));
            }

            ValidateNodes(nodes);
            ValidateRelations(nodes, relations);
            return new PotatoProductionDistributionMemoryProjection
            {
                WorldStableId = source.StableId,
                Revision = source.Revision,
                GeneratedAt = source.GeneratedAt,
                SourceModeCode = source.SourceModeCode,
                Nodes = nodes.OrderBy(value => value.StableId, StringComparer.Ordinal).ToArray(),
                Relations = relations.OrderBy(value => value.FromStableId, StringComparer.Ordinal)
                    .ThenBy(value => value.RelationCode, StringComparer.Ordinal).ToArray(),
            };
        }

        private static PotatoProductionDistributionMemoryNode Node(
            string stableId, string revision, string kind, string zone,
            string visualKey, string state, string sourceMode)
            => new PotatoProductionDistributionMemoryNode
            {
                StableId = stableId,
                Revision = revision,
                ObjectKindCode = kind,
                ZoneCode = zone,
                VisualKey = visualKey,
                StateCode = state,
                SourceModeCode = sourceMode,
            };

        private static PotatoProductionDistributionMemoryRelation Relation(
            string from, string code, string to)
            => new PotatoProductionDistributionMemoryRelation
            {
                FromStableId = from,
                RelationCode = code,
                ToStableId = to,
            };

        private static void ValidateNodes(IReadOnlyCollection<PotatoProductionDistributionMemoryNode> nodes)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var node in nodes)
            {
                StableDataId.EnsureValid(node.StableId, nameof(node.StableId));
                if (!ids.Add(node.StableId))
                    throw new InvalidOperationException("PotatoProductionDistributionDuplicateStableId:" + node.StableId);
                if (string.IsNullOrWhiteSpace(node.Revision)
                    || string.IsNullOrWhiteSpace(node.ObjectKindCode)
                    || string.IsNullOrWhiteSpace(node.ZoneCode)
                    || string.IsNullOrWhiteSpace(node.VisualKey)
                    || string.IsNullOrWhiteSpace(node.StateCode)
                    || string.IsNullOrWhiteSpace(node.SourceModeCode)
                    || node.VisualKey.Contains("Assets/", StringComparison.OrdinalIgnoreCase)
                    || node.VisualKey.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("PotatoProductionDistributionMemoryNodeInvalid:" + node.StableId);
            }
        }

        private static void ValidateRelations(
            IReadOnlyCollection<PotatoProductionDistributionMemoryNode> nodes,
            IEnumerable<PotatoProductionDistributionMemoryRelation> relations)
        {
            var ids = new HashSet<string>(nodes.Select(value => value.StableId), StringComparer.Ordinal);
            foreach (var relation in relations)
                if (!ids.Contains(relation.FromStableId) || !ids.Contains(relation.ToStableId)
                    || string.IsNullOrWhiteSpace(relation.RelationCode))
                    throw new InvalidOperationException("PotatoProductionDistributionMemoryRelationInvalid");
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PotatoProductionDistributionWorldMemoryStore
    {
        private readonly PotatoProductionDistributionWorldMemoryProjector projector;
        private Dictionary<string, PotatoProductionDistributionMemoryNode> nodes =
            new Dictionary<string, PotatoProductionDistributionMemoryNode>(StringComparer.Ordinal);

        public PotatoProductionDistributionWorldMemoryStore(
            PotatoProductionDistributionWorldMemoryProjector? memoryProjector = null)
            => projector = memoryProjector ?? new PotatoProductionDistributionWorldMemoryProjector();

        public string CacheBoundaryKey { get; private set; } = string.Empty;
        public PotatoProductionDistributionMemoryProjection? Current { get; private set; }
        public IReadOnlyCollection<PotatoProductionDistributionMemoryNode> Nodes => nodes.Values;

        public PotatoProductionDistributionMemoryChangeSet Load(
            PotatoJourneySnapshot snapshot,
            string cacheBoundaryKey)
        {
            if (string.IsNullOrWhiteSpace(cacheBoundaryKey))
                throw new ArgumentException("PotatoProductionDistributionCacheBoundaryMissing", nameof(cacheBoundaryKey));
            var incoming = projector.Project(snapshot);
            var normalizedBoundary = cacheBoundaryKey.Trim();
            var sameBoundary = string.Equals(CacheBoundaryKey, normalizedBoundary, StringComparison.Ordinal);
            if (sameBoundary && Current != null)
            {
                if (incoming.GeneratedAt < Current.GeneratedAt)
                    throw new InvalidOperationException("PotatoProductionDistributionLowerGeneratedAt");
                if (incoming.GeneratedAt == Current.GeneratedAt
                    && !string.Equals(incoming.Revision, Current.Revision, StringComparison.Ordinal))
                    throw new InvalidOperationException("PotatoProductionDistributionRevisionConflict");
            }

            var before = sameBoundary
                ? nodes
                : new Dictionary<string, PotatoProductionDistributionMemoryNode>(StringComparer.Ordinal);
            var projected = incoming.Nodes.ToDictionary(value => value.StableId, StringComparer.Ordinal);
            var committed = new Dictionary<string, PotatoProductionDistributionMemoryNode>(StringComparer.Ordinal);
            var added = new List<PotatoProductionDistributionMemoryNode>();
            var updated = new List<PotatoProductionDistributionMemoryNode>();
            var unchanged = new List<PotatoProductionDistributionMemoryNode>();
            foreach (var pair in projected.OrderBy(value => value.Key, StringComparer.Ordinal))
            {
                if (!before.TryGetValue(pair.Key, out var existing))
                {
                    added.Add(pair.Value);
                    committed.Add(pair.Key, pair.Value);
                }
                else if (Equivalent(existing, pair.Value))
                {
                    unchanged.Add(existing);
                    committed.Add(pair.Key, existing);
                }
                else
                {
                    updated.Add(pair.Value);
                    committed.Add(pair.Key, pair.Value);
                }
            }

            var removed = before.Where(pair => !projected.ContainsKey(pair.Key))
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => pair.Value).ToArray();

            nodes = committed;
            CacheBoundaryKey = normalizedBoundary;
            Current = incoming;
            return new PotatoProductionDistributionMemoryChangeSet
            {
                Added = added.ToArray(),
                Updated = updated.ToArray(),
                Removed = removed,
                Unchanged = unchanged.ToArray(),
            };
        }

        public bool TryGet(string stableId, out PotatoProductionDistributionMemoryNode? node)
            => nodes.TryGetValue(stableId, out node);

        public void Clear()
        {
            nodes.Clear();
            Current = null;
            CacheBoundaryKey = string.Empty;
        }

        private static bool Equivalent(
            PotatoProductionDistributionMemoryNode current,
            PotatoProductionDistributionMemoryNode incoming)
            => string.Equals(current.Revision, incoming.Revision, StringComparison.Ordinal)
               && string.Equals(current.ObjectKindCode, incoming.ObjectKindCode, StringComparison.Ordinal)
               && string.Equals(current.ZoneCode, incoming.ZoneCode, StringComparison.Ordinal)
               && string.Equals(current.VisualKey, incoming.VisualKey, StringComparison.Ordinal)
               && string.Equals(current.StateCode, incoming.StateCode, StringComparison.Ordinal)
               && string.Equals(current.SourceModeCode, incoming.SourceModeCode, StringComparison.Ordinal);
    }

    public sealed class PotatoProductionDistributionBootstrapResult
    {
        public PotatoJourneySnapshot Snapshot { get; set; } = null!;
        public PotatoProductionDistributionMemoryChangeSet Changes { get; set; } = null!;
    }

    public sealed class PotatoProductionDistributionWorldBootstrapLoader
    {
        private readonly PotatoJourneyQueryUseCase query;
        private readonly PotatoProductionDistributionWorldMemoryStore store;

        public PotatoProductionDistributionWorldBootstrapLoader(
            PotatoJourneyQueryUseCase queryUseCase,
            PotatoProductionDistributionWorldMemoryStore memoryStore)
        {
            query = queryUseCase ?? throw new ArgumentNullException(nameof(queryUseCase));
            store = memoryStore ?? throw new ArgumentNullException(nameof(memoryStore));
        }

        public async Task<PotatoProductionDistributionBootstrapResult> LoadAsync(
            string? cultivationStableId,
            string cacheBoundaryKey,
            CancellationToken cancellationToken = default)
        {
            var snapshot = await query.ExecuteAsync(cultivationStableId, cancellationToken)
                .ConfigureAwait(false);
            var changes = store.Load(snapshot, cacheBoundaryKey);
            return new PotatoProductionDistributionBootstrapResult
            {
                Snapshot = snapshot,
                Changes = changes,
            };
        }
    }
}
