using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Services;

internal static class CustomsUiModule
{
    internal static IServiceCollection AddCustomsUiModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<I화주HS코드검토Client, 화주HS코드검토Client>();
        services.TryAddScoped<I화주HS코드검토접근Service, 화주HS코드검토접근Service>();
        services.TryAddTransient<화주HS코드검토접근ViewModel>();
        services.TryAddTransient<화주HS코드검토목록ViewModel>();
        services.TryAddTransient<화주HS코드검토상세ViewModel>();
        services.TryAddTransient<화주HS코드검토PageViewModel>();

        return services;
    }
}
