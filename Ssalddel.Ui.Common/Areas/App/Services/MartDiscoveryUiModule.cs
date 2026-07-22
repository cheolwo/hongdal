using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Ui.Common.Areas.App.Services;

internal static class MartDiscoveryUiModule
{
    internal static IServiceCollection AddMartDiscoveryUiModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<I마트페이지접근Service, 마트페이지접근Service>();
        services.TryAddScoped<I마트공개상품읽기Service, 마트공개상품Client>();
        services.TryAddScoped<I마트공개상품후기작성Service, 마트공개상품후기Client>();
        services.TryAddScoped<I마트주문요청Service, 마트주문요청Client>();
        services.TryAddScoped<I마트피킹읽기Service, 마트피킹Client>();
        services.TryAddTransient<마트페이지접근ViewModel>();
        services.TryAddTransient<마트공개상품목록ViewModel>();
        services.TryAddTransient<마트공개상품상세ViewModel>();
        services.TryAddTransient<마트공개상품후기작성ViewModel>();
        services.TryAddTransient<마트공개상품후기PageViewModel>();
        services.TryAddTransient<마트주문작성ViewModel>();
        services.TryAddTransient<마트주문요청상세ViewModel>();
        services.TryAddTransient<마트주문작성PageViewModel>();
        services.TryAddTransient<마트피킹주문목록ViewModel>();
        services.TryAddTransient<마트피킹주문상세ViewModel>();
        services.TryAddTransient<마트피킹작업PageViewModel>();

        return services;
    }
}
