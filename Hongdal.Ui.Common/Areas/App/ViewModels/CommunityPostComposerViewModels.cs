using System.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using Hongdal.Contracts.Common.Community;
using Hongdal.Ui.Common.Areas.App.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace Hongdal.Ui.Common.Areas.App.ViewModels;

public enum CommunityComposerMessageKind
{
    Info,
    Success,
    Warning,
    Error
}

public sealed record CommunityPostComposerSnapshot
{
    public DateTime SavedAtUtc { get; init; }
    public string Nickname { get; init; } = string.Empty;
    public bool IsAuthorDisplayCountryPublic { get; init; }
    public string AuthorDisplayCountryCode { get; init; } = string.Empty;
    public string AuthorDisplayCountryName { get; init; } = string.Empty;
    public string Category { get; init; } = "자유";
    public string WorkflowTag { get; init; } = "커뮤니티 신뢰";
    public string RoleTag { get; init; } = "플랫폼 구성원";
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public string SharedLinkUrl { get; init; } = string.Empty;
    public bool IsSalesPost { get; init; }
    public string SalesProductTitle { get; init; } = string.Empty;
    public decimal SalesAvailableQuantity { get; init; } = 1;
    public string SalesQuantityUnit { get; init; } = "개";
    public decimal SalesUnitPrice { get; init; }
    public string SalesCurrencyCode { get; init; } = "KRW";
    public bool AcceptsTossPayments { get; init; }
    public bool AcceptsNaverPay { get; init; }
    public bool AcceptsPayPal { get; init; }
    public bool AcceptsDirectCash { get; init; } = true;
    public bool AllowsGroupPurchase { get; init; } = true;
    public string SalesStatus { get; init; } = PlatformCommunitySalesOfferStatuses.Open;
    public string 커뮤니티원장Id { get; init; } = string.Empty;
    public bool IsReportBoardPost { get; init; }
    public string ReporterDisplayName { get; init; } = string.Empty;
    public string ReportedDisplayName { get; init; } = string.Empty;
}

public interface ICommunityPostComposerDraftStore
{
    Task<CommunityPostComposerSnapshot?> LoadAsync(
        string appKey,
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        string appKey,
        CommunityPostComposerSnapshot snapshot,
        CancellationToken cancellationToken = default);

    Task ClearAsync(
        string appKey,
        CancellationToken cancellationToken = default);
}

public sealed class CommunityPostComposerDraftViewModel : ObservableObject
{
    private string _nickname = string.Empty;
    private bool _isAuthorDisplayCountryPublic;
    private string _authorDisplayCountryCode = string.Empty;
    private string _authorDisplayCountryName = string.Empty;
    private string _password = string.Empty;
    private string _category = "자유";
    private string _workflowTag = "커뮤니티 신뢰";
    private string _roleTag = "플랫폼 구성원";
    private string _title = string.Empty;
    private string _body = string.Empty;
    private string _sharedLinkUrl = string.Empty;
    private bool _isSalesPost;
    private string _salesProductTitle = string.Empty;
    private decimal _salesAvailableQuantity = 1;
    private string _salesQuantityUnit = "개";
    private decimal _salesUnitPrice;
    private string _salesCurrencyCode = "KRW";
    private bool _acceptsTossPayments;
    private bool _acceptsNaverPay;
    private bool _acceptsPayPal;
    private bool _acceptsDirectCash = true;
    private bool _allowsGroupPurchase = true;
    private string _salesStatus = PlatformCommunitySalesOfferStatuses.Open;
    private string _커뮤니티원장Id = string.Empty;
    private bool _isReportBoardPost;
    private string _reporterDisplayName = string.Empty;
    private string _reportedDisplayName = string.Empty;

    public string Nickname { get => _nickname; set => SetProperty(ref _nickname, value); }
    public bool IsAuthorDisplayCountryPublic { get => _isAuthorDisplayCountryPublic; set => SetProperty(ref _isAuthorDisplayCountryPublic, value); }
    public string AuthorDisplayCountryCode { get => _authorDisplayCountryCode; set => SetProperty(ref _authorDisplayCountryCode, value); }
    public string AuthorDisplayCountryName { get => _authorDisplayCountryName; set => SetProperty(ref _authorDisplayCountryName, value); }
    public string Password { get => _password; set => SetProperty(ref _password, value); }
    public string Category { get => _category; set => SetProperty(ref _category, value); }
    public string WorkflowTag { get => _workflowTag; set => SetProperty(ref _workflowTag, value); }
    public string RoleTag { get => _roleTag; set => SetProperty(ref _roleTag, value); }
    public string Title { get => _title; set => SetProperty(ref _title, value); }
    public string Body { get => _body; set => SetProperty(ref _body, value); }
    public string SharedLinkUrl { get => _sharedLinkUrl; set => SetProperty(ref _sharedLinkUrl, value); }
    public bool IsSalesPost { get => _isSalesPost; set => SetProperty(ref _isSalesPost, value); }
    public string SalesProductTitle { get => _salesProductTitle; set => SetProperty(ref _salesProductTitle, value); }
    public decimal SalesAvailableQuantity { get => _salesAvailableQuantity; set => SetProperty(ref _salesAvailableQuantity, value); }
    public string SalesQuantityUnit { get => _salesQuantityUnit; set => SetProperty(ref _salesQuantityUnit, value); }
    public decimal SalesUnitPrice { get => _salesUnitPrice; set => SetProperty(ref _salesUnitPrice, value); }
    public string SalesCurrencyCode { get => _salesCurrencyCode; set => SetProperty(ref _salesCurrencyCode, value); }
    public bool AcceptsTossPayments { get => _acceptsTossPayments; set => SetProperty(ref _acceptsTossPayments, value); }
    public bool AcceptsNaverPay { get => _acceptsNaverPay; set => SetProperty(ref _acceptsNaverPay, value); }
    public bool AcceptsPayPal { get => _acceptsPayPal; set => SetProperty(ref _acceptsPayPal, value); }
    public bool AcceptsDirectCash { get => _acceptsDirectCash; set => SetProperty(ref _acceptsDirectCash, value); }
    public bool AllowsGroupPurchase { get => _allowsGroupPurchase; set => SetProperty(ref _allowsGroupPurchase, value); }
    public string SalesStatus { get => _salesStatus; set => SetProperty(ref _salesStatus, value); }
    public string 커뮤니티원장Id { get => _커뮤니티원장Id; set => SetProperty(ref _커뮤니티원장Id, value); }
    public bool IsReportBoardPost { get => _isReportBoardPost; set => SetProperty(ref _isReportBoardPost, value); }
    public string ReporterDisplayName { get => _reporterDisplayName; set => SetProperty(ref _reporterDisplayName, value); }
    public string ReportedDisplayName { get => _reportedDisplayName; set => SetProperty(ref _reportedDisplayName, value); }

    public bool HasContent
        => !string.IsNullOrWhiteSpace(Title)
           || !string.IsNullOrWhiteSpace(Body)
           || !string.IsNullOrWhiteSpace(SharedLinkUrl)
           || IsSalesPost
           || !string.IsNullOrWhiteSpace(커뮤니티원장Id);

    public string? Validate()
    {
        if (string.IsNullOrWhiteSpace(Nickname)
            || string.IsNullOrWhiteSpace(Password)
            || string.IsNullOrWhiteSpace(Category)
            || string.IsNullOrWhiteSpace(WorkflowTag)
            || string.IsNullOrWhiteSpace(RoleTag)
            || string.IsNullOrWhiteSpace(Title)
            || (string.IsNullOrWhiteSpace(Body) && string.IsNullOrWhiteSpace(SharedLinkUrl) && !IsSalesPost))
        {
            return "닉네임, 비밀번호, 게시판/분류, 워크플로우 태그, 역할 태그, 제목과 본문·링크·판매 정보 중 하나를 입력하세요.";
        }

        if (IsSalesPost)
        {
            if (string.IsNullOrWhiteSpace(SalesProductTitle))
            {
                return "판매할 상품명을 입력하세요.";
            }

            if (SalesAvailableQuantity <= 0 || string.IsNullOrWhiteSpace(SalesQuantityUnit))
            {
                return "판매 가능 수량과 단위를 확인하세요.";
            }

            if (SalesUnitPrice <= 0)
            {
                return "상품 가격을 입력하세요.";
            }

            if (!AcceptsTossPayments && !AcceptsNaverPay && !AcceptsPayPal && !AcceptsDirectCash)
            {
                return "협의 가능한 결제 방법을 하나 이상 선택하세요.";
            }

            if (IsReportBoardPost)
            {
                return "신고·분쟁 게시글에는 판매 정보를 함께 등록할 수 없습니다.";
            }
        }

        if (IsAuthorDisplayCountryPublic
            && (AuthorDisplayCountryCode.Trim().Length != 2
                || string.IsNullOrWhiteSpace(AuthorDisplayCountryName)))
        {
            return "활동 국가를 공개하려면 ISO 알파-2 국가 코드와 국가 이름을 입력하세요.";
        }

        return null;
    }

    public void Reset(string defaultRoleTag)
    {
        Nickname = string.Empty;
        IsAuthorDisplayCountryPublic = false;
        AuthorDisplayCountryCode = string.Empty;
        AuthorDisplayCountryName = string.Empty;
        Password = string.Empty;
        Category = "자유";
        WorkflowTag = "커뮤니티 신뢰";
        RoleTag = defaultRoleTag;
        Title = string.Empty;
        Body = string.Empty;
        SharedLinkUrl = string.Empty;
        IsSalesPost = false;
        SalesProductTitle = string.Empty;
        SalesAvailableQuantity = 1;
        SalesQuantityUnit = "개";
        SalesUnitPrice = 0;
        SalesCurrencyCode = "KRW";
        AcceptsTossPayments = false;
        AcceptsNaverPay = false;
        AcceptsPayPal = false;
        AcceptsDirectCash = true;
        AllowsGroupPurchase = true;
        SalesStatus = PlatformCommunitySalesOfferStatuses.Open;
        커뮤니티원장Id = string.Empty;
        IsReportBoardPost = false;
        ReporterDisplayName = string.Empty;
        ReportedDisplayName = string.Empty;
    }

    public void Apply(PlatformCommunityPostResponse post)
    {
        Nickname = post.Nickname;
        IsAuthorDisplayCountryPublic = post.IsAuthorDisplayCountryPublic;
        AuthorDisplayCountryCode = post.AuthorDisplayCountryCode ?? string.Empty;
        AuthorDisplayCountryName = post.AuthorDisplayCountryName ?? string.Empty;
        Password = string.Empty;
        Category = post.Category;
        WorkflowTag = post.WorkflowTag;
        RoleTag = post.RoleTag;
        Title = post.Title;
        Body = post.Body;
        SharedLinkUrl = post.SharedLinkUrl ?? string.Empty;
        Apply(post.SalesOffer);
        커뮤니티원장Id = post.커뮤니티원장Id ?? string.Empty;
        IsReportBoardPost = string.Equals(post.Category, "신고/분쟁", StringComparison.OrdinalIgnoreCase)
                            || post.IsReportBoardPost;
        ReporterDisplayName = post.ReporterDisplayName;
        ReportedDisplayName = post.ReportedDisplayName;
    }

    public void Apply(CommunityPostComposerSnapshot snapshot)
    {
        Nickname = snapshot.Nickname;
        IsAuthorDisplayCountryPublic = snapshot.IsAuthorDisplayCountryPublic;
        AuthorDisplayCountryCode = snapshot.AuthorDisplayCountryCode;
        AuthorDisplayCountryName = snapshot.AuthorDisplayCountryName;
        Password = string.Empty;
        Category = snapshot.Category;
        WorkflowTag = snapshot.WorkflowTag;
        RoleTag = snapshot.RoleTag;
        Title = snapshot.Title;
        Body = snapshot.Body;
        SharedLinkUrl = snapshot.SharedLinkUrl;
        IsSalesPost = snapshot.IsSalesPost;
        SalesProductTitle = snapshot.SalesProductTitle;
        SalesAvailableQuantity = snapshot.SalesAvailableQuantity;
        SalesQuantityUnit = snapshot.SalesQuantityUnit;
        SalesUnitPrice = snapshot.SalesUnitPrice;
        SalesCurrencyCode = snapshot.SalesCurrencyCode;
        AcceptsTossPayments = snapshot.AcceptsTossPayments;
        AcceptsNaverPay = snapshot.AcceptsNaverPay;
        AcceptsPayPal = snapshot.AcceptsPayPal;
        AcceptsDirectCash = snapshot.AcceptsDirectCash;
        AllowsGroupPurchase = snapshot.AllowsGroupPurchase;
        SalesStatus = snapshot.SalesStatus;
        커뮤니티원장Id = snapshot.커뮤니티원장Id;
        IsReportBoardPost = snapshot.IsReportBoardPost;
        ReporterDisplayName = snapshot.ReporterDisplayName;
        ReportedDisplayName = snapshot.ReportedDisplayName;
    }

    public CommunityPostComposerSnapshot CreateSnapshot(DateTime savedAtUtc)
        => new()
        {
            SavedAtUtc = savedAtUtc,
            Nickname = Nickname,
            IsAuthorDisplayCountryPublic = IsAuthorDisplayCountryPublic,
            AuthorDisplayCountryCode = AuthorDisplayCountryCode,
            AuthorDisplayCountryName = AuthorDisplayCountryName,
            Category = Category,
            WorkflowTag = WorkflowTag,
            RoleTag = RoleTag,
            Title = Title,
            Body = Body,
            SharedLinkUrl = SharedLinkUrl,
            IsSalesPost = IsSalesPost,
            SalesProductTitle = SalesProductTitle,
            SalesAvailableQuantity = SalesAvailableQuantity,
            SalesQuantityUnit = SalesQuantityUnit,
            SalesUnitPrice = SalesUnitPrice,
            SalesCurrencyCode = SalesCurrencyCode,
            AcceptsTossPayments = AcceptsTossPayments,
            AcceptsNaverPay = AcceptsNaverPay,
            AcceptsPayPal = AcceptsPayPal,
            AcceptsDirectCash = AcceptsDirectCash,
            AllowsGroupPurchase = AllowsGroupPurchase,
            SalesStatus = SalesStatus,
            커뮤니티원장Id = 커뮤니티원장Id,
            IsReportBoardPost = IsReportBoardPost,
            ReporterDisplayName = ReporterDisplayName,
            ReportedDisplayName = ReportedDisplayName
        };

    public PlatformCommunityPostCreateRequest CreateRequest(string appKey)
        => new()
        {
            AppKey = appKey,
            Category = Category,
            WorkflowTag = WorkflowTag,
            RoleTag = RoleTag,
            Title = Title,
            Body = Body,
            SharedLinkUrl = SharedLinkUrl,
            SalesOffer = CreateSalesOfferRequest(),
            커뮤니티원장Id = 커뮤니티원장Id,
            Nickname = Nickname,
            IsAuthorDisplayCountryPublic = IsAuthorDisplayCountryPublic,
            AuthorDisplayCountryCode = AuthorDisplayCountryCode,
            AuthorDisplayCountryName = AuthorDisplayCountryName,
            IsReportBoardPost = IsReportBoardPost,
            ReporterDisplayName = ReporterDisplayName,
            ReportedDisplayName = ReportedDisplayName,
            Password = Password
        };

    public PlatformCommunityPostUpdateRequest CreateUpdateRequest()
        => new()
        {
            Category = Category,
            WorkflowTag = WorkflowTag,
            RoleTag = RoleTag,
            Title = Title,
            Body = Body,
            SharedLinkUrl = SharedLinkUrl,
            SalesOffer = CreateSalesOfferRequest(),
            커뮤니티원장Id = 커뮤니티원장Id,
            Nickname = Nickname,
            IsAuthorDisplayCountryPublic = IsAuthorDisplayCountryPublic,
            AuthorDisplayCountryCode = AuthorDisplayCountryCode,
            AuthorDisplayCountryName = AuthorDisplayCountryName,
            IsReportBoardPost = IsReportBoardPost,
            ReporterDisplayName = ReporterDisplayName,
            ReportedDisplayName = ReportedDisplayName,
            Password = Password
        };

    private void Apply(PlatformCommunityPostSalesOfferResponse? salesOffer)
    {
        IsSalesPost = salesOffer is not null;
        SalesProductTitle = salesOffer?.ProductTitle ?? string.Empty;
        SalesAvailableQuantity = salesOffer?.AvailableQuantity ?? 1;
        SalesQuantityUnit = salesOffer?.QuantityUnit ?? "개";
        SalesUnitPrice = salesOffer?.UnitPrice ?? 0;
        SalesCurrencyCode = salesOffer?.CurrencyCode ?? "KRW";
        AcceptsTossPayments = AcceptsPaymentMethod(salesOffer, PlatformCommunitySalesPaymentMethodCodes.TossPayments);
        AcceptsNaverPay = AcceptsPaymentMethod(salesOffer, PlatformCommunitySalesPaymentMethodCodes.NaverPay);
        AcceptsPayPal = AcceptsPaymentMethod(salesOffer, PlatformCommunitySalesPaymentMethodCodes.PayPal);
        AcceptsDirectCash = salesOffer is null
            || AcceptsPaymentMethod(salesOffer, PlatformCommunitySalesPaymentMethodCodes.DirectCash);
        AllowsGroupPurchase = salesOffer?.AllowsGroupPurchase ?? true;
        SalesStatus = salesOffer?.Status ?? PlatformCommunitySalesOfferStatuses.Open;
    }

    private PlatformCommunityPostSalesOfferRequest? CreateSalesOfferRequest()
    {
        if (!IsSalesPost)
        {
            return null;
        }

        var paymentMethods = new List<string>();
        if (AcceptsTossPayments) paymentMethods.Add(PlatformCommunitySalesPaymentMethodCodes.TossPayments);
        if (AcceptsNaverPay) paymentMethods.Add(PlatformCommunitySalesPaymentMethodCodes.NaverPay);
        if (AcceptsPayPal) paymentMethods.Add(PlatformCommunitySalesPaymentMethodCodes.PayPal);
        if (AcceptsDirectCash) paymentMethods.Add(PlatformCommunitySalesPaymentMethodCodes.DirectCash);

        return new PlatformCommunityPostSalesOfferRequest
        {
            ProductTitle = SalesProductTitle,
            AvailableQuantity = SalesAvailableQuantity,
            QuantityUnit = SalesQuantityUnit,
            UnitPrice = SalesUnitPrice,
            CurrencyCode = SalesCurrencyCode,
            AcceptedPaymentMethods = paymentMethods,
            AllowsGroupPurchase = AllowsGroupPurchase,
            Status = SalesStatus
        };
    }

    private static bool AcceptsPaymentMethod(
        PlatformCommunityPostSalesOfferResponse? salesOffer,
        string paymentMethod)
        => salesOffer?.AcceptedPaymentMethods.Contains(paymentMethod, StringComparer.OrdinalIgnoreCase) == true;
}

public sealed record CommunityPostComposerSaveResult(
    bool Succeeded,
    bool WasEditing,
    PlatformCommunityPostResponse? Post,
    string Message);

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

    public void Configure(string appKey, string defaultRoleTag)
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

        IsSaving = true;
        var wasEditing = EditingPostId is not null;
        try
        {
            var saved = EditingPostId is long postId
                ? await _communityService.UpdatePostAsync(
                    postId,
                    Draft.CreateUpdateRequest(),
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
            var message = wasEditing ? "글을 수정했습니다." : "글을 등록했습니다.";
            SetStatus(message, CommunityComposerMessageKind.Success);
            return new(true, wasEditing, saved, message);
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

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_appKey))
        {
            throw new InvalidOperationException("커뮤니티 글쓰기 AppKey가 설정되지 않았습니다.");
        }
    }
}

public enum CommunityPostViewMode
{
    List,
    Cards
}

public sealed class CommunityPostListPageViewModel(
    PlatformCommunityService communityService) : PageViewModelBase
{
    private string _appKey = string.Empty;
    private readonly List<PlatformCommunityPostResponse> _items = [];
    private string _selectedBoard = "전체";
    private string _selectedListFilter = "전체글";
    private CommunityPostViewMode _viewMode;
    private string _searchText = string.Empty;
    private long? _selectedPostId;

    public IReadOnlyList<PlatformCommunityPostResponse> Items => _items;

    public IReadOnlyList<PlatformCommunityPostResponse> VisibleItems
        => _items
            .Where(post => string.Equals(SelectedBoard, "전체", StringComparison.OrdinalIgnoreCase)
                || string.Equals(post.Category, SelectedBoard, StringComparison.OrdinalIgnoreCase))
            .Where(MatchesListFilter)
            .Where(MatchesSearch)
            .OrderByDescending(post => post.IsOperatorPinned)
            .ThenByDescending(post => post.IsTrending)
            .ThenByDescending(post => post.LastEngagedAtUtc ?? post.CreatedAtUtc)
            .ToArray();

    public string SelectedBoard
    {
        get => _selectedBoard;
        set
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? "전체" : value.Trim();
            if (SetProperty(ref _selectedBoard, normalized))
            {
                SelectedPostId = null;
                OnPropertyChanged(nameof(VisibleItems));
            }
        }
    }

    public string SelectedListFilter
    {
        get => _selectedListFilter;
        set
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? "전체글" : value.Trim();
            if (SetProperty(ref _selectedListFilter, normalized))
            {
                SelectedPostId = null;
                OnPropertyChanged(nameof(VisibleItems));
            }
        }
    }

    public CommunityPostViewMode ViewMode
    {
        get => _viewMode;
        set => SetProperty(ref _viewMode, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            var normalized = value ?? string.Empty;
            if (SetProperty(ref _searchText, normalized))
            {
                SelectedPostId = null;
                OnPropertyChanged(nameof(VisibleItems));
            }
        }
    }

    public long? SelectedPostId
    {
        get => _selectedPostId;
        set => SetProperty(ref _selectedPostId, value);
    }

    public void Configure(string appKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appKey);
        _appKey = appKey.Trim();
    }

    public void Replace(PlatformCommunityPostResponse post)
    {
        var index = _items.FindIndex(item => item.Id == post.Id);
        if (index < 0)
        {
            return;
        }

        _items[index] = post;
        OnPropertyChanged(nameof(Items));
        OnPropertyChanged(nameof(VisibleItems));
    }

    public async Task<PlatformCommunityPostResponse?> RefreshItemAsync(
        long postId,
        CancellationToken cancellationToken = default)
    {
        var detail = await communityService.GetPostAsync(postId, cancellationToken);
        if (detail is not null)
        {
            Replace(detail);
        }

        return detail;
    }

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_appKey))
        {
            throw new InvalidOperationException("커뮤니티 게시글 목록 AppKey가 설정되지 않았습니다.");
        }

        var result = await communityService.GetPostsAsync(_appKey, cancellationToken);
        _items.Clear();
        _items.AddRange(result.Items);
        OnPropertyChanged(nameof(Items));
        OnPropertyChanged(nameof(VisibleItems));
    }

    private bool MatchesListFilter(PlatformCommunityPostResponse post)
        => SelectedListFilter switch
        {
            "공지" => post.IsOperatorPinned,
            "추천글" => post.IsTrending || post.RecommendationCount >= 5,
            _ => true
        };

    private bool MatchesSearch(PlatformCommunityPostResponse post)
    {
        var searchText = SearchText.Trim();
        return searchText.Length == 0
               || ContainsSearchText(post.Title, searchText)
               || ContainsSearchText(post.Body, searchText)
               || ContainsSearchText(post.Nickname, searchText)
               || ContainsSearchText(post.AuthorDisplayCountryCode, searchText)
               || ContainsSearchText(post.AuthorDisplayCountryName, searchText)
               || ContainsSearchText(post.Category, searchText)
               || ContainsSearchText(post.WorkflowTag, searchText)
               || ContainsSearchText(post.RoleTag, searchText);
    }

    private static bool ContainsSearchText(string? value, string searchText)
        => !string.IsNullOrWhiteSpace(value)
           && value.Contains(searchText, StringComparison.OrdinalIgnoreCase);
}

public sealed class PlatformCommunityHomePageViewModel : PageViewModelBase
{
    public PlatformCommunityHomePageViewModel(
        CommunityPostComposerViewModel composer,
        CommunityPostListPageViewModel postList,
        PlatformCommunityHomeShellViewModel shell,
        PlatformCommunityBoardWorkspaceViewModel boards,
        PlatformCommunityPostEngagementViewModel engagement,
        PlatformCommunityLedgerPickerViewModel ledgerPicker,
        YouTubeFoodCommunityDiscoveryViewModel foodDiscovery)
    {
        Composer = 하위ViewModel등록(composer, 수명소유: true);
        PostList = 하위ViewModel등록(postList, 수명소유: true);
        Shell = 하위ViewModel등록(shell, 수명소유: true);
        Boards = 하위ViewModel등록(boards, 수명소유: true);
        Engagement = 하위ViewModel등록(engagement, 수명소유: true);
        LedgerPicker = 하위ViewModel등록(ledgerPicker, 수명소유: true);
        FoodDiscovery = 하위ViewModel등록(foodDiscovery, 수명소유: true);
    }

    public CommunityPostComposerViewModel Composer { get; }
    public CommunityPostListPageViewModel PostList { get; }
    public PlatformCommunityHomeShellViewModel Shell { get; }
    public PlatformCommunityBoardWorkspaceViewModel Boards { get; }
    public PlatformCommunityPostEngagementViewModel Engagement { get; }
    public PlatformCommunityLedgerPickerViewModel LedgerPicker { get; }
    public YouTubeFoodCommunityDiscoveryViewModel FoodDiscovery { get; }

    public void Configure(string appKey, string defaultRoleTag)
    {
        Composer.Configure(appKey, defaultRoleTag);
        PostList.Configure(appKey);
    }

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        var postsLoaded = 새로고침
            ? await PostList.새로고침Async(cancellationToken)
            : await PostList.초기화Async(cancellationToken);
        if (!postsLoaded)
        {
            throw new InvalidOperationException(
                PostList.오류메시지 ?? "커뮤니티 게시글 목록을 불러오지 못했습니다.");
        }
    }
}
