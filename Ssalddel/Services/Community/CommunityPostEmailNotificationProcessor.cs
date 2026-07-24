using System.Globalization;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Services.Notifications;
using 살뜰.Data;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public enum CommunityPostEmailNotificationProcessStatus
{
    Sent,
    Skipped,
    ConfigurationRequired,
    RetryableFailure
}

public sealed record CommunityPostEmailNotificationProcessResult(
    CommunityPostEmailNotificationProcessStatus Status,
    string? Detail = null);

public interface ICommunityPostEmailNotificationProcessor
{
    Task<CommunityPostEmailNotificationProcessResult> ProcessAsync(
        long postId,
        CancellationToken cancellationToken);
}

public sealed class CommunityPostEmailNotificationProcessor
    : ICommunityPostEmailNotificationProcessor
{
    private const string DefaultSubjectPrefix = "[살뜰 새 게시글]";

    private readonly SsalddelContext _db;
    private readonly ICommunityPostEmailSender _sender;
    private readonly IOptionsMonitor<CommunityPostEmailNotificationOptions> _options;

    public CommunityPostEmailNotificationProcessor(
        SsalddelContext db,
        ICommunityPostEmailSender sender,
        IOptionsMonitor<CommunityPostEmailNotificationOptions> options)
    {
        _db = db;
        _sender = sender;
        _options = options;
    }

    public async Task<CommunityPostEmailNotificationProcessResult> ProcessAsync(
        long postId,
        CancellationToken cancellationToken)
    {
        var options = _options.CurrentValue;
        if (!options.Enabled)
        {
            return new(
                CommunityPostEmailNotificationProcessStatus.Skipped,
                "게시글 이메일 알림이 비활성화되어 있습니다.");
        }

        if (!TryNormalizeEmail(options.RecipientEmail, out var recipientEmail))
        {
            return new(
                CommunityPostEmailNotificationProcessStatus.ConfigurationRequired,
                "게시글 알림 수신 이메일 설정이 필요합니다.");
        }

        var post = await _db.PlatformCommunityPosts
            .AsNoTracking()
            .Where(item => item.Id == postId && !item.IsDeleted)
            .Select(item => new CommunityPostEmailProjection(
                item.Id,
                item.AppKey,
                item.Category,
                item.WorkflowTag,
                item.Title,
                item.Nickname,
                item.IsReportBoardPost,
                item.PublishedAtUtc,
                item.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
        if (post is null)
        {
            return new(
                CommunityPostEmailNotificationProcessStatus.Skipped,
                "공개 상태의 게시글을 찾을 수 없습니다.");
        }

        var message = new CommunityPostEmailMessage(
            post.Id,
            recipientEmail!,
            BuildSubject(options.SubjectPrefix, post),
            BuildBody(options.PublicBaseUrl, post));
        var delivery = await _sender.SendAsync(message, cancellationToken);

        return delivery.Status switch
        {
            CommunityPostEmailDeliveryStatus.Sent => new(
                CommunityPostEmailNotificationProcessStatus.Sent),
            CommunityPostEmailDeliveryStatus.ConfigurationRequired => new(
                CommunityPostEmailNotificationProcessStatus.ConfigurationRequired,
                delivery.Error),
            _ => new(
                CommunityPostEmailNotificationProcessStatus.RetryableFailure,
                delivery.Error)
        };
    }

    private static string BuildSubject(
        string configuredPrefix,
        CommunityPostEmailProjection post)
    {
        var prefix = SingleLine(configuredPrefix, 60);
        if (string.IsNullOrWhiteSpace(prefix))
        {
            prefix = DefaultSubjectPrefix;
        }

        if (post.IsReportBoardPost)
        {
            return $"{prefix} 신고 게시글 #{post.Id}";
        }

        var title = SingleLine(post.Title, 120);
        return string.IsNullOrWhiteSpace(title)
            ? $"{prefix} 게시글 #{post.Id}"
            : $"{prefix} {title}";
    }

    private static string BuildBody(
        string publicBaseUrl,
        CommunityPostEmailProjection post)
    {
        var publishedAtUtc = post.PublishedAtUtc ?? post.CreatedAtUtc;
        var lines = new List<string>
        {
            "새 게시글이 등록되었습니다.",
            string.Empty,
            $"게시글 ID: {post.Id.ToString(CultureInfo.InvariantCulture)}",
            $"앱: {SingleLine(post.AppKey, 80)}",
            $"게시판: {SingleLine(post.Category, 120)}",
            $"업무 흐름: {SingleLine(post.WorkflowTag, 160)}",
            post.IsReportBoardPost
                ? "제목: 신고 게시판 보호 정보는 이메일에 포함하지 않습니다."
                : $"제목: {SingleLine(post.Title, 200)}",
            post.IsReportBoardPost
                ? "작성자: 보호됨"
                : $"작성자: {SingleLine(post.Nickname, 80)}",
            $"발행 시각(UTC): {publishedAtUtc:yyyy-MM-dd HH:mm:ss}"
        };

        var postUrl = BuildPostUrl(publicBaseUrl, post.Id);
        if (postUrl is not null)
        {
            lines.Add($"게시글 보기: {postUrl}");
        }

        lines.Add(string.Empty);
        lines.Add("게시글 본문은 개인정보 보호를 위해 이메일에 포함하지 않았습니다.");
        return string.Join(Environment.NewLine, lines);
    }

    private static string? BuildPostUrl(string publicBaseUrl, long postId)
    {
        var candidate = publicBaseUrl.Trim().TrimEnd('/')
            + "/community/posts/"
            + postId.ToString(CultureInfo.InvariantCulture);
        if (!Uri.TryCreate(candidate, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return null;
        }

        return uri.AbsoluteUri;
    }

    private static string SingleLine(string? value, int maxLength)
    {
        var normalized = (value ?? string.Empty)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();
        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength];
    }

    private static bool TryNormalizeEmail(string? value, out string? normalized)
    {
        try
        {
            normalized = string.IsNullOrWhiteSpace(value)
                ? null
                : new MailAddress(value.Trim()).Address;
            return normalized is not null;
        }
        catch (FormatException)
        {
            normalized = null;
            return false;
        }
    }

    private sealed record CommunityPostEmailProjection(
        long Id,
        string AppKey,
        string Category,
        string WorkflowTag,
        string Title,
        string Nickname,
        bool IsReportBoardPost,
        DateTime? PublishedAtUtc,
        DateTime CreatedAtUtc);
}
