using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Ui.Common.Areas.App.Services;
using SsalddelApp.Services.Application;
using SsalddelApp.Services.Commerce;
using SsalddelApp.Services.Commerce.Amazon;
using SsalddelApp.Services.Commerce.Coupang;
using SsalddelApp.Services.Commerce.Ebay;
using SsalddelApp.Services.Commerce.Listings.Commands;
using SsalddelApp.Services.Commerce.Listings.Events;
using SsalddelApp.Services.Commerce.Naver;
using SsalddelApp.Services.Commerce.Orders;
using SsalddelApp.Services.Commerce.Orders.Commands;
using SsalddelApp.Services.Commerce.Orders.Events;
using SsalddelApp.Services.Commerce.Shopify;
using SsalddelApp.ViewModels.Shipper;
using Microsoft.Extensions.DependencyInjection;

namespace SsalddelApp.Services;

internal static class ShipperSalesModule
{
    internal static IServiceCollection AddShipperSalesModule(this IServiceCollection services)
    {
        services.AddScoped<ShipperSalesService>();
        services.AddScoped<IShipperSalesService>(provider => provider.GetRequiredService<ShipperSalesService>());
        services.AddScoped<I판매채널계정Service>(provider => provider.GetRequiredService<ShipperSalesService>());
        services.AddScoped<I판매채널계정읽기Service>(provider => provider.GetRequiredService<ShipperSalesService>());
        services.AddScoped<I상품등록Service>(provider => provider.GetRequiredService<ShipperSalesService>());
        services.AddScoped<I채널출품Service>(provider => provider.GetRequiredService<ShipperSalesService>());
        services.AddTransient<화주판매채널계정PageViewModel>();
        services.AddSingleton<ICommerceChannelCatalog, CommerceChannelCatalog>();
        services.AddScoped<ICommerceChannelListingService, CommerceChannelListingService>();
        services.AddScoped<ICommerceOrderFulfillmentService, CommerceOrderFulfillmentService>();
        services.AddSingleton<ICommerceOrderSampleFeedService, CommerceOrderSampleFeedService>();
        services.AddTransient<OrderFulfillmentReadViewModel>();
        services.AddTransient<OrderFulfillmentSimulationViewModel>();
        services.AddTransient<OrderFulfillmentRestockPolicyViewModel>();
        services.AddTransient<OrderFulfillmentPickingViewModel>();
        services.AddTransient<OrderFulfillmentPackingViewModel>();
        services.AddTransient<OrderFulfillmentPageViewModel>();
        services.AddTransient<ProductListingReadViewModel>();
        services.AddTransient<ProductListingDraftViewModel>();
        services.AddTransient<ProductListingCreateViewModel>();
        services.AddTransient<ProductListingsPageViewModel>();
        services.AddScoped<IAppCommandHandler<ProcessCommerceOrderCommand, CommerceOrderFulfillmentResult>,
            ProcessCommerceOrderCommandHandler>();
        services.AddSingleton<IAppEventHandler<CommerceOrderProcessedEvent>, CommerceOrderProcessedEventHandler>();
        services.AddScoped<IAppCommandHandler<CreateChannelListingCommand, 채널출품항목응답?>,
            CreateChannelListingCommandHandler>();
        services.AddSingleton<IAppEventHandler<ChannelListingCreatedEvent>, ChannelListingCreatedEventHandler>();
        services.AddSingleton<IProductListingPayloadBuilder, NaverSmartStoreProductPayloadBuilder>();
        services.AddSingleton<IProductListingPayloadBuilder, CoupangWingProductPayloadBuilder>();
        services.AddSingleton<IProductListingPayloadBuilder, ShopifyProductPayloadBuilder>();
        services.AddSingleton<IProductListingPayloadBuilder, AmazonSpApiProductPayloadBuilder>();
        services.AddSingleton<IProductListingPayloadBuilder, EbayInventoryProductPayloadBuilder>();
        services.AddScoped<화주운송의뢰BulkApiService>();
        services.AddScoped<IShipperBulkRequestService>(provider =>
            provider.GetRequiredService<화주운송의뢰BulkApiService>());
        return services;
    }
}
