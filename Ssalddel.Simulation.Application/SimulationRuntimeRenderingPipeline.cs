using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Simulation.Contracts;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Application
{
public sealed class SimulationFreight렌더링의도Projector
{
    public const string RuleRevision = "simulation-render-intent.freight.v1";

    public Simulation렌더링의도[] Project(경영SimulationSessionSnapshot session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        var intents = new List<Simulation렌더링의도>();
        foreach (var freight in session.FreightTransports
            .Where(item => item.StateCode == 화물운송상태코드.운송중)
            .OrderBy(item => item.TransportRequestStableId, StringComparer.Ordinal))
        {
            var movement = session.LogisticsMovements.SingleOrDefault(item =>
                item.CargoStableId == freight.CargoStableId
                && item.TaskStableId == freight.LogisticsTaskStableId
                && item.StateCode == SimulationLogisticsMovementStateCodes.InTransit);
            if (movement == null)
                continue;

            var sourceRevision = Math.Max(freight.Revision, movement.Revision);
            intents.Add(Intent(
                freight,
                session.Revision,
                sourceRevision,
                Simulation렌더링의도Codes.화물운송중,
                Simulation렌더링ChannelCodes.ObjectState,
                Simulation렌더링범위Codes.Object,
                freight.CargoStableId,
                movement.RouteStableId,
                50));
            intents.Add(Intent(
                freight,
                session.Revision,
                sourceRevision,
                Simulation렌더링의도Codes.차량이동활성,
                Simulation렌더링ChannelCodes.Animation,
                Simulation렌더링범위Codes.Object,
                freight.VehicleStableId,
                movement.RouteStableId,
                60));
            intents.Add(Intent(
                freight,
                session.Revision,
                sourceRevision,
                Simulation렌더링의도Codes.경로운송흐름활성,
                Simulation렌더링ChannelCodes.Attention,
                Simulation렌더링범위Codes.Route,
                movement.RouteStableId,
                freight.VehicleStableId,
                70));
            intents.Add(Intent(
                freight,
                session.Revision,
                sourceRevision,
                Simulation렌더링의도Codes.흙길먼지후보,
                Simulation렌더링ChannelCodes.Fx,
                Simulation렌더링범위Codes.Object,
                freight.VehicleStableId,
                movement.RouteStableId,
                40));
        }

        return intents
            .OrderBy(item => item.IntentStableId, StringComparer.Ordinal)
            .ToArray();
    }

    private static Simulation렌더링의도 Intent(
        SimulationFreightTransportSnapshot freight,
        long sessionRevision,
        long sourceRevision,
        string intentCode,
        string channelCode,
        string scopeCode,
        string targetStableId,
        string? contextStableId,
        int priority) => new()
        {
            IntentStableId = "render-intent:" + freight.TransportRequestStableId + ":" + intentCode,
            SourceStateStableId = freight.TransportRequestStableId,
            SourceStateRevision = sourceRevision,
            SessionRevision = sessionRevision,
            IntentCode = intentCode,
            ChannelCode = channelCode,
            ScopeCode = scopeCode,
            TargetStableId = targetStableId,
            ContextStableId = contextStableId,
            Priority = priority,
            LifetimeCode = Simulation렌더링수명Codes.상태일치동안,
            EvidenceKindCode = "Derived",
            PresentationOnly = true,
        };
}

public sealed class Simulation렌더링의도합성결과
{
    public Simulation렌더링의도[] Selected { get; set; } = Array.Empty<Simulation렌더링의도>();
    public Simulation렌더링의도억제기록[] Suppressed { get; set; } =
        Array.Empty<Simulation렌더링의도억제기록>();
}

public sealed class Simulation렌더링의도합성Policy
{
    public Simulation렌더링의도합성결과 Compose(
        IEnumerable<Simulation렌더링의도> intents,
        long sessionRevision,
        int worldTick,
        IEnumerable<string>? acknowledgedOneShotIntentStableIds = null)
    {
        if (intents == null)
            throw new ArgumentNullException(nameof(intents));
        var acknowledged = new HashSet<string>(
            acknowledgedOneShotIntentStableIds ?? Array.Empty<string>(),
            StringComparer.Ordinal);
        var active = intents
            .Where(item => IsActive(item, sessionRevision, worldTick, acknowledged))
            .ToArray();
        foreach (var intent in active)
            Simulation렌더링PipelineValidator.ValidateIntent(intent, sessionRevision);

        var selected = new List<Simulation렌더링의도>();
        var suppressed = new List<Simulation렌더링의도억제기록>();
        foreach (var group in active.GroupBy(
            item => item.TargetStableId + "\u001f" + item.ChannelCode,
            StringComparer.Ordinal))
        {
            var ordered = group
                .OrderByDescending(item => item.Priority)
                .ThenBy(item => item.IntentStableId, StringComparer.Ordinal)
                .ToArray();
            var winner = ordered[0];
            selected.Add(winner);
            suppressed.AddRange(ordered.Skip(1).Select(item => new Simulation렌더링의도억제기록
            {
                SuppressedIntentStableId = item.IntentStableId,
                WinningIntentStableId = winner.IntentStableId,
                TargetStableId = winner.TargetStableId,
                ChannelCode = winner.ChannelCode,
                ReasonCode = item.Priority == winner.Priority
                    ? "StableIdTieBreak"
                    : "LowerPriorityInChannel",
            }));
        }

        return new Simulation렌더링의도합성결과
        {
            Selected = selected.OrderBy(item => item.IntentStableId, StringComparer.Ordinal).ToArray(),
            Suppressed = suppressed
                .OrderBy(item => item.SuppressedIntentStableId, StringComparer.Ordinal)
                .ToArray(),
        };
    }

    private static bool IsActive(
        Simulation렌더링의도 intent,
        long sessionRevision,
        int worldTick,
        ISet<string> acknowledged)
    {
        if (intent.SessionRevision != sessionRevision)
            return false;
        if (intent.LifetimeCode == Simulation렌더링수명Codes.개정까지)
            return intent.UntilRevision.HasValue && sessionRevision <= intent.UntilRevision.Value;
        if (intent.LifetimeCode == Simulation렌더링수명Codes.기간)
            return intent.ExpiresAtWorldTick.HasValue && worldTick <= intent.ExpiresAtWorldTick.Value;
        if (intent.LifetimeCode == Simulation렌더링수명Codes.일회)
            return !acknowledged.Contains(intent.IntentStableId);
        return true;
    }
}

public sealed class Simulation기본Urp표현Catalog
{
    public const string Revision = "urp-runtime-presentation-catalog.v1";

    public SimulationRuntime표현지시? Resolve(
        Simulation렌더링의도 intent,
        Simulation렌더CapabilityProfile capability,
        IReadOnlyDictionary<string, SimulationRoute렌더링Context> routeContexts)
    {
        if (intent.IntentCode == Simulation렌더링의도Codes.화물운송중)
            return null;
        if (intent.IntentCode == Simulation렌더링의도Codes.차량이동활성)
            return Instruction(
                intent,
                Simulation렌더링AdapterCodes.Animation,
                Simulation렌더링지시Codes.차량경로이동,
                "animation.vehicle.route-follow.v1",
                Simulation렌더링FallbackCodes.필요없음,
                true);
        if (intent.IntentCode == Simulation렌더링의도Codes.경로운송흐름활성)
        {
            var supportsDepth = capability.SupportsDepthTexture;
            return Instruction(
                intent,
                Simulation렌더링AdapterCodes.UrpMaterialPropertyBlock,
                Simulation렌더링지시Codes.경로발광강조,
                supportsDepth
                    ? "urp.route-flow.emission.v1"
                    : "urp.route-flow.simple-color.v1",
                supportsDepth
                    ? Simulation렌더링FallbackCodes.필요없음
                    : Simulation렌더링FallbackCodes.DepthTexture미지원단순강조,
                true);
        }
        if (intent.IntentCode == Simulation렌더링의도Codes.흙길먼지후보)
        {
            var route = intent.ContextStableId != null
                && routeContexts.TryGetValue(intent.ContextStableId, out var resolved)
                    ? resolved
                    : null;
            if (route == null || route.SurfaceCode != Simulation공간표면Codes.흙길)
                return Instruction(
                    intent,
                    Simulation렌더링AdapterCodes.Particle,
                    Simulation렌더링지시Codes.차량흙길먼지,
                    "particle.vehicle.dirt-road.omitted.v1",
                    Simulation렌더링FallbackCodes.흙길근거없어생략,
                    false);
            if (!capability.SupportsParticle || capability.ParticleBudget <= 0)
                return Instruction(
                    intent,
                    Simulation렌더링AdapterCodes.Particle,
                    Simulation렌더링지시Codes.차량흙길먼지,
                    "particle.vehicle.dirt-road.omitted.v1",
                    Simulation렌더링FallbackCodes.Particle미지원으로생략,
                    false);
            return Instruction(
                intent,
                Simulation렌더링AdapterCodes.Particle,
                Simulation렌더링지시Codes.차량흙길먼지,
                capability.TargetPlatformCode == "Mobile"
                    ? "particle.vehicle.dirt-road.mobile.v1"
                    : "particle.vehicle.dirt-road.pc.v1",
                Simulation렌더링FallbackCodes.필요없음,
                true);
        }
        return null;
    }

    private static SimulationRuntime표현지시 Instruction(
        Simulation렌더링의도 intent,
        string adapterCode,
        string instructionCode,
        string profileKey,
        string fallbackCode,
        bool enabled) => new()
        {
            InstructionStableId = "render-instruction:" + intent.IntentStableId,
            TargetStableId = intent.TargetStableId,
            ChannelCode = intent.ChannelCode,
            AdapterCode = adapterCode,
            InstructionCode = instructionCode,
            ProfileKey = profileKey,
            FallbackCode = fallbackCode,
            SourceIntentStableId = intent.IntentStableId,
            Priority = intent.Priority,
            Enabled = enabled,
            PresentationOnly = true,
        };
}

public sealed class SimulationRuntimeWorldPresentationService
{
    private readonly SimulationFreight렌더링의도Projector _projector;
    private readonly Simulation렌더링의도합성Policy _compositionPolicy;
    private readonly Simulation기본Urp표현Catalog _urpCatalog;

    public SimulationRuntimeWorldPresentationService(
        SimulationFreight렌더링의도Projector projector,
        Simulation렌더링의도합성Policy compositionPolicy,
        Simulation기본Urp표현Catalog urpCatalog)
    {
        _projector = projector;
        _compositionPolicy = compositionPolicy;
        _urpCatalog = urpCatalog;
    }

    public SimulationRuntimeWorldPresentationSnapshot Create(
        SimulationRuntime표현요청 request)
    {
        Simulation렌더링PipelineValidator.ValidateRequest(request);
        var intents = _projector.Project(request.Session);
        var composition = _compositionPolicy.Compose(
            intents,
            request.Session.Revision,
            request.Session.WorldContext.WorldTick,
            request.AcknowledgedOneShotIntentStableIds);
        var routeContexts = request.RouteContexts.ToDictionary(
            item => item.RouteStableId,
            StringComparer.Ordinal);
        var instructions = composition.Selected
            .Select(item => _urpCatalog.Resolve(item, request.Capability, routeContexts))
            .Where(item => item != null)
            .Cast<SimulationRuntime표현지시>()
            .OrderBy(item => item.InstructionStableId, StringComparer.Ordinal)
            .ToArray();
        var snapshot = new SimulationRuntimeWorldPresentationSnapshot
        {
            SessionStableId = request.Session.SessionStableId,
            SessionRevision = request.Session.Revision,
            WorldRevision = request.Session.WorldContext.WorldRevision,
            WorldTick = request.Session.WorldContext.WorldTick,
            SpatialBuildStableId = request.SpatialBuildStableId,
            SpatialOutputHashSha256 = request.SpatialOutputHashSha256,
            SyntyVisualBuildStableId = request.SyntyVisualBuildStableId,
            SyntyVisualOutputHashSha256 = request.SyntyVisualOutputHashSha256,
            RenderIntentRuleRevision = SimulationFreight렌더링의도Projector.RuleRevision,
            UrpProfileCatalogRevision = Simulation기본Urp표현Catalog.Revision,
            CapabilityProfileRevision = request.Capability.ProfileRevision,
            Intents = composition.Selected,
            Instructions = instructions,
            SuppressedIntents = composition.Suppressed,
            PresentationOnly = true,
        };
        snapshot.PresentationHashSha256 = SimulationRuntimeWorldPresentationHash.Compute(snapshot);
        return snapshot;
    }
}

public static class Simulation렌더링PipelineValidator
{
    public const string InvalidCode = "SimulationRenderingPipelineInvalid";

    public static void ValidateRequest(SimulationRuntime표현요청 request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        var session = request.Session
            ?? throw new InvalidOperationException(InvalidCode + ":Simulation 상태 사본이 필요합니다.");
        RequireText(session.SessionStableId, "Simulation session 식별자");
        Require(session.ModeCode == SimulationModeCodes.Simulation,
            "Simulation 모드 상태 사본만 렌더링 의도로 투영할 수 있습니다.");
        Require(!session.IsOperationalState,
            "운영 상태를 Simulation 렌더링 규칙으로 처리할 수 없습니다.");
        Require(session.Revision >= 0 && session.WorldContext.WorldRevision >= 0,
            "Simulation 개정 번호가 유효하지 않습니다.");
        RequireText(request.SpatialBuildStableId, "공간 실행 식별자");
        RequireSha256(request.SpatialOutputHashSha256, "공간 출력 SHA-256");
        RequireText(request.SyntyVisualBuildStableId, "Synty 시각 실행 식별자");
        RequireSha256(request.SyntyVisualOutputHashSha256, "Synty 시각 출력 SHA-256");
        ValidateCapability(request.Capability);
        RequireDistinct(request.AcknowledgedOneShotIntentStableIds, "확인한 일회성 렌더링 의도");
        RequireDistinct(request.RouteContexts.Select(item => item.RouteStableId), "경로 표현 Context");
        foreach (var route in request.RouteContexts)
        {
            RequireText(route.RouteStableId, "경로 식별자");
            Require(route.SurfaceCode == Simulation공간표면Codes.흙길
                    || route.SurfaceCode == Simulation공간표면Codes.자갈길
                    || route.SurfaceCode == Simulation공간표면Codes.포장도로
                    || route.SurfaceCode == Simulation공간표면Codes.미확인,
                "지원하지 않는 경로 표면 코드입니다.");
            RequireText(route.EvidenceKindCode, "경로 표면 근거 종류");
            RequireText(route.SpatialBuildStableId, "공간 실행 식별자");
            RequireSha256(route.SpatialOutputHashSha256, "공간 출력 SHA-256");
            Require(route.SpatialBuildStableId == request.SpatialBuildStableId
                    && string.Equals(route.SpatialOutputHashSha256, request.SpatialOutputHashSha256,
                        StringComparison.OrdinalIgnoreCase),
                "경로 표현 Context의 공간 실행본이 Runtime 표현 요청과 일치하지 않습니다.");
        }
    }

    public static void ValidateIntent(Simulation렌더링의도 intent, long sessionRevision)
    {
        RequireText(intent.IntentStableId, "렌더링 의도 식별자");
        RequireText(intent.SourceStateStableId, "렌더링 의도 원본 상태 식별자");
        Require(intent.SourceStateRevision >= 0, "렌더링 의도 원본 개정 번호가 유효하지 않습니다.");
        Require(intent.SessionRevision == sessionRevision,
            "렌더링 의도와 현재 Simulation 개정 번호가 다릅니다.");
        RequireText(intent.IntentCode, "렌더링 의도 코드");
        RequireChannel(intent.ChannelCode);
        RequireScope(intent.ScopeCode);
        RequireText(intent.TargetStableId, "렌더링 의도 대상 식별자");
        Require(intent.Priority is >= 0 and <= 1000, "렌더링 의도 우선순위가 유효하지 않습니다.");
        RequireLifetime(intent.LifetimeCode);
        if (intent.LifetimeCode == Simulation렌더링수명Codes.개정까지)
            Require(intent.UntilRevision.HasValue, "개정 수명 의도에는 종료 개정 번호가 필요합니다.");
        if (intent.LifetimeCode == Simulation렌더링수명Codes.기간)
            Require(intent.ExpiresAtWorldTick.HasValue, "기간 의도에는 종료 WorldTick이 필요합니다.");
        if (intent.LifetimeCode == Simulation렌더링수명Codes.일회)
            Require(intent.OccurrenceSequence.HasValue, "일회성 의도에는 발생 순번이 필요합니다.");
        RequireText(intent.EvidenceKindCode, "렌더링 의도 근거 종류");
        Require(intent.PresentationOnly, "렌더링 의도는 표현 전용이어야 합니다.");
    }

    private static void ValidateCapability(Simulation렌더CapabilityProfile? capability)
    {
        if (capability == null)
            throw new InvalidOperationException(InvalidCode + ":렌더 Capability Profile이 필요합니다.");
        RequireText(capability.ProfileStableId, "렌더 Capability Profile 식별자");
        RequireText(capability.ProfileRevision, "렌더 Capability Profile 개정 번호");
        Require(capability.TargetPlatformCode is "PC" or "Mobile",
            "지원하지 않는 대상 플랫폼입니다.");
        RequireText(capability.QualityTierCode, "렌더 품질 단계 코드");
        Require(capability.MaximumShadowedAdditionalLights >= 0
                && capability.ParticleBudget >= 0
                && capability.ShadowCasterBudget >= 0,
            "렌더 성능 예산은 음수일 수 없습니다.");
    }

    private static void RequireChannel(string code) =>
        Require(code == Simulation렌더링ChannelCodes.Environment
                || code == Simulation렌더링ChannelCodes.Surface
                || code == Simulation렌더링ChannelCodes.Lighting
                || code == Simulation렌더링ChannelCodes.ObjectState
                || code == Simulation렌더링ChannelCodes.Attention
                || code == Simulation렌더링ChannelCodes.Fx
                || code == Simulation렌더링ChannelCodes.Animation,
            "지원하지 않는 렌더링 Channel입니다.");

    private static void RequireScope(string code) =>
        Require(code == Simulation렌더링범위Codes.World
                || code == Simulation렌더링범위Codes.AreaSet
                || code == Simulation렌더링범위Codes.Area
                || code == Simulation렌더링범위Codes.Tile
                || code == Simulation렌더링범위Codes.Route
                || code == Simulation렌더링범위Codes.Facility
                || code == Simulation렌더링범위Codes.Object,
            "지원하지 않는 렌더링 범위입니다.");

    private static void RequireLifetime(string code) =>
        Require(code == Simulation렌더링수명Codes.상태일치동안
                || code == Simulation렌더링수명Codes.개정까지
                || code == Simulation렌더링수명Codes.기간
                || code == Simulation렌더링수명Codes.일회
                || code == Simulation렌더링수명Codes.선택해제까지,
            "지원하지 않는 렌더링 의도 수명입니다.");

    private static void RequireDistinct(IEnumerable<string> values, string name)
    {
        var items = values.ToArray();
        Require(items.Distinct(StringComparer.Ordinal).Count() == items.Length,
            name + " 식별자가 중복되었습니다.");
    }

    private static void RequireText(string? value, string name) =>
        Require(!string.IsNullOrWhiteSpace(value), name + "이(가) 필요합니다.");

    private static void RequireSha256(string value, string name) =>
        Require(value != null && value.Length == 64 && value.All(Uri.IsHexDigit),
            name + "은(는) 64자리 SHA-256이어야 합니다.");

    private static void Require(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(InvalidCode + ":" + message);
    }
}

public static class SimulationRuntimeWorldPresentationHash
{
    public static string Compute(SimulationRuntimeWorldPresentationSnapshot snapshot)
    {
        var canonical = new StringBuilder()
            .Append(snapshot.SessionStableId).Append('|')
            .Append(snapshot.SessionRevision.ToString(CultureInfo.InvariantCulture)).Append('|')
            .Append(snapshot.WorldRevision.ToString(CultureInfo.InvariantCulture)).Append('|')
            .Append(snapshot.WorldTick.ToString(CultureInfo.InvariantCulture)).Append('|')
            .Append(snapshot.SpatialBuildStableId).Append('|')
            .Append(snapshot.SpatialOutputHashSha256.ToLowerInvariant()).Append('|')
            .Append(snapshot.SyntyVisualBuildStableId).Append('|')
            .Append(snapshot.SyntyVisualOutputHashSha256.ToLowerInvariant()).Append('|')
            .Append(snapshot.RenderIntentRuleRevision).Append('|')
            .Append(snapshot.UrpProfileCatalogRevision).Append('|')
            .Append(snapshot.CapabilityProfileRevision);
        foreach (var intent in snapshot.Intents.OrderBy(item => item.IntentStableId, StringComparer.Ordinal))
            canonical.Append("|I:").Append(intent.IntentStableId).Append(':')
                .Append(intent.SourceStateStableId).Append(':').Append(intent.SourceStateRevision)
                .Append(':').Append(intent.SessionRevision).Append(':').Append(intent.IntentCode)
                .Append(':').Append(intent.ChannelCode).Append(':').Append(intent.ScopeCode)
                .Append(':').Append(intent.TargetStableId).Append(':').Append(intent.ContextStableId)
                .Append(':').Append(intent.Priority).Append(':').Append(intent.LifetimeCode)
                .Append(':').Append(intent.UntilRevision).Append(':').Append(intent.ExpiresAtWorldTick)
                .Append(':').Append(intent.OccurrenceSequence).Append(':').Append(intent.EvidenceKindCode);
        foreach (var instruction in snapshot.Instructions.OrderBy(
            item => item.InstructionStableId, StringComparer.Ordinal))
            canonical.Append("|R:").Append(instruction.InstructionStableId).Append(':')
                .Append(instruction.TargetStableId).Append(':').Append(instruction.ChannelCode)
                .Append(':').Append(instruction.AdapterCode).Append(':').Append(instruction.InstructionCode)
                .Append(':').Append(instruction.ProfileKey).Append(':').Append(instruction.FallbackCode)
                .Append(':').Append(instruction.SourceIntentStableId).Append(':')
                .Append(instruction.Priority).Append(':').Append(instruction.Enabled ? "1" : "0");
        foreach (var suppressed in snapshot.SuppressedIntents.OrderBy(
            item => item.SuppressedIntentStableId, StringComparer.Ordinal))
            canonical.Append("|S:").Append(suppressed.SuppressedIntentStableId).Append(':')
                .Append(suppressed.WinningIntentStableId).Append(':').Append(suppressed.TargetStableId)
                .Append(':').Append(suppressed.ChannelCode).Append(':').Append(suppressed.ReasonCode);
        using (var sha = SHA256.Create())
        {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical.ToString()));
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
}
