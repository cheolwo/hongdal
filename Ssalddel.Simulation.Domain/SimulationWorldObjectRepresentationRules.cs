using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Simulation.Domain
{
public static class SimulationWorld규칙상태Codes
{
    public const string 초안 = "Draft";
    public const string 활성 = "Active";
    public const string 폐기 = "Retired";
}

public static class SimulationWorld객체표현적용범위Codes
{
    public const string 영역 = "Area";
    public const string 타일 = "Tile";
    public const string 경로 = "Route";
    public const string 건물 = "Building";
    public const string 객체 = "Object";
}

public static class SimulationWorld규칙미충족처리Codes
{
    public const string 공간표현만 = "SpatialOnly";
    public const string 대체표현 = "Placeholder";
    public const string 숨김 = "Hidden";
    public const string 거부 = "Rejected";
}

public static class SimulationWorld객체표현해석Codes
{
    public const string 공간규칙적용 = "SpatialRuleApplied";
    public const string 공간Simulation규칙적용 = "SpatialAndSimulationRulesApplied";
    public const string 일치규칙없음 = "NoMatchingRule";
}

public sealed class SimulationWorld공간규칙Metadata
{
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string SpatialFactKindCode { get; set; } = string.Empty;
    public string OperatorCode { get; set; } = string.Empty;
    public string ExpectedValueCode { get; set; } = string.Empty;
    public string RequiredEvidenceKindCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class SimulationWorldSimulation규칙Metadata
{
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string StateTypeCode { get; set; } = string.Empty;
    public string ExpectedStateCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public sealed class SimulationWorld객체표현결합규칙
{
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string ObjectSemanticCode { get; set; } = string.Empty;
    public string ScopeCode { get; set; } = string.Empty;
    public string SpatialRuleStableId { get; set; } = string.Empty;
    public string SpatialRuleRevision { get; set; } = string.Empty;
    public string? SimulationRuleStableId { get; set; }
    public string? SimulationRuleRevision { get; set; }
    public bool SimulationRuleRequired { get; set; }
    public string MinimumEvidenceKindCode { get; set; } = string.Empty;
    public string DefaultCompositionKey { get; set; } = string.Empty;
    public string? DynamicIntentBundleKey { get; set; }
    public string UnmetRuleHandlingCode { get; set; } = string.Empty;
    public int Priority { get; set; }
    public bool PresentationOnly { get; set; } = true;
}

public sealed class SimulationWorld객체표현규칙대장
{
    public int SchemaVersion { get; set; } = 1;
    public string CatalogRevision { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public IReadOnlyList<SimulationWorld공간규칙Metadata> SpatialRules { get; set; } =
        Array.Empty<SimulationWorld공간규칙Metadata>();
    public IReadOnlyList<SimulationWorldSimulation규칙Metadata> SimulationRules { get; set; } =
        Array.Empty<SimulationWorldSimulation규칙Metadata>();
    public IReadOnlyList<SimulationWorld객체표현결합규칙> BindingRules { get; set; } =
        Array.Empty<SimulationWorld객체표현결합규칙>();
}

public sealed class SimulationWorld객체표현대상사실
{
    public string TargetNodeStableId { get; set; } = string.Empty;
    public string ObjectSemanticCode { get; set; } = string.Empty;
    public string ScopeCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public IReadOnlyList<string> MatchedSpatialRuleStableIds { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> MatchedSimulationRuleStableIds { get; set; } = Array.Empty<string>();
}

public sealed class SimulationWorld객체표현해석요청
{
    public string InterpretationStableId { get; set; } = string.Empty;
    public string SpatialBuildStableId { get; set; } = string.Empty;
    public string SpatialOutputHashSha256 { get; set; } = string.Empty;
    public string? SimulationSessionStableId { get; set; }
    public long? SimulationSessionRevision { get; set; }
    public long? WorldTick { get; set; }
    public string RuleCatalogRevision { get; set; } = string.Empty;
    public DateTimeOffset InterpretedAtUtc { get; set; }
    public IReadOnlyList<SimulationWorld객체표현대상사실> Targets { get; set; } =
        Array.Empty<SimulationWorld객체표현대상사실>();
}

public sealed class SimulationWorld객체표현해석결과
{
    public string StableId { get; set; } = string.Empty;
    public string TargetNodeStableId { get; set; } = string.Empty;
    public string ObjectSemanticCode { get; set; } = string.Empty;
    public string ScopeCode { get; set; } = string.Empty;
    public string ResolutionCode { get; set; } = string.Empty;
    public string? AppliedBindingRuleStableId { get; set; }
    public string? AppliedBindingRuleRevision { get; set; }
    public string? AppliedSpatialRuleStableId { get; set; }
    public string? AppliedSimulationRuleStableId { get; set; }
    public string? DefaultCompositionKey { get; set; }
    public string? DynamicIntentBundleKey { get; set; }
    public string UnmetRuleHandlingCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public bool PresentationOnly { get; set; } = true;
}

public sealed class SimulationWorld객체표현해석원장
{
    public int SchemaVersion { get; set; } = 1;
    public string InterpretationStableId { get; set; } = string.Empty;
    public string SpatialBuildStableId { get; set; } = string.Empty;
    public string SpatialOutputHashSha256 { get; set; } = string.Empty;
    public string? SimulationSessionStableId { get; set; }
    public long? SimulationSessionRevision { get; set; }
    public long? WorldTick { get; set; }
    public string RuleCatalogRevision { get; set; } = string.Empty;
    public string InputFingerprintSha256 { get; set; } = string.Empty;
    public string OutputHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset InterpretedAtUtc { get; set; }
    public IReadOnlyList<SimulationWorld객체표현해석결과> Results { get; set; } =
        Array.Empty<SimulationWorld객체표현해석결과>();
}

public static class SimulationWorld객체표현규칙Validator
{
    public const string InvalidCode = "SimulationWorldObjectRepresentationRuleInvalid";

    public static void Validate(SimulationWorld객체표현규칙대장 catalog)
    {
        if (catalog == null) throw new ArgumentNullException(nameof(catalog));
        Require(catalog.SchemaVersion == 1, "지원하지 않는 객체 표현 규칙 schema입니다.");
        Text(catalog.CatalogRevision, "규칙 대장 개정 번호");
        Require(catalog.CreatedAtUtc != default, "규칙 대장 생성 시각이 필요합니다.");
        Distinct(catalog.SpatialRules.Select(Key), "공간 규칙");
        Distinct(catalog.SimulationRules.Select(Key), "Simulation 규칙");
        Distinct(catalog.BindingRules.Select(Key), "객체 표현 결합 규칙");

        foreach (var rule in catalog.SpatialRules)
        {
            CommonRule(rule.StableId, rule.Revision, rule.StatusCode);
            Text(rule.SpatialFactKindCode, "공간 사실 종류 코드");
            Text(rule.OperatorCode, "공간 규칙 연산자 코드");
            Text(rule.ExpectedValueCode, "공간 규칙 기대 값 코드");
            Text(rule.RequiredEvidenceKindCode, "공간 규칙 최소 근거 종류");
            Text(rule.Description, "공간 규칙 설명");
        }
        foreach (var rule in catalog.SimulationRules)
        {
            CommonRule(rule.StableId, rule.Revision, rule.StatusCode);
            Text(rule.StateTypeCode, "Simulation 상태 종류 코드");
            Text(rule.ExpectedStateCode, "Simulation 기대 상태 코드");
            Text(rule.Description, "Simulation 규칙 설명");
        }
        foreach (var rule in catalog.BindingRules)
        {
            CommonRule(rule.StableId, rule.Revision, rule.StatusCode);
            Text(rule.ObjectSemanticCode, "객체 의미 코드");
            Scope(rule.ScopeCode);
            Text(rule.SpatialRuleStableId, "공간 규칙 식별자");
            Text(rule.SpatialRuleRevision, "공간 규칙 개정 번호");
            Text(rule.MinimumEvidenceKindCode, "결합 규칙 최소 근거 종류");
            SemanticKey(rule.DefaultCompositionKey, "기본 구성 키");
            if (rule.DynamicIntentBundleKey != null)
                SemanticKey(rule.DynamicIntentBundleKey, "동적 표현 의도 묶음 키");
            Unmet(rule.UnmetRuleHandlingCode);
            Require(rule.PresentationOnly, "객체 표현 결합 규칙은 표현 전용이어야 합니다.");
            var spatial = catalog.SpatialRules.SingleOrDefault(item =>
                item.StableId == rule.SpatialRuleStableId && item.Revision == rule.SpatialRuleRevision);
            Require(spatial != null, "결합 규칙이 참조하는 공간 규칙이 없습니다.");
            if (rule.SimulationRuleStableId == null)
            {
                Require(rule.SimulationRuleRevision == null && !rule.SimulationRuleRequired,
                    "Simulation 규칙이 없으면 개정 번호와 필수 여부를 지정할 수 없습니다.");
            }
            else
            {
                Text(rule.SimulationRuleRevision, "Simulation 규칙 개정 번호");
                Require(rule.SimulationRuleRequired,
                    "Simulation 규칙을 참조하는 결합 규칙은 현재 규칙을 필수로 표시해야 합니다.");
                var simulation = catalog.SimulationRules.SingleOrDefault(item =>
                    item.StableId == rule.SimulationRuleStableId
                    && item.Revision == rule.SimulationRuleRevision);
                Require(simulation != null, "결합 규칙이 참조하는 Simulation 규칙이 없습니다.");
                if (rule.StatusCode == SimulationWorld규칙상태Codes.활성)
                    Require(simulation!.StatusCode == SimulationWorld규칙상태Codes.활성,
                        "활성 결합 규칙은 초안 Simulation 규칙을 참조할 수 없습니다.");
            }
            if (rule.StatusCode == SimulationWorld규칙상태Codes.활성)
                Require(spatial!.StatusCode == SimulationWorld규칙상태Codes.활성,
                    "활성 결합 규칙은 초안 공간 규칙을 참조할 수 없습니다.");
        }
    }

    public static void ValidateRequest(
        SimulationWorld객체표현해석요청 request,
        SimulationWorld객체표현규칙대장 catalog)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        Validate(catalog);
        Text(request.InterpretationStableId, "객체 표현 해석 식별자");
        Text(request.SpatialBuildStableId, "공간 실행 식별자");
        Sha(request.SpatialOutputHashSha256, "공간 출력 SHA-256");
        Require(request.RuleCatalogRevision == catalog.CatalogRevision,
            "요청과 객체 표현 규칙 대장의 개정 번호가 다릅니다.");
        Require(request.InterpretedAtUtc != default, "객체 표현 해석 시각이 필요합니다.");
        var simulationReferenceAbsent = request.SimulationSessionStableId == null
            && !request.SimulationSessionRevision.HasValue && !request.WorldTick.HasValue;
        var simulationReferenceComplete = request.SimulationSessionStableId != null
            && request.SimulationSessionRevision.HasValue && request.WorldTick.HasValue;
        Require(simulationReferenceAbsent || simulationReferenceComplete,
            "Simulation 상태를 사용하면 세션 식별자·개정·WorldTick을 함께 지정해야 합니다.");
        if (request.SimulationSessionStableId != null)
        {
            Text(request.SimulationSessionStableId, "Simulation 세션 식별자");
            Require(request.SimulationSessionRevision >= 0 && request.WorldTick >= 0,
                "Simulation 개정과 WorldTick은 0 이상이어야 합니다.");
        }
        Distinct(request.Targets.Select(item => item.TargetNodeStableId), "객체 표현 해석 대상");
        foreach (var target in request.Targets)
        {
            Text(target.TargetNodeStableId, "대상 node 식별자");
            Text(target.ObjectSemanticCode, "대상 객체 의미 코드");
            Scope(target.ScopeCode);
            Text(target.EvidenceKindCode, "대상 근거 종류 코드");
            Distinct(target.MatchedSpatialRuleStableIds, "일치 공간 규칙");
            Distinct(target.MatchedSimulationRuleStableIds, "일치 Simulation 규칙");
        }
    }

    public static void Validate(SimulationWorld객체표현해석원장 ledger)
    {
        if (ledger == null) throw new ArgumentNullException(nameof(ledger));
        Require(ledger.SchemaVersion == 1, "지원하지 않는 객체 표현 해석 schema입니다.");
        Text(ledger.InterpretationStableId, "객체 표현 해석 식별자");
        Text(ledger.SpatialBuildStableId, "공간 실행 식별자");
        Sha(ledger.SpatialOutputHashSha256, "공간 출력 SHA-256");
        Text(ledger.RuleCatalogRevision, "규칙 대장 개정 번호");
        Sha(ledger.InputFingerprintSha256, "해석 입력 fingerprint");
        Sha(ledger.OutputHashSha256, "해석 출력 SHA-256");
        Require(string.Equals(
                ledger.OutputHashSha256,
                SimulationWorld객체표현해석기.ComputeOutputHash(ledger),
                StringComparison.OrdinalIgnoreCase),
            "해석 출력 SHA-256이 결과 내용과 일치하지 않습니다.");
        Require(ledger.InterpretedAtUtc != default, "객체 표현 해석 시각이 필요합니다.");
        Distinct(ledger.Results.Select(item => item.StableId), "객체 표현 해석 결과");
        Distinct(ledger.Results.Select(item => item.TargetNodeStableId), "객체 표현 해석 결과 대상");
        foreach (var result in ledger.Results)
        {
            Text(result.StableId, "해석 결과 식별자");
            Text(result.TargetNodeStableId, "해석 결과 대상 node 식별자");
            Text(result.ObjectSemanticCode, "해석 결과 객체 의미 코드");
            Scope(result.ScopeCode);
            Text(result.ResolutionCode, "해석 결과 코드");
            Unmet(result.UnmetRuleHandlingCode);
            Text(result.EvidenceKindCode, "해석 결과 근거 종류");
            Require(result.PresentationOnly, "객체 표현 해석 결과는 표현 전용이어야 합니다.");
            if (result.ResolutionCode == SimulationWorld객체표현해석Codes.일치규칙없음)
            {
                Require(result.AppliedBindingRuleStableId == null && result.AppliedSpatialRuleStableId == null
                    && result.AppliedSimulationRuleStableId == null && result.DefaultCompositionKey == null
                    && result.DynamicIntentBundleKey == null, "일치 규칙이 없는 결과에 적용 규칙이나 표현 키를 둘 수 없습니다.");
            }
            else
            {
                Require(result.ResolutionCode == SimulationWorld객체표현해석Codes.공간규칙적용
                    || result.ResolutionCode == SimulationWorld객체표현해석Codes.공간Simulation규칙적용,
                    "지원하지 않는 객체 표현 해석 결과 코드입니다.");
                Text(result.AppliedBindingRuleStableId, "적용 결합 규칙 식별자");
                Text(result.AppliedBindingRuleRevision, "적용 결합 규칙 개정 번호");
                Text(result.AppliedSpatialRuleStableId, "적용 공간 규칙 식별자");
                SemanticKey(result.DefaultCompositionKey, "해석 결과 기본 구성 키");
                if (result.DynamicIntentBundleKey != null)
                    SemanticKey(result.DynamicIntentBundleKey, "해석 결과 동적 표현 의도 묶음 키");
                if (result.ResolutionCode == SimulationWorld객체표현해석Codes.공간Simulation규칙적용)
                    Text(result.AppliedSimulationRuleStableId, "적용 Simulation 규칙 식별자");
                else
                    Require(result.AppliedSimulationRuleStableId == null,
                        "공간 규칙만 적용한 결과에 Simulation 규칙을 둘 수 없습니다.");
            }
        }
    }

    private static string Key(SimulationWorld공간규칙Metadata item) => item.StableId + "@" + item.Revision;
    private static string Key(SimulationWorldSimulation규칙Metadata item) => item.StableId + "@" + item.Revision;
    private static string Key(SimulationWorld객체표현결합규칙 item) => item.StableId + "@" + item.Revision;
    private static void CommonRule(string id, string revision, string status)
    {
        Text(id, "규칙 식별자"); Text(revision, "규칙 개정 번호"); Status(status);
    }
    private static void Status(string code) => Require(
        code == SimulationWorld규칙상태Codes.초안 || code == SimulationWorld규칙상태Codes.활성
        || code == SimulationWorld규칙상태Codes.폐기, "지원하지 않는 규칙 상태입니다.");
    private static void Scope(string code) => Require(
        code == SimulationWorld객체표현적용범위Codes.영역 || code == SimulationWorld객체표현적용범위Codes.타일
        || code == SimulationWorld객체표현적용범위Codes.경로 || code == SimulationWorld객체표현적용범위Codes.건물
        || code == SimulationWorld객체표현적용범위Codes.객체, "지원하지 않는 객체 표현 적용 범위입니다.");
    private static void Unmet(string code) => Require(
        code == SimulationWorld규칙미충족처리Codes.공간표현만 || code == SimulationWorld규칙미충족처리Codes.대체표현
        || code == SimulationWorld규칙미충족처리Codes.숨김 || code == SimulationWorld규칙미충족처리Codes.거부,
        "지원하지 않는 규칙 미충족 처리입니다.");
    private static void SemanticKey(string? value, string name)
    {
        Text(value, name);
        Require(!value!.Contains("Assets/", StringComparison.OrdinalIgnoreCase)
            && !value.Contains(".prefab", StringComparison.OrdinalIgnoreCase),
            name + "에는 Unity 자산 경로를 저장할 수 없습니다.");
    }
    private static void Sha(string value, string name) =>
        Require(value.Length == 64 && value.All(Uri.IsHexDigit), name + " 형식이 올바르지 않습니다.");
    private static void Text(string? value, string name) => Require(!string.IsNullOrWhiteSpace(value), name + "이(가) 필요합니다.");
    private static void Distinct(IEnumerable<string> values, string name)
    {
        var array = values.ToArray();
        Require(array.Distinct(StringComparer.Ordinal).Count() == array.Length, name + "이(가) 중복되었습니다.");
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(InvalidCode + ": " + message);
    }
}

public static class SimulationWorld객체표현해석기
{
    public static SimulationWorld객체표현해석원장 Interpret(
        SimulationWorld객체표현해석요청 request,
        SimulationWorld객체표현규칙대장 catalog)
    {
        SimulationWorld객체표현규칙Validator.ValidateRequest(request, catalog);
        var results = request.Targets
            .OrderBy(item => item.TargetNodeStableId, StringComparer.Ordinal)
            .Select(target => Resolve(target, catalog))
            .ToArray();
        var input = ComputeInputHash(request, catalog);
        var ledger = new SimulationWorld객체표현해석원장
        {
            InterpretationStableId = request.InterpretationStableId,
            SpatialBuildStableId = request.SpatialBuildStableId,
            SpatialOutputHashSha256 = request.SpatialOutputHashSha256.ToLowerInvariant(),
            SimulationSessionStableId = request.SimulationSessionStableId,
            SimulationSessionRevision = request.SimulationSessionRevision,
            WorldTick = request.WorldTick,
            RuleCatalogRevision = request.RuleCatalogRevision,
            InputFingerprintSha256 = input,
            InterpretedAtUtc = request.InterpretedAtUtc,
            Results = results,
        };
        ledger.OutputHashSha256 = ComputeOutputHash(ledger);
        SimulationWorld객체표현규칙Validator.Validate(ledger);
        return ledger;
    }

    public static string ComputeCatalogHash(SimulationWorld객체표현규칙대장 catalog)
    {
        SimulationWorld객체표현규칙Validator.Validate(catalog);
        var parts = new List<string> { catalog.SchemaVersion.ToString(CultureInfo.InvariantCulture), catalog.CatalogRevision };
        parts.AddRange(catalog.SpatialRules.OrderBy(item => item.StableId, StringComparer.Ordinal).ThenBy(item => item.Revision)
            .Select(item => string.Join("|", item.StableId, item.Revision, item.StatusCode, item.SpatialFactKindCode,
                item.OperatorCode, item.ExpectedValueCode, item.RequiredEvidenceKindCode, item.Description)));
        parts.AddRange(catalog.SimulationRules.OrderBy(item => item.StableId, StringComparer.Ordinal).ThenBy(item => item.Revision)
            .Select(item => string.Join("|", item.StableId, item.Revision, item.StatusCode, item.StateTypeCode,
                item.ExpectedStateCode, item.Description)));
        parts.AddRange(catalog.BindingRules.OrderBy(item => item.StableId, StringComparer.Ordinal).ThenBy(item => item.Revision)
            .Select(item => string.Join("|", item.StableId, item.Revision, item.StatusCode, item.ObjectSemanticCode,
                item.ScopeCode, item.SpatialRuleStableId, item.SpatialRuleRevision, item.SimulationRuleStableId ?? "",
                item.SimulationRuleRevision ?? "", item.SimulationRuleRequired, item.MinimumEvidenceKindCode,
                item.DefaultCompositionKey, item.DynamicIntentBundleKey ?? "", item.UnmetRuleHandlingCode,
                item.Priority, item.PresentationOnly)));
        return Hash(parts);
    }

    private static SimulationWorld객체표현해석결과 Resolve(
        SimulationWorld객체표현대상사실 target,
        SimulationWorld객체표현규칙대장 catalog)
    {
        var spatial = new HashSet<string>(target.MatchedSpatialRuleStableIds, StringComparer.Ordinal);
        var simulation = new HashSet<string>(target.MatchedSimulationRuleStableIds, StringComparer.Ordinal);
        var selected = catalog.BindingRules
            .Where(rule => rule.StatusCode == SimulationWorld규칙상태Codes.활성
                && rule.ObjectSemanticCode == target.ObjectSemanticCode
                && rule.ScopeCode == target.ScopeCode
                && spatial.Contains(rule.SpatialRuleStableId)
                && EvidenceSatisfies(target.EvidenceKindCode, rule.MinimumEvidenceKindCode)
                && (rule.SimulationRuleStableId == null || simulation.Contains(rule.SimulationRuleStableId)))
            .OrderByDescending(rule => rule.Priority)
            .ThenBy(rule => rule.StableId, StringComparer.Ordinal)
            .FirstOrDefault();
        if (selected == null)
            return new SimulationWorld객체표현해석결과
            {
                StableId = "object-presentation-result:" + StableSuffix(target.TargetNodeStableId),
                TargetNodeStableId = target.TargetNodeStableId,
                ObjectSemanticCode = target.ObjectSemanticCode,
                ScopeCode = target.ScopeCode,
                ResolutionCode = SimulationWorld객체표현해석Codes.일치규칙없음,
                UnmetRuleHandlingCode = SimulationWorld규칙미충족처리Codes.거부,
                EvidenceKindCode = target.EvidenceKindCode,
            };
        return new SimulationWorld객체표현해석결과
        {
            StableId = "object-presentation-result:" + StableSuffix(target.TargetNodeStableId),
            TargetNodeStableId = target.TargetNodeStableId,
            ObjectSemanticCode = target.ObjectSemanticCode,
            ScopeCode = target.ScopeCode,
            ResolutionCode = selected.SimulationRuleStableId == null
                ? SimulationWorld객체표현해석Codes.공간규칙적용
                : SimulationWorld객체표현해석Codes.공간Simulation규칙적용,
            AppliedBindingRuleStableId = selected.StableId,
            AppliedBindingRuleRevision = selected.Revision,
            AppliedSpatialRuleStableId = selected.SpatialRuleStableId,
            AppliedSimulationRuleStableId = selected.SimulationRuleStableId,
            DefaultCompositionKey = selected.DefaultCompositionKey,
            DynamicIntentBundleKey = selected.DynamicIntentBundleKey,
            UnmetRuleHandlingCode = selected.UnmetRuleHandlingCode,
            EvidenceKindCode = target.EvidenceKindCode,
        };
    }

    private static string ComputeInputHash(
        SimulationWorld객체표현해석요청 request,
        SimulationWorld객체표현규칙대장 catalog)
    {
        var parts = new List<string>
        {
            request.InterpretationStableId, request.SpatialBuildStableId,
            request.SpatialOutputHashSha256.ToLowerInvariant(), request.SimulationSessionStableId ?? "",
            request.SimulationSessionRevision?.ToString(CultureInfo.InvariantCulture) ?? "",
            request.WorldTick?.ToString(CultureInfo.InvariantCulture) ?? "",
            request.RuleCatalogRevision, ComputeCatalogHash(catalog),
        };
        parts.AddRange(request.Targets.OrderBy(item => item.TargetNodeStableId, StringComparer.Ordinal).Select(item =>
            string.Join("|", item.TargetNodeStableId, item.ObjectSemanticCode, item.ScopeCode, item.EvidenceKindCode,
                string.Join(",", item.MatchedSpatialRuleStableIds.OrderBy(value => value, StringComparer.Ordinal)),
                string.Join(",", item.MatchedSimulationRuleStableIds.OrderBy(value => value, StringComparer.Ordinal)))));
        return Hash(parts);
    }

    public static string ComputeOutputHash(SimulationWorld객체표현해석원장 ledger)
    {
        var parts = new List<string> { ledger.InputFingerprintSha256 };
        parts.AddRange(ledger.Results.OrderBy(item => item.StableId, StringComparer.Ordinal).Select(item =>
            string.Join("|", item.StableId, item.TargetNodeStableId, item.ObjectSemanticCode, item.ScopeCode,
                item.ResolutionCode, item.AppliedBindingRuleStableId ?? "", item.AppliedBindingRuleRevision ?? "",
                item.AppliedSpatialRuleStableId ?? "", item.AppliedSimulationRuleStableId ?? "",
                item.DefaultCompositionKey ?? "", item.DynamicIntentBundleKey ?? "", item.UnmetRuleHandlingCode,
                item.EvidenceKindCode, item.PresentationOnly)));
        return Hash(parts);
    }

    private static string StableSuffix(string value)
    {
        var hash = Hash(new[] { value });
        return hash.Substring(0, 24);
    }

    private static bool EvidenceSatisfies(string actual, string minimum)
    {
        if (string.Equals(actual, minimum, StringComparison.Ordinal)) return true;
        var actualRank = EvidenceRank(actual);
        var minimumRank = EvidenceRank(minimum);
        return actualRank > 0 && minimumRank > 0 && actualRank >= minimumRank;
    }

    private static int EvidenceRank(string code)
    {
        if (code == "Observed") return 5;
        if (code == "Derived") return 4;
        if (code == "StatisticallyAllocated") return 3;
        if (code == "Scenario") return 2;
        if (code == "Decorative") return 1;
        return 0;
    }

    private static string Hash(IEnumerable<string> parts)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(string.Join("\n", parts));
        return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", string.Empty).ToLowerInvariant();
    }
}
}
