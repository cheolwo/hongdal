using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Ssalddel.Application.Community;
using Ssalddel.Domain.Community;
using Ssalddel.Extensions;
using Ssalddel.Services.Community;
using Ssalddel.Services.Notifications;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Community;

public sealed class CommunityPostEmailNotificationTests
{
    [Fact]
    public async Task EnabledEventHandler_PersistsPublishedPostIdToDbOutbox()
    {
        await using var db = CreateContext();
        var outbox = new CommunityPostEmailNotificationOutboxStore(db);
        var handler = new 커뮤니티게시글이메일알림EventHandler(
            outbox,
            OptionsMonitor(new CommunityPostEmailNotificationOptions { Enabled = true }),
            NullLogger<커뮤니티게시글이메일알림EventHandler>.Instance);

        await handler.Handle(new 커뮤니티게시글등록됨Event(72), CancellationToken.None);

        var saved = Assert.Single(await db.CommunityPostEmailNotificationOutbox.ToListAsync());
        Assert.Equal(72, saved.PostId);
        Assert.Equal(CommunityPostEmailNotificationOutboxStatuses.Pending, saved.Status);
    }

    [Fact]
    public async Task DisabledEventHandler_DoesNotPersistOutbox()
    {
        await using var db = CreateContext();
        var outbox = new CommunityPostEmailNotificationOutboxStore(db);
        var handler = new 커뮤니티게시글이메일알림EventHandler(
            outbox,
            OptionsMonitor(new CommunityPostEmailNotificationOptions()),
            NullLogger<커뮤니티게시글이메일알림EventHandler>.Instance);

        await handler.Handle(new 커뮤니티게시글등록됨Event(72), CancellationToken.None);

        Assert.Empty(await db.CommunityPostEmailNotificationOutbox.ToListAsync());
    }

    [Fact]
    public async Task DbOutbox_DeduplicatesClaimsAndCompletesPersistently()
    {
        await using var db = CreateContext();
        var outbox = new CommunityPostEmailNotificationOutboxStore(db);
        await outbox.EnqueueAsync(10);
        await outbox.EnqueueAsync(10);

        var work = Assert.IsType<CommunityPostEmailNotificationOutboxWork>(
            await outbox.ClaimNextAsync(TimeSpan.FromMinutes(1)));
        Assert.Equal(10, work.PostId);
        Assert.Null(await outbox.ClaimNextAsync(TimeSpan.FromMinutes(1)));

        await outbox.CompleteAsync(
            work,
            CommunityPostEmailNotificationOutboxStatuses.Sent,
            null);

        var saved = Assert.Single(await db.CommunityPostEmailNotificationOutbox.ToListAsync());
        Assert.Equal(CommunityPostEmailNotificationOutboxStatuses.Sent, saved.Status);
        Assert.NotNull(saved.ProcessedAtUtc);
    }

    [Fact]
    public async Task Processor_SendsMetadataAndLinkWithoutPostBody()
    {
        await using var db = CreateContext();
        var post = PublishedPost();
        post.Title = "양파 공동구매 제안";
        post.Body = "EMAIL-MUST-NOT-CONTAIN-THIS-BODY";
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();

        var sender = new RecordingSender(
            new CommunityPostEmailDeliveryResult(CommunityPostEmailDeliveryStatus.Sent));
        var processor = new CommunityPostEmailNotificationProcessor(
            db,
            sender,
            OptionsMonitor(EnabledOptions()));

        var result = await processor.ProcessAsync(post.Id, CancellationToken.None);

        Assert.Equal(CommunityPostEmailNotificationProcessStatus.Sent, result.Status);
        var message = Assert.IsType<CommunityPostEmailMessage>(sender.Message);
        Assert.Equal("owner@example.com", message.RecipientEmail);
        Assert.Contains(post.Title, message.Subject, StringComparison.Ordinal);
        Assert.Contains(
            $"https://example.com/community/posts/{post.Id}",
            message.Body,
            StringComparison.Ordinal);
        Assert.DoesNotContain(post.Body, message.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Processor_RedactsReportTitleAuthorAndBody()
    {
        await using var db = CreateContext();
        var post = PublishedPost();
        post.IsReportBoardPost = true;
        post.Title = "SECRET-REPORT-TITLE";
        post.Nickname = "SECRET-REPORTER";
        post.Body = "SECRET-REPORT-BODY";
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();

        var sender = new RecordingSender(
            new CommunityPostEmailDeliveryResult(CommunityPostEmailDeliveryStatus.Sent));
        var processor = new CommunityPostEmailNotificationProcessor(
            db,
            sender,
            OptionsMonitor(EnabledOptions()));

        var result = await processor.ProcessAsync(post.Id, CancellationToken.None);

        Assert.Equal(CommunityPostEmailNotificationProcessStatus.Sent, result.Status);
        var message = Assert.IsType<CommunityPostEmailMessage>(sender.Message);
        Assert.DoesNotContain(post.Title, message.Subject, StringComparison.Ordinal);
        Assert.DoesNotContain(post.Title, message.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(post.Nickname, message.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(post.Body, message.Body, StringComparison.Ordinal);
        Assert.Contains("보호됨", message.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Processor_SkipsDeletedPost()
    {
        await using var db = CreateContext();
        var post = PublishedPost();
        post.IsDeleted = true;
        db.PlatformCommunityPosts.Add(post);
        await db.SaveChangesAsync();

        var sender = new RecordingSender(
            new CommunityPostEmailDeliveryResult(CommunityPostEmailDeliveryStatus.Sent));
        var processor = new CommunityPostEmailNotificationProcessor(
            db,
            sender,
            OptionsMonitor(EnabledOptions()));

        var result = await processor.ProcessAsync(post.Id, CancellationToken.None);

        Assert.Equal(CommunityPostEmailNotificationProcessStatus.Skipped, result.Status);
        Assert.Null(sender.Message);
    }

    [Fact]
    public async Task GmailSender_RequiresAppPasswordBeforeNetworkCall()
    {
        var sender = new GmailCommunityPostEmailSender(
            OptionsMonitor(new CommunityPostEmailNotificationOptions
            {
                Enabled = true,
                Gmail = new CommunityPostEmailGmailOptions
                {
                    UserName = "sender@gmail.com"
                }
            }));

        var result = await sender.SendAsync(
            new CommunityPostEmailMessage(
                1,
                "owner@example.com",
                "새 게시글",
                "알림"),
            CancellationToken.None);

        Assert.Equal(CommunityPostEmailDeliveryStatus.ConfigurationRequired, result.Status);
    }

    [Fact]
    public async Task GmailSender_OptInSmtpIntegration_SendsActualMessageWhenSecretsArePresent()
    {
        var required = string.Equals(
            Environment.GetEnvironmentVariable("SSALDDEL_GMAIL_INTEGRATION_REQUIRED"),
            "true",
            StringComparison.OrdinalIgnoreCase);
        var userName = Environment.GetEnvironmentVariable("SSALDDEL_GMAIL_USER_NAME");
        var appPassword = Environment.GetEnvironmentVariable("SSALDDEL_GMAIL_APP_PASSWORD");
        var recipient = Environment.GetEnvironmentVariable("SSALDDEL_GMAIL_INTEGRATION_RECIPIENT");
        if (string.IsNullOrWhiteSpace(userName)
            || string.IsNullOrWhiteSpace(appPassword)
            || string.IsNullOrWhiteSpace(recipient))
        {
            Assert.False(
                required,
                "Gmail 통합 검증이 필수이지만 Gmail 사용자·앱 비밀번호·수신자 환경변수가 없습니다.");
            return;
        }

        var sender = new GmailCommunityPostEmailSender(
            OptionsMonitor(new CommunityPostEmailNotificationOptions
            {
                Enabled = true,
                Gmail = new CommunityPostEmailGmailOptions
                {
                    UserName = userName,
                    AppPassword = appPassword,
                    FromAddress = userName,
                    FromDisplayName = "Ssalddel CI"
                }
            }));
        var marker = $"ssalddel-gmail-integration-{Guid.NewGuid():N}";

        var result = await sender.SendAsync(
            new CommunityPostEmailMessage(
                0,
                recipient,
                $"[Ssalddel integration] {marker}",
                $"Gmail SMTP integration marker: {marker}"),
            CancellationToken.None);

        Assert.Equal(CommunityPostEmailDeliveryStatus.Sent, result.Status);
    }

    [Fact]
    public void Options_UseBootstrapAdminAsDefaultRecipient()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["IdentitySeed:BootstrapAdmin:Email"] = "admin@example.com",
                [$"{CommunityPostEmailNotificationOptions.SectionName}:Gmail:UserName"] =
                    "sender@gmail.com"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddSsalddelOptions(configuration);

        using var provider = services.BuildServiceProvider();
        var options = provider
            .GetRequiredService<IOptions<CommunityPostEmailNotificationOptions>>()
            .Value;

        Assert.Equal("admin@example.com", options.RecipientEmail);
    }

    private static CommunityPostEmailNotificationOptions EnabledOptions()
        => new()
        {
            Enabled = true,
            RecipientEmail = "owner@example.com",
            PublicBaseUrl = "https://example.com",
            Gmail = new CommunityPostEmailGmailOptions
            {
                UserName = "sender@gmail.com",
                AppPassword = "abcdefghijklmnop"
            }
        };

    private static PlatformCommunityPost PublishedPost()
        => new()
        {
            AppKey = "platform",
            Category = "공동구매",
            WorkflowTag = "수요 모집",
            RoleTag = "주문자",
            Title = "게시글 제목",
            Body = "게시글 본문",
            Nickname = "작성자",
            PasswordHash = "hash",
            PublicationStatusCode = PlatformCommunityPostPublicationStatusCodes.Published,
            PublishedAtUtc = new DateTime(2026, 7, 23, 3, 0, 0, DateTimeKind.Utc),
            CreatedAtUtc = new DateTime(2026, 7, 23, 3, 0, 0, DateTimeKind.Utc),
            UpdatedAtUtc = new DateTime(2026, 7, 23, 3, 0, 0, DateTimeKind.Utc)
        };

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"community-post-email-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(options, new DummyPersonalDataEncryptionService());
    }

    private static IOptionsMonitor<T> OptionsMonitor<T>(T value)
        => new StaticOptionsMonitor<T>(value);

    private sealed class RecordingSender(CommunityPostEmailDeliveryResult result)
        : ICommunityPostEmailSender
    {
        public CommunityPostEmailMessage? Message { get; private set; }

        public Task<CommunityPostEmailDeliveryResult> SendAsync(
            CommunityPostEmailMessage message,
            CancellationToken cancellationToken)
        {
            Message = message;
            return Task.FromResult(result);
        }
    }

    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;
        public T Get(string? name) => value;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
