using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ssalddel.Extensions;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class ProcessManagerModuleRegistrationTests
{
    [Fact]
    public void 공동구매_ProcessModule은_공동수입과_BackgroundService를_자동등록하지않는다()
    {
        var services = new ServiceCollection();

        services.AddSsalddelGroupPurchaseDemandProcessModule();

        AssertRegistered<I공동구매수요모집ProcessManager>(services);
        AssertRegistered<I공동구매수요모집ProcessStore>(services);
        AssertRegistered<I공동구매수요모집BatchCatalog>(services);
        AssertNotRegistered<I공동수입준비ProcessManager>(services);
        Assert.Equal(
            0,
            HostedServiceCount<공동구매수요모집DeadlineScanBackgroundService>(services));
    }

    [Fact]
    public void 공동수입_ProcessModule은_Port와_BackgroundService를_서버선택에맡긴다()
    {
        var services = new ServiceCollection();

        services.AddSsalddelGroupImportReadinessProcessModule();

        AssertRegistered<I공동수입준비ProcessManager>(services);
        AssertNotRegistered<I공동수입준비SourceGroupReader>(services);
        AssertNotRegistered<I공동수입준비BusinessCaseStore>(services);
        AssertNotRegistered<I공동수입준비EvidenceBatchReader>(services);
        AssertNotRegistered<I공동구매수요모집ProcessManager>(services);
        Assert.Equal(
            0,
            HostedServiceCount<공동수입준비정기점검BackgroundService>(services));
    }

    [Fact]
    public void 공동수입_LocalAdapter는_앞단계ProcessManager없이_Port만연결한다()
    {
        var services = new ServiceCollection();

        services.AddSsalddelGroupImportReadinessLocalAdapters();

        AssertRegistered<I공동수입준비SourceGroupReader>(services);
        AssertRegistered<I공동수입준비BusinessCaseStore>(services);
        AssertRegistered<I공동수입준비EvidenceBatchReader>(services);
        AssertNotRegistered<I공동구매수요모집ProcessManager>(services);
        AssertNotRegistered<I공동수입준비ProcessManager>(services);
    }

    [Fact]
    public void 공동수입_서버가제공한_RemoteAdapter는_LocalAdapter가덮어쓰지않는다()
    {
        var services = new ServiceCollection();
        services.AddSingleton<I공동수입준비SourceGroupReader>(_ =>
            throw new NotSupportedException("remote source adapter"));
        services.AddSingleton<I공동수입준비BusinessCaseStore>(_ =>
            throw new NotSupportedException("remote business-case adapter"));
        services.AddSingleton<I공동수입준비EvidenceBatchReader>(_ =>
            throw new NotSupportedException("remote evidence adapter"));

        services.AddSsalddelGroupImportReadinessLocalAdapters();

        Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(I공동수입준비SourceGroupReader));
        Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(I공동수입준비BusinessCaseStore));
        Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(I공동수입준비EvidenceBatchReader));
    }

    [Fact]
    public void BackgroundProcessing은_여러번호출해도_Worker를_한번만등록한다()
    {
        var services = new ServiceCollection();

        services.AddSsalddelGroupPurchaseDemandBackgroundProcessing();
        services.AddSsalddelGroupPurchaseDemandBackgroundProcessing();
        services.AddSsalddelGroupImportReadinessBackgroundProcessing();
        services.AddSsalddelGroupImportReadinessBackgroundProcessing();

        Assert.Equal(
            1,
            HostedServiceCount<공동구매수요모집DeadlineScanBackgroundService>(services));
        Assert.Equal(
            1,
            HostedServiceCount<공동수입준비정기점검BackgroundService>(services));
    }

    [Fact]
    public void 기존_전체등록은_두모듈과_LocalAdapter와_Worker를_유지한다()
    {
        var services = new ServiceCollection();

        services.AddSsalddelDomainServices();

        AssertRegistered<I공동구매수요모집ProcessManager>(services);
        AssertRegistered<I공동수입준비ProcessManager>(services);
        AssertRegistered<I공동수입준비SourceGroupReader>(services);
        AssertRegistered<I공동수입준비BusinessCaseStore>(services);
        AssertRegistered<I공동수입준비EvidenceBatchReader>(services);
        Assert.Equal(
            1,
            HostedServiceCount<공동구매수요모집DeadlineScanBackgroundService>(services));
        Assert.Equal(
            1,
            HostedServiceCount<공동수입준비정기점검BackgroundService>(services));
    }

    private static void AssertRegistered<TService>(IServiceCollection services)
        => Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(TService));

    private static void AssertNotRegistered<TService>(IServiceCollection services)
        => Assert.DoesNotContain(services, descriptor => descriptor.ServiceType == typeof(TService));

    private static int HostedServiceCount<THostedService>(IServiceCollection services)
        where THostedService : class, IHostedService
        => services.Count(descriptor =>
            descriptor.ServiceType == typeof(IHostedService)
            && descriptor.ImplementationType == typeof(THostedService));
}
