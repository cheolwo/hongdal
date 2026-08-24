using System;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    /// <summary>
    /// E9에서 E1로 내려가는 첫 검토 주기에서 공통으로 전달할 WI 문맥이다.
    /// 이 형식은 이름 골격이며 Simulation 상태를 변경하지 않는다.
    /// </summary>
    public sealed class 세계상호작용E단계ReviewContext
    {
        public string WorkOrderId { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string TargetStableRevision { get; set; } = string.Empty;
        public string CurrentEvidenceStage { get; set; } = string.Empty;
    }

    /// <summary>
    /// 각 모듈이 이름으로 먼저 드러낼 책임과 다음 왕복 재개방 조건이다.
    /// 실제 판정과 증거는 후속 구현에서 별도 계약으로 구체화한다.
    /// </summary>
    public sealed class 세계상호작용E단계ModuleOutline
    {
        public string EvidenceStage { get; set; } = string.Empty;
        public string ModuleTechnicalName { get; set; } = string.Empty;
        public string[] NamedResponsibilities { get; set; } = Array.Empty<string>();
        public string[] ReopenConditions { get; set; } = Array.Empty<string>();
    }

    public static class 세계상호작용ModuleTechnicalNames
    {
        public const string E9변화봉투 = "E9변화봉투Module";
        public const string E8생활연속성 = "E8생활연속성Module";
        public const string E7플레이경험폐루프 = "E7플레이경험폐루프Module";
        public const string E6세계정제 = "E6세계정제Module";
        public const string E5세계발현 = "E5세계발현Module";
        public const string E4실행문맥결속 = "E4실행문맥결속Module";
        public const string E3회귀증거 = "E3회귀증거Module";
        public const string E2실행경계 = "E2실행경계Module";
        public const string E1핵심계약 = "E1핵심계약Module";
    }

    /// <summary>
    /// 한 WI가 사용하는 실제 Preview/Confirm 실행 머리와 E9→E1 검토 모듈을
    /// 함께 가리키는 코드 대장 항목이다. 이 대장은 호출을 대신 실행하지 않는다.
    /// </summary>
    public sealed class 세계상호작용ExecutionHead
    {
        public string WorldInteractionId { get; set; } = string.Empty;
        public string PreviewMethodName { get; set; } = string.Empty;
        public string ConfirmMethodName { get; set; } = string.Empty;
        public string[] DownwardModuleTechnicalNames { get; set; }
            = Array.Empty<string>();
    }

    /// <summary>
    /// 현재 Nature·Farm 플레이 범위에서 실제 Runtime 포트가 있는 WI만 관리한다.
    /// 같은 Runtime 메서드를 공유하더라도 WI 식별자는 분리해 변경 영향과 증거를
    /// 각각 추적한다.
    /// </summary>
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "WI 실행 머리와 E9→E1 책임 연결을 안정 계약으로 관리한다.",
        Boundary = "실행 호출이나 E 증거 승격을 수행하지 않는 코드 대장이다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약)]
    public static class 세계상호작용ExecutionHeadCatalog
    {
        private static readonly string[] 단계ModuleTechnicalNames =
        {
            세계상호작용ModuleTechnicalNames.E9변화봉투,
            세계상호작용ModuleTechnicalNames.E8생활연속성,
            세계상호작용ModuleTechnicalNames.E7플레이경험폐루프,
            세계상호작용ModuleTechnicalNames.E6세계정제,
            세계상호작용ModuleTechnicalNames.E5세계발현,
            세계상호작용ModuleTechnicalNames.E4실행문맥결속,
            세계상호작용ModuleTechnicalNames.E3회귀증거,
            세계상호작용ModuleTechnicalNames.E2실행경계,
            세계상호작용ModuleTechnicalNames.E1핵심계약,
        };

        public static 세계상호작용ExecutionHead[] All { get; } = new[]
        {
            Create("WI-FARM-04",
                nameof(ISimulationFarmWorldInteractionRuntime.PreviewFarmWorkAsync),
                nameof(ISimulationFarmWorldInteractionRuntime.ConfirmFarmWorkAsync)),
            Create("WI-FARM-05",
                nameof(ISimulationFarmWorldInteractionRuntime.PreviewFarmWorkAsync),
                nameof(ISimulationFarmWorldInteractionRuntime.ConfirmFarmWorkAsync)),
            Create("WI-FARM-06",
                nameof(ISimulationFarmWorldInteractionRuntime.PreviewFarmWorkAsync),
                nameof(ISimulationFarmWorldInteractionRuntime.ConfirmFarmWorkAsync)),
            Create("WI-NATURE-01",
                nameof(ISimulationNatureWorldInteractionRuntime.PreviewNatureThreatObservationAsync),
                nameof(ISimulationNatureWorldInteractionRuntime.ConfirmNatureThreatObservationAsync)),
            Create("WI-NATURE-02",
                nameof(ISimulationNatureWorldInteractionRuntime.PreviewNatureEmergencyRetreatAsync),
                nameof(ISimulationNatureWorldInteractionRuntime.ConfirmNatureEmergencyRetreatAsync)),
            Create("WI-NATURE-03",
                nameof(ISimulationNatureWorldInteractionRuntime.PreviewNatureRestorationAsync),
                nameof(ISimulationNatureWorldInteractionRuntime.ConfirmNatureRestorationAsync)),
            Create("WI-NATURE-04",
                nameof(ISimulationNatureWorldInteractionRuntime.PreviewNaturePartyRecoveryAsync),
                nameof(ISimulationNatureWorldInteractionRuntime.ConfirmNaturePartyRecoveryAsync)),
            Create(SimulationNatureSurvivalCodes.AcquireAxeWorldInteractionId,
                nameof(ISimulationNatureSurvivalRuntime.PreviewAsync),
                nameof(ISimulationNatureSurvivalRuntime.ConfirmAsync)),
            Create(SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
                nameof(ISimulationNatureSurvivalRuntime.PreviewAsync),
                nameof(ISimulationNatureSurvivalRuntime.ConfirmAsync)),
            Create(SimulationNatureSurvivalCodes.PlaceCabinBlueprintWorldInteractionId,
                nameof(ISimulationNatureSurvivalRuntime.PreviewAsync),
                nameof(ISimulationNatureSurvivalRuntime.ConfirmAsync)),
            Create(SimulationNatureSurvivalCodes.BeginCabinBuildWorldInteractionId,
                nameof(ISimulationNatureSurvivalRuntime.PreviewAsync),
                nameof(ISimulationNatureSurvivalRuntime.ConfirmAsync)),
            Create(SimulationNatureSurvivalCodes.EnterCabinWorldInteractionId,
                nameof(ISimulationNatureSurvivalRuntime.PreviewAsync),
                nameof(ISimulationNatureSurvivalRuntime.ConfirmAsync)),
            Create(SimulationNatureSurvivalCodes.LeaveCabinWorldInteractionId,
                nameof(ISimulationNatureSurvivalRuntime.PreviewAsync),
                nameof(ISimulationNatureSurvivalRuntime.ConfirmAsync)),
            Create(SimulationNatureSurvivalCodes.ResolveEncounterWorldInteractionId,
                nameof(ISimulationNatureSurvivalRuntime.PreviewAsync),
                nameof(ISimulationNatureSurvivalRuntime.ConfirmAsync)),
            Create(SimulationNatureSurvivalCodes.CancelActiveWorkWorldInteractionId,
                nameof(ISimulationNatureSurvivalRuntime.PreviewAsync),
                nameof(ISimulationNatureSurvivalRuntime.ConfirmAsync)),
        };

        private static 세계상호작용ExecutionHead Create(
            string worldInteractionId,
            string previewMethodName,
            string confirmMethodName)
            => new 세계상호작용ExecutionHead
            {
                WorldInteractionId = worldInteractionId,
                PreviewMethodName = previewMethodName,
                ConfirmMethodName = confirmMethodName,
                DownwardModuleTechnicalNames =
                    (string[]) 단계ModuleTechnicalNames.Clone(),
            };
    }

    [SsalddelEvidenceCoverageExclusion(
        SsalddelEvidenceCoverageExclusionCategory.TechnicalHelper,
        "E1~E9 전용 모듈 인터페이스가 공유하는 기술 기반이다.")]
    public interface I세계상호작용E단계Module : IE단계Module
    {
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E9,
        "WI 변경 봉투와 하위 영향 범위를 검토한다.",
        Boundary = "변경 영향과 재검증 범위를 다루며 E9 달성을 주장하지 않는다.")]
    public interface I세계상호작용E9변화봉투Module :
        I세계상호작용E단계Module, IE9변화봉투Module
    {
        세계상호작용E단계ModuleOutline 변화봉투Review(
            세계상호작용E단계ReviewContext context);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E8,
        "WI가 NPC 생활 연속성에 미치는 책임을 검토한다.",
        Boundary = "NPC 자율 생활의 책임 자리이며 실행 증거를 대신하지 않는다.")]
    public interface I세계상호작용E8생활연속성Module :
        I세계상호작용E단계Module, IE8생활연속성Module
    {
        세계상호작용E단계ModuleOutline 생활연속성Review(
            세계상호작용E단계ReviewContext context);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E7,
        "WI의 플레이어 입력·피드백·복귀 폐루프를 검토한다.",
        Boundary = "Play Mode와 Game View 증거를 Attribute만으로 충족하지 않는다.")]
    public interface I세계상호작용E7플레이경험폐루프Module :
        I세계상호작용E단계Module, IE7플레이경험폐루프Module
    {
        세계상호작용E단계ModuleOutline 플레이경험폐루프Review(
            세계상호작용E단계ReviewContext context);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E6,
        "WI의 의미·인과·근거와 플레이 준비도를 검토한다.",
        Boundary = "현실 근거와 정제 질문의 책임 자리다.")]
    public interface I세계상호작용E6세계정제Module :
        I세계상호작용E단계Module, IE6세계정제Module
    {
        세계상호작용E단계ModuleOutline 세계정제Review(
            세계상호작용E단계ReviewContext context);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E5,
        "WI가 실제 Simulation 세계에서 발생하고 결과와 다음 WI로 이어지는지 검토한다.",
        Boundary = "공간 조립은 공간 WI의 입력 증거이며 그 자체로 E5를 주장하지 않는다.")]
    public interface I세계상호작용E5세계발현Module :
        I세계상호작용E단계Module, IE5세계발현Module
    {
        세계상호작용E단계ModuleOutline 세계발현Review(
            세계상호작용E단계ReviewContext context);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E4,
        "WI 발생원·주체·대상·자료·자원·시간과 선택적 H 공간 문맥을 결속한다.",
        Boundary = "공간이 필요하지 않은 WI에 H 결속을 강제하지 않는다.")]
    public interface I세계상호작용E4실행문맥결속Module :
        I세계상호작용E단계Module, IE4실행문맥결속Module
    {
        세계상호작용E단계ModuleOutline 실행문맥결속Review(
            세계상호작용E단계ReviewContext context);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E3,
        "WI 계약·결정성·Save/Replay 회귀 증거를 관리한다.",
        Boundary = "시험 코드 존재와 실제 상위 증거를 구분한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3계약회귀)]
    public interface I세계상호작용E3회귀증거Module :
        I세계상호작용E단계Module, IE3회귀증거Module
    {
        세계상호작용E단계ModuleOutline 회귀증거Review(
            세계상호작용E단계ReviewContext context);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "WI 공통 Core와 Local·Remote 실행 경계를 관리한다.",
        Boundary = "실행 위치와 게임 규칙을 분리한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행)]
    public interface I세계상호작용E2실행경계Module :
        I세계상호작용E단계Module, IE2실행경계Module
    {
        세계상호작용E단계ModuleOutline 실행경계Review(
            세계상호작용E단계ReviewContext context);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "WI 목적·권위·식별자·판본의 핵심 계약을 관리한다.",
        Boundary = "상위 E 모듈이 지켜야 하는 불변 경계다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약)]
    public interface I세계상호작용E1핵심계약Module :
        I세계상호작용E단계Module, IE1핵심계약Module
    {
        세계상호작용E단계ModuleOutline 핵심계약Review(
            세계상호작용E단계ReviewContext context);
    }

    /// <summary>
    /// 아홉 하향 검토 모듈의 이름만 묶는 향후 조립 지점이다.
    /// LocalSimulationRuntime과 Remote Adapter는 아직 이를 구현하지 않는다.
    /// </summary>
    [SsalddelEvidenceCoverageExclusion(
        SsalddelEvidenceCoverageExclusionCategory.TechnicalHelper,
        "아홉 책임 인터페이스를 묶는 조립 편의 타입이며 독립 E 책임은 없다.")]
    public interface I세계상호작용E단계ModuleSet :
        I세계상호작용E9변화봉투Module,
        I세계상호작용E8생활연속성Module,
        I세계상호작용E7플레이경험폐루프Module,
        I세계상호작용E6세계정제Module,
        I세계상호작용E5세계발현Module,
        I세계상호작용E4실행문맥결속Module,
        I세계상호작용E3회귀증거Module,
        I세계상호작용E2실행경계Module,
        I세계상호작용E1핵심계약Module
    {
    }
}
