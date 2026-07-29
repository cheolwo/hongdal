using Microsoft.AspNetCore.Components;
using Ssalddel.Ui.Common.Areas.App.Models.Auth;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Food;

public partial class OrdererRestaurantWorkspace
{
    private bool _initialized;

    [Parameter]
    public long? RestaurantId { get; set; }

    [Parameter]
    public EventCallback<long?> RestaurantSelected { get; set; }

    [Parameter]
    public EventCallback<string> OrderSubmitted { get; set; }

    private 음식배달페이지접근ViewModel Access => ViewModel.접근;
    private 주문자앱인증ViewModel Authentication => ViewModel.인증;
    private 음식점탐색기준ViewModel Criteria => ViewModel.기준;
    private 음식점공개목록ViewModel List => ViewModel.목록;
    private 음식점공개상세ViewModel Detail => ViewModel.상세;
    private 음식주문작성ViewModel Writer => ViewModel.작성;

    protected override async Task OnInitializedAsync()
    {
        await InitializeAsync();
        _initialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!_initialized || !Access.사용가능)
        {
            return;
        }

        if (RestaurantId is long restaurantId)
        {
            if (Detail.요청RestaurantId != restaurantId)
            {
                await Detail.조회Async(restaurantId);
                Writer.음식점설정(Detail.상세);
            }
        }
        else if (Detail.요청RestaurantId.HasValue)
        {
            Detail.선택해제();
            Writer.음식점설정(null);
        }
    }

    private async Task InitializeAsync()
    {
        if (!await Access.확인Async() || !Access.사용가능)
        {
            return;
        }

        var authenticationTask = Authentication.초기화됨
            ? Task.FromResult(true)
            : Authentication.복원Async();
        var criteriaTask = Criteria.초기화됨 ? Task.FromResult(true) : Criteria.준비Async();
        var detailTask = RestaurantId is long restaurantId
            ? Detail.조회Async(restaurantId)
            : Task.FromResult(true);
        await Task.WhenAll(authenticationTask, criteriaTask, detailTask);
        Writer.음식점설정(Detail.상세);
    }

    private Task PrepareCriteriaAsync() => Criteria.준비Async();

    private Task SearchAsync()
        => List.조회Async(Criteria.선택배달권키, Criteria.반경Km);

    private Task ReloadListAsync() => List.새로고침Async();

    private Task ChangePageAsync(int page) => List.페이지조회Async(page);

    private void SetRadius(double radiusKm) => Criteria.빠른반경설정(radiusKm);

    private async Task SelectRestaurantAsync(long restaurantId, bool updateAddress = true)
    {
        if (updateAddress && RestaurantSelected.HasDelegate)
        {
            await RestaurantSelected.InvokeAsync(restaurantId);
        }

        await Detail.조회Async(restaurantId);
        Writer.음식점설정(Detail.상세);
    }

    private Task SelectRestaurantFromListAsync(long restaurantId)
        => SelectRestaurantAsync(restaurantId);

    private Task RetryRestaurantAsync(long restaurantId)
        => SelectRestaurantAsync(restaurantId, updateAddress: false);

    private Task LoginAsync(공통로그인요청 request)
        => Authentication.로그인Async(
            request.UserNameOrEmail,
            request.Password);

    private async Task SubmitOrderAsync()
    {
        if (!Authentication.로그인됨 || !await Writer.등록Async())
        {
            return;
        }

        var orderNo = Writer.등록응답?.주문번호;
        if (!string.IsNullOrWhiteSpace(orderNo) && OrderSubmitted.HasDelegate)
        {
            await OrderSubmitted.InvokeAsync(orderNo);
        }
    }

    private async Task ClearSelectionAsync()
    {
        Detail.선택해제();
        Writer.음식점설정(null);
        if (RestaurantSelected.HasDelegate)
        {
            await RestaurantSelected.InvokeAsync(null);
        }
    }

}
