using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.Community;
using Ssalddel.Services.Community;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityPostViewCountUseCaseTests
{
    [Fact]
    public async Task 공개_게시글_상세_조회는_요청마다_조회수를_원자적으로_증가시킨다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var post = CreatePost();
        post.ViewCount = 7;
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();
        var useCase = new 커뮤니티게시글조회수기록UseCase(db);

        Assert.True(await useCase.조회기록Async(post.Id, CancellationToken.None));
        Assert.True(await useCase.조회기록Async(post.Id, CancellationToken.None));

        db.ChangeTracker.Clear();
        Assert.Equal(
            9,
            await db.PlatformCommunityPosts
                .Where(candidate => candidate.Id == post.Id)
                .Select(candidate => candidate.ViewCount)
                .SingleAsync());
    }

    [Fact]
    public async Task 존재하지_않거나_비공개인_게시글은_조회수를_기록하지_않는다()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = CreateContext(connection);
        await db.Database.EnsureCreatedAsync();

        var scheduled = CreatePost();
        scheduled.PublicationStatusCode = PlatformCommunityPostPublicationStatusCodes.Scheduled;
        scheduled.PublishedAtUtc = null;
        db.PlatformCommunityPosts.Add(scheduled);
        await db.SaveChangesAsync();
        var useCase = new 커뮤니티게시글조회수기록UseCase(db);

        Assert.False(await useCase.조회기록Async(999, CancellationToken.None));
        Assert.False(await useCase.조회기록Async(scheduled.Id, CancellationToken.None));

        db.ChangeTracker.Clear();
        Assert.Equal(
            0,
            await db.PlatformCommunityPosts
                .IgnoreQueryFilters()
                .Where(candidate => candidate.Id == scheduled.Id)
                .Select(candidate => candidate.ViewCount)
                .SingleAsync());
    }

    private static PlatformCommunityPost CreatePost()
        => new()
        {
            AppKey = "platform",
            Category = "자유·생활",
            WorkflowTag = "커뮤니티 신뢰",
            RoleTag = "구성원",
            Title = "조회수 테스트",
            Body = "상세 조회마다 조회수를 기록합니다.",
            Nickname = "작성자",
            PasswordHash = "hash",
            PublicationStatusCode = PlatformCommunityPostPublicationStatusCodes.Published,
            PublishedAtUtc = DateTime.UtcNow
        };

    private static SsalddelContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseSqlite(connection)
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
