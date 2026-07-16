using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using Hongdal.Client.Infrastructure;
using Hongdal.Client.Infrastructure.Security;
using Hongdal.Client.Infrastructure.Transport;
using Hongdal.Contracts.Common.Sales;
using Hongdal.Contracts.Shipper.Request;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HongdalApp.Services.Application;
using HongdalApp.Services.Commerce;
using HongdalApp.Options;
using HongdalApp.Services.Commerce.Amazon;
using HongdalApp.Services.Commerce.Coupang;
using HongdalApp.Services.Commerce.Ebay;
using HongdalApp.Services.Commerce.Naver;
using HongdalApp.Services.Commerce.Listings.Commands;
using HongdalApp.Services.Commerce.Listings.Events;
using HongdalApp.Services.Commerce.Orders;
using HongdalApp.Services.Commerce.Orders.Commands;
using HongdalApp.Services.Commerce.Orders.Events;
using HongdalApp.Services.Commerce.Shopify;
using HongdalApp.Services.CommonContents;
using HongdalApp.Services.Customs;
using HongdalApp.Services.Customs.Commands;
using HongdalApp.Services.Customs.Events;
using HongdalApp.Services.Localization;
using HongdalApp.Services.Samples;
using HongdalApp.Services.Samples.Commands;
using HongdalApp.Services.Samples.Events;
using HongdalApp.Services.Security;
using Hongdal.Client.Infrastructure.Notifications;
using HongdalApp.Services.Warehouse.Fulfillment;
using HongdalApp.Services.Warehouse.Reconsignment.Commands;
using HongdalApp.Services.Warehouse.Reconsignment.Events;
using HongdalApp.ViewModels.Shipper;

namespace HongdalApp.Services;

public static class ShipperServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalAppServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddShipperOptions(configuration);
        services.AddShipperCoreServices();
        services.AddShipperWarehouseServices();
        services.AddShipperSalesServices();
        services.AddShipperExternalApiClients();

        return services;
    }

    private static IServiceCollection AddShipperOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ClientDataModeOptions>(configuration.GetSection(ClientDataModeOptions.SectionName));
        services.Configure<ClientDataModeOptions>(ApplyClientDataModeEnvironmentOverrides);
        services.Configure<ShipperSmokeOptions>(configuration.GetSection(ShipperSmokeOptions.SectionName));
        services.Configure<ShipperSmokeOptions>(ApplyShipperSmokeEnvironmentOverrides);
        services.Configure<CoupangWingOptions>(configuration.GetSection(CoupangWingOptions.SectionName));
        services.Configure<NaverCommerceOptions>(configuration.GetSection(NaverCommerceOptions.SectionName));

        return services;
    }

    private static IServiceCollection AddShipperCoreServices(this IServiceCollection services)
    {
        services.AddTransient<화주Controller기능모음ViewModel>();
        services.AddTransient<화주운송의뢰기능ViewModel>();
        services.AddTransient<화주창고기능ViewModel>();
        services.AddTransient<화주판매기능ViewModel>();
        services.AddTransient<화주Api기능모음ViewModel>();
        services.AddSingleton<InMemoryShipperStore>();
        services.AddSingleton<SampleShipperOperationsService>();
        services.AddScoped<FakeShipperPaymentService>();
        services.AddSingleton<ITransportRequestLedgerObserver, TransportRequestLedgerObserver>();
        services.AddScoped<ServerBackedShipperOperationsService>();
        services.AddScoped<IShipperOperationsService, SmokeAwareShipperOperationsService>();
        services.AddSingleton<IClientSecureTokenStore, MauiSecureTokenStore>();
        services.AddSingleton<IClientSessionGuard, ClientSessionGuard>();
        services.AddSingleton<IAuthSession, AuthSession>();
        services.AddSingleton<IHongdalMobilePushTokenProvider, NullHongdalMobilePushTokenProvider>();
        services.AddScoped<AuthApiService>();
        services.AddSingleton<I꾸미기보유권LocalStore, Maui꾸미기보유권LocalStore>();
        services.AddScoped<꾸미기보유권동기화Service>();
        services.AddSingleton<HongdalClientRoleService>();
        services.AddSingleton<ShipperOperatingProfileService>();
        services.AddSingleton<IWarehouseWorkEntryGateService, SampleWarehouseWorkEntryGateService>();
        services.AddSingleton<ShipperViewVisibilityService>();
        services.AddSingleton<ShipperLocalizationService>();
        services.AddSingleton<탐색문의샘플Service>();
        services.AddSingleton<IShipperExplorationInquiryService>(sp => sp.GetRequiredService<탐색문의샘플Service>());
        services.AddSingleton<I화주공통콘텐츠Service, 샘플화주공통콘텐츠Service>();
        services.AddSingleton<IAppEventPublisher, AppEventPublisher>();
        services.AddSingleton<IProductHsCodeInferenceService, ProductHsCodeInferenceService>();
        services.AddSingleton<IHsCodeAgencyCapabilityService, SampleHsCodeAgencyCapabilityService>();
        services.AddSingleton<ICustomsBrokerDirectory, SampleCustomsBrokerDirectory>();
        services.AddSingleton<ICustomsHsReviewService, CustomsHsReviewService>();
        services.AddSingleton<IAppCommandHandler<RequestCustomsHsReviewCommand, CustomsHsReviewRequest?>, RequestCustomsHsReviewCommandHandler>();
        services.AddSingleton<IAppCommandHandler<AssignCustomsBrokerCommand, bool>, AssignCustomsBrokerCommandHandler>();
        services.AddSingleton<IAppCommandHandler<CompleteCustomsHsReviewCommand, bool>, CompleteCustomsHsReviewCommandHandler>();
        services.AddSingleton<IAppCommandHandler<AddShipperRequestCommand, bool>, AddShipperRequestCommandHandler>();
        services.AddSingleton<IAppEventHandler<CustomsHsReviewRequestedEvent>, CustomsHsReviewRequestedEventHandler>();
        services.AddSingleton<IAppEventHandler<CustomsBrokerAssignedEvent>, CustomsBrokerAssignedEventHandler>();
        services.AddSingleton<IAppEventHandler<CustomsHsReviewCompletedEvent>, CustomsHsReviewCompletedEventHandler>();
        services.AddSingleton<IAppEventHandler<ShipperRequestAddedEvent>, ShipperRequestAddedEventHandler>();

        return services;
    }

    private static void ApplyClientDataModeEnvironmentOverrides(ClientDataModeOptions options)
    {
        ApplyBooleanEnvironmentOverride("ClientDataMode__AllowSampleFallback", value => options.AllowSampleFallback = value);
        ApplyBooleanEnvironmentOverride("ClientDataMode__AllowDevelopmentSnapshotFallback", value => options.AllowDevelopmentSnapshotFallback = value);
        ApplyBooleanEnvironmentOverride("ClientDataMode__RequireServerLedgerForV1Smoke", value => options.RequireServerLedgerForV1Smoke = value);
    }

    private static void ApplyShipperSmokeEnvironmentOverrides(ShipperSmokeOptions options)
    {
        var startPath = Environment.GetEnvironmentVariable("ShipperSmoke__StartPath");
        if (!string.IsNullOrWhiteSpace(startPath))
        {
            options.StartPath = startPath;
        }
    }

    private static void ApplyBooleanEnvironmentOverride(string name, Action<bool> apply)
    {
        var rawValue = Environment.GetEnvironmentVariable(name);
        if (bool.TryParse(rawValue, out var value))
        {
            apply(value);
        }
    }

    private static IServiceCollection AddShipperWarehouseServices(this IServiceCollection services)
    {
        services.AddScoped<ShipperWarehouseService>();
        services.AddScoped<IWarehouseWorkspaceService>(sp => sp.GetRequiredService<ShipperWarehouseService>());
        services.AddScoped<IShipperWarehouseWorkflowService>(sp => sp.GetRequiredService<ShipperWarehouseService>());
        services.AddScoped<IWarehousePickingPlanner, WarehousePickingPlanner>();
        services.AddScoped<IAppCommandHandler<CreateReconsignmentOrderCommand, 화주운송의뢰응답?>, CreateReconsignmentOrderCommandHandler>();
        services.AddSingleton<IAppEventHandler<ReconsignmentOrderCreatedEvent>, ReconsignmentOrderCreatedEventHandler>();

        return services;
    }

    private static IServiceCollection AddShipperSalesServices(this IServiceCollection services)
    {
        services.AddScoped<ShipperSalesService>();
        services.AddScoped<IShipperSalesService>(sp => sp.GetRequiredService<ShipperSalesService>());
        services.AddSingleton<ICommerceChannelCatalog, CommerceChannelCatalog>();
        services.AddScoped<ICommerceChannelListingService, CommerceChannelListingService>();
        services.AddScoped<ICommerceOrderFulfillmentService, CommerceOrderFulfillmentService>();
        services.AddSingleton<ICommerceOrderSampleFeedService, CommerceOrderSampleFeedService>();
        services.AddScoped<IAppCommandHandler<ProcessCommerceOrderCommand, CommerceOrderFulfillmentResult>, ProcessCommerceOrderCommandHandler>();
        services.AddSingleton<IAppEventHandler<CommerceOrderProcessedEvent>, CommerceOrderProcessedEventHandler>();
        services.AddScoped<IAppCommandHandler<CreateChannelListingCommand, 채널출품항목응답?>, CreateChannelListingCommandHandler>();
        services.AddSingleton<IAppEventHandler<ChannelListingCreatedEvent>, ChannelListingCreatedEventHandler>();
        services.AddSingleton<IProductListingPayloadBuilder, NaverSmartStoreProductPayloadBuilder>();
        services.AddSingleton<IProductListingPayloadBuilder, CoupangWingProductPayloadBuilder>();
        services.AddSingleton<IProductListingPayloadBuilder, ShopifyProductPayloadBuilder>();
        services.AddSingleton<IProductListingPayloadBuilder, AmazonSpApiProductPayloadBuilder>();
        services.AddSingleton<IProductListingPayloadBuilder, EbayInventoryProductPayloadBuilder>();
        services.AddScoped<화주운송의뢰BulkApiService>();
        services.AddScoped<IShipperBulkRequestService>(sp => sp.GetRequiredService<화주운송의뢰BulkApiService>());

        return services;
    }

    private static IServiceCollection AddShipperExternalApiClients(this IServiceCollection services)
    {
        services.AddScoped<배차주소ApiService>();
        services.AddSingleton<INaverCommerceSignatureGenerator, BCryptNaverCommerceSignatureGenerator>();
        services.AddHttpClient<INaverCommerceTokenProvider, NaverCommerceTokenProvider>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NaverCommerceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddHttpClient<INaverSmartStoreProductClient, NaverSmartStoreProductClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NaverCommerceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddSingleton<ICoupangWingSignatureGenerator, HmacCoupangWingSignatureGenerator>();
        services.AddHttpClient<ICoupangWingProductClient, CoupangWingProductClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CoupangWingOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}
