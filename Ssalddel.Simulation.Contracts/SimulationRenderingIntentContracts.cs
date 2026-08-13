using System;

namespace Ssalddel.Simulation.Contracts
{
    public static class Simulation렌더링ChannelCodes
    {
        public const string Environment = "Environment";
        public const string Surface = "Surface";
        public const string Lighting = "Lighting";
        public const string ObjectState = "ObjectState";
        public const string Attention = "Attention";
        public const string Fx = "Fx";
        public const string Animation = "Animation";
    }

    public static class Simulation렌더링범위Codes
    {
        public const string World = "World";
        public const string AreaSet = "AreaSet";
        public const string Area = "Area";
        public const string Tile = "Tile";
        public const string Route = "Route";
        public const string Facility = "Facility";
        public const string Object = "Object";
    }

    public static class Simulation렌더링수명Codes
    {
        public const string 상태일치동안 = "WhileStateMatches";
        public const string 개정까지 = "UntilRevision";
        public const string 기간 = "Duration";
        public const string 일회 = "OneShot";
        public const string 선택해제까지 = "UntilDeselected";
    }

    public static class Simulation렌더링의도Codes
    {
        public const string 화물운송중 = "CargoInTransit";
        public const string 경로운송흐름활성 = "RouteFlowActive";
        public const string 차량이동활성 = "VehicleMovementActive";
        public const string 흙길먼지후보 = "DirtRoadDustCandidate";
    }

    public static class Simulation렌더링AdapterCodes
    {
        public const string UrpMaterialPropertyBlock = "UrpMaterialPropertyBlock";
        public const string UrpVolume = "UrpVolume";
        public const string UrpRendererFeature = "UrpRendererFeature";
        public const string Particle = "Particle";
        public const string Animation = "Animation";
    }

    public static class Simulation렌더링지시Codes
    {
        public const string 차량경로이동 = "VehicleRouteMovement";
        public const string 경로발광강조 = "RouteEmissionHighlight";
        public const string 차량흙길먼지 = "VehicleDirtRoadDust";
    }

    public static class Simulation렌더링FallbackCodes
    {
        public const string 필요없음 = "None";
        public const string Particle미지원으로생략 = "ParticleUnsupportedOmitted";
        public const string 흙길근거없어생략 = "DirtSurfaceEvidenceMissingOmitted";
        public const string DepthTexture미지원단순강조 = "DepthTextureUnsupportedSimpleHighlight";
    }

    public static class Simulation공간표면Codes
    {
        public const string 흙길 = "DirtRoad";
        public const string 자갈길 = "GravelRoad";
        public const string 포장도로 = "PavedRoad";
        public const string 미확인 = "Unresolved";
    }

    public sealed class Simulation렌더링의도
    {
        public string IntentStableId { get; set; } = string.Empty;
        public string SourceStateStableId { get; set; } = string.Empty;
        public long SourceStateRevision { get; set; }
        public long SessionRevision { get; set; }
        public string IntentCode { get; set; } = string.Empty;
        public string ChannelCode { get; set; } = string.Empty;
        public string ScopeCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string? ContextStableId { get; set; }
        public int Priority { get; set; }
        public string LifetimeCode { get; set; } = string.Empty;
        public long? UntilRevision { get; set; }
        public int? ExpiresAtWorldTick { get; set; }
        public int? OccurrenceSequence { get; set; }
        public string EvidenceKindCode { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class Simulation렌더CapabilityProfile
    {
        public string ProfileStableId { get; set; } = string.Empty;
        public string ProfileRevision { get; set; } = string.Empty;
        public string TargetPlatformCode { get; set; } = string.Empty;
        public string QualityTierCode { get; set; } = string.Empty;
        public bool SupportsForwardPlus { get; set; }
        public bool SupportsDepthTexture { get; set; }
        public bool SupportsOpaqueTexture { get; set; }
        public bool SupportsSsao { get; set; }
        public bool SupportsDecal { get; set; }
        public bool SupportsGpuInstancing { get; set; }
        public bool SupportsParticle { get; set; }
        public int MaximumShadowedAdditionalLights { get; set; }
        public int ParticleBudget { get; set; }
        public int ShadowCasterBudget { get; set; }
    }

    public sealed class SimulationRoute렌더링Context
    {
        public string RouteStableId { get; set; } = string.Empty;
        public string SurfaceCode { get; set; } = Simulation공간표면Codes.미확인;
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string SpatialBuildStableId { get; set; } = string.Empty;
        public string SpatialOutputHashSha256 { get; set; } = string.Empty;
    }

    public sealed class SimulationRuntime표현요청
    {
        public 경영SimulationSessionSnapshot Session { get; set; }
            = new 경영SimulationSessionSnapshot();
        public string SpatialBuildStableId { get; set; } = string.Empty;
        public string SpatialOutputHashSha256 { get; set; } = string.Empty;
        public string SyntyVisualBuildStableId { get; set; } = string.Empty;
        public string SyntyVisualOutputHashSha256 { get; set; } = string.Empty;
        public Simulation렌더CapabilityProfile Capability { get; set; }
            = new Simulation렌더CapabilityProfile();
        public SimulationRoute렌더링Context[] RouteContexts { get; set; }
            = Array.Empty<SimulationRoute렌더링Context>();
        public string[] AcknowledgedOneShotIntentStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class SimulationRuntime표현지시
    {
        public string InstructionStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ChannelCode { get; set; } = string.Empty;
        public string AdapterCode { get; set; } = string.Empty;
        public string InstructionCode { get; set; } = string.Empty;
        public string ProfileKey { get; set; } = string.Empty;
        public string FallbackCode { get; set; } = Simulation렌더링FallbackCodes.필요없음;
        public string SourceIntentStableId { get; set; } = string.Empty;
        public int Priority { get; set; }
        public bool Enabled { get; set; }
        public bool PresentationOnly { get; set; } = true;
    }

    public sealed class Simulation렌더링의도억제기록
    {
        public string SuppressedIntentStableId { get; set; } = string.Empty;
        public string WinningIntentStableId { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ChannelCode { get; set; } = string.Empty;
        public string ReasonCode { get; set; } = string.Empty;
    }

    public sealed class SimulationRuntimeWorldPresentationSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public long SessionRevision { get; set; }
        public long WorldRevision { get; set; }
        public int WorldTick { get; set; }
        public string SpatialBuildStableId { get; set; } = string.Empty;
        public string SpatialOutputHashSha256 { get; set; } = string.Empty;
        public string SyntyVisualBuildStableId { get; set; } = string.Empty;
        public string SyntyVisualOutputHashSha256 { get; set; } = string.Empty;
        public string RenderIntentRuleRevision { get; set; } = string.Empty;
        public string UrpProfileCatalogRevision { get; set; } = string.Empty;
        public string CapabilityProfileRevision { get; set; } = string.Empty;
        public Simulation렌더링의도[] Intents { get; set; } = Array.Empty<Simulation렌더링의도>();
        public SimulationRuntime표현지시[] Instructions { get; set; } =
            Array.Empty<SimulationRuntime표현지시>();
        public Simulation렌더링의도억제기록[] SuppressedIntents { get; set; } =
            Array.Empty<Simulation렌더링의도억제기록>();
        public string PresentationHashSha256 { get; set; } = string.Empty;
        public bool PresentationOnly { get; set; } = true;
    }
}
