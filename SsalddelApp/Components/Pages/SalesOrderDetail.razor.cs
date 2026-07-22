using Microsoft.AspNetCore.Components;
using Ssalddel.Contracts.Common.Sales;

namespace SsalddelApp.Components.Pages;

public partial class SalesOrderDetail
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    [Parameter]
    public long OrderId { get; set; }

    private string ReturnPath
        => SalesOrderNavigationContext.Parse(Navigation.Uri)
            .ResolveReturnPath();
}
