using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.UrbanMarket
{
    public static class 도심마트LocationKindCodes
    {
        public const string Backroom = "Backroom";
        public const string SalesFloor = "SalesFloor";
    }

    public static class 도심마트TaskKindCodes
    {
        public const string ShelfReplenishment = "ShelfReplenishment";
    }

    public static class 도심마트TaskStateCodes
    {
        public const string Requested = "Requested";
        public const string Assigned = "Assigned";
        public const string InProgress = "InProgress";
        public const string Completed = "Completed";
    }

    public static class 도심마트AllocationStateCodes
    {
        public const string Active = "Active";
        public const string Released = "Released";
        public const string Consumed = "Consumed";
    }

    public static class 도심마트CapabilityCodes
    {
        public const string CreateShelfReplenishment = "CreateShelfReplenishment";
    }

    public sealed class 도심마트운영상품Data
    {
        public string StableId { get; set; } = string.Empty;
        public string 상품명 { get; set; } = string.Empty;
        public string 판매단위 { get; set; } = string.Empty;
    }

    public sealed class 도심마트운영위치Data
    {
        public string StableId { get; set; } = string.Empty;
        public string 이름 { get; set; } = string.Empty;
        public string KindCode { get; set; } = string.Empty;
    }

    public sealed class 도심마트운영재고Data
    {
        public string StableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string LocationStableId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string QuantityUnit { get; set; } = string.Empty;
    }

    public sealed class 도심마트운영진열대Data
    {
        public string StableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string LocationStableId { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public string QuantityUnit { get; set; } = string.Empty;
    }

    public sealed class 도심마트운영작업Data
    {
        public string StableId { get; set; } = string.Empty;
        public string KindCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string SourceInventoryStableId { get; set; } = string.Empty;
        public string TargetShelfStableId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string QuantityUnit { get; set; } = string.Empty;
    }

    public sealed class 도심마트운영작업재고할당Data
    {
        public string StableId { get; set; } = string.Empty;
        public string TaskStableId { get; set; } = string.Empty;
        public string InventoryStableId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string QuantityUnit { get; set; } = string.Empty;
        public string StateCode { get; set; } = 도심마트AllocationStateCodes.Active;
        public string Revision { get; set; } = string.Empty;
    }

    /// <summary>
    /// 권한 있는 마트 운영 Projection의 목표 Data 계약입니다.
    /// 현재는 명시적 Simulation fixture만 제공하며 public 상품 API에서 만들지 않습니다.
    /// </summary>
    public sealed class 도심마트운영DataSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string DataRevision { get; set; } = string.Empty;
        public string 마트명 { get; set; } = string.Empty;
        public string ProjectionAudienceCode { get; set; } = 도심마트ProjectionAudienceCodes.MarketOperatorAuthorized;
        public DataScopeKind ScopeKind { get; set; } = DataScopeKind.AuthorizedUserWorld;
        public DataRuntimeMode Mode { get; set; } = DataRuntimeMode.Simulation;
        public DateTimeOffset GeneratedAt { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string[] ServerCapabilityCodes { get; set; } = Array.Empty<string>();
        public 도심마트운영상품Data[] 상품목록 { get; set; } = Array.Empty<도심마트운영상품Data>();
        public 도심마트운영위치Data[] 위치목록 { get; set; } = Array.Empty<도심마트운영위치Data>();
        public 도심마트운영재고Data[] 재고목록 { get; set; } = Array.Empty<도심마트운영재고Data>();
        public 도심마트운영진열대Data[] 진열대목록 { get; set; } = Array.Empty<도심마트운영진열대Data>();
        public 도심마트운영작업Data[] 작업목록 { get; set; } = Array.Empty<도심마트운영작업Data>();
        public 도심마트운영작업재고할당Data[] 작업재고할당목록 { get; set; } = Array.Empty<도심마트운영작업재고할당Data>();
    }

    public interface I도심마트운영DataQuery
    {
        Task<도심마트운영DataSnapshot> 조회Async(
            CancellationToken cancellationToken = default);
    }

    public sealed class 도심마트운영DataSnapshotValidator
    {
        public string[] Validate(도심마트운영DataSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
            var errors = new List<string>();
            if (!StableDataId.IsValid(snapshot.StableId)) errors.Add("MarketStableIdInvalid");
            Require(snapshot.DataRevision, "MarketDataRevisionMissing", errors);
            Require(snapshot.마트명, "MarketNameMissing", errors);
            Require(snapshot.SourceName, "MarketSourceNameMissing", errors);
            if (!string.Equals(
                    snapshot.ProjectionAudienceCode,
                    도심마트ProjectionAudienceCodes.MarketOperatorAuthorized,
                    StringComparison.Ordinal))
                errors.Add("MarketOperationAudienceInvalid");
            if (snapshot.ScopeKind != DataScopeKind.AuthorizedUserWorld)
                errors.Add("MarketOperationScopeInvalid");
            if (snapshot.GeneratedAt == default) errors.Add("MarketGeneratedAtMissing");
            if (snapshot.ServerCapabilityCodes == null) errors.Add("MarketCapabilitiesMissing");
            if (snapshot.상품목록 == null) errors.Add("MarketProductsMissing");
            if (snapshot.위치목록 == null) errors.Add("MarketLocationsMissing");
            if (snapshot.재고목록 == null) errors.Add("MarketInventoriesMissing");
            if (snapshot.진열대목록 == null) errors.Add("MarketShelvesMissing");
            if (snapshot.작업목록 == null) errors.Add("MarketTasksMissing");
            if (snapshot.작업재고할당목록 == null) errors.Add("MarketTaskAllocationsMissing");
            if (errors.Count > 0) return errors.ToArray();

            var productItems = snapshot.상품목록!;
            var locationItems = snapshot.위치목록!;
            var inventoryItems = snapshot.재고목록!;
            var shelfItems = snapshot.진열대목록!;
            var taskItems = snapshot.작업목록!;
            var allocationItems = snapshot.작업재고할당목록!;
            var products = Index(productItems, value => value.StableId, "Product", errors);
            var locations = Index(locationItems, value => value.StableId, "Location", errors);
            var inventories = Index(inventoryItems, value => value.StableId, "Inventory", errors);
            var shelves = Index(shelfItems, value => value.StableId, "Shelf", errors);
            var tasks = Index(taskItems, value => value.StableId, "Task", errors);
            Index(allocationItems, value => value.StableId, "TaskAllocation", errors);
            if (errors.Count > 0) return errors.ToArray();

            var allIds = productItems.Select(value => value.StableId)
                .Concat(locationItems.Select(value => value.StableId))
                .Concat(inventoryItems.Select(value => value.StableId))
                .Concat(shelfItems.Select(value => value.StableId))
                .Concat(taskItems.Select(value => value.StableId))
                .Concat(allocationItems.Select(value => value.StableId))
                .Append(snapshot.StableId);
            var duplicateWorldId = allIds.GroupBy(value => value, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateWorldId != null)
                errors.Add("DuplicateMarketWorldStableId:" + duplicateWorldId.Key);

            foreach (var product in productItems)
            {
                Require(product.상품명, "ProductNameMissing:" + product.StableId, errors);
                Require(product.판매단위, "ProductSaleUnitMissing:" + product.StableId, errors);
            }

            foreach (var location in locationItems)
            {
                Require(location.이름, "LocationNameMissing:" + location.StableId, errors);
                if (location.KindCode != 도심마트LocationKindCodes.Backroom
                    && location.KindCode != 도심마트LocationKindCodes.SalesFloor)
                    errors.Add("LocationKindInvalid:" + location.StableId);
            }

            foreach (var inventory in inventoryItems)
            {
                if (!products.ContainsKey(inventory.ProductStableId))
                    errors.Add("InventoryProductUnknown:" + inventory.StableId);
                if (!locations.ContainsKey(inventory.LocationStableId))
                    errors.Add("InventoryLocationUnknown:" + inventory.StableId);
                if (inventory.Quantity < 0) errors.Add("InventoryQuantityInvalid:" + inventory.StableId);
                Require(inventory.QuantityUnit, "InventoryQuantityUnitMissing:" + inventory.StableId, errors);
            }

            foreach (var shelf in shelfItems)
            {
                if (!products.ContainsKey(shelf.ProductStableId))
                    errors.Add("ShelfProductUnknown:" + shelf.StableId);
                if (!locations.ContainsKey(shelf.LocationStableId))
                    errors.Add("ShelfLocationUnknown:" + shelf.StableId);
                if (shelf.Capacity <= 0) errors.Add("ShelfCapacityInvalid:" + shelf.StableId);
                Require(shelf.QuantityUnit, "ShelfQuantityUnitMissing:" + shelf.StableId, errors);
            }

            foreach (var task in taskItems)
            {
                if (!products.ContainsKey(task.ProductStableId))
                    errors.Add("TaskProductUnknown:" + task.StableId);
                if (!inventories.TryGetValue(task.SourceInventoryStableId, out var inventory))
                    errors.Add("TaskInventoryUnknown:" + task.StableId);
                else if (!string.Equals(inventory.ProductStableId, task.ProductStableId, StringComparison.Ordinal))
                    errors.Add("TaskInventoryProductMismatch:" + task.StableId);
                if (!shelves.TryGetValue(task.TargetShelfStableId, out var shelf))
                    errors.Add("TaskShelfUnknown:" + task.StableId);
                else if (!string.Equals(shelf.ProductStableId, task.ProductStableId, StringComparison.Ordinal))
                    errors.Add("TaskShelfProductMismatch:" + task.StableId);
                if (task.KindCode != 도심마트TaskKindCodes.ShelfReplenishment)
                    errors.Add("TaskKindInvalid:" + task.StableId);
                if (task.Quantity <= 0) errors.Add("TaskQuantityInvalid:" + task.StableId);
                Require(task.StateCode, "TaskStateMissing:" + task.StableId, errors);
                Require(task.QuantityUnit, "TaskQuantityUnitMissing:" + task.StableId, errors);
            }

            foreach (var allocation in allocationItems)
            {
                if (!tasks.TryGetValue(allocation.TaskStableId, out var task))
                {
                    errors.Add("TaskAllocationTaskUnknown:" + allocation.StableId);
                }
                if (!inventories.TryGetValue(allocation.InventoryStableId, out var inventory))
                {
                    errors.Add("TaskAllocationInventoryUnknown:" + allocation.StableId);
                }
                else if (task != null
                         && !string.Equals(inventory.ProductStableId, task.ProductStableId, StringComparison.Ordinal))
                {
                    errors.Add("TaskAllocationProductMismatch:" + allocation.StableId);
                }
                if (allocation.Quantity <= 0)
                    errors.Add("TaskAllocationQuantityInvalid:" + allocation.StableId);
                Require(allocation.QuantityUnit, "TaskAllocationQuantityUnitMissing:" + allocation.StableId, errors);
                Require(allocation.Revision, "TaskAllocationRevisionMissing:" + allocation.StableId, errors);
                if (allocation.StateCode != 도심마트AllocationStateCodes.Active
                    && allocation.StateCode != 도심마트AllocationStateCodes.Released
                    && allocation.StateCode != 도심마트AllocationStateCodes.Consumed)
                    errors.Add("TaskAllocationStateInvalid:" + allocation.StableId);
                if (inventory != null
                    && !string.Equals(inventory.QuantityUnit, allocation.QuantityUnit, StringComparison.Ordinal))
                    errors.Add("TaskAllocationUnitMismatch:" + allocation.StableId);
                if (task != null
                    && !string.Equals(task.QuantityUnit, allocation.QuantityUnit, StringComparison.Ordinal))
                    errors.Add("TaskAllocationTaskUnitMismatch:" + allocation.StableId);
            }

            foreach (var task in taskItems.Where(value => value.StateCode != 도심마트TaskStateCodes.Completed))
            {
                var explicitAllocations = allocationItems
                    .Where(value => string.Equals(value.TaskStableId, task.StableId, StringComparison.Ordinal))
                    .ToArray();
                if (explicitAllocations.Length == 0) continue;
                var activeQuantity = explicitAllocations
                    .Where(value => value.StateCode == 도심마트AllocationStateCodes.Active)
                    .Sum(value => value.Quantity);
                if (activeQuantity != task.Quantity)
                    errors.Add("TaskAllocationQuantityMismatch:" + task.StableId);
            }

            return errors.ToArray();
        }

        private static Dictionary<string, T> Index<T>(
            IEnumerable<T> values,
            Func<T, string> id,
            string kind,
            ICollection<string> errors)
            where T : class
        {
            var result = new Dictionary<string, T>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (value == null)
                {
                    errors.Add(kind + "Missing");
                    continue;
                }

                var stableId = id(value);
                if (!StableDataId.IsValid(stableId))
                {
                    errors.Add(kind + "StableIdInvalid:" + stableId);
                    continue;
                }
                if (!result.TryAdd(stableId, value))
                    errors.Add("Duplicate" + kind + "StableId:" + stableId);
            }
            return result;
        }

        private static void Require(string value, string error, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value)) errors.Add(error);
        }
    }

    public sealed class Simulated도심마트운영DataQuery : I도심마트운영DataQuery
    {
        private static readonly DateTimeOffset FixtureAsOf =
            DateTimeOffset.Parse("2026-08-09T09:00:00+09:00");

        public Task<도심마트운영DataSnapshot> 조회Async(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new 도심마트운영DataSnapshot
            {
                StableId = "market:urban-demo-001",
                DataRevision = "simulation:market-operations:1",
                마트명 = "살뜰 도심 마트",
                ProjectionAudienceCode = 도심마트ProjectionAudienceCodes.MarketOperatorAuthorized,
                ScopeKind = DataScopeKind.AuthorizedUserWorld,
                Mode = DataRuntimeMode.Simulation,
                GeneratedAt = FixtureAsOf,
                SourceName = "SIMULATED urban-market operations fixture",
                ServerCapabilityCodes = new[] { 도심마트CapabilityCodes.CreateShelfReplenishment },
                상품목록 = new[]
                {
                    new 도심마트운영상품Data
                    {
                        StableId = "market-product:potato",
                        상품명 = "감자",
                        판매단위 = "20kg",
                    },
                    new 도심마트운영상품Data
                    {
                        StableId = "market-product:onion",
                        상품명 = "양파",
                        판매단위 = "10kg",
                    },
                },
                위치목록 = new[]
                {
                    new 도심마트운영위치Data
                    {
                        StableId = "market-location:backroom-a",
                        이름 = "후방 보관 A",
                        KindCode = 도심마트LocationKindCodes.Backroom,
                    },
                    new 도심마트운영위치Data
                    {
                        StableId = "market-location:sales-floor-a",
                        이름 = "판매장 A",
                        KindCode = 도심마트LocationKindCodes.SalesFloor,
                    },
                },
                재고목록 = new[]
                {
                    Inventory("market-inventory:potato-backroom", "market-product:potato", "market-location:backroom-a", 20),
                    Inventory("market-inventory:potato-display", "market-product:potato", "market-location:sales-floor-a", 2),
                    Inventory("market-inventory:onion-backroom", "market-product:onion", "market-location:backroom-a", 8),
                    Inventory("market-inventory:onion-display", "market-product:onion", "market-location:sales-floor-a", 1),
                },
                진열대목록 = new[]
                {
                    Shelf("market-shelf:potato", "market-product:potato", 10),
                    Shelf("market-shelf:onion", "market-product:onion", 8),
                },
                작업목록 = new[]
                {
                    new 도심마트운영작업Data
                    {
                        StableId = "market-task:replenishment-onion",
                        KindCode = 도심마트TaskKindCodes.ShelfReplenishment,
                        StateCode = 도심마트TaskStateCodes.Assigned,
                        ProductStableId = "market-product:onion",
                        SourceInventoryStableId = "market-inventory:onion-backroom",
                        TargetShelfStableId = "market-shelf:onion",
                        Quantity = 4,
                        QuantityUnit = "상자",
                    },
                },
            });
        }

        private static 도심마트운영재고Data Inventory(
            string stableId,
            string productStableId,
            string locationStableId,
            int quantity)
            => new 도심마트운영재고Data
            {
                StableId = stableId,
                ProductStableId = productStableId,
                LocationStableId = locationStableId,
                Quantity = quantity,
                QuantityUnit = "상자",
            };

        private static 도심마트운영진열대Data Shelf(
            string stableId,
            string productStableId,
            int capacity)
            => new 도심마트운영진열대Data
            {
                StableId = stableId,
                ProductStableId = productStableId,
                LocationStableId = "market-location:sales-floor-a",
                Capacity = capacity,
                QuantityUnit = "상자",
            };
    }
}
