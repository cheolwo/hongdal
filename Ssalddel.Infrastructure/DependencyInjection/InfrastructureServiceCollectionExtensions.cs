using Microsoft.Extensions.DependencyInjection;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace 살뜰.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddSsalddelInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<IdentitySeedOptions>()
            .BindConfiguration(IdentitySeedOptions.SectionName)
            .Validate(
                options => !options.BootstrapAdmin.Enabled ||
                    (!string.IsNullOrWhiteSpace(options.BootstrapAdmin.UserName) &&
                     !string.IsNullOrWhiteSpace(options.BootstrapAdmin.Email) &&
                     !string.IsNullOrWhiteSpace(options.BootstrapAdmin.Password)),
                "IdentitySeed:BootstrapAdmin requires UserName, Email, and Password when enabled.")
            .Validate(
                options => !options.DevelopmentAccounts.Enabled ||
                    (!string.IsNullOrWhiteSpace(options.DevelopmentAccounts.AdminPassword) &&
                     !string.IsNullOrWhiteSpace(options.DevelopmentAccounts.DriverPassword) &&
                     !string.IsNullOrWhiteSpace(options.DevelopmentAccounts.ShipperPassword)),
                "IdentitySeed:DevelopmentAccounts requires all three passwords when enabled.")
            .ValidateOnStart();
        services.AddOptions<IsmsPProtectedDataOptions>()
            .BindConfiguration(IsmsPProtectedDataOptions.SectionName)
            .Validate(
                options => IsAes256Key(options.Aes256GcmKeyBase64),
                "IsmsPProtectedData:Aes256GcmKeyBase64 must be a Base64-encoded 32-byte key.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.HashSalt),
                "IsmsPProtectedData:HashSalt is required.")
            .ValidateOnStart();
        services.AddSingleton<IPersonalDataEncryptionService, DataProtectionPersonalDataEncryptionService>();
        services.AddSingleton<IIsmsPProtectedDataCryptoService, AesGcmIsmsPProtectedDataCryptoService>();
        services.AddSingleton<IIsmsPClientTransportProtectionService, RsaOaepAesGcmClientTransportProtectionService>();
        services.AddScoped<IIsmsPProtectedDataStorePreparationService, IsmsPProtectedDataStorePreparationService>();
        services.AddScoped<IIsmsPProtectedDataResponsePreparationService, IsmsPProtectedDataResponsePreparationService>();

        return services;
    }

    private static bool IsAes256Key(string? encodedKey)
    {
        if (string.IsNullOrWhiteSpace(encodedKey))
        {
            return false;
        }

        Span<byte> decoded = stackalloc byte[32];
        return Convert.TryFromBase64String(encodedKey, decoded, out var bytesWritten)
               && bytesWritten == decoded.Length;
    }
}
