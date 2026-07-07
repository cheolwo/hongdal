using System.Net.Http.Json;
using Hongdal.Contracts.Common.Community;
using Microsoft.AspNetCore.Components.Forms;

namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed class PlatformCommunityService
{
    private readonly HttpClient _httpClient;
    private readonly HongdalProtectedApiClient _protectedApiClient;

    public PlatformCommunityService(
        HttpClient httpClient,
        HongdalProtectedApiClient protectedApiClient)
    {
        _httpClient = httpClient;
        _protectedApiClient = protectedApiClient;
    }

    public async Task<PlatformCommunityPostListResponse> GetPostsAsync(
        string appKey,
        CancellationToken cancellationToken = default)
    {
        var path = $"api/v1/community/posts?appKey={Uri.EscapeDataString(appKey)}&page=1&pageSize=20";
        return await _httpClient.GetFromJsonAsync<PlatformCommunityPostListResponse>(path, cancellationToken)
               ?? new PlatformCommunityPostListResponse();
    }

    public async Task<PlatformCommunityBoardListResponse> GetBoardsAsync(
        string appKey,
        string status = PlatformCommunityBoardRequestStatuses.Approved,
        CancellationToken cancellationToken = default)
    {
        var path = $"api/v1/community/boards?appKey={Uri.EscapeDataString(appKey)}&status={Uri.EscapeDataString(status)}";
        return await _httpClient.GetFromJsonAsync<PlatformCommunityBoardListResponse>(path, cancellationToken)
               ?? new PlatformCommunityBoardListResponse();
    }

    public async Task<PlatformCommunityBoardResponse?> CreateBoardRequestAsync(
        PlatformCommunityBoardCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync("api/v1/community/boards", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityBoardResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityBoardResponse?> ApproveBoardAsync(
        long boardRequestId,
        string operatorMemo,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/boards/{boardRequestId}/approve",
            new PlatformCommunityBoardReviewRequest { OperatorMemo = operatorMemo },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityBoardResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityBoardResponse?> RejectBoardAsync(
        long boardRequestId,
        string operatorMemo,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/boards/{boardRequestId}/reject",
            new PlatformCommunityBoardReviewRequest { OperatorMemo = operatorMemo },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityBoardResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityPostResponse?> CreatePostAsync(
        PlatformCommunityPostCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync("api/v1/community/posts", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityPostResponse?> UpdatePostAsync(
        long postId,
        PlatformCommunityPostUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PutAsProtectedJsonAsync($"api/v1/community/posts/{postId}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityPostResponse?> SetOperatorPinAsync(
        long postId,
        bool isOperatorPinned,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/posts/{postId}/operator-pin",
            new PlatformCommunityPostOperatorPinRequest { IsOperatorPinned = isOperatorPinned },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityPostResponse?> RecommendAsync(
        long postId,
        string recommenderKey,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/posts/{postId}/recommendations",
            new PlatformCommunityPostRecommendationRequest { RecommenderKey = recommenderKey },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostResponse>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<PlatformCommunityPostCommentResponse>> GetCommentsAsync(
        long postId,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<PlatformCommunityPostCommentResponse>>(
                   $"api/v1/community/posts/{postId}/comments",
                   cancellationToken)
               ?? [];
    }

    public async Task<PlatformCommunityPostCommentResponse?> CreateCommentAsync(
        long postId,
        PlatformCommunityPostCommentCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync($"api/v1/community/posts/{postId}/comments", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostCommentResponse>(cancellationToken: cancellationToken);
    }

    public async Task DeleteCommentAsync(
        long postId,
        long commentId,
        string password,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.SendAsProtectedJsonAsync(
            HttpMethod.Delete,
            $"api/v1/community/posts/{postId}/comments/{commentId}",
            new PlatformCommunityPostPasswordRequest { Password = password },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReportCommentAsync(long commentId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"api/v1/community/posts/comments/{commentId}/reports", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<PlatformCommunityPostAttachmentCommentResponse>> GetAttachmentCommentsAsync(
        long attachmentId,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<PlatformCommunityPostAttachmentCommentResponse>>(
                   $"api/v1/community/posts/attachments/{attachmentId}/comments",
                   cancellationToken)
               ?? [];
    }

    public async Task<PlatformCommunityPostAttachmentCommentResponse?> CreateAttachmentCommentAsync(
        long attachmentId,
        PlatformCommunityPostAttachmentCommentCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/posts/attachments/{attachmentId}/comments",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostAttachmentCommentResponse>(cancellationToken: cancellationToken);
    }

    public async Task DeleteAttachmentCommentAsync(
        long attachmentId,
        long commentId,
        string password,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.SendAsProtectedJsonAsync(
            HttpMethod.Delete,
            $"api/v1/community/posts/attachments/{attachmentId}/comments/{commentId}",
            new PlatformCommunityPostPasswordRequest { Password = password },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReportAttachmentCommentAsync(long commentId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"api/v1/community/posts/attachments/comments/{commentId}/reports", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<PlatformCommunityPostAttachmentResponse?> UploadAttachmentAsync(
        long postId,
        string password,
        IBrowserFile file,
        long maxAllowedSize,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(password), "Password");

        await using var stream = file.OpenReadStream(maxAllowedSize);
        using var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "File", file.Name);

        using var response = await _httpClient.PostAsync($"api/v1/community/posts/{postId}/attachments", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostAttachmentResponse>(cancellationToken: cancellationToken);
    }
}
