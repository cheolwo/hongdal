using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Data;

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
    }

    public sealed class CommunityMarketSquareMapper
    {
        public CommunityMarketSquareSnapshot Map(CommunityMarketSquareSnapshotApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            RequireStableId(source.StableId);
            Require(source.Revision, "CommunitySquareRevisionMissing");
            if (source.GeneratedAtUtc == default) throw new InvalidOperationException("CommunitySquareGeneratedAtMissing");
            if (source.Boards == null || source.Posts == null || source.ActivitySignals == null || source.Ledgers == null)
                throw new InvalidOperationException("CommunitySquareCollectionsMissing");

            RejectNullOrDuplicate(source.Boards.Select(item => item?.StableId), "CommunitySquareBoard");
            RejectNullOrDuplicate(source.Posts.Select(item => item?.StableId), "CommunitySquarePost");
            RejectNullOrDuplicate(source.ActivitySignals.Select(item => item?.StableId), "CommunitySquareActivity");
            RejectNullOrDuplicate(source.Ledgers.Select(item => item?.StableId), "CommunitySquareLedger");

            var postIds = new HashSet<string>(source.Posts.Select(item => item.StableId), StringComparer.Ordinal);
            foreach (var ledger in source.Ledgers)
            {
                if (!postIds.Contains(ledger.SourcePostStableId))
                    throw new InvalidOperationException("CommunitySquareLedgerPostUnknown:" + ledger.StableId);
                if (!ledger.DetailAvailable && !string.IsNullOrWhiteSpace(ledger.DetailHref))
                    throw new InvalidOperationException("CommunitySquareLedgerDetailScopeInvalid:" + ledger.StableId);
            }

            var items = source.Boards.Select(item => Item(item.StableId, "Board", item.DisplayName, item.Description, item.PostingAccessCode, string.Empty, item.PostCount, item.LatestPostAtUtc))
                .Concat(source.Posts.Select(item => Item(item.StableId, "Post", item.Title, item.Excerpt, item.Category, item.DetailHref, item.CommentCount, item.PublishedAtUtc)))
                .Concat(source.ActivitySignals.Select(item => Item(item.StableId, "Activity", item.Title, item.Summary, item.ActivityKind, string.Empty, item.AggregationCount, item.OccurredAtUtc)))
                .Concat(source.Ledgers.Select(item => Item(item.StableId, "Ledger", item.Title, item.TemplateName, item.State + "/" + item.CurrentStage, item.DetailAvailable ? item.DetailHref : string.Empty, 0, null)))
                .ToArray();
            RejectNullOrDuplicate(items.Select(item => item.StableId), "CommunitySquareWorldItem");

            return new CommunityMarketSquareSnapshot
            {
                StableId = source.StableId.Trim(),
                Revision = source.Revision.Trim(),
                GeneratedAtUtc = source.GeneratedAtUtc,
                Items = items,
            };
        }

        private static CommunitySquareWorldItem Item(string id, string kind, string title, string summary, string status, string href, int count, DateTimeOffset? asOf)
        {
            RequireStableId(id);
            Require(title, "CommunitySquareTitleMissing:" + id);
            return new CommunitySquareWorldItem
            {
                StableId = id.Trim(), Kind = kind, Title = title.Trim(),
                Summary = summary?.Trim() ?? string.Empty, Status = status?.Trim() ?? string.Empty,
                DetailHref = href?.Trim() ?? string.Empty, Count = count, AsOfUtc = asOf,
            };
        }

        private static void RejectNullOrDuplicate(IEnumerable<string?> values, string kind)
        {
            if (values.Any(value => string.IsNullOrWhiteSpace(value))) throw new InvalidOperationException(kind + "StableIdMissing");
            var duplicate = values.Where(value => value != null)
                .GroupBy(value => value!, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null) throw new InvalidOperationException("Duplicate" + kind + ":" + duplicate.Key);
        }

        private static void RequireStableId(string value)
        {
            if (!StableDataId.IsValid(value)) throw new InvalidOperationException("CommunitySquareStableIdInvalid:" + value);
        }

        private static void Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(error);
        }
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
        public CommunityMarketSquareChangeSet Reconcile(IReadOnlyList<CommunitySquareWorldItem> current, IReadOnlyList<CommunitySquareWorldItem> incoming)
        {
            var before = Index(current); var after = Index(incoming);
            var added = new List<CommunitySquareWorldItem>(); var updated = new List<CommunitySquareWorldItem>(); var unchanged = new List<CommunitySquareWorldItem>();
            foreach (var pair in after)
            {
                if (!before.TryGetValue(pair.Key, out var old)) added.Add(pair.Value);
                else if (Equivalent(old, pair.Value)) unchanged.Add(old);
                else updated.Add(pair.Value);
            }
            return new CommunityMarketSquareChangeSet
            {
                Added = added.ToArray(), Updated = updated.ToArray(), Unchanged = unchanged.ToArray(),
                Removed = before.Where(pair => !after.ContainsKey(pair.Key)).Select(pair => pair.Value).ToArray(),
            };
        }

        private static Dictionary<string, CommunitySquareWorldItem> Index(IReadOnlyList<CommunitySquareWorldItem> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var result = new Dictionary<string, CommunitySquareWorldItem>(StringComparer.Ordinal);
            foreach (var item in values) if (item == null || !result.TryAdd(item.StableId, item)) throw new InvalidOperationException("CommunitySquareSnapshotInvalid");
            return result;
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

    public sealed class CommunityMarketSquareLoadCoordinator
    {
        private readonly CommunityMarketSquareQueryUseCase query;
        private readonly CommunityMarketSquareReconciler reconciler;
        private CommunityMarketSquareSnapshot? lastSuccessful;

        public CommunityMarketSquareLoadCoordinator(CommunityMarketSquareQueryUseCase query, CommunityMarketSquareReconciler reconciler)
        {
            this.query = query ?? throw new ArgumentNullException(nameof(query));
            this.reconciler = reconciler ?? throw new ArgumentNullException(nameof(reconciler));
        }

        public async Task<CommunityMarketSquareLoadResult> LoadAsync(CancellationToken cancellationToken = default)
        {
            var refreshing = lastSuccessful != null;
            try
            {
                var snapshot = await query.실행Async(cancellationToken).ConfigureAwait(false);
                var changes = reconciler.Reconcile(lastSuccessful?.Items ?? Array.Empty<CommunitySquareWorldItem>(), snapshot.Items);
                lastSuccessful = snapshot;
                return new CommunityMarketSquareLoadResult { StateCode = CommunityMarketSquareLoadStateCodes.Success, Snapshot = snapshot, Changes = changes };
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception exception)
            {
                return new CommunityMarketSquareLoadResult
                {
                    StateCode = refreshing ? CommunityMarketSquareLoadStateCodes.RefreshError : CommunityMarketSquareLoadStateCodes.InitialLoadError,
                    Snapshot = lastSuccessful, Error = exception,
                };
            }
        }
    }
}
