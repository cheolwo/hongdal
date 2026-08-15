using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Ssalddel.Simulation.Domain
{
public static class SimulationWorldUI화면종류Codes
{
    public const string 선택정보판 = "SelectionPanel";
    public const string 업무상세판 = "TaskDetailPanel";
    public const string WorldHud = "WorldHud";
}

public static class SimulationWorldUI행동종류Codes
{
    public const string 조회 = "Inspect";
    public const string 미리보기 = "Preview";
    public const string 확정 = "Confirm";
}

public static class SimulationWorldUI관점Codes
{
    public const string Simulation참여자 = "SimulationParticipant";
}

public static class SimulationWorldUI규칙연결목적Codes
{
    public const string 상태설명과행동제안 = "ExplainStateAndOfferAction";
}

public static class SimulationWorldUI상태Codes
{
    public const string 대기 = "Idle";
    public const string 불러오는중 = "Loading";
    public const string 준비 = "Ready";
    public const string 미리보기준비 = "PreviewReady";
    public const string 진행중 = "InProgress";
    public const string 완료 = "Completed";
    public const string 차단 = "Blocked";
    public const string 오류 = "Error";
}

public static class SimulationWorldUI역할Codes
{
    public const string 공동체 = "Community";
    public const string 주문자 = "Orderer";
    public const string 화주 = "Shipper";
    public const string 기사 = "Driver";
    public const string 창고관리자 = "Warehouse";
    public const string 음식점 = "Restaurant";
}

public sealed class SimulationWorldUI설계근거
{
    public string StableId { get; set; } = string.Empty;
    public string ProviderCode { get; set; } = string.Empty;
    public string FileKey { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string KoreanTitle { get; set; } = string.Empty;
    public string ObservedStructureCode { get; set; } = string.Empty;
    public DateTimeOffset ObservedAtUtc { get; set; }
}

public sealed class SimulationWorldUI화면영역기획
{
    public string StableId { get; set; } = string.Empty;
    public string FacilityStableId { get; set; } = string.Empty;
    public string SurfaceKindCode { get; set; } = string.Empty;
    public string PerspectiveCode { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string WorkflowStageCode { get; set; } = string.Empty;
    public string KoreanTitle { get; set; } = string.Empty;
    public string AnchorSemanticCode { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool DefaultVisible { get; set; }
    public string DesignEvidenceStableId { get; set; } = string.Empty;
}

public sealed class SimulationWorldUI정보항목기획
{
    public string StableId { get; set; } = string.Empty;
    public string SurfaceStableId { get; set; } = string.Empty;
    public string InformationKindCode { get; set; } = string.Empty;
    public string KoreanLabel { get; set; } = string.Empty;
    public string ValueSemanticCode { get; set; } = string.Empty;
    public string SourceContractKey { get; set; } = string.Empty;
    public string FormatCode { get; set; } = string.Empty;
    public string? UnitCode { get; set; }
    public int Priority { get; set; }
    public bool ProvenanceRequired { get; set; }
}

public sealed class SimulationWorldUI상태표현기획
{
    public string StableId { get; set; } = string.Empty;
    public string SurfaceStableId { get; set; } = string.Empty;
    public string StateCode { get; set; } = string.Empty;
    public string KoreanLabel { get; set; } = string.Empty;
    public string SeverityCode { get; set; } = string.Empty;
    public string PresentationIntentCode { get; set; } = string.Empty;
    public bool BlocksMutationActions { get; set; }
    public int DisplayOrder { get; set; }
}

public sealed class SimulationWorldUI행동후보기획
{
    public string StableId { get; set; } = string.Empty;
    public string SurfaceStableId { get; set; } = string.Empty;
    public string ActionKindCode { get; set; } = string.Empty;
    public string KoreanLabel { get; set; } = string.Empty;
    public string CapabilityKey { get; set; } = string.Empty;
    public string? ServerCommandKey { get; set; }
    public bool RequiresPreview { get; set; }
    public bool RequiresExplicitConfirmation { get; set; }
    public bool RequiresExpectedRevision { get; set; }
    public bool SimulationOnly { get; set; } = true;
    public int DisplayOrder { get; set; }
}

public sealed class SimulationWorldUI업무규칙연결
{
    public string StableId { get; set; } = string.Empty;
    public string BusinessRuleBindingStableId { get; set; } = string.Empty;
    public string FacilityCapabilityCode { get; set; } = string.Empty;
    public string RuleStableId { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public string SurfaceStableId { get; set; } = string.Empty;
    public string PurposeCode { get; set; } = string.Empty;
    public int Priority { get; set; }
}

public sealed class SimulationWorldUI기획원장
{
    public int SchemaVersion { get; set; } = 1;
    public string CatalogRevision { get; set; } = string.Empty;
    public string BusinessRuleCatalogRevision { get; set; } = string.Empty;
    public string BusinessRuleCatalogHashSha256 { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public IReadOnlyList<SimulationWorldUI설계근거> DesignEvidence { get; set; } = Array.Empty<SimulationWorldUI설계근거>();
    public IReadOnlyList<SimulationWorldUI화면영역기획> Surfaces { get; set; } = Array.Empty<SimulationWorldUI화면영역기획>();
    public IReadOnlyList<SimulationWorldUI정보항목기획> InformationItems { get; set; } = Array.Empty<SimulationWorldUI정보항목기획>();
    public IReadOnlyList<SimulationWorldUI상태표현기획> StatePresentations { get; set; } = Array.Empty<SimulationWorldUI상태표현기획>();
    public IReadOnlyList<SimulationWorldUI행동후보기획> ActionCandidates { get; set; } = Array.Empty<SimulationWorldUI행동후보기획>();
    public IReadOnlyList<SimulationWorldUI업무규칙연결> RuleBindings { get; set; } = Array.Empty<SimulationWorldUI업무규칙연결>();
}

public static class SimulationWorldUI기획Validator
{
    public const string InvalidCode = "SimulationWorldUiPlanningCatalogInvalid";
    public const int CurrentSchemaVersion = 2;

    public static void Validate(SimulationWorldUI기획원장 plan, SimulationWorld업무규칙집결원장 businessRules)
    {
        if (plan == null) throw new ArgumentNullException(nameof(plan));
        if (businessRules == null) throw new ArgumentNullException(nameof(businessRules));
        Require(plan.SchemaVersion == CurrentSchemaVersion, "지원하지 않는 UI 기획 schema입니다.");
        Text(plan.CatalogRevision, "UI 기획 개정");
        Text(plan.BusinessRuleCatalogRevision, "업무 규칙 대장 개정");
        Sha(plan.BusinessRuleCatalogHashSha256, "업무 규칙 대장 SHA-256");
        Require(plan.CreatedAtUtc != default, "생성 시각이 필요합니다.");
        Require(string.Equals(plan.BusinessRuleCatalogRevision, businessRules.CatalogRevision, StringComparison.Ordinal), "업무 규칙 대장 개정이 일치하지 않습니다.");
        Require(string.Equals(plan.BusinessRuleCatalogHashSha256, SimulationWorld업무규칙집결Validator.ComputeHash(businessRules), StringComparison.OrdinalIgnoreCase), "업무 규칙 대장 hash가 일치하지 않습니다.");

        Distinct(plan.Surfaces.Select(x => x.StableId), "UI 화면 영역");
        Distinct(plan.DesignEvidence.Select(x => x.StableId), "UI 설계 근거");
        Distinct(plan.InformationItems.Select(x => x.StableId), "UI 정보 항목");
        Distinct(plan.StatePresentations.Select(x => x.StableId), "UI 상태 표현");
        Distinct(plan.ActionCandidates.Select(x => x.StableId), "UI 행동 후보");
        Distinct(plan.RuleBindings.Select(x => x.StableId), "UI 업무 규칙 연결");
        Distinct(plan.InformationItems.Select(x => x.SurfaceStableId + ":" + x.InformationKindCode), "UI 영역별 정보 종류");
        Distinct(plan.StatePresentations.Select(x => x.SurfaceStableId + ":" + x.StateCode), "UI 영역별 상태 표현");
        Distinct(plan.ActionCandidates.Select(x => x.SurfaceStableId + ":" + x.ActionKindCode), "UI 영역별 행동 종류");
        Distinct(plan.RuleBindings.Select(x => x.BusinessRuleBindingStableId), "원본 객체 업무 규칙 연결 참조");

        var facilities = new HashSet<string>(businessRules.Facilities.Select(x => x.StableId), StringComparer.Ordinal);
        var evidence = new HashSet<string>(plan.DesignEvidence.Select(x => x.StableId), StringComparer.Ordinal);
        var rules = new HashSet<string>(businessRules.Rules.Select(x => RuleKey(x.StableId, x.Revision)), StringComparer.Ordinal);
        var surfaces = new HashSet<string>(plan.Surfaces.Select(x => x.StableId), StringComparer.Ordinal);
        var surfaceById = plan.Surfaces.ToDictionary(x => x.StableId, StringComparer.Ordinal);
        var sourceBindingById = businessRules.Bindings.ToDictionary(x => x.StableId, StringComparer.Ordinal);
        foreach (var source in plan.DesignEvidence)
        {
            Text(source.StableId, "설계 근거 식별자");
            Text(source.ProviderCode, "설계 근거 제공자"); Text(source.FileKey, "Figma 파일 키");
            Text(source.NodeId, "Figma node 식별자"); Text(source.KoreanTitle, "설계 근거 제목");
            Text(source.ObservedStructureCode, "관측 구조 코드"); Require(source.ObservedAtUtc != default, "설계 근거 확인 시각이 필요합니다.");
        }
        foreach (var surface in plan.Surfaces)
        {
            Text(surface.StableId, "UI 화면 영역 식별자");
            Require(facilities.Contains(surface.FacilityStableId), "UI 영역이 존재하지 않는 시설을 참조합니다.");
            Text(surface.SurfaceKindCode, "화면 종류"); Text(surface.PerspectiveCode, "관점");
            Text(surface.RoleCode, "역할"); Text(surface.WorkflowStageCode, "업무 단계");
            Text(surface.KoreanTitle, "화면 제목"); Text(surface.AnchorSemanticCode, "공간 anchor 의미");
            Require(surface.DisplayOrder > 0, "UI 화면 표시 순서는 1 이상이어야 합니다.");
            Require(evidence.Contains(surface.DesignEvidenceStableId), "UI 영역이 존재하지 않는 설계 근거를 참조합니다.");
        }
        foreach (var item in plan.InformationItems)
        {
            Text(item.StableId, "정보 항목 식별자");
            Require(surfaces.Contains(item.SurfaceStableId), "정보 항목이 존재하지 않는 UI 영역을 참조합니다.");
            Text(item.InformationKindCode, "정보 종류"); Text(item.KoreanLabel, "정보 이름");
            Text(item.ValueSemanticCode, "값 의미"); Text(item.SourceContractKey, "원본 계약 키"); Text(item.FormatCode, "표시 형식");
            Require(item.Priority > 0, "정보 항목 우선순위는 1 이상이어야 합니다.");
        }
        foreach (var state in plan.StatePresentations)
        {
            Text(state.StableId, "상태 표현 식별자");
            Require(surfaces.Contains(state.SurfaceStableId), "상태 표현이 존재하지 않는 UI 영역을 참조합니다.");
            Text(state.StateCode, "상태 코드"); Text(state.KoreanLabel, "상태 이름");
            Text(state.SeverityCode, "심각도"); Text(state.PresentationIntentCode, "표현 의도");
            Require(state.DisplayOrder > 0, "상태 표현 순서는 1 이상이어야 합니다.");
        }
        foreach (var action in plan.ActionCandidates)
        {
            Text(action.StableId, "행동 후보 식별자");
            Require(surfaces.Contains(action.SurfaceStableId), "행동 후보가 존재하지 않는 UI 영역을 참조합니다.");
            Text(action.ActionKindCode, "행동 종류"); Text(action.KoreanLabel, "행동 이름"); Text(action.CapabilityKey, "기능 키");
            Require(action.DisplayOrder > 0, "행동 후보 표시 순서는 1 이상이어야 합니다.");
            Require(action.SimulationOnly, "첫 UI 기획에는 Simulation 전용 행동만 허용합니다.");
            if (action.ActionKindCode == SimulationWorldUI행동종류Codes.확정)
            {
                Text(action.ServerCommandKey, "확정 Command 키");
                Require(action.RequiresPreview && action.RequiresExplicitConfirmation && action.RequiresExpectedRevision,
                    "확정 행동은 Preview, 명시적 확인과 기대 개정 번호가 모두 필요합니다.");
                Require(plan.ActionCandidates.Any(x => x.SurfaceStableId == action.SurfaceStableId && x.ActionKindCode == SimulationWorldUI행동종류Codes.미리보기),
                    "확정 행동이 있는 UI 영역에는 Preview 행동이 필요합니다.");
            }
            else Require(string.IsNullOrWhiteSpace(action.ServerCommandKey), "조회·Preview 행동은 확정 Command를 소유할 수 없습니다.");
        }
        foreach (var binding in plan.RuleBindings)
        {
            Text(binding.StableId, "UI 업무 규칙 연결 식별자");
            Require(rules.Contains(RuleKey(binding.RuleStableId, binding.RuleRevision)), "UI 연결이 존재하지 않는 업무 규칙을 참조합니다.");
            Require(surfaces.Contains(binding.SurfaceStableId), "UI 연결이 존재하지 않는 영역을 참조합니다.");
            Text(binding.BusinessRuleBindingStableId, "원본 객체 업무 규칙 연결 식별자");
            Text(binding.FacilityCapabilityCode, "시설 기능 코드");
            Text(binding.PurposeCode, "UI 규칙 연결 목적");
            Require(binding.Priority > 0, "UI 업무 규칙 연결 우선순위는 1 이상이어야 합니다.");
            Require(sourceBindingById.TryGetValue(binding.BusinessRuleBindingStableId, out var sourceBinding),
                "UI 연결이 존재하지 않는 원본 객체 업무 규칙 연결을 참조합니다.");
            var surface = surfaceById[binding.SurfaceStableId];
            Require(sourceBinding.Active, "UI에는 비활성 업무 규칙 연결을 사용할 수 없습니다.");
            Require(sourceBinding.FacilityStableId == surface.FacilityStableId, "UI 영역의 시설과 업무 규칙 연결의 시설이 일치하지 않습니다.");
            Require(sourceBinding.CapabilityCode == binding.FacilityCapabilityCode, "UI 연결의 시설 기능이 원본 업무 규칙 연결과 일치하지 않습니다.");
            Require(sourceBinding.RuleStableId == binding.RuleStableId && sourceBinding.RuleRevision == binding.RuleRevision,
                "UI 연결의 규칙과 원본 업무 규칙 연결이 일치하지 않습니다.");
        }

        foreach (var surface in plan.Surfaces)
        {
            Require(plan.InformationItems.Any(x => x.SurfaceStableId == surface.StableId), "UI 영역에는 정보 항목이 하나 이상 필요합니다.");
            Require(plan.StatePresentations.Any(x => x.SurfaceStableId == surface.StableId), "UI 영역에는 상태 표현이 하나 이상 필요합니다.");
            Require(plan.ActionCandidates.Any(x => x.SurfaceStableId == surface.StableId), "UI 영역에는 행동 후보가 하나 이상 필요합니다.");
            Require(plan.RuleBindings.Any(x => x.SurfaceStableId == surface.StableId), "UI 영역에는 업무 규칙 연결이 하나 이상 필요합니다.");
        }

        var activeSourceBindings = new HashSet<string>(businessRules.Bindings.Where(x => x.Active).Select(x => x.StableId), StringComparer.Ordinal);
        var uiSourceBindings = new HashSet<string>(plan.RuleBindings.Select(x => x.BusinessRuleBindingStableId), StringComparer.Ordinal);
        Require(activeSourceBindings.SetEquals(uiSourceBindings), "활성 객체 업무 규칙 연결은 UI 기획에 빠짐없이 한 번씩 연결되어야 합니다.");
    }

    public static string ComputeHash(SimulationWorldUI기획원장 plan, SimulationWorld업무규칙집결원장 businessRules)
    {
        Validate(plan, businessRules);
        var text = new StringBuilder().Append(plan.SchemaVersion).Append('|').Append(plan.CatalogRevision)
            .Append('|').Append(plan.BusinessRuleCatalogRevision).Append('|').Append(plan.BusinessRuleCatalogHashSha256);
        foreach (var x in plan.DesignEvidence.OrderBy(x => x.StableId, StringComparer.Ordinal)) text.Append("|D:").Append(x.StableId).Append(':').Append(x.ProviderCode).Append(':').Append(x.FileKey).Append(':').Append(x.NodeId).Append(':').Append(x.KoreanTitle).Append(':').Append(x.ObservedStructureCode).Append(':').Append(x.ObservedAtUtc.ToUniversalTime().ToString("O"));
        foreach (var x in plan.Surfaces.OrderBy(x => x.StableId, StringComparer.Ordinal)) text.Append("|S:").Append(x.StableId).Append(':').Append(x.FacilityStableId).Append(':').Append(x.SurfaceKindCode).Append(':').Append(x.PerspectiveCode).Append(':').Append(x.RoleCode).Append(':').Append(x.WorkflowStageCode).Append(':').Append(x.KoreanTitle).Append(':').Append(x.AnchorSemanticCode).Append(':').Append(x.DisplayOrder).Append(':').Append(x.DefaultVisible).Append(':').Append(x.DesignEvidenceStableId);
        foreach (var x in plan.InformationItems.OrderBy(x => x.StableId, StringComparer.Ordinal)) text.Append("|I:").Append(x.StableId).Append(':').Append(x.SurfaceStableId).Append(':').Append(x.InformationKindCode).Append(':').Append(x.KoreanLabel).Append(':').Append(x.ValueSemanticCode).Append(':').Append(x.SourceContractKey).Append(':').Append(x.FormatCode).Append(':').Append(x.UnitCode).Append(':').Append(x.Priority).Append(':').Append(x.ProvenanceRequired);
        foreach (var x in plan.StatePresentations.OrderBy(x => x.StableId, StringComparer.Ordinal)) text.Append("|P:").Append(x.StableId).Append(':').Append(x.SurfaceStableId).Append(':').Append(x.StateCode).Append(':').Append(x.KoreanLabel).Append(':').Append(x.SeverityCode).Append(':').Append(x.PresentationIntentCode).Append(':').Append(x.BlocksMutationActions).Append(':').Append(x.DisplayOrder);
        foreach (var x in plan.ActionCandidates.OrderBy(x => x.StableId, StringComparer.Ordinal)) text.Append("|A:").Append(x.StableId).Append(':').Append(x.SurfaceStableId).Append(':').Append(x.ActionKindCode).Append(':').Append(x.KoreanLabel).Append(':').Append(x.CapabilityKey).Append(':').Append(x.ServerCommandKey).Append(':').Append(x.RequiresPreview).Append(':').Append(x.RequiresExplicitConfirmation).Append(':').Append(x.RequiresExpectedRevision).Append(':').Append(x.SimulationOnly).Append(':').Append(x.DisplayOrder);
        foreach (var x in plan.RuleBindings.OrderBy(x => x.StableId, StringComparer.Ordinal)) text.Append("|B:").Append(x.StableId).Append(':').Append(x.BusinessRuleBindingStableId).Append(':').Append(x.FacilityCapabilityCode).Append(':').Append(x.RuleStableId).Append('@').Append(x.RuleRevision).Append(':').Append(x.SurfaceStableId).Append(':').Append(x.PurposeCode).Append(':').Append(x.Priority);
        using (var sha = SHA256.Create()) return ToHex(sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString())));
    }

    private static string RuleKey(string id, string revision) => id + "@" + revision;
    private static void Distinct(IEnumerable<string> values, string name) { var list = values.ToArray(); Require(list.Length == new HashSet<string>(list, StringComparer.Ordinal).Count, name + " 식별자가 중복됩니다."); }
    private static void Text(string? value, string name) => Require(!string.IsNullOrWhiteSpace(value), name + " 값이 필요합니다.");
    private static void Sha(string value, string name) => Require(value != null && value.Length == 64 && value.All(Uri.IsHexDigit), name + " 형식이 올바르지 않습니다.");
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(InvalidCode + ":" + message); }
    private static string ToHex(byte[] bytes) { var builder = new StringBuilder(bytes.Length * 2); foreach (var value in bytes) builder.Append(value.ToString("x2")); return builder.ToString(); }
}
}
