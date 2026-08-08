using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Warehouse
{
    public static class WarehouseWorldApiRoutes
    {
        public const string AuthorizedSnapshot = "api/v1/warehouse-operations/world/zones/warehouse";
    }

    public static class WarehouseWorldLoadStateCodes
    {
        public const string Idle = "Idle";
        public const string Loading = "Loading";
        public const string Success = "Success";
        public const string InitialLoadError = "InitialLoadError";
        public const string Refreshing = "Refreshing";
        public const string RefreshError = "RefreshError";
    }

    public sealed class WarehouseWorldInventoryItemApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string WarehouseStableId { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string OptionName { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public string StorageLocation { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool HasCommunityLedger { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
    }

    public sealed class WarehouseWorldTaskApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string WarehouseStableId { get; set; } = string.Empty;
        public string InventoryItemStableId { get; set; } = string.Empty;
        public string TaskKind { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool CanExecute { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
    }

    public sealed class WarehouseWorldNpcApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string WarehouseStableId { get; set; } = string.Empty;
        public string SourceTaskStableId { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;
        public string RouteCode { get; set; } = string.Empty;
        public string CurrentWaypointKey { get; set; } = string.Empty;
        public string DestinationWaypointKey { get; set; } = string.Empty;
        public string ActivityCode { get; set; } = string.Empty;
    }

    public sealed class WarehouseWorldSnapshotApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public int TotalAvailableQuantity { get; set; }
        public int TotalReservedQuantity { get; set; }
        public int UnassignedLocationCount { get; set; }
        public WarehouseWorldInventoryItemApiModel[] InventoryItems { get; set; } = Array.Empty<WarehouseWorldInventoryItemApiModel>();
        public WarehouseWorldTaskApiModel[] Tasks { get; set; } = Array.Empty<WarehouseWorldTaskApiModel>();
        public WarehouseWorldNpcApiModel[] Npcs { get; set; } = Array.Empty<WarehouseWorldNpcApiModel>();
    }

    public sealed class WarehouseWorldObject
    {
        public string StableId { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string WarehouseStableId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string CurrentLocationCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public bool CanExecute { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }
    }

    public sealed class WarehouseWorldSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public int TotalAvailableQuantity { get; set; }
        public int TotalReservedQuantity { get; set; }
        public int UnassignedLocationCount { get; set; }
        public WarehouseWorldObject[] Objects { get; set; } = Array.Empty<WarehouseWorldObject>();
    }

    public sealed class WarehouseWorldMapper
    {
        public WarehouseWorldSnapshot Map(WarehouseWorldSnapshotApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            RequireStableId(source.StableId);
            Require(source.Revision, "WarehouseWorldRevisionMissing");
            if (source.GeneratedAtUtc == default) throw new InvalidOperationException("WarehouseWorldGeneratedAtMissing");
            if (source.InventoryItems == null || source.Tasks == null || source.Npcs == null)
                throw new InvalidOperationException("WarehouseWorldCollectionsMissing");
            if (source.TotalAvailableQuantity < 0 || source.TotalReservedQuantity < 0 || source.UnassignedLocationCount < 0)
                throw new InvalidOperationException("WarehouseWorldTotalsInvalid");

            ValidateIds(source.InventoryItems.Select(item => item?.StableId), "WarehouseInventory");
            ValidateIds(source.Tasks.Select(item => item?.StableId), "WarehouseTask");
            ValidateIds(source.Npcs.Select(item => item?.StableId), "WarehouseNpc");
            var inventoryIds = new HashSet<string>(source.InventoryItems.Select(item => item.StableId), StringComparer.Ordinal);
            var taskIds = new HashSet<string>(source.Tasks.Select(item => item.StableId), StringComparer.Ordinal);

            foreach (var item in source.InventoryItems)
            {
                RequireStableId(item.WarehouseStableId); Require(item.ProductName, "WarehouseInventoryProductMissing:" + item.StableId);
                if (item.AvailableQuantity < 0 || item.ReservedQuantity < 0 || item.UpdatedAtUtc == default)
                    throw new InvalidOperationException("WarehouseInventoryValuesInvalid:" + item.StableId);
            }
            foreach (var task in source.Tasks)
            {
                RequireStableId(task.WarehouseStableId); Require(task.TaskKind, "WarehouseTaskKindMissing:" + task.StableId);
                if (task.TaskKind == "PutAway" && !inventoryIds.Contains(task.InventoryItemStableId))
                    throw new InvalidOperationException("WarehouseTaskInventoryUnknown:" + task.StableId);
                if (task.Quantity < 0 || task.UpdatedAtUtc == default)
                    throw new InvalidOperationException("WarehouseTaskValuesInvalid:" + task.StableId);
            }
            foreach (var npc in source.Npcs)
            {
                RequireStableId(npc.WarehouseStableId); Require(npc.RoleCode, "WarehouseNpcRoleMissing:" + npc.StableId);
                if (!taskIds.Contains(npc.SourceTaskStableId))
                    throw new InvalidOperationException("WarehouseNpcTaskUnknown:" + npc.StableId);
                Require(npc.CurrentWaypointKey, "WarehouseNpcCurrentWaypointMissing:" + npc.StableId);
                Require(npc.DestinationWaypointKey, "WarehouseNpcDestinationWaypointMissing:" + npc.StableId);
            }

            var objects = source.InventoryItems.Select(item => Object(item.StableId, "Inventory", item.WarehouseStableId, item.ProductName, item.Status, item.StorageLocation, string.Empty, item.AvailableQuantity, false, item.UpdatedAtUtc))
                .Concat(source.Tasks.Select(task => Object(task.StableId, "Task", task.WarehouseStableId, task.ProductName, task.Status, task.LocationCode, task.InventoryItemStableId, task.Quantity, task.CanExecute, task.UpdatedAtUtc)))
                .Concat(source.Npcs.Select(npc => Object(npc.StableId, "Npc", npc.WarehouseStableId, npc.RoleCode, npc.ActivityCode, npc.DestinationWaypointKey, npc.SourceTaskStableId, 0, false, null, npc.CurrentWaypointKey)))
                .ToArray();
            ValidateIds(objects.Select(item => item.StableId), "WarehouseWorldObject");
            return new WarehouseWorldSnapshot
            {
                StableId = source.StableId.Trim(), Revision = source.Revision.Trim(), GeneratedAtUtc = source.GeneratedAtUtc,
                TotalAvailableQuantity = source.TotalAvailableQuantity, TotalReservedQuantity = source.TotalReservedQuantity,
                UnassignedLocationCount = source.UnassignedLocationCount, Objects = objects,
            };
        }

        private static WarehouseWorldObject Object(string id, string kind, string warehouse, string title, string status, string location, string source, int quantity, bool canExecute, DateTimeOffset? updated, string currentLocation = "")
            => new WarehouseWorldObject { StableId = id, Kind = kind, WarehouseStableId = warehouse, Title = title, Status = status, LocationCode = location, CurrentLocationCode = currentLocation, SourceStableId = source, Quantity = quantity, CanExecute = canExecute, UpdatedAtUtc = updated };

        private static void ValidateIds(IEnumerable<string?> values, string kind)
        {
            var array = values.ToArray();
            if (array.Any(value => !StableDataId.IsValid(value))) throw new InvalidOperationException(kind + "StableIdInvalid");
            var duplicate = array.GroupBy(value => value!, StringComparer.Ordinal).FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null) throw new InvalidOperationException("Duplicate" + kind + ":" + duplicate.Key);
        }
        private static void RequireStableId(string value) { if (!StableDataId.IsValid(value)) throw new InvalidOperationException("WarehouseWorldStableIdInvalid:" + value); }
        private static void Require(string value, string error) { if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(error); }
    }

    public interface IWarehouseWorldApiClient
    {
        Task<WarehouseWorldSnapshotApiModel> GetAsync(long warehouseId, CancellationToken cancellationToken = default);
    }
    public interface IWarehouseWorldRepository
    {
        Task<WarehouseWorldSnapshot> 조회Async(long warehouseId, CancellationToken cancellationToken = default);
    }
    public sealed class WarehouseWorldApiRepository : IWarehouseWorldRepository
    {
        private readonly IWarehouseWorldApiClient client; private readonly WarehouseWorldMapper mapper;
        public WarehouseWorldApiRepository(IWarehouseWorldApiClient client, WarehouseWorldMapper mapper) { this.client = client; this.mapper = mapper; }
        public async Task<WarehouseWorldSnapshot> 조회Async(long warehouseId, CancellationToken cancellationToken = default)
        {
            if (warehouseId <= 0) throw new ArgumentOutOfRangeException(nameof(warehouseId));
            return mapper.Map(await client.GetAsync(warehouseId, cancellationToken).ConfigureAwait(false));
        }
    }
    public sealed class WarehouseWorldQueryUseCase
    {
        private readonly IWarehouseWorldRepository repository;
        public WarehouseWorldQueryUseCase(IWarehouseWorldRepository repository) => this.repository = repository;
        public Task<WarehouseWorldSnapshot> 실행Async(long warehouseId, CancellationToken cancellationToken = default) => repository.조회Async(warehouseId, cancellationToken);
    }

    public sealed class WarehouseWorldChangeSet
    {
        public WarehouseWorldObject[] Added { get; set; } = Array.Empty<WarehouseWorldObject>();
        public WarehouseWorldObject[] Updated { get; set; } = Array.Empty<WarehouseWorldObject>();
        public WarehouseWorldObject[] Removed { get; set; } = Array.Empty<WarehouseWorldObject>();
        public WarehouseWorldObject[] Unchanged { get; set; } = Array.Empty<WarehouseWorldObject>();
    }
    public sealed class WarehouseWorldReconciler
    {
        public WarehouseWorldChangeSet Reconcile(IReadOnlyList<WarehouseWorldObject> current, IReadOnlyList<WarehouseWorldObject> incoming)
        {
            var before = Index(current); var after = Index(incoming); var added = new List<WarehouseWorldObject>(); var updated = new List<WarehouseWorldObject>(); var unchanged = new List<WarehouseWorldObject>();
            foreach (var pair in after) { if (!before.TryGetValue(pair.Key, out var old)) added.Add(pair.Value); else if (Equivalent(old, pair.Value)) unchanged.Add(old); else updated.Add(pair.Value); }
            return new WarehouseWorldChangeSet { Added = added.ToArray(), Updated = updated.ToArray(), Unchanged = unchanged.ToArray(), Removed = before.Where(pair => !after.ContainsKey(pair.Key)).Select(pair => pair.Value).ToArray() };
        }
        private static Dictionary<string, WarehouseWorldObject> Index(IReadOnlyList<WarehouseWorldObject> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values)); var result = new Dictionary<string, WarehouseWorldObject>(StringComparer.Ordinal);
            foreach (var item in values) if (item == null || !result.TryAdd(item.StableId, item)) throw new InvalidOperationException("WarehouseWorldSnapshotInvalid"); return result;
        }
        private static bool Equivalent(WarehouseWorldObject a, WarehouseWorldObject b) => a.Kind == b.Kind && a.Title == b.Title && a.Status == b.Status && a.LocationCode == b.LocationCode && a.CurrentLocationCode == b.CurrentLocationCode && a.SourceStableId == b.SourceStableId && a.Quantity == b.Quantity && a.CanExecute == b.CanExecute && a.UpdatedAtUtc == b.UpdatedAtUtc;
    }

    public sealed class WarehouseWorldLoadResult
    {
        public string StateCode { get; set; } = WarehouseWorldLoadStateCodes.Idle;
        public WarehouseWorldSnapshot? Snapshot { get; set; }
        public WarehouseWorldChangeSet? Changes { get; set; }
        public Exception? Error { get; set; }
    }
    public sealed class WarehouseWorldLoadCoordinator
    {
        private readonly WarehouseWorldQueryUseCase query; private readonly WarehouseWorldReconciler reconciler; private WarehouseWorldSnapshot? lastSuccessful;
        public WarehouseWorldLoadCoordinator(WarehouseWorldQueryUseCase query, WarehouseWorldReconciler reconciler) { this.query = query; this.reconciler = reconciler; }
        public async Task<WarehouseWorldLoadResult> LoadAsync(long warehouseId, CancellationToken cancellationToken = default)
        {
            var refreshing = lastSuccessful != null;
            try
            {
                var snapshot = await query.실행Async(warehouseId, cancellationToken).ConfigureAwait(false);
                var changes = reconciler.Reconcile(lastSuccessful?.Objects ?? Array.Empty<WarehouseWorldObject>(), snapshot.Objects); lastSuccessful = snapshot;
                return new WarehouseWorldLoadResult { StateCode = WarehouseWorldLoadStateCodes.Success, Snapshot = snapshot, Changes = changes };
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception error) { return new WarehouseWorldLoadResult { StateCode = refreshing ? WarehouseWorldLoadStateCodes.RefreshError : WarehouseWorldLoadStateCodes.InitialLoadError, Snapshot = lastSuccessful, Error = error }; }
        }
    }
}
