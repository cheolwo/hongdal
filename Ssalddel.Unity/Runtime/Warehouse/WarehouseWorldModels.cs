using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Unity.Application;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.PresentationContracts.Reconciliation;

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
        public CargoWarehouseHandoffApiModel[] InboundHandoffs { get; set; } = Array.Empty<CargoWarehouseHandoffApiModel>();
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
        public string CanonicalTaskStableId { get; set; } = string.Empty;
        public string CanonicalRelationStableId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReservedQuantity { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string OptionName { get; set; } = string.Empty;
        public bool HasCommunityLedger { get; set; }
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
        public InterpretationLineage? Lineage { get; set; }
    }

    /// <summary>
    /// 기존 소비 코드의 호환 facade입니다. 새 코드는 WarehouseDataMapper와
    /// WarehouseWorldInterpreter를 명시적으로 조합합니다.
    /// </summary>
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class WarehouseWorldMapper
    {
        private readonly WarehouseDataMapper dataMapper;
        private readonly WarehouseWorldInterpreter interpreter;

        public WarehouseWorldMapper()
            : this(new WarehouseDataMapper(), new WarehouseWorldInterpreter())
        {
        }

        public WarehouseWorldMapper(WarehouseDataMapper dataMapper, WarehouseWorldInterpreter interpreter)
        {
            this.dataMapper = dataMapper ?? throw new ArgumentNullException(nameof(dataMapper));
            this.interpreter = interpreter ?? throw new ArgumentNullException(nameof(interpreter));
        }

        public WarehouseWorldSnapshot Map(WarehouseWorldSnapshotApiModel source)
            => interpreter.Interpret(dataMapper.Map(source));
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
    public interface IWarehouseWorldApiClient
    {
        Task<WarehouseWorldSnapshotApiModel> GetAsync(long warehouseId, CancellationToken cancellationToken = default);
    }
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public interface IWarehouseWorldRepository
    {
        Task<WarehouseWorldSnapshot> 조회Async(long warehouseId, CancellationToken cancellationToken = default);
    }
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Unity가 권위 Core 또는 원격 Host와 통신하는 Adapter 경계를 제공한다.",
        Boundary = "Unity 표현은 서버·Local Runtime의 권위 상태를 대신하지 않는다.")]
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
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class WarehouseWorldQueryUseCase
    {
        private readonly IWarehouseDataRepository? dataRepository;
        private readonly WarehouseWorldInterpreter? interpreter;
        private readonly IWarehouseWorldRepository? compatibilityRepository;

        public WarehouseWorldQueryUseCase(
            IWarehouseDataRepository dataRepository,
            WarehouseWorldInterpreter interpreter)
        {
            this.dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            this.interpreter = interpreter ?? throw new ArgumentNullException(nameof(interpreter));
        }

        /// <summary>기존 W1 조립 코드와의 호환 constructor입니다.</summary>
        public WarehouseWorldQueryUseCase(IWarehouseWorldRepository repository)
            => compatibilityRepository = repository ?? throw new ArgumentNullException(nameof(repository));

        public async Task<WarehouseWorldSnapshot> 실행Async(
            long warehouseId,
            CancellationToken cancellationToken = default)
        {
            if (dataRepository != null)
            {
                var data = await dataRepository.조회Async(warehouseId, cancellationToken).ConfigureAwait(false);
                return interpreter!.Interpret(data);
            }

            return await compatibilityRepository!.조회Async(warehouseId, cancellationToken).ConfigureAwait(false);
        }
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
        private static readonly StableIdReconciler<WarehouseWorldObject> Reconciler =
            new StableIdReconciler<WarehouseWorldObject>(
                new StableIdReconciliationPolicy<WarehouseWorldObject>(
                    item => item.StableId,
                    presentationEquivalent: Equivalent));

        public WarehouseWorldChangeSet Reconcile(IReadOnlyList<WarehouseWorldObject> current, IReadOnlyList<WarehouseWorldObject> incoming)
        {
            try
            {
                var changes = Reconciler.Reconcile(current, incoming);
                return new WarehouseWorldChangeSet
                {
                    Added = changes.Added,
                    Updated = changes.Updated,
                    Unchanged = changes.Unchanged,
                    Removed = changes.Removed,
                };
            }
            catch (StableIdReconciliationException error)
                when (error.ErrorCode == "StableIdReconcileItemMissing"
                      || error.ErrorCode == "StableIdInvalid"
                      || error.ErrorCode == "DuplicateStableId")
            {
                throw new InvalidOperationException("WarehouseWorldSnapshotInvalid", error);
            }
        }
        private static bool Equivalent(WarehouseWorldObject a, WarehouseWorldObject b) => a.Kind == b.Kind && a.Title == b.Title && a.Status == b.Status && a.LocationCode == b.LocationCode && a.CurrentLocationCode == b.CurrentLocationCode && a.SourceStableId == b.SourceStableId && a.Quantity == b.Quantity && a.ReservedQuantity == b.ReservedQuantity && a.Sku == b.Sku && a.OptionName == b.OptionName && a.HasCommunityLedger == b.HasCommunityLedger && a.CanExecute == b.CanExecute && a.UpdatedAtUtc == b.UpdatedAtUtc;
    }

    public sealed class WarehouseWorldLoadResult
    {
        public string StateCode { get; set; } = WarehouseWorldLoadStateCodes.Idle;
        public WarehouseWorldSnapshot? Snapshot { get; set; }
        public WarehouseWorldChangeSet? Changes { get; set; }
        public Exception? Error { get; set; }
    }
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.UnityResilientWorldLoad,
        SsalddelCodeLayer.ClientAdapter,
        "창고 World Snapshot 조회와 마지막 성공 상태 조정을 연결한다.",
        StepKey = "client.warehouse-load",
        DependsOnStepKeys = new string[] { "client.last-successful-runtime" },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.UiStateMutation,
        ReadsFrom = SsalddelCodeDataScope.OperationalState,
        WritesTo = SsalddelCodeDataScope.ClientPresentation,
        FlowOrder = 20,
        Boundary = "권한 적용된 창고 Projection만 표현하며 Unity가 입출고 완료를 확정하지 않는다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class WarehouseWorldLoadCoordinator
    {
        private readonly WarehouseWorldQueryUseCase query;
        private readonly WarehouseWorldReconciler reconciler;
        private readonly LastSuccessfulLoadRuntime<WarehouseWorldSnapshot,
            WarehouseWorldChangeSet> runtime = new();
        public WarehouseWorldLoadCoordinator(WarehouseWorldQueryUseCase query, WarehouseWorldReconciler reconciler) { this.query = query; this.reconciler = reconciler; }
        public async Task<WarehouseWorldLoadResult> LoadAsync(long warehouseId, CancellationToken cancellationToken = default)
        {
            var result = await runtime.LoadAsync(
                token => query.실행Async(warehouseId, token),
                (previous, snapshot) => reconciler.Reconcile(
                    previous?.Objects ?? Array.Empty<WarehouseWorldObject>(),
                    snapshot.Objects),
                cancellationToken).ConfigureAwait(false);
            return new WarehouseWorldLoadResult
            {
                StateCode = result.StateCode switch
                {
                    ZoneRuntimeStateCode.Ready => WarehouseWorldLoadStateCodes.Success,
                    ZoneRuntimeStateCode.RefreshError => WarehouseWorldLoadStateCodes.RefreshError,
                    ZoneRuntimeStateCode.InitialError => WarehouseWorldLoadStateCodes.InitialLoadError,
                    _ => WarehouseWorldLoadStateCodes.Loading,
                },
                Snapshot = result.Snapshot,
                Changes = result.Changes,
                Error = result.Error,
            };
        }
    }
}
