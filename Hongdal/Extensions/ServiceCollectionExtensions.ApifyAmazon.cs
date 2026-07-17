using Hongdal.Services.Content;
using Hongdal.Services.External.Apify;
using 홍달.Services.Options;

namespace Hongdal.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddApifyAmazonProductResearch(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ApifyAmazonOptions>(
            configuration.GetSection(ApifyAmazonOptions.SectionName));
        services.AddHttpClient<IApifyAmazonProductClient, ApifyAmazonProductClient>((sp, client) =>
        {
            var options = sp
                .GetRequiredService<Microsoft.Extensions.Options.IOptions<ApifyAmazonOptions>>()
                .Value;
            client.BaseAddress = new Uri($"{options.BaseUrl.TrimEnd('/')}/");
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(options.TimeoutSeconds, 30, 300));
        });
        services.AddScoped<IAmazon상품참고자료Service, Amazon상품참고자료Service>();
        return services;
    }
}
