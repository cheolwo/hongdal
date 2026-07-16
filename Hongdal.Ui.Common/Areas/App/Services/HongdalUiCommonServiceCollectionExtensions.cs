using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Hongdal.Ui.Common.Areas.App.ViewModels;

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
        services.TryAddScoped<I공동구매업무Service, PlatformCommunity공동구매업무Service>();
        services.TryAddScoped<I공동구매공급Service, PlatformCommunity공동구매공급Service>();
        services.TryAddScoped<I공동구매물류Service, PlatformCommunity공동구매물류Service>();
        services.TryAddScoped<I공동구매실행Service, 공동구매실행Service>();
        services.TryAddScoped<I공동구매창고Service, 공동구매창고Service>();
        services.TryAddScoped<I공동구매원장절차Client, 공동구매원장절차Client>();
        services.TryAddScoped<I공동수입원장전환Client, 공동수입원장전환Client>();
        services.TryAddScoped<I공동수입선적통관Client, 공동수입선적통관Client>();
        services.TryAddScoped<I판매채널Client, 판매채널Client>();
        services.TryAddScoped<I공동구매가격의사결정Service, 공동구매가격의사결정Service>();
        services.TryAddScoped<PlatformHomeModeStateService>();
        services.TryAddScoped<PlatformDiagramPaletteStateService>();
        services.AddScoped<HongdalIsmsPClientEncryptionService>();
        services.TryAddScoped<IHongdalAccessTokenProvider, EmptyHongdalAccessTokenProvider>();
        services.AddScoped<HongdalProtectedApiClient>();
        services.TryAddScoped<IHongdalJsonApiClient, HongdalJsonApiClient>();
        services.TryAddTransient<공통Controller기능모음ViewModel>();
        services.TryAddScoped<공동구매화면상태ViewModel>();
        services.TryAddTransient<공동구매목록ViewModel>();
        services.TryAddScoped<공동구매거래경로판정ViewModel>();
        services.TryAddScoped<공동수입전환준비ViewModel>();
        services.TryAddScoped<공동구매거래경로분기ViewModel>();
        services.TryAddScoped<공동구매가격의사결정ViewModel>();
        services.TryAddTransient<공동구매제안ViewModel>();
        services.TryAddTransient<공동구매수요참여ViewModel>();
        services.TryAddTransient<공동구매이의검토ViewModel>();
        services.TryAddTransient<공동구매모집기능ViewModel>();
        services.TryAddTransient<공동구매모집마감ViewModel>();
        services.TryAddTransient<공동구매결의ViewModel>();
        services.TryAddTransient<공동구매전자서명ViewModel>();
        services.TryAddTransient<공동구매절차상태ViewModel>();
        services.TryAddTransient<공동구매합의기능ViewModel>();
        services.TryAddTransient<공동구매생산자연결ViewModel>();
        services.TryAddTransient<공동구매공급제안ViewModel>();
        services.TryAddTransient<공동구매공급적합성ViewModel>();
        services.TryAddTransient<공동구매협상ViewModel>();
        services.TryAddTransient<공동구매공급기능ViewModel>();
        services.TryAddTransient<공동구매이행계획ViewModel>();
        services.TryAddTransient<공동구매물류기능ViewModel>();
        services.TryAddTransient<국내공동구매분기ViewModel>();
        services.TryAddTransient<공동수입원장물류ViewModel>();
        services.TryAddTransient<공동수입선적통관ViewModel>();
        services.TryAddTransient<공동수입분기ViewModel>();
        services.TryAddTransient<국내판매ViewModel>();
        services.TryAddTransient<해외수출ViewModel>();
        services.TryAddScoped<공동구매실행상태ViewModel>();
        services.TryAddScoped<공동구매창고상태ViewModel>();
        services.TryAddTransient<공동구매자동집단ViewModel>();
        services.TryAddTransient<공동구매주문집계ViewModel>();
        services.TryAddTransient<공동구매재고배분ViewModel>();
        services.TryAddTransient<공동구매주문원장조회ViewModel>();
        services.TryAddTransient<공동구매하위원장ViewModel>();
        services.TryAddTransient<공동구매주문원장서명ViewModel>();
        services.TryAddTransient<공동구매주문원장ViewModel>();
        services.TryAddTransient<공동구매커머스이행ViewModel>();
        services.TryAddTransient<공동구매창고기준정보ViewModel>();
        services.TryAddTransient<공동구매입고원장ViewModel>();
        services.TryAddTransient<공동구매출고원장ViewModel>();
        services.TryAddTransient<공동구매창고기능ViewModel>();
        services.TryAddTransient<공동구매실행기능ViewModel>();
        services.TryAddTransient<공동구매화면ViewModel>();
        services.TryAddTransient<IBagua업무영역ViewModelFactory, Bagua업무영역ViewModelFactory>();
        services.TryAddSingleton<IBaguaTargetWorkspaceResolver, DefaultBaguaTargetWorkspaceResolver>();
        services.TryAddTransient<BaguaRoleTransitionPageViewModel>();
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
