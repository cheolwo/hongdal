using Microsoft.AspNetCore.Components;
using MudBlazor;
using Ssalddel.Contracts.Food;
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
    private 주문자음식주문인증ViewModel Authentication => ViewModel.인증;
    private 주문자음식주문목록ViewModel List => ViewModel.목록;
    private 주문자음식주문상세ViewModel Detail => ViewModel.상세;

    private Severity AuthenticationSeverity
        => Authentication.오류발생 ? Severity.Error : Severity.Info;

    protected override async Task OnInitializedAsync()
    {
        await InitializeAsync();
        _initialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!_initialized || !Access.사용가능 || !Authentication.로그인됨)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(OrderNo))
        {
            var orderNo = OrderNo.Trim();
            if (!string.Equals(Detail.요청OrderNo, orderNo, StringComparison.Ordinal))
            {
                await Detail.조회Async(orderNo);
            }
        }
        else if (!string.IsNullOrWhiteSpace(Detail.요청OrderNo))
        {
            Detail.선택해제();
        }
    }

    private async Task InitializeAsync()
    {
        if (!await Access.확인Async() || !Access.사용가능)
        {
            return;
        }

        if (!Authentication.초기화됨 && !await Authentication.복원Async())
        {
            return;
        }

        if (!Authentication.로그인됨)
        {
            return;
        }

        await LoadAuthenticatedContentAsync();
    }

    private async Task LoginAsync(공통로그인요청 request)
    {
        if (await Authentication.로그인Async(request.UserNameOrEmail, request.Password))
        {
            await LoadAuthenticatedContentAsync();
        }
    }

    private async Task LogoutAsync()
    {
        await Authentication.로그아웃Async();
        List.세션초기화();
        Detail.선택해제();
        if (OrderSelected.HasDelegate)
        {
            await OrderSelected.InvokeAsync(null);
        }
    }

    private async Task LoadAuthenticatedContentAsync()
    {
        var listTask = List.조회Async();
        var detailTask = string.IsNullOrWhiteSpace(OrderNo)
            ? Task.FromResult(true)
            : Detail.조회Async(OrderNo.Trim());
        await Task.WhenAll(listTask, detailTask);
    }

    private Task SearchAsync() => List.조회Async();

    private Task ReloadListAsync() => List.페이지조회Async(Math.Max(1, List.현재페이지));

    private Task ChangePageAsync(int page) => List.페이지조회Async(page);

    private async Task ResetFiltersAsync()
    {
        List.필터초기화();
        await List.조회Async();
    }

    private async Task SelectOrderAsync(string orderNo, bool updateAddress = true)
    {
        if (updateAddress && OrderSelected.HasDelegate)
        {
            await OrderSelected.InvokeAsync(orderNo);
        }

        await Detail.조회Async(orderNo);
    }

    private async Task ClearSelectionAsync()
    {
        Detail.선택해제();
        if (OrderSelected.HasDelegate)
        {
            await OrderSelected.InvokeAsync(null);
        }
    }

    private static string RestaurantLabel(주문자음식주문요약응답 order)
        => string.IsNullOrWhiteSpace(order.음식점명)
            ? $"음식점 #{order.음식점Id}"
            : order.음식점명.Trim();

    private static string DispatchLabel(string? value)
        => string.IsNullOrWhiteSpace(value) || value == 음식주문배차상태코드.미요청
            ? "배차 전"
            : $"배차 {value.Trim()}";

    private static Color StatusColor(string? status)
        => 음식주문상태코드.Normalize(status) switch
        {
            음식주문상태코드.전달완료 => Color.Success,
            음식주문상태코드.취소 => Color.Error,
            음식주문상태코드.조리중 or 음식주문상태코드.픽업대기 => Color.Info,
            음식주문상태코드.기사배정 or 음식주문상태코드.픽업완료 => Color.Primary,
            _ => Color.Warning
        };

    private static string Address(string? address, string? detailAddress)
        => string.Join(" ", new[] { address, detailAddress }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())) is { Length: > 0 } value
            ? value
            : "—";

    private static string FormatDate(DateTime value)
        => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime().ToString("yyyy.MM.dd HH:mm");

    private static string FormatOptionalDate(DateTime? value)
        => value.HasValue ? FormatDate(value.Value) : "—";

    private static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string ErrorMessage(string? value)
        => string.IsNullOrWhiteSpace(value) ? "서버 응답을 확인할 수 없습니다." : value;
}
