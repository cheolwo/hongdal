using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hongdal.Ui.Common.Areas.App.Services;

public static class HongdalUiCommonServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalUiCommonAppServices(this IServiceCollection services)
        => AddHongdalUiCommonCoreServices(services);

    public static IServiceCollection AddHongdalUiCommonAppServices<TAccessTokenProvider>(
        this IServiceCollection services)
        where TAccessTokenProvider : class, IHongdalAccessTokenProvider
    {
        services.TryAddScoped<IHongdalAccessTokenProvider>(
            serviceProvider => serviceProvider.GetRequiredService<TAccessTokenProvider>());

        return AddHongdalUiCommonCoreServices(services);
    }

    public static IServiceCollection AddHongdalApiHttpClient(
        this IServiceCollection services,
        Uri baseAddress,
        ServiceLifetime lifetime = ServiceLifetime.Scoped,
        TimeSpan? timeout = null)
        => services.AddHongdalApiHttpClient(_ => baseAddress, lifetime, timeout);

    public static IServiceCollection AddHongdalApiHttpClient(
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
            serviceProvider => CreateHttpClient(baseAddressFactory(serviceProvider), timeout),
            lifetime));

        return services;
    }

    private static IServiceCollection AddHongdalUiCommonCoreServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<PlatformCommunityService>();
        services.TryAddScoped<PlatformHomeModeStateService>();
        services.TryAddScoped<PlatformDiagramPaletteStateService>();
        services.AddScoped<HongdalIsmsPClientEncryptionService>();
        services.TryAddScoped<IHongdalAccessTokenProvider, EmptyHongdalAccessTokenProvider>();
        services.AddScoped<HongdalProtectedApiClient>();
        services.TryAddScoped<I농수산공공데이터Client, 농수산공공데이터Client>();
        services.AddScoped<CommunityLedgerNodeActionService>();
        services.AddScoped<YouTube관리콘텐츠Service>();
        services.AddScoped<PlatformCommunityDecorationStateService>();
        services.AddScoped<PlatformCommunityPostDraftStateService>();
        services.AddScoped<IDiagramCollaborationClientService>(_ => NoopDiagramCollaborationClientService.Instance);
        services.AddSingleton<IHongdalIdentifierCodeGenerator, ZxingHongdalIdentifierCodeGenerator>();
        return services;
    }

    private static HttpClient CreateHttpClient(Uri baseAddress, TimeSpan? timeout)
    {
        var client = new HttpClient
        {
            BaseAddress = HongdalApiEndpoint.NormalizeBaseAddress(baseAddress)
        };

        if (timeout is { } value)
        {
            client.Timeout = value;
        }

        return client;
    }
}
