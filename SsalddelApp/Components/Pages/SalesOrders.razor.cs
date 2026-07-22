using Microsoft.AspNetCore.Components;
using Ssalddel.Contracts.Common.Sales;

namespace SsalddelApp.Components.Pages;

public partial class SalesOrders
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    [SupplyParameterFromQuery(Name = "orderId")]
    public long? LegacyOrderId { get; set; }

    private SalesOrderNavigationContext NavigationContext
        => SalesOrderNavigationContext.Parse(Navigation.Uri);

    protected override void OnParametersSet()
    {
        if (LegacyOrderId is not > 0)
        {
            return;
        }

        var current = NavigationContext;
        var listPath = new SalesOrderNavigationContext()
            .WithListState(current.Search, current.SyncScope, current.Status, current.Page)
            .PathFor(SalesOrderScreenKind.List);
        var detailPath = new SalesOrderNavigationContext { From = listPath }
            .PathFor(SalesOrderScreenKind.Detail, LegacyOrderId.Value);

        Navigation.NavigateTo(detailPath, replace: true);
    }
}
