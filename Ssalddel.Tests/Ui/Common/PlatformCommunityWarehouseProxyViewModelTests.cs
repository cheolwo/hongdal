using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Ssalddel.Tests.Ui.Common;

public sealed class PlatformCommunityWarehouseProxyViewModelTests
{
    [Fact]
    public async Task OpenAsync_PreparesFallbackCandidateAndDraftWithoutWarehouseService()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var viewModel = new PlatformCommunityWarehouseProxyViewModel(services);

        await viewModel.OpenAsync(new(
            "공동 주문 입고",
            "창고",
            "모인 상품을 입고합니다."));

        Assert.NotNull(viewModel.SourceNode);
        Assert.NotEmpty(viewModel.Candidates);
        Assert.NotNull(viewModel.SelectedCandidate);
        Assert.Equal("다이어그램 물류 대행 신청", viewModel.Draft.SupplierName);
        Assert.Equal(입고계약유형코드.보관대행, viewModel.Draft.ContractType);
        Assert.Contains("공동 주문 입고", viewModel.Draft.Notes);
        Assert.False(viewModel.CanSubmit);
    }

    [Fact]
    public async Task BuildWorkspaceUrl_UsesSelectedCandidateAndSourceNode()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var viewModel = new PlatformCommunityWarehouseProxyViewModel(services);
        await viewModel.OpenAsync(new(
            "입고 및 검수",
            "창고 역할",
            "도착 상품을 확인합니다."));

        var url = viewModel.BuildWorkspaceUrl();

        Assert.NotNull(url);
        Assert.StartsWith("/shipper/inbound/requests?", url, StringComparison.Ordinal);
        Assert.Contains("source=diagram-warehouse-proxy", url, StringComparison.Ordinal);
        Assert.Contains("nodeTitle=%EC%9E%85%EA%B3%A0%20%EB%B0%8F%20%EA%B2%80%EC%88%98", url, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SelectCandidate_UrbanLogisticsCenter_UsesSharedWarehouseFlow()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var viewModel = new PlatformCommunityWarehouseProxyViewModel(services);
        await viewModel.OpenAsync(new(
            "공동주문 생활물류",
            "창고",
            "공동 물량을 검수하고 주민 수령 단위로 분류합니다."));

        viewModel.SelectCandidate("default:urban-logistics-center");

        Assert.Equal(
            Ssalddel.Contracts.Common.Warehouse.LogisticsProxySiteTypes.UrbanLogisticsCenter,
            viewModel.SelectedCandidate?.ProxyTypeCode);
        Assert.Equal("도심 생활물류센터", viewModel.SelectedCandidate?.ProxyTypeLabel);
        Assert.Equal(입고계약유형코드.보관대행, viewModel.Draft.ContractType);
        Assert.Contains("공동주문 생활물류", viewModel.Draft.Notes);
    }

    [Fact]
    public async Task Close_ClearsPanelStateAndDraft()
    {
        using var services = new ServiceCollection().BuildServiceProvider();
        var viewModel = new PlatformCommunityWarehouseProxyViewModel(services);
        await viewModel.OpenAsync(new("입고", "창고", "상품 입고"));

        viewModel.Close();

        Assert.Null(viewModel.SourceNode);
        Assert.Null(viewModel.SelectedCandidate);
        Assert.Empty(viewModel.Candidates);
        Assert.Equal(string.Empty, viewModel.Draft.SupplierName);
        Assert.Null(viewModel.Message);
    }
}
