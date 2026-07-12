using Microsoft.Extensions.DependencyInjection;

namespace Hongdal.Ui.Common.Areas.App.Services;

public static class HongdalUiCommonServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalUiCommonAppServices(this IServiceCollection services)
    {
        services.AddScoped<HongdalIsmsPClientEncryptionService>();
        services.AddScoped<HongdalProtectedApiClient>();
        services.AddScoped<IDiagramCollaborationClientService>(_ => NoopDiagramCollaborationClientService.Instance);
        services.AddSingleton<IHongdalIdentifierCodeGenerator, ZxingHongdalIdentifierCodeGenerator>();
        return services;
    }
}
