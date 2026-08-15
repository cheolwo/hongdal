using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
public sealed class SimulationWorld공간실행Snapshot
{
    public string BuildStableId { get; set; } = string.Empty;
    public string AreaSetStableId { get; set; } = string.Empty;
    public string OutputHashSha256 { get; set; } = string.Empty;
    public IReadOnlyList<SimulationWorld파생Node> Nodes { get; set; } =
        Array.Empty<SimulationWorld파생Node>();
    public int BuildingPlacementCount { get; set; }
    public int UnityArtifactCount { get; set; }
}

public interface ISimulationWorld공간실행Reader
{
    Task<SimulationWorld공간실행Snapshot?> 조회Async(
        string buildStableId,
        CancellationToken cancellationToken);
}

public sealed class SimulationWorldSynty경관저장결과
{
    public bool Inserted { get; set; }
    public string VisualBuildStableId { get; set; } = string.Empty;
    public string OutputHashSha256 { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public int GraphicsPlanCount { get; set; }
    public int VisualPlacementCount { get; set; }
    public int RejectionCount { get; set; }
}

public interface ISimulationWorldSynty경관Store
{
    Task<SimulationWorldSynty경관저장결과> 저장Async(
        SimulationWorldSynty경관실행원장 ledger,
        CancellationToken cancellationToken);
}

public sealed class SimulationWorldSynty경관계획결과
{
    public string StatusCode { get; set; } = string.Empty;
    public IReadOnlyList<SimulationWorld그래픽표현계획> GraphicsPlans { get; set; } =
        Array.Empty<SimulationWorld그래픽표현계획>();
    public IReadOnlyList<SimulationWorld시각배치계획> VisualPlacements { get; set; } =
        Array.Empty<SimulationWorld시각배치계획>();
    public IReadOnlyList<SimulationWorldSynty배치거부> Rejections { get; set; } =
        Array.Empty<SimulationWorldSynty배치거부>();
}

public interface ISimulationWorldSynty경관Planner
{
    SimulationWorldSynty경관계획결과 계획(
        SimulationWorldSynty경관Job요청 request,
        SimulationWorld공간실행Snapshot spatialBuild,
        IReadOnlyList<SimulationWorld파생Node> targetNodes);
}

public sealed class SimulationWorldSynty경관JobShell
{
    public const string SpatialBuildNotFoundCode = "SimulationWorldSpatialBuildNotFound";
    public const string SpatialOutputMismatchCode = "SimulationWorldSpatialOutputMismatch";
    public const string AreaSetMismatchCode = "SimulationWorldAreaSetMismatch";
    public const string ScopeNotFoundCode = "SimulationWorldSyntyScopeNotFound";
    public const string PlannerTargetMismatchCode = "SimulationWorldSyntyPlannerTargetMismatch";

    private readonly ISimulationWorld공간실행Reader _spatialReader;
    private readonly ISimulationWorldSynty경관Planner _planner;
    private readonly ISimulationWorldSynty경관Store _store;

    public SimulationWorldSynty경관JobShell(
        ISimulationWorld공간실행Reader spatialReader,
        ISimulationWorldSynty경관Planner planner,
        ISimulationWorldSynty경관Store store)
    {
        _spatialReader = spatialReader;
        _planner = planner;
        _store = store;
    }

    public async Task<SimulationWorldSynty경관저장결과> 실행Async(
        SimulationWorldSynty경관Job요청 request,
        CancellationToken cancellationToken)
    {
        SimulationWorldSynty경관Validator.ValidateRequest(request);
        var spatialBuild = await _spatialReader.조회Async(
            request.SpatialBuildStableId,
            cancellationToken);
        if (spatialBuild == null)
            throw new InvalidOperationException(SpatialBuildNotFoundCode);
        if (!string.Equals(
                spatialBuild.OutputHashSha256,
                request.SpatialOutputHashSha256,
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(SpatialOutputMismatchCode);
        if (!string.Equals(
                spatialBuild.AreaSetStableId,
                request.AreaSetStableId,
                StringComparison.Ordinal))
            throw new InvalidOperationException(AreaSetMismatchCode);

        var targetNodes = ResolveTargets(request, spatialBuild);
        if (targetNodes.Count == 0)
            throw new InvalidOperationException(ScopeNotFoundCode);

        var plan = _planner.계획(request, spatialBuild, targetNodes);
        EnsurePlannerTargets(targetNodes, plan);
        var inputFingerprint = SimulationWorldSynty경관Hash.ComputeInputFingerprint(request);
        var ledger = new SimulationWorldSynty경관실행원장
        {
            VisualBuildStableId = "synty-visual-build:" + inputFingerprint.Substring(0, 24),
            JobStableId = request.JobStableId,
            SpatialBuildStableId = request.SpatialBuildStableId,
            SpatialOutputHashSha256 = request.SpatialOutputHashSha256,
            AreaSetStableId = request.AreaSetStableId,
            ScopeKindCode = request.ScopeKindCode,
            ScopeStableId = request.ScopeStableId,
            LandscapeRuleRevision = request.LandscapeRuleRevision,
            VisualCatalogRevision = request.VisualCatalogRevision,
            UrpProfileCatalogRevision = request.UrpProfileCatalogRevision,
            Seed = request.Seed,
            TargetPlatformCode = request.TargetPlatformCode,
            QualityTierCode = request.QualityTierCode,
            InputFingerprintSha256 = inputFingerprint,
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            StatusCode = plan.StatusCode,
            GraphicsPlans = plan.GraphicsPlans,
            VisualPlacements = plan.VisualPlacements,
            Rejections = plan.Rejections,
        };
        return await _store.저장Async(ledger, cancellationToken);
    }

    private static IReadOnlyList<SimulationWorld파생Node> ResolveTargets(
        SimulationWorldSynty경관Job요청 request,
        SimulationWorld공간실행Snapshot spatialBuild)
    {
        if (request.ScopeKindCode == SimulationWorldSynty범위Codes.영역묶음)
        {
            if (!string.Equals(request.ScopeStableId, spatialBuild.AreaSetStableId, StringComparison.Ordinal))
                return Array.Empty<SimulationWorld파생Node>();
            return spatialBuild.Nodes
                .Where(item => item.NodeKindCode == "Area")
                .OrderBy(item => item.StableId, StringComparer.Ordinal)
                .ToArray();
        }
        if (request.ScopeKindCode == SimulationWorldSynty범위Codes.영역)
            return spatialBuild.Nodes
                .Where(item => item.NodeKindCode == "Area"
                    && item.StableId == request.ScopeStableId)
                .ToArray();
        return spatialBuild.Nodes
            .Where(item => item.TileKey == request.ScopeStableId)
            .OrderBy(item => item.StableId, StringComparer.Ordinal)
            .ToArray();
    }

    private static void EnsurePlannerTargets(
        IReadOnlyList<SimulationWorld파생Node> targetNodes,
        SimulationWorldSynty경관계획결과 plan)
    {
        var targetIds = new HashSet<string>(
            targetNodes.Select(item => item.StableId),
            StringComparer.Ordinal);
        if (plan.GraphicsPlans.Any(item => !targetIds.Contains(item.TargetNodeStableId))
            || plan.VisualPlacements.Any(item => !targetIds.Contains(item.TargetNodeStableId))
            || plan.Rejections.Any(item => item.TargetNodeStableId != null
                && !targetIds.Contains(item.TargetNodeStableId)))
            throw new InvalidOperationException(PlannerTargetMismatchCode);
    }
}

public sealed class SimulationWorld기본Synty경관Planner : ISimulationWorldSynty경관Planner
{
    public SimulationWorldSynty경관계획결과 계획(
        SimulationWorldSynty경관Job요청 request,
        SimulationWorld공간실행Snapshot spatialBuild,
        IReadOnlyList<SimulationWorld파생Node> targetNodes)
    {
        var graphicsPlans = targetNodes
            .Where(item => item.NodeKindCode == "Area")
            .Select(item => Graphics(request, item))
            .ToArray();
        var rejections = targetNodes
            .Where(item => item.NodeKindCode == "Area")
            .Select(item => new SimulationWorldSynty배치거부
            {
                StableId = "synty-rejection:placement-anchor-pending:" + StableSuffix(item.StableId),
                TargetNodeStableId = item.StableId,
                ReasonCode = spatialBuild.UnityArtifactCount == 0
                    ? "UnitySpatialArtifactMissing"
                    : "UnityAssetBindingWorkerPending",
                Detail = spatialBuild.UnityArtifactCount == 0
                    ? "공간 실행에 Terrain·Mask·배치 기준점 산출물이 없어 VisualKey 위치 배치를 보류했습니다."
                    : "공간 산출물은 준비되었으나 Unity BatchMode 자산 연결 작업자가 아직 결과를 제출하지 않았습니다.",
            })
            .ToArray();
        return new SimulationWorldSynty경관계획결과
        {
            StatusCode = graphicsPlans.Length == 0
                ? SimulationWorldSynty작업상태Codes.자료부족
                : SimulationWorldSynty작업상태Codes.일부완료,
            GraphicsPlans = graphicsPlans,
            VisualPlacements = Array.Empty<SimulationWorld시각배치계획>(),
            Rejections = rejections,
        };
    }

    private static SimulationWorld그래픽표현계획 Graphics(
        SimulationWorldSynty경관Job요청 request,
        SimulationWorld파생Node area)
    {
        var profile = ResolveProfile(area.RegionCode);
        return new SimulationWorld그래픽표현계획
        {
            StableId = "synty-graphics:" + profile.Role + ":" + StableSuffix(area.StableId) + ":v2",
            TargetNodeStableId = area.StableId,
            PresentationScopeCode = "AreaEnvironment",
            TextureSetKey = "texture." + profile.Role + ".ground.v2",
            MaterialVariantKey = "material." + profile.Role + ".regional.v2",
            ColorPaletteKey = profile.Palette,
            BackgroundProfileKey = profile.Background,
            LightingProfileKey = "lighting.pyeongchang.shared-day.v2",
            TimeOfDayProfileKey = "timeofday.shared-day.v2",
            ShadowPolicyCode = SimulationWorld그림자정책Codes.혼합,
            CastShadows = true,
            ReceiveShadows = true,
            ContactShadowStrength = profile.ContactShadowStrength,
            ShadowDistanceMeters = request.TargetPlatformCode == SimulationWorldSynty대상플랫폼Codes.Mobile
                ? 40m
                : 50m,
            AmbientOcclusionStrength = request.TargetPlatformCode == SimulationWorldSynty대상플랫폼Codes.Mobile
                ? 0m
                : profile.AmbientOcclusionStrength,
            LodCode = "L1",
            QualityTierCode = request.QualityTierCode,
            PresentationOnly = true,
        };
    }

    private static SyntyAreaProfile ResolveProfile(string? regionCode)
    {
        if (regionCode == "5176038000")
            return new SyntyAreaProfile(
                "farm", "palette.farm.nature-warm-earth-green.v2",
                "background.daegwallyeong.nature-forest-ridge.v2", 0.45m, 0.25m);
        if (regionCode == "5176036000")
            return new SyntyAreaProfile(
                "hub", "palette.hub.nature-concrete-orange.v2",
                "background.jinbu.nature-forest-buffer.v2", 0.6m, 0.4m);
        if (regionCode == "5176025000")
            return new SyntyAreaProfile(
                "town", "palette.town.nature-cream-brick.v2",
                "background.pyeongchang.nature-low-town.v2", 0.55m, 0.35m);
        return new SyntyAreaProfile(
            "generic", "palette.rural.nature-neutral.v2",
            "background.rural.nature-generic.v2", 0.5m, 0.3m);
    }

    private static string StableSuffix(string stableId) =>
        stableId.Replace(':', '-').Replace('/', '-').Replace('\\', '-');

    private sealed class SyntyAreaProfile
    {
        public SyntyAreaProfile(
            string role,
            string palette,
            string background,
            decimal contactShadowStrength,
            decimal ambientOcclusionStrength)
        {
            Role = role;
            Palette = palette;
            Background = background;
            ContactShadowStrength = contactShadowStrength;
            AmbientOcclusionStrength = ambientOcclusionStrength;
        }

        public string Role { get; }
        public string Palette { get; }
        public string Background { get; }
        public decimal ContactShadowStrength { get; }
        public decimal AmbientOcclusionStrength { get; }
    }
}
}
