using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Warehouse;
using UnityEngine;
using UnityEngine.Networking;

namespace Ssalddel.Unity.Samples.WarehouseWorld
{
    public sealed class WarehouseWorldApiOptions { public string BaseUrl { get; set; } = string.Empty; public int TimeoutSeconds { get; set; } = 15; }
    public sealed class WarehouseRuntimeSessionTokenProvider : MonoBehaviour
    {
        private static string accessToken = string.Empty;
        public static void SetAccessToken(string token) => accessToken = token?.Trim() ?? string.Empty;
        public string GetRequiredToken() => string.IsNullOrWhiteSpace(accessToken) ? throw new InvalidOperationException("WarehouseWorldAccessTokenMissing") : accessToken;
        private void OnDestroy() => accessToken = string.Empty;
    }

    public sealed class SimulatedWarehouseWorldApiClient : IWarehouseWorldApiClient
    {
        public Task<WarehouseWorldSnapshotApiModel> GetAsync(long warehouseId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested(); var time = DateTimeOffset.Parse("2026-08-08T15:00:00+09:00");
            return Task.FromResult(new WarehouseWorldSnapshotApiModel
            {
                StableId = "warehouse-zone:" + warehouseId, Revision = "simulation-warehouse-world-1", GeneratedAtUtc = time,
                TotalAvailableQuantity = 16, TotalReservedQuantity = 3, UnassignedLocationCount = 1,
                InventoryItems = new[]
                {
                    new WarehouseWorldInventoryItemApiModel { StableId = "warehouse-inventory:31", WarehouseStableId = "warehouse:" + warehouseId, WarehouseName = "SIMULATED 도심 창고", ProductName = "감자", Sku = "POTATO-20", AvailableQuantity = 12, ReservedQuantity = 3, StorageLocation = "A-01", Status = "적재완료", UpdatedAtUtc = time },
                    new WarehouseWorldInventoryItemApiModel { StableId = "warehouse-inventory:32", WarehouseStableId = "warehouse:" + warehouseId, WarehouseName = "SIMULATED 도심 창고", ProductName = "양파", Sku = "ONION-10", AvailableQuantity = 4, Status = "검수완료", UpdatedAtUtc = time },
                },
                Tasks = new[]
                {
                    new WarehouseWorldTaskApiModel { StableId = "warehouse-putaway:32", WarehouseStableId = "warehouse:" + warehouseId, InventoryItemStableId = "warehouse-inventory:32", TaskKind = "PutAway", ProductName = "양파", Sku = "ONION-10", Quantity = 4, Status = "검수완료", CanExecute = true, UpdatedAtUtc = time },
                    new WarehouseWorldTaskApiModel { StableId = "warehouse-picking:fixture", WarehouseStableId = "warehouse:" + warehouseId, TaskKind = "Picking", ProductName = "감자", Sku = "POTATO-20", Quantity = 3, LocationCode = "A-01", Status = "대기", CanExecute = true, UpdatedAtUtc = time },
                },
                Npcs = new[]
                {
                    new WarehouseWorldNpcApiModel { StableId = "warehouse-npc:dock-worker:fixture", WarehouseStableId = "warehouse:" + warehouseId, SourceTaskStableId = "warehouse-putaway:32", RoleCode = "DockWorker", RouteCode = "warehouse.inbound-to-storage", CurrentWaypointKey = "warehouse.inbound-dock", DestinationWaypointKey = "warehouse.storage-zone", ActivityCode = "PutAway" },
                    new WarehouseWorldNpcApiModel { StableId = "warehouse-npc:picker:fixture", WarehouseStableId = "warehouse:" + warehouseId, SourceTaskStableId = "warehouse-picking:fixture", RoleCode = "Picker", RouteCode = "warehouse.rack-to-outbound", CurrentWaypointKey = "warehouse.rack-zone", DestinationWaypointKey = "warehouse.outbound-staging", ActivityCode = "Picking" },
                },
            });
        }
    }

    public sealed class OperationalWarehouseWorldApiClient : IWarehouseWorldApiClient
    {
        private readonly WarehouseWorldApiOptions options; private readonly WarehouseRuntimeSessionTokenProvider tokenProvider;
        public OperationalWarehouseWorldApiClient(WarehouseWorldApiOptions options, WarehouseRuntimeSessionTokenProvider tokenProvider) { this.options = options; this.tokenProvider = tokenProvider; }
        public async Task<WarehouseWorldSnapshotApiModel> GetAsync(long warehouseId, CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri)) throw new InvalidOperationException("WarehouseWorldApiBaseUrlInvalid");
            var route = WarehouseWorldApiRoutes.AuthorizedSnapshot + "?warehouseId=" + warehouseId.ToString(CultureInfo.InvariantCulture);
            using (var request = UnityWebRequest.Get(new Uri(baseUri, route)))
            using (cancellationToken.Register(request.Abort))
            {
                request.timeout = Math.Max(1, options.TimeoutSeconds); request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + tokenProvider.GetRequiredToken());
                var operation = request.SendWebRequest(); while (!operation.isDone) { cancellationToken.ThrowIfCancellationRequested(); await Task.Yield(); }
                cancellationToken.ThrowIfCancellationRequested();
                if (request.result != UnityWebRequest.Result.Success) throw new InvalidOperationException("WarehouseWorldApiRequestFailed:" + request.responseCode);
                var wire = JsonUtility.FromJson<WarehouseWorldSnapshotWire>(request.downloadHandler.text);
                return wire?.ToApiModel() ?? throw new InvalidOperationException("WarehouseWorldJsonInvalid");
            }
        }
    }

    [Serializable] internal sealed class WarehouseWorldSnapshotWire
    {
        public string stableId = string.Empty, revision = string.Empty, generatedAtUtc = string.Empty; public int totalAvailableQuantity, totalReservedQuantity, unassignedLocationCount;
        public WarehouseInventoryWire[] inventoryItems = Array.Empty<WarehouseInventoryWire>(); public WarehouseTaskWire[] tasks = Array.Empty<WarehouseTaskWire>(); public WarehouseNpcWire[] npcs = Array.Empty<WarehouseNpcWire>();
        public WarehouseWorldSnapshotApiModel ToApiModel() => new WarehouseWorldSnapshotApiModel { StableId = stableId, Revision = revision, GeneratedAtUtc = Parse(generatedAtUtc), TotalAvailableQuantity = totalAvailableQuantity, TotalReservedQuantity = totalReservedQuantity, UnassignedLocationCount = unassignedLocationCount, InventoryItems = Array.ConvertAll(inventoryItems, value => value.ToApiModel()), Tasks = Array.ConvertAll(tasks, value => value.ToApiModel()), Npcs = Array.ConvertAll(npcs, value => value.ToApiModel()) };
        internal static DateTimeOffset Parse(string value) { if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result)) throw new InvalidOperationException("WarehouseWorldDateInvalid"); return result; }
    }
    [Serializable] internal sealed class WarehouseInventoryWire
    {
        public string stableId = string.Empty, warehouseStableId = string.Empty, warehouseName = string.Empty, productName = string.Empty, sku = string.Empty, optionName = string.Empty, storageLocation = string.Empty, status = string.Empty, updatedAtUtc = string.Empty; public int availableQuantity, reservedQuantity; public bool hasCommunityLedger;
        public WarehouseWorldInventoryItemApiModel ToApiModel() => new WarehouseWorldInventoryItemApiModel { StableId = stableId, WarehouseStableId = warehouseStableId, WarehouseName = warehouseName, ProductName = productName, Sku = sku, OptionName = optionName, AvailableQuantity = availableQuantity, ReservedQuantity = reservedQuantity, StorageLocation = storageLocation, Status = status, HasCommunityLedger = hasCommunityLedger, UpdatedAtUtc = WarehouseWorldSnapshotWire.Parse(updatedAtUtc) };
    }
    [Serializable] internal sealed class WarehouseTaskWire
    {
        public string stableId = string.Empty, warehouseStableId = string.Empty, inventoryItemStableId = string.Empty, taskKind = string.Empty, productName = string.Empty, sku = string.Empty, locationCode = string.Empty, status = string.Empty, updatedAtUtc = string.Empty; public int quantity; public bool canExecute;
        public WarehouseWorldTaskApiModel ToApiModel() => new WarehouseWorldTaskApiModel { StableId = stableId, WarehouseStableId = warehouseStableId, InventoryItemStableId = inventoryItemStableId, TaskKind = taskKind, ProductName = productName, Sku = sku, Quantity = quantity, LocationCode = locationCode, Status = status, CanExecute = canExecute, UpdatedAtUtc = WarehouseWorldSnapshotWire.Parse(updatedAtUtc) };
    }
    [Serializable] internal sealed class WarehouseNpcWire
    {
        public string stableId = string.Empty, warehouseStableId = string.Empty, sourceTaskStableId = string.Empty, roleCode = string.Empty, routeCode = string.Empty, currentWaypointKey = string.Empty, destinationWaypointKey = string.Empty, activityCode = string.Empty;
        public WarehouseWorldNpcApiModel ToApiModel() => new WarehouseWorldNpcApiModel { StableId = stableId, WarehouseStableId = warehouseStableId, SourceTaskStableId = sourceTaskStableId, RoleCode = roleCode, RouteCode = routeCode, CurrentWaypointKey = currentWaypointKey, DestinationWaypointKey = destinationWaypointKey, ActivityCode = activityCode };
    }
}
