using System.Security.Cryptography;
using System.Text;
using FluentResults;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Services.Community;

public interface I커뮤니티시장광장조회UseCase
{
    Task<Result<CommunityMarketSquareSnapshotResponse>> 조회Async(
        string? appKey,
        CancellationToken cancellationToken = default);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Content,
    SsalddelModuleKind.Application,
    "공개 커뮤니티 정보를 Unity 광장용 read-only snapshot으로 압축",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "기존 공개 조회 결과만 결합하며 원장 실행 권한이나 개인정보를 확대하지 않는다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Application,
    "게시판·게시글·비식별 활동 신호를 커뮤니티 시장 광장 aggregate로 구성한다.",
    Effects = SsalddelCodeEffect.PersistentRead,
    ContractType = typeof(CommunityMarketSquareSnapshotResponse),
    FlowOrder = 20)]
public sealed class 커뮤니티시장광장조회UseCase(
    I커뮤니티게시글조회UseCase postReader,
    I커뮤니티활동신호UseCase activityReader)
    : I커뮤니티시장광장조회UseCase
{
    private const int MaximumItems = 12;

    public async Task<Result<CommunityMarketSquareSnapshotResponse>> 조회Async(
        string? appKey,
        CancellationToken cancellationToken = default)
    {
        var boardsResult = await postReader.게시판요약목록Async(appKey, cancellationToken);
        if (boardsResult.IsFailed)
        {
            return Result.Fail<CommunityMarketSquareSnapshotResponse>(
                string.Join(", ", boardsResult.Errors.Select(error => error.Message)));
        }

        var postsResult = await postReader.목록Async(
            appKey, null, null, null, null, 1, MaximumItems, cancellationToken);
        if (postsResult.IsFailed)
        {
            return Result.Fail<CommunityMarketSquareSnapshotResponse>(
                string.Join(", ", postsResult.Errors.Select(error => error.Message)));
        }

        var activityResult = await activityReader.조회Async(
            new CommunityActivitySignalQuery
            {
                AppKey = appKey,
                Page = 1,
                PageSize = MaximumItems,
            },
            cancellationToken);
        if (activityResult.IsFailed)
        {
            return Result.Fail<CommunityMarketSquareSnapshotResponse>(
                string.Join(", ", activityResult.Errors.Select(error => error.Message)));
        }

        var boards = boardsResult.Value.Select(MapBoard).ToArray();
        var posts = postsResult.Value.Items.Select(MapPost).ToArray();
        var activities = activityResult.Value.Items.Select(MapActivity).ToArray();
        var ledgers = postsResult.Value.Items
            .Where(post => post.원장Context is not null)
            .Select(MapLedger)
            .ToArray();
        var generatedAt = DateTimeOffset.UtcNow;

        return Result.Ok(new CommunityMarketSquareSnapshotResponse
        {
            Revision = ComputeRevision(boards, posts, activities, ledgers),
            GeneratedAtUtc = generatedAt,
            Boards = boards,
            Posts = posts,
            ActivitySignals = activities,
            Ledgers = ledgers,
        });
    }

    private static CommunitySquareBoardResponse MapBoard(CommunityBoardSummaryResponse source)
        => new()
        {
            StableId = "community-board:" + source.BoardKey,
            BoardKey = source.BoardKey,
            DisplayName = source.DisplayName,
            Description = source.Description,
            GroupDisplayName = source.GroupDisplayName,
            PostingAccessCode = source.PostingAccessCode,
            PostCount = source.PostCount,
            LatestPostAtUtc = AsUtc(source.LatestPostAtUtc),
        };

    private static CommunitySquarePostResponse MapPost(PlatformCommunityPostResponse source)
        => new()
        {
            StableId = $"community-post:{source.Id}",
            PostId = source.Id,
            Category = source.Category,
            WorkflowTag = source.WorkflowTag,
            RoleTag = source.RoleTag,
            Title = source.Title,
            Excerpt = Excerpt(source.Body),
            TopicClassificationCode = source.TopicClassificationCode,
            IsSystemGenerated = source.IsSystemGenerated,
            IsInterestGatheringEnabled = source.IsInterestGatheringEnabled,
            RecommendationCount = source.RecommendationCount,
            CommentCount = source.CommentCount,
            PublishedAtUtc = AsUtc(source.PublishedAtUtc ?? source.CreatedAtUtc),
            DetailHref = $"/community/posts/{source.Id}",
        };

    private static CommunitySquareActivityResponse MapActivity(CommunityActivitySignalResponse source)
        => new()
        {
            StableId = "community-activity:" + source.SignalId,
            CommunityScope = source.CommunityScope,
            ActivityKind = source.ActivityKind,
            Title = source.Title,
            Summary = source.Summary,
            TimeBucketLabel = source.TimeBucketLabel,
            TimePrecision = source.TimePrecision,
            AggregationCount = source.AggregationCount,
            OccurredAtUtc = AsUtc(source.OccurredAtUtc),
            PrivacyPolicyVersion = source.PrivacyPolicyVersion,
        };

    private static CommunitySquareLedgerResponse MapLedger(PlatformCommunityPostResponse source)
    {
        var ledger = source.원장Context!;
        return new CommunitySquareLedgerResponse
        {
            StableId = $"community-ledger-summary:post-{source.Id}",
            SourcePostStableId = $"community-post:{source.Id}",
            TemplateName = ledger.원장템플릿명,
            Title = ledger.제목,
            State = ledger.상태,
            CurrentStage = ledger.현재단계,
            DetailAvailable = ledger.상세조회가능여부,
            DetailHref = ledger.상세조회가능여부
                ? $"/community/posts/{source.Id}"
                : string.Empty,
        };
    }

    private static string Excerpt(string? value)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= 240 ? normalized : normalized[..240] + "…";
    }

    private static DateTimeOffset AsUtc(DateTime value)
    {
        var utc = value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
        return new DateTimeOffset(utc);
    }

    private static DateTimeOffset? AsUtc(DateTime? value)
        => value.HasValue ? AsUtc(value.Value) : null;

    private static string ComputeRevision(
        IReadOnlyList<CommunitySquareBoardResponse> boards,
        IReadOnlyList<CommunitySquarePostResponse> posts,
        IReadOnlyList<CommunitySquareActivityResponse> activities,
        IReadOnlyList<CommunitySquareLedgerResponse> ledgers)
    {
        var values = boards.Select(item => $"{item.StableId}:{item.PostCount}:{item.LatestPostAtUtc:O}")
            .Concat(posts.Select(item => $"{item.StableId}:{item.PublishedAtUtc:O}:{item.CommentCount}:{item.RecommendationCount}"))
            .Concat(activities.Select(item => $"{item.StableId}:{item.OccurredAtUtc:O}:{item.AggregationCount}"))
            .Concat(ledgers.Select(item => $"{item.StableId}:{item.State}:{item.CurrentStage}"))
            .OrderBy(value => value, StringComparer.Ordinal);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", values))))
            .ToLowerInvariant();
    }
}
