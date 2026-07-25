using Microsoft.AspNetCore.Components;
using MudBlazor;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.App.Models.Auth;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace OrdererApp.ViewModels;

/// <summary>02.02 재료 후보 탐색 route의 상태와 사용자 행동을 조립합니다.</summary>
public sealed class 주문자재료후보PageViewModel : 주문자PageViewModelBase
{
    private readonly NavigationManager _navigation;

    public 주문자재료후보PageViewModel(
        GroupPurchaseCatalogViewModel catalog,
        OrdererIngredientCardAutoGroupingViewModel autoGrouping,
        주문자앱인증ViewModel authentication,
        NavigationManager navigation)
    {
        Catalog = catalog;
        AutoGrouping = autoGrouping;
        Authentication = 하위ViewModel등록(authentication);
        _navigation = navigation;
    }

    public GroupPurchaseCatalogViewModel Catalog { get; }
    public OrdererIngredientCardAutoGroupingViewModel AutoGrouping { get; }
    public 주문자앱인증ViewModel Authentication { get; }
    public Severity AuthenticationSeverity
        => Authentication.오류발생 ? Severity.Error : Severity.Info;

    protected override bool 하위ViewModel처리중 => Authentication.처리중;

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        if (!await Catalog.LoadAsync(새로고침, cancellationToken))
        {
            throw new InvalidOperationException(
                Catalog.ErrorMessage ?? "공동구매 상품 후보를 불러오지 못했습니다.");
        }

        if (!Authentication.초기화됨)
        {
            await Authentication.복원Async(cancellationToken);
        }
    }

    public async Task LoginAsync(공통로그인요청 request)
    {
        if (await Authentication.로그인Async(
                request.UserNameOrEmail,
                request.Password))
        {
            AutoGrouping.ClearUserState();
            OnPropertyChanged(string.Empty);
        }
    }

    public Task OpenProductAsync(string productId)
    {
        _navigation.NavigateTo(GroupPurchasePageRoutes.ProductDetailFor(productId));
        return Task.CompletedTask;
    }

    public async Task JoinGroupAsync(HS먹거리공동구매상품카드 product)
    {
        await AutoGrouping.JoinAsync(product);
        OnPropertyChanged(string.Empty);
    }

    public async Task WithdrawGroupAsync(HS먹거리공동구매상품카드 product)
    {
        await AutoGrouping.WithdrawAsync(product);
        OnPropertyChanged(string.Empty);
    }
}

/// <summary>02.03 여러 재료 의향 등록 route의 초기화와 로그인 경계를 조립합니다.</summary>
public sealed class 주문자의향등록PageViewModel : 주문자PageViewModelBase
{
    public 주문자의향등록PageViewModel(
        GroupPurchaseCatalogViewModel catalog,
        GroupPurchaseWishBatchViewModel wishBatch,
        주문자앱인증ViewModel authentication)
    {
        Catalog = catalog;
        WishBatch = wishBatch;
        Authentication = 하위ViewModel등록(authentication);
    }

    public GroupPurchaseCatalogViewModel Catalog { get; }
    public GroupPurchaseWishBatchViewModel WishBatch { get; }
    public 주문자앱인증ViewModel Authentication { get; }

    protected override bool 하위ViewModel처리중
        => Authentication.처리중 || WishBatch.IsBusy;

    protected override async Task 불러오기Async(
        bool 새로고침,
        CancellationToken cancellationToken)
    {
        if (!await Catalog.LoadAsync(새로고침, cancellationToken))
        {
            throw new InvalidOperationException(
                Catalog.ErrorMessage ?? "공동구매 상품 후보를 불러오지 못했습니다.");
        }

        if (!Authentication.초기화됨)
        {
            await Authentication.복원Async(cancellationToken);
        }

        WishBatch.Initialize(Catalog.ProductCards);
        WishBatch.PrepareForCurrentUser();
        OnPropertyChanged(string.Empty);
    }

    public async Task LoginAsync(공통로그인요청 request)
    {
        if (await Authentication.로그인Async(
                request.UserNameOrEmail,
                request.Password))
        {
            WishBatch.PrepareForCurrentUser();
            OnPropertyChanged(string.Empty);
        }
    }
}
