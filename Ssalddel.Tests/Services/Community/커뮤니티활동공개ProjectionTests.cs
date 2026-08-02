using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.Driver.Transport;
using Ssalddel.Community;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.Community;
using Ssalddel.Services.Community;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.도메인.설정;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티활동공개ProjectionTests
{
    [Fact]
    public async Task RecordAsync_DeduplicatesSameOccurrence()
    {
        await using var database = await TestDatabase.CreateAsync();
        var recorder = new 커뮤니티활동공개ProjectionRecorder(
            database.Context,
            TimeProvider.System);
        var definition = FindDefinition();
        var occurrence = CreateOccurrence(1);

        await recorder.RecordAsync(definition, occurrence);
        await recorder.RecordAsync(definition, occurrence);

        var projection = await database.Context.커뮤니티활동공개Projections.SingleAsync();
        Assert.Equal(1, projection.ActivityCount);
        Assert.Equal(1, await database.Context.커뮤니티활동처리기록.CountAsync());
    }

    [Fact]
    public async Task GetSignalsAsync_ExposesOnlyWeeklyAggregateAfterPrivacyThreshold()
    {
        await using var database = await TestDatabase.CreateAsync();
        var recorder = new 커뮤니티활동공개ProjectionRecorder(
            database.Context,
            TimeProvider.System);
        var service = new CommunityActivitySignalService(database.Context);
        var definition = FindDefinition();
        var query = new CommunityActivitySignalQuery
        {
            FromUtc = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc),
            ToUtc = new DateTime(2026, 7, 27, 0, 0, 0, DateTimeKind.Utc)
        };

        for (var index = 1; index < 커뮤니티활동공개Policy.최소공개활동수; index++)
        {
            await recorder.RecordAsync(definition, CreateOccurrence(index));
        }

        database.Context.사용자행위로그.Add(new 사용자행위로그
        {
            AppKey = "FDriverApp",
            UserId = "raw-user-secret",
            UserName = "실명 비공개",
            RoleName = "기사",
            EmailMasked = "private@example.com",
            PhoneLast4 = "1234",
            ActionType = "Update",
            ActionName = "민감한 원문 행위",
            Route = "/api/v1/driver/transports/private",
            TraceId = "raw-trace-secret",
            IsSuccess = true,
            ClientIp = "192.0.2.1",
            UserAgent = "private-agent",
            MetadataJson = "{\"address\":\"서울시 비공개 주소\"}",
            OccurredAtUtc = new DateTime(2026, 7, 23, 1, 0, 0, DateTimeKind.Utc)
        });
        await database.Context.SaveChangesAsync();

        var belowThreshold = await service.GetSignalsAsync(query, CancellationToken.None);
        Assert.Empty(belowThreshold.Items);

        await recorder.RecordAsync(
            definition,
            CreateOccurrence(커뮤니티활동공개Policy.최소공개활동수));

        var result = await service.GetSignalsAsync(query, CancellationToken.None);
        var signal = Assert.Single(result.Items);
        Assert.Equal(커뮤니티활동공개Policy.최소공개활동수, signal.AggregationCount);
        Assert.Equal(커뮤니티활동공개Policy.공개범위, signal.VisibilityScope);
        Assert.Equal(커뮤니티활동공개Policy.시간정밀도, signal.TimePrecision);
        Assert.Equal(new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc), signal.OccurredAtUtc);
        Assert.Equal("2026-07-20 주간", signal.TimeBucketLabel);
        Assert.DoesNotContain("raw-user-secret", signal.Summary);
        Assert.DoesNotContain("실명 비공개", signal.Summary);
        Assert.DoesNotContain("TR-PRIVATE", signal.Summary);
        Assert.DoesNotContain("서울시 비공개", signal.Summary);
        Assert.Empty(database.Context.PlatformCommunityPosts);

        var projection = await database.Context.커뮤니티활동공개Projections.SingleAsync();
        var persistedPublicText = string.Join(
            "\n",
            projection.AppKey,
            projection.CommunityScope,
            projection.ActivityKind,
            projection.Title,
            projection.PublicSummary,
            projection.TopicTagsJson);
        Assert.DoesNotContain("driver-secret", persistedPublicText);
        Assert.DoesNotContain("TR-PRIVATE", persistedPublicText);
        Assert.DoesNotContain("서울시 비공개", persistedPublicText);
        Assert.All(
            await database.Context.커뮤니티활동처리기록.ToArrayAsync(),
            receipt => Assert.DoesNotContain("secret", receipt.OccurrenceKey));
    }

    [Fact]
    public async Task RecordAsync_WhenDeliveryActivityReachesPrivacyThreshold_PublishesOneSafeBoardPostDraft()
    {
        await using var database = await TestDatabase.CreateAsync();
        var publisher = new RecordingAutomatedPostPublisher();
        var recorder = new 커뮤니티활동공개ProjectionRecorder(
            database.Context,
            TimeProvider.System,
            publisher);
        var definition = FindDefinition();

        for (var index = 1; index <= 커뮤니티활동공개Policy.최소공개활동수; index++)
        {
            await recorder.RecordAsync(definition, CreateOccurrence(index));
        }

        var draft = Assert.Single(publisher.Drafts);
        Assert.Equal(CommunityAutomatedPostSourceKeys.ActivityDigest, draft.SourceKey);
        Assert.Equal(definition.Board.DisplayName, draft.Category);
        Assert.Contains(definition.PublicActivitySummary, draft.Body);
        Assert.Contains("비식별 집계", draft.Body);
        Assert.DoesNotContain("driver-secret", draft.Body);
        Assert.DoesNotContain("TR-PRIVATE", draft.Body);
        Assert.DoesNotContain("서울시 비공개", draft.Body);

        await recorder.RecordAsync(definition, CreateOccurrence(6));

        Assert.Single(publisher.Drafts);
    }

    private static CommunityActivityBoardDefinition FindDefinition()
        => CommunityActivityBoardCatalog.FindSource(
            CommunityActivitySourceKinds.Event,
            nameof(운송상차완료됨Event))!;

    private static 운송상차완료됨Event CreateOccurrence(int index)
        => new(
            $"driver-secret-{index}",
            index,
            $"TR-PRIVATE-{index}",
            $"서울시 비공개 출발지 {index}",
            $"부산시 비공개 도착지 {index}",
            "배차완료",
            "상차완료",
            new DateTime(2026, 7, 23, 1, index, 0, DateTimeKind.Utc),
            $"trace-secret-{index}",
            null);

    private sealed class TestDatabase : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TestDatabase(SqliteConnection connection, SsalddelContext context)
        {
            _connection = connection;
            Context = context;
        }

        public SsalddelContext Context { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<SsalddelContext>()
                .UseSqlite(connection)
                .Options;
            var context = new SsalddelContext(
                options,
                new DummyPersonalDataEncryptionService());
            await context.Database.EnsureCreatedAsync();
            return new TestDatabase(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }

    private sealed class RecordingAutomatedPostPublisher : ICommunityAutomatedPostPublisher
    {
        public List<CommunityAutomatedPostDraft> Drafts { get; } = [];

        public Task<CommunityAutomatedPostPublishResult> PublishIfMissingAsync(
            CommunityAutomatedPostDraft draft,
            CancellationToken cancellationToken = default)
        {
            Drafts.Add(draft);
            return Task.FromResult(new CommunityAutomatedPostPublishResult(Drafts.Count, true));
        }
    }
}
