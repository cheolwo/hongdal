using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티세계지도질문UseCaseTests
{
    [Fact]
    public async Task 질문초안은_공개근거를구조화하지만_게시글이나가원장을저장하지않는다()
    {
        await using var database = await TestDatabase.CreateAsync();
        var useCase = CreateUseCase(database.Context);

        var draft = await useCase.초안생성Async(
            "kosis:cpi:kr",
            new 커뮤니티세계지도질문초안Request
            {
                DatasetCode = CommunityPageRoutes.WorldMapDayWorkDataset
            });

        Assert.NotNull(draft);
        Assert.Equal("kosis:cpi:kr", draft.Evidence.ObservationStableId);
        Assert.Equal("snapshot-revision-1", draft.Evidence.SnapshotRevision);
        Assert.Equal("source-version-1", draft.Evidence.SourceVersion);
        Assert.Contains("observation=kosis%3Acpi%3Akr", draft.Evidence.MapHref, StringComparison.Ordinal);
        Assert.Contains("snapshot=snapshot-revision-1", draft.Evidence.MapHref, StringComparison.Ordinal);
        Assert.Contains("sourceVersion=source-version-1", draft.Evidence.MapHref, StringComparison.Ordinal);
        Assert.Contains("국가통계포털", draft.SuggestedPost.Body, StringComparison.Ordinal);
        Assert.True(draft.RequiresUserConfirmation);
        Assert.False(draft.CreatesPost);
        Assert.False(draft.CreatesProvisionalLedger);
        Assert.Empty(await database.Context.PlatformCommunityPosts.ToListAsync());
    }

    [Fact]
    public async Task 출처확인없는질문게시요청은_저장하지않는다()
    {
        await using var database = await TestDatabase.CreateAsync();
        var useCase = CreateUseCase(database.Context);

        var result = await useCase.게시Async(
            "kosis:cpi:kr",
            PublishRequest(confirmSourceReference: false));

        Assert.True(result.IsFailed);
        Assert.Contains("출처", result.Errors.Single().Message, StringComparison.Ordinal);
        Assert.Empty(await database.Context.PlatformCommunityPosts.ToListAsync());
    }

    [Fact]
    public async Task 확인게시후_observation과snapshotRevision을영속하고_같은게시글을재조회한다()
    {
        await using var database = await TestDatabase.CreateAsync();
        var useCase = CreateUseCase(database.Context);

        var result = await useCase.게시Async(
            "kosis:cpi:kr",
            PublishRequest(confirmSourceReference: true));

        Assert.True(result.IsSuccess, string.Join(" | ", result.Errors.Select(error => error.Message)));
        Assert.False(result.Value.ProvisionalLedgerCreated);
        Assert.Equal("kosis:cpi:kr", result.Value.Post.SourceEvidence?.ObservationStableId);
        Assert.Equal("snapshot-revision-1", result.Value.Post.SourceEvidence?.SnapshotRevision);
        Assert.Equal("source-version-1", result.Value.Post.SourceEvidence?.SourceVersion);
        Assert.True(result.Value.Post.IsInterestGatheringEnabled);
        Assert.Equal(CommunityBoardCatalog.Participation.DisplayName, result.Value.Post.Category);
        Assert.Null(result.Value.Post.커뮤니티원장Id);

        database.Context.ChangeTracker.Clear();
        var stored = await database.Context.PlatformCommunityPosts
            .SingleAsync(post => post.Id == result.Value.Post.Id);
        Assert.Equal("kosis:cpi:kr", stored.SourceObservationStableId);
        Assert.Equal(CommunityPageRoutes.WorldMapDayWorkDataset, stored.SourceDatasetCode);
        Assert.Equal("snapshot-revision-1", stored.SourceSnapshotRevision);
        Assert.Contains("kosis-consumer-price-index", stored.SourceEvidenceJson, StringComparison.Ordinal);
        Assert.Contains("source-version-1", stored.SourceEvidenceJson, StringComparison.Ordinal);
        Assert.Contains("/community/home?country=KR", stored.SourceEvidenceJson, StringComparison.Ordinal);
        Assert.Null(stored.커뮤니티원장Id);
    }

    [Fact]
    public async Task 다른dataset의동일하지않은Observation은_게시하지않는다()
    {
        await using var database = await TestDatabase.CreateAsync();
        var useCase = CreateUseCase(database.Context);
        var request = PublishRequest(confirmSourceReference: true);
        request.DatasetCode = CommunityPageRoutes.WorldMapNightLearningDataset;

        var result = await useCase.게시Async("kosis:cpi:kr", request);

        Assert.True(result.IsFailed);
        Assert.Equal(404, result.Errors.Single().Metadata["StatusCode"]);
        Assert.Empty(await database.Context.PlatformCommunityPosts.ToListAsync());
    }

    private static 커뮤니티세계지도질문UseCase CreateUseCase(SsalddelContext db)
        => new(
            new FakeMapUseCase(),
            new 커뮤니티게시글생성Service(
                db,
                new 커뮤니티게시글음성작업예약Service(),
                new CommunityKeywordNotificationQueue(),
                null!,
                null!,
                new AllowAllBoardWritePolicy(),
                new TestUserAccessor(),
                new RecordingPublisher(),
                NullLogger<커뮤니티게시글생성Service>.Instance));

    private static 커뮤니티세계지도질문게시Request PublishRequest(bool confirmSourceReference)
        => new()
        {
            DatasetCode = CommunityPageRoutes.WorldMapDayWorkDataset,
            Title = "소비자물가 공개 근거를 함께 확인해요",
            Body = "지역 생활과 공동행동에 어떤 의미가 있는지 질문합니다.",
            Nickname = "근거를 살피는 이웃",
            Password = "post-password",
            IsInterestGatheringEnabled = true,
            ConfirmSourceReference = confirmSourceReference
        };

    private sealed class FakeMapUseCase : I커뮤니티세계지도조회UseCase
    {
        public Task<커뮤니티세계지도SnapshotDto> 조회Async(
            string? datasetCode,
            CancellationToken cancellationToken = default)
        {
            var normalized = string.IsNullOrWhiteSpace(datasetCode)
                ? CommunityPageRoutes.WorldMapDayWorkDataset
                : datasetCode.Trim();
            IReadOnlyList<커뮤니티세계지도ObservationDto> observations = string.Equals(
                normalized,
                CommunityPageRoutes.WorldMapDayWorkDataset,
                StringComparison.Ordinal)
                ? [Observation()]
                : [];
            return Task.FromResult(new 커뮤니티세계지도SnapshotDto(
                normalized,
                "snapshot-revision-1",
                DateTimeOffset.Parse("2026-08-04T00:00:00Z"),
                [],
                observations));
        }

        private static 커뮤니티세계지도ObservationDto Observation()
            => new(
                "kosis:cpi:kr",
                CommunityPageRoutes.WorldMapDayWorkDataset,
                커뮤니티세계지도LayerCodes.KosisStatisticalContext,
                "KR",
                "대한민국",
                36.5,
                127.8,
                "소비자물가지수",
                "전국 월별 소비자물가지수의 공개 통계 맥락입니다.",
                "국가통계포털 KOSIS",
                DateTimeOffset.Parse("2026-07-01T00:00:00Z"),
                "official",
                "/information/public-data/kosis",
                SourceHref: "https://kosis.kr",
                LocationPrecisionCode: 커뮤니티세계지도위치정밀도Codes.CountryRepresentative,
                SourceDatasetKey: "kosis-consumer-price-index",
                CollectedAtUtc: DateTimeOffset.Parse("2026-08-03T00:00:00Z"),
                UpdateCycle: "월별",
                BoundaryNotice: "개별 상품 가격·재고·계약 조건을 뜻하지 않습니다.",
                SourceVersion: "source-version-1");
    }

    private sealed class AllowAllBoardWritePolicy : ICommunityBoardWritePolicy
    {
        public Task<bool> CanWriteAsync(
            string? appKey,
            string? category,
            string? userId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class TestUserAccessor : ICurrentUserAccessor
    {
        public string? UserId => "user-1";

        public string? Role => "CommunityMember";
    }

    private sealed class RecordingPublisher : IPublisher
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;
    }

    private sealed class TestDatabase : IAsyncDisposable
    {
        private TestDatabase(SqliteConnection connection, SsalddelContext context)
        {
            Connection = connection;
            Context = context;
        }

        private SqliteConnection Connection { get; }

        public SsalddelContext Context { get; }

        public static async Task<TestDatabase> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<SsalddelContext>()
                .UseSqlite(connection)
                .Options;
            var context = new SsalddelContext(options, new DummyPersonalDataEncryptionService());
            await context.Database.EnsureCreatedAsync();
            return new TestDatabase(connection, context);
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await Connection.DisposeAsync();
        }
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
