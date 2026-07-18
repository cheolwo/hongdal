using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public sealed record CommunityPostComposerSaveResult(
    bool Succeeded,
    bool WasEditing,
    PlatformCommunityPostResponse? Post,
    string Message)
{
    public CommunityPostComposerSnapshot? SubmittedDraft { get; init; }
    public bool WasScheduled { get; init; }
    public DateTime? ScheduledPublishAtUtc { get; init; }
}

public sealed class CommunityPostComposerViewModel : 조립ViewModelBase
{
    private const long MaxUploadFileBytes = 5 * 1024 * 1024;
    private readonly PlatformCommunityService _communityService;
    private readonly ICommunityPostComposerDraftStore _draftStore;
    private string _appKey = string.Empty;
    private string _defaultRoleTag = "플랫폼 구성원";
    private string? _loadedDraftAppKey;
    private bool _isOpen;
    private bool _isSettingsOpen;
    private bool _isSaving;
    private long? _editingPostId;
    private CommunityPostComposerSnapshot? _localDraft;
    private DateTime? _localDraftSavedAtUtc;
    private string? _statusMessage;
    private CommunityComposerMessageKind _statusKind = CommunityComposerMessageKind.Info;
    private bool _allowScheduledPublication;
    private bool _isScheduledPublication;
    private DateTime? _scheduledPublishDateLocal;
    private TimeSpan? _scheduledPublishTimeLocal;

    public CommunityPostComposerViewModel(
        PlatformCommunityService communityService,
        ICommunityPostComposerDraftStore draftStore)
    {
        _communityService = communityService;
        _draftStore = draftStore;
        Draft = 하위ViewModel등록(new CommunityPostComposerDraftViewModel());
    }

    public CommunityPostComposerDraftViewModel Draft { get; }
    public List<IBrowserFile> SelectedFiles { get; } = [];

    public bool IsOpen { get => _isOpen; internal set => SetProperty(ref _isOpen, value); }
    public bool IsSettingsOpen { get => _isSettingsOpen; internal set => SetProperty(ref _isSettingsOpen, value); }
    public bool IsSaving { get => _isSaving; private set => SetProperty(ref _isSaving, value); }
    public long? EditingPostId { get => _editingPostId; internal set => SetProperty(ref _editingPostId, value); }
    public DateTime? LocalDraftSavedAtUtc { get => _localDraftSavedAtUtc; private set => SetProperty(ref _localDraftSavedAtUtc, value); }
    public string? StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public CommunityComposerMessageKind StatusKind { get => _statusKind; private set => SetProperty(ref _statusKind, value); }
    public bool AllowScheduledPublication
    {
        get => _allowScheduledPublication;
        private set => SetProperty(ref _allowScheduledPublication, value);
    }

    public bool IsScheduledPublication
    {
        get => _isScheduledPublication;
        set
        {
            var normalized = AllowScheduledPublication && EditingPostId is null && value;
            if (!SetProperty(ref _isScheduledPublication, normalized))
            {
                return;
            }

            if (normalized)
            {
                EnsureScheduledPublicationDefaults();
            }

            OnPropertyChanged(nameof(ScheduledPublishAtUtc));
        }
    }

    public DateTime? ScheduledPublishDateLocal
    {
        get => _scheduledPublishDateLocal;
        set
        {
            if (SetProperty(ref _scheduledPublishDateLocal, value?.Date))
            {
                OnPropertyChanged(nameof(ScheduledPublishAtUtc));
            }
        }
    }

    public TimeSpan? ScheduledPublishTimeLocal
    {
        get => _scheduledPublishTimeLocal;
        set
        {
            if (SetProperty(ref _scheduledPublishTimeLocal, value))
            {
                OnPropertyChanged(nameof(ScheduledPublishAtUtc));
            }
        }
    }

    public DateTime? ScheduledPublishAtUtc
        => BuildScheduledPublishAtUtc();

    public string LocalTimeZoneDisplayName
        => TimeZoneInfo.Local.DisplayName;

    public void Configure(
        string appKey,
        string defaultRoleTag,
        bool allowScheduledPublication = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultRoleTag);

        if (!string.Equals(_appKey, appKey, StringComparison.OrdinalIgnoreCase))
        {
            _appKey = appKey.Trim();
            _loadedDraftAppKey = null;
            _localDraft = null;
            LocalDraftSavedAtUtc = null;
        }

        _defaultRoleTag = defaultRoleTag.Trim();
        AllowScheduledPublication = allowScheduledPublication;
        if (!allowScheduledPublication)
        {
            IsScheduledPublication = false;
        }

        if (string.IsNullOrWhiteSpace(Draft.RoleTag))
        {
            Draft.RoleTag = _defaultRoleTag;
        }
    }

    public async Task LoadLocalDraftAsync(CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        if (string.Equals(_loadedDraftAppKey, _appKey, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _loadedDraftAppKey = _appKey;
        try
        {
            _localDraft = await _draftStore.LoadAsync(_appKey, cancellationToken);
            LocalDraftSavedAtUtc = _localDraft?.SavedAtUtc;
            if (IsOpen)
            {
                RestoreLocalDraftIfNeeded();
            }
        }
        catch (Exception)
        {
            _localDraft = null;
            LocalDraftSavedAtUtc = null;
        }
    }

    public void Open()
    {
        RestoreLocalDraftIfNeeded();
        IsOpen = true;
    }

    public void Close()
        => IsOpen = false;

    public void ToggleSettings()
        => IsSettingsOpen = !IsSettingsOpen;

    public void OpenSettings()
        => IsSettingsOpen = true;

    public void SelectCategory(string category)
    {
        if (Draft.IsSalesPost
            && !string.Equals(category, PlatformCommunityPostCategories.Sales, StringComparison.OrdinalIgnoreCase))
        {
            Draft.Category = PlatformCommunityPostCategories.Sales;
            SetStatus(
                "판매 정보가 붙은 글은 판매 게시판에 자동으로 등록됩니다.",
                CommunityComposerMessageKind.Info);
            return;
        }

        Draft.Category = category;
        ClearStatus();
    }

    public void SetFiles(IEnumerable<IBrowserFile> files)
    {
        SelectedFiles.Clear();
        SelectedFiles.AddRange(files.Take(5));
        OnPropertyChanged(nameof(SelectedFiles));
    }

    public void BeginEdit(PlatformCommunityPostResponse post)
    {
        ArgumentNullException.ThrowIfNull(post);
        EditingPostId = post.Id;
        IsScheduledPublication = false;
        SelectedFiles.Clear();
        Draft.Apply(post);
        SetStatus(
            "작성할 때 입력한 비밀번호를 넣고 수정 저장을 누르세요.",
            CommunityComposerMessageKind.Info);
        Open();
        IsSettingsOpen = true;
    }

    public void CancelEdit()
    {
        Reset();
        IsOpen = false;
    }

    public void Reset()
    {
        EditingPostId = null;
        IsScheduledPublication = false;
        ScheduledPublishDateLocal = null;
        ScheduledPublishTimeLocal = null;
        IsSettingsOpen = false;
        SelectedFiles.Clear();
        Draft.Reset(_defaultRoleTag);
        ClearStatus();
        OnPropertyChanged(nameof(SelectedFiles));
    }

    public void SetStatus(string message, CommunityComposerMessageKind kind)
    {
        StatusKind = kind;
        StatusMessage = message;
    }

    public void ClearStatus()
        => StatusMessage = null;

    public async Task SaveLocalDraftAsync(CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        if (!Draft.HasContent)
        {
            SetStatus(
                "임시 저장할 제목, 내용, 링크 또는 원장 연결이 없습니다.",
                CommunityComposerMessageKind.Warning);
            return;
        }

        var snapshot = Draft.CreateSnapshot(DateTime.UtcNow);
        try
        {
            await _draftStore.SaveAsync(_appKey, snapshot, cancellationToken);
            _localDraft = snapshot;
            LocalDraftSavedAtUtc = snapshot.SavedAtUtc;
            SetStatus(
                SelectedFiles.Count == 0
                    ? "이 브라우저에 임시 저장했습니다. 글 비밀번호는 저장하지 않습니다."
                    : "이 브라우저에 임시 저장했습니다. 글 비밀번호와 첨부 사진은 저장하지 않습니다.",
                CommunityComposerMessageKind.Success);
        }
        catch (Exception)
        {
            SetStatus(
                "브라우저 임시 저장을 사용할 수 없습니다. 현재 화면의 내용은 닫기 전까지 유지됩니다.",
                CommunityComposerMessageKind.Warning);
        }
    }

    public async Task<CommunityPostComposerSaveResult> SaveAsync(
        CancellationToken cancellationToken = default)
    {
        EnsureConfigured();
        if (IsSaving)
        {
            return new(false, EditingPostId is not null, null, "이미 글을 저장하고 있습니다.");
        }

        var validationMessage = Draft.Validate();
        if (validationMessage is not null)
        {
            IsSettingsOpen = true;
            SetStatus(validationMessage, CommunityComposerMessageKind.Warning);
            return new(false, EditingPostId is not null, null, validationMessage);
        }

        var scheduleValidationMessage = ValidateScheduledPublication();
        if (scheduleValidationMessage is not null)
        {
            IsSettingsOpen = true;
            SetStatus(scheduleValidationMessage, CommunityComposerMessageKind.Warning);
            return new(false, EditingPostId is not null, null, scheduleValidationMessage);
        }

        IsSaving = true;
        var wasEditing = EditingPostId is not null;
        var scheduledPublishAtUtc = !wasEditing && IsScheduledPublication
            ? ScheduledPublishAtUtc
            : null;
        var submittedDraft = Draft.CreateSnapshot(DateTime.UtcNow);
        try
        {
            var saved = EditingPostId is long postId
                ? await _communityService.UpdatePostAsync(
                    postId,
                    Draft.CreateUpdateRequest(),
                    cancellationToken)
                : scheduledPublishAtUtc is DateTime publishAtUtc
                    ? await _communityService.SchedulePostAsync(
                        new PlatformCommunityPostScheduleCreateRequest
                        {
                            Post = Draft.CreateRequest(_appKey),
                            ScheduledPublishAtUtc = publishAtUtc
                        },
                        cancellationToken)
                    : await _communityService.CreatePostAsync(
                        Draft.CreateRequest(_appKey),
                        cancellationToken);

            if (saved is null)
            {
                const string emptyResponseMessage = "글 저장 응답을 확인하지 못했습니다.";
                SetStatus(emptyResponseMessage, CommunityComposerMessageKind.Error);
                return new(false, wasEditing, null, emptyResponseMessage);
            }

            foreach (var file in SelectedFiles)
            {
                await _communityService.UploadAttachmentAsync(
                    saved.Id,
                    Draft.Password,
                    file,
                    MaxUploadFileBytes,
                    cancellationToken);
            }

            await ClearLocalDraftAsync(cancellationToken);
            Reset();
            IsOpen = false;
            var message = wasEditing
                ? "글을 수정했습니다."
                : scheduledPublishAtUtc is DateTime scheduledAtUtc
                    ? $"{scheduledAtUtc.ToLocalTime():yyyy-MM-dd HH:mm}에 발행하도록 예약했습니다."
                    : "글을 등록했습니다.";
            SetStatus(message, CommunityComposerMessageKind.Success);
            return new(true, wasEditing, saved, message)
            {
                SubmittedDraft = submittedDraft,
                WasScheduled = scheduledPublishAtUtc.HasValue,
                ScheduledPublishAtUtc = scheduledPublishAtUtc
            };
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            const string loginRequiredMessage =
                "선택한 게시판은 로그인한 사용자만 글을 작성할 수 있습니다. 로그인 후 다시 등록해 주세요.";
            SetStatus(loginRequiredMessage, CommunityComposerMessageKind.Warning);
            return new(false, wasEditing, null, loginRequiredMessage);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
        {
            const string forbiddenMessage = "비밀번호가 맞지 않아 수정할 수 없습니다.";
            SetStatus(forbiddenMessage, CommunityComposerMessageKind.Error);
            return new(false, wasEditing, null, forbiddenMessage);
        }
        catch (Exception ex)
        {
            var message = $"저장에 실패했습니다: {ex.Message}";
            SetStatus(message, CommunityComposerMessageKind.Error);
            return new(false, wasEditing, null, message);
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool RestoreLocalDraftIfNeeded()
    {
        if (EditingPostId is not null || _localDraft is null || Draft.HasContent)
        {
            return false;
        }

        Draft.Apply(_localDraft);
        SetStatus(
            "이 브라우저에 임시 저장한 글을 불러왔습니다.",
            CommunityComposerMessageKind.Info);
        return true;
    }

    private async Task ClearLocalDraftAsync(CancellationToken cancellationToken)
    {
        _localDraft = null;
        LocalDraftSavedAtUtc = null;
        try
        {
            await _draftStore.ClearAsync(_appKey, cancellationToken);
        }
        catch (Exception)
        {
            // A successful post must not fail because browser storage is unavailable.
        }
    }

    private string? ValidateScheduledPublication()
    {
        if (!IsScheduledPublication)
        {
            return null;
        }

        if (!AllowScheduledPublication || EditingPostId is not null)
        {
            return "새 글을 작성할 때만 예약 발행을 사용할 수 있습니다.";
        }

        if (ScheduledPublishAtUtc is not DateTime publishAtUtc)
        {
            return "예약 발행 날짜와 시간을 모두 선택해 주세요.";
        }

        var now = DateTime.UtcNow;
        if (publishAtUtc < now.Add(PlatformCommunityPostSchedulePolicy.MinimumLeadTime))
        {
            return "예약 발행 시각은 현재보다 1분 이후여야 합니다.";
        }

        if (publishAtUtc > now.Add(PlatformCommunityPostSchedulePolicy.MaximumLeadTime))
        {
            return "예약 발행 시각은 현재부터 365일 이내여야 합니다.";
        }

        return null;
    }

    private DateTime? BuildScheduledPublishAtUtc()
    {
        if (ScheduledPublishDateLocal is not DateTime date
            || ScheduledPublishTimeLocal is not TimeSpan time)
        {
            return null;
        }

        var local = DateTime.SpecifyKind(date.Date.Add(time), DateTimeKind.Unspecified);
        return local.ToUniversalTime();
    }

    private void EnsureScheduledPublicationDefaults()
    {
        if (ScheduledPublishDateLocal.HasValue && ScheduledPublishTimeLocal.HasValue)
        {
            return;
        }

        var nextHour = DateTime.Now.AddHours(2);
        var defaultLocal = new DateTime(
            nextHour.Year,
            nextHour.Month,
            nextHour.Day,
            nextHour.Hour,
            0,
            0,
            DateTimeKind.Local);
        ScheduledPublishDateLocal = defaultLocal.Date;
        ScheduledPublishTimeLocal = defaultLocal.TimeOfDay;
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_appKey))
        {
            throw new InvalidOperationException("커뮤니티 글쓰기 AppKey가 설정되지 않았습니다.");
        }
    }
}
