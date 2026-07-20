using Microsoft.Extensions.DependencyInjection.Extensions;
using Ssalddel.Client.Infrastructure.Security;
using Ssalddel.Ui.Common.Areas.App.Services;
using OrdererApp.Services.Security;

namespace OrdererApp.Services;

public static class OrdererSecurityServiceCollectionExtensions
{
    public static IServiceCollection AddOrdererSecurityServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<IClientSecureTokenStore, OrdererMauiSecureTokenStore>();
        services.TryAddSingleton<IClientSessionGuard, ClientSessionGuard>();
        services.TryAddSingleton<ClientAuthSession>();
        services.TryAddSingleton<OrdererAccessTokenProvider>();
        services.TryAddScoped<OrdererAuthApiService>();
        services.TryAddScoped<OrdererSessionService>();
        services.TryAddScoped<I주문자앱인증Service>(provider =>
            provider.GetRequiredService<OrdererSessionService>());

        return services;
    }
}
