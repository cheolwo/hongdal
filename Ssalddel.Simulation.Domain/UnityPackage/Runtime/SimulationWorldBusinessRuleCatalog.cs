using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Simulation.Domain
{
public static class SimulationWorld시설종류Codes
{
    public const string 농장 = "Farm";
    public const string 물류Hub = "LogisticsHub";
    public const string 마트 = "Mart";
    public const string 음식점 = "Restaurant";
}

public static class SimulationWorld시설기능Codes
{
    public const string 생산 = "Produce";
    public const string 수확 = "Harvest";
    public const string 포장 = "Package";
    public const string 출하 = "Dispatch";
    public const string 입고 = "Receive";
    public const string 검수 = "Inspect";
    public const string 보관 = "Store";
    public const string 상차 = "Load";
    public const string 하차 = "Unload";
    public const string 주문 = "Order";
    public const string 진열 = "Display";
    public const string 판매 = "Sell";
    public const string 소비 = "Consume";
    public const string 방어 = "Defense";
}

public static class SimulationWorld업무규칙영역Codes
{
    public const string 생산 = "Production";
    public const string 주문 = "Order";
    public const string 마트 = "Mart";
    public const string 창고 = "Warehouse";
    public const string 물류 = "Logistics";
    public const string 화물 = "Freight";
    public const string 음식점 = "Restaurant";
    public const string 팀역할 = "TeamRole";
    public const string 수집보상 = "CollectibleReward";
    public const string 전투 = "Combat";
}

public sealed class SimulationWorld시설의미
{
    public string StableId { get; set; } = string.Empty;
    public string SpatialNodeStableId { get; set; } = string.Empty;
    public string FacilityTypeCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string EvidenceSourceStableId { get; set; } = string.Empty;
    public string ConfidenceCode { get; set; } = string.Empty;
    public bool ScenarioAssigned { get; set; }
}

public sealed class SimulationWorld시설기능
{
    public string StableId { get; set; } = string.Empty;
    public string FacilityStableId { get; set; } = string.Empty;
    public string CapabilityCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
}

public sealed class SimulationWorld업무Simulation규칙
{
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string DomainCode { get; set; } = string.Empty;
    public string RuleTypeCode { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string EngineKey { get; set; } = string.Empty;
    public string InputContractKey { get; set; } = string.Empty;
    public string OutputContractKey { get; set; } = string.Empty;
    public bool Deterministic { get; set; }
    public bool SimulationOnly { get; set; } = true;
    public string Description { get; set; } = string.Empty;
}

public sealed class SimulationWorld업무Simulation규칙Parameter
{
    public string RuleStableId { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public string ParameterCode { get; set; } = string.Empty;
    public string ValueTypeCode { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? UnitCode { get; set; }
    public string EvidenceKindCode { get; set; } = string.Empty;
}

public sealed class SimulationWorld객체업무규칙연결
{
    public string StableId { get; set; } = string.Empty;
    public string FacilityStableId { get; set; } = string.Empty;
    public string CapabilityCode { get; set; } = string.Empty;
    public string RuleStableId { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public string ScopeCode { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string EvidenceKindCode { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}

public sealed class SimulationWorldScenario규칙항목
{
    public string RuleStableId { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public int ApplyOrder { get; set; }
    public bool Required { get; set; }
}

public sealed class SimulationWorldScenario규칙묶음
{
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string AreaSetStableId { get; set; } = string.Empty;
    public IReadOnlyList<SimulationWorldScenario규칙항목> Items { get; set; } =
        Array.Empty<SimulationWorldScenario규칙항목>();
}

public sealed class SimulationWorld업무규칙집결원장
{
    public int SchemaVersion { get; set; } = 1;
    public string CatalogRevision { get; set; } = string.Empty;
    public string SpatialBuildStableId { get; set; } = string.Empty;
    public string SpatialOutputHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public IReadOnlyList<SimulationWorld시설의미> Facilities { get; set; } =
        Array.Empty<SimulationWorld시설의미>();
    public IReadOnlyList<SimulationWorld시설기능> Capabilities { get; set; } =
        Array.Empty<SimulationWorld시설기능>();
    public IReadOnlyList<SimulationWorld업무Simulation규칙> Rules { get; set; } =
        Array.Empty<SimulationWorld업무Simulation규칙>();
    public IReadOnlyList<SimulationWorld업무Simulation규칙Parameter> Parameters { get; set; } =
        Array.Empty<SimulationWorld업무Simulation규칙Parameter>();
    public IReadOnlyList<SimulationWorld객체업무규칙연결> Bindings { get; set; } =
        Array.Empty<SimulationWorld객체업무규칙연결>();
    public IReadOnlyList<SimulationWorldScenario규칙묶음> ScenarioRuleSets { get; set; } =
        Array.Empty<SimulationWorldScenario규칙묶음>();
}

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E1,
    "구성 요소의 핵심 계약과 불변 경계를 정의한다.",
    Boundary = "계약과 도메인 정의는 실행 위치나 E 단계 달성 증거를 소유하지 않는다.")]
public static class SimulationWorld업무규칙집결Validator
{
    public const string InvalidCode = "SimulationWorldBusinessRuleCatalogInvalid";

    public static void Validate(SimulationWorld업무규칙집결원장 catalog)
    {
        if (catalog == null) throw new ArgumentNullException(nameof(catalog));
        Require(catalog.SchemaVersion == 1, "지원하지 않는 업무 규칙 대장 schema입니다.");
        Text(catalog.CatalogRevision, "대장 개정 번호");
        Text(catalog.SpatialBuildStableId, "공간 실행 식별자");
        Sha(catalog.SpatialOutputHashSha256, "공간 출력 SHA-256");
        Require(catalog.CreatedAtUtc != default, "생성 시각이 필요합니다.");

        Distinct(catalog.Facilities.Select(x => x.StableId), "시설");
        Distinct(catalog.Capabilities.Select(x => x.StableId), "시설 기능");
        Distinct(catalog.Rules.Select(RuleKey), "업무 규칙");
        Distinct(catalog.Parameters.Select(x => RuleKey(x.RuleStableId, x.RuleRevision) + ":" + x.ParameterCode), "규칙 Parameter");
        Distinct(catalog.Bindings.Select(x => x.StableId), "객체 규칙 연결");
        Distinct(catalog.ScenarioRuleSets.Select(x => x.StableId + "@" + x.Revision), "Scenario 규칙 묶음");

        var facilities = new HashSet<string>(catalog.Facilities.Select(x => x.StableId), StringComparer.Ordinal);
        var capabilities = new HashSet<string>(catalog.Capabilities.Select(x => x.FacilityStableId + ":" + x.CapabilityCode), StringComparer.Ordinal);
        var rules = new HashSet<string>(catalog.Rules.Select(RuleKey), StringComparer.Ordinal);
        foreach (var facility in catalog.Facilities)
        {
            Text(facility.StableId, "시설 식별자"); Text(facility.SpatialNodeStableId, "공간 node 식별자");
            Text(facility.FacilityTypeCode, "시설 종류"); Evidence(facility.EvidenceKindCode);
            Text(facility.EvidenceSourceStableId, "시설 근거 식별자"); Text(facility.ConfidenceCode, "신뢰 수준");
        }
        foreach (var capability in catalog.Capabilities)
        {
            Text(capability.StableId, "시설 기능 식별자"); Text(capability.CapabilityCode, "시설 기능 코드");
            Require(facilities.Contains(capability.FacilityStableId), "시설 기능이 존재하지 않는 시설을 참조합니다.");
            Evidence(capability.EvidenceKindCode);
        }
        foreach (var rule in catalog.Rules)
        {
            Text(rule.StableId, "규칙 식별자"); Text(rule.Revision, "규칙 개정"); Text(rule.DomainCode, "규칙 영역");
            Text(rule.RuleTypeCode, "규칙 종류"); Text(rule.StatusCode, "규칙 상태"); Text(rule.EngineKey, "규칙 Engine 키");
            Text(rule.InputContractKey, "입력 계약 키"); Text(rule.OutputContractKey, "출력 계약 키"); Text(rule.Description, "규칙 설명");
            Require(rule.SimulationOnly, "업무 규칙 대장에는 Simulation 전용 규칙만 허용합니다.");
        }
        foreach (var parameter in catalog.Parameters)
        {
            Require(rules.Contains(RuleKey(parameter.RuleStableId, parameter.RuleRevision)), "Parameter가 존재하지 않는 규칙을 참조합니다.");
            Text(parameter.ParameterCode, "Parameter 코드"); Text(parameter.ValueTypeCode, "Parameter 값 종류"); Text(parameter.Value, "Parameter 값");
            Evidence(parameter.EvidenceKindCode);
        }
        foreach (var binding in catalog.Bindings)
        {
            Text(binding.StableId, "객체 규칙 연결 식별자");
            Require(facilities.Contains(binding.FacilityStableId), "연결이 존재하지 않는 시설을 참조합니다.");
            Require(capabilities.Contains(binding.FacilityStableId + ":" + binding.CapabilityCode), "연결이 시설에 없는 기능을 참조합니다.");
            Require(rules.Contains(RuleKey(binding.RuleStableId, binding.RuleRevision)), "연결이 존재하지 않는 규칙을 참조합니다.");
            Text(binding.ScopeCode, "적용 범위"); Evidence(binding.EvidenceKindCode);
        }
        foreach (var set in catalog.ScenarioRuleSets)
        {
            Text(set.StableId, "Scenario 규칙 묶음 식별자"); Text(set.Revision, "Scenario 규칙 묶음 개정"); Text(set.AreaSetStableId, "AreaSet 식별자");
            Distinct(set.Items.Select(x => RuleKey(x.RuleStableId, x.RuleRevision)), "Scenario 규칙 항목");
            foreach (var item in set.Items)
                Require(rules.Contains(RuleKey(item.RuleStableId, item.RuleRevision)), "Scenario가 존재하지 않는 규칙을 참조합니다.");
        }
    }

    public static string ComputeHash(SimulationWorld업무규칙집결원장 catalog)
    {
        Validate(catalog);
        var text = new StringBuilder().Append(catalog.SchemaVersion).Append('|').Append(catalog.CatalogRevision)
            .Append('|').Append(catalog.SpatialBuildStableId).Append('|').Append(catalog.SpatialOutputHashSha256);
        foreach (var x in catalog.Facilities.OrderBy(x => x.StableId, StringComparer.Ordinal)) text.Append("|F:").Append(x.StableId).Append(':').Append(x.SpatialNodeStableId).Append(':').Append(x.FacilityTypeCode).Append(':').Append(x.EvidenceKindCode).Append(':').Append(x.EvidenceSourceStableId).Append(':').Append(x.ConfidenceCode).Append(':').Append(x.ScenarioAssigned);
        foreach (var x in catalog.Capabilities.OrderBy(x => x.StableId, StringComparer.Ordinal)) text.Append("|C:").Append(x.StableId).Append(':').Append(x.FacilityStableId).Append(':').Append(x.CapabilityCode).Append(':').Append(x.EvidenceKindCode);
        foreach (var x in catalog.Rules.OrderBy(RuleKey, StringComparer.Ordinal)) text.Append("|R:").Append(RuleKey(x)).Append(':').Append(x.DomainCode).Append(':').Append(x.RuleTypeCode).Append(':').Append(x.StatusCode).Append(':').Append(x.EngineKey).Append(':').Append(x.InputContractKey).Append(':').Append(x.OutputContractKey).Append(':').Append(x.Deterministic).Append(':').Append(x.SimulationOnly).Append(':').Append(x.Description);
        foreach (var x in catalog.Parameters.OrderBy(x => RuleKey(x.RuleStableId, x.RuleRevision) + ":" + x.ParameterCode, StringComparer.Ordinal)) text.Append("|P:").Append(RuleKey(x.RuleStableId, x.RuleRevision)).Append(':').Append(x.ParameterCode).Append(':').Append(x.ValueTypeCode).Append(':').Append(x.Value).Append(':').Append(x.UnitCode).Append(':').Append(x.EvidenceKindCode);
        foreach (var x in catalog.Bindings.OrderBy(x => x.StableId, StringComparer.Ordinal)) text.Append("|B:").Append(x.StableId).Append(':').Append(x.FacilityStableId).Append(':').Append(x.CapabilityCode).Append(':').Append(RuleKey(x.RuleStableId, x.RuleRevision)).Append(':').Append(x.ScopeCode).Append(':').Append(x.Priority).Append(':').Append(x.EvidenceKindCode).Append(':').Append(x.Active);
        foreach (var set in catalog.ScenarioRuleSets.OrderBy(x => x.StableId, StringComparer.Ordinal))
        {
            text.Append("|S:").Append(set.StableId).Append(':').Append(set.Revision).Append(':').Append(set.AreaSetStableId);
            foreach (var x in set.Items.OrderBy(x => x.ApplyOrder)) text.Append(':').Append(RuleKey(x.RuleStableId, x.RuleRevision)).Append('@').Append(x.ApplyOrder).Append('@').Append(x.Required);
        }
        using var sha = SHA256.Create();
        return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-", string.Empty).ToLowerInvariant();
    }

    private static string RuleKey(SimulationWorld업무Simulation규칙 x) => RuleKey(x.StableId, x.Revision);
    private static string RuleKey(string id, string revision) => id + "@" + revision;
    private static void Evidence(string value) => Require(value == SimulationWorld근거종류Codes.관측 || value == SimulationWorld근거종류Codes.파생 || value == SimulationWorld근거종류Codes.통계배분 || value == SimulationWorld근거종류Codes.시나리오 || value == SimulationWorld근거종류Codes.장식, "지원하지 않는 근거 종류입니다.");
    private static void Text(string? value, string label) => Require(!string.IsNullOrWhiteSpace(value), label + "이(가) 필요합니다.");
    private static void Sha(string value, string label) => Require(value.Length == 64 && value.All(Uri.IsHexDigit), label + " 형식이 잘못되었습니다.");
    private static void Distinct(IEnumerable<string> values, string label) { var array = values.ToArray(); Require(array.Distinct(StringComparer.Ordinal).Count() == array.Length, label + " 식별자가 중복됩니다."); }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(InvalidCode + ":" + message); }
}
}
