using Hongdal.Ui.Common.Areas.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hongdal.Ui.Common.Areas.App.Services;

internal static class CommunityPlatformUiModule
{
    internal static IServiceCollection AddCommunityPlatformUiModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<PlatformCommunityService>();
        services.TryAddScoped<YouTubeFoodCommunityDiscoveryService>();
        services.TryAddScoped<ICommunityDynamicDiscoveryClient, CommunityDynamicDiscoveryClient>();
        services.TryAddScoped<ICommunityPostComposerDraftStore, BrowserCommunityPostComposerDraftStore>();
        services.TryAddTransient<CommunityPostComposerViewModel>();
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
        services.TryAddTransient<PlatformCommunityDiagramChatViewModel>();
        services.TryAddTransient<PlatformCommunityDiagramCanvasViewModel>();
        services.TryAddTransient<PlatformCommunityDiagramWorkspaceViewModel>();
        services.TryAddTransient<PlatformCommunityWarehouseProxyViewModel>();
        services.TryAddTransient<PlatformCommunityHomePageViewModel>();
        services.TryAddTransient<ICommunityCollectiveActionSource, PlatformCommunityCollectiveActionSource>();
        services.TryAddTransient<CommunityActionJourneyNavigationViewModel>();
        services.TryAddTransient<CommunityActionCollectionViewModel>();
        services.TryAddTransient<CommunityActionConditionsViewModel>();
        services.TryAddTransient<CommunityActionPartyViewModel>();
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
        services.TryAddScoped<CommunityLedgerNodeActionService>();
        services.TryAddScoped<YouTube관리콘텐츠Service>();
        services.TryAddScoped<PlatformCommunityDecorationStateService>();
        services.TryAddScoped<PlatformCommunityPostDraftStateService>();
        services.TryAddScoped<IDiagramCollaborationClientService>(_ => NoopDiagramCollaborationClientService.Instance);
        services.TryAddSingleton<IHongdalIdentifierCodeGenerator, ZxingHongdalIdentifierCodeGenerator>();

        return services;
    }
}
