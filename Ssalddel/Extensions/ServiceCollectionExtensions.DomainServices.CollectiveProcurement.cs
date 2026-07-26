using Ssalddel.Services.CollectiveProcurement;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.Sales;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 공동구매 수요·모집 Process Manager와 그 전용 저장·판단 구성요소를 등록합니다.
    /// 정기 점검 BackgroundService는 포함하지 않으므로 API 서버에서 독립적으로 사용할 수 있습니다.
    /// </summary>
    public static IServiceCollection AddSsalddelGroupPurchaseDemandProcessModule(
        this IServiceCollection services)
    {
        services.AddSsalddelGroupPurchaseDemandLocalInfrastructure();
        services.TryAddSingleton<
            I공동구매수요모집ProcessManager,
            공동구매수요모집ProcessManager>();
        return services;
    }

    /// <summary>
    /// 공동구매 모집 마감과 장기 정체를 점검하는 BackgroundService를 선택적으로 등록합니다.
    /// 같은 저장소를 사용하는 여러 서버에서는 한 실행 주체에만 등록하거나 분산 lease를 함께 사용해야 합니다.
    /// </summary>
    public static IServiceCollection AddSsalddelGroupPurchaseDemandBackgroundProcessing(
        this IServiceCollection services)
    {
        services.AddSsalddelGroupPurchaseDemandProcessModule();
        services.AddHostedService<공동구매수요모집DeadlineScanBackgroundService>();
        return services;
    }

    /// <summary>
    /// 공동수입 준비 Process Manager를 등록합니다.
    /// 앞 단계 집단, Business Case와 공공데이터 배치는 이 모듈이 소유한 Port 구현을 서버가 별도로 제공해야 합니다.
    /// </summary>
    public static IServiceCollection AddSsalddelGroupImportReadinessProcessModule(
        this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<
            I공동수입준비ProcessManager,
            공동수입준비ProcessManager>();
        return services;
    }

    /// <summary>
    /// 단일 서버 구성에서 공동수입 준비 Port를 기존 Mongo·커뮤니티·배치 구현으로 연결합니다.
    /// 서버를 분리할 때는 이 메서드 대신 같은 Port의 HTTP 또는 메시지 Adapter를 등록합니다.
    /// </summary>
    public static IServiceCollection AddSsalddelGroupImportReadinessLocalAdapters(
        this IServiceCollection services)
    {
        services.AddSsalddelGroupPurchaseDemandLocalInfrastructure();
        services.TryAddSingleton<
            I공동수입준비SourceGroupReader,
            공동수입준비LocalSourceGroupReader>();
        services.TryAddSingleton<
            I공동수입준비BusinessCaseStore,
            공동수입준비LocalBusinessCaseStore>();
        services.TryAddSingleton<
            I공동수입준비EvidenceBatchReader,
            공동수입준비LocalEvidenceBatchReader>();
        return services;
    }

    /// <summary>
    /// 공동수입 준비 정기 점검 BackgroundService를 선택적으로 등록합니다.
    /// </summary>
    public static IServiceCollection AddSsalddelGroupImportReadinessBackgroundProcessing(
        this IServiceCollection services)
    {
        services.AddSsalddelGroupImportReadinessProcessModule();
        services.AddHostedService<공동수입준비정기점검BackgroundService>();
        return services;
    }

    private static IServiceCollection AddSsalddelGroupPurchaseDemandLocalInfrastructure(
        this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<
            I공동구매주문자집단화Engine,
            공동구매주문자집단화Engine>();
        services.TryAddSingleton<Mongo공동구매자동집단화저장소>();
        services.TryAddSingleton<I공동구매자동집단화저장소>(provider =>
            provider.GetRequiredService<Mongo공동구매자동집단화저장소>());
        services.TryAddSingleton<I공동구매수요모집ProcessStore>(provider =>
            provider.GetRequiredService<Mongo공동구매자동집단화저장소>());
        services.TryAddSingleton(공동구매수요모집BatchRegistrationPlan.빈계획());
        services.TryAddSingleton<
            I공동구매수요모집BatchCatalog,
            공동구매수요모집BatchCatalog>();
        return services;
    }

    private static IServiceCollection AddSsalddelCollectiveProcurementDomainServices(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<I주문자집단배송권조회Service, 주문자집단배송권조회Service>();
        services.AddSingleton<I주문자집단자동배정Service, 주문자집단자동배정Service>();
        services.AddSingleton<I공동구매물류워크플로우저장소, Mongo공동구매물류워크플로우저장소>();
        services.AddSingleton<I공동구매체험Service, 공동구매체험Service>();
        services.AddSsalddelGroupPurchaseDemandBackgroundProcessing();
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
        services.AddScoped<I주문방식비교UseCase, 주문방식비교UseCase>();
        services.AddScoped<I같이주문레시피활용UseCase, 같이주문레시피활용UseCase>();
        services.AddScoped<I공급자Membership혜택계산Service, 공급자Membership혜택계산Service>();
        services.AddSingleton<I공급자관심구독DraftStore, Mongo공급자관심구독DraftStore>();
        services.AddScoped<I공급자관심구독Service, 공급자관심구독Service>();
        services.AddSingleton<ICollectiveProcurementPlanningStore, MongoCollectiveProcurementPlanningStore>();
        services.AddScoped<ICollectiveProcurementPlanningService, CollectiveProcurementPlanningService>();
        services.AddScoped<I공동수입준비원장Service, 공동수입준비원장Service>();
        services.AddScoped<I공동수입준비주문자조회UseCase, 공동수입준비주문자조회UseCase>();
        services.AddSingleton<IIncoterms도움말조회UseCase, Incoterms도움말조회UseCase>();
        services.AddSsalddelGroupImportReadinessLocalAdapters();
        services.AddSsalddelGroupImportReadinessBackgroundProcessing();
        services.AddSingleton<I공동구매해외선적추적저장소, Mongo공동구매해외선적추적저장소>();
        services.AddSingleton<I공동구매커머스이행계획저장소, Mongo공동구매커머스이행계획저장소>();
        services.AddSingleton<I주문자집단운영주체저장소, Mongo주문자집단운영주체저장소>();
        services.AddSingleton<I판매페이지초안저장소, Mongo판매페이지초안저장소>();

        return services;
    }
}
