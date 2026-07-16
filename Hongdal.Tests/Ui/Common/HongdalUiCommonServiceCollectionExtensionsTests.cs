using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Hongdal.Tests.Ui.Common;

public sealed class HongdalUiCommonServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(typeof(PlatformCommunityService))]
    [InlineData(typeof(PlatformHomeModeStateService))]
    [InlineData(typeof(PlatformDiagramPaletteStateService))]
    public void AddHongdalUiCommonAppServices_RegistersSharedStateAsScoped(
        Type serviceType)
    {
        var services = new ServiceCollection();

        services.AddHongdalUiCommonAppServices();

        var descriptor = Assert.Single(
            services,
            candidate => candidate.ServiceType == serviceType);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(serviceType, descriptor.ImplementationType);
    }

    [Fact]
    public void AddHongdalUiCommonAppServices_PreservesExistingRegistration()
    {
        var services = new ServiceCollection();
        services.AddSingleton<PlatformHomeModeStateService>();

        services.AddHongdalUiCommonAppServices();

        var descriptor = Assert.Single(
            services,
            candidate => candidate.ServiceType == typeof(PlatformHomeModeStateService));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddHongdalUiCommonAppServices_RegistersAgriculturalFisheriesPublicDataClient()
    {
        var services = new ServiceCollection();

        services.AddHongdalUiCommonAppServices();

        var descriptor = Assert.Single(
            services,
            candidate => candidate.ServiceType == typeof(I농수산공공데이터Client));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        Assert.Equal(typeof(농수산공공데이터Client), descriptor.ImplementationType);
    }

    [Fact]
    public void AddHongdalUiCommonAppServices_RegistersMvvmApiCompositionServices()
    {
        var services = new ServiceCollection();

        services.AddHongdalUiCommonAppServices();

        Assert.Contains(services, x =>
            x.ServiceType == typeof(IHongdalJsonApiClient)
            && x.ImplementationType == typeof(HongdalJsonApiClient)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공통Controller기능모음ViewModel)
            && x.ImplementationType == typeof(공통Controller기능모음ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I공동구매업무Service)
            && x.ImplementationType == typeof(PlatformCommunity공동구매업무Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I공동구매공급Service)
            && x.ImplementationType == typeof(PlatformCommunity공동구매공급Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I공동구매물류Service)
            && x.ImplementationType == typeof(PlatformCommunity공동구매물류Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I공동구매실행Service)
            && x.ImplementationType == typeof(공동구매실행Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I주문원장Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I공동구매창고Service)
            && x.ImplementationType == typeof(공동구매창고Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I입출고작업Service)
            && x.ImplementationType == typeof(입출고작업Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I입출고원장조회Service)
            && x.ImplementationType == typeof(PlatformCommunity입출고원장조회Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I공동구매원장절차Client)
            && x.ImplementationType == typeof(공동구매원장절차Client)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I공동수입원장전환Client)
            && x.ImplementationType == typeof(공동수입원장전환Client)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I판매채널Client)
            && x.ImplementationType == typeof(판매채널Client)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I판매채널계정Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I상품등록Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I채널출품Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I공동구매가격의사결정Service)
            && x.ImplementationType == typeof(공동구매가격의사결정Service)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매화면상태ViewModel)
            && x.ImplementationType == typeof(공동구매화면상태ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매실행상태ViewModel)
            && x.ImplementationType == typeof(공동구매실행상태ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매창고상태ViewModel)
            && x.ImplementationType == typeof(공동구매창고상태ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(입출고화면상태ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(입출고원장상태ViewModel)
            && x.ImplementationType == typeof(입출고원장상태ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매가격의사결정ViewModel)
            && x.ImplementationType == typeof(공동구매가격의사결정ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매실행기능ViewModel)
            && x.ImplementationType == typeof(공동구매실행기능ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매주문집계ViewModel)
            && x.ImplementationType == typeof(공동구매주문집계ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매재고배분ViewModel)
            && x.ImplementationType == typeof(공동구매재고배분ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매입고원장ViewModel)
            && x.ImplementationType == typeof(공동구매입고원장ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(입고ViewModel)
            && x.ImplementationType == typeof(입고ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(출고ViewModel)
            && x.ImplementationType == typeof(출고ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(입출고화면ViewModel)
            && x.ImplementationType == typeof(입출고화면ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(입고조회ViewModel)
            && x.ImplementationType == typeof(입고조회ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(출고포장ViewModel)
            && x.ImplementationType == typeof(출고포장ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동수입원장물류ViewModel)
            && x.ImplementationType == typeof(공동수입원장물류ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(I공동수입선적통관Client)
            && x.ImplementationType == typeof(공동수입선적통관Client)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동수입선적통관ViewModel)
            && x.ImplementationType == typeof(공동수입선적통관ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동수입통관동기화ViewModel)
            && x.ImplementationType == typeof(공동수입통관동기화ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매협상쟁점합의ViewModel)
            && x.ImplementationType == typeof(공동구매협상쟁점합의ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(국내판매ViewModel)
            && x.ImplementationType == typeof(국내판매ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(판매ViewModel)
            && x.ImplementationType == typeof(판매ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(상품등록ViewModel)
            && x.ImplementationType == typeof(상품등록ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(판매상품조회ViewModel)
            && x.ImplementationType == typeof(판매상품조회ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(주문ViewModel)
            && x.ImplementationType == typeof(주문ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(주문서명등록ViewModel)
            && x.ImplementationType == typeof(주문서명등록ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(해외수출ViewModel)
            && x.ImplementationType == typeof(해외수출ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매출고원장ViewModel)
            && x.ImplementationType == typeof(공동구매출고원장ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매화면ViewModel)
            && x.ImplementationType == typeof(공동구매화면ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public async Task AddHongdalUiCommonAppServices_SharesFineGrainedViewModelsWithinScope()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri("https://api.hongdal.test/")
        });
        services.AddSingleton<IJSRuntime, TestJsRuntime>();
        services.AddHongdalUiCommonAppServices();

        await using var provider = services.BuildServiceProvider(validateScopes: true);
        await using var scope = provider.CreateAsyncScope();
        var scopedProvider = scope.ServiceProvider;

        Assert.Same(
            scopedProvider.GetRequiredService<입고조회ViewModel>(),
            scopedProvider.GetRequiredService<입고ViewModel>().조회);
        Assert.Same(
            scopedProvider.GetRequiredService<출고포장ViewModel>(),
            scopedProvider.GetRequiredService<출고ViewModel>().포장);
        Assert.Same(
            scopedProvider.GetRequiredService<판매상품조회ViewModel>(),
            scopedProvider.GetRequiredService<판매ViewModel>().상품조회);
        Assert.Same(
            scopedProvider.GetRequiredService<주문서명등록ViewModel>(),
            scopedProvider.GetRequiredService<주문ViewModel>().서명등록);

        var warehouse = scopedProvider.GetRequiredService<입출고화면ViewModel>();
        Assert.Contains(
            scopedProvider.GetRequiredService<창고목록조회ViewModel>(),
            warehouse.기준정보세부업무목록);

        var groupPurchase = scopedProvider.GetRequiredService<공동구매화면ViewModel>();
        Assert.Contains(
            scopedProvider.GetRequiredService<공동구매목록조회조각ViewModel>(),
            groupPurchase.모집.세부업무목록);
        Assert.Contains(
            scopedProvider.GetRequiredService<공동구매협상쟁점합의ViewModel>(),
            groupPurchase.공급.세부업무목록);
        Assert.Contains(
            scopedProvider.GetRequiredService<공동수입통관동기화ViewModel>(),
            groupPurchase.공동수입.세부업무목록);
        Assert.Contains(
            scopedProvider.GetRequiredService<공동구매커머스문서조회ViewModel>(),
            groupPurchase.실행.세부업무목록);
    }

    [Fact]
    public void AddHongdalUiCommonAppServices_UsesRegisteredAccessTokenProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestAccessTokenProvider>();

        services.AddHongdalUiCommonAppServices<TestAccessTokenProvider>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        Assert.Same(
            provider.GetRequiredService<TestAccessTokenProvider>(),
            scope.ServiceProvider.GetRequiredService<IHongdalAccessTokenProvider>());
    }

    [Fact]
    public void AddHongdalApiHttpClient_NormalizesAddressAndPreservesOptions()
    {
        var services = new ServiceCollection();

        services.AddHongdalApiHttpClient(
            new Uri("https://api.hongdal.test/v1"),
            ServiceLifetime.Singleton,
            TimeSpan.FromSeconds(20));

        var descriptor = Assert.Single(
            services,
            candidate => candidate.ServiceType == typeof(HttpClient));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<HttpClient>();
        Assert.Equal(new Uri("https://api.hongdal.test/v1/"), client.BaseAddress);
        Assert.Equal(TimeSpan.FromSeconds(20), client.Timeout);
    }

    [Fact]
    public void AddHongdalApiHttpClient_PreservesExistingRegistration()
    {
        var existingClient = new HttpClient
        {
            BaseAddress = new Uri("https://existing.hongdal.test/")
        };
        var services = new ServiceCollection();
        services.AddSingleton(existingClient);

        services.AddHongdalApiHttpClient(new Uri("https://replacement.hongdal.test/"));

        var descriptor = Assert.Single(
            services,
            candidate => candidate.ServiceType == typeof(HttpClient));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);

        using var provider = services.BuildServiceProvider();
        Assert.Same(existingClient, provider.GetRequiredService<HttpClient>());
    }

    [Theory]
    [InlineData("https://api.hongdal.test/v1", "https://api.hongdal.test/v1/")]
    [InlineData("http://localhost:5104/", "http://localhost:5104/")]
    public void ResolveBaseAddress_NormalizesValidAddress(string value, string expected)
    {
        var result = HongdalApiEndpoint.ResolveBaseAddress(value);

        Assert.Equal(new Uri(expected), result);
    }

    [Fact]
    public void ResolveBaseAddress_RejectsNonHttpAddress()
    {
        Assert.Throws<ArgumentException>(
            () => HongdalApiEndpoint.ResolveBaseAddress("file:///tmp/hongdal"));
    }

    private sealed class TestAccessTokenProvider : IHongdalAccessTokenProvider
    {
        public string AccessToken => "test-token";
    }

    private sealed class TestJsRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            ValueTask.FromResult(default(TValue)!);

        public ValueTask<TValue> InvokeAsync<TValue>(
            string identifier,
            CancellationToken cancellationToken,
            object?[]? args) => ValueTask.FromResult(default(TValue)!);
    }
}
