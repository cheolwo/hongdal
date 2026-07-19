using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Domain.Community;
using Ssalddel.Services.Community;
using Microsoft.EntityFrameworkCore;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityPostEngagementUseCaseTests
{
    [Fact]
    public async Task 같은_참여자의_추천은_한번만_집계된다()
    {
        await using var db = CreateContext();
        var post = CreatePost();
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();
        var useCase = new 커뮤니티게시글참여UseCase(db, new AnonymousUserAccessor());

        var first = await useCase.추천Async(post.Id, null, "client-1", CancellationToken.None);
        var second = await useCase.추천Async(post.Id, null, "client-1", CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(1, second.Value.RecommendationCount);
        Assert.Single(await db.PlatformCommunityPostRecommendations.ToListAsync());
    }

    [Fact]
    public async Task 익명_게시판_댓글은_게시판별_닉네임으로_작성되고_비밀번호로_삭제된다()
    {
        await using var db = CreateContext();
        var post = CreatePost();
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();
        var useCase = new 커뮤니티게시글참여UseCase(db, new AnonymousUserAccessor());

        var created = await useCase.댓글작성Async(
            post.Id,
            new PlatformCommunityPostCommentCreateRequest
            {
                Nickname = string.Empty,
                Password = "comment-password",
                Body = "함께 살펴보겠습니다."
            },
            CancellationToken.None);

        Assert.True(created.IsSuccess);
        Assert.StartsWith(
            CommunityAnonymousNicknameCatalog.ResolveBaseName(post.Category),
            created.Value.Nickname,
            StringComparison.Ordinal);
        Assert.Equal(1, post.CommentCount);

        var deleted = await useCase.댓글삭제Async(
            post.Id,
            created.Value.Id,
            new PlatformCommunityPostPasswordRequest { Password = "comment-password" },
            CancellationToken.None);

        Assert.True(deleted.IsSuccess);
        Assert.Equal(0, post.CommentCount);
        Assert.True((await db.PlatformCommunityPostComments.FindAsync(created.Value.Id))!.IsDeleted);
    }

    private static PlatformCommunityPost CreatePost()
        => new()
        {
            AppKey = "platform",
            Category = CommunityBoardCatalog.FreeLife.DisplayName,
            WorkflowTag = "커뮤니티 신뢰",
            RoleTag = "구성원",
            Title = "참여 테스트",
            Body = "추천과 댓글 참여를 검증합니다.",
            OriginalLanguageCode = "ko",
            Nickname = "작성자",
            PasswordHash = "hash",
            PublicationStatusCode = PlatformCommunityPostPublicationStatusCodes.Published,
            PublishedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private sealed class AnonymousUserAccessor : ICurrentUserAccessor
    {
        public string? UserId => null;
        public string? Role => null;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
