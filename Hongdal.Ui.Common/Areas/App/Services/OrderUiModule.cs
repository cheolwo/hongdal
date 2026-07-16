using Hongdal.Ui.Common.Areas.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hongdal.Ui.Common.Areas.App.Services;

internal static class OrderUiModule
{
    internal static IServiceCollection AddOrderUiModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<I주문원장Service>(provider =>
            provider.GetRequiredService<I공동구매실행Service>());
        services.TryAddScoped<주문업무상태ViewModel>();
        services.TryAddScoped<주문조회ViewModel>();
        services.TryAddTransient<주문하위원장ViewModel>();
        services.TryAddTransient<주문서명ViewModel>();
        services.TryAddScoped<주문하위원장조회ViewModel>();
        services.TryAddScoped<주문하위원장연결ViewModel>();
        services.TryAddScoped<주문하위원장수정ViewModel>();
        services.TryAddScoped<주문하위원장분리ViewModel>();
        services.TryAddScoped<주문하위원장관계CrudViewModel>();
        services.TryAddScoped<주문서명상태조회ViewModel>();
        services.TryAddScoped<주문서명준비ViewModel>();
        services.TryAddScoped<주문서명등록ViewModel>();
        services.TryAddTransient<주문ViewModel>();

        return services;
    }
}
