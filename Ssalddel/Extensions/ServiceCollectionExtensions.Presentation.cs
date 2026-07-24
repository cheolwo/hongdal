using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Ssalddel.Filters;
using Ssalddel.Security;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddSsalddelPresentation(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        IHostEnvironment? environment = null)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<SsalddelApiVersionFeatureFilter>();
        });
        services.AddHttpContextAccessor();
        services.AddMemoryCache();
        services.AddSignalR();
        services.AddOpenApi();
        AddPersonalDataProtection(services, configuration, environment);
        AddRequestRateLimiting(services);
        return services;
    }

    private static void AddPersonalDataProtection(
        IServiceCollection services,
        IConfiguration? configuration,
        IHostEnvironment? environment)
    {
        var options = configuration?
            .GetSection(PersonalDataProtectionOptions.SectionName)
            .Get<PersonalDataProtectionOptions>()
            ?? new PersonalDataProtectionOptions();
        services.AddSingleton(Options.Create(options));

        if (string.IsNullOrWhiteSpace(options.ApplicationName))
        {
            throw new InvalidOperationException(
                "PersonalDataProtection:ApplicationName is required.");
        }

        if (string.IsNullOrWhiteSpace(options.KeyRingPath))
        {
            throw new InvalidOperationException(
                "PersonalDataProtection:KeyRingPath is required.");
        }

        var contentRoot = environment?.ContentRootPath ?? AppContext.BaseDirectory;
        var keyRingPath = Path.IsPathRooted(options.KeyRingPath)
            ? options.KeyRingPath
            : Path.GetFullPath(Path.Combine(contentRoot, options.KeyRingPath));
        var dataProtection = services
            .AddDataProtection()
            .SetApplicationName(options.ApplicationName)
            .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath));

        if (!options.RequireCertificate &&
            string.IsNullOrWhiteSpace(options.CertificatePath))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.CertificatePath))
        {
            throw new InvalidOperationException(
                "PersonalDataProtection:CertificatePath is required when key encryption at rest is enabled.");
        }

        var certificatePath = Path.IsPathRooted(options.CertificatePath)
            ? options.CertificatePath
            : Path.GetFullPath(Path.Combine(contentRoot, options.CertificatePath));
        if (!File.Exists(certificatePath))
        {
            throw new InvalidOperationException(
                $"Data Protection certificate was not found at '{certificatePath}'.");
        }

        var certificate = X509CertificateLoader.LoadPkcs12FromFile(
            certificatePath,
            options.CertificatePassword,
            X509KeyStorageFlags.EphemeralKeySet);
        if (!certificate.HasPrivateKey)
        {
            certificate.Dispose();
            throw new InvalidOperationException(
                "The Data Protection certificate must contain a private key.");
        }

        dataProtection.ProtectKeysWithCertificate(certificate);
    }

    private static void AddRequestRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(
                RequestRateLimitPolicyNames.Authentication,
                httpContext => RateLimitPartition.GetSlidingWindowLimiter(
                    $"auth:{ResolveClientPartition(httpContext)}",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(5),
                        SegmentsPerWindow = 5,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            options.AddPolicy(
                RequestRateLimitPolicyNames.CommunityMutation,
                httpContext => RateLimitPartition.GetSlidingWindowLimiter(
                    $"community:{ResolveClientPartition(httpContext)}",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 20,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Title = "요청이 너무 많습니다.",
                        Status = StatusCodes.Status429TooManyRequests,
                        Type = "https://httpstatuses.com/429",
                        Detail = "잠시 후 다시 시도해 주세요.",
                        Instance = context.HttpContext.Request.Path.Value,
                        Extensions =
                        {
                            ["errorCode"] = "RateLimitExceeded",
                            ["traceId"] = context.HttpContext.TraceIdentifier
                        }
                    },
                    cancellationToken);
            };
        });
    }

    private static string ResolveClientPartition(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
