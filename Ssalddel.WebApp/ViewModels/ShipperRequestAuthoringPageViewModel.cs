using Microsoft.AspNetCore.Components;
using MudBlazor;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Privacy;
using Ssalddel.Contracts.Shipper.Request;
using Ssalddel.Ui.Common.Areas.App.Models;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Ssalddel.WebApp.Services;

namespace Ssalddel.WebApp.ViewModels;

/// <summary>
/// Web route adapter가 자동 저장, 서버 기능 경계, 커뮤니티 글 가져오기를 조율합니다.
/// 실제 입력 draft와 validation은 Ui.Common의 <see cref="운송의뢰작성ViewModel"/>이 소유합니다.
/// </summary>
public sealed class ShipperRequestAuthoringPageViewModel
{
    private const string DomesticTransportFeatureKey = "DomesticTransportWorkflow";

    private static readonly IReadOnlyList<string> DefaultVehicleTypes =
    [
        "오토바이 퀵",
        "1톤 카고",
        "1.4톤 윙바디",
        "2.5톤 탑차",
        "냉동탑차",
        "5톤 트럭",
        "컨테이너 트레일러"
    ];

    private readonly 운송의뢰자동저장Service _draftStorage;
    private readonly 커뮤니티화물글초안가져오기Service _sourcePostImporter;
    private readonly 화주운송의뢰등록Service _registration;
    private readonly WebAuthSessionService _authSession;
    private readonly ICommunityPostClient _communityPosts;
    private readonly ICommunityProcurementClient _workflowMetadata;
    private readonly NavigationManager _navigation;
    private bool _initialized;
    private bool _autoSaveReady;
    private long? _loadedSourcePostId;
    private ShipperRequestNavigationContext _navigationContext = new();

    public ShipperRequestAuthoringPageViewModel(
        운송의뢰작성ViewModel state,
        운송의뢰자동저장Service draftStorage,
        커뮤니티화물글초안가져오기Service sourcePostImporter,
        화주운송의뢰등록Service registration,
        WebAuthSessionService authSession,
        ICommunityPostClient communityPosts,
        ICommunityProcurementClient workflowMetadata,
        NavigationManager navigation)
    {
        State = state;
        _draftStorage = draftStorage;
        _sourcePostImporter = sourcePostImporter;
        _registration = registration;
        _authSession = authSession;
        _communityPosts = communityPosts;
        _workflowMetadata = workflowMetadata;
        _navigation = navigation;
    }

    public 운송의뢰작성ViewModel State { get; }
    public IReadOnlyList<string> VehicleTypes => DefaultVehicleTypes;
    public 운송모델작성Draft? LastDraft { get; private set; }
    public string StatusMessage { get; private set; } = string.Empty;
    public Severity StatusSeverity { get; private set; } = Severity.Info;
    public string AutoSaveMessage { get; private set; } = string.Empty;
    public string? CreatedRequestId { get; private set; }
    public bool IsBusy { get; private set; }
    public PlatformCommunityPostResponse? SourcePost { get; private set; }
    public string? SourcePostError { get; private set; }
    public bool IsSourcePostLoading { get; private set; }
    public bool IsSourcePostApplying { get; private set; }
    public bool SourcePostApplied { get; private set; }
    public bool? RegistrationEnabled { get; private set; }
    public bool IsRegistrationAvailabilityLoading { get; private set; }
    public Guid? ApplicationPrivacyConsentEvidenceId { get; private set; }
    public string ApplicationSourceCode { get; private set; } = string.Empty;

    public string RegistrationBoundaryMessage { get; private set; }
        = "서버 등록 가능 여부를 확인하고 있습니다. 초안은 계속 작성할 수 있습니다.";

    public string SourcePostPreview => BuildSourcePostPreview(SourcePost?.Body);

    public async Task InitializeAsync(ShipperRequestNavigationContext context)
    {
        _navigationContext = context;
        if (!_initialized)
        {
            _initialized = true;
            await RestoreDraftAsync();
            await LoadTransportRegistrationAvailabilityAsync();
        }

        if (context.SourcePostId is > 0 && context.SourcePostId != _loadedSourcePostId)
        {
            await LoadSourcePostAsync(context.SourcePostId.Value);
        }
    }

    public async Task LoadTransportRegistrationAvailabilityAsync()
    {
        if (IsRegistrationAvailabilityLoading)
        {
            return;
        }

        IsRegistrationAvailabilityLoading = true;
        RegistrationBoundaryMessage = "서버 등록 가능 여부를 확인하고 있습니다. 초안은 계속 작성할 수 있습니다.";

        try
        {
            var metadata = await _workflowMetadata.GetVersionWorkflowMetadataAsync();
            RegistrationEnabled = metadata.Flags.TryGetValue(DomesticTransportFeatureKey, out var enabled)
                                  && enabled;
            RegistrationBoundaryMessage = RegistrationEnabled == true
                ? "서버에 운송 의뢰 원장을 등록할 수 있습니다. 추천·자동 배차·계약·결제는 이 등록만으로 확정되지 않습니다."
                : "현재 환경에서는 국내 화물 운송의 서버 등록이 비활성화되어 있습니다. 초안 작성·저장은 가능하며 배차나 계약은 시작되지 않습니다.";
        }
        catch
        {
            RegistrationEnabled = false;
            RegistrationBoundaryMessage = "서버 등록 가능 여부를 확인하지 못해 등록을 안전하게 보류했습니다. 초안은 계속 저장할 수 있습니다.";
        }
        finally
        {
            IsRegistrationAvailabilityLoading = false;
        }
    }

    public async Task ApplySourcePostAsync()
    {
        if (SourcePost is null || IsSourcePostApplying)
        {
            return;
        }

        IsSourcePostApplying = true;
        try
        {
            var result = _sourcePostImporter.가져오기(State, SourcePost);
            LastDraft = State.ToDraft();
            await _draftStorage.SaveAsync(LastDraft);
            CreatedRequestId = null;
            SourcePostApplied = true;
            StatusSeverity = result.변경됨 ? Severity.Success : Severity.Info;
            StatusMessage = result.변경됨
                ? "화물 글의 제목과 본문을 초안에 반영했습니다. 주소·연락처·수량·운임을 확인해 주세요."
                : "이 화물 글은 이미 현재 초안에 반영되어 있습니다.";
            AutoSaveMessage = $"가져온 초안을 저장했습니다. 저장 시각: {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            StatusSeverity = Severity.Error;
            StatusMessage = ex.Message;
        }
        finally
        {
            IsSourcePostApplying = false;
        }
    }

    public async Task SaveAsync()
    {
        LastDraft = State.ToDraft();
        await _draftStorage.SaveAsync(LastDraft);
        AutoSaveMessage = $"초안을 저장했습니다. 저장 시각: {DateTime.Now:HH:mm:ss}";
        StatusSeverity = State.서버등록가능 ? Severity.Success : Severity.Warning;
        StatusMessage = State.서버등록가능
            ? "서버 등록에 필요한 기본 입력이 채워졌습니다."
            : "초안을 저장했지만 서버 등록 전 보완할 입력이 남아 있습니다.";
    }

    public async Task AutoSaveAsync()
    {
        if (!_autoSaveReady || IsBusy)
        {
            return;
        }

        LastDraft = State.ToDraft();
        await _draftStorage.SaveAsync(LastDraft);
        AutoSaveMessage = $"자동 저장됨: {DateTime.Now:HH:mm:ss}";
    }

    public async Task ResetAsync()
    {
        State.Reset();
        LastDraft = null;
        CreatedRequestId = null;
        SourcePostApplied = false;
        await _draftStorage.ClearAsync();
        StatusSeverity = Severity.Info;
        StatusMessage = "운송 의뢰 입력값을 초기화했습니다.";
        AutoSaveMessage = string.Empty;
    }

    public Task SubmitAsync()
        => SubmitAsync(null);

    public async Task SubmitAsync(Func<string, Task>? afterCreated)
    {
        await SaveAsync();
        if (RegistrationEnabled != true)
        {
            StatusSeverity = Severity.Warning;
            StatusMessage = "현재 환경에서는 국내 화물 운송 서버 등록이 비활성화되어 있습니다. 초안은 브라우저에 저장했습니다.";
            return;
        }

        if (!State.서버등록가능)
        {
            StatusSeverity = Severity.Warning;
            StatusMessage = "서버 등록 전에 등록 전 점검 목록의 필수 항목을 보완해 주세요.";
            return;
        }

        IsBusy = true;
        StatusSeverity = Severity.Info;
        StatusMessage = "서버에 운송 의뢰를 등록하는 중입니다.";

        try
        {
            var created = await _registration.등록Async(
                State,
                ApplicationPrivacyConsentEvidenceId,
                ApplicationSourceCode);
            CreatedRequestId = created.의뢰Id;
            await _draftStorage.ClearAsync();
            LastDraft = null;
            StatusSeverity = Severity.Success;
            StatusMessage = $"서버 운송 의뢰를 등록했습니다. 의뢰 ID: {created.의뢰Id}";
            if (afterCreated is not null)
            {
                await afterCreated(created.의뢰Id);
            }
            var detailPath = ShipperRoutes.CreatedRequestDetailFor(created.의뢰Id);
            if (string.Equals(
                    _navigationContext.Source,
                    CommunityMapApplicationRoutes.SourceCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                detailPath += $"&source={Uri.EscapeDataString(CommunityMapApplicationRoutes.SourceCode)}";
            }
            _navigation.NavigateTo(detailPath);
        }
        catch (Exception ex)
        {
            StatusSeverity = Severity.Error;
            StatusMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void SetApplicationPrivacyConsent(신청개인정보동의증적Response evidence)
    {
        ApplicationPrivacyConsentEvidenceId = evidence.증적Id;
        ApplicationSourceCode = evidence.출처Code;
    }

    private async Task RestoreDraftAsync()
    {
        try
        {
            await _authSession.RestoreAsync();
            var restored = await _draftStorage.LoadAsync();
            if (restored is not null)
            {
                State.ApplyDraft(restored);
                LastDraft = restored;
                AutoSaveMessage = $"자동 저장 초안을 복구했습니다. 저장 시각: {restored.작성일시:MM-dd HH:mm}";
            }
        }
        catch (Exception ex)
        {
            StatusSeverity = Severity.Warning;
            StatusMessage = $"브라우저 초안을 복구하지 못했습니다: {ex.Message}";
        }
        finally
        {
            _autoSaveReady = true;
        }
    }

    private async Task LoadSourcePostAsync(long postId)
    {
        _loadedSourcePostId = postId;
        IsSourcePostLoading = true;
        SourcePost = null;
        SourcePostError = null;

        try
        {
            var post = await _communityPosts.GetPostAsync(postId);
            if (post is null)
            {
                SourcePostError = "가져올 커뮤니티 글을 찾을 수 없습니다.";
                return;
            }

            if (!_sourcePostImporter.가져올수있음(post))
            {
                SourcePostError = "화물 게시판 글만 운송 의뢰 초안으로 가져올 수 있습니다.";
                return;
            }

            SourcePost = post;
            SourcePostApplied = _sourcePostImporter.이미반영됨(State, post.Id);
        }
        catch (Exception ex)
        {
            SourcePostError = $"화물 글을 불러오지 못했습니다: {ex.Message}";
        }
        finally
        {
            IsSourcePostLoading = false;
        }
    }

    private static string BuildSourcePostPreview(string? body)
    {
        const int maximumLength = 240;
        var normalized = string.Join(
            " ",
            (body ?? string.Empty).Split(
                ['\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return normalized.Length <= maximumLength
            ? normalized
            : $"{normalized[..maximumLength].TrimEnd()}…";
    }
}
