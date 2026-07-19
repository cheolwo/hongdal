using Microsoft.Extensions.DependencyInjection;
using 살뜰.Infrastructure.Security;

namespace 살뜰.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSsalddelInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<IsmsPProtectedDataOptions>()
            .BindConfiguration(IsmsPProtectedDataOptions.SectionName)
            .Validate(
                options => !options.FailWhenKeyMissing ||
                    !string.IsNullOrWhiteSpace(options.Aes256GcmKeyBase64),
                "IsmsPProtectedData:Aes256GcmKeyBase64 is required when FailWhenKeyMissing is true.");
        services.AddSingleton<IPersonalDataEncryptionService, DataProtectionPersonalDataEncryptionService>();
        services.AddSingleton<IIsmsPProtectedDataCryptoService, AesGcmIsmsPProtectedDataCryptoService>();
        services.AddSingleton<IIsmsPClientTransportProtectionService, RsaOaepAesGcmClientTransportProtectionService>();
        services.AddScoped<IIsmsPProtectedDataStorePreparationService, IsmsPProtectedDataStorePreparationService>();
        services.AddScoped<IIsmsPProtectedDataResponsePreparationService, IsmsPProtectedDataResponsePreparationService>();

        return services;
    }
}
