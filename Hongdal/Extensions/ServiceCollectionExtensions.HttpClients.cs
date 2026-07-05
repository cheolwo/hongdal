using 홍달.Services.Dispatch.Notification;
using 홍달.Services.External.Customs;
using 홍달.Services.External.Google;
using 홍달.Services.External.KieAi;
using 홍달.Services.Notifications;
using 홍달.Services.Options;
using 홍달.Services.Payments;

namespace Hongdal.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<IGeocodingService, GoogleGeocodingService>();
        services.AddHttpClient<IRouteDistanceService, GoogleRouteDistanceService>();
        services.AddHttpClient<INaverCloudDirectionsService, NaverCloudDirectionsService>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NaverCloudDirectionsOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddHttpClient<IOpinetAveragePriceService, OpinetAveragePriceService>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpinetOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddHttpClient<INtsBusinessRegistrationService, NtsBusinessRegistrationService>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<NtsBusinessRegistrationOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddHttpClient<I해외제조업소조회Service, 해외제조업소조회Service>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<해외제조업소조회Options>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddHttpClient<I수입식품제품조회Service, 수입식품제품조회Service>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<수입식품제품조회Options>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddHttpClient<ITossPaymentsService, TossPaymentsService>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<TossPaymentsOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddHttpClient<IKieAiImageGenerationClient, KieAiImageGenerationClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<KieAiOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });
        services.AddHttpClient<I개인통관부호검증Service, Unipass개인통관부호검증Service>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CustomsOptions>>().Value;
            client.BaseAddress = new Uri(options.UnipassBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
        });
        services.AddHttpClient<I화물통관진행조회Service, 공공데이터화물통관진행조회Service>((sp, client) =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CustomsOptions>>().Value;
            client.BaseAddress = new Uri(options.CargoTrackingBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
        });
        services.AddHttpClient<IDriverRecommendationPushService, FcmDriverRecommendationPushService>();
        services.AddHttpClient<IFcmPushService, FirebaseFcmPushService>();

        return services;
    }
}
