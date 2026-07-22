using Quartz;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Infrastructure.BackgroundJobs.AgriculturalFisheries;
using Ssalddel.Infrastructure.BackgroundJobs.Community;
using Ssalddel.Infrastructure.BackgroundJobs.Content;
using Ssalddel.Infrastructure.BackgroundJobs.SalesOrders;
using Ssalddel.Services.LogisticsProcessing.SalesOrders;
using Ssalddel.Services.Orderer;
using 살뜰.Infrastructure.BackgroundJobs.Customs;
using 살뜰.Infrastructure.BackgroundJobs.DispatchQueue;
using 살뜰.Infrastructure.BackgroundJobs.Notifications;
using 살뜰.Infrastructure.BackgroundJobs.Payments;
using 살뜰.Services.Options;

namespace Ssalddel.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddSsalddelBackgroundJobs(
        this IServiceCollection services,
        배차큐배치작업Options jobOptions,
        SalesChannelOrderSyncOptions salesOrderSyncOptions,
        YouTubeOptions youTubeOptions,
        HongikHakdangCardOptions hongikHakdangCardOptions,
        AgriculturalFisheriesBatchOptions agriculturalFisheriesBatchOptions,
        CommunityEditorialBatchOptions communityEditorialBatchOptions,
        SsalddelExecutionOptions executionOptions)
    {
        var 공동구매Os배치등록계획 = 공동구매수요모집Os배치등록계획.생성(
            agriculturalFisheriesBatchOptions,
            communityEditorialBatchOptions);
        services.AddSingleton(공동구매Os배치등록계획);
        services.AddScoped<AgriculturalFisheriesBatchRunner>();
        services.AddScoped<OfficialFoodIngredientCompanyBatchRunner>();
        services.AddScoped<CommunityEditorialBatchRunner>();
        services.AddScoped<AgriculturalFisheriesCommunityPipelineRunner>();
        services.AddQuartz(q =>
        {
            if (executionOptions.Mode == SsalddelExecutionMode.Operational)
            {
                var scanJobKey = new JobKey("DispatchQueueScan");
                q.AddJob<배차큐스캔Job>(opts => opts.WithIdentity(scanJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(scanJobKey)
                    .WithIdentity("DispatchQueueScan-trigger")
                    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(Math.Max(5, jobOptions.큐스캔주기초))).RepeatForever()));

                var expireJobKey = new JobKey("DispatchRecommendationExpire");
                q.AddJob<추천만료정리Job>(opts => opts.WithIdentity(expireJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(expireJobKey)
                    .WithIdentity("DispatchRecommendationExpire-trigger")
                    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(Math.Max(5, jobOptions.추천만료정리주기초))).RepeatForever()));

                var pushJobKey = new JobKey("DispatchRecommendationPush");
                q.AddJob<배차추천알림발송Job>(opts => opts.WithIdentity(pushJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(pushJobKey)
                    .WithIdentity("DispatchRecommendationPush-trigger")
                    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(Math.Max(5, jobOptions.알림발송주기초))).RepeatForever()));

                var commandNotificationJobKey = new JobKey("CommandNotificationOutboxSend");
                q.AddJob<Command알림Outbox발송Job>(opts => opts.WithIdentity(commandNotificationJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(commandNotificationJobKey)
                    .WithIdentity("CommandNotificationOutboxSend-trigger")
                    .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(Math.Max(5, jobOptions.알림발송주기초))).RepeatForever()));
            }

            var paymentOutboxJobKey = new JobKey("PaymentApprovedOutboxPublish");
            q.AddJob<결제승인완료Outbox발행Job>(opts => opts.WithIdentity(paymentOutboxJobKey));
            q.AddTrigger(opts => opts
                .ForJob(paymentOutboxJobKey)
                .WithIdentity("PaymentApprovedOutboxPublish-trigger")
                .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(Math.Max(5, jobOptions.결제승인Outbox발행주기초))).RepeatForever()));

            var customsSyncJobKey = new JobKey("CustomsStatusSync");
            q.AddJob<통관상태동기화Job>(opts => opts.WithIdentity(customsSyncJobKey));
            q.AddTrigger(opts => opts
                .ForJob(customsSyncJobKey)
                .WithIdentity("CustomsStatusSync-trigger")
                .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(Math.Max(30, jobOptions.통관상태동기화주기초))).RepeatForever()));

            var domesticSalesOrderSyncJobKey = new JobKey("DomesticSalesChannelOrderSync");
            q.AddJob<DomesticSalesChannelOrderSyncJob>(opts => opts.WithIdentity(domesticSalesOrderSyncJobKey));
            q.AddTrigger(opts => opts
                .ForJob(domesticSalesOrderSyncJobKey)
                .WithIdentity("DomesticSalesChannelOrderSync-trigger")
                .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(Math.Max(60, salesOrderSyncOptions.DomesticSyncIntervalSeconds))).RepeatForever()));

            var overseasSalesOrderSyncJobKey = new JobKey("OverseasSalesChannelOrderSync");
            q.AddJob<OverseasSalesChannelOrderSyncJob>(opts => opts.WithIdentity(overseasSalesOrderSyncJobKey));
            q.AddTrigger(opts => opts
                .ForJob(overseasSalesOrderSyncJobKey)
                .WithIdentity("OverseasSalesChannelOrderSync-trigger")
                .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromSeconds(Math.Max(60, salesOrderSyncOptions.OverseasSyncIntervalSeconds))).RepeatForever()));

            if (youTubeOptions.Enabled)
            {
                var youTubeSyncJobKey = new JobKey("YouTubeChannelSync");
                q.AddJob<YouTube채널동기화Job>(opts => opts.WithIdentity(youTubeSyncJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(youTubeSyncJobKey)
                    .WithIdentity("YouTubeChannelSync-trigger")
                    .WithSimpleSchedule(x => x
                        .WithInterval(TimeSpan.FromSeconds(Math.Max(60, youTubeOptions.SyncIntervalSeconds)))
                        .RepeatForever()));
            }

            if (hongikHakdangCardOptions.Enabled)
            {
                var cardSyncJobKey = new JobKey("HongikHakdangCardSync");
                q.AddJob<HongikHakdangCardSyncJob>(opts => opts.WithIdentity(cardSyncJobKey));
                q.AddTrigger(opts => opts
                    .ForJob(cardSyncJobKey)
                    .WithIdentity("HongikHakdangCardSync-trigger")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithInterval(TimeSpan.FromHours(Math.Max(1, hongikHakdangCardOptions.SyncIntervalHours)))
                        .RepeatForever()));

                if (hongikHakdangCardOptions.DeliveryEnabled)
                {
                    var cardDeliveryJobKey = new JobKey("HongikHakdangCardDelivery");
                    q.AddJob<HongikHakdangCardDeliveryJob>(opts => opts.WithIdentity(cardDeliveryJobKey));
                    q.AddTrigger(opts => opts
                        .ForJob(cardDeliveryJobKey)
                        .WithIdentity("HongikHakdangCardDelivery-trigger")
                        .StartNow()
                        .WithSimpleSchedule(x => x
                            .WithInterval(TimeSpan.FromSeconds(Math.Max(
                                30,
                                hongikHakdangCardOptions.DeliveryPollingSeconds)))
                            .RepeatForever()));
                }
            }

            if (agriculturalFisheriesBatchOptions.Enabled)
            {
                AddAgriculturalFisheriesBatchJobs(
                    q,
                    agriculturalFisheriesBatchOptions,
                    공동구매Os배치등록계획);
            }

            if (communityEditorialBatchOptions.Enabled)
            {
                AddCommunityEditorialBatchJobs(
                    q,
                    communityEditorialBatchOptions,
                    공동구매Os배치등록계획);
            }
        });

        services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });
        return services;
    }

    private static void AddAgriculturalFisheriesBatchJobs(
        IServiceCollectionQuartzConfigurator quartz,
        AgriculturalFisheriesBatchOptions options,
        공동구매수요모집Os배치등록계획 등록계획)
    {
        var timeZone = AgriculturalFisheriesBatchSchedule.ResolveTimeZone(options.TimeZoneId);

        if (등록계획.Quartz등록여부(
                공동구매수요모집Os배치작업코드.Kamis일별가격수집))
        {
            var jobKey = new JobKey("KamisDailyPriceCollection");
            quartz.AddJob<KamisDailyPriceCollectionJob>(job => job.WithIdentity(jobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("KamisDailyPriceCollection-trigger")
                .WithCronSchedule(
                    options.KamisDailyCronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionDoNothing()));
        }

        if (등록계획.Quartz등록여부(
                공동구매수요모집Os배치작업코드.Kamis월별가격이력수집))
        {
            var jobKey = new JobKey("KamisMonthlyPriceCollection");
            quartz.AddJob<KamisMonthlyPriceCollectionJob>(job => job.WithIdentity(jobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("KamisMonthlyPriceCollection-trigger")
                .WithCronSchedule(
                    options.KamisMonthlyCronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionDoNothing()));
        }

        if (등록계획.Quartz등록여부(
                공동구매수요모집Os배치작업코드.UsdaNass월별가격수집))
        {
            var jobKey = new JobKey("UsdaMonthlyPriceCollection");
            quartz.AddJob<UsdaMonthlyPriceCollectionJob>(job => job.WithIdentity(jobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("UsdaMonthlyPriceCollection-trigger")
                .WithCronSchedule(
                    options.UsdaMonthlyCronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionDoNothing()));
        }

        if (등록계획.Quartz등록여부(
                공동구매수요모집Os배치작업코드.공식재료기업근거수집))
        {
            var jobKey = new JobKey("OfficialFoodIngredientCompanyResearch");
            quartz.AddJob<OfficialFoodIngredientCompanyResearchJob>(job =>
                job.WithIdentity(jobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("OfficialFoodIngredientCompanyResearch-trigger")
                .WithCronSchedule(
                    options.IngredientCompanyResearchCronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionDoNothing()));
        }
    }

    private static void AddCommunityEditorialBatchJobs(
        IServiceCollectionQuartzConfigurator quartz,
        CommunityEditorialBatchOptions options,
        공동구매수요모집Os배치등록계획 등록계획)
    {
        var timeZone = AgriculturalFisheriesBatchSchedule.ResolveTimeZone(options.TimeZoneId);

        if (등록계획.Quartz등록여부(
                공동구매수요모집Os배치작업코드.Kamis가격브리프게시))
        {
            var jobKey = new JobKey("CommunityKamisPriceBrief");
            quartz.AddJob<CommunityKamisPriceBriefJob>(job => job.WithIdentity(jobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("CommunityKamisPriceBrief-trigger")
                .WithCronSchedule(
                    options.KamisPriceBriefCronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionDoNothing()));
        }

        if (등록계획.Quartz등록여부(
                공동구매수요모집Os배치작업코드.UsdaNass가격브리프게시))
        {
            var jobKey = new JobKey("CommunityUsdaNassPriceBrief");
            quartz.AddJob<CommunityUsdaNassPriceBriefJob>(job => job.WithIdentity(jobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("CommunityUsdaNassPriceBrief-trigger")
                .WithCronSchedule(
                    options.UsdaNassPriceBriefCronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionDoNothing()));
        }

        if (options.ReflectionEnabled)
        {
            var jobKey = new JobKey("CommunityReflection");
            quartz.AddJob<CommunityReflectionJob>(job => job.WithIdentity(jobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("CommunityReflection-trigger")
                .WithCronSchedule(
                    options.ReflectionCronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionDoNothing()));
        }

        if (options.ActivityDigestEnabled)
        {
            var jobKey = new JobKey("CommunityActivityDigest");
            quartz.AddJob<CommunityActivityDigestJob>(job => job.WithIdentity(jobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("CommunityActivityDigest-trigger")
                .WithCronSchedule(
                    options.ActivityDigestCronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionDoNothing()));
        }

        if (options.PrajnaPublicationEnabled)
        {
            var jobKey = new JobKey("CommunityPrajnaPublication");
            quartz.AddJob<CommunityPrajnaPublicationJob>(job => job.WithIdentity(jobKey));
            quartz.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("CommunityPrajnaPublication-trigger")
                .WithCronSchedule(
                    options.PrajnaPublicationCronExpression,
                    schedule => schedule
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionDoNothing()));
        }
    }
}
