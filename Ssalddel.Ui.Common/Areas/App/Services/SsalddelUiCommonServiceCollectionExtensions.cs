using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public static class SsalddelUiCommonServiceCollectionExtensions
{
    public static IServiceCollection AddSsalddelCommunityWritingServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSsalddelUiCoreModule();
        services.AddCommunityWritingUiModule();
        return services;
    }

    public static IServiceCollection AddSsalddelCommunityWritingServices<TAccessTokenProvider>(
        this IServiceCollection services)
        where TAccessTokenProvider : class, ISsalddelAccessTokenProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<ISsalddelAccessTokenProvider>(provider =>
            provider.GetRequiredService<TAccessTokenProvider>());

        return services.AddSsalddelCommunityWritingServices();
    }

    public static IServiceCollection AddSsalddelUiCommonAppServices(this IServiceCollection services)
        => AddSsalddelUiCommonModules(services);

    public static IServiceCollection AddSsalddelUiCommonAppServices<TAccessTokenProvider>(
        this IServiceCollection services)
        where TAccessTokenProvider : class, ISsalddelAccessTokenProvider
    {
        services.TryAddScoped<ISsalddelAccessTokenProvider>(provider =>
            provider.GetRequiredService<TAccessTokenProvider>());

        return AddSsalddelUiCommonModules(services);
    }

    public static IServiceCollection AddSsalddelApiHttpClient(
        this IServiceCollection services,
        Uri baseAddress,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        TimeSpan? timeout = null)
        => services.AddSsalddelApiHttpClient(_ => baseAddress, lifetime, timeout);

    public static IServiceCollection AddSsalddelApiHttpClient(
        this IServiceCollection services,
        Func<IServiceProvider, Uri> baseAddressFactory,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(baseAddressFactory);

        if (timeout is { } value
            && value != Timeout.InfiniteTimeSpan
            && value <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        services.TryAdd(new ServiceDescriptor(
            typeof(HttpClient),
            provider => CreateHttpClient(baseAddressFactory(provider), timeout),
            lifetime));

        return services;
    }

    private static IServiceCollection AddSsalddelUiCommonModules(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSsalddelUiCoreModule();
        services.AddCommunityPlatformUiModule();
        services.AddGroupPurchaseUiModule();
        services.AddSalesUiModule();
        services.AddOrderUiModule();
        services.AddWarehouseUiModule();
        services.AddCustomsUiModule();
        services.AddFoodDiscoveryUiModule();
        services.AddMartDiscoveryUiModule();
        services.AddHumanResourcesUiModule();
        return services;
    }

    private static HttpClient CreateHttpClient(Uri baseAddress, TimeSpan? timeout)
    {
        var client = new HttpClient
        {
            BaseAddress = SsalddelApiEndpoint.NormalizeBaseAddress(baseAddress)
        };

        if (timeout is { } value)
        {
            client.Timeout = value;
        }

        return client;
    }
}
