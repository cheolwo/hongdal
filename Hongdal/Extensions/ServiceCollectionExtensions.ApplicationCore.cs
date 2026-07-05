using FluentValidation;
using Hongdal.Application.Behaviors;
using Hongdal.Application.CommandProcessing;
using Hongdal.Middleware;
using Hongdal.Application.HumanResources;
using Hongdal.Services.HumanResources;
using Hongdal.Services.LogisticsProcessing.SalesOrders;
using 홍달.Infrastructure;
using 홍달.Services.Audit;
using 홍달.Services.Sales;
using 홍달.Services.ViewSettings;
using Hongdal.Services.LogisticsProcessing.Warehouse;
using Hongdal.Application.Driver.Transport;

namespace Hongdal.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHongdalApplicationCore(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Command후처리Behavior<,>));

        services.AddHongdalInfrastructure();
        services.AddScoped<ICurrentUserAccessor, HttpContextCurrentUserAccessor>();
        services.AddScoped<I참여자실행권한검사, 참여자실행권한검사>();
        services.AddScoped<I기사운송상태변경CommandExecutor, 기사운송상태변경CommandExecutor>();
        services.AddScoped<ICommand기능설정Resolver, Command기능설정Resolver>();
        services.AddScoped<ICommand기능CatalogResolver, Command기능CatalogResolver>();
        services.AddScoped<IWorkRelationshipSnapshotCollector, WorkRelationshipSnapshotCollector>();
        services.AddScoped<ICommand후처리Processor, Command감사로그Processor>();
        services.AddScoped<ICommand후처리Processor, Command알림의도Processor>();
        services.AddScoped<ICommand후처리Processor, Command인연스냅샷Processor>();

        services.AddScoped<I사용자행위로그Service, 사용자행위로그Service>();
        services.AddScoped<ISalesChannelService, SalesChannelService>();
        services.AddScoped<IView가시성Service, View가시성Service>();
        services.AddScoped<IWarehouseOperationService, WarehouseOperationService>();
        services.AddScoped<IWorkRelationshipSnapshotService, WorkRelationshipSnapshotService>();
        services.AddScoped<ISalesChannelOrderSyncService, SalesChannelOrderSyncService>();
        services.AddSingleton<ISalesChannelOrderFeedClient, EmptySalesChannelOrderFeedClient>();
        services.AddScoped<IHrRoleAssignmentStore, EfCoreHrRoleAssignmentStore>();
        services.AddScoped<IHrEmploymentContractService, HrEmploymentContractService>();
        services.AddSingleton<IHrParticipationBenefitRecordService, InMemoryHrParticipationBenefitRecordService>();
        services.AddScoped<HrRoleAccessMiddleware>();
        services.AddScoped<사용자행위로그Middleware>();

        return services;
    }
}
