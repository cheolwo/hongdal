using MudBlazor;
using Ssalddel.Contracts.Common.Versioning;
using Ssalddel.WebApp.Models;

namespace Ssalddel.WebApp.Services;

public static class IntegratedBetaCatalog
{
    private static readonly IntegratedBetaPageState Fallback = new(
        IntegratedBetaStage.Preparing,
        WebInteractionBoundary.Simulation,
        false,
        "아직 통합 베타 상태가 지정되지 않은 화면입니다.",
        IsCataloged: false);

    public static IReadOnlyList<IntegratedBetaStage> StageOrder { get; } =
    [
        IntegratedBetaStage.Live,
        IntegratedBetaStage.Beta,
        IntegratedBetaStage.Experience,
        IntegratedBetaStage.Preparing
    ];

    public static IntegratedBetaPageState Resolve(string? href)
        => TryResolve(href, out var state) ? state : Fallback;

    public static bool TryResolve(string? href, out IntegratedBetaPageState state)
    {
        if (!SsalddelPageCapabilityCatalog.TryResolve(
                SsalddelPageAppCodes.IntegratedWeb,
                href,
                out var capability))
        {
            state = Fallback;
            return false;
        }

        state = new IntegratedBetaPageState(
            ToWebStage(capability.Stage),
            ToWebBoundary(capability.Boundary),
            capability.RequiresAuthentication,
            capability.Notice);
        return true;
    }

    public static Color StageColor(IntegratedBetaStage stage)
        => stage switch
        {
            IntegratedBetaStage.Live => Color.Success,
            IntegratedBetaStage.Beta => Color.Primary,
            IntegratedBetaStage.Experience => Color.Info,
            IntegratedBetaStage.Preparing => Color.Warning,
            _ => Color.Default
        };

    public static string StageIcon(IntegratedBetaStage stage)
        => stage switch
        {
            IntegratedBetaStage.Live => Icons.Material.Filled.CheckCircle,
            IntegratedBetaStage.Beta => Icons.Material.Filled.Science,
            IntegratedBetaStage.Experience => Icons.Material.Filled.Visibility,
            IntegratedBetaStage.Preparing => Icons.Material.Filled.Construction,
            _ => Icons.Material.Filled.HelpOutline
        };

    public static string StageCssClass(IntegratedBetaStage stage)
        => $"integrated-stage--{stage.ToString().ToLowerInvariant()}";

    private static IntegratedBetaStage ToWebStage(PageCapabilityStage stage)
        => stage switch
        {
            PageCapabilityStage.Live => IntegratedBetaStage.Live,
            PageCapabilityStage.Beta => IntegratedBetaStage.Beta,
            PageCapabilityStage.Experience => IntegratedBetaStage.Experience,
            PageCapabilityStage.Preparing => IntegratedBetaStage.Preparing,
            _ => IntegratedBetaStage.Preparing
        };

    private static WebInteractionBoundary ToWebBoundary(PageInteractionBoundary boundary)
        => boundary switch
        {
            PageInteractionBoundary.ReadOnly => WebInteractionBoundary.ReadOnly,
            PageInteractionBoundary.PlatformPersistence => WebInteractionBoundary.PlatformPersistence,
            PageInteractionBoundary.Simulation => WebInteractionBoundary.Simulation,
            _ => WebInteractionBoundary.Simulation
        };
}
