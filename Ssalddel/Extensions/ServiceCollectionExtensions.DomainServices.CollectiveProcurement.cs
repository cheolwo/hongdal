using Ssalddel.Services.CollectiveProcurement;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Sales;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    private static IServiceCollection AddSsalddelCollectiveProcurementDomainServices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<I주문자집단배송권조회Service, 주문자집단배송권조회Service>();
        services.AddSingleton<I주문자집단자동배정Service, 주문자집단자동배정Service>();
        services.AddSingleton<I공동구매물류워크플로우저장소, Mongo공동구매물류워크플로우저장소>();
        services.AddSingleton<I공동구매주문자집단화Engine, 공동구매주문자집단화Engine>();
        services.AddSingleton<I공동구매체험Service, 공동구매체험Service>();
        services.AddSingleton<Mongo공동구매자동집단화저장소>();
        services.AddSingleton<I공동구매자동집단화저장소>(provider =>
            provider.GetRequiredService<Mongo공동구매자동집단화저장소>());
        services.AddSingleton<I공동구매수요모집ProcessStore>(provider =>
            provider.GetRequiredService<Mongo공동구매자동집단화저장소>());
        services.AddSingleton<I공동구매수요모집ProcessManager, 공동구매수요모집ProcessManager>();
        services.AddHostedService<공동구매수요모집DeadlineScanBackgroundService>();
        services.AddScoped<I공동구매수령창고Service, 공동구매수령창고Service>();
        services.AddScoped<I공동구매개별원함원장Service, 공동구매개별원함원장Service>();
        services.AddScoped<I공동구매개별원함자동집단투영Service, 공동구매개별원함자동집단투영Service>();
        services.AddScoped<I공동구매내원함조회UseCase, 공동구매내원함조회UseCase>();
        services.AddScoped<I공동구매개별주문원장Service, 공동구매개별주문원장Service>();
        services.AddSingleton<ICommunityGroupPurchaseDemandHandoff, CommunityVoteOrdererDemandHandoff>();
        services.AddSingleton<ICommunityProducerMemberDirectory, UnconnectedCommunityProducerMemberDirectory>();
        services.AddSingleton<ICommunityGroupPurchaseRepresentativeDirectory, UnconnectedCommunityGroupPurchaseRepresentativeDirectory>();
        services.AddSingleton<IDomesticProducerContactRequestDraftStore, MongoDomesticProducerContactRequestDraftStore>();
        services.AddSingleton<IDomesticProducerSupplyOfferDraftStore, MongoDomesticProducerSupplyOfferDraftStore>();
        services.AddScoped<IDomesticGroupPurchaseProducerConnectionService, DomesticGroupPurchaseProducerConnectionService>();
        services.AddSingleton<IDomesticGroupPurchaseFulfillmentOrderDraftStore, MongoDomesticGroupPurchaseFulfillmentOrderDraftStore>();
        services.AddScoped<IDomesticGroupPurchaseFulfillmentPlanService, DomesticGroupPurchaseFulfillmentPlanService>();
        services.AddScoped<IDomesticGroupPurchaseVehicleRecommendationService, DomesticGroupPurchaseVehicleRecommendationService>();
        services.AddSingleton<IDomesticGroupPurchaseNegotiationClock, SystemDomesticGroupPurchaseNegotiationClock>();
        services.AddSingleton<IDomesticGroupPurchaseNegotiationStore, MongoDomesticGroupPurchaseNegotiationStore>();
        services.AddScoped<IDomesticGroupPurchaseNegotiationService, DomesticGroupPurchaseNegotiationService>();
        services.AddSingleton<ICollectiveProcurementPlanningClock, SystemCollectiveProcurementPlanningClock>();
        services.AddSingleton<ICollectiveProcurementEconomicsEngine, CollectiveProcurementEconomicsEngine>();
        services.AddSingleton<ICollectiveProcurementPlanningStore, MongoCollectiveProcurementPlanningStore>();
        services.AddScoped<ICollectiveProcurementPlanningService, CollectiveProcurementPlanningService>();
        services.AddScoped<I공동수입준비원장Service, 공동수입준비원장Service>();
        services.AddScoped<I공동수입준비주문자조회UseCase, 공동수입준비주문자조회UseCase>();
        services.AddSingleton<I공동수입준비OS, 공동수입준비OS>();
        services.AddHostedService<공동수입준비OsWorker>();
        services.AddSingleton<I공동구매해외선적추적저장소, Mongo공동구매해외선적추적저장소>();
        services.AddSingleton<I공동구매커머스이행계획저장소, Mongo공동구매커머스이행계획저장소>();
        services.AddSingleton<I주문자집단운영주체저장소, Mongo주문자집단운영주체저장소>();
        services.AddSingleton<I판매페이지초안저장소, Mongo판매페이지초안저장소>();

        return services;
    }
}
