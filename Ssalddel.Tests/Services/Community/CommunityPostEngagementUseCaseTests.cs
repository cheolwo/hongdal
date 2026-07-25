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

    [Fact]
    public async Task 댓글_활동국가는_서버카탈로그이름으로_snapshot되고_숨김을선택할수있다()
    {
        await using var db = CreateContext();
        var post = CreatePost();
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();
        var useCase = new 커뮤니티게시글참여UseCase(db, new AnonymousUserAccessor());

        var publicComment = await useCase.댓글작성Async(
            post.Id,
            new PlatformCommunityPostCommentCreateRequest
            {
                Password = "comment-password",
                Body = "메인의 생활 방식이 궁금합니다.",
                IsAuthorDisplayCountryPublic = true,
                AuthorDisplayCountryCode = "kr"
            },
            CancellationToken.None);
        var hiddenComment = await useCase.댓글작성Async(
            post.Id,
            new PlatformCommunityPostCommentCreateRequest
            {
                Password = "comment-password",
                Body = "국가 문맥을 숨깁니다.",
                IsAuthorDisplayCountryPublic = false,
                AuthorDisplayCountryCode = "US"
            },
            CancellationToken.None);

        Assert.True(publicComment.IsSuccess);
        Assert.True(publicComment.Value.IsAuthorDisplayCountryPublic);
        Assert.Equal("KR", publicComment.Value.AuthorDisplayCountryCode);
        Assert.Equal("대한민국", publicComment.Value.AuthorDisplayCountryName);
        Assert.False(hiddenComment.Value.IsAuthorDisplayCountryPublic);
        Assert.Null(hiddenComment.Value.AuthorDisplayCountryCode);
        Assert.Null(hiddenComment.Value.AuthorDisplayCountryName);
    }

    [Fact]
    public async Task 댓글_활동국가는_잘못된코드와_신고게시판공개를거부한다()
    {
        await using var db = CreateContext();
        var normalPost = CreatePost();
        var reportPost = CreatePost();
        reportPost.Category = PlatformCommunityPostCategories.ReportDispute;
        reportPost.IsReportBoardPost = true;
        db.PlatformCommunityPosts.AddRange(normalPost, reportPost);
        await db.SaveChangesAsync();
        var useCase = new 커뮤니티게시글참여UseCase(db, new AnonymousUserAccessor());

        var invalid = await useCase.댓글작성Async(
            normalPost.Id,
            new PlatformCommunityPostCommentCreateRequest
            {
                Password = "comment-password",
                Body = "잘못된 코드",
                IsAuthorDisplayCountryPublic = true,
                AuthorDisplayCountryCode = "ZZ"
            },
            CancellationToken.None);
        var protectedComment = await useCase.댓글작성Async(
            reportPost.Id,
            new PlatformCommunityPostCommentCreateRequest
            {
                Nickname = "신고자",
                Password = "comment-password",
                Body = "보호되어야 하는 댓글",
                IsAuthorDisplayCountryPublic = true,
                AuthorDisplayCountryCode = "KR"
            },
            CancellationToken.None);

        Assert.True(invalid.IsFailed);
        Assert.Contains("ISO 3166-1", invalid.Errors[0].Message);
        Assert.True(protectedComment.IsSuccess);
        Assert.False(protectedComment.Value.IsAuthorDisplayCountryPublic);
        Assert.Null(protectedComment.Value.AuthorDisplayCountryCode);
    }

    [Fact]
    public async Task 첨부이미지댓글도_같은활동국가정책을사용한다()
    {
        await using var db = CreateContext();
        var post = CreatePost();
        var attachment = new PlatformCommunityPostAttachment
        {
            Post = post,
            Url = "/image.jpg",
            BucketName = "test",
            ObjectName = "image.jpg",
            OriginalFileName = "image.jpg",
            ContentType = "image/jpeg",
            UploadedAtUtc = DateTime.UtcNow
        };
        post.Attachments.Add(attachment);
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();
        var useCase = new 커뮤니티게시글참여UseCase(db, new AnonymousUserAccessor());

        var result = await useCase.첨부댓글작성Async(
            attachment.Id,
            new PlatformCommunityPostAttachmentCommentCreateRequest
            {
                Password = "comment-password",
                Body = "사진 속 식재료가 궁금합니다.",
                IsAuthorDisplayCountryPublic = true,
                AuthorDisplayCountryCode = "US"
            },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("US", result.Value.AuthorDisplayCountryCode);
        Assert.Equal("미국", result.Value.AuthorDisplayCountryName);
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
