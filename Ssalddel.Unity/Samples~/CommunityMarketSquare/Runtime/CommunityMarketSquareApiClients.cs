using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Community;
using UnityEngine;
using UnityEngine.Networking;

namespace Ssalddel.Unity.Samples.CommunityMarketSquare
{
    public sealed class CommunityMarketSquareApiOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 15;
    }

    public sealed class SimulatedCommunityMarketSquareApiClient : ICommunityMarketSquareApiClient
    {
        public Task<CommunityMarketSquareSnapshotApiModel> GetPublicSnapshotAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new CommunityMarketSquareSnapshotApiModel
            {
                StableId = "community-market-square:public",
                Revision = "simulation-community-square-1",
                GeneratedAtUtc = DateTimeOffset.Parse("2026-08-08T15:00:00+09:00"),
                Boards = new[]
                {
                    new CommunitySquareBoardApiModel { StableId = "community-board:regional-culture", BoardKey = "regional-culture", DisplayName = "지역문화", Description = "지역 이야기", PostingAccessCode = "authenticated", PostCount = 8 },
                    new CommunitySquareBoardApiModel { StableId = "community-board:sales-supply", BoardKey = "sales-supply", DisplayName = "판매·공급", Description = "공개 공급 이야기", PostingAccessCode = "authenticated", PostCount = 4 },
                },
                Posts = new[]
                {
                    new CommunitySquarePostApiModel { StableId = "community-post:101", PostId = 101, Category = "판매·공급", Title = "감자 공동 수요", Excerpt = "SIMULATED 공개 게시글", PublishedAtUtc = DateTimeOffset.Parse("2026-08-08T05:00:00Z"), DetailHref = "/community/posts/101", CommentCount = 2 },
                },
                ActivitySignals = new[]
                {
                    new CommunitySquareActivityApiModel { StableId = "community-activity:signal-1", CommunityScope = "CommunityTrust", ActivityKind = "InterestGathering", Title = "수요가 모이고 있습니다", Summary = "SIMULATED 비식별 집계", AggregationCount = 4, OccurredAtUtc = DateTimeOffset.Parse("2026-08-08T05:00:00Z"), PrivacyPolicyVersion = "fixture" },
                },
                Ledgers = new[]
                {
                    new CommunitySquareLedgerApiModel { StableId = "community-ledger-summary:post-101", SourcePostStableId = "community-post:101", TemplateName = "공동행동 준비", Title = "감자 수요 확인", State = "관심모집", CurrentStage = "수요확인", DetailAvailable = true, DetailHref = "/community/posts/101" },
                },
            });
        }
    }

    public sealed class OperationalCommunityMarketSquareApiClient : ICommunityMarketSquareApiClient
    {
        private readonly CommunityMarketSquareApiOptions options;
        public OperationalCommunityMarketSquareApiClient(CommunityMarketSquareApiOptions options) => this.options = options;

        public async Task<CommunityMarketSquareSnapshotApiModel> GetPublicSnapshotAsync(CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var baseUri))
                throw new InvalidOperationException("CommunitySquareApiBaseUrlInvalid");
            using (var request = UnityWebRequest.Get(new Uri(baseUri, CommunityMarketSquareApiRoutes.PublicSnapshot)))
            using (cancellationToken.Register(request.Abort))
            {
                request.timeout = Math.Max(1, options.TimeoutSeconds);
                request.SetRequestHeader("Accept", "application/json");
                var operation = request.SendWebRequest();
                while (!operation.isDone) { cancellationToken.ThrowIfCancellationRequested(); await Task.Yield(); }
                cancellationToken.ThrowIfCancellationRequested();
                if (request.result != UnityWebRequest.Result.Success)
                    throw new InvalidOperationException("CommunitySquareApiRequestFailed:" + request.responseCode);
                var wire = JsonUtility.FromJson<CommunityMarketSquareSnapshotWire>(request.downloadHandler.text);
                return wire?.ToApiModel() ?? throw new InvalidOperationException("CommunitySquareJsonInvalid");
            }
        }
    }

    [Serializable]
    internal sealed class CommunityMarketSquareSnapshotWire
    {
        public string stableId = string.Empty; public string revision = string.Empty; public string generatedAtUtc = string.Empty;
        public CommunitySquareBoardWire[] boards = Array.Empty<CommunitySquareBoardWire>();
        public CommunitySquarePostWire[] posts = Array.Empty<CommunitySquarePostWire>();
        public CommunitySquareActivityWire[] activitySignals = Array.Empty<CommunitySquareActivityWire>();
        public CommunitySquareLedgerWire[] ledgers = Array.Empty<CommunitySquareLedgerWire>();
        public CommunityMarketSquareSnapshotApiModel ToApiModel() => new CommunityMarketSquareSnapshotApiModel
        {
            StableId = stableId, Revision = revision, GeneratedAtUtc = ParseDate(generatedAtUtc),
            Boards = Array.ConvertAll(boards, value => value.ToApiModel()), Posts = Array.ConvertAll(posts, value => value.ToApiModel()),
            ActivitySignals = Array.ConvertAll(activitySignals, value => value.ToApiModel()), Ledgers = Array.ConvertAll(ledgers, value => value.ToApiModel()),
        };
        internal static DateTimeOffset ParseDate(string value)
        {
            if (!DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
                throw new InvalidOperationException("CommunitySquareDateInvalid");
            return result;
        }
    }

    [Serializable] internal sealed class CommunitySquareBoardWire
    {
        public string stableId = string.Empty, boardKey = string.Empty, displayName = string.Empty, description = string.Empty, groupDisplayName = string.Empty, postingAccessCode = string.Empty, latestPostAtUtc = string.Empty; public int postCount;
        public CommunitySquareBoardApiModel ToApiModel() => new CommunitySquareBoardApiModel { StableId = stableId, BoardKey = boardKey, DisplayName = displayName, Description = description, GroupDisplayName = groupDisplayName, PostingAccessCode = postingAccessCode, PostCount = postCount, LatestPostAtUtc = OptionalDate(latestPostAtUtc) };
        private static DateTimeOffset? OptionalDate(string value) => string.IsNullOrWhiteSpace(value) ? (DateTimeOffset?)null : CommunityMarketSquareSnapshotWire.ParseDate(value);
    }
    [Serializable] internal sealed class CommunitySquarePostWire
    {
        public string stableId = string.Empty, category = string.Empty, workflowTag = string.Empty, roleTag = string.Empty, title = string.Empty, excerpt = string.Empty, topicClassificationCode = string.Empty, publishedAtUtc = string.Empty, detailHref = string.Empty; public long postId; public bool isSystemGenerated, isInterestGatheringEnabled; public int recommendationCount, commentCount;
        public CommunitySquarePostApiModel ToApiModel() => new CommunitySquarePostApiModel { StableId = stableId, PostId = postId, Category = category, WorkflowTag = workflowTag, RoleTag = roleTag, Title = title, Excerpt = excerpt, TopicClassificationCode = topicClassificationCode, IsSystemGenerated = isSystemGenerated, IsInterestGatheringEnabled = isInterestGatheringEnabled, RecommendationCount = recommendationCount, CommentCount = commentCount, PublishedAtUtc = CommunityMarketSquareSnapshotWire.ParseDate(publishedAtUtc), DetailHref = detailHref };
    }
    [Serializable] internal sealed class CommunitySquareActivityWire
    {
        public string stableId = string.Empty, communityScope = string.Empty, activityKind = string.Empty, title = string.Empty, summary = string.Empty, timeBucketLabel = string.Empty, timePrecision = string.Empty, occurredAtUtc = string.Empty, privacyPolicyVersion = string.Empty; public int aggregationCount;
        public CommunitySquareActivityApiModel ToApiModel() => new CommunitySquareActivityApiModel { StableId = stableId, CommunityScope = communityScope, ActivityKind = activityKind, Title = title, Summary = summary, TimeBucketLabel = timeBucketLabel, TimePrecision = timePrecision, AggregationCount = aggregationCount, OccurredAtUtc = CommunityMarketSquareSnapshotWire.ParseDate(occurredAtUtc), PrivacyPolicyVersion = privacyPolicyVersion };
    }
    [Serializable] internal sealed class CommunitySquareLedgerWire
    {
        public string stableId = string.Empty, sourcePostStableId = string.Empty, templateName = string.Empty, title = string.Empty, state = string.Empty, currentStage = string.Empty, detailHref = string.Empty; public bool detailAvailable;
        public CommunitySquareLedgerApiModel ToApiModel() => new CommunitySquareLedgerApiModel { StableId = stableId, SourcePostStableId = sourcePostStableId, TemplateName = templateName, Title = title, State = state, CurrentStage = currentStage, DetailAvailable = detailAvailable, DetailHref = detailHref };
    }
}
