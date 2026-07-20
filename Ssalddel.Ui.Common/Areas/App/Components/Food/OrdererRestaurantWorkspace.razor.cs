using Microsoft.AspNetCore.Components;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Components.Food;

public partial class OrdererRestaurantWorkspace
{
    private bool _initialized;

    [Parameter]
    public long? RestaurantId { get; set; }

    [Parameter]
    public EventCallback<long?> RestaurantSelected { get; set; }

    private 음식배달페이지접근ViewModel Access => ViewModel.접근;
    private 음식점탐색기준ViewModel Criteria => ViewModel.기준;
    private 음식점공개목록ViewModel List => ViewModel.목록;
    private 음식점공개상세ViewModel Detail => ViewModel.상세;

    private string EmptyMessage => List.검색조건사용중
        ? "검색어나 주문 가능 조건을 바꿔 다시 조회해 주세요."
        : "운영자가 공개 프로필과 메뉴를 등록하면 이 권역에 표시됩니다.";

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
            }
        }
        else if (Detail.요청RestaurantId.HasValue)
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

        var criteriaTask = Criteria.초기화됨 ? Task.FromResult(true) : Criteria.준비Async();
        var detailTask = RestaurantId is long restaurantId
            ? Detail.조회Async(restaurantId)
            : Task.FromResult(true);
        await Task.WhenAll(criteriaTask, detailTask);
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
    }

    private async Task ClearSelectionAsync()
    {
        Detail.선택해제();
        if (RestaurantSelected.HasDelegate)
        {
            await RestaurantSelected.InvokeAsync(null);
        }
    }

    private static string DistanceLabel(decimal? distanceKm)
        => distanceKm.HasValue ? $"기준점 {distanceKm.Value:0.##}km" : "거리 기준 없음";

    private static string ValueOrDash(string? value)
        => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();

    private static string ErrorMessage(string? value)
        => string.IsNullOrWhiteSpace(value) ? "서버 응답을 확인할 수 없습니다." : value;
}
