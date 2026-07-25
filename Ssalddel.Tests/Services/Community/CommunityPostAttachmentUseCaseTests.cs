using Ssalddel.Domain.Community;
using Ssalddel.Services.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Options;
using Ssalddel.Services.Storage;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityPostAttachmentUseCaseTests
{
    [Fact]
    public async Task 유효한_이미지는_객체저장소와_게시글_첨부에_한번씩_저장된다()
    {
        await using var db = CreateContext();
        var post = CreatePost("correct-password");
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();
        var storage = new RecordingStorageService();
        var useCase = CreateUseCase(db, storage);
        await using var content = new MemoryStream([1, 2, 3]);

        var result = await useCase.첨부업로드Async(
            post.Id,
            new 커뮤니티게시글첨부업로드Command(
                "correct-password",
                content,
                "sample.png",
                "image/png",
                content.Length),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal($"community/posts/{post.Id}", storage.UploadFolder);
        Assert.Equal(ObjectStorageAccess.Public, storage.UploadAccess);
        var attachment = await db.PlatformCommunityPostAttachments.SingleAsync();
        Assert.Equal(post.Id, attachment.PostId);
        Assert.Equal("object/sample.png", attachment.ObjectName);
        Assert.Equal(attachment.Id, result.Value.Id);
    }

    [Fact]
    public async Task MP4_동영상은_별도_용량한도로_검증한뒤_미디어첨부로_저장된다()
    {
        await using var db = CreateContext();
        var post = CreatePost("correct-password");
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();
        var storage = new RecordingStorageService();
        var useCase = CreateUseCase(db, storage);
        await using var content = new MemoryStream([1, 2, 3]);

        var result = await useCase.첨부업로드Async(
            post.Id,
            new 커뮤니티게시글첨부업로드Command(
                "correct-password",
                content,
                "clip.mp4",
                "video/mp4",
                14 * 1024 * 1024),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("video/mp4", result.Value.ContentType);
        Assert.Equal(14 * 1024 * 1024, result.Value.FileSizeBytes);
        Assert.Equal(1, storage.UploadCount);
    }

    [Theory]
    [InlineData("image/png", 5242881, "이미지 크기는 최대 5MB")]
    [InlineData("video/mp4", 15728641, "동영상 크기는 최대 15MB")]
    public async Task 미디어_유형별_용량한도를_넘으면_업로드하지_않는다(
        string contentType,
        long length,
        string expectedMessage)
    {
        await using var db = CreateContext();
        var post = CreatePost("correct-password");
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();
        var storage = new RecordingStorageService();
        var useCase = CreateUseCase(db, storage);
        await using var content = new MemoryStream([1]);

        var result = await useCase.첨부업로드Async(
            post.Id,
            new 커뮤니티게시글첨부업로드Command(
                "correct-password",
                content,
                contentType.StartsWith("video/", StringComparison.Ordinal) ? "large.mp4" : "large.png",
                contentType,
                length),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains(expectedMessage, result.Errors.Single().Message);
        Assert.Equal(0, storage.UploadCount);
    }

    [Fact]
    public async Task 존재하지_않는_게시글과_틀린_비밀번호는_HTTP_오류_의미를_보존한다()
    {
        await using var db = CreateContext();
        var post = CreatePost("correct-password");
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();
        var storage = new RecordingStorageService();
        var useCase = CreateUseCase(db, storage);

        var notFound = await UploadAsync(useCase, post.Id + 1, "correct-password");
        var forbidden = await UploadAsync(useCase, post.Id, "wrong-password");

        Assert.Equal(404, notFound.Errors.Single().Metadata["StatusCode"]);
        Assert.Equal(403, forbidden.Errors.Single().Metadata["StatusCode"]);
        Assert.Equal(0, storage.UploadCount);
    }

    private static 커뮤니티게시글첨부UseCase CreateUseCase(
        SsalddelContext db,
        IObjectStorageService storage)
        => new(
            db,
            storage,
            Options.Create(new CommunityPostStorageOptions()));

    private static async Task<FluentResults.Result<
        Ssalddel.Contracts.Common.Community.PlatformCommunityPostAttachmentResponse>> UploadAsync(
        I커뮤니티게시글첨부UseCase useCase,
        long postId,
        string password)
    {
        await using var content = new MemoryStream([1]);
        return await useCase.첨부업로드Async(
            postId,
            new 커뮤니티게시글첨부업로드Command(
                password,
                content,
                "sample.png",
                "image/png",
                content.Length),
            CancellationToken.None);
    }

    private static PlatformCommunityPost CreatePost(string password)
        => new()
        {
            AppKey = "platform",
            Category = "정보",
            WorkflowTag = "커뮤니티 신뢰",
            RoleTag = "구성원",
            Title = "첨부 테스트",
            Body = "이미지 첨부 경계를 검증합니다.",
            OriginalLanguageCode = "ko",
            Nickname = "테스터",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
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

    private sealed class RecordingStorageService : IObjectStorageService
    {
        public int UploadCount { get; private set; }
        public string? UploadFolder { get; private set; }
        public ObjectStorageAccess? UploadAccess { get; private set; }

        public bool IsConfigured(ObjectStorageAccess access) => true;

        public Task<ObjectStorageUploadResult> UploadAsync(
            Stream stream,
            string originalFileName,
            string? contentType,
            string? folder,
            ObjectStorageAccess access,
            CancellationToken cancellationToken = default)
        {
            UploadCount++;
            UploadFolder = folder;
            UploadAccess = access;
            return Task.FromResult(new ObjectStorageUploadResult(
                "test-bucket",
                $"object/{Path.GetFileName(originalFileName)}",
                $"https://storage.test/{Path.GetFileName(originalFileName)}"));
        }

        public Task<byte[]> DownloadAsync(
            string bucketName,
            string objectName,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Array.Empty<byte>());
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
