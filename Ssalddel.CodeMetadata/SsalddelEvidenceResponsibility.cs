using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ssalddel.Contracts.Common.Metadata
{

public enum SsalddelEvidenceStage
{
    Unspecified = 0,
    E1 = 1,
    E2 = 2,
    E3 = 3,
    E4 = 4,
    E5 = 5,
    E6 = 6,
    E7 = 7,
    E8 = 8,
    E9 = 9,
    E10 = 10,
}

public enum SsalddelEvidenceSubjectKind
{
    Unspecified = 0,
    PlayableUnit = 1,
    AreaHarmonySet = 2,
    WorldHarmonySet = 3,
    HumanPlaytestCampaign = 4,
    LimitedOperationWindow = 5,
    LimitedOperationCandidate = LimitedOperationWindow,
}

public static class SsalddelEvidenceModelRevisions
{
    public const string LegacyChangeAdaptiveR10 = "legacy-change-adaptive.r10";
    public const string HorizontalHarmonyR1 = "horizontal-harmony-evidence.r1";
    public const string HorizontalDualCycleR2 = "horizontal-dual-cycle-evidence.r2";
    public const string Current = HorizontalDualCycleR2;
}

public sealed record SsalddelEvidenceStageHandle
{
    public SsalddelEvidenceStageHandle(
        string evidenceModelRevision,
        SsalddelEvidenceStage evidenceStage,
        SsalddelEvidenceSubjectKind subjectKind)
    {
        if (string.IsNullOrWhiteSpace(evidenceModelRevision))
            throw new ArgumentException("E 증거 모델 판본은 비어 있을 수 없습니다.",
                nameof(evidenceModelRevision));
        if (evidenceStage == SsalddelEvidenceStage.Unspecified)
            throw new ArgumentOutOfRangeException(nameof(evidenceStage));
        if (subjectKind == SsalddelEvidenceSubjectKind.Unspecified)
            throw new ArgumentOutOfRangeException(nameof(subjectKind));

        var normalizedRevision = evidenceModelRevision.Trim();
        if (string.Equals(normalizedRevision,
                SsalddelEvidenceModelRevisions.Current,
                StringComparison.Ordinal))
            ValidateCurrentSubject(evidenceStage, subjectKind);

        EvidenceModelRevision = normalizedRevision;
        EvidenceStage = evidenceStage;
        SubjectKind = subjectKind;
    }

    public string EvidenceModelRevision { get; }
    public SsalddelEvidenceStage EvidenceStage { get; }
    public SsalddelEvidenceSubjectKind SubjectKind { get; }

    private static void ValidateCurrentSubject(
        SsalddelEvidenceStage evidenceStage,
        SsalddelEvidenceSubjectKind subjectKind)
    {
        var valid = evidenceStage switch
        {
            >= SsalddelEvidenceStage.E1 and <= SsalddelEvidenceStage.E7
                => subjectKind == SsalddelEvidenceSubjectKind.PlayableUnit,
            SsalddelEvidenceStage.E8
                => subjectKind is SsalddelEvidenceSubjectKind.AreaHarmonySet
                    or SsalddelEvidenceSubjectKind.WorldHarmonySet,
            SsalddelEvidenceStage.E9
                => subjectKind ==
                    SsalddelEvidenceSubjectKind.HumanPlaytestCampaign,
            SsalddelEvidenceStage.E10
                => subjectKind ==
                    SsalddelEvidenceSubjectKind.LimitedOperationWindow,
            _ => false,
        };
        if (!valid)
            throw new ArgumentException(
                "현재 증거 모델의 E 단계와 판정 주체가 일치하지 않습니다.",
                nameof(subjectKind));
    }
}

public enum SsalddelEvidenceResponsibilityRole
{
    Primary = 0,
    Secondary = 1,
}

public enum SsalddelEvidenceCoverageExclusionCategory
{
    CompatibilityFacade = 0,
    TechnicalHelper = 1,
    GeneratedOrThirdParty = 2,
    SampleOrExperiment = 3,
    NoGameMaturityResponsibility = 4,
}

public sealed record SsalddelEvidenceStageDefinition(
    SsalddelEvidenceStage EvidenceStage,
    string ManagementSystem,
    string KoreanName,
    string TechnicalName);

public sealed record SsalddelEvidenceSubmoduleDefinition(
    string SubmoduleKey,
    SsalddelEvidenceStage EvidenceStage,
    string KoreanName,
    string TechnicalName,
    string Responsibility);

/// <summary>
/// E1~E3의 넓은 책임을 사람이 탐색 가능한 하위 모듈로 묶는 안정 key다.
/// 하위 모듈은 새로운 E 단계가 아니며 구성 요소의 대표 E 책임을 바꾸지 않는다.
/// </summary>
public static class SsalddelEvidenceSubmoduleKeys
{
    public const string E1세션권위계약 = "E1.SessionAuthorityContract";
    public const string E1세계상호작용계약 = "E1.WorldInteractionContract";
    public const string E1공간계약 = "E1.SpatialContract";
    public const string E1저장재생계약 = "E1.SaveReplayContract";
    public const string E1전투위협계약 = "E1.CombatThreatContract";

    public const string E2세션실행 = "E2.SessionExecution";
    public const string E2세계상호작용실행 = "E2.WorldInteractionExecution";
    public const string E2로컬권위Adapter = "E2.LocalAuthorityAdapter";
    public const string E2원격HostAdapter = "E2.RemoteHostAdapter";
    public const string E2Unity권위Client = "E2.UnityAuthorityClient";

    public const string E3계약회귀 = "E3.ContractRegression";
    public const string E3결정성검증 = "E3.DeterminismRegression";
    public const string E3저장재생검증 = "E3.SaveReplayRegression";
    public const string E3로컬원격동등성 = "E3.LocalRemoteParityRegression";
    public const string E3Unity소비자회귀 = "E3.UnityConsumerRegression";
}

public static class SsalddelEvidenceSubmoduleDefinitionCatalog
{
    public static IReadOnlyList<SsalddelEvidenceSubmoduleDefinition> All { get; }
        = new[]
        {
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E1세션권위계약,
                SsalddelEvidenceStage.E1, "세션 권위 계약",
                "E1세션권위계약Module",
                "Session 식별자·Revision·시간과 상태 권위의 불변 경계를 정의한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
                SsalddelEvidenceStage.E1, "세계 상호작용 계약",
                "E1세계상호작용계약Module",
                "WI 목적·StableId·허용 발생원과 Preview·Confirm 계약을 정의한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E1공간계약,
                SsalddelEvidenceStage.E1, "공간 계약",
                "E1공간계약Module",
                "H·AreaSet·Graph·Handover의 안정 식별자와 구조 계약을 정의한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E1저장재생계약,
                SsalddelEvidenceStage.E1, "저장·재생 계약",
                "E1저장재생계약Module",
                "Save schema·Command Log·Replay hash의 호환 계약을 정의한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E1전투위협계약,
                SsalddelEvidenceStage.E1, "전투·위협 계약",
                "E1전투위협계약Module",
                "전투 입력·관찰·위협 압력과 결과 경계를 정의한다."),

            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E2세션실행,
                SsalddelEvidenceStage.E2, "세션 실행",
                "E2세션실행Module",
                "Session 생성·조회·Tick·Save/Load의 공통 실행 포트를 제공한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
                SsalddelEvidenceStage.E2, "세계 상호작용 실행",
                "E2세계상호작용실행Module",
                "Farm·Nature WI Preview·Confirm 실행 포트를 제공한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E2로컬권위Adapter,
                SsalddelEvidenceStage.E2, "로컬 권위 Adapter",
                "E2로컬권위AdapterModule",
                "Solo LocalProcess에서 공통 Simulation Core를 실행한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E2원격HostAdapter,
                SsalddelEvidenceStage.E2, "원격 Host Adapter",
                "E2원격HostAdapterModule",
                "Hosted Server에서 같은 Core를 HTTP 경계로 노출한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E2Unity권위Client,
                SsalddelEvidenceStage.E2, "Unity 권위 Client",
                "E2Unity권위ClientModule",
                "Unity 입력을 Local 또는 Remote 권위 포트에 전달한다."),

            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E3계약회귀,
                SsalddelEvidenceStage.E3, "계약 회귀",
                "E3계약회귀Module",
                "StableId·요청·응답·WI metadata 계약의 회귀를 검증한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E3결정성검증,
                SsalddelEvidenceStage.E3, "결정성 검증",
                "E3결정성검증Module",
                "같은 Seed·명령·시간이 같은 canonical 상태를 만드는지 검증한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E3저장재생검증,
                SsalddelEvidenceStage.E3, "저장·재생 검증",
                "E3저장재생검증Module",
                "Save schema 호환·복원·Replay hash 회귀를 검증한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E3로컬원격동등성,
                SsalddelEvidenceStage.E3, "로컬·원격 동등성",
                "E3로컬원격동등성Module",
                "LocalProcess와 RemoteHost가 같은 권위 결과를 만드는지 검증한다."),
            new SsalddelEvidenceSubmoduleDefinition(
                SsalddelEvidenceSubmoduleKeys.E3Unity소비자회귀,
                SsalddelEvidenceStage.E3, "Unity 소비자 회귀",
                "E3Unity소비자회귀Module",
                "Unity Adapter·Projection이 권위 계약을 임의 변경하지 않는지 검증한다."),
        };

    public static SsalddelEvidenceSubmoduleDefinition? Find(string? key)
        => All.SingleOrDefault(value => string.Equals(value.SubmoduleKey,
            key?.Trim(), StringComparison.Ordinal));
}

/// <summary>
/// E1~E10을 특정 기능의 완료 선언이 아니라 반복 가능한 코드 책임으로 표현하는
/// 공통 모듈 머리다. 도메인별 모듈은 필요한 단계 인터페이스만 상속한다.
/// </summary>
[SsalddelEvidenceCoverageExclusion(
    SsalddelEvidenceCoverageExclusionCategory.TechnicalHelper,
    "E1~E10 공통 모듈 인터페이스가 공유하는 기술 기반이다.")]
public interface IE단계Module
{
    SsalddelEvidenceStage EvidenceStage { get; }
    string ModuleTechnicalName { get; }
}

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E1,
    "핵심 계약 검토 책임의 공통 모듈 이름을 제공한다.",
    Boundary = "모듈 타입 존재는 E1 증거 완료가 아니다.")]
public interface IE1핵심계약Module : IE단계Module { }
[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E2,
    "실행 경계 검토 책임의 공통 모듈 이름을 제공한다.",
    Boundary = "모듈 타입 존재는 E2 증거 완료가 아니다.")]
public interface IE2실행경계Module : IE단계Module { }
[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E3,
    "회귀 증거 검토 책임의 공통 모듈 이름을 제공한다.",
    Boundary = "모듈 타입 존재는 E3 증거 완료가 아니다.")]
public interface IE3회귀증거Module : IE단계Module { }
[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E4,
    "실행 문맥 결속 검토 책임의 공통 모듈 이름을 제공한다.",
    Boundary = "모듈 타입 존재는 E4 증거 완료가 아니다.")]
public interface IE4실행문맥결속Module : IE단계Module { }
[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E5,
    "세계 발현 검토 책임의 공통 모듈 이름을 제공한다.",
    Boundary = "모듈 타입 존재는 E5 증거 완료가 아니다.")]
public interface IE5세계발현Module : IE단계Module { }
[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E6,
    "세계 정제 검토 책임의 공통 모듈 이름을 제공한다.",
    Boundary = "모듈 타입 존재는 E6 증거 완료가 아니다.")]
public interface IE6세계정제Module : IE단계Module { }
[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E7,
    "플레이 경험 폐루프 검토 책임의 공통 모듈 이름을 제공한다.",
    Boundary = "모듈 타입 존재는 E7 증거 완료가 아니다.")]
public interface IE7플레이경험폐루프Module : IE단계Module { }
[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E8,
    "둘 이상의 E7 폐루프가 영역 안에서 조화를 이루는지 검토할 공통 모듈 이름을 제공한다.",
    Boundary = "모듈 타입 존재는 E8 증거 완료가 아니다.")]
public interface IE8영역폐루프조화Module : IE단계Module { }
[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E9,
    "E8 조화본을 사람이 반복 플레이하며 개선하는 책임의 공통 모듈 이름을 제공한다.",
    Boundary = "모듈 타입 존재는 E9 증거 완료가 아니다.")]
public interface IE9사람통합플레이개선Module : IE단계Module { }
[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E10,
    "승인된 후보 빌드의 제한 운영을 검증하는 책임의 공통 모듈 이름을 제공한다.",
    Boundary = "모듈 타입 존재는 E10 증거 완료나 운영 권한 부여가 아니다.")]
public interface IE10제한운영검증Module : IE단계Module { }

[SsalddelEvidenceResponsibility(SsalddelEvidenceStage.E8,
    "NPC 판단·이동·WI·결과·다음 판단이 영역 폐루프 사이에서 이어지는지 검토한다.",
    Role = SsalddelEvidenceResponsibilityRole.Secondary,
    Boundary = "NPC가 관련된 E8 조화 묶음의 조건 모듈이며 단독 E8 증거가 아니다.")]
[SsalddelEvidenceCoverageExclusion(
    SsalddelEvidenceCoverageExclusionCategory.CompatibilityFacade,
    "구판 E8 기술 이름을 보존하며 현재 E8에서는 NPC 조건 모듈로만 사용한다.")]
public interface IE8생활연속성Module : IE단계Module { }

[SsalddelEvidenceCoverageExclusion(
    SsalddelEvidenceCoverageExclusionCategory.CompatibilityFacade,
    "구판 legacy-change-adaptive.r10의 E9 변화 봉투 이름을 읽기 위해 보존한다.")]
public interface IE9변화봉투Module : IE단계Module { }

[SsalddelEvidenceCoverageExclusion(
    SsalddelEvidenceCoverageExclusionCategory.NoGameMaturityResponsibility,
    "Revision·Migration·Save/API 호환과 rollback을 검토하는 횡단 책임이며 E 단계가 아니다.")]
public interface I변경영향검토Module { }

/// <summary>
/// Unity Editor처럼 JSON 원장에 직접 접근하지 않는 소비자를 위한 컴파일 투영이다.
/// Hongdal 코드 지도 검증이 현재 E 책임 모듈 JSON과 모든 값을 대조한다.
/// </summary>
public static class SsalddelEvidenceStageDefinitionCatalog
{
    public static IReadOnlyList<SsalddelEvidenceStageDefinition> All { get; }
        = new[]
        {
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E1,
                "G1", "핵심 계약", "E1핵심계약Module"),
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E2,
                "G1", "실행 경계", "E2실행경계Module"),
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E3,
                "G1", "회귀 증거", "E3회귀증거Module"),
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E4,
                "G1", "실행 문맥 결속", "E4실행문맥결속Module"),
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E5,
                "G1", "세계 발현", "E5세계발현Module"),
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E6,
                "G1", "세계 정제", "E6세계정제Module"),
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E7,
                "G2", "플레이 경험 폐루프", "E7플레이경험폐루프Module"),
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E8,
                "G3", "영역 폐루프 조화", "E8영역폐루프조화Module"),
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E9,
                "G4", "사람 통합 플레이 개선", "E9사람통합플레이개선Module"),
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E10,
                "G5", "제한 운영 검증", "E10제한운영검증Module"),
        };
}

public static class SsalddelLegacyEvidenceStageDefinitionCatalog
{
    public static IReadOnlyList<SsalddelEvidenceStageDefinition> E8AndE9 { get; }
        = new[]
        {
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E8,
                "G3", "NPC 생활세계 폐루프", "E8생활연속성Module"),
            new SsalddelEvidenceStageDefinition(SsalddelEvidenceStage.E9,
                "G4", "변화 적응형 세계", "E9변화봉투Module"),
        };
}

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Interface |
    AttributeTargets.Struct | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = false)]
public sealed class SsalddelEvidenceResponsibilityAttribute : Attribute
{
    public SsalddelEvidenceResponsibilityAttribute(
        SsalddelEvidenceStage evidenceStage,
        string responsibility)
    {
        if (evidenceStage == SsalddelEvidenceStage.Unspecified)
            throw new ArgumentOutOfRangeException(nameof(evidenceStage),
                "E 책임 단계는 E1~E10 중 하나여야 합니다.");
        if (string.IsNullOrWhiteSpace(responsibility))
            throw new ArgumentException("E 책임 설명은 비어 있을 수 없습니다.",
                nameof(responsibility));

        EvidenceStage = evidenceStage;
        Responsibility = responsibility.Trim();
    }

    public SsalddelEvidenceStage EvidenceStage { get; }
    public string Responsibility { get; }
    public SsalddelEvidenceResponsibilityRole Role { get; set; }
        = SsalddelEvidenceResponsibilityRole.Primary;
    public string Boundary { get; set; } = string.Empty;
    public string SubmoduleKey { get; set; } = string.Empty;
    public string[] WorldInteractionIds { get; set; } = Array.Empty<string>();
    public string[] WorkOrderIds { get; set; } = Array.Empty<string>();
}

[AttributeUsage(
    AttributeTargets.Assembly | AttributeTargets.Class |
    AttributeTargets.Interface | AttributeTargets.Struct,
    AllowMultiple = false,
    Inherited = false)]
public sealed class SsalddelEvidenceCoverageExclusionAttribute : Attribute
{
    public SsalddelEvidenceCoverageExclusionAttribute(
        SsalddelEvidenceCoverageExclusionCategory category,
        string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("E 책임 제외 사유는 비어 있을 수 없습니다.",
                nameof(reason));

        Category = category;
        Reason = reason.Trim();
    }

    public SsalddelEvidenceCoverageExclusionCategory Category { get; }
    public string Reason { get; }
}

public sealed record SsalddelEvidenceResponsibilityDescriptor(
    Type ComponentType,
    MethodInfo? ComponentMethod,
    SsalddelEvidenceStage EvidenceStage,
    SsalddelEvidenceResponsibilityRole Role,
    string Responsibility,
    string Boundary,
    string SubmoduleKey,
    IReadOnlyList<string> WorldInteractionIds,
    IReadOnlyList<string> WorkOrderIds)
{
    public string ComponentId => ComponentMethod is null
        ? BuildTypeId(ComponentType)
        : BuildTypeId(ComponentType) + "::" + BuildMethodId(ComponentMethod);

    public string MemberKind => ComponentMethod is null ? "Type" : "Method";

    private static string BuildTypeId(Type type)
        => (type.Assembly.GetName().Name ?? string.Empty) + ":" +
           (type.FullName ?? type.Name);

    private static string BuildMethodId(MethodInfo method)
        => method.Name + "(" + string.Join(",", method.GetParameters()
            .Select(parameter => parameter.ParameterType.FullName
                ?? parameter.ParameterType.Name)) + ")";
}

public sealed record SsalddelEvidenceCoverageExclusionDescriptor(
    Assembly Assembly,
    Type? ComponentType,
    SsalddelEvidenceCoverageExclusionCategory Category,
    string Reason)
{
    public string ComponentId => ComponentType is null
        ? (Assembly.GetName().Name ?? string.Empty) + ":*"
        : (Assembly.GetName().Name ?? string.Empty) + ":" +
          (ComponentType.FullName ?? ComponentType.Name);
}

public static class SsalddelEvidenceResponsibilityReader
{
    public static IReadOnlyList<SsalddelEvidenceResponsibilityDescriptor> Read(
        params Assembly[] assemblies)
    {
        if (assemblies is null) throw new ArgumentNullException(nameof(assemblies));

        return assemblies.Where(assembly => assembly is not null).Distinct()
            .SelectMany(GetLoadableTypes)
            .SelectMany(ReadTypeAndMethods)
            .OrderBy(item => item.EvidenceStage)
            .ThenBy(item => item.Role)
            .ThenBy(item => item.ComponentId, StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyList<SsalddelEvidenceResponsibilityDescriptor> Read(
        Type componentType)
    {
        if (componentType is null) throw new ArgumentNullException(nameof(componentType));
        return ReadTypeAndMethods(componentType)
            .OrderBy(item => item.EvidenceStage)
            .ThenBy(item => item.Role)
            .ThenBy(item => item.ComponentId, StringComparer.Ordinal)
            .ToArray();
    }

    public static IReadOnlyList<SsalddelEvidenceCoverageExclusionDescriptor>
        ReadExclusions(params Assembly[] assemblies)
    {
        if (assemblies is null) throw new ArgumentNullException(nameof(assemblies));
        var result = new List<SsalddelEvidenceCoverageExclusionDescriptor>();
        foreach (var assembly in assemblies.Where(value => value is not null).Distinct())
        {
            var assemblyExclusion = assembly
                .GetCustomAttribute<SsalddelEvidenceCoverageExclusionAttribute>();
            if (assemblyExclusion is not null)
                result.Add(new SsalddelEvidenceCoverageExclusionDescriptor(
                    assembly, null, assemblyExclusion.Category,
                    assemblyExclusion.Reason));

            foreach (var type in GetLoadableTypes(assembly))
            {
                var exclusion = type
                    .GetCustomAttribute<SsalddelEvidenceCoverageExclusionAttribute>(false);
                if (exclusion is not null)
                    result.Add(new SsalddelEvidenceCoverageExclusionDescriptor(
                        assembly, type, exclusion.Category, exclusion.Reason));
            }
        }

        return result.OrderBy(item => item.ComponentId, StringComparer.Ordinal)
            .ToArray();
    }

    private static IEnumerable<SsalddelEvidenceResponsibilityDescriptor>
        ReadTypeAndMethods(Type componentType)
    {
        foreach (var attribute in componentType
                     .GetCustomAttributes<SsalddelEvidenceResponsibilityAttribute>(false))
            yield return Create(componentType, null, attribute);

        foreach (var method in componentType.GetMethods(
                     BindingFlags.Public | BindingFlags.Instance |
                     BindingFlags.Static | BindingFlags.DeclaredOnly))
        foreach (var attribute in method
                     .GetCustomAttributes<SsalddelEvidenceResponsibilityAttribute>(false))
            yield return Create(componentType, method, attribute);
    }

    private static SsalddelEvidenceResponsibilityDescriptor Create(
        Type componentType,
        MethodInfo? method,
        SsalddelEvidenceResponsibilityAttribute attribute)
        => new(
            componentType,
            method,
            attribute.EvidenceStage,
            attribute.Role,
            attribute.Responsibility,
            attribute.Boundary.Trim(),
            attribute.SubmoduleKey.Trim(),
            NormalizeIds(attribute.WorldInteractionIds),
            NormalizeIds(attribute.WorkOrderIds));

    private static IReadOnlyList<string> NormalizeIds(IEnumerable<string>? values)
        => (values ?? Array.Empty<string>())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

    internal static IReadOnlyList<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.OfType<Type>().ToArray();
        }
    }
}

public enum SsalddelEvidenceCoverageDiagnosticSeverity
{
    Warning = 0,
    Error = 1,
}

public sealed record SsalddelEvidenceCoverageDiagnostic(
    SsalddelEvidenceCoverageDiagnosticSeverity Severity,
    string Code,
    string ComponentId,
    string Message);

public static class SsalddelEvidenceCoveragePolicy
{
    private static readonly string[] CandidateTechnicalRoleSuffixes =
    {
        "Aggregate", "Runtime", "Controller", "Service", "UseCase",
        "ProcessManager", "Coordinator", "Orchestrator", "Planner",
        "Store", "Repository", "Adapter", "Client", "Mapper",
        "Presenter", "ViewModel", "View", "Builder", "Validator",
        "Policy", "Projector", "Factory", "JobShell", "Bootstrap",
        "CompositionRoot", "Module", "Tests",
    };

    public static bool IsCandidate(Type componentType)
    {
        if (componentType is null) throw new ArgumentNullException(nameof(componentType));
        if (!(componentType.IsPublic || componentType.IsNestedPublic)) return false;
        if (componentType.IsNested || componentType.IsDefined(
                typeof(CompilerGeneratedAttribute), false)) return false;
        if (componentType.GetCustomAttributes<SsalddelCodeMetadataAttribute>(false)
            .Any()) return true;

        var name = componentType.Name.Split('`')[0];
        return CandidateTechnicalRoleSuffixes.Any(suffix =>
            name.EndsWith(suffix, StringComparison.Ordinal));
    }
}

public static class SsalddelEvidenceCoverageValidator
{
    public static IReadOnlyList<SsalddelEvidenceCoverageDiagnostic> Validate(
        bool requireCoverage,
        params Assembly[] assemblies)
    {
        if (assemblies is null) throw new ArgumentNullException(nameof(assemblies));
        var selected = assemblies.Where(value => value is not null).Distinct().ToArray();
        var responsibilities = SsalddelEvidenceResponsibilityReader.Read(selected);
        var exclusions = SsalddelEvidenceResponsibilityReader.ReadExclusions(selected);
        var diagnostics = new List<SsalddelEvidenceCoverageDiagnostic>();

        foreach (var assembly in selected)
        {
            var assemblyName = assembly.GetName().Name ?? string.Empty;
            if (exclusions.Any(item => item.ComponentType is null &&
                                      item.Assembly == assembly)) continue;

            foreach (var type in SsalddelEvidenceResponsibilityReader
                         .GetLoadableTypes(assembly)
                         .Where(SsalddelEvidenceCoveragePolicy.IsCandidate))
            {
                var componentId = assemblyName + ":" + (type.FullName ?? type.Name);
                var typeResponsibilities = responsibilities.Where(item =>
                    item.ComponentType == type && item.ComponentMethod is null).ToArray();
                var exclusion = exclusions.SingleOrDefault(item =>
                    item.ComponentType == type);

                if (exclusion is not null && typeResponsibilities.Length > 0)
                    AddError(diagnostics, "EVIDENCE003", componentId,
                        "E 책임과 제외 사유를 동시에 지정할 수 없습니다.");
                if (exclusion is not null) continue;

                var primaryCount = typeResponsibilities.Count(item =>
                    item.Role == SsalddelEvidenceResponsibilityRole.Primary);
                if (primaryCount == 0)
                {
                    diagnostics.Add(new SsalddelEvidenceCoverageDiagnostic(
                        requireCoverage
                            ? SsalddelEvidenceCoverageDiagnosticSeverity.Error
                            : SsalddelEvidenceCoverageDiagnosticSeverity.Warning,
                        "EVIDENCE001", componentId,
                        "대표 E 책임 또는 사유 있는 제외가 없습니다."));
                }
                else if (primaryCount > 1)
                    AddError(diagnostics, "EVIDENCE002", componentId,
                        "대표 E 책임은 정확히 하나여야 합니다.");

                foreach (var duplicate in typeResponsibilities.GroupBy(item =>
                             new { item.EvidenceStage, item.Role })
                         .Where(group => group.Count() > 1))
                    AddError(diagnostics, "EVIDENCE004", componentId,
                        "같은 E 단계와 역할의 책임이 중복되었습니다.");

                ValidateResponsibilities(typeResponsibilities, componentId,
                    diagnostics);
            }
        }

        foreach (var methodGroup in responsibilities
                     .Where(item => item.ComponentMethod is not null)
                     .GroupBy(item => item.ComponentId, StringComparer.Ordinal))
        {
            if (methodGroup.Count(item => item.Role ==
                    SsalddelEvidenceResponsibilityRole.Primary) != 1)
                AddError(diagnostics, "EVIDENCE005", methodGroup.Key,
                    "메서드 E 책임에는 대표 책임이 정확히 하나 필요합니다.");
            ValidateResponsibilities(methodGroup.ToArray(), methodGroup.Key,
                diagnostics);
        }

        return diagnostics
            .OrderBy(item => item.Severity)
            .ThenBy(item => item.Code, StringComparer.Ordinal)
            .ThenBy(item => item.ComponentId, StringComparer.Ordinal)
            .ToArray();
    }

    private static void ValidateResponsibilities(
        IReadOnlyList<SsalddelEvidenceResponsibilityDescriptor> responsibilities,
        string componentId,
        ICollection<SsalddelEvidenceCoverageDiagnostic> diagnostics)
    {
        foreach (var responsibility in responsibilities)
        {
            if (responsibility.Role == SsalddelEvidenceResponsibilityRole.Primary &&
                string.IsNullOrWhiteSpace(responsibility.Boundary))
                AddError(diagnostics, "EVIDENCE006", componentId,
                    "대표 E 책임에는 Boundary가 필요합니다.");
            if (responsibility.WorldInteractionIds.Any(value =>
                    !value.StartsWith("WI-", StringComparison.Ordinal)))
                AddError(diagnostics, "EVIDENCE007", componentId,
                    "WorldInteractionIds에는 WI- 고유 식별자만 사용할 수 있습니다.");
            if (!string.IsNullOrWhiteSpace(responsibility.SubmoduleKey))
            {
                var submodule = SsalddelEvidenceSubmoduleDefinitionCatalog.Find(
                    responsibility.SubmoduleKey);
                if (submodule is null)
                    AddError(diagnostics, "EVIDENCE008", componentId,
                        "알 수 없는 E 하위 모듈 key다.");
                else if (submodule.EvidenceStage != responsibility.EvidenceStage)
                    AddError(diagnostics, "EVIDENCE009", componentId,
                        "E 책임 단계와 하위 모듈 단계가 일치하지 않는다.");
            }
        }
    }

    private static void AddError(
        ICollection<SsalddelEvidenceCoverageDiagnostic> diagnostics,
        string code,
        string componentId,
        string message)
        => diagnostics.Add(new SsalddelEvidenceCoverageDiagnostic(
            SsalddelEvidenceCoverageDiagnosticSeverity.Error,
            code,
            componentId,
            message));
}

}
