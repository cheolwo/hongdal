using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Unity.Application;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.PresentationContracts.Reconciliation;

namespace Ssalddel.Unity.Community
{
    public static class CommunityMarketSquareApiRoutes
    {
        public const string PublicSnapshot = "api/v1/community/world/zones/community-market-square";
    }

    public static class CommunityMarketSquareLoadStateCodes
    {
        public const string Idle = "Idle";
        public const string Loading = "Loading";
        public const string Success = "Success";
        public const string InitialLoadError = "InitialLoadError";
        public const string Refreshing = "Refreshing";
        public const string RefreshError = "RefreshError";
    }

    public sealed class CommunitySquareBoardApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string BoardKey { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GroupDisplayName { get; set; } = string.Empty;
        public string PostingAccessCode { get; set; } = string.Empty;
        public int PostCount { get; set; }
        public DateTimeOffset? LatestPostAtUtc { get; set; }
    }

    public sealed class CommunitySquarePostApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public long PostId { get; set; }
        public string Category { get; set; } = string.Empty;
        public string WorkflowTag { get; set; } = string.Empty;
        public string RoleTag { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string TopicClassificationCode { get; set; } = string.Empty;
        public bool IsSystemGenerated { get; set; }
        public bool IsInterestGatheringEnabled { get; set; }
        public int RecommendationCount { get; set; }
        public int CommentCount { get; set; }
        public DateTimeOffset PublishedAtUtc { get; set; }
        public string DetailHref { get; set; } = string.Empty;
    }

    public sealed class CommunitySquareActivityApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string CommunityScope { get; set; } = string.Empty;
        public string ActivityKind { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string TimeBucketLabel { get; set; } = string.Empty;
        public string TimePrecision { get; set; } = string.Empty;
        public int AggregationCount { get; set; }
        public DateTimeOffset OccurredAtUtc { get; set; }
        public string PrivacyPolicyVersion { get; set; } = string.Empty;
    }

    public sealed class CommunitySquareLedgerApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string SourcePostStableId { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string CurrentStage { get; set; } = string.Empty;
        public bool DetailAvailable { get; set; }
        public string DetailHref { get; set; } = string.Empty;
    }

    public sealed class CommunityMarketSquareSnapshotApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public CommunitySquareBoardApiModel[] Boards { get; set; } = Array.Empty<CommunitySquareBoardApiModel>();
        public CommunitySquarePostApiModel[] Posts { get; set; } = Array.Empty<CommunitySquarePostApiModel>();
        public CommunitySquareActivityApiModel[] ActivitySignals { get; set; } = Array.Empty<CommunitySquareActivityApiModel>();
        public CommunitySquareLedgerApiModel[] Ledgers { get; set; } = Array.Empty<CommunitySquareLedgerApiModel>();
    }

    public sealed class CommunitySquareWorldItem
    {
        public string StableId { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DetailHref { get; set; } = string.Empty;
        public int Count { get; set; }
        public DateTimeOffset? AsOfUtc { get; set; }
    }

    public sealed class CommunityMarketSquareSnapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public CommunitySquareWorldItem[] Items { get; set; } = Array.Empty<CommunitySquareWorldItem>();
        public InterpretationLineage? Lineage { get; set; }
    }

    public sealed class CommunityMarketSquareMapper
    {
        public CommunityMarketSquareSnapshot Map(CommunityMarketSquareSnapshotApiModel source)
            => new CommunitySquareWorldInterpreter().Interpret(new CommunitySquareDataMapper().Map(source));
    }

    public interface ICommunityMarketSquareApiClient
    {
        Task<CommunityMarketSquareSnapshotApiModel> GetPublicSnapshotAsync(CancellationToken cancellationToken = default);
    }

    public interface ICommunityMarketSquareRepository
    {
        Task<CommunityMarketSquareSnapshot> 조회Async(CancellationToken cancellationToken = default);
    }

    public sealed class CommunityMarketSquareApiRepository : ICommunityMarketSquareRepository
    {
        private readonly ICommunityMarketSquareApiClient apiClient;
        private readonly CommunityMarketSquareMapper mapper;

        public CommunityMarketSquareApiRepository(ICommunityMarketSquareApiClient apiClient, CommunityMarketSquareMapper mapper)
        {
            this.apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<CommunityMarketSquareSnapshot> 조회Async(CancellationToken cancellationToken = default)
            => mapper.Map(await apiClient.GetPublicSnapshotAsync(cancellationToken).ConfigureAwait(false));
    }

    public sealed class CommunityMarketSquareQueryUseCase
    {
        private readonly ICommunityMarketSquareRepository repository;
        public CommunityMarketSquareQueryUseCase(ICommunityMarketSquareRepository repository) => this.repository = repository;
        public Task<CommunityMarketSquareSnapshot> 실행Async(CancellationToken cancellationToken = default)
            => repository.조회Async(cancellationToken);
    }

    public sealed class CommunityMarketSquareChangeSet
    {
        public CommunitySquareWorldItem[] Added { get; set; } = Array.Empty<CommunitySquareWorldItem>();
        public CommunitySquareWorldItem[] Updated { get; set; } = Array.Empty<CommunitySquareWorldItem>();
        public CommunitySquareWorldItem[] Removed { get; set; } = Array.Empty<CommunitySquareWorldItem>();
        public CommunitySquareWorldItem[] Unchanged { get; set; } = Array.Empty<CommunitySquareWorldItem>();
    }

    public sealed class CommunityMarketSquareReconciler
    {
        private static readonly StableIdReconciler<CommunitySquareWorldItem> Reconciler =
            new StableIdReconciler<CommunitySquareWorldItem>(
                new StableIdReconciliationPolicy<CommunitySquareWorldItem>(
                    item => item.StableId,
                    presentationEquivalent: Equivalent));

        public CommunityMarketSquareChangeSet Reconcile(IReadOnlyList<CommunitySquareWorldItem> current, IReadOnlyList<CommunitySquareWorldItem> incoming)
        {
            try
            {
                var changes = Reconciler.Reconcile(current, incoming);
                return new CommunityMarketSquareChangeSet
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
                throw new InvalidOperationException("CommunitySquareSnapshotInvalid", error);
            }
        }

        private static bool Equivalent(CommunitySquareWorldItem left, CommunitySquareWorldItem right)
            => left.Kind == right.Kind && left.Title == right.Title && left.Summary == right.Summary
               && left.Status == right.Status && left.DetailHref == right.DetailHref
               && left.Count == right.Count && left.AsOfUtc == right.AsOfUtc;
    }

    public sealed class CommunityMarketSquareLoadResult
    {
        public string StateCode { get; set; } = CommunityMarketSquareLoadStateCodes.Idle;
        public CommunityMarketSquareSnapshot? Snapshot { get; set; }
        public CommunityMarketSquareChangeSet? Changes { get; set; }
        public Exception? Error { get; set; }
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.UnityResilientWorldLoad,
        SsalddelCodeLayer.ClientAdapter,
        "커뮤니티 광장 Snapshot 조회와 마지막 성공 상태 조정을 연결한다.",
        StepKey = "client.community-load",
        DependsOnStepKeys = new string[] { "client.last-successful-runtime" },
        ExecutionStage = SsalddelCodeExecutionStage.Query,
        Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.UiStateMutation,
        ReadsFrom = SsalddelCodeDataScope.OperationalState,
        WritesTo = SsalddelCodeDataScope.ClientPresentation,
        FlowOrder = 20,
        Boundary = "공개 Projection만 읽고 커뮤니티 원장이나 서버 개정을 변경하지 않는다.")]
    public sealed class CommunityMarketSquareLoadCoordinator
    {
        private readonly CommunityMarketSquareQueryUseCase query;
        private readonly CommunityMarketSquareReconciler reconciler;
        private readonly LastSuccessfulLoadRuntime<CommunityMarketSquareSnapshot,
            CommunityMarketSquareChangeSet> runtime = new();

        public CommunityMarketSquareLoadCoordinator(CommunityMarketSquareQueryUseCase query, CommunityMarketSquareReconciler reconciler)
        {
            this.query = query ?? throw new ArgumentNullException(nameof(query));
            this.reconciler = reconciler ?? throw new ArgumentNullException(nameof(reconciler));
        }

        public async Task<CommunityMarketSquareLoadResult> LoadAsync(CancellationToken cancellationToken = default)
        {
            var result = await runtime.LoadAsync(
                token => query.실행Async(token),
                (previous, snapshot) => reconciler.Reconcile(
                    previous?.Items ?? Array.Empty<CommunitySquareWorldItem>(),
                    snapshot.Items),
                cancellationToken).ConfigureAwait(false);
            return new CommunityMarketSquareLoadResult
            {
                StateCode = result.StateCode switch
                {
                    ZoneRuntimeStateCode.Ready => CommunityMarketSquareLoadStateCodes.Success,
                    ZoneRuntimeStateCode.RefreshError => CommunityMarketSquareLoadStateCodes.RefreshError,
                    ZoneRuntimeStateCode.InitialError => CommunityMarketSquareLoadStateCodes.InitialLoadError,
                    _ => CommunityMarketSquareLoadStateCodes.Loading,
                },
                Snapshot = result.Snapshot,
                Changes = result.Changes,
                Error = result.Error,
            };
        }
    }
}
