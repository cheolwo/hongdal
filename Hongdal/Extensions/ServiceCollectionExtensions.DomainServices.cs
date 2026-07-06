using Hongdal.Application.Driver.Transport;
using 홍달.Services.Dispatch.Engine;
using 홍달.Services.Dispatch.Notification;
using 홍달.Services.Dispatch.Queue;
using 홍달.Services.Documents;
using 홍달.Services.External.Customs;
using 홍달.Services.External.PublicData;
using 홍달.Services.Images;
using 홍달.Services.Payments;
using 홍달.Services.Sales;

namespace Hongdal.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalDomainServices(this IServiceCollection services)
    {
        services.AddScoped<I결제Provider, Toss결제Provider>();
        services.AddScoped<I공통결제Service, 공통결제Service>();
        services.AddScoped<I콘텐츠혜택계산Service, 콘텐츠혜택계산Service>();
        services.AddScoped<I결제승인완료OutboxService, 결제승인완료OutboxService>();
        services.AddScoped<통관상태동기화Service>();
        services.AddSingleton<IPublicDataApiMetadataCatalog, PublicDataApiMetadataCatalog>();

        services.AddSingleton<IGoogleCloudStorageService, GoogleCloudStorageService>();
        services.AddSingleton<IDriverLocationStore, DriverLocationStore>();
        services.AddSingleton<IDriverWorkQueueStore, RedisDriverWorkQueueStore>();
        services.AddSingleton<IDriverRejectedRequestStore, RedisDriverRejectedRequestStore>();
        services.AddSingleton<IDriverPushTokenStore, RedisDriverPushTokenStore>();
        services.AddSingleton<IDriverRecommendationPushStateStore, RedisDriverRecommendationPushStateStore>();
        services.AddSingleton<IDriverCallScopeStore, RedisDriverCallScopeStore>();
        services.AddSingleton<IDriverNotificationSettingsStore, RedisDriverNotificationSettingsStore>();
        services.AddSingleton<ICommandFileStoragePathResolver, CommandFileStoragePathResolver>();
        services.AddSingleton<IDispatchRecommendationLogStore, DispatchRecommendationLogStore>();
        services.AddSingleton<IDispatchAcceptanceLogStore, DispatchAcceptanceLogStore>();
        services.AddScoped<I배차큐전환Service, 배차큐전환Service>();
        services.AddScoped<I배차추천알림Service, 배차추천알림Service>();
        services.AddSingleton<I탐색캠페인이벤트저장소, 탐색캠페인이벤트저장소>();
        services.AddSingleton<IAdminFilePodStore, AdminFilePodStore>();
        services.AddSingleton<I문서관리Store, 문서관리Store>();
        services.AddSingleton<I문서관리Service, 문서관리Service>();
        services.AddSingleton<이미지프롬프트생성기Resolver, 기본이미지프롬프트생성기Resolver>();
        services.AddScoped<I샘플이미지대상ResolverResolver, 샘플이미지대상ResolverResolver>();
        services.AddSingleton<I이미지프롬프트생성기, 화주상품사진프롬프트생성기>();
        services.AddSingleton<I이미지프롬프트생성기, 기사상차인증사진프롬프트생성기>();
        services.AddSingleton<I이미지프롬프트생성기, 기사배차완료인증사진프롬프트생성기>();
        services.AddSingleton<I이미지프롬프트생성기, 음식상품썸네일프롬프트생성기>();
        services.AddSingleton<I이미지프롬프트생성기, 주문후기사진프롬프트생성기>();
        services.AddSingleton<I이미지프롬프트생성기, 상품상세페이지이미지프롬프트생성기>();
        services.AddScoped<I샘플이미지대상Resolver, 판매상품샘플이미지대상Resolver>();
        services.AddScoped<I샘플이미지대상Resolver, 상품상세이미지생성작업대상Resolver>();
        services.AddScoped<I샘플이미지생성Service, 샘플이미지생성Service>();
        services.AddScoped<I배차추천경로Service, 배차추천경로Service>();
        services.AddScoped<I기사운송일정구성Service, 기사운송일정구성Service>();
        services.AddScoped<I운송일정삽입평가Service, 운송일정삽입평가Service>();
        services.AddScoped<I배차추천판정Service, 배차추천판정Service>();
        services.AddScoped<I배차추천평가Service, 배차추천평가Service>();
        services.AddScoped<I배차업무정책, 용달운송배차업무정책>();
        services.AddScoped<I배차업무정책, 음식배달배차업무정책>();
        services.AddScoped<I화물용달배차흐름Resolver, 화물용달배차흐름Resolver>();
        services.AddScoped<I음식배달배차흐름Resolver, 음식배달배차흐름Resolver>();
        services.AddScoped<I배차엔진, 화물용달배차엔진>();
        services.AddScoped<I배차엔진, 음식배달배차엔진>();
        services.AddScoped<I배차추천후보선정Service, 배차추천후보선정Service>();
        services.AddScoped<I공개배차Service, 공개배차Service>();
        services.AddScoped<홍달.Services.Dispatch.Recommendation.I차량화물적합성Service, 홍달.Services.Dispatch.Recommendation.차량화물적합성Service>();
        services.AddScoped<Hongdal.Application.Shipper.Request.I차량추천Service, Hongdal.Application.Shipper.Request.차량추천Service>();
        services.AddScoped<Hongdal.Application.Shipper.Request.I화주운송의뢰추천Service, Hongdal.Application.Shipper.Request.화주운송의뢰추천Service>();
        services.AddScoped<Hongdal.Application.Shipper.Request.I화주운송의뢰일괄등록파서Service, Hongdal.Application.Shipper.Request.화주운송의뢰일괄등록파서Service>();
        services.AddScoped<I판매상품샘플시드Service, 판매상품샘플시드Service>();
        services.AddScoped<I배차추천Service, 화물배차추천Service>();
        services.AddScoped<I음식배차추천Service, 음식배차추천Service>();
        services.AddScoped<I운행중배차추천Service, 운행중배차추천Service>();
        services.AddScoped<I비운행중배차추천Service, 비운행중배차추천Service>();
        services.AddScoped<I기사운송상태전이Service, 기사운송상태전이Service>();
        services.AddScoped<I탐색대상추천Service, 탐색대상추천Service>();
        services.AddScoped<I탐색캠페인상태전이Service, 탐색캠페인상태전이Service>();
        services.AddScoped<INationalDispatchRequestService, NationalDispatchRequestService>();
        services.AddScoped<I기사월정산Service, 기사월정산Service>();
        services.AddScoped<IPlatformProfitReturnService, PlatformProfitReturnService>();
        services.AddHostedService<KieAiTaskPollingWorker>();

        return services;
    }
}
