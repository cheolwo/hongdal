using Microsoft.AspNetCore.Components;
using SsalddelApp.Services;
using SsalddelApp.ViewModels.Shipper;

namespace SsalddelApp.Components.Pages;

public partial class ProductListings
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private ProductListingReadViewModel Read => ViewModel.조회;
    private ProductListingDraftViewModel Draft => ViewModel.초안;
    private ProductListingCreateViewModel Create => ViewModel.생성;

    protected override async Task OnInitializedAsync()
        => await ViewModel.새로고침Async();

    private async Task RefreshAsync()
        => await ViewModel.새로고침Async();

    private async Task SelectProductAsync(long? productId)
        => await ViewModel.상품선택Async(productId);

    private async Task SelectAccountAsync(long? accountId)
        => await ViewModel.계정선택Async(accountId);

    private async Task CreateSimulationListingAsync()
        => await ViewModel.Simulation원장생성Async();

    private void OpenSalesPageComposer()
        => Navigation.NavigateTo(ShipperRoutes.SalesPageComposer);

    private void OpenSalesChannels()
        => Navigation.NavigateTo(ShipperRoutes.SalesChannels);
}
