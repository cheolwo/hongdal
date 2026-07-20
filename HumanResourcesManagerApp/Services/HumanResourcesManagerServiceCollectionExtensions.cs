using HumanResourcesManagerApp.Services.Security;
using HumanResourcesManagerApp.ViewModels;
using HumanResourcesManagerApp.ViewModels.HumanResources;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace HumanResourcesManagerApp.Services;

public static class HumanResourcesManagerServiceCollectionExtensions
{
    public static IServiceCollection AddHumanResourcesManagerApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IClientSecureTokenStore, HumanResourcesMauiSecureTokenStore>();
        services.TryAddSingleton<IClientSessionGuard, ClientSessionGuard>();
        services.TryAddSingleton<ClientAuthSession>();
        services.TryAddSingleton<HumanResourcesAccessTokenProvider>();
        services.TryAddSingleton<HumanResourcesAccessPolicyService>();
        services.TryAddScoped<HumanResourcesAuthApiService>();
        services.TryAddScoped<HumanResourcesPageAvailabilityService>();

        services.AddTransient<인사Controller기능모음ViewModel>();
        services.AddTransient<고용계약기능ViewModel>();
        services.AddTransient<참여혜택기능ViewModel>();
        services.AddTransient<인사역할기능ViewModel>();
        services.AddTransient<사회보험신고기능ViewModel>();
        services.AddTransient<인사Api기능모음ViewModel>();
        services.AddTransient<인사로그인ViewModel>();
        services.AddTransient<인사역할검토HomePageViewModel>();

        return services;
    }
}
