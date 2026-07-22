using Ssalddel.Client.Infrastructure.Transport;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using SsalddelApp.Services.Application;
using SsalddelApp.Services.Samples;
using SsalddelApp.Services.Samples.Commands;
using SsalddelApp.Services.Samples.Events;
using SsalddelApp.ViewModels.Shipper;
using Microsoft.Extensions.DependencyInjection;

namespace SsalddelApp.Services;

internal static class ShipperTransportModule
{
    internal static IServiceCollection AddShipperTransportModule(this IServiceCollection services)
    {
        services.AddTransient<화주Controller기능모음ViewModel>();
        services.AddTransient<화주운송의뢰조회ViewModel>();
        services.AddTransient<화주운송의뢰작성ViewModel>();
        services.AddScoped<운송의뢰작성ViewModel>();
        services.AddScoped<ShipperRequestAuthoringPageViewModel>();
        services.AddTransient<화주운송의뢰일괄ViewModel>();
        services.AddScoped<화주운송의뢰상태ViewModel>();
        services.AddScoped<화주운송의뢰목록조회ViewModel>();
        services.AddScoped<화주운송의뢰등록ViewModel>();
        services.AddScoped<화주운송의뢰수정ViewModel>();
        services.AddScoped<화주운송의뢰삭제ViewModel>();
        services.AddScoped<화주운송의뢰CrudViewModel>();
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
        services.AddSingleton<IAppCommandHandler<AddShipperRequestCommand, bool>, AddShipperRequestCommandHandler>();
        services.AddSingleton<IAppEventHandler<ShipperRequestAddedEvent>, ShipperRequestAddedEventHandler>();
        return services;
    }
}
