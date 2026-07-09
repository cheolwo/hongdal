using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Client.Infrastructure;
using Hongdal.Client.Infrastructure.Security;
using Hongdal.Contracts.Common.Sales;
using Hongdal.Contracts.Shipper.Request;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShipperApp.Services.Application;
using ShipperApp.Services.Commerce;
using ShipperApp.Options;
using ShipperApp.Services.Commerce.Amazon;
using ShipperApp.Services.Commerce.Coupang;
using ShipperApp.Services.Commerce.Ebay;
using ShipperApp.Services.Commerce.Naver;
using ShipperApp.Services.Commerce.Listings.Commands;
using ShipperApp.Services.Commerce.Listings.Events;
using ShipperApp.Services.Commerce.Orders;
using ShipperApp.Services.Commerce.Orders.Commands;
using ShipperApp.Services.Commerce.Orders.Events;
using ShipperApp.Services.Commerce.Shopify;
using ShipperApp.Services.CommonContents;
using ShipperApp.Services.Customs;
using ShipperApp.Services.Customs.Commands;
using ShipperApp.Services.Customs.Events;
using ShipperApp.Services.Localization;
using ShipperApp.Services.Samples;
using ShipperApp.Services.Samples.Commands;
using ShipperApp.Services.Samples.Events;
using ShipperApp.Services.Security;
using ShipperApp.Services.Warehouse.Fulfillment;
using ShipperApp.Services.Warehouse.Reconsignment.Commands;
using ShipperApp.Services.Warehouse.Reconsignment.Events;

namespace ShipperApp.Services;

public static class ShipperServiceCollectionExtensions
{
    public static IServiceCollection AddShipperAppServices(this IServiceCollection services, IConfiguration configuration)
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
        services.Configure<FoodApiOptions>(configuration.GetSection(FoodApiOptions.SectionName));
        services.Configure<CoupangWingOptions>(configuration.GetSection(CoupangWingOptions.SectionName));
        services.Configure<NaverCommerceOptions>(configuration.GetSection(NaverCommerceOptions.SectionName));

        return services;
    }

    private static IServiceCollection AddShipperCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<InMemoryShipperStore>();
        services.AddSingleton<SampleShipperOperationsService>();
        services.AddScoped<IShipperOperationsService, ServerBackedShipperOperationsService>();
        services.AddSingleton<IClientSecureTokenStore, MauiSecureTokenStore>();
        services.AddSingleton<IClientSessionGuard, ClientSessionGuard>();
        services.AddSingleton<IAuthSession, AuthSession>();
        services.AddScoped<AuthApiService>();
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
