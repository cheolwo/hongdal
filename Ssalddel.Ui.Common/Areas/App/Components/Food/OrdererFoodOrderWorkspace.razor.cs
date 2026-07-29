using Microsoft.AspNetCore.Components;
using Ssalddel.Ui.Common.Areas.App.Models.Auth;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Food;

public partial class OrdererFoodOrderWorkspace
{
    private bool _initialized;

    [Parameter]
    public string? OrderNo { get; set; }

    [Parameter]
    public EventCallback<string?> OrderSelected { get; set; }

    private 음식배달페이지접근ViewModel Access => ViewModel.접근;
    private 주문자앱인증ViewModel Authentication => ViewModel.인증;
    private 주문자음식주문목록ViewModel List => ViewModel.목록;
    private 주문자음식주문상세ViewModel Detail => ViewModel.상세;

    protected override async Task OnInitializedAsync()
    {
        await InitializeAsync();
        _initialized = true;
    }

    protected override Task OnParametersSetAsync()
        => !_initialized
            ? Task.CompletedTask
            : ViewModel.경로선택반영Async(OrderNo);

    private Task InitializeAsync() => ViewModel.초기화Async(OrderNo);

    private Task LoginAsync(공통로그인요청 request)
        => ViewModel.로그인Async(request, OrderNo);

    private async Task LogoutAsync()
    {
        if (await ViewModel.로그아웃Async() && OrderSelected.HasDelegate)
        {
            await OrderSelected.InvokeAsync(null);
        }
    }

    private async Task SelectOrderAsync(string orderNo)
    {
        await ViewModel.주문선택Async(orderNo);
        if (OrderSelected.HasDelegate)
        {
            await OrderSelected.InvokeAsync(orderNo);
        }
    }

    private Task RetryOrderAsync(string orderNo)
        => ViewModel.주문선택Async(orderNo);

    private Task RefreshOrderAsync(string orderNo)
        => ViewModel.주문진행새로고침Async();

    private Task ConfirmReceiptAsync()
        => ViewModel.주문수령확인Async();

    private async Task ClearSelectionAsync()
    {
        ViewModel.주문선택해제();
        if (OrderSelected.HasDelegate)
        {
            await OrderSelected.InvokeAsync(null);
        }
    }
}
