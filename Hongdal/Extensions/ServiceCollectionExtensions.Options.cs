using Hongdal.Application.CommandProcessing;
using Hongdal.Services.LogisticsProcessing.SalesOrders;
using Hongdal.Security;
using Hongdal.Services.Security;
using 홍달.Infrastructure.BackgroundJobs.DispatchQueue;
using 홍달.Services.Dispatch.Queue;
using 홍달.Services.Documents;
using 홍달.Services.External.Customs;
using 홍달.Services.External.KieAi;
using 홍달.Services.External.PublicData;
using 홍달.Services.HIOPSAI;
using 홍달.Services.Notifications;
using 홍달.Services.Options;
using 홍달.Services.Payments;

namespace Hongdal.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<TossPaymentsOptions>(configuration.GetSection(TossPaymentsOptions.SectionName));
        services.Configure<GoogleCloudStorageOptions>(configuration.GetSection(GoogleCloudStorageOptions.SectionName));
        services.Configure<CommunityPostStorageOptions>(configuration.GetSection(CommunityPostStorageOptions.SectionName));
        services.Configure<KieAiOptions>(configuration.GetSection(KieAiOptions.SectionName));
        services.Configure<HIOPSAIOptions>(configuration.GetSection(HIOPSAIOptions.SectionName));
        services.Configure<NaverCloudDirectionsOptions>(configuration.GetSection(NaverCloudDirectionsOptions.SectionName));
        services.Configure<OpinetOptions>(configuration.GetSection(OpinetOptions.SectionName));
        services.Configure<NtsBusinessRegistrationOptions>(configuration.GetSection(NtsBusinessRegistrationOptions.SectionName));
        services.Configure<해외제조업소조회Options>(configuration.GetSection(해외제조업소조회Options.SectionName));
        services.Configure<수입식품제품조회Options>(configuration.GetSection(수입식품제품조회Options.SectionName));
        services.Configure<기사이용료정책Options>(configuration.GetSection(기사이용료정책Options.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<MongoDbOptions>(configuration.GetSection(MongoDbOptions.SectionName));
        services.Configure<PushNotificationsOptions>(configuration.GetSection(PushNotificationsOptions.SectionName));
        services.Configure<KakaoAlimTalkOptions>(configuration.GetSection(KakaoAlimTalkOptions.SectionName));
        services.Configure<KakaoLocalOptions>(configuration.GetSection(KakaoLocalOptions.SectionName));
        services.Configure<CommandProcessingOptions>(configuration.GetSection(CommandProcessingOptions.SectionName));
        services.Configure<WorkRelationshipSnapshotOptions>(configuration.GetSection(WorkRelationshipSnapshotOptions.SectionName));
        services.Configure<CommandFileStorageOptions>(configuration.GetSection(CommandFileStorageOptions.SectionName));
        services.Configure<CustomsOptions>(configuration.GetSection(CustomsOptions.SectionName));
        services.Configure<PublicDataOptions>(configuration.GetSection(PublicDataOptions.SectionName));
        services.Configure<VersionFeatureFlagsOptions>(configuration.GetSection(VersionFeatureFlagsOptions.SectionName));
        services.Configure<SalesChannelOrderSyncOptions>(configuration.GetSection(SalesChannelOrderSyncOptions.SectionName));
        services.Configure<배차큐정책Options>(configuration.GetSection("DispatchQueue"));
        services.Configure<배차큐배치작업Options>(configuration.GetSection(배차큐배치작업Options.SectionName));

        return services;
    }
}
