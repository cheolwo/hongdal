using Microsoft.JSInterop;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Ssalddel.WebApp.Services;

namespace Ssalddel.WebApp.ViewModels;

public enum CommunityPersonalMessageTone
{
    Info,
    Success,
    Warning
}

public sealed class CommunityPersonalActivityViewModel(
    WebAuthSessionService authSession,
    PlatformCommunityService communityService,
    Action<string, CommunityPersonalMessageTone> publishFeedback) : 조립ViewModelBase
{
    private readonly List<PlatformCommunityPostResponse> _posts = [];
    private readonly List<PlatformCommunityPostLedgerChoiceResponse> _ledgers = [];
    private bool _isLoading;

    public IReadOnlyList<PlatformCommunityPostResponse> Posts => _posts;
    public IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse> Ledgers => _ledgers;
    public bool IsLoggedIn => authSession.IsLoggedIn;
    public string IdentityLabel => authSession.UserName ?? "방문자";
    public string RoleLabel => authSession.CurrentTheme.RoleLabel;
    public string IdentityDescription => authSession.IsLoggedIn
        ? $"{RoleLabel} 역할로 연결된 개인 활동입니다."
        : "이 브라우저에서 작성과 꾸미기 설정을 이어서 볼 수 있습니다.";

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public void NotifyIdentityChanged()
    {
        OnPropertyChanged(nameof(IsLoggedIn));
        OnPropertyChanged(nameof(IdentityLabel));
        OnPropertyChanged(nameof(RoleLabel));
        OnPropertyChanged(nameof(IdentityDescription));
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        IsLoading = true;
        _posts.Clear();
        _ledgers.Clear();
        NotifyCollectionsChanged();

        try
        {
            if (authSession.IsLoggedIn && !string.IsNullOrWhiteSpace(authSession.UserName))
            {
                var response = await communityService.GetPostsAsync("shipper", cancellationToken);
                _posts.AddRange(response.Items.Where(post =>
                    post.Nickname.Equals(authSession.UserName, StringComparison.OrdinalIgnoreCase)));
                _ledgers.AddRange(await communityService.GetMyLedgersAsync(
                    cancellationToken: cancellationToken));
                NotifyCollectionsChanged();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            publishFeedback(
                "계정 활동을 아직 불러오지 못했습니다. 개인 설정과 꾸미기는 계속 사용할 수 있습니다.",
                CommunityPersonalMessageTone.Info);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void NotifyCollectionsChanged()
    {
        OnPropertyChanged(nameof(Posts));
        OnPropertyChanged(nameof(Ledgers));
    }
}

public sealed class CommunityPersonalPreferencesViewModel(
    WebAuthSessionService authSession,
    CommunityPersonalPreferenceService preferenceService,
    Action<string, CommunityPersonalMessageTone> publishFeedback) : 조립ViewModelBase
{
    public CommunityPersonalPreferences Current => preferenceService.Current;

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        await preferenceService.LoadAsync(authSession.UserId, cancellationToken);
        OnPropertyChanged(nameof(Current));
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await preferenceService.SaveAsync(authSession.UserId, cancellationToken);
            publishFeedback(
                "내 사용 설정을 이 브라우저에 저장했습니다.",
                CommunityPersonalMessageTone.Success);
        }
        catch (JSException)
        {
            publishFeedback(
                "브라우저 저장소를 사용할 수 없어 설정을 저장하지 못했습니다.",
                CommunityPersonalMessageTone.Warning);
        }
    }

    public async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await preferenceService.ResetAsync(authSession.UserId, cancellationToken);
            OnPropertyChanged(nameof(Current));
            publishFeedback(
                "개인 설정을 기본값으로 되돌렸습니다.",
                CommunityPersonalMessageTone.Info);
        }
        catch (JSException)
        {
            publishFeedback(
                "브라우저 저장소를 사용할 수 없어 설정을 초기화하지 못했습니다.",
                CommunityPersonalMessageTone.Warning);
        }
    }
}

public sealed class CommunityPersonalDecorationsViewModel(
    PlatformCommunityDecorationStateService decorationState,
    ICommunityDecorationSelectionStore selectionStore,
    Action<string, CommunityPersonalMessageTone> publishFeedback) : 조립ViewModelBase
{
    public int OwnedCount => decorationState.Products.Count(decorationState.IsProductOwned);
    public string ActiveThemeTitle => decorationState.Products.FirstOrDefault(product =>
                                          product.IsHomeTheme
                                          && decorationState.IsProductActive(product))?.Title
                                      ?? "기본 홈";
    public bool IsHomeThemeEnabled => decorationState.IsHomeThemeEnabled;
    public bool IsTraditionalMarketThemeEnabled => decorationState.IsTraditionalMarketThemeEnabled;
    public bool IsBaguaDecorationEnabled => decorationState.IsBaguaDecorationEnabled;
    public bool IsNodeDecorationEnabled => decorationState.IsNodeDecorationEnabled;
    public bool IsBaguaMotionEnabled => decorationState.IsBaguaMotionEnabled;

    public IReadOnlyList<CommunityDecorationProduct> ProductsFor(string? productKey)
        => string.IsNullOrWhiteSpace(productKey)
            ? decorationState.Products
            : decorationState.Products
                .Where(product => product.Key.Equals(productKey, StringComparison.OrdinalIgnoreCase)
                                  || product.PackKey.Equals(productKey, StringComparison.OrdinalIgnoreCase))
                .ToArray();

    public bool IsProductActive(CommunityDecorationProduct product)
        => decorationState.IsProductActive(product);

    public bool IsProductOwned(CommunityDecorationProduct product)
        => decorationState.IsProductOwned(product);

    public async Task RestoreAsync(CancellationToken cancellationToken = default)
    {
        var selection = await selectionStore.LoadAsync(cancellationToken);
        if (selection is null)
        {
            return;
        }

        decorationState.ApplyHomeThemePack(selection.ActiveHomeThemePackKey);
        decorationState.SetTargetEnabled(
            CommunityDecorationTarget.HomeNavigatorTheme,
            selection.IsHomeThemeEnabled);
        foreach (var (marketScopeKey, packKey) in selection.ActiveTraditionalMarketThemePackByScope
                     ?? new Dictionary<string, string>())
        {
            decorationState.ApplyTraditionalMarketThemePack(marketScopeKey, packKey);
        }

        decorationState.SetTargetEnabled(
            CommunityDecorationTarget.TraditionalMarketTheme,
            selection.IsTraditionalMarketThemeEnabled);
        NotifyStateChanged();
    }

    public async Task SetTargetAsync(
        CommunityDecorationTarget target,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        decorationState.SetTargetEnabled(target, enabled);
        NotifyStateChanged();
        if (target is CommunityDecorationTarget.HomeNavigatorTheme
            or CommunityDecorationTarget.TraditionalMarketTheme)
        {
            await SaveSelectionAsync(cancellationToken);
        }
    }

    public async Task ApplyAsync(
        CommunityDecorationProduct product,
        CancellationToken cancellationToken = default)
    {
        if (!decorationState.ApplyProduct(product))
        {
            publishFeedback(
                "먼저 꾸미기를 보유해야 사용할 수 있습니다.",
                CommunityPersonalMessageTone.Warning);
            return;
        }

        NotifyStateChanged();
        if (!await SaveSelectionAsync(cancellationToken))
        {
            return;
        }

        publishFeedback(
            $"{product.Title} 꾸미기를 적용했습니다.",
            CommunityPersonalMessageTone.Success);
    }

    public void NotifyStateChanged()
    {
        OnPropertyChanged(nameof(OwnedCount));
        OnPropertyChanged(nameof(ActiveThemeTitle));
        OnPropertyChanged(nameof(IsHomeThemeEnabled));
        OnPropertyChanged(nameof(IsTraditionalMarketThemeEnabled));
        OnPropertyChanged(nameof(IsBaguaDecorationEnabled));
        OnPropertyChanged(nameof(IsNodeDecorationEnabled));
        OnPropertyChanged(nameof(IsBaguaMotionEnabled));
    }

    private async Task<bool> SaveSelectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            await selectionStore.SaveAsync(new(
                decorationState.ActiveHomeThemePackKey,
                decorationState.IsHomeThemeEnabled,
                new Dictionary<string, string>(decorationState.ActiveTraditionalMarketThemePackByScope),
                decorationState.IsTraditionalMarketThemeEnabled), cancellationToken);
            return true;
        }
        catch (JSException)
        {
            publishFeedback(
                "브라우저 저장소를 사용할 수 없어 꾸미기 선택을 이번 화면에서만 적용했습니다.",
                CommunityPersonalMessageTone.Warning);
            return false;
        }
    }
}

public sealed class CommunityPersonalPageViewModel : 조립ViewModelBase
{
    private readonly WebAuthSessionService _authSession;
    private readonly PlatformCommunityDecorationStateService _decorationState;
    private readonly CancellationTokenSource _lifetimeCancellation = new();
    private bool _initialized;
    private string? _statusMessage;
    private CommunityPersonalMessageTone _statusTone = CommunityPersonalMessageTone.Info;

    public CommunityPersonalPageViewModel(
        WebAuthSessionService authSession,
        PlatformCommunityService communityService,
        CommunityPersonalPreferenceService preferenceService,
        PlatformCommunityDecorationStateService decorationState,
        ICommunityDecorationSelectionStore selectionStore)
    {
        _authSession = authSession;
        _decorationState = decorationState;
        Activity = 하위ViewModel등록(new CommunityPersonalActivityViewModel(
            authSession,
            communityService,
            PublishFeedback));
        Preferences = 하위ViewModel등록(new CommunityPersonalPreferencesViewModel(
            authSession,
            preferenceService,
            PublishFeedback));
        Decorations = 하위ViewModel등록(new CommunityPersonalDecorationsViewModel(
            decorationState,
            selectionStore,
            PublishFeedback));
        _authSession.Changed += HandleAuthenticationChanged;
        decorationState.Changed += HandleDecorationChanged;
    }

    public CommunityPersonalActivityViewModel Activity { get; }
    public CommunityPersonalPreferencesViewModel Preferences { get; }
    public CommunityPersonalDecorationsViewModel Decorations { get; }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public CommunityPersonalMessageTone StatusTone
    {
        get => _statusTone;
        private set => SetProperty(ref _statusTone, value);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _lifetimeCancellation.Token);
        await Preferences.LoadAsync(linkedCancellation.Token);
        await Decorations.RestoreAsync(linkedCancellation.Token);
        await Activity.LoadAsync(linkedCancellation.Token);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _authSession.Changed -= HandleAuthenticationChanged;
            _decorationState.Changed -= HandleDecorationChanged;
            _lifetimeCancellation.Cancel();
            _lifetimeCancellation.Dispose();
        }

        base.Dispose(disposing);
    }

    private void HandleAuthenticationChanged()
    {
        Activity.NotifyIdentityChanged();
        _ = RefreshAuthenticationStateAsync();
    }

    private async Task RefreshAuthenticationStateAsync()
    {
        try
        {
            await Preferences.LoadAsync(_lifetimeCancellation.Token);
            await Activity.LoadAsync(_lifetimeCancellation.Token);
        }
        catch (OperationCanceledException) when (_lifetimeCancellation.IsCancellationRequested)
        {
        }
    }

    private void HandleDecorationChanged()
        => Decorations.NotifyStateChanged();

    private void PublishFeedback(string message, CommunityPersonalMessageTone tone)
    {
        StatusTone = tone;
        StatusMessage = message;
    }
}
