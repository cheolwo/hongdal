using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.Application;
using Ssalddel.Unity.PresentationContracts.Reconciliation;

namespace Ssalddel.Unity.PublicData
{
    public static class PublicWorldMapApiRoutes
    {
        public const string Observations = "api/v1/community/world-map/observations";
    }

    public static class PublicWorldMapDatasetCodes
    {
        public const string DayWork = "day-work";
        public const string NightLearning = "night-learning";
    }

    public static class PublicDataHallLoadStateCodes
    {
        public const string Idle = "Idle";
        public const string Loading = "Loading";
        public const string Success = "Success";
        public const string InitialLoadError = "InitialLoadError";
        public const string Refreshing = "Refreshing";
        public const string RefreshError = "RefreshError";
    }

    public sealed class PublicWorldMapMetricApiModel
    {
        public string Code { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Unit { get; set; } = string.Empty;
    }

    public sealed class PublicWorldMapLayerApiModel
    {
        public string Code { get; set; } = string.Empty;
        public string DatasetCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string MarkerShape { get; set; } = string.Empty;
    }

    public sealed class PublicWorldMapObservationApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string DatasetCode { get; set; } = string.Empty;
        public string LayerCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public DateTimeOffset? EvidenceAsOfUtc { get; set; }
        public string EvidenceStatusCode { get; set; } = string.Empty;
        public string DetailHref { get; set; } = string.Empty;
        public string SourceHref { get; set; } = string.Empty;
        public string LocationPrecisionCode { get; set; } = string.Empty;
        public string MarkerStatusCode { get; set; } = string.Empty;
        public string SourceDatasetKey { get; set; } = string.Empty;
        public DateTimeOffset? SourceUpdatedAtUtc { get; set; }
        public DateTimeOffset? CollectedAtUtc { get; set; }
        public string UpdateCycle { get; set; } = string.Empty;
        public string FreshnessCode { get; set; } = string.Empty;
        public string BoundaryNotice { get; set; } = string.Empty;
        public PublicWorldMapMetricApiModel[] Metrics { get; set; } =
            Array.Empty<PublicWorldMapMetricApiModel>();
        public string SourceVersion { get; set; } = string.Empty;
    }

    public sealed class PublicWorldMapSnapshotApiModel
    {
        public string DatasetCode { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public PublicWorldMapLayerApiModel[] Layers { get; set; } =
            Array.Empty<PublicWorldMapLayerApiModel>();
        public PublicWorldMapObservationApiModel[] Observations { get; set; } =
            Array.Empty<PublicWorldMapObservationApiModel>();
    }

    public sealed class PublicWorldMapObservation
    {
        public string StableId { get; set; } = string.Empty;
        public string DatasetCode { get; set; } = string.Empty;
        public string LayerCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string SourceName { get; set; } = string.Empty;
        public DateTimeOffset? EvidenceAsOfUtc { get; set; }
        public string EvidenceStatusCode { get; set; } = string.Empty;
        public string DetailHref { get; set; } = string.Empty;
        public string SourceHref { get; set; } = string.Empty;
        public string LocationPrecisionCode { get; set; } = string.Empty;
        public string MarkerStatusCode { get; set; } = string.Empty;
        public string FreshnessCode { get; set; } = string.Empty;
        public string BoundaryNotice { get; set; } = string.Empty;
        public string SourceVersion { get; set; } = string.Empty;
    }

    public sealed class PublicWorldMapSnapshot
    {
        public string DatasetCode { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public PublicWorldMapLayerApiModel[] Layers { get; set; } =
            Array.Empty<PublicWorldMapLayerApiModel>();
        public PublicWorldMapObservation[] Observations { get; set; } =
            Array.Empty<PublicWorldMapObservation>();
        public InterpretationLineage? Lineage { get; set; }
    }

    public sealed class PublicWorldMapMapper
    {
        public PublicWorldMapSnapshot Map(PublicWorldMapSnapshotApiModel source)
            => new PublicWorldMapInterpreter().Interpret(new PublicWorldMapDataMapper().Map(source));
    }

    public sealed class PublicWorldMapQuery
    {
        public string DatasetCode { get; set; } = PublicWorldMapDatasetCodes.DayWork;
    }

    public interface IPublicWorldMapApiClient
    {
        Task<PublicWorldMapSnapshotApiModel> GetAsync(
            PublicWorldMapQuery query,
            CancellationToken cancellationToken = default);
    }

    public interface IPublicWorldMapRepository
    {
        Task<PublicWorldMapSnapshot> 조회Async(
            PublicWorldMapQuery query,
            CancellationToken cancellationToken = default);
    }

    public sealed class PublicWorldMapApiRepository : IPublicWorldMapRepository
    {
        private readonly IPublicWorldMapApiClient apiClient;
        private readonly PublicWorldMapMapper mapper;

        public PublicWorldMapApiRepository(
            IPublicWorldMapApiClient apiClient,
            PublicWorldMapMapper mapper)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PublicWorldMapSnapshot> 조회Async(
            PublicWorldMapQuery query,
            CancellationToken cancellationToken = default)
        {
            if (query == null || string.IsNullOrWhiteSpace(query.DatasetCode))
            {
                throw new ArgumentException("PublicWorldMapQueryInvalid", nameof(query));
            }

            var source = await apiClient.GetAsync(query, cancellationToken).ConfigureAwait(false);
            var snapshot = mapper.Map(source);
            if (!string.Equals(snapshot.DatasetCode, query.DatasetCode, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("PublicWorldMapQueryDatasetMismatch");
            }

            return snapshot;
        }
    }

    public sealed class PublicWorldMapQueryUseCase
    {
        private readonly IPublicWorldMapRepository repository;

        public PublicWorldMapQueryUseCase(IPublicWorldMapRepository repository)
        {
            this.repository = repository;
        }

        public Task<PublicWorldMapSnapshot> 실행Async(
            PublicWorldMapQuery query,
            CancellationToken cancellationToken = default)
        {
            return repository.조회Async(query, cancellationToken);
        }
    }

    public sealed class PublicWorldMapChangeSet
    {
        public PublicWorldMapObservation[] Added { get; set; } = Array.Empty<PublicWorldMapObservation>();
        public PublicWorldMapObservation[] Updated { get; set; } = Array.Empty<PublicWorldMapObservation>();
        public PublicWorldMapObservation[] Removed { get; set; } = Array.Empty<PublicWorldMapObservation>();
        public PublicWorldMapObservation[] Unchanged { get; set; } = Array.Empty<PublicWorldMapObservation>();
    }

    public sealed class PublicWorldMapReconciler
    {
        private static readonly StableIdReconciler<PublicWorldMapObservation> Reconciler =
            new StableIdReconciler<PublicWorldMapObservation>(
                new StableIdReconciliationPolicy<PublicWorldMapObservation>(
                    item => item.StableId,
                    presentationEquivalent: Equivalent));

        public PublicWorldMapChangeSet Reconcile(
            IReadOnlyList<PublicWorldMapObservation> current,
            IReadOnlyList<PublicWorldMapObservation> incoming)
        {
            try
            {
                var changes = Reconciler.Reconcile(current, incoming);
                return new PublicWorldMapChangeSet
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
                throw new InvalidOperationException(
                    "PublicWorldMapSnapshotInvalid:" + error.CollectionName,
                    error);
            }
        }

        private static bool Equivalent(PublicWorldMapObservation left, PublicWorldMapObservation right)
        {
            return left.LayerCode == right.LayerCode
                && left.Latitude.Equals(right.Latitude)
                && left.Longitude.Equals(right.Longitude)
                && left.Title == right.Title
                && left.Summary == right.Summary
                && left.SourceName == right.SourceName
                && left.EvidenceAsOfUtc == right.EvidenceAsOfUtc
                && left.EvidenceStatusCode == right.EvidenceStatusCode
                && left.FreshnessCode == right.FreshnessCode
                && left.MarkerStatusCode == right.MarkerStatusCode;
        }
    }

    public sealed class PublicDataHallLoadResult
    {
        public string StateCode { get; set; } = PublicDataHallLoadStateCodes.Idle;
        public PublicWorldMapSnapshot? Snapshot { get; set; }
        public PublicWorldMapChangeSet? Changes { get; set; }
        public Exception? Error { get; set; }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.UnityResilientWorldLoad,
        SsalddelCodeLayer.ClientAdapter,
        "공공데이터 World Map Snapshot 조회와 마지막 성공 상태 조정을 연결한다.",
        StepKey = "client.public-data-load",
        DependsOnStepKeys = new string[] { "client.last-successful-runtime" },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.UiStateMutation,
        ReadsFrom = SsalddelCodeDataScope.SharedPublicData,
        WritesTo = SsalddelCodeDataScope.ClientPresentation,
        FlowOrder = 20,
        Boundary = "출처와 자료 상태가 있는 조회 결과만 표현하며 공공데이터 원본을 수정하지 않는다.")]
    public sealed class PublicDataHallLoadCoordinator
    {
        private readonly PublicWorldMapQueryUseCase query;
        private readonly PublicWorldMapReconciler reconciler;
        private readonly LastSuccessfulLoadRuntime<PublicWorldMapSnapshot,
            PublicWorldMapChangeSet> runtime = new();

        public PublicDataHallLoadCoordinator(
            PublicWorldMapQueryUseCase query,
            PublicWorldMapReconciler reconciler)
        {
            this.query = query ?? throw new ArgumentNullException(nameof(query));
            this.reconciler = reconciler ?? throw new ArgumentNullException(nameof(reconciler));
        }

        public string StateCode { get; private set; } = PublicDataHallLoadStateCodes.Idle;

        public async Task<PublicDataHallLoadResult> LoadAsync(
            PublicWorldMapQuery request,
            CancellationToken cancellationToken = default)
        {
            var result = await runtime.LoadAsync(
                token => query.실행Async(request, token),
                (previous, snapshot) => reconciler.Reconcile(
                    previous?.Observations ?? Array.Empty<PublicWorldMapObservation>(),
                    snapshot.Observations),
                cancellationToken).ConfigureAwait(false);
            StateCode = MapState(result.StateCode);
            return new PublicDataHallLoadResult
            {
                StateCode = StateCode,
                Snapshot = result.Snapshot,
                Changes = result.Changes,
                Error = result.Error,
            };
        }

        private static string MapState(ZoneRuntimeStateCode stateCode)
            => stateCode switch
            {
                ZoneRuntimeStateCode.Ready => PublicDataHallLoadStateCodes.Success,
                ZoneRuntimeStateCode.Refreshing => PublicDataHallLoadStateCodes.Refreshing,
                ZoneRuntimeStateCode.RefreshError => PublicDataHallLoadStateCodes.RefreshError,
                ZoneRuntimeStateCode.InitialError => PublicDataHallLoadStateCodes.InitialLoadError,
                _ => PublicDataHallLoadStateCodes.Loading,
            };
    }
}
