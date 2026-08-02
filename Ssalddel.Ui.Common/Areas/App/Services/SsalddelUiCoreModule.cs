using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ssalddel.Ui.Common.Areas.App.Services;

internal static class SsalddelUiCoreModule
{
    internal static IServiceCollection AddSsalddelUiCoreModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<SsalddelIsmsPClientEncryptionService>();
        services.TryAddScoped<ISsalddelAccessTokenProvider, EmptySsalddelAccessTokenProvider>();
        services.TryAddScoped<ISsalddel현재사용자Context, SsalddelAccessToken현재사용자Context>();
        services.TryAddScoped<SsalddelProtectedApiClient>();
        services.TryAddScoped<ISsalddelJsonApiClient, SsalddelJsonApiClient>();
        services.TryAddScoped<AppContextImageAssetClient>();
        services.TryAddScoped<I주문자앱인증Service, 미구성주문자앱인증Service>();
        services.TryAddTransient<공통Controller기능모음ViewModel>();
        services.TryAddTransient<주문자앱인증ViewModel>();

        return services;
    }
}
