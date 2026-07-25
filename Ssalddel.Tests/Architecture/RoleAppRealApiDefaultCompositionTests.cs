namespace Ssalddel.Tests.Architecture;

public sealed class RoleAppRealApiDefaultCompositionTests
{
    [Fact]
    public void 기사_탐색문의함은_샘플이아닌_실제Api기능ViewModel을사용한다()
    {
        var page = Read("DriverApp/Components/Pages/Driver/02_Recommendation/탐색캠페인Page.razor");
        var viewModel = Read("DriverApp/ViewModels/Driver/Recommendation/기사탐색캠페인PageViewModel.cs");
        var client = Read("DriverApp/Services/DriverExplorationCampaignApiService.cs");
        var controller = Read("Ssalddel/Controllers/Driver/02_Recommendation/기사탐색캠페인Controller.cs");

        Assert.Contains("MvvmComponentBase<기사탐색캠페인PageViewModel>", page);
        Assert.DoesNotContain("SampleService", page);
        Assert.Contains("기사탐색캠페인기능ViewModel", viewModel);
        Assert.Contains("api/v1/driver/exploration-campaigns", client);
        Assert.Contains("api/v1/driver/exploration-campaigns", controller);
    }

    [Fact]
    public void 기사_공통콘텐츠의_기본등록은_서버구현이다()
    {
        var registrations = Read("DriverApp/Services/DriverServiceCollectionExtensions.cs");
        var client = Read("DriverApp/Services/CommonContents/Http공통콘텐츠Service.cs");
        var controller = Read("Ssalddel/Controllers/App/공통콘텐츠Controller.cs");

        Assert.Contains("I공통콘텐츠Service, Http공통콘텐츠Service", registrations);
        Assert.DoesNotContain("I공통콘텐츠Service, 샘플공통콘텐츠Service", registrations);
        Assert.Contains("api/v1/app/common-contents", client);
        Assert.Contains("api/v1/app/common-contents", controller);
    }

    [Fact]
    public void 화주_탐색문의함은_조회와응답을_서버Api로보낸다()
    {
        var page = Read("SsalddelApp/Components/Pages/ExplorationInbox.razor");
        var registrations = Read("SsalddelApp/Services/ShipperPlatformModule.cs");
        var client = Read("SsalddelApp/Services/HttpShipperExplorationInquiryService.cs");
        var controller = Read("Ssalddel/Controllers/Shipper/01_Request/화주탐색문의Controller.cs");

        Assert.Contains("InquiryService.목록조회Async", page);
        Assert.Contains("InquiryService.상세조회Async", page);
        Assert.Contains("InquiryService.응답Async", page);
        Assert.DoesNotContain("샘플 모드", page);
        Assert.Contains(
            "IShipperExplorationInquiryService, HttpShipperExplorationInquiryService",
            registrations);
        Assert.DoesNotContain("IShipperExplorationInquiryService>(provider", registrations);
        Assert.Contains("api/v1/shipper/exploration-inbox", client);
        Assert.Contains("api/v1/shipper/exploration-inbox", controller);
        Assert.Contains("HttpPost(\"{campaignId:long}/reply\")", controller);
    }

    [Fact]
    public void 화주_공통콘텐츠의_기본등록은_서버구현이다()
    {
        var registrations = Read("SsalddelApp/Services/ShipperPlatformModule.cs");
        var client = Read("SsalddelApp/Services/CommonContents/Http화주공통콘텐츠Service.cs");

        Assert.Contains(
            "I화주공통콘텐츠Service, Http화주공통콘텐츠Service",
            registrations);
        Assert.DoesNotContain(
            "I화주공통콘텐츠Service, 샘플화주공통콘텐츠Service",
            registrations);
        Assert.Contains("api/v1/app/common-contents/widget", client);
        Assert.DoesNotContain("example.invalid", client);
    }

    [Fact]
    public void 주문자_공동구매상품은_서버CatalogApi에서_조회한다()
    {
        var viewModel = Read("OrdererApp/ViewModels/GroupPurchaseCatalogViewModel.cs");
        var client = Read("OrdererApp/Services/GroupPurchaseProductCatalogService.cs");
        var controller = Read("Ssalddel/Controllers/Orderer/공동구매상품CatalogController.cs");

        Assert.Contains("service.GetProductsAsync", viewModel);
        Assert.DoesNotContain("hs-food-0203-pork-frozen", viewModel);
        Assert.Contains("api/v1/orderer/group-purchase-products", client);
        Assert.Contains("api/v1/orderer/group-purchase-products", controller);
    }

    [Fact]
    public void 화주_판매채널주문은_운영동기화후_서버원장을재조회한다()
    {
        var page = Read("SsalddelApp/Components/Pages/OrderFulfillmentSamples.razor");
        var viewModel = Read("SsalddelApp/ViewModels/Shipper/OrderFulfillmentServerInboxPageViewModel.cs");
        var controller = Read("Ssalddel/Controllers/Common/판매채널Controller.cs");

        Assert.Contains("Operational 서버 원장", Read("SsalddelApp/Components/Pages/OrderFulfillmentComponents/OrderFulfillmentRouteFrame.razor"));
        Assert.Contains("SyncDomesticAsync", page);
        Assert.Contains("syncClient.동기화Async", viewModel);
        Assert.Contains("원장조회Async", viewModel);
        Assert.Contains("HttpPost(\"orders/sync\")", controller);
    }

    [Fact]
    public void 기사_알림함은_서버목록과_읽음상태를사용한다()
    {
        var pageViewModel = Read("DriverApp/ViewModels/Driver/Notification/기사알림PageViewModels.cs");
        var client = Read("DriverApp/Services/DriverNotificationApiService.cs");
        var controller = Read("Ssalddel/Controllers/Driver/07_Notification/기사알림Controller.cs");

        Assert.DoesNotContain("IDriverSampleDataService", pageViewModel);
        Assert.Contains("_notificationApi.알림함조회Async", pageViewModel);
        Assert.Contains("_notificationApi.읽음처리Async", pageViewModel);
        Assert.Contains("알림함조회Async", client);
        Assert.Contains("HttpPut(\"{notificationId:long}/read\")", controller);
    }

    [Fact]
    public void 창고_작업진입_입고_피킹은_서버구현을기본등록한다()
    {
        var registrations = Read("WarehouseManagerApp/Services/WarehouseManagerServiceCollectionExtensions.cs");

        Assert.Contains("IWarehouseWorkEntryGateService, HttpWarehouseWorkEntryGateService", registrations);
        Assert.Contains("IInboundReceivingWorkflowService, HttpInboundReceivingWorkflowService", registrations);
        Assert.Contains("IWarehousePickingBatchWorkspaceService, HttpWarehousePickingBatchWorkspaceService", registrations);
        Assert.DoesNotContain("IWarehouseWorkEntryGateService, SampleWarehouseWorkEntryGateService", registrations);
        Assert.DoesNotContain("IInboundReceivingWorkflowService, SampleInboundReceivingWorkflowService", registrations);
        Assert.DoesNotContain("IWarehousePickingBatchWorkspaceService, SampleWarehousePickingBatchWorkspaceService", registrations);
    }

    private static string Read(string relativePath)
        => File.ReadAllText(Path.Combine(
            FindRepositoryRoot(),
            relativePath.Replace('/', Path.DirectorySeparatorChar)));

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Ssalddel 저장소 루트를 찾지 못했습니다.");
    }
}
