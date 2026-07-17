using System.Globalization;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Inbound;
using Hongdal.Contracts.Common.Versioning;
using Hongdal.Contracts.Common.Warehouse;
using Hongdal.Ui.Common.Areas.App.Models;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;

namespace Hongdal.Ui.Common.Areas.App.Components.Community;

public partial class PlatformCommunityHome
{
    private async Task LoadPostsAsync()
    {
        isLoading = true;
        try
        {
            ViewModel.Configure(AppKey, ResolveRoleTag(RoleLabel));
            if (!await ViewModel.새로고침Async())
            {
                throw new InvalidOperationException(
                    ViewModel.오류메시지 ?? "커뮤니티 게시글을 불러오지 못했습니다.");
            }

            if (QueryPostId is long requestedPostId)
            {
                var requestedPost = posts.FirstOrDefault(post => post.Id == requestedPostId);
                if (requestedPost is not null)
                {
                    selectedBoardFilter = requestedPost.Category;
                    selectedForumPostId = requestedPost.Id;
                    selectedForumSeedPostTitle = null;
                }
            }

            if (selectedForumPostId is long selectedPostId)
            {
                await LoadPostDetailAsync(selectedPostId);
            }
        }
        catch
        {
            statusSeverity = Severity.Info;
            statusMessage = "커뮤니티 API가 연결되면 글 작성과 조회가 활성화됩니다.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadBoardsAsync()
        => await Boards.LoadAsync(AppKey, CanManageCommunityPosts);

    private async Task LoadPostDetailAsync(long postId)
    {
        var detail = await ViewModel.PostList.RefreshItemAsync(postId);
        if (detail is null)
        {
            return;
        }

        try
        {
            await RefreshPostOpportunitiesAsync(postId);
        }
        catch (HttpRequestException)
        {
            postOpportunities.Remove(postId);
        }
    }

    private async Task SaveBoardRequestAsync()
    {
        Shell.ClearStatus();
        ApplyCommandResult(await Boards.SubmitRequestAsync(AppKey, CanManageCommunityPosts));
    }

    private async Task ReviewBoardAsync(PlatformCommunityBoardResponse board, bool approve)
        => ApplyCommandResult(await Boards.ReviewAsync(AppKey, board, approve));

    private void SelectBoard(string title)
    {
        form.Category = title;
        statusSeverity = Severity.Info;
        statusMessage = $"'{title}' 게시판으로 글 분류를 선택했습니다.";
    }

    private void ApplyCommandResult(PlatformCommunityCommandResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.Message))
        {
            Shell.SetStatus(result.Message, result.MessageKind);
        }
    }

    private async Task ApplyCommandResultAsync(PlatformCommunityCommandResult result)
    {
        ApplyCommandResult(result);
        if (result.RefreshPosts)
        {
            await LoadPostsAsync();
        }
    }

    private async Task HandleComposerSavedAsync(CommunityPostComposerSaveResult result)
    {
        statusSeverity = Severity.Success;
        statusMessage = result.Message;
        await LoadPostsAsync();
    }


    private async Task RecommendPostAsync(PlatformCommunityPostResponse post)
        => await ApplyCommandResultAsync(await Engagement.RecommendAsync(post.Id));

    private async Task ToggleOperatorPinAsync(PlatformCommunityPostResponse post)
        => await ApplyCommandResultAsync(
            await Engagement.SetOperatorPinAsync(post.Id, !post.IsOperatorPinned));

    private async Task SaveCommentAsync(PlatformCommunityPostResponse post)
        => await ApplyCommandResultAsync(await Engagement.SaveCommentAsync(post.Id));

    private Task BeginSalesInquiryAsync(PlatformCommunityPostResponse post, string message)
    {
        var commentForm = GetCommentForm(post.Id);
        commentForm.Body = message;
        if (!IsPostCommentsExpanded(post.Id))
        {
            TogglePostComments(post.Id);
        }

        return Task.CompletedTask;
    }

    private async Task DeleteCommentAsync(PlatformCommunityPostResponse post, PlatformCommunityPostCommentResponse comment)
        => await ApplyCommandResultAsync(
            await Engagement.DeleteCommentAsync(post.Id, comment.Id));

    private async Task ReportCommentAsync(PlatformCommunityPostCommentResponse comment)
        => await ApplyCommandResultAsync(await Engagement.ReportCommentAsync(comment.Id));

    private PlatformCommunityCommentForm GetCommentForm(long postId)
        => Engagement.GetCommentForm(postId);

    private async Task SaveAttachmentCommentAsync(PlatformCommunityPostAttachmentResponse attachment)
        => await ApplyCommandResultAsync(
            await Engagement.SaveAttachmentCommentAsync(attachment.Id));

    private async Task DeleteAttachmentCommentAsync(
        PlatformCommunityPostAttachmentResponse attachment,
        PlatformCommunityPostAttachmentCommentResponse comment)
        => await ApplyCommandResultAsync(
            await Engagement.DeleteAttachmentCommentAsync(attachment.Id, comment.Id));

    private async Task ReportAttachmentCommentAsync(PlatformCommunityPostAttachmentCommentResponse comment)
        => await ApplyCommandResultAsync(
            await Engagement.ReportAttachmentCommentAsync(comment.Id));

    private PlatformCommunityCommentForm GetAttachmentCommentForm(long attachmentId)
        => Engagement.GetAttachmentCommentForm(attachmentId);

    private void BeginEdit(PlatformCommunityPostResponse post)
    {
        Composer.BeginEdit(post);
        statusMessage = "작성할 때 입력한 비밀번호를 넣고 수정 저장을 누르세요.";
        statusSeverity = Severity.Info;
        OpenCommunityMode();
    }

    private void CancelEdit()
    {
        ResetForm();
        statusMessage = null;
        isComposeOpen = false;
    }

    private bool ApplyPendingCommunityPostDraft()
    {
        var draft = CommunityPostDrafts.Consume();
        if (draft is null)
        {
            return false;
        }

        ResetForm();
        form.Category = draft.Category;
        form.WorkflowTag = draft.WorkflowTag;
        form.RoleTag = ResolveRoleTag(RoleLabel);
        form.Title = draft.Title;
        form.Body = draft.Body;
        form.SharedLinkUrl = draft.SharedLinkUrl;
        selectedBoardFilter = BoardCategoryOptions.Contains(draft.Category, StringComparer.OrdinalIgnoreCase)
            ? draft.Category
            : "전체";
        OpenCompose();
        statusSeverity = Severity.Info;
        statusMessage = draft.SourceKind switch
        {
            PlatformCommunityPostDraftSourceKinds.YouTubeFood
                => $"{draft.SourceLabel}의 음식 영상 공유 초안입니다. 나눌 내용을 확인한 뒤 등록하세요.",
            PlatformCommunityPostDraftSourceKinds.PrajnaLecture
                => $"{draft.SourceLabel}에서 가져온 글귀 초안입니다. 공개할 내용을 확인한 뒤 등록하세요.",
            _ => $"{draft.SourceLabel}에서 가져온 공유 초안입니다. 공개할 내용을 확인하세요."
        };
        return true;
    }

    private void HandleYouTubeFoodShareRequested(PlatformCommunityPostDraft draft)
    {
        CommunityPostDrafts.Prepare(draft);
        ApplyPendingCommunityPostDraft();
    }

    private static bool IsYouTubeSharedLink(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var host = uri.Host.TrimStart('.');
        return host.Equals("youtu.be", StringComparison.OrdinalIgnoreCase)
               || host.Equals("youtube.com", StringComparison.OrdinalIgnoreCase)
               || host.EndsWith(".youtube.com", StringComparison.OrdinalIgnoreCase)
               || host.Equals("youtube-nocookie.com", StringComparison.OrdinalIgnoreCase)
               || host.EndsWith(".youtube-nocookie.com", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFoodYouTubeSharedPost(PlatformCommunityPostResponse post)
        => post.Title.StartsWith("[음식 발견]", StringComparison.OrdinalIgnoreCase);

    private static string ResolveSharedVideoEyebrow(PlatformCommunityPostResponse post)
        => IsFoodYouTubeSharedPost(post)
            ? "영상에서 발견한 음식"
            : post.Title.StartsWith("[반야 나눔]", StringComparison.OrdinalIgnoreCase)
                ? "영상과 함께 나눈 글귀"
                : "커뮤니티에 공유한 영상";

    private static string ResolveSharedVideoTitle(string title)
    {
        foreach (var prefix in new[] { "[반야 나눔] ", "[음식 발견] " })
        {
            if (title.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return title[prefix.Length..];
            }
        }

        return title;
    }

    private void ResetForm()
    {
        Composer.Reset();
        isLedgerDetailOpen = false;
        isLedgerPickerOpen = false;
        pendingLedgerId = null;
    }

}
