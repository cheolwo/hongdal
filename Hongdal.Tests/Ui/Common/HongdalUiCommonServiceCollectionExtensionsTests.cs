using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

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
            x.ServiceType == typeof(공동구매화면상태ViewModel)
            && x.ImplementationType == typeof(공동구매화면상태ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매실행상태ViewModel)
            && x.ImplementationType == typeof(공동구매실행상태ViewModel)
            && x.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매실행기능ViewModel)
            && x.ImplementationType == typeof(공동구매실행기능ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, x =>
            x.ServiceType == typeof(공동구매화면ViewModel)
            && x.ImplementationType == typeof(공동구매화면ViewModel)
            && x.Lifetime == ServiceLifetime.Transient);
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
}
