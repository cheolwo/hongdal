using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.Npcs;

namespace Ssalddel.Unity.Warehouse
{
    public static class WarehouseWorldInterpretationVersions
    {
        public const string Contract = "warehouse-world-interpreter-v2";
        public const string RuleSet = "warehouse-inbound-handoff-rules-v1";
    }

    public sealed class WarehouseInventoryData
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

    public sealed class WarehouseTaskData
    {
        public string StableId { get; set; } = string.Empty;
        public string WarehouseStableId { get; set; } = string.Empty;
        public string InventoryItemStableId { get; set; } = string.Empty;
        public string CanonicalTaskStableId { get; set; } = string.Empty;
        public string TaskKind { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool CanExecute { get; set; }
        public DateTimeOffset UpdatedAtUtc { get; set; }
    }

    public sealed class WarehouseNpcData
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

    public sealed class WarehouseDataSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string DataRevision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public int TotalAvailableQuantity { get; set; }
        public int TotalReservedQuantity { get; set; }
        public int UnassignedLocationCount { get; set; }
        public WarehouseInventoryData[] InventoryItems { get; set; } = Array.Empty<WarehouseInventoryData>();
        public WarehouseTaskData[] Tasks { get; set; } = Array.Empty<WarehouseTaskData>();
        public WarehouseNpcData[] Npcs { get; set; } = Array.Empty<WarehouseNpcData>();
        public CargoWarehouseHandoffSnapshot[] InboundHandoffs { get; set; } = Array.Empty<CargoWarehouseHandoffSnapshot>();
    }

    /// <summary>Wire contract를 검증하고 표현 의미가 없는 Warehouse Data Snapshot으로 복사합니다.</summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class WarehouseDataMapper
    {
        private readonly CargoWarehouseHandoffMapper handoffMapper;

        public WarehouseDataMapper()
            : this(new CargoWarehouseHandoffMapper(new NpcMovementMapper()))
        {
        }

        public WarehouseDataMapper(CargoWarehouseHandoffMapper handoffMapper)
            => this.handoffMapper = handoffMapper ?? throw new ArgumentNullException(nameof(handoffMapper));

        public WarehouseDataSnapshot Map(WarehouseWorldSnapshotApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            RequireStableId(source.StableId);
            Require(source.Revision, "WarehouseWorldRevisionMissing");
            if (source.GeneratedAtUtc == default) throw new InvalidOperationException("WarehouseWorldGeneratedAtMissing");
            if (source.InventoryItems == null || source.Tasks == null || source.Npcs == null || source.InboundHandoffs == null)
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
                RequireStableId(item.WarehouseStableId);
                Require(item.ProductName, "WarehouseInventoryProductMissing:" + item.StableId);
                if (item.AvailableQuantity < 0 || item.ReservedQuantity < 0 || item.UpdatedAtUtc == default)
                    throw new InvalidOperationException("WarehouseInventoryValuesInvalid:" + item.StableId);
            }

            foreach (var task in source.Tasks)
            {
                RequireStableId(task.WarehouseStableId);
                Require(task.TaskKind, "WarehouseTaskKindMissing:" + task.StableId);
                if (task.TaskKind == "PutAway" && !inventoryIds.Contains(task.InventoryItemStableId))
                    throw new InvalidOperationException("WarehouseTaskInventoryUnknown:" + task.StableId);
                if (task.TaskKind == "PutAway"
                    && source.InboundHandoffs.Length > 0
                    && !StableDataId.IsValid(task.CanonicalTaskStableId))
                    throw new InvalidOperationException("WarehouseTaskCanonicalReferenceInvalid:" + task.StableId);
                if (task.Quantity < 0 || task.UpdatedAtUtc == default)
                    throw new InvalidOperationException("WarehouseTaskValuesInvalid:" + task.StableId);
            }

            foreach (var npc in source.Npcs)
            {
                RequireStableId(npc.WarehouseStableId);
                Require(npc.RoleCode, "WarehouseNpcRoleMissing:" + npc.StableId);
                if (!taskIds.Contains(npc.SourceTaskStableId))
                    throw new InvalidOperationException("WarehouseNpcTaskUnknown:" + npc.StableId);
                Require(npc.CurrentWaypointKey, "WarehouseNpcCurrentWaypointMissing:" + npc.StableId);
                Require(npc.DestinationWaypointKey, "WarehouseNpcDestinationWaypointMissing:" + npc.StableId);
            }

            return new WarehouseDataSnapshot
            {
                StableId = source.StableId.Trim(),
                DataRevision = source.Revision.Trim(),
                GeneratedAtUtc = source.GeneratedAtUtc,
                TotalAvailableQuantity = source.TotalAvailableQuantity,
                TotalReservedQuantity = source.TotalReservedQuantity,
                UnassignedLocationCount = source.UnassignedLocationCount,
                InventoryItems = source.InventoryItems.Select(item => new WarehouseInventoryData
                {
                    StableId = item.StableId,
                    WarehouseStableId = item.WarehouseStableId,
                    WarehouseName = item.WarehouseName,
                    ProductName = item.ProductName,
                    Sku = item.Sku,
                    OptionName = item.OptionName,
                    AvailableQuantity = item.AvailableQuantity,
                    ReservedQuantity = item.ReservedQuantity,
                    StorageLocation = item.StorageLocation,
                    Status = item.Status,
                    HasCommunityLedger = item.HasCommunityLedger,
                    UpdatedAtUtc = item.UpdatedAtUtc,
                }).ToArray(),
                Tasks = source.Tasks.Select(task => new WarehouseTaskData
                {
                    StableId = task.StableId,
                    WarehouseStableId = task.WarehouseStableId,
                    InventoryItemStableId = task.InventoryItemStableId,
                    CanonicalTaskStableId = task.CanonicalTaskStableId,
                    TaskKind = task.TaskKind,
                    ProductName = task.ProductName,
                    Sku = task.Sku,
                    Quantity = task.Quantity,
                    LocationCode = task.LocationCode,
                    Status = task.Status,
                    CanExecute = task.CanExecute,
                    UpdatedAtUtc = task.UpdatedAtUtc,
                }).ToArray(),
                Npcs = source.Npcs.Select(npc => new WarehouseNpcData
                {
                    StableId = npc.StableId,
                    WarehouseStableId = npc.WarehouseStableId,
                    SourceTaskStableId = npc.SourceTaskStableId,
                    RoleCode = npc.RoleCode,
                    RouteCode = npc.RouteCode,
                    CurrentWaypointKey = npc.CurrentWaypointKey,
                    DestinationWaypointKey = npc.DestinationWaypointKey,
                    ActivityCode = npc.ActivityCode,
                }).ToArray(),
                InboundHandoffs = source.InboundHandoffs.Select(handoffMapper.Map).ToArray(),
            };
        }

        private static void ValidateIds(IEnumerable<string?> values, string kind)
        {
            var array = values.ToArray();
            if (array.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException(kind + "StableIdInvalid");
            var duplicate = array.GroupBy(value => value!, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException("Duplicate" + kind + ":" + duplicate.Key);
        }

        private static void RequireStableId(string value)
        {
            if (!StableDataId.IsValid(value))
                throw new InvalidOperationException("WarehouseWorldStableIdInvalid:" + value);
        }

        private static void Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(error);
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public interface IWarehouseDataRepository
    {
        Task<WarehouseDataSnapshot> 조회Async(
            long warehouseId,
            CancellationToken cancellationToken = default);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public sealed class WarehouseApiDataRepository : IWarehouseDataRepository
    {
        private readonly IWarehouseWorldApiClient client;
        private readonly WarehouseDataMapper mapper;

        public WarehouseApiDataRepository(
            IWarehouseWorldApiClient client,
            WarehouseDataMapper mapper)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<WarehouseDataSnapshot> 조회Async(
            long warehouseId,
            CancellationToken cancellationToken = default)
        {
            if (warehouseId <= 0) throw new ArgumentOutOfRangeException(nameof(warehouseId));
            var source = await client.GetAsync(warehouseId, cancellationToken).ConfigureAwait(false);
            return mapper.Map(source);
        }
    }

    /// <summary>Warehouse 사실을 stable-ID World object와 relation 입력으로 해석합니다.</summary>
    public sealed class WarehouseWorldInterpreter
    {
        private readonly WarehouseInboundHandoffInterpreter handoffInterpreter;

        public WarehouseWorldInterpreter()
            : this(new WarehouseInboundHandoffInterpreter())
        {
        }

        public WarehouseWorldInterpreter(WarehouseInboundHandoffInterpreter handoffInterpreter)
            => this.handoffInterpreter = handoffInterpreter ?? throw new ArgumentNullException(nameof(handoffInterpreter));

        public WarehouseWorldSnapshot Interpret(WarehouseDataSnapshot source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            var inputs = new DataRevisionSet(new[]
            {
                new DataRevisionReference(
                    source.StableId,
                    source.DataRevision,
                    source.GeneratedAtUtc,
                    DataQualityCodes.Observed),
            }.Concat(source.InboundHandoffs.Select(handoff => new DataRevisionReference(
                handoff.StableId,
                handoff.Revision.ToString(System.Globalization.CultureInfo.InvariantCulture),
                handoff.GeneratedAt,
                DataQualityCodes.Observed))));
            var interpretationRevision = WorldDataFlowRevisionCalculator.CalculateInterpretation(
                inputs,
                WarehouseWorldInterpretationVersions.Contract,
                WarehouseWorldInterpretationVersions.RuleSet);
            var lineage = new InterpretationLineage(
                inputs,
                WarehouseWorldInterpretationVersions.Contract,
                WarehouseWorldInterpretationVersions.RuleSet,
                interpretationRevision);

            var objects = source.InventoryItems.Select(item => Object(
                    item.StableId, "Inventory", item.WarehouseStableId, item.ProductName, item.Status,
                    item.StorageLocation, string.Empty, item.AvailableQuantity, false, item.UpdatedAtUtc,
                    reservedQuantity: item.ReservedQuantity, sku: item.Sku, optionName: item.OptionName,
                    hasCommunityLedger: item.HasCommunityLedger))
                .Concat(source.Tasks.Select(task => Object(
                    task.StableId, "Task", task.WarehouseStableId, task.ProductName, task.Status,
                    task.LocationCode, task.InventoryItemStableId, task.Quantity, task.CanExecute,
                    task.UpdatedAtUtc, sku: task.Sku, canonicalTaskStableId: task.CanonicalTaskStableId)))
                .Concat(source.Npcs.Select(npc => Object(
                    npc.StableId, "Npc", npc.WarehouseStableId, npc.RoleCode, npc.ActivityCode,
                    npc.DestinationWaypointKey, npc.SourceTaskStableId, 0, false, null,
                    npc.CurrentWaypointKey,
                    canonicalTaskStableId: source.Tasks.First(task => task.StableId == npc.SourceTaskStableId).CanonicalTaskStableId)))
                .ToArray();

            var snapshot = new WarehouseWorldSnapshot
            {
                StableId = source.StableId,
                Revision = source.DataRevision,
                GeneratedAtUtc = source.GeneratedAtUtc,
                TotalAvailableQuantity = source.TotalAvailableQuantity,
                TotalReservedQuantity = source.TotalReservedQuantity,
                UnassignedLocationCount = source.UnassignedLocationCount,
                Objects = objects,
                Lineage = lineage,
            };
            return handoffInterpreter.Compose(snapshot, source.InboundHandoffs);
        }

        private static WarehouseWorldObject Object(
            string id,
            string kind,
            string warehouse,
            string title,
            string status,
            string location,
            string source,
            int quantity,
            bool canExecute,
            DateTimeOffset? updated,
            string currentLocation = "",
            int reservedQuantity = 0,
            string sku = "",
            string optionName = "",
            bool hasCommunityLedger = false,
            string canonicalTaskStableId = "")
            => new WarehouseWorldObject
            {
                StableId = id,
                Kind = kind,
                WarehouseStableId = warehouse,
                Title = title,
                Status = status,
                LocationCode = location,
                CurrentLocationCode = currentLocation,
                SourceStableId = source,
                CanonicalTaskStableId = canonicalTaskStableId,
                CanonicalRelationStableId = canonicalTaskStableId,
                Quantity = quantity,
                ReservedQuantity = reservedQuantity,
                Sku = sku,
                OptionName = optionName,
                HasCommunityLedger = hasCommunityLedger,
                CanExecute = canExecute,
                UpdatedAtUtc = updated,
            };
    }
}
