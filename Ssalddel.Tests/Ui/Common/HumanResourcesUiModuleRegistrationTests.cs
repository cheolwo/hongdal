using Microsoft.Extensions.DependencyInjection;
using Ssalddel.Ui.Common.Areas.App.Services;
using Ssalddel.Ui.Common.Areas.App.ViewModels;

namespace Ssalddel.Tests.Ui.Common;

public sealed class HumanResourcesUiModuleRegistrationTests
{
    [Fact]
    public void 공용앱서비스는_Hr역할검토의읽기Service와분리된ViewModel을등록한다()
    {
        var services = new ServiceCollection();

        services.AddSsalddelUiCommonAppServices();

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(I인사역할검토읽기Service)
            && descriptor.ImplementationType == typeof(인사역할검토Client)
            && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(인사역할검토목록ViewModel)
            && descriptor.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(인사역할검토상세ViewModel)
            && descriptor.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(인사역할검토PageViewModel)
            && descriptor.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(I인사역할지원Service)
            && descriptor.ImplementationType == typeof(인사역할지원Client)
            && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(인사역할지원목록ViewModel)
            && descriptor.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(인사역할지원작성ViewModel)
            && descriptor.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(인사역할지원철회ViewModel)
            && descriptor.Lifetime == ServiceLifetime.Transient);
        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(인사역할지원PageViewModel)
            && descriptor.Lifetime == ServiceLifetime.Transient);
    }
}
