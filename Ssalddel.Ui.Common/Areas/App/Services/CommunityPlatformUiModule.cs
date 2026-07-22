using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Ui.Common.Areas.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ssalddel.Ui.Common.Areas.App.Services;

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Ui,
    SsalddelModuleKind.ClientComposition,
    "커뮤니티 홈의 공개 게시판·연결 도구 ViewModel과 browser state adapter를 조립",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.IndependentExecution,
    Boundary = "후속 업무 proxy의 DI 등록은 운영 노출을 뜻하지 않으며 메뉴와 API는 버전 기능 플래그를 따라야 합니다.")]
internal static class CommunityPlatformUiModule
{
    internal static IServiceCollection AddCommunityPlatformUiModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddCommunityWritingUiModule();
        services.TryAddScoped<PlatformCommunityService>();
        services.TryAddScoped<ICommunityParticipationClient>(provider =>
            provider.GetRequiredService<PlatformCommunityService>());
        services.TryAddScoped<ICommunityLedgerClient>(provider =>
            provider.GetRequiredService<PlatformCommunityService>());
        services.TryAddScoped<ICommunityProcurementClient>(provider =>
            provider.GetRequiredService<PlatformCommunityService>());
        services.TryAddScoped<ICommunityVoteClient>(provider =>
            provider.GetRequiredService<PlatformCommunityService>());
        services.TryAddScoped<IDiagramOrganizationDirectoryClient, DiagramOrganizationDirectoryClient>();
        services.TryAddScoped<YouTubeFoodCommunityDiscoveryService>();
        services.TryAddScoped<ICommunityDynamicDiscoveryClient, CommunityDynamicDiscoveryClient>();
        services.TryAddScoped<ICommunityDecorationSelectionStore, BrowserCommunityDecorationSelectionStore>();
        services.TryAddScoped<ICommunityDecorationPurchaseClient, LocalSimulationCommunityDecorationPurchaseClient>();
        services.TryAddTransient<CommunityScheduledPostListViewModel>();
        services.TryAddTransient<CommunityPostListPageViewModel>();
        services.TryAddTransient<PlatformCommunityHomeShellViewModel>();
        services.TryAddTransient<PlatformCommunityBoardWorkspaceViewModel>();
        services.TryAddTransient<CommunityPostJourneyCollectionViewModel>();
        services.TryAddTransient<PlatformCommunityPostEngagementViewModel>();
        services.TryAddTransient<PlatformCommunityLedgerPickerViewModel>();
        services.TryAddTransient<YouTubeFoodCommunityDiscoveryViewModel>();
        services.TryAddTransient<CommunityDynamicDiscoveryViewModel>();
        services.TryAddTransient<CommunityDynamicTopicDirectoryViewModel>();
        services.TryAddTransient<CommunityDynamicTopicFeedViewModel>();
        services.TryAddTransient<CommunityAuthoringDiagramViewModel>();
        services.TryAddTransient<CommunityAuthoringMutualBenefitViewModel>();
        services.TryAddTransient<CommunityAuthoringEvidenceChartViewModel>();
        services.TryAddTransient<CommunityOperatorWritingPersonaViewModel>();
        services.TryAddTransient<CommunityVowVersionViewModel>();
        services.TryAddTransient<CommunityVowJourneyTemplateViewModel>();
        services.TryAddTransient<PlatformCommunityDiagramChatViewModel>();
        services.TryAddTransient<PlatformCommunityDiagramCanvasViewModel>();
        services.TryAddTransient<PlatformCommunityDiagramWorkspaceViewModel>();
        services.TryAddTransient<PlatformCommunityWishFlowViewModel>();
        services.TryAddTransient<PlatformCommunityWarehouseProxyViewModel>();
        services.TryAddTransient<PlatformCommunityPublicBoardViewModel>();
        services.TryAddTransient<PlatformCommunityConnectedToolsViewModel>();
        services.TryAddTransient<PlatformCommunityHomePageViewModel>();
        services.TryAddTransient<ICommunityCollectiveActionSource, PlatformCommunityCollectiveActionSource>();
        services.TryAddTransient<CommunityActionJourneyNavigationViewModel>();
        services.TryAddTransient<CommunityActionCollectionViewModel>();
        services.TryAddTransient<CommunityActionConditionsViewModel>();
        services.TryAddTransient<CommunityActionPartyViewModel>();
        services.TryAddTransient<CommunityActionDeliveryViewModel>();
        services.TryAddTransient<CommunityActionTraditionalMarketImportedMeatViewModel>();
        services.TryAddTransient<CommunityActionMarketDayViewModel>();
        services.TryAddTransient<CommunityActionReadinessViewModel>();
        services.TryAddTransient<CommunityActionExecutionViewModel>();
        services.TryAddTransient<CommunityActionOutcomeViewModel>();
        services.TryAddTransient<CommunityCollectiveActionPageViewModel>();
        services.TryAddScoped<PlatformHomeModeStateService>();
        services.TryAddScoped<PlatformDiagramPaletteStateService>();
        services.TryAddTransient<IBagua업무영역ViewModelFactory, Bagua업무영역ViewModelFactory>();
        services.TryAddSingleton<IBaguaTargetWorkspaceResolver, DefaultBaguaTargetWorkspaceResolver>();
        services.TryAddTransient<BaguaRoleTransitionPageViewModel>();
        services.TryAddScoped<I농수산공공데이터Client, 농수산공공데이터Client>();
        services.TryAddScoped<IOfficialFoodIngredientDiscoveryClient, OfficialFoodIngredientDiscoveryClient>();
        services.TryAddTransient<국내농수산가격조회ViewModel>();
        services.TryAddTransient<미국농수산가격조회ViewModel>();
        services.TryAddTransient<호주농수산가격조회ViewModel>();
        services.TryAddTransient<농수산가격비교PageViewModel>();
        services.TryAddTransient<OfficialFoodIngredientJourneyViewModel>();
        services.TryAddScoped<CommunityLedgerNodeActionService>();
        services.TryAddScoped<ICommunityLedgerNodeActionService>(provider =>
            provider.GetRequiredService<CommunityLedgerNodeActionService>());
        services.TryAddTransient<CommunityLedgerNodeActionViewModel>();
        services.TryAddTransient<CommunityLedgerDiagramDetailViewModel>();
        services.TryAddScoped<YouTube관리콘텐츠Service>();
        services.TryAddScoped<PlatformCommunityDecorationStateService>();
        services.TryAddScoped<PlatformCommunityPostDraftStateService>();
        services.TryAddScoped<IDiagramCollaborationClientService>(_ => NoopDiagramCollaborationClientService.Instance);
        services.TryAddSingleton<ISsalddelIdentifierCodeGenerator, ZxingSsalddelIdentifierCodeGenerator>();

        return services;
    }
}
