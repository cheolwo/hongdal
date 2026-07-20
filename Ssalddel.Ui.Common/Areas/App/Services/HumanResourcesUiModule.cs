using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Services;

internal static class HumanResourcesUiModule
{
    internal static IServiceCollection AddHumanResourcesUiModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<I인사역할검토읽기Service, 인사역할검토Client>();
        services.TryAddScoped<I인사역할지원Service, 인사역할지원Client>();
        services.TryAddTransient<인사역할검토목록ViewModel>();
        services.TryAddTransient<인사역할검토상세ViewModel>();
        services.TryAddTransient<인사역할검토PageViewModel>();
        services.TryAddTransient<인사역할지원목록ViewModel>();
        services.TryAddTransient<인사역할지원작성ViewModel>();
        services.TryAddTransient<인사역할지원철회ViewModel>();
        services.TryAddTransient<인사역할지원PageViewModel>();

        return services;
    }
}
