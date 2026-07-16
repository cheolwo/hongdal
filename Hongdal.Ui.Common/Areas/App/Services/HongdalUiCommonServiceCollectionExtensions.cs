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
        services.TryAddScoped<I주문원장Service>(serviceProvider =>
            serviceProvider.GetRequiredService<I공동구매실행Service>());
        services.TryAddScoped<I공동구매창고Service, 공동구매창고Service>();
        services.TryAddScoped<I입출고작업Service, 입출고작업Service>();
        services.TryAddScoped<I입출고원장조회Service, PlatformCommunity입출고원장조회Service>();
        services.TryAddScoped<I공동구매원장절차Client, 공동구매원장절차Client>();
        services.TryAddScoped<I공동수입원장전환Client, 공동수입원장전환Client>();
        services.TryAddScoped<I공동수입선적통관Client, 공동수입선적통관Client>();
        services.TryAddScoped<I판매채널Client, 판매채널Client>();
        services.TryAddScoped<I판매채널계정Service>(serviceProvider =>
            serviceProvider.GetRequiredService<I판매채널Client>());
        services.TryAddScoped<I상품등록Service>(serviceProvider =>
            serviceProvider.GetRequiredService<I판매채널Client>());
        services.TryAddScoped<I채널출품Service>(serviceProvider =>
            serviceProvider.GetRequiredService<I판매채널Client>());
        services.TryAddScoped<I공동구매가격의사결정Service, 공동구매가격의사결정Service>();
        services.TryAddScoped<PlatformHomeModeStateService>();
        services.TryAddScoped<PlatformDiagramPaletteStateService>();
        services.AddScoped<HongdalIsmsPClientEncryptionService>();
        services.TryAddScoped<IHongdalAccessTokenProvider, EmptyHongdalAccessTokenProvider>();
        services.TryAddScoped<IHongdal현재사용자Context, HongdalAccessToken현재사용자Context>();
        services.AddScoped<HongdalProtectedApiClient>();
        services.TryAddScoped<IHongdalJsonApiClient, HongdalJsonApiClient>();
        services.TryAddTransient<공통Controller기능모음ViewModel>();
        services.TryAddScoped<공동구매화면상태ViewModel>();
        services.TryAddScoped<공동구매목록ViewModel>();
        services.TryAddScoped<공동구매거래경로판정ViewModel>();
        services.TryAddScoped<공동수입전환준비ViewModel>();
        services.TryAddScoped<공동구매거래경로분기ViewModel>();
        services.TryAddScoped<공동구매가격의사결정ViewModel>();
        services.TryAddScoped<공동구매제안ViewModel>();
        services.TryAddScoped<공동구매수요참여ViewModel>();
        services.TryAddScoped<공동구매이의검토ViewModel>();
        services.TryAddScoped<공동구매목록조회조각ViewModel>();
        services.TryAddScoped<공동구매상세조회ViewModel>();
        services.TryAddScoped<공동구매제안등록조각ViewModel>();
        services.TryAddScoped<공동구매수요참여등록ViewModel>();
        services.TryAddScoped<공동구매이의등록ViewModel>();
        services.TryAddTransient<공동구매모집기능ViewModel>();
        services.TryAddScoped<공동구매모집마감ViewModel>();
        services.TryAddScoped<공동구매결의ViewModel>();
        services.TryAddScoped<공동구매전자서명ViewModel>();
        services.TryAddScoped<공동구매모집마감처리ViewModel>();
        services.TryAddScoped<공동구매결의문등록ViewModel>();
        services.TryAddScoped<공동구매결의서명준비ViewModel>();
        services.TryAddScoped<공동구매전자서명등록ViewModel>();
        services.TryAddTransient<공동구매절차상태ViewModel>();
        services.TryAddTransient<공동구매합의기능ViewModel>();
        services.TryAddScoped<공동구매생산자연결ViewModel>();
        services.TryAddScoped<공동구매공급제안ViewModel>();
        services.TryAddScoped<공동구매공급적합성ViewModel>();
        services.TryAddScoped<공동구매협상ViewModel>();
        services.TryAddScoped<공동구매생산자후보조회ViewModel>();
        services.TryAddScoped<공동구매생산자연락요청ViewModel>();
        services.TryAddScoped<공동구매대표후보조회ViewModel>();
        services.TryAddScoped<공동구매공급제안등록ViewModel>();
        services.TryAddScoped<공동구매공급적합성미리보기ViewModel>();
        services.TryAddScoped<공동구매협상이력조회ViewModel>();
        services.TryAddScoped<공동구매협상이벤트등록ViewModel>();
        services.TryAddScoped<공동구매협상쟁점등록ViewModel>();
        services.TryAddScoped<공동구매숙고의견등록ViewModel>();
        services.TryAddScoped<공동구매협상쟁점합의ViewModel>();
        services.TryAddTransient<공동구매공급기능ViewModel>();
        services.TryAddScoped<공동구매이행계획ViewModel>();
        services.TryAddScoped<공동구매이행계획미리보기ViewModel>();
        services.TryAddScoped<공동구매발주초안등록ViewModel>();
        services.TryAddTransient<공동구매물류기능ViewModel>();
        services.TryAddTransient<국내공동구매분기ViewModel>();
        services.TryAddScoped<공동수입원장물류ViewModel>();
        services.TryAddScoped<공동수입선적통관ViewModel>();
        services.TryAddScoped<공동수입원장조회ViewModel>();
        services.TryAddScoped<공동수입원장미리보기ViewModel>();
        services.TryAddScoped<공동수입원장전환ViewModel>();
        services.TryAddScoped<공동수입선적공개조회ViewModel>();
        services.TryAddScoped<공동수입선적관리목록조회ViewModel>();
        services.TryAddScoped<공동수입선적등록ViewModel>();
        services.TryAddScoped<공동수입선적이벤트등록ViewModel>();
        services.TryAddScoped<공동수입통관동기화ViewModel>();
        services.TryAddTransient<공동수입분기ViewModel>();
        services.TryAddScoped<판매업무상태ViewModel>();
        services.TryAddTransient<판매채널계정ViewModel>();
        services.TryAddTransient<상품등록ViewModel>();
        services.TryAddTransient<채널출품ViewModel>();
        services.TryAddScoped<판매채널계정조회ViewModel>();
        services.TryAddScoped<판매채널계정등록ViewModel>();
        services.TryAddScoped<판매상품조회ViewModel>();
        services.TryAddScoped<판매상품등록ViewModel>();
        services.TryAddScoped<채널출품조회ViewModel>();
        services.TryAddScoped<채널출품등록ViewModel>();
        services.TryAddTransient<판매ViewModel>();
        services.TryAddTransient<국내판매ViewModel>();
        services.TryAddTransient<해외수출ViewModel>();
        services.TryAddScoped<공동구매실행상태ViewModel>();
        services.TryAddScoped<주문업무상태ViewModel>();
        services.TryAddScoped<주문조회ViewModel>();
        services.TryAddTransient<주문하위원장ViewModel>();
        services.TryAddTransient<주문서명ViewModel>();
        services.TryAddScoped<주문하위원장연결ViewModel>();
        services.TryAddScoped<주문하위원장분리ViewModel>();
        services.TryAddScoped<주문서명상태조회ViewModel>();
        services.TryAddScoped<주문서명준비ViewModel>();
        services.TryAddScoped<주문서명등록ViewModel>();
        services.TryAddTransient<주문ViewModel>();
        services.TryAddScoped<공동구매창고상태ViewModel>();
        services.TryAddScoped<입출고화면상태ViewModel>(serviceProvider =>
            serviceProvider.GetRequiredService<공동구매창고상태ViewModel>());
        services.TryAddScoped<입출고원장상태ViewModel>();
        services.TryAddScoped<공동구매자동집단ViewModel>();
        services.TryAddScoped<공동구매자동집단조회ViewModel>();
        services.TryAddScoped<공동구매자동수요등록ViewModel>();
        services.TryAddTransient<공동구매주문집계ViewModel>();
        services.TryAddTransient<공동구매재고배분ViewModel>();
        services.TryAddScoped<공동구매주문원장조회ViewModel>();
        services.TryAddScoped<공동구매하위원장ViewModel>();
        services.TryAddScoped<공동구매주문원장서명ViewModel>();
        services.TryAddScoped<공동구매주문원장상세조회ViewModel>();
        services.TryAddScoped<공동구매주문하위원장연결ViewModel>();
        services.TryAddScoped<공동구매주문하위원장분리ViewModel>();
        services.TryAddScoped<공동구매주문서명상태조회ViewModel>();
        services.TryAddScoped<공동구매주문서명준비ViewModel>();
        services.TryAddScoped<공동구매주문서명등록ViewModel>();
        services.TryAddTransient<공동구매주문원장ViewModel>();
        services.TryAddScoped<공동구매커머스이행ViewModel>();
        services.TryAddScoped<공동구매커머스이행조회ViewModel>();
        services.TryAddScoped<공동구매커머스문서조회ViewModel>();
        services.TryAddTransient<입출고원장목록ViewModel>();
        services.TryAddTransient<입고원장ViewModel>();
        services.TryAddTransient<출고원장ViewModel>();
        services.TryAddScoped<창고기준정보ViewModel>();
        services.TryAddScoped<창고목록조회ViewModel>();
        services.TryAddScoped<창고등록ViewModel>();
        services.TryAddScoped<창고사용자조회ViewModel>();
        services.TryAddScoped<창고사용자등록ViewModel>();
        services.TryAddScoped<입고조회ViewModel>();
        services.TryAddScoped<입고등록ViewModel>();
        services.TryAddScoped<입고완료ViewModel>();
        services.TryAddScoped<입고재고조회ViewModel>();
        services.TryAddScoped<입고검수ViewModel>();
        services.TryAddScoped<입고적재ViewModel>();
        services.TryAddScoped<출고재고조회ViewModel>();
        services.TryAddScoped<출고포장ViewModel>();
        services.TryAddScoped<출고운송인계ViewModel>();
        services.TryAddTransient<입고ViewModel>();
        services.TryAddTransient<출고ViewModel>();
        services.TryAddTransient<입출고화면ViewModel>();
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
