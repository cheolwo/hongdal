using Microsoft.AspNetCore.Components;
using Ssalddel.Contracts.Common.Sales;
using SsalddelApp.ViewModels.Shipper;

namespace SsalddelApp.Components.Pages;

public partial class OrderFulfillmentOrders
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private OrderFulfillmentReadViewModel Read => ViewModel.조회;

    protected override Task OnInitializedAsync()
    {
        var context = FulfillmentOrderNavigationContext.Parse(Navigation.Uri);
        Read.검색어 = context.Search ?? string.Empty;
        Read.국내외필터 = context.Scope ?? OrderFulfillmentFilterValues.All;
        Read.상태필터 = context.Status ?? OrderFulfillmentFilterValues.All;
        return ViewModel.새로고침Async();
    }

    private Task RefreshAsync()
        => ViewModel.새로고침Async();

    private string OrderDetailHref(OrderFulfillmentOrderSummary order)
        => new FulfillmentOrderNavigationContext()
            .WithListState(Read.검색어, Read.국내외필터, Read.상태필터)
            .DetailPath(order.채널종류, order.채널주문번호);
}
