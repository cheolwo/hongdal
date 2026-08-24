using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Domain
{
public static class SimulationWorldSynty범위Codes
{
    public const string 타일 = "Tile";
    public const string 영역 = "Area";
    public const string 영역묶음 = "AreaSet";
}

public static class SimulationWorldSynty작업상태Codes
{
    public const string 완료 = "Completed";
    public const string 일부완료 = "Partial";
    public const string 자료부족 = "InsufficientSourceData";
    public const string 실패 = "Failed";
    public const string 성능예산초과 = "PerformanceBudgetExceeded";
}

public static class SimulationWorldSynty대상플랫폼Codes
{
    public const string PC = "PC";
    public const string Mobile = "Mobile";
}

public sealed class SimulationWorldSynty경관Job요청
{
    public string JobStableId { get; set; } = string.Empty;
    public string SpatialBuildStableId { get; set; } = string.Empty;
    public string SpatialOutputHashSha256 { get; set; } = string.Empty;
    public string AreaSetStableId { get; set; } = string.Empty;
    public string ScopeKindCode { get; set; } = string.Empty;
    public string ScopeStableId { get; set; } = string.Empty;
    public string LandscapeRuleRevision { get; set; } = string.Empty;
    public string VisualCatalogRevision { get; set; } = string.Empty;
    public string UrpProfileCatalogRevision { get; set; } = string.Empty;
    public int Seed { get; set; }
    public string TargetPlatformCode { get; set; } = string.Empty;
    public string QualityTierCode { get; set; } = string.Empty;
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.SimulationSyntyLandscape,
    SsalddelCodeLayer.Domain,
    "공간 출력과 Synty·URP 대장 개정을 결합한 경관 실행 결과를 정의한다.",
    StepKey = "domain.synty-ledger",
    ExecutionStage = SsalddelCodeExecutionStage.Definition,
    FlowOrder = 10,
    Boundary = "경관 실행은 표현 계획이며 공간 원본·법정동·Simulation 상태를 변경하지 않는다.")]
[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "공간 WI의 실행 문맥·세계 발현 판정에 사용할 공간 조립 증거를 제공한다.",
    Boundary = "AreaSet·Graph·배치·통행은 조건부 입력이며 그 자체로 E4·E5를 완료하지 않는다.")]
public sealed class SimulationWorldSynty경관실행원장
{
    public int SchemaVersion { get; set; } = 1;
    public string VisualBuildStableId { get; set; } = string.Empty;
    public string JobStableId { get; set; } = string.Empty;
    public string SpatialBuildStableId { get; set; } = string.Empty;
    public string SpatialOutputHashSha256 { get; set; } = string.Empty;
    public string AreaSetStableId { get; set; } = string.Empty;
    public string ScopeKindCode { get; set; } = string.Empty;
    public string ScopeStableId { get; set; } = string.Empty;
    public string LandscapeRuleRevision { get; set; } = string.Empty;
    public string VisualCatalogRevision { get; set; } = string.Empty;
    public string UrpProfileCatalogRevision { get; set; } = string.Empty;
    public int Seed { get; set; }
    public string TargetPlatformCode { get; set; } = string.Empty;
    public string QualityTierCode { get; set; } = string.Empty;
    public string InputFingerprintSha256 { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public IReadOnlyList<SimulationWorld그래픽표현계획> GraphicsPlans { get; set; } =
        Array.Empty<SimulationWorld그래픽표현계획>();
    public IReadOnlyList<SimulationWorld시각배치계획> VisualPlacements { get; set; } =
        Array.Empty<SimulationWorld시각배치계획>();
    public IReadOnlyList<SimulationWorldSynty배치거부> Rejections { get; set; } =
        Array.Empty<SimulationWorldSynty배치거부>();
}

public sealed class SimulationWorldSynty배치거부
{
    public string StableId { get; set; } = string.Empty;
    public string? TargetNodeStableId { get; set; }
    public string ReasonCode { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E4,
    "공간 WI의 실행 문맥·세계 발현 판정에 사용할 공간 조립 증거를 제공한다.",
    Boundary = "AreaSet·Graph·배치·통행은 조건부 입력이며 그 자체로 E4·E5를 완료하지 않는다.")]
public static class SimulationWorldSynty경관Validator
{
    public const string InvalidCode = "SimulationWorldSyntyLandscapeInvalid";

    public static void ValidateRequest(SimulationWorldSynty경관Job요청 request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
        RequireText(request.JobStableId, "Synty 작업 식별자");
        RequireText(request.SpatialBuildStableId, "공간 실행 식별자");
        RequireSha256(request.SpatialOutputHashSha256, "공간 출력 SHA-256");
        RequireText(request.AreaSetStableId, "AreaSet 식별자");
        RequireScope(request.ScopeKindCode);
        RequireText(request.ScopeStableId, "Synty 작업 범위 식별자");
        RequireText(request.LandscapeRuleRevision, "경관 규칙 개정 번호");
        RequireText(request.VisualCatalogRevision, "Synty 구성 대장 개정 번호");
        RequireText(request.UrpProfileCatalogRevision, "URP 표현 대장 개정 번호");
        RequirePlatform(request.TargetPlatformCode);
        RequireText(request.QualityTierCode, "품질 단계 코드");
    }

    public static void Validate(SimulationWorldSynty경관실행원장 ledger)
    {
        if (ledger == null)
            throw new ArgumentNullException(nameof(ledger));
        Require(ledger.SchemaVersion == 1, "지원하지 않는 Synty 경관 실행 schema입니다.");
        RequireText(ledger.VisualBuildStableId, "Synty 시각 실행 식별자");
        RequireText(ledger.JobStableId, "Synty 작업 식별자");
        RequireText(ledger.SpatialBuildStableId, "공간 실행 식별자");
        RequireSha256(ledger.SpatialOutputHashSha256, "공간 출력 SHA-256");
        RequireText(ledger.AreaSetStableId, "AreaSet 식별자");
        RequireScope(ledger.ScopeKindCode);
        RequireText(ledger.ScopeStableId, "Synty 작업 범위 식별자");
        RequireText(ledger.LandscapeRuleRevision, "경관 규칙 개정 번호");
        RequireText(ledger.VisualCatalogRevision, "Synty 구성 대장 개정 번호");
        RequireText(ledger.UrpProfileCatalogRevision, "URP 표현 대장 개정 번호");
        RequirePlatform(ledger.TargetPlatformCode);
        RequireText(ledger.QualityTierCode, "품질 단계 코드");
        RequireSha256(ledger.InputFingerprintSha256, "Synty 입력 fingerprint");
        Require(ledger.GeneratedAtUtc != default, "Synty 실행 생성 시각이 필요합니다.");
        RequireStatus(ledger.StatusCode);

        RequireDistinct(ledger.GraphicsPlans.Select(item => item.StableId), "그래픽 표현 계획");
        foreach (var plan in ledger.GraphicsPlans)
        {
            RequireText(plan.StableId, "그래픽 표현 계획 식별자");
            RequireText(plan.TargetNodeStableId, "그래픽 표현 대상 node 식별자");
            RequireText(plan.PresentationScopeCode, "그래픽 표현 범위 코드");
            RequireSemanticKey(plan.TextureSetKey, "질감 세트 키");
            RequireSemanticKey(plan.MaterialVariantKey, "재질 변형 키");
            RequireSemanticKey(plan.ColorPaletteKey, "색조 팔레트 키");
            RequireSemanticKey(plan.BackgroundProfileKey, "배경 Profile 키");
            RequireSemanticKey(plan.LightingProfileKey, "조명 Profile 키");
            RequireSemanticKey(plan.TimeOfDayProfileKey, "시간대 Profile 키");
            RequireShadowPolicy(plan.ShadowPolicyCode);
            Require(plan.ContactShadowStrength >= 0m && plan.ContactShadowStrength <= 1m,
                "접지 그림자 강도는 0~1이어야 합니다.");
            Require(plan.ShadowDistanceMeters == null || plan.ShadowDistanceMeters > 0m,
                "그림자 거리는 0보다 커야 합니다.");
            Require(plan.AmbientOcclusionStrength >= 0m && plan.AmbientOcclusionStrength <= 1m,
                "주변광 차폐 강도는 0~1이어야 합니다.");
            RequireText(plan.LodCode, "그래픽 LOD 코드");
            RequireText(plan.QualityTierCode, "그래픽 품질 단계 코드");
            Require(plan.PresentationOnly, "그래픽 표현 계획은 표현 전용이어야 합니다.");
        }

        RequireDistinct(ledger.VisualPlacements.Select(item => item.StableId), "Synty 시각 배치 계획");
        foreach (var placement in ledger.VisualPlacements)
        {
            RequireText(placement.StableId, "Synty 시각 배치 식별자");
            RequireText(placement.TargetNodeStableId, "Synty 시각 배치 대상 node 식별자");
            RequireSemanticKey(placement.VisualKey, "VisualKey");
            RequireText(placement.LodCode, "Synty 시각 배치 LOD 코드");
            Require(placement.UniformScale > 0m, "Synty 시각 배치 축척은 0보다 커야 합니다.");
            Require(placement.PresentationOnly, "Synty 시각 배치는 표현 전용이어야 합니다.");
        }

        RequireDistinct(ledger.Rejections.Select(item => item.StableId), "Synty 배치 거부 기록");
        foreach (var rejection in ledger.Rejections)
        {
            RequireText(rejection.StableId, "Synty 배치 거부 식별자");
            RequireText(rejection.ReasonCode, "Synty 배치 거부 사유 코드");
            RequireText(rejection.Detail, "Synty 배치 거부 상세");
        }
    }

    private static void RequireScope(string code) =>
        Require(code == SimulationWorldSynty범위Codes.타일
                || code == SimulationWorldSynty범위Codes.영역
                || code == SimulationWorldSynty범위Codes.영역묶음,
            "지원하지 않는 Synty 작업 범위입니다.");

    private static void RequirePlatform(string code) =>
        Require(code == SimulationWorldSynty대상플랫폼Codes.PC
                || code == SimulationWorldSynty대상플랫폼Codes.Mobile,
            "지원하지 않는 Synty 대상 플랫폼입니다.");

    private static void RequireStatus(string code) =>
        Require(code == SimulationWorldSynty작업상태Codes.완료
                || code == SimulationWorldSynty작업상태Codes.일부완료
                || code == SimulationWorldSynty작업상태Codes.자료부족
                || code == SimulationWorldSynty작업상태Codes.실패
                || code == SimulationWorldSynty작업상태Codes.성능예산초과,
            "지원하지 않는 Synty 작업 상태입니다.");

    private static void RequireShadowPolicy(string code) =>
        Require(code == SimulationWorld그림자정책Codes.없음
                || code == SimulationWorld그림자정책Codes.접지
                || code == SimulationWorld그림자정책Codes.실시간
                || code == SimulationWorld그림자정책Codes.혼합
                || code == SimulationWorld그림자정책Codes.원거리통합,
            "지원하지 않는 그림자 정책입니다.");

    private static void RequireSemanticKey(string value, string name)
    {
        RequireText(value, name);
        Require(!value.Contains("/") && !value.Contains("\\"),
            name + "에는 자산 파일 경로를 저장할 수 없습니다.");
    }

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

public static class SimulationWorldSynty경관Hash
{
    public static string ComputeInputFingerprint(SimulationWorldSynty경관Job요청 request)
    {
        SimulationWorldSynty경관Validator.ValidateRequest(request);
        return Sha256(new StringBuilder()
            .Append(request.SpatialBuildStableId).Append('|')
            .Append(request.SpatialOutputHashSha256.ToLowerInvariant()).Append('|')
            .Append(request.AreaSetStableId).Append('|')
            .Append(request.ScopeKindCode).Append('|')
            .Append(request.ScopeStableId).Append('|')
            .Append(request.LandscapeRuleRevision).Append('|')
            .Append(request.VisualCatalogRevision).Append('|')
            .Append(request.UrpProfileCatalogRevision).Append('|')
            .Append(request.Seed.ToString(CultureInfo.InvariantCulture)).Append('|')
            .Append(request.TargetPlatformCode).Append('|')
            .Append(request.QualityTierCode)
            .ToString());
    }

    public static string Compute(SimulationWorldSynty경관실행원장 ledger)
    {
        SimulationWorldSynty경관Validator.Validate(ledger);
        var canonical = new StringBuilder()
            .Append(ledger.SchemaVersion).Append('|')
            .Append(ledger.VisualBuildStableId).Append('|')
            .Append(ledger.SpatialBuildStableId).Append('|')
            .Append(ledger.SpatialOutputHashSha256.ToLowerInvariant()).Append('|')
            .Append(ledger.AreaSetStableId).Append('|')
            .Append(ledger.ScopeKindCode).Append('|')
            .Append(ledger.ScopeStableId).Append('|')
            .Append(ledger.LandscapeRuleRevision).Append('|')
            .Append(ledger.VisualCatalogRevision).Append('|')
            .Append(ledger.UrpProfileCatalogRevision).Append('|')
            .Append(ledger.Seed.ToString(CultureInfo.InvariantCulture)).Append('|')
            .Append(ledger.TargetPlatformCode).Append('|')
            .Append(ledger.QualityTierCode).Append('|')
            .Append(ledger.InputFingerprintSha256.ToLowerInvariant()).Append('|')
            .Append(ledger.StatusCode);
        foreach (var plan in ledger.GraphicsPlans.OrderBy(item => item.StableId, StringComparer.Ordinal))
            canonical.Append("|G:").Append(plan.StableId).Append(':').Append(plan.TargetNodeStableId)
                .Append(':').Append(plan.PresentationScopeCode).Append(':').Append(plan.TextureSetKey)
                .Append(':').Append(plan.MaterialVariantKey).Append(':').Append(plan.ColorPaletteKey)
                .Append(':').Append(plan.BackgroundProfileKey).Append(':').Append(plan.LightingProfileKey)
                .Append(':').Append(plan.TimeOfDayProfileKey).Append(':').Append(plan.ShadowPolicyCode)
                .Append(':').Append(plan.CastShadows ? "1" : "0").Append(':')
                .Append(plan.ReceiveShadows ? "1" : "0").Append(':')
                .Append(plan.ContactShadowStrength.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(plan.ShadowDistanceMeters?.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(plan.AmbientOcclusionStrength.ToString(CultureInfo.InvariantCulture)).Append(':')
                .Append(plan.LodCode).Append(':').Append(plan.QualityTierCode);
        foreach (var placement in ledger.VisualPlacements.OrderBy(item => item.StableId, StringComparer.Ordinal))
            canonical.Append("|V:").Append(placement.StableId).Append(':').Append(placement.TargetNodeStableId)
                .Append(':').Append(placement.VisualKey).Append(':').Append(placement.LodCode)
                .Append(':').Append(placement.PositionX.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(placement.PositionY.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(placement.PositionZ.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(placement.RotationY.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(placement.UniformScale.ToString(CultureInfo.InvariantCulture));
        foreach (var rejection in ledger.Rejections.OrderBy(item => item.StableId, StringComparer.Ordinal))
            canonical.Append("|X:").Append(rejection.StableId).Append(':')
                .Append(rejection.TargetNodeStableId).Append(':').Append(rejection.ReasonCode)
                .Append(':').Append(rejection.Detail);
        return Sha256(canonical.ToString());
    }

    private static string Sha256(string value)
    {
        using (var sha = SHA256.Create())
        {
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}
}
