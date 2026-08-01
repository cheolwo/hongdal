using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Admin.Community;

namespace SsalddelAdmin.Services;

public sealed class CommunityManagementService
{
    private const string Endpoint = "api/v1/admin/community-management";
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;
    private readonly bool _useMemory;
    private readonly CommunityManagementUserResponse _memoryUser = BuildMemoryUser();

    public CommunityManagementService(
        HttpClient httpClient,
        관리자인증세션Service session,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _session = session;
        _useMemory = configuration.GetValue("AdminData:UseMemory", false);
    }

    public async Task<CommunityManagementUserResponse> GetUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeRequired(userId, nameof(userId));
        if (_useMemory)
        {
            return string.Equals(normalized, _memoryUser.UserId, StringComparison.OrdinalIgnoreCase)
                ? _memoryUser
                : new CommunityManagementUserResponse { UserId = normalized };
        }

        using var request = CreateRequest(
            HttpMethod.Get,
            $"{Endpoint}/users/{Uri.EscapeDataString(normalized)}");
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityManagementUserResponse>(
                   cancellationToken: cancellationToken)
               ?? new CommunityManagementUserResponse { UserId = normalized };
    }

    public Task<CommunityManagementActionResponse> SetPostVisibilityAsync(
        long postId,
        bool hidden,
        string reason,
        CancellationToken cancellationToken = default)
        => SetVisibilityAsync("posts", postId, hidden, reason, cancellationToken);

    public Task<CommunityManagementActionResponse> SetCommentVisibilityAsync(
        long commentId,
        bool hidden,
        string reason,
        CancellationToken cancellationToken = default)
        => SetVisibilityAsync("comments", commentId, hidden, reason, cancellationToken);

    public Task<CommunityManagementActionResponse> SetAttachmentCommentVisibilityAsync(
        long commentId,
        bool hidden,
        string reason,
        CancellationToken cancellationToken = default)
        => SetVisibilityAsync("attachment-comments", commentId, hidden, reason, cancellationToken);

    private async Task<CommunityManagementActionResponse> SetVisibilityAsync(
        string resource,
        long id,
        bool hidden,
        string reason,
        CancellationToken cancellationToken)
    {
        var normalizedReason = NormalizeReason(reason);
        if (_useMemory)
        {
            ApplyMemoryVisibility(resource, id, hidden);
            return new CommunityManagementActionResponse
            {
                Succeeded = true,
                Message = hidden ? "개발 샘플을 숨김 처리했습니다." : "개발 샘플을 복구했습니다.",
                RecordedAtUtc = DateTime.UtcNow
            };
        }

        using var request = CreateRequest(HttpMethod.Put, $"{Endpoint}/{resource}/{id}/visibility");
        request.Content = JsonContent.Create(new CommunityManagementVisibilityRequest
        {
            Hidden = hidden,
            Reason = normalizedReason
        });
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityManagementActionResponse>(
                   cancellationToken: cancellationToken)
               ?? new CommunityManagementActionResponse
               {
                   Succeeded = true,
                   Message = "커뮤니티 운영 조치를 기록했습니다.",
                   RecordedAtUtc = DateTime.UtcNow
               };
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
        }

        return request;
    }

    private void ApplyMemoryVisibility(string resource, long id, bool hidden)
    {
        if (resource == "posts")
        {
            var post = _memoryUser.Posts.FirstOrDefault(item => item.Id == id)
                       ?? throw new InvalidOperationException("개발 샘플 게시글을 찾지 못했습니다.");
            post.IsDeleted = hidden;
            return;
        }

        if (resource == "comments")
        {
            var comment = _memoryUser.Posts
                .SelectMany(post => post.Comments)
                .FirstOrDefault(item => item.Id == id)
                ?? throw new InvalidOperationException("개발 샘플 댓글을 찾지 못했습니다.");
            comment.IsOperatorHidden = hidden;
            return;
        }

        var attachmentComment = _memoryUser.Posts
            .SelectMany(post => post.Attachments)
            .SelectMany(attachment => attachment.Comments)
            .FirstOrDefault(item => item.Id == id)
            ?? throw new InvalidOperationException("개발 샘플 첨부 댓글을 찾지 못했습니다.");
        attachmentComment.IsOperatorHidden = hidden;
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? throw new ArgumentException("사용자 ID를 입력해야 합니다.", parameterName)
            : normalized;
    }

    private static string NormalizeReason(string reason)
    {
        var normalized = reason?.Trim();
        return normalized is null || normalized.Length is < 2 or > 1000
            ? throw new ArgumentException("조치 사유는 2자 이상 1000자 이하로 입력해야 합니다.", nameof(reason))
            : normalized;
    }

    private static CommunityManagementUserResponse BuildMemoryUser()
        => new()
        {
            UserId = "sample-community-user",
            AccountExists = true,
            UserName = "커뮤니티샘플",
            Email = "community@example.test",
            PhoneNumber = "010-0000-1234",
            Roles = ["회원"],
            Posts =
            [
                new CommunityManagementPostResponse
                {
                    Id = 1001,
                    AppKey = "SsalddelApp",
                    Category = "신고·분쟁",
                    Title = "운영 검토가 필요한 개발 샘플 게시글",
                    Body = "실제 운영 데이터가 아닌 커뮤니티 관리 화면 검증용 내용입니다.",
                    Nickname = "샘플 이웃",
                    CreatedAtUtc = DateTime.UtcNow.AddHours(-2),
                    UpdatedAtUtc = DateTime.UtcNow.AddHours(-1),
                    Comments =
                    [
                        new CommunityManagementCommentResponse
                        {
                            Id = 2001,
                            Nickname = "신고된 댓글 샘플",
                            Body = "운영자가 신고 사유를 확인하는 개발 샘플입니다.",
                            ReportCount = 2,
                            CreatedAtUtc = DateTime.UtcNow.AddMinutes(-40)
                        }
                    ]
                }
            ]
        };
}
