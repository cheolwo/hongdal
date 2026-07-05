using Microsoft.Extensions.DependencyInjection;
using 홍달.Infrastructure.Security;

namespace 홍달.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPersonalDataEncryptionService, DataProtectionPersonalDataEncryptionService>();

        return services;
    }
}
