using CommunityToolkit.Mvvm.ComponentModel;
using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.Services;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public sealed record PlatformCommunityCommandResult(
    bool Succeeded,
    string? Message = null,
    CommunityComposerMessageKind MessageKind = CommunityComposerMessageKind.Info,
    bool RefreshPosts = false);

public sealed record PlatformCommunityLedgerReuseResult(
    PlatformCommunityCommandResult Command,
    커뮤니티원장재사용Response? ReusedLedger = null);

/// <summary>
/// 플랫폼 커뮤니티 홈의 화면 전환과 공통 메시지 상태를 소유합니다.
/// DOM 포인터와 JS interop 상태는 Razor 컴포넌트에 남깁니다.
/// </summary>
public sealed class PlatformCommunityHomeShellViewModel : ObservableObject
{
    private bool _isLoading = true;
    private bool _isWorkMode;
    private bool _isCompactHomeSummary;
    private bool _isBaguaNavigatorOpen;
    private string? _statusMessage;
    private CommunityComposerMessageKind _statusKind = CommunityComposerMessageKind.Info;

    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    public bool IsWorkMode { get => _isWorkMode; set => SetProperty(ref _isWorkMode, value); }
    public bool IsCompactHomeSummary { get => _isCompactHomeSummary; set => SetProperty(ref _isCompactHomeSummary, value); }
    public bool IsBaguaNavigatorOpen { get => _isBaguaNavigatorOpen; set => SetProperty(ref _isBaguaNavigatorOpen, value); }
    public string? StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public CommunityComposerMessageKind StatusKind { get => _statusKind; set => SetProperty(ref _statusKind, value); }

    public void SetStatus(string message, CommunityComposerMessageKind kind)
    {
        StatusKind = kind;
        StatusMessage = message;
    }

    public void ClearStatus()
        => StatusMessage = null;
}

public sealed class PlatformCommunityBoardForm : ObservableObject
{
    private string _title = string.Empty;
    private string _description = string.Empty;
    private string _requestedBy = string.Empty;
    private string _requestReason = string.Empty;

    public string Title { get => _title; set => SetProperty(ref _title, value ?? string.Empty); }
    public string Description { get => _description; set => SetProperty(ref _description, value ?? string.Empty); }
    public string RequestedBy { get => _requestedBy; set => SetProperty(ref _requestedBy, value ?? string.Empty); }
    public string RequestReason { get => _requestReason; set => SetProperty(ref _requestReason, value ?? string.Empty); }

    public bool IsValid
        => !string.IsNullOrWhiteSpace(Title)
           && !string.IsNullOrWhiteSpace(RequestedBy)
           && !string.IsNullOrWhiteSpace(RequestReason);

    public void ResetAfterSubmit()
    {
        Title = string.Empty;
        Description = string.Empty;
        RequestReason = string.Empty;
    }
}

/// <summary>
/// 승인 게시판, 개설 신청과 운영자 검토 상태를 소유합니다.
/// </summary>
public sealed class PlatformCommunityBoardWorkspaceViewModel : 조립ViewModelBase
{
    private readonly PlatformCommunityService _communityService;
    private bool _isLoading;
    private bool _isSavingRequest;

    public PlatformCommunityBoardWorkspaceViewModel(PlatformCommunityService communityService)
    {
        _communityService = communityService;
        Form = 하위ViewModel등록(new PlatformCommunityBoardForm());
    }

    public List<PlatformCommunityBoardResponse> ApprovedBoards { get; } = [];
    public List<PlatformCommunityBoardResponse> PendingBoardRequests { get; } = [];
    public Dictionary<long, string> ReviewMemos { get; } = [];
    public PlatformCommunityBoardForm Form { get; }
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    public bool IsSavingRequest { get => _isSavingRequest; set => SetProperty(ref _isSavingRequest, value); }

    public void ReplaceApproved(IEnumerable<PlatformCommunityBoardResponse> boards)
    {
        ApprovedBoards.Clear();
        ApprovedBoards.AddRange(boards);
        OnPropertyChanged(nameof(ApprovedBoards));
    }

    public void ReplacePending(IEnumerable<PlatformCommunityBoardResponse> boards)
    {
        PendingBoardRequests.Clear();
        PendingBoardRequests.AddRange(boards);
        foreach (var board in PendingBoardRequests)
        {
            ReviewMemos.TryAdd(board.Id, string.Empty);
        }

        OnPropertyChanged(nameof(PendingBoardRequests));
        OnPropertyChanged(nameof(ReviewMemos));
    }

    public async Task LoadAsync(
        string appKey,
        bool canManageCommunityPosts,
        CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        try
        {
            var approved = await _communityService.GetBoardsAsync(
                appKey,
                PlatformCommunityBoardRequestStatuses.Approved,
                cancellationToken);
            ReplaceApproved(approved.Items);

            if (canManageCommunityPosts)
            {
                var pending = await _communityService.GetBoardsAsync(
                    appKey,
                    PlatformCommunityBoardRequestStatuses.Pending,
                    cancellationToken);
                ReplacePending(pending.Items);
            }
            else
            {
                ReplacePending([]);
            }
        }
        catch
        {
            // 게시판 신청 기능이 연결되지 않아도 기본 게시글 목록은 계속 사용합니다.
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<PlatformCommunityCommandResult> SubmitRequestAsync(
        string appKey,
        bool canManageCommunityPosts,
        CancellationToken cancellationToken = default)
    {
        if (!Form.IsValid)
        {
            return new(
                false,
                "게시판 이름, 신청자, 개설 이유를 입력하세요.",
                CommunityComposerMessageKind.Warning);
        }

        IsSavingRequest = true;
        try
        {
            await _communityService.CreateBoardRequestAsync(
                new PlatformCommunityBoardCreateRequest
                {
                    AppKey = appKey,
                    Title = Form.Title,
                    Description = Form.Description,
                    RequestedBy = Form.RequestedBy,
                    RequestReason = Form.RequestReason
                },
                cancellationToken);
            Form.ResetAfterSubmit();
            await LoadAsync(appKey, canManageCommunityPosts, cancellationToken);
            return new(
                true,
                "게시판 개설 신청을 접수했습니다. 운영자 승인 후 게시판 목록에 표시됩니다.",
                CommunityComposerMessageKind.Success);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"게시판 개설 신청에 실패했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
        finally
        {
            IsSavingRequest = false;
        }
    }

    public async Task<PlatformCommunityCommandResult> ReviewAsync(
        string appKey,
        PlatformCommunityBoardResponse board,
        bool approve,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var memo = ReviewMemos.TryGetValue(board.Id, out var value) ? value : string.Empty;
            if (approve)
            {
                await _communityService.ApproveBoardAsync(board.Id, memo, cancellationToken);
            }
            else
            {
                await _communityService.RejectBoardAsync(board.Id, memo, cancellationToken);
            }

            await LoadAsync(appKey, canManageCommunityPosts: true, cancellationToken);
            return new(
                true,
                approve
                    ? $"'{board.Title}' 게시판을 승인했습니다."
                    : $"'{board.Title}' 게시판 신청을 반려했습니다.",
                CommunityComposerMessageKind.Success);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"게시판 검토 처리에 실패했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }
}

public sealed class PlatformCommunityCommentForm : ObservableObject
{
    private string _nickname = string.Empty;
    private string _password = string.Empty;
    private string _body = string.Empty;

    public string Nickname { get => _nickname; set => SetProperty(ref _nickname, value ?? string.Empty); }
    public string Password { get => _password; set => SetProperty(ref _password, value ?? string.Empty); }
    public string Body { get => _body; set => SetProperty(ref _body, value ?? string.Empty); }
    public bool IsValid => !string.IsNullOrWhiteSpace(Nickname)
                           && !string.IsNullOrWhiteSpace(Password)
                           && !string.IsNullOrWhiteSpace(Body);
}

/// <summary>
/// 게시글 댓글, 관심 역할과 가원장 참여 상태를 게시글별로 관리합니다.
/// </summary>
public sealed class PlatformCommunityPostEngagementViewModel(
    PlatformCommunityService communityService,
    CommunityPostJourneyCollectionViewModel? journeys = null) : ObservableObject
{
    private string? _selectedSeedPostTitle;

    public string RecommendationSessionKey { get; } = Guid.NewGuid().ToString("N");
    public Dictionary<long, PlatformCommunityCommentForm> CommentForms { get; } = [];
    public Dictionary<long, PlatformCommunityCommentForm> AttachmentCommentForms { get; } = [];
    public Dictionary<long, CommunityPostOpportunityListResponse> Opportunities { get; } = [];
    public CommunityPostJourneyCollectionViewModel Journeys { get; } = journeys ?? new();
    public Dictionary<long, HashSet<string>> SelectedParticipationOptionIds { get; } = [];
    public HashSet<long> PendingPostParticipationIds { get; } = [];
    public HashSet<long> ExpandedCommentPostIds { get; } = [];

    public string? SelectedSeedPostTitle
    {
        get => _selectedSeedPostTitle;
        set => SetProperty(ref _selectedSeedPostTitle, value);
    }

    public PlatformCommunityCommentForm GetCommentForm(long postId)
        => GetOrCreateForm(CommentForms, postId);

    public PlatformCommunityCommentForm GetAttachmentCommentForm(long attachmentId)
        => GetOrCreateForm(AttachmentCommentForms, attachmentId);

    public HashSet<string> GetParticipationOptionSelection(long postId)
    {
        if (!SelectedParticipationOptionIds.TryGetValue(postId, out var selected))
        {
            selected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            SelectedParticipationOptionIds[postId] = selected;
        }

        return selected;
    }

    public bool IsParticipationPending(long postId)
        => PendingPostParticipationIds.Contains(postId);

    public bool IsParticipationRoleSelected(long postId, string optionId)
        => !string.IsNullOrWhiteSpace(optionId)
           && SelectedParticipationOptionIds.TryGetValue(postId, out var selected)
           && selected.Contains(optionId);

    public bool IsCommentsExpanded(long postId)
        => ExpandedCommentPostIds.Contains(postId);

    public void ToggleComments(long postId)
    {
        if (!ExpandedCommentPostIds.Add(postId))
        {
            ExpandedCommentPostIds.Remove(postId);
        }

        OnPropertyChanged(nameof(ExpandedCommentPostIds));
    }

    public void SetOpportunity(long postId, CommunityPostOpportunityListResponse? opportunity)
    {
        if (opportunity is null)
        {
            Opportunities.Remove(postId);
        }
        else
        {
            Opportunities[postId] = opportunity;
        }

        Journeys.Set(postId, opportunity?.Journey);

        OnPropertyChanged(nameof(Opportunities));
    }

    public async Task RefreshOpportunityAsync(
        long postId,
        CancellationToken cancellationToken = default)
    {
        var result = await communityService.GetPostOpportunitiesAsync(
            postId,
            cancellationToken: cancellationToken);
        SetOpportunity(postId, result);
    }

    public async Task<PlatformCommunityCommandResult> StartParticipationAsync(
        long postId,
        CancellationToken cancellationToken = default)
    {
        if (!PendingPostParticipationIds.Add(postId))
        {
            return new(false);
        }

        try
        {
            await communityService.StartPostParticipationAsync(
                postId,
                new StartCommunityPostParticipationRequest
                {
                    DisplayLanguageCode = CommunityDisplayLanguageCodes.Korean,
                    ConfirmExplicitStart = true,
                    ConfirmNonBindingParticipation = true
                },
                cancellationToken);
            await RefreshOpportunityAsync(postId, cancellationToken);
            return new(
                true,
                "게시글에서 비구속적 관심 모집을 시작했습니다.",
                CommunityComposerMessageKind.Success);
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode is System.Net.HttpStatusCode.Unauthorized
                or System.Net.HttpStatusCode.Forbidden)
        {
            return new(
                false,
                "관심 모집을 시작하려면 먼저 로그인해 주세요.",
                CommunityComposerMessageKind.Warning);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"관심 모집을 시작하지 못했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
        finally
        {
            PendingPostParticipationIds.Remove(postId);
        }
    }

    public async Task<PlatformCommunityCommandResult> ToggleParticipationRoleAsync(
        long postId,
        CommunityPostParticipationRoleOptionResponse role,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(role.OptionId)
            || !Opportunities.TryGetValue(postId, out var opportunity)
            || opportunity.Participation.InterestVoteId is not Guid voteId
            || !PendingPostParticipationIds.Add(postId))
        {
            return new(false);
        }

        var selected = GetParticipationOptionSelection(postId);
        var wasSelected = selected.Contains(role.OptionId);
        if (wasSelected && selected.Count == 1)
        {
            PendingPostParticipationIds.Remove(postId);
            return new(
                false,
                "관심 역할은 하나 이상 남겨야 합니다.",
                CommunityComposerMessageKind.Info);
        }

        if (wasSelected)
        {
            selected.Remove(role.OptionId);
        }
        else
        {
            selected.Add(role.OptionId);
        }

        try
        {
            var displayName = CommentForms.TryGetValue(postId, out var commentForm)
                              && !string.IsNullOrWhiteSpace(commentForm.Nickname)
                ? commentForm.Nickname.Trim()
                : "관심 참여자";
            await communityService.CastCommunityVoteAsync(
                voteId,
                new CommunityVoteCastRequest
                {
                    VoterKey = $"community-interest:{RecommendationSessionKey}",
                    VoterDisplayName = displayName,
                    OptionIds = selected.ToArray()
                },
                cancellationToken);
            await RefreshOpportunityAsync(postId, cancellationToken);
            return new(
                true,
                "가능한 참여 역할을 반영했습니다.",
                CommunityComposerMessageKind.Success);
        }
        catch (Exception ex)
        {
            if (wasSelected)
            {
                selected.Add(role.OptionId);
            }
            else
            {
                selected.Remove(role.OptionId);
            }

            return new(
                false,
                $"관심 역할을 반영하지 못했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
        finally
        {
            PendingPostParticipationIds.Remove(postId);
        }
    }

    public async Task<PlatformCommunityCommandResult> PromoteParticipationAsync(
        long postId,
        PromoteCommunityPostParticipationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await communityService.PromotePostParticipationAsync(
                postId,
                request,
                cancellationToken);
            await RefreshOpportunityAsync(postId, cancellationToken);
            return new(
                true,
                result?.ReusedExistingProvisionalLedger == true
                    ? "이미 만들어진 가원장을 다시 확인했습니다."
                    : "모인 관심을 비구속적 가원장으로 기록하고 참여자 알림을 요청했습니다.",
                CommunityComposerMessageKind.Success,
                RefreshPosts: true);
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode is System.Net.HttpStatusCode.Unauthorized
                or System.Net.HttpStatusCode.Forbidden)
        {
            return new(
                false,
                "로그인한 게시글 작성자만 가원장을 만들 수 있습니다.",
                CommunityComposerMessageKind.Warning);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"가원장을 만들지 못했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    public async Task<PlatformCommunityCommandResult> JoinProfessionalRoleAsync(
        long postId,
        string provisionalLedgerId,
        string roleCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await communityService.JoinPostProfessionalRoleAsync(
                postId,
                new JoinCommunityPostProfessionalRequest
                {
                    ProvisionalLedgerId = provisionalLedgerId,
                    ProfessionalRoleCode = roleCode,
                    DisplayLanguageCode = CommunityDisplayLanguageCodes.Korean,
                    ConfirmProfessionalCapacity = true,
                    ConfirmVoluntaryNonBindingParticipation = true,
                    ConfirmParticipantNotification = true
                },
                cancellationToken);
            await RefreshOpportunityAsync(postId, cancellationToken);
            return new(
                true,
                result?.ReusedExistingParticipation == true
                    ? "이미 참여 중인 역할을 다시 확인했습니다."
                    : "거래 참여팀 역할 참여를 기록하고 기존 참여자 알림을 요청했습니다.",
                CommunityComposerMessageKind.Success,
                RefreshPosts: true);
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode is System.Net.HttpStatusCode.Unauthorized
                or System.Net.HttpStatusCode.Forbidden)
        {
            return new(
                false,
                "현재 계정의 플랫폼 프로필에서 이 역할을 확인할 수 없습니다.",
                CommunityComposerMessageKind.Warning);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"역할 참여를 기록하지 못했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    public async Task<PlatformCommunityCommandResult> JoinPartyRoleAsync(
        long postId,
        string provisionalLedgerId,
        string roleCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await communityService.JoinPostPartyRoleAsync(
                postId,
                new JoinCommunityPostPartyRoleRequest
                {
                    ProvisionalLedgerId = provisionalLedgerId,
                    PartyRoleCode = roleCode,
                    DisplayLanguageCode = CommunityDisplayLanguageCodes.Korean,
                    ConfirmRoleCapacity = true,
                    ConfirmVoluntaryNonBindingParticipation = true,
                    ConfirmParticipantNotification = true
                },
                cancellationToken);
            await RefreshOpportunityAsync(postId, cancellationToken);
            return new(
                true,
                result?.ReusedExistingParticipation == true
                    ? "이미 수락한 거래 역할을 다시 확인했습니다."
                    : "비구속적 거래 역할 수락을 기록하고 기존 참여자 알림을 요청했습니다.",
                CommunityComposerMessageKind.Success,
                RefreshPosts: true);
        }
        catch (HttpRequestException ex) when (
            ex.StatusCode is System.Net.HttpStatusCode.Unauthorized
                or System.Net.HttpStatusCode.Forbidden)
        {
            return new(
                false,
                "거래 역할을 수락하려면 로그인해야 합니다.",
                CommunityComposerMessageKind.Warning);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"거래 역할 수락을 기록하지 못했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    public async Task<PlatformCommunityCommandResult> RecommendAsync(
        long postId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await communityService.RecommendAsync(postId, RecommendationSessionKey, cancellationToken);
            return new(true, RefreshPosts: true);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"추천 처리에 실패했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    public async Task<PlatformCommunityCommandResult> SetOperatorPinAsync(
        long postId,
        bool isPinned,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await communityService.SetOperatorPinAsync(postId, isPinned, cancellationToken);
            return new(true, RefreshPosts: true);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"상단 고정 처리에 실패했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    public async Task<PlatformCommunityCommandResult> SaveCommentAsync(
        long postId,
        CancellationToken cancellationToken = default)
    {
        var form = GetCommentForm(postId);
        if (!form.IsValid)
        {
            return new(
                false,
                "댓글 닉네임, 비밀번호, 내용을 입력하세요.",
                CommunityComposerMessageKind.Warning);
        }

        try
        {
            await communityService.CreateCommentAsync(
                postId,
                new PlatformCommunityPostCommentCreateRequest
                {
                    Nickname = form.Nickname,
                    Password = form.Password,
                    Body = form.Body
                },
                cancellationToken);
            CommentForms.Remove(postId);
            return new(true, RefreshPosts: true);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"댓글 등록에 실패했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    public async Task<PlatformCommunityCommandResult> DeleteCommentAsync(
        long postId,
        long commentId,
        CancellationToken cancellationToken = default)
    {
        var form = GetCommentForm(postId);
        if (string.IsNullOrWhiteSpace(form.Password))
        {
            return new(
                false,
                "Enter the comment password before deleting.",
                CommunityComposerMessageKind.Warning);
        }

        try
        {
            await communityService.DeleteCommentAsync(postId, commentId, form.Password, cancellationToken);
            return new(true, RefreshPosts: true);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"Comment delete failed: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    public async Task<PlatformCommunityCommandResult> ReportCommentAsync(
        long commentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await communityService.ReportCommentAsync(commentId, cancellationToken);
            return new(true, "Comment report received.", CommunityComposerMessageKind.Info);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"Comment report failed: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    public async Task<PlatformCommunityCommandResult> SaveAttachmentCommentAsync(
        long attachmentId,
        CancellationToken cancellationToken = default)
    {
        var form = GetAttachmentCommentForm(attachmentId);
        if (!form.IsValid)
        {
            return new(
                false,
                "Image comment nickname, password, and body are required.",
                CommunityComposerMessageKind.Warning);
        }

        try
        {
            await communityService.CreateAttachmentCommentAsync(
                attachmentId,
                new PlatformCommunityPostAttachmentCommentCreateRequest
                {
                    Nickname = form.Nickname,
                    Password = form.Password,
                    Body = form.Body
                },
                cancellationToken);
            AttachmentCommentForms.Remove(attachmentId);
            return new(true, RefreshPosts: true);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"Image comment failed: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    public async Task<PlatformCommunityCommandResult> DeleteAttachmentCommentAsync(
        long attachmentId,
        long commentId,
        CancellationToken cancellationToken = default)
    {
        var form = GetAttachmentCommentForm(attachmentId);
        if (string.IsNullOrWhiteSpace(form.Password))
        {
            return new(
                false,
                "Enter the image comment password before deleting.",
                CommunityComposerMessageKind.Warning);
        }

        try
        {
            await communityService.DeleteAttachmentCommentAsync(
                attachmentId,
                commentId,
                form.Password,
                cancellationToken);
            return new(true, RefreshPosts: true);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"Image comment delete failed: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    public async Task<PlatformCommunityCommandResult> ReportAttachmentCommentAsync(
        long commentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await communityService.ReportAttachmentCommentAsync(commentId, cancellationToken);
            return new(true, "Image comment report received.", CommunityComposerMessageKind.Info);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"Image comment report failed: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    private static PlatformCommunityCommentForm GetOrCreateForm(
        IDictionary<long, PlatformCommunityCommentForm> forms,
        long key)
    {
        if (!forms.TryGetValue(key, out var form))
        {
            form = new PlatformCommunityCommentForm();
            forms[key] = form;
        }

        return form;
    }
}

/// <summary>
/// 글에 연결할 내 원장과 공개 원장 탐색, 상세 보기와 공유 설정 상태를 소유합니다.
/// </summary>
public sealed class PlatformCommunityLedgerPickerViewModel(
    PlatformCommunityService communityService) : ObservableObject
{
    private string _searchText = string.Empty;
    private string _scope = "전체";
    private string? _pendingLedgerId;
    private bool _isLoading;
    private bool _isPickerOpen;
    private bool _isDetailOpen;
    private bool _isHierarchyOpen;
    private bool _isDetailLoading;
    private bool _detailOpenedFromHierarchy;
    private bool _isSharingSaving;
    private bool _isSharedLedgerReusing;
    private string? _loadMessage;
    private string? _detailErrorMessage;
    private 커뮤니티원장공개설정Response? _sharingSettings;
    private PlatformCommunityPostLedgerContextResponse? _detailContext;
    private PlatformCommunityPostLedgerContextResponse? _hierarchyContext;

    public List<PlatformCommunityPostLedgerChoiceResponse> Items { get; } = [];

    public IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse> FilteredItems
        => Items
            .Where(MatchesScope)
            .Where(MatchesSearch)
            .OrderByDescending(ledger => ledger.내접근원장여부)
            .ThenByDescending(ledger => ledger.수정시각Utc)
            .ToArray();

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value ?? string.Empty))
            {
                OnPropertyChanged(nameof(FilteredItems));
            }
        }
    }

    public string Scope
    {
        get => _scope;
        set
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? "전체" : value.Trim();
            if (SetProperty(ref _scope, normalized))
            {
                OnPropertyChanged(nameof(FilteredItems));
            }
        }
    }

    public string? PendingLedgerId { get => _pendingLedgerId; set => SetProperty(ref _pendingLedgerId, value); }
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    public bool IsPickerOpen { get => _isPickerOpen; set => SetProperty(ref _isPickerOpen, value); }
    public bool IsDetailOpen { get => _isDetailOpen; set => SetProperty(ref _isDetailOpen, value); }
    public bool IsHierarchyOpen { get => _isHierarchyOpen; set => SetProperty(ref _isHierarchyOpen, value); }
    public bool IsDetailLoading { get => _isDetailLoading; set => SetProperty(ref _isDetailLoading, value); }
    public bool DetailOpenedFromHierarchy { get => _detailOpenedFromHierarchy; set => SetProperty(ref _detailOpenedFromHierarchy, value); }
    public bool IsSharingSaving { get => _isSharingSaving; set => SetProperty(ref _isSharingSaving, value); }
    public bool IsSharedLedgerReusing { get => _isSharedLedgerReusing; set => SetProperty(ref _isSharedLedgerReusing, value); }
    public string? LoadMessage { get => _loadMessage; set => SetProperty(ref _loadMessage, value); }
    public string? DetailErrorMessage { get => _detailErrorMessage; set => SetProperty(ref _detailErrorMessage, value); }
    public 커뮤니티원장공개설정Response? SharingSettings { get => _sharingSettings; set => SetProperty(ref _sharingSettings, value); }
    public PlatformCommunityPostLedgerContextResponse? DetailContext { get => _detailContext; set => SetProperty(ref _detailContext, value); }
    public PlatformCommunityPostLedgerContextResponse? HierarchyContext { get => _hierarchyContext; set => SetProperty(ref _hierarchyContext, value); }

    public void ReplaceItems(IEnumerable<PlatformCommunityPostLedgerChoiceResponse> items)
    {
        Items.Clear();
        Items.AddRange(items);
        OnPropertyChanged(nameof(Items));
        OnPropertyChanged(nameof(FilteredItems));
    }

    public void ResetFilters()
    {
        SearchText = string.Empty;
        Scope = "전체";
    }

    public bool IsPending(PlatformCommunityPostLedgerChoiceResponse ledger)
        => string.Equals(ledger.원장Id, PendingLedgerId, StringComparison.OrdinalIgnoreCase);

    public void NotifyItemsChanged()
    {
        OnPropertyChanged(nameof(Items));
        OnPropertyChanged(nameof(FilteredItems));
    }

    public void Open(string? attachedLedgerId)
    {
        IsPickerOpen = true;
        IsDetailOpen = false;
        IsHierarchyOpen = false;
        DetailOpenedFromHierarchy = false;
        DetailContext = null;
        HierarchyContext = null;
        DetailErrorMessage = null;
        SharingSettings = null;
        ResetFilters();
        PendingLedgerId = attachedLedgerId;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        LoadMessage = null;
        var items = new List<PlatformCommunityPostLedgerChoiceResponse>();
        var loginRequired = false;
        var sharedLoadFailed = false;
        try
        {
            items.AddRange(await communityService.GetMyLedgersAsync(
                cancellationToken: cancellationToken));
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            loginRequired = true;
        }
        catch (Exception)
        {
            loginRequired = true;
        }

        try
        {
            var sharedLedgers = await communityService.GetSharedLedgersAsync(
                cancellationToken: cancellationToken);
            foreach (var ledger in sharedLedgers)
            {
                if (!items.Any(item => string.Equals(
                        item.원장Id,
                        ledger.원장Id,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    items.Add(ledger);
                }
            }
        }
        catch (Exception)
        {
            sharedLoadFailed = true;
        }

        ReplaceItems(items);
        if (Items.Count == 0)
        {
            LoadMessage = sharedLoadFailed
                ? "원장 목록을 불러오지 못했습니다. 서버 연결 상태를 확인해 주세요."
                : loginRequired
                    ? "로그인하면 내 원장을 함께 볼 수 있습니다. 현재 재공유가 허용된 공개 원장은 없습니다."
                    : "내 원장과 재공유가 허용된 공개 원장이 아직 없습니다.";
        }

        IsLoading = false;
    }

    public async Task<PlatformCommunityCommandResult> OpenPendingDetailAsync(
        CancellationToken cancellationToken = default)
    {
        var ledger = Items.FirstOrDefault(item => IsPending(item));
        if (ledger is null)
        {
            return new(
                false,
                "내부 데이터를 확인할 원장을 먼저 선택해 주세요.",
                CommunityComposerMessageKind.Warning);
        }

        IsDetailOpen = true;
        IsHierarchyOpen = false;
        DetailOpenedFromHierarchy = false;
        IsDetailLoading = true;
        DetailContext = null;
        DetailErrorMessage = null;
        try
        {
            DetailContext = await communityService.GetLedgerContextAsync(ledger.원장Id, cancellationToken);
            if (DetailContext is null)
            {
                DetailErrorMessage = "이 원장의 공개 범위 또는 참여 권한을 확인해 주세요.";
            }
            else if (DetailContext.포함원장목록.Count > 0)
            {
                HierarchyContext = DetailContext;
                IsHierarchyOpen = true;
                IsDetailOpen = false;
            }

            return new(true);
        }
        catch (Exception)
        {
            DetailErrorMessage = "원장 내부 데이터를 불러오지 못했습니다. 서버 연결 상태를 확인해 주세요.";
            return new(false);
        }
        finally
        {
            IsDetailLoading = false;
        }
    }

    public void OpenHierarchyLedgerDiagram(PlatformCommunityPostLedgerContextResponse context)
    {
        DetailContext = context;
        DetailErrorMessage = null;
        IsDetailLoading = false;
        DetailOpenedFromHierarchy = true;
        IsHierarchyOpen = false;
        IsDetailOpen = true;
    }

    public async Task RefreshDetailAsync(CancellationToken cancellationToken = default)
    {
        var ledgerId = DetailContext?.원장Id ?? PendingLedgerId;
        if (string.IsNullOrWhiteSpace(ledgerId))
        {
            return;
        }

        try
        {
            var refreshed = await communityService.GetLedgerContextAsync(ledgerId, cancellationToken);
            if (refreshed is not null)
            {
                DetailContext = refreshed;
                DetailErrorMessage = null;
            }
        }
        catch (Exception)
        {
            DetailErrorMessage = "원장 최신 상태를 불러오지 못했습니다. 잠시 후 다시 시도해 주세요.";
        }
    }

    public void ReturnToCompose()
    {
        IsDetailOpen = false;
        IsHierarchyOpen = false;
        IsPickerOpen = false;
        PendingLedgerId = null;
        DetailOpenedFromHierarchy = false;
        HierarchyContext = null;
    }

    public void ReturnToPicker()
    {
        IsDetailOpen = false;
        IsHierarchyOpen = false;
        IsPickerOpen = true;
        DetailContext = null;
        DetailErrorMessage = null;
        DetailOpenedFromHierarchy = false;
        HierarchyContext = null;
    }

    public bool ReturnFromDetail()
    {
        if (DetailOpenedFromHierarchy && HierarchyContext is not null)
        {
            IsDetailOpen = false;
            IsHierarchyOpen = true;
            DetailContext = null;
            DetailErrorMessage = null;
            DetailOpenedFromHierarchy = false;
            return true;
        }

        ReturnToPicker();
        return false;
    }

    public async Task<PlatformCommunityCommandResult> LoadSharingSettingsAsync(
        string? attachedLedgerId,
        CancellationToken cancellationToken = default)
    {
        var selected = Items.FirstOrDefault(item => string.Equals(
            item.원장Id,
            attachedLedgerId,
            StringComparison.OrdinalIgnoreCase));
        if (selected?.내가만든원장 != true)
        {
            return new(
                false,
                "원장 생성자만 공개 설정을 변경할 수 있습니다.",
                CommunityComposerMessageKind.Warning);
        }

        try
        {
            SharingSettings = await communityService.GetLedgerSharingSettingsAsync(
                selected.원장Id,
                cancellationToken);
            return new(true);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"원장 공개 설정을 불러오지 못했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
    }

    public async Task<PlatformCommunityCommandResult> SaveSharingSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        if (SharingSettings is null)
        {
            return new(false);
        }

        IsSharingSaving = true;
        try
        {
            SharingSettings = await communityService.UpdateLedgerSharingSettingsAsync(
                SharingSettings.원장Id,
                new 커뮤니티원장공개설정변경Request
                {
                    공개범위 = SharingSettings.공개범위,
                    재사용허용여부 = SharingSettings.재사용허용여부,
                    재공유허용여부 = SharingSettings.재공유허용여부,
                    기대Revision = SharingSettings.Revision,
                    공개항목Key목록 = SharingSettings.항목목록
                        .Where(item => item.공개여부)
                        .Select(item => item.항목Key)
                        .ToArray()
                },
                cancellationToken);
            var message = SharingSettings?.공개범위 == 커뮤니티원장공개범위.비공개
                ? "원장을 비공개로 전환했습니다."
                : "선택한 항목만 공개되도록 원장 공유 설정을 저장했습니다.";
            await LoadAsync(cancellationToken);
            return new(true, message, CommunityComposerMessageKind.Success);
        }
        catch (Exception ex)
        {
            return new(
                false,
                $"원장 공개 설정 저장에 실패했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error);
        }
        finally
        {
            IsSharingSaving = false;
        }
    }

    public async Task<PlatformCommunityLedgerReuseResult> ReuseSharedLedgerAsync(
        string ledgerId,
        CancellationToken cancellationToken = default)
    {
        if (IsSharedLedgerReusing)
        {
            return new(new(false));
        }

        IsSharedLedgerReusing = true;
        try
        {
            var reused = await communityService.ReuseSharedLedgerAsync(
                ledgerId,
                cancellationToken: cancellationToken);
            if (reused is null)
            {
                return new(new(
                    false,
                    "원장 사본을 만들지 못했습니다.",
                    CommunityComposerMessageKind.Error));
            }

            await LoadAsync(cancellationToken);
            return new(
                new(
                    true,
                    $"'{reused.제목}'을 내 비공개 원장으로 가져와 글에 첨부했습니다.",
                    CommunityComposerMessageKind.Success),
                reused);
        }
        catch (Exception ex)
        {
            return new(new(
                false,
                $"원장을 가져오지 못했습니다: {ex.Message}",
                CommunityComposerMessageKind.Error));
        }
        finally
        {
            IsSharedLedgerReusing = false;
        }
    }

    private bool MatchesScope(PlatformCommunityPostLedgerChoiceResponse ledger)
        => Scope switch
        {
            "내 원장" => ledger.내접근원장여부,
            "공개 원장" => !ledger.내접근원장여부,
            _ => true
        };

    private bool MatchesSearch(PlatformCommunityPostLedgerChoiceResponse ledger)
    {
        var searchText = SearchText.Trim();
        return searchText.Length == 0
               || Contains(ledger.제목, searchText)
               || Contains(ledger.원장템플릿명, searchText)
               || Contains(ledger.상태, searchText)
               || Contains(ledger.WorkflowTag, searchText)
               || Contains(ledger.참여역할, searchText)
               || Contains(ledger.원장Id, searchText);
    }

    private static bool Contains(string? value, string searchText)
        => !string.IsNullOrWhiteSpace(value)
           && value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
}
