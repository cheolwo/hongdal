using System.Reflection;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Controllers.Common;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티시장광장조회UseCaseTests
{
    [Fact]
    public async Task 공개_자료만_광장_snapshot으로_압축한다()
    {
        var activityReader = new 활동신호조회Fake();
        var useCase = new 커뮤니티시장광장조회UseCase(
            new 게시글조회Fake(),
            activityReader);

        var result = await useCase.조회Async("platform");

        Assert.True(result.IsSuccess);
        var snapshot = result.Value;
        Assert.Equal("community-market-square:public", snapshot.StableId);
        Assert.Equal(64, snapshot.Revision.Length);
        Assert.Single(snapshot.Boards);
        Assert.Single(snapshot.Posts);
        Assert.Single(snapshot.ActivitySignals);
        Assert.Single(snapshot.Ledgers);
        Assert.Equal("community-post:101", snapshot.Posts[0].StableId);
        Assert.Equal(241, snapshot.Posts[0].Excerpt.Length);
        Assert.EndsWith("…", snapshot.Posts[0].Excerpt);
        Assert.Equal(snapshot.Posts[0].StableId, snapshot.Ledgers[0].SourcePostStableId);
        Assert.Equal("/community/posts/101", snapshot.Ledgers[0].DetailHref);
        Assert.Equal("platform", activityReader.LastQuery?.AppKey);
    }

    [Fact]
    public void 공개_contract는_개인정보와_원장_실행정보를_노출하지_않는다()
    {
        var propertyNames = typeof(CommunityMarketSquareSnapshotResponse).Assembly
            .GetTypes()
            .Where(type => type.Namespace == typeof(CommunityMarketSquareSnapshotResponse).Namespace)
            .Where(type => type.Name.StartsWith("CommunitySquare", StringComparison.Ordinal))
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.DoesNotContain("Nickname", propertyNames);
        Assert.DoesNotContain("UserId", propertyNames);
        Assert.DoesNotContain("Contact", propertyNames);
        Assert.DoesNotContain("원장Id", propertyNames);
        Assert.DoesNotContain("가능한행동목록", propertyNames);
        Assert.DoesNotContain("담당자목록", propertyNames);
    }

    [Fact]
    public void Controller는_고정된_공개_Get_route를_사용한다()
    {
        var type = typeof(커뮤니티시장광장Controller);

        Assert.NotNull(type.GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal(
            CommunityMarketSquareRoutes.PublicSnapshot,
            type.GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(type.GetMethod(nameof(커뮤니티시장광장Controller.조회))?
            .GetCustomAttribute<HttpGetAttribute>());
    }

    [Fact]
    public async Task 하위_공개조회가_실패하면_snapshot을_만들지_않는다()
    {
        var useCase = new 커뮤니티시장광장조회UseCase(
            new 게시글조회Fake(failBoards: true),
            new 활동신호조회Fake());

        var result = await useCase.조회Async(null);

        Assert.True(result.IsFailed);
        Assert.Contains("board-read-failed", result.Errors[0].Message);
    }

    private sealed class 게시글조회Fake(bool failBoards = false) : I커뮤니티게시글조회UseCase
    {
        public Task<Result<PlatformCommunityPostListResponse>> 목록Async(
            string? appKey,
            string? category,
            string? boardKey,
            string? workflowTag,
            string? roleTag,
            int page,
            int pageSize,
            CancellationToken cancellationToken,
            string? periodicVisibility = null)
            => Task.FromResult(Result.Ok(new PlatformCommunityPostListResponse
            {
                Items =
                [
                    new PlatformCommunityPostResponse
                    {
                        Id = 101,
                        Category = "판매·공급",
                        WorkflowTag = "market",
                        RoleTag = "producer",
                        Title = "감자 공동 수요를 확인합니다",
                        Body = new string('가', 300),
                        Nickname = "공개 계약에 복사하면 안 되는 작성자",
                        PublishedAtUtc = new DateTime(2026, 8, 8, 1, 0, 0, DateTimeKind.Utc),
                        IsInterestGatheringEnabled = true,
                        RecommendationCount = 3,
                        CommentCount = 2,
                        원장Context = new PlatformCommunityPostLedgerContextResponse
                        {
                            원장Id = "private-ledger-id",
                            원장템플릿명 = "공동행동 준비",
                            제목 = "감자 수요 확인",
                            상태 = "관심모집",
                            현재단계 = "수요확인",
                            상세조회가능여부 = true,
                            가능한행동목록 = ["execute-private-action"],
                            블록목록 =
                            [
                                new PlatformCommunityLedgerBlockResponse
                                {
                                    블록Id = "private-block",
                                    담당자목록 =
                                    [
                                        new PlatformCommunityLedgerBlockAssigneeResponse
                                        {
                                            UserId = "private-user",
                                            DisplayName = "비공개 담당자",
                                        },
                                    ],
                                },
                            ],
                        },
                    },
                ],
                TotalCount = 1,
                Page = 1,
                PageSize = 12,
            }));

        public Task<Result<IReadOnlyList<CommunityBoardSummaryResponse>>> 게시판요약목록Async(
            string? appKey,
            CancellationToken cancellationToken)
            => Task.FromResult(failBoards
                ? Result.Fail<IReadOnlyList<CommunityBoardSummaryResponse>>("board-read-failed")
                : Result.Ok<IReadOnlyList<CommunityBoardSummaryResponse>>(
                [
                    new CommunityBoardSummaryResponse
                    {
                        BoardKey = "sales-supply",
                        DisplayName = "판매·공급",
                        Description = "공개 판매와 공급 이야기",
                        GroupDisplayName = "공동행동",
                        PostingAccessCode = "authenticated",
                        PostCount = 1,
                        LatestPostAtUtc = new DateTime(2026, 8, 8, 1, 0, 0, DateTimeKind.Utc),
                    },
                ]));

        public Task<Result<PlatformCommunityPostResponse>> 상세Async(
            long id,
            CancellationToken cancellationToken)
            => Task.FromResult(Result.Fail<PlatformCommunityPostResponse>("unused"));
    }

    private sealed class 활동신호조회Fake : I커뮤니티활동신호UseCase
    {
        public CommunityActivitySignalQuery? LastQuery { get; private set; }

        public Task<Result<CommunityActivitySignalListResponse>> 조회Async(
            CommunityActivitySignalQuery? query,
            CancellationToken cancellationToken)
        {
            LastQuery = query;
            return Task.FromResult(Result.Ok(new CommunityActivitySignalListResponse
            {
                Items =
                [
                    new CommunityActivitySignalResponse
                    {
                        SignalId = "signal-1",
                        CommunityScope = CommunityActivityScopes.CommunityTrust,
                        ActivityKind = "InterestGathering",
                        Title = "공동 수요가 모이고 있습니다",
                        Summary = "개인을 식별하지 않는 집계 신호",
                        TimeBucketLabel = "오늘 오전",
                        TimePrecision = "bucket",
                        AggregationCount = 4,
                        OccurredAtUtc = new DateTime(2026, 8, 8, 1, 0, 0, DateTimeKind.Utc),
                        PrivacyPolicyVersion = "v1",
                    },
                ],
                Page = 1,
                PageSize = 12,
                TotalCount = 1,
            }));
        }
    }
}
