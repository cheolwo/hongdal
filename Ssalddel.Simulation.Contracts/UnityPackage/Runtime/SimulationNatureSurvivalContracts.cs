using System;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Simulation.Contracts
{
    public static class SimulationNatureSleepProtectionSpatialLayerCandidateCodes
    {
        public const string SchemaVersion =
            "nature-sleep-protection-spatial-layer-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string SleepSurfaceLayer = "SleepSurfaceLayer";
        public const string ShelterInteriorLayer = "ShelterInteriorLayer";
        public const string HeatInfluenceLayer = "HeatInfluenceLayer";
        public const string PhysicalPerimeterLayer =
            "PhysicalPerimeterLayer";
        public const string HigherThreatBoundaryLayer =
            "HigherThreatBoundaryLayer";
        public const string SpatialPolicyRevisionRequired =
            "SpatialPolicyRevisionRequired";
        public const string WeatherProfileCandidateRequired =
            "WeatherProfileCandidateRequired";
        public const string RequiredLayerMissing = "RequiredLayerMissing";
        public const string LayerStableIdRequired = "LayerStableIdRequired";
        public const string PlacementStableIdRequired =
            "PlacementStableIdRequired";
        public const string GeometryRevisionRequired =
            "GeometryRevisionRequired";
        public const string BoundaryGraphRevisionRequired =
            "BoundaryGraphRevisionRequired";
        public const string LayerStableIdDuplicated =
            "LayerStableIdDuplicated";
        public const string RangeAndShapeUnresolved =
            "RangeAndShapeUnresolved";
        public const string DoorOpeningPolicyUnresolved =
            "DoorOpeningPolicyUnresolved";
        public const string CompleteBoundaryGraphRuleUnresolved =
            "CompleteBoundaryGraphRuleUnresolved";

        public static string[] RequiredLayerRoleCodes() => new[]
        {
            SleepSurfaceLayer,
            ShelterInteriorLayer,
            HeatInfluenceLayer,
            PhysicalPerimeterLayer,
            HigherThreatBoundaryLayer,
        };
    }

    public sealed class SimulationNatureSleepProtectionSpatialLayerDefinition
    {
        public string LayerStableId { get; set; } = string.Empty;
        public string LayerRoleCode { get; set; } = string.Empty;
        public string PlacementStableId { get; set; } = string.Empty;
        public string GeometryRevision { get; set; } = string.Empty;
        public string BoundaryGraphRevision { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureSleepProtectionSpatialLayerCandidateRequest
    {
        public SimulationNatureWeatherProfileFreezeCandidateSnapshot
            WeatherProfileCandidate { get; set; } =
                new SimulationNatureWeatherProfileFreezeCandidateSnapshot();
        public string SpatialPolicyRevision { get; set; } = string.Empty;
        public SimulationNatureSleepProtectionSpatialLayerDefinition[] Layers
            { get; set; } =
                Array.Empty<SimulationNatureSleepProtectionSpatialLayerDefinition>();
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "침상·오두막 실내·열원 영향권·울타리 물리 경계·마법진 상위 위협 경계를 중첩 가능한 수면 보호 공간층으로 전달한다.",
        StepKey = "contract.nature-sleep-protection-spatial-layer-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 16,
        Boundary = "공간층 역할과 배치·형상·Graph 근거 요구만 정의하며 실제 범위·좌표·충돌·보호 결과를 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q025 Nature 수면 보호를 다섯 중첩 공간층과 배치·형상·경계 Graph 근거로 분리한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "기획 후보이며 실제 H 배치·Collider·Bounds·Graph·Runtime 증거가 아니다.")]
    public sealed class SimulationNatureSleepProtectionSpatialLayerCandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                .SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                .PlanningCandidate;
        public string ReadinessCode { get; set; } =
            SimulationNatureSleepProtectionSpatialLayerCandidateCodes.Gap;
        public string SpatialPolicyRevision { get; set; } = string.Empty;
        public SimulationNatureSleepProtectionSpatialLayerDefinition[] Layers
            { get; set; } =
                Array.Empty<SimulationNatureSleepProtectionSpatialLayerDefinition>();
        public string[] MissingRequirementCodes { get; set; } =
            Array.Empty<string>();
        public string[] MissingLayerRoleCodes { get; set; } =
            Array.Empty<string>();
        public bool SupportsOverlappingLayers { get; set; } = true;
        public bool UsesPlacementAndGeometryEvidence { get; set; } = true;
        public bool UsesBoundaryGraphEvidence { get; set; } = true;
        public bool AppliesSpatialProtection { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                .RangeAndShapeUnresolved,
            SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                .DoorOpeningPolicyUnresolved,
            SimulationNatureSleepProtectionSpatialLayerCandidateCodes
                .CompleteBoundaryGraphRuleUnresolved,
        };
    }

    public static class SimulationNatureWeatherProfileFreezeCandidateCodes
    {
        public const string SchemaVersion =
            "nature-weather-profile-freeze-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string NewWorldBoundary = "NewWorldBoundary";
        public const string GameDayStartBoundary = "GameDayStartBoundary";
        public const string PublicObservationSource =
            "PublicObservationSource";
        public const string GameClimateFixtureSource =
            "GameClimateFixtureSource";
        public const string RiskySleepOutcomeCandidateRequired =
            "RiskySleepOutcomeCandidateRequired";
        public const string FreezeBoundaryRequired =
            "FreezeBoundaryRequired";
        public const string SourceTypeRequired = "SourceTypeRequired";
        public const string SourceSnapshotHashRequired =
            "SourceSnapshotHashRequired";
        public const string ObservationQualityApprovalRequired =
            "ObservationQualityApprovalRequired";
        public const string GeneralizationRuleRevisionRequired =
            "GeneralizationRuleRevisionRequired";
        public const string WeatherProfileCodeRequired =
            "WeatherProfileCodeRequired";
        public const string SaveReplayBindingRequired =
            "SaveReplayBindingRequired";
        public const string UnavailableFallbackProfileUnresolved =
            "UnavailableFallbackProfileUnresolved";
    }

    public sealed class SimulationNatureWeatherProfileFreezeCandidateRequest
    {
        public SimulationNatureRiskySleepOutcomeCandidateSnapshot
            RiskySleepOutcomeCandidate { get; set; } =
                new SimulationNatureRiskySleepOutcomeCandidateSnapshot();
        public string FreezeBoundaryCode { get; set; } = string.Empty;
        public string GameDayStableId { get; set; } = string.Empty;
        public string SourceTypeCode { get; set; } = string.Empty;
        public string SourceSnapshotHashSha256 { get; set; } = string.Empty;
        public bool ObservationQualityApproved { get; set; }
        public string GeneralizationRuleRevision { get; set; } = string.Empty;
        public string WeatherProfileCode { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "품질 승인 관측을 새 세계·하루 시작 경계에서 일반화된 날씨 Profile과 출처 hash·규칙 판본으로 봉인하는 후보를 전달한다.",
        StepKey = "contract.nature-weather-profile-freeze-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SharedPublicData,
        FlowOrder = 15,
        Boundary = "외부 API 응답을 플레이 중 직접 반영하지 않으며 실제 수집·품질 승인·Save 판본·Sky 표현을 수행하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q024 기상 관측을 새 세계·하루 경계의 날씨 Profile과 출처 계보로 동결하는 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "기획 후보이며 실제 Provider 호출·Save/Replay·Sky·Runtime 증거가 아니다.")]
    public sealed class SimulationNatureWeatherProfileFreezeCandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            SimulationNatureWeatherProfileFreezeCandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            SimulationNatureWeatherProfileFreezeCandidateCodes
                .PlanningCandidate;
        public string ReadinessCode { get; set; } =
            SimulationNatureWeatherProfileFreezeCandidateCodes.Gap;
        public string FreezeBoundaryCode { get; set; } = string.Empty;
        public string GameDayStableId { get; set; } = string.Empty;
        public string SourceTypeCode { get; set; } = string.Empty;
        public string SourceSnapshotHashSha256 { get; set; } = string.Empty;
        public string GeneralizationRuleRevision { get; set; } = string.Empty;
        public string WeatherProfileCode { get; set; } = string.Empty;
        public string[] MissingRequirementCodes { get; set; } =
            Array.Empty<string>();
        public bool FrozenForGameDay { get; set; }
        public bool AllowsMidDayExternalMutation { get; set; }
        public bool RequiresSourceLineageInSave { get; set; } = true;
        public bool AppliesWeatherProfile { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            SimulationNatureWeatherProfileFreezeCandidateCodes
                .SaveReplayBindingRequired,
            SimulationNatureWeatherProfileFreezeCandidateCodes
                .UnavailableFallbackProfileUnresolved,
        };
    }

    public static class SimulationNatureRiskySleepOutcomeCandidateCodes
    {
        public const string SchemaVersion =
            "nature-risky-sleep-outcome-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string AnimalApproach = "AnimalApproach";
        public const string MonsterApproach = "MonsterApproach";
        public const string ColdExposure = "ColdExposure";
        public const string PrecipitationExposure =
            "PrecipitationExposure";
        public const string FatigueWakeOutcome = "FatigueWakeOutcome";
        public const string TemperatureWakeOutcome =
            "TemperatureWakeOutcome";
        public const string DiseaseRiskWakeOutcome =
            "DiseaseRiskWakeOutcome";
        public const string CombatOrRetreatChoice =
            "CombatOrRetreatChoice";
        public const string SleepSafetyCandidateRequired =
            "SleepSafetyCandidateRequired";
        public const string WeatherInputRevisionRequired =
            "WeatherInputRevisionRequired";
        public const string WeatherProfileBindingOwnedByQ024 =
            "WeatherProfileBindingOwnedByQ024";
        public const string MultipleThreatPriorityUnresolved =
            "MultipleThreatPriorityUnresolved";
        public const string WakeOutcomeNumericRulesUnresolved =
            "WakeOutcomeNumericRulesUnresolved";

        public static string[] OrderedThreatApproachCodes() => new[]
        {
            AnimalApproach,
            MonsterApproach,
        };
    }

    public sealed class SimulationNatureRiskySleepOutcomeCandidateRequest
    {
        public SimulationNatureSleepSafetyCandidateSnapshot SleepSafetyCandidate
            { get; set; } = new SimulationNatureSleepSafetyCandidateSnapshot();
        public bool AnimalApproachDetected { get; set; }
        public bool MonsterApproachDetected { get; set; }
        public bool ColdExposureDetected { get; set; }
        public bool PrecipitationExposureDetected { get; set; }
        public bool DiseaseRiskAccumulated { get; set; }
        public string WeatherInputRevision { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "위험 수면 중 동물·몬스터 접근은 강제 각성과 전투·후퇴 선택으로, 추위·강수·질병 위험은 기상 결과로 분리해 전달한다.",
        StepKey = "contract.nature-risky-sleep-outcome-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 14,
        Boundary = "중단·누적 결과 종류만 정의하며 실제 전투 생성·피로·체온·질병 변경이나 기상청 자료 동결을 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q023 위험 수면의 위협 접근 중단과 날씨·질병 기상 결과 후보를 분리한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-14", "WI-NATURE-11" },
        Boundary = "기획 후보이며 실제 수면 중단·전투/후퇴·신체 상태·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class SimulationNatureRiskySleepOutcomeCandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            SimulationNatureRiskySleepOutcomeCandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            SimulationNatureRiskySleepOutcomeCandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            SimulationNatureRiskySleepOutcomeCandidateCodes.Gap;
        public string WeatherInputRevision { get; set; } = string.Empty;
        public string[] MissingRequirementCodes { get; set; } =
            Array.Empty<string>();
        public string[] ThreatApproachCodes { get; set; } =
            Array.Empty<string>();
        public string[] AccumulatedWakeOutcomeCodes { get; set; } =
            Array.Empty<string>();
        public bool InterruptsSleepForThreatApproach { get; set; }
        public bool ReturnsCombatOrRetreatChoice { get; set; }
        public bool DefersEnvironmentalOutcomeUntilWake { get; set; }
        public bool AppliesSleepInterruption { get; set; }
        public bool AppliesWakeOutcome { get; set; }
        public bool ChangesWorldState { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; } = new[]
        {
            SimulationNatureRiskySleepOutcomeCandidateCodes
                .WeatherProfileBindingOwnedByQ024,
            SimulationNatureRiskySleepOutcomeCandidateCodes
                .MultipleThreatPriorityUnresolved,
            SimulationNatureRiskySleepOutcomeCandidateCodes
                .WakeOutcomeNumericRulesUnresolved,
        };
    }

    public static class SimulationNatureExpertThreatCandidateCodes
    {
        public const string SchemaVersion =
            "simulation-nature-expert-threat-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string SpawnFrequency = "SpawnFrequency";
        public const string GroupSize = "GroupSize";
        public const string IndividualAbility = "IndividualAbility";
        public const string FocusRequirement = "FocusRequirement";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string FocusInsufficiencyOutcomeUnresolved =
            "FocusInsufficiencyOutcomeUnresolved";
        public const string ThreatRewardScalingUnresolved =
            "ThreatRewardScalingUnresolved";
        public const string ProgressionCouplingUnresolved =
            "ProgressionCouplingUnresolved";

        public static string[] RequiredIntensityDimensionCodes()
            => new[] { SpawnFrequency, GroupSize, IndividualAbility };

        public static string[] UnresolvedDecisionCodes()
            => new[]
            {
                FocusInsufficiencyOutcomeUnresolved,
                ThreatRewardScalingUnresolved,
                ProgressionCouplingUnresolved,
            };
    }

    public sealed class SimulationNatureThreatIntensityDimensionRevision
    {
        public string DimensionCode { get; set; } = string.Empty;
        public string RuleRevision { get; set; } = string.Empty;
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "숙련자 위협의 빈도·무리 규모·개별 능력 강화와 기존 집중 체계 결속 후보를 전달한다.",
        StepKey = "contract.nature-expert-threat-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 13,
        Boundary = "강화 차원과 기존 집중 Profile 결속을 기술하며 수치·보상·집중 부족 결과를 확정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q005 숙련자 위협 강화 세 축과 명상·집중 체계 재사용 후보 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-11", "WI-NATURE-14" },
        Boundary = "기획 후보이며 실제 Spawn·전투 보정·집중 소비·Runtime 증거가 아니다.")]
    public sealed class SimulationNatureExpertThreatCandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            SimulationNatureExpertThreatCandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            SimulationNatureExpertThreatCandidateCodes.PlanningCandidate;
        public string ReadinessCode { get; set; } =
            SimulationNatureExpertThreatCandidateCodes.Gap;
        public SimulationNatureThreatIntensityDimensionRevision[]
            IntensityDimensionRevisions { get; set; }
            = Array.Empty<SimulationNatureThreatIntensityDimensionRevision>();
        public string[] MissingRequirementCodes { get; set; }
            = SimulationNatureExpertThreatCandidateCodes
                .RequiredIntensityDimensionCodes();
        public string FocusProfileCatalogRevision { get; set; } =
            Simulation집중판정Codes.FocusProfileCatalogRevision;
        public string FocusRequirementRevision { get; set; } = string.Empty;
        public bool ReusesExistingMeditationSystem { get; set; } = true;
        public bool ChangesBaseWorldInteractionOutcome { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; }
            = SimulationNatureExpertThreatCandidateCodes
                .UnresolvedDecisionCodes();
    }

    public static class SimulationNatureDifficultyBoundaryCodes
    {
        public const string SchemaVersion =
            "simulation-nature-difficulty-boundary.v1";
        public const string StandardWarningInformation =
            "StandardWarningInformation";
        public const string ReducedWarningInformation =
            "ReducedWarningInformation";
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "Nature 난이도의 공통 수면 판정식과 별도 위협 출몰 Profile 경계를 전달한다.",
        StepKey = "contract.nature-difficulty-boundary",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 12,
        Boundary = "난이도는 같은 주변 상태의 수면 안전 공식을 바꾸지 않고 출몰 입력과 경고 정보량만 선택한다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q004 공통 수면 판정식과 모드별 Spawn Profile·경고 정보 경계를 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "Profile 선택 계약이며 실제 출몰 빈도·Save/Replay·Runtime 증거가 아니다.")]
    public sealed class SimulationNatureDifficultyBoundarySnapshot
    {
        public string SchemaVersion { get; set; } =
            SimulationNatureDifficultyBoundaryCodes.SchemaVersion;
        public string DifficultyCode { get; set; } =
            SimulationNatureRiskySleepWarningCodes.Normal;
        public string SleepSafetyFormulaRevision { get; set; } = string.Empty;
        public bool UsesSharedSleepSafetyFormula { get; set; } = true;
        public string SelectedSpawnProfileRevision { get; set; } =
            string.Empty;
        public bool IncreasedThreatExposure { get; set; }
        public string WarningInformationLevelCode { get; set; } =
            SimulationNatureDifficultyBoundaryCodes
                .StandardWarningInformation;
        public bool ChangesCurrentSafetyForSameInputs { get; set; }
    }

    public static class SimulationNatureRiskySleepWarningCodes
    {
        public const string SchemaVersion =
            "simulation-nature-risky-sleep-warning.v1";
        public const string Beginner = "Beginner";
        public const string Normal = "Normal";
        public const string Expert = "Expert";
        public const string UseModeDefault = "UseModeDefault";
        public const string AlwaysShow = "AlwaysShow";
        public const string NeverShow = "NeverShow";
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "위험 수면 허용과 경고 표시의 난이도·사용자 설정 경계를 전달한다.",
        StepKey = "contract.nature-risky-sleep-warning",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 11,
        Boundary = "경고 가시성은 정보 표현이며 수면 안전 판정이나 실제 위험도를 변경하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q003 위험 수면 허용과 모드별 경고 기본값·사용자 설정 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "계약 정의이며 실제 Preview UI·수면 결과·Runtime 증거가 아니다.")]
    public sealed class SimulationNatureRiskySleepWarningSnapshot
    {
        public string SchemaVersion { get; set; } =
            SimulationNatureRiskySleepWarningCodes.SchemaVersion;
        public string DifficultyCode { get; set; } =
            SimulationNatureRiskySleepWarningCodes.Normal;
        public string PreferenceCode { get; set; } =
            SimulationNatureRiskySleepWarningCodes.UseModeDefault;
        public bool RiskDetected { get; set; }
        public string[] WarningReasonCodes { get; set; } =
            Array.Empty<string>();
        public bool WarningVisible { get; set; }
        public bool SleepSelectionAllowed { get; set; } = true;
        public bool ChangesAuthoritySafetyJudgement { get; set; }
    }

    public static class SimulationNatureSleepSafetyCandidateCodes
    {
        public const string SchemaVersion =
            "simulation-nature-sleep-safety-candidate.v1";
        public const string PlanningCandidate = "PlanningCandidate";
        public const string Temperate = "Temperate";
        public const string AnimalThreat = "AnimalThreat";
        public const string MonsterThreat = "MonsterThreat";
        public const string Cabin = "Cabin";
        public const string Fire = "Fire";
        public const string Fence = "Fence";
        public const string MagicCircle = "MagicCircle";
        public const string Ready = "Ready";
        public const string Gap = "Gap";
        public const string DiseaseIncrementBoundsRequired =
            "DiseaseIncrementBoundsRequired";
        public const string SleepPermissionPolicyUnresolved =
            "SleepPermissionPolicyUnresolved";
        public const string FireFuelCostUnresolved =
            "FireFuelCostUnresolved";
        public const string DiseaseOnsetRecoveryUnresolved =
            "DiseaseOnsetRecoveryUnresolved";

        public static string[] UnresolvedDecisionCodes()
            => new[]
            {
                SleepPermissionPolicyUnresolved,
                FireFuelCostUnresolved,
                DiseaseOnsetRecoveryUnresolved,
            };
    }

    public sealed class SimulationNatureSleepProtectionRequirement
    {
        public string RequirementStableId { get; set; } = string.Empty;
        public string[] AlternativeProtectionCodes { get; set; }
            = Array.Empty<string>();
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "Nature 수면 안전 단계의 기획 후보와 구현 공백을 전달한다.",
        StepKey = "contract.nature-sleep-safety-candidate",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 10,
        Boundary = "Q002의 합성 후보를 확정 규칙과 구분하며 수면 허용·연료·질병 결과를 결정하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q002 오두막·불·울타리·마법진의 단계적 수면 안전 후보 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "기획 후보 계약이며 실제 수면 판정·상태 변경·Runtime 증거가 아니다.")]
    public sealed class SimulationNatureSleepSafetyCandidateSnapshot
    {
        public string SchemaVersion { get; set; } =
            SimulationNatureSleepSafetyCandidateCodes.SchemaVersion;
        public string DecisionStatusCode { get; set; } =
            SimulationNatureSleepSafetyCandidateCodes.PlanningCandidate;
        public string SituationCode { get; set; } = string.Empty;
        public string ReadinessCode { get; set; } =
            SimulationNatureSleepSafetyCandidateCodes.Gap;
        public SimulationNatureSleepProtectionRequirement[]
            ProtectionRequirements { get; set; }
            = Array.Empty<SimulationNatureSleepProtectionRequirement>();
        public string[] AvailableProtectionCodes { get; set; }
            = Array.Empty<string>();
        public string[] MissingRequirementStableIds { get; set; }
            = Array.Empty<string>();
        public bool DiseaseIncrementBoundsDefined { get; set; }
        public int DiseaseRiskIncrementMinimum { get; set; }
        public int DiseaseRiskIncrementMaximum { get; set; }
        public string[] UnresolvedDecisionCodes { get; set; }
            = SimulationNatureSleepSafetyCandidateCodes
                .UnresolvedDecisionCodes();
    }

    public static class SimulationNatureShelterPurposeCodes
    {
        public const string SchemaVersion =
            "simulation-nature-shelter-purpose.v1";
        public const string SafeSleep = "SafeSleep";
        public const string TemperatureStability = "TemperatureStability";
        public const string FatigueRecovery = "FatigueRecovery";
        public const string DiseaseRiskReduction = "DiseaseRiskReduction";
        public const string Storage = "Storage";
        public const string Ready = "Ready";
        public const string Gap = "Gap";

        public static string[] CoreBenefitCodes()
            => new[]
            {
                TemperatureStability,
                FatigueRecovery,
                DiseaseRiskReduction,
            };

        public static string[] SecondaryBenefitCodes()
            => new[] { Storage };
    }

    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationWorldDerivation,
        SsalddelCodeLayer.Contract,
        "Nature 오두막의 1차 목적과 핵심·보조 효용의 구현 준비 상태를 전달한다.",
        StepKey = "contract.nature-shelter-purpose",
        ExecutionStage = SsalddelCodeExecutionStage.Definition,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 9,
        Boundary = "안전한 수면을 1차 목적으로 고정하며 보관 용량을 체온·피로·질병 규칙의 대체 증거로 사용하지 않는다.")]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E1,
        "Q001 오두막의 안전한 수면 목적과 핵심·보조 효용 계약을 정의한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E1세계상호작용계약,
        WorldInteractionIds = new[] { "WI-NATURE-14" },
        Boundary = "계약 정의는 체온·피로·질병 수치 적용이나 실제 수면 Runtime 증거가 아니다.")]
    public sealed class SimulationNatureShelterPurposeReadinessSnapshot
    {
        public string SchemaVersion { get; set; } =
            SimulationNatureShelterPurposeCodes.SchemaVersion;
        public string PrimaryPurposeCode { get; set; } =
            SimulationNatureShelterPurposeCodes.SafeSleep;
        public bool CabinOperational { get; set; }
        public bool LegacyRecoverySignalAvailable { get; set; }
        public string CoreBenefitReadinessCode { get; set; } =
            SimulationNatureShelterPurposeCodes.Gap;
        public string[] RequiredCoreBenefitCodes { get; set; }
            = SimulationNatureShelterPurposeCodes.CoreBenefitCodes();
        public string[] ImplementedCoreBenefitCodes { get; set; }
            = Array.Empty<string>();
        public string[] MissingCoreBenefitCodes { get; set; }
            = SimulationNatureShelterPurposeCodes.CoreBenefitCodes();
        public string[] SecondaryBenefitCodes { get; set; }
            = SimulationNatureShelterPurposeCodes.SecondaryBenefitCodes();
    }

    public static class SimulationNatureSurvivalCodes
    {
        public const string ProfileRevisionR1 = "nature-survival.realtime.r1";
        public const string ProfileRevisionR2 = "nature-survival.realtime.r2";
        public const string ProfileRevisionR3 = "nature-survival.realtime.r3";
        public const string ProfileRevisionR4 = "nature-survival.realtime.r4";
        public const string ProfileRevisionR5 = "nature-survival.realtime.r5";
        public const string ProfileRevision = ProfileRevisionR1;
        public const string AreaSetStableId = "area-set:sim:pyeongchang:nature-home.v1";
        public const string HomeH3StableId = "h3-candidate:nature-home-encounter-defense";
        public const string HomeH2StableId = "h2-candidate:nature-home-core";
        public const string HarvestH2StableId = "h2-candidate:nature-encounter-route";
        public const string SafeClearingH1StableId = "h1-stock:nature-trailhead";
        public const string CabinSiteH1StableId = "h1-stock:nature-shelter";
        public const string AxeItemCode = "tool:axe.basic";
        public const string AxePickupStableId = "pickup:nature-safe-clearing:basic-axe";
        public const string TimberItemCode = "material:timber-log";
        public const string RebuildPartItemCode = "material:rebuild-part";
        public const string NatureFieldSupplyPackItemCode =
            "supply:nature-field-pack";
        public const string CabinStorageContainerStableId = "container:nature-cabin:storage";
        public const string CabinStorageTimberStackStableId =
            "item-stack:nature-cabin:timber";
        public const string UnitEach = "EA";
        public const string SkeletonPlaceholderCode = "placeholder:synty-generic-skeleton";

        public const string BeginHarvest = "BeginHarvest";
        public const string AcquireAxe = "AcquireAxe";
        public const string PlaceCabinBlueprint = "PlaceCabinBlueprint";
        public const string BeginCabinBuild = "BeginCabinBuild";
        public const string ResolveEncounter = "ResolveEncounter";
        public const string EnterCabin = "EnterCabin";
        public const string LeaveCabin = "LeaveCabin";
        public const string CancelActiveWork = "CancelActiveWork";
        public const string StoreAtCabin = "StoreAtCabin";
        public const string SleepInCabin = "SleepInCabin";
        public const string SelectExpansionPlan = "SelectExpansionPlan";
        public const string BeginBuildingConstruction =
            Simulation영역건물발전Codes.BeginBuildingConstruction;
        public const string PrepareFieldSupply = "PrepareFieldSupply";
        public const string PrepareFieldSupplyDelegated =
            "PrepareFieldSupplyDelegated";
        public const string CollectDroppedTimber = "CollectDroppedTimber";

        public const string AcquireAxeWorldInteractionId = "WI-NATURE-05";
        public const string BeginHarvestWorldInteractionId = "WI-NATURE-06";
        public const string PlaceCabinBlueprintWorldInteractionId = "WI-NATURE-07";
        public const string BeginCabinBuildWorldInteractionId = "WI-NATURE-08";
        public const string EnterCabinWorldInteractionId = "WI-NATURE-09";
        public const string LeaveCabinWorldInteractionId = "WI-NATURE-10";
        public const string ResolveEncounterWorldInteractionId = "WI-NATURE-11";
        public const string CancelActiveWorkWorldInteractionId = "WI-NATURE-12";
        public const string StoreAtCabinWorldInteractionId = "WI-NATURE-13";
        public const string SleepInCabinWorldInteractionId = "WI-NATURE-14";
        public const string SelectExpansionPlanWorldInteractionId = "WI-NATURE-15";
        public const string PrepareFieldSupplyWorldInteractionId = "WI-NATURE-16";
        public const string PrepareFieldSupplyDelegatedWorldInteractionId =
            "WI-NATURE-17";
        public const string CollectDroppedTimberWorldInteractionId =
            "WI-NATURE-18";
        public const string TacticalSelfNavigationPlayableLoopStableId =
            "playable-loop:nature-tactical-self-navigation.v1";
        public const string ShelterFoundationPlayableLoopStableId =
            "playable-loop:nature-shelter-foundation.v1";
        public const string TwilightReturnPlayableLoopStableId =
            "playable-loop:nature-twilight-return.v1";
        public const string NightDay2PlayableLoopStableId =
            "playable-loop:nature-night-day2.v1";
        public const string BuildingLearningPlayableLoopStableId =
            "playable-loop:nature-building-learning.v1";
        public const string FieldSupplyReturnPlayableLoopStableId =
            "playable-loop:nature-field-supply-return.v1";
        public const string Fight = "Fight";
        public const string Retreat = "Retreat";
        public const string Victory = "Victory";
        public const string Defeat = "Defeat";
        public const string CombatActive = "CombatActive";
        public const string Sleeping = "Sleeping";
        public const string Workbench = "Workbench";
        public const string StorageRack = "StorageRack";
        public const string Palisade = "Palisade";
        public const string None = "None";
        public const string Harvest = "Harvest";
        public const string CabinBuild = "CabinBuild";
        public const string FieldSupplyCraft = "FieldSupplyCraft";
        public const string FieldSupplyNpcCraft = "FieldSupplyNpcCraft";
        public const string UseFieldSupplyPack = "UseFieldSupplyPack";
        public const int FieldSupplyTimberCost = 2;
        public const int FieldSupplyRebuildPartCost = 1;
        public const int FieldSupplyCraftSeconds = 4;
        public const string Standing = "Standing";
        public const string Stump = "Stump";
        public const string Planned = "Planned";
        public const string Building = "Building";
        public const string Completed = "Completed";
        public const string Pending = "Pending";
        public const string Resolved = "Resolved";
        public const string Menu = "Menu";
        public const string ApplicationInactive = "ApplicationInactive";
        public const string DroppedTimberAvailable = "Available";
        public const string DroppedTimberCollected = "Collected";

        public const string Disabled = "SimulationNatureSurvivalDisabled";
        public const string ExpectedRevisionMismatch = "SimulationExpectedRevisionMismatch";
        public const string ActionBlocked = "SimulationNatureSurvivalActionBlocked";
        public const string ResourceNodeNotFound = "SimulationNatureResourceNodeNotFound";
        public const string ResourceNodeUnavailable = "SimulationNatureResourceNodeUnavailable";
        public const string AxeRequired = "SimulationNatureAxeRequired";
        public const string TimberInsufficient = "SimulationNatureTimberInsufficient";
        public const string CabinBlueprintRequired = "SimulationNatureCabinBlueprintRequired";
        public const string EncounterNotPending = "SimulationNatureEncounterNotPending";
        public const string CommandPayloadConflict = "SimulationCommandPayloadConflict";
        public const string DurationExceeded = "SimulationDurationExceeded";
        public const string ActiveWorkRequired = "SimulationNatureActiveWorkRequired";
        public const string CabinRequired = "SimulationNatureCabinRequired";
        public const string CabinAccessRequired = "SimulationNatureCabinAccessRequired";
        public const string CabinStorageFull = "SimulationNatureCabinStorageFull";
        public const string TimberNotCarried = "SimulationNatureTimberNotCarried";
        public const string CabinStoredResourceRequired =
            "SimulationNatureCabinStoredResourceRequired";
        public const string NightRequired = "SimulationNatureNightRequired";
        public const string CombatActiveClockFrozen =
            "SimulationNatureCombatActiveClockFrozen";
        public const string ExpansionPlanInvalid = "SimulationNatureExpansionPlanInvalid";
        public const string ExpansionPlanAlreadySelected =
            "SimulationNatureExpansionPlanAlreadySelected";
        public const string SpatialEvidenceUnavailable =
            "SimulationNatureSpatialEvidenceUnavailable";
        public const string WorkbenchRequired =
            "SimulationNatureWorkbenchRequired";
        public const string FieldSupplyTimberInsufficient =
            "SimulationNatureFieldSupplyTimberInsufficient";
        public const string FieldSupplyRebuildPartInsufficient =
            "SimulationNatureFieldSupplyRebuildPartInsufficient";
        public const string FieldSupplyPackRequired =
            "SimulationNatureFieldSupplyPackRequired";
        public const string ExpeditionAlreadyPrepared =
            "SimulationNatureExpeditionAlreadyPrepared";
        public const string NpcRoutineNatureRevisionRequired =
            "SimulationNpcRoutineNatureRevisionRequired";
        public const string FieldSupplyAlreadyAvailable =
            "SimulationNatureFieldSupplyAlreadyAvailable";
        public const string DroppedTimberNotFound =
            "SimulationNatureDroppedTimberNotFound";
        public const string DroppedTimberUnavailable =
            "SimulationNatureDroppedTimberUnavailable";

        /// <summary>
        /// 플레이어가 명시적으로 선택하고 권위 상태를 바꾸는 Nature 생존 행동을
        /// 정식 WI 식별자로 정규화한다. 시간 경과와 작업 진행은 WI가 아니다.
        /// </summary>
        public static string WorldInteractionIdForAction(string actionCode)
            => actionCode switch
            {
                AcquireAxe => AcquireAxeWorldInteractionId,
                BeginHarvest => BeginHarvestWorldInteractionId,
                PlaceCabinBlueprint => PlaceCabinBlueprintWorldInteractionId,
                BeginCabinBuild => BeginCabinBuildWorldInteractionId,
                EnterCabin => EnterCabinWorldInteractionId,
                LeaveCabin => LeaveCabinWorldInteractionId,
                ResolveEncounter => ResolveEncounterWorldInteractionId,
                CancelActiveWork => CancelActiveWorkWorldInteractionId,
                StoreAtCabin => StoreAtCabinWorldInteractionId,
                SleepInCabin => SleepInCabinWorldInteractionId,
                SelectExpansionPlan => SelectExpansionPlanWorldInteractionId,
                BeginBuildingConstruction =>
                    Simulation영역건물발전Codes.ConstructionWorldInteractionId,
                PrepareFieldSupply => PrepareFieldSupplyWorldInteractionId,
                PrepareFieldSupplyDelegated =>
                    PrepareFieldSupplyDelegatedWorldInteractionId,
                CollectDroppedTimber => CollectDroppedTimberWorldInteractionId,
                _ => string.Empty,
            };

        /// <summary>
        /// Nature 행동의 권위 행위 기록과 Unity 표현 Trace가 같은
        /// PlayableLoop 주제를 사용하도록 단일 결속을 제공합니다.
        /// </summary>
        public static string PlayableLoopStableIdForAction(string actionCode)
            => actionCode switch
            {
                AcquireAxe => TacticalSelfNavigationPlayableLoopStableId,
                BeginHarvest or PlaceCabinBlueprint or BeginCabinBuild or
                    EnterCabin or LeaveCabin or CancelActiveWork or
                    CollectDroppedTimber => ShelterFoundationPlayableLoopStableId,
                ResolveEncounter => TwilightReturnPlayableLoopStableId,
                StoreAtCabin or SleepInCabin or SelectExpansionPlan =>
                    NightDay2PlayableLoopStableId,
                BeginBuildingConstruction => BuildingLearningPlayableLoopStableId,
                PrepareFieldSupply or PrepareFieldSupplyDelegated =>
                    FieldSupplyReturnPlayableLoopStableId,
                _ => string.Empty,
            };

        public static string PlayerActivityTrackCodeForAction(string actionCode)
            => actionCode switch
            {
                AcquireAxe or BeginHarvest or ResolveEncounter or
                    CollectDroppedTimber =>
                    Simulation플레이어활동경로Codes.FieldExpedition,
                StoreAtCabin or SleepInCabin or SelectExpansionPlan =>
                    Simulation플레이어활동경로Codes.AreaOperation,
                PlaceCabinBlueprint or BeginCabinBuild or
                    BeginBuildingConstruction or PrepareFieldSupply =>
                    Simulation플레이어활동경로Codes.AreaManufacturing,
                PrepareFieldSupplyDelegated =>
                    Simulation플레이어활동경로Codes.AreaOperation,
                _ => Simulation플레이어활동경로Codes.AreaOperation,
            };

        public static string PlayerFlowCodeForAction(string actionCode)
            => actionCode switch
            {
                BeginHarvest or ResolveEncounter =>
                    Simulation플레이흐름Codes.발산,
                PlaceCabinBlueprint or BeginCabinBuild or
                    BeginBuildingConstruction or PrepareFieldSupply =>
                    Simulation플레이흐름Codes.순환연결부,
                _ => Simulation플레이흐름Codes.수렴,
            };

        public static string NextPlayerFlowCodeForAction(string actionCode)
            => PlayerFlowCodeForAction(actionCode) ==
                Simulation플레이흐름Codes.발산
                ? Simulation플레이흐름Codes.수렴
                : Simulation플레이흐름Codes.발산;

        public static string CycleHandoffCodeForAction(string actionCode)
            => PlayerFlowCodeForAction(actionCode) ==
                Simulation플레이흐름Codes.발산
                ? Simulation플레이흐름인계Codes.발산에서수렴
                : Simulation플레이흐름인계Codes.수렴에서발산;

        public static bool IsR2(string profileRevision)
            => string.Equals(profileRevision?.Trim(), ProfileRevisionR2,
                   StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR3,
                    StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR4,
                    StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR5,
                    StringComparison.Ordinal);

        public static bool IsR3(string profileRevision)
            => string.Equals(profileRevision?.Trim(), ProfileRevisionR3,
                   StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR4,
                   StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR5,
                   StringComparison.Ordinal);

        public static bool IsR4(string profileRevision)
            => string.Equals(profileRevision?.Trim(), ProfileRevisionR4,
                   StringComparison.Ordinal)
               || string.Equals(profileRevision?.Trim(), ProfileRevisionR5,
                   StringComparison.Ordinal);

        public static bool IsR5(string profileRevision)
            => string.Equals(profileRevision?.Trim(), ProfileRevisionR5,
                StringComparison.Ordinal);

        public static string ActualE5SpatialStableId(string worldInteractionId)
            => "spatial:actual-e5:" + (worldInteractionId ?? string.Empty)
                .Trim().ToLowerInvariant();
    }

    public static class Simulation플레이어활동경로Codes
    {
        public const string FieldExpedition = "FieldExpedition";
        public const string AreaOperation = "AreaOperation";
        public const string AreaManufacturing = "AreaManufacturing";
    }

    public static class Simulation플레이흐름Codes
    {
        public const string 발산 = "Outward";
        public const string 수렴 = "Inward";
        public const string 순환연결부 = "TransformationBridge";
    }

    public static class Simulation플레이흐름인계Codes
    {
        public const string 발산에서수렴 = "OutwardToInward";
        public const string 수렴에서발산 = "InwardToOutward";
    }

    public sealed class SimulationNatureSurvivalInitialStateRequest
    {
        public string ProfileRevision { get; set; } = SimulationNatureSurvivalCodes.ProfileRevision;
        public string PlayerStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = SimulationNatureSurvivalCodes.AreaSetStableId;
        public string H3StableId { get; set; } = SimulationNatureSurvivalCodes.HomeH3StableId;
        public string SpawnH2StableId { get; set; } = SimulationNatureSurvivalCodes.HomeH2StableId;
        public string SpawnH1StableId { get; set; } = SimulationNatureSurvivalCodes.SafeClearingH1StableId;
        public decimal InventoryCapacityUnits { get; set; } = 24m;
        public bool StartsWithAxe { get; set; } = true;
        public string FocusAccessibilityModeCode { get; set; }
            = Simulation집중판정Codes.Standard;
        public SimulationNatureResourceNodeInitialStateRequest[] ResourceNodes { get; set; }
            = Array.Empty<SimulationNatureResourceNodeInitialStateRequest>();
        public SimulationNatureCooperativeActorInitialStateRequest[]
            CooperativeActors { get; set; }
            = Array.Empty<SimulationNatureCooperativeActorInitialStateRequest>();
        public Simulation영역건물발전CatalogSnapshot? BuildingProgressionCatalog
            { get; set; }
    }

    public sealed class SimulationNatureCooperativeActorInitialStateRequest
    {
        public string ActorStableId { get; set; } = string.Empty;
        public decimal InventoryCapacityUnits { get; set; } = 24m;
        public long RegisteredWorldRevision { get; set; }
    }

    public sealed class SimulationNatureResourceNodeInitialStateRequest
    {
        public string ResourceNodeStableId { get; set; } = string.Empty;
        public string H2StableId { get; set; } = SimulationNatureSurvivalCodes.HarvestH2StableId;
        public string H1StableId { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
    }

    public sealed class SimulationNatureSurvivalActionPreviewRequest
    {
        public long ObservedWorldRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ChoiceCode { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public double YawDegrees { get; set; }
    }

    public sealed class SimulationNatureSurvivalCommandRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public string PlayerStableId { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string ChoiceCode { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public double YawDegrees { get; set; }
        /// <summary>
        /// 전투 권위가 직접 개입 성과를 판정한 뒤 내부 인계에만 사용하는 보상량이다.
        /// 일반 Unity/API 명령에서는 항상 0이어야 한다.
        /// </summary>
        public int AuthoritativeRewardBonusQuantity { get; set; }
    }

    /// <summary>
    /// 현재 입력 프레임을 결정적 정수 초로 환산한 명령이다. 실제 벽시계 timestamp를
    /// 저장하지 않으므로 종료 중 경과 시간은 따라잡지 않는다.
    /// </summary>
    public sealed class SimulationNatureSurvivalClockAdvanceRequest
    {
        public string CommandId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public int ElapsedRealtimeSeconds { get; set; }
        public bool WorkInputHeld { get; set; }
        public string PauseReasonCode { get; set; } = string.Empty;
    }

    public sealed class SimulationNatureSurvivalActionPreviewSnapshot
    {
        public string SessionStableId { get; set; } = string.Empty;
        public long WorldRevision { get; set; }
        public string WorldInteractionId { get; set; } = string.Empty;
        public string WorldInteractionName { get; set; } = string.Empty;
        public string WorldInteractionDisplayName { get; set; } = string.Empty;
        public string ResponsibilityKindCode { get; set; } = string.Empty;
        public string PrimaryOutcomeCode { get; set; } = string.Empty;
        public string SingleResponsibilityAssessmentCode { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string PlayerActivityTrackCode { get; set; } = string.Empty;
        public string PlayerFlowCode { get; set; } = string.Empty;
        public string NextPlayerFlowCode { get; set; } = string.Empty;
        public string CycleHandoffCode { get; set; } = string.Empty;
        public bool CanConfirm { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public int RequiredTimberQuantity { get; set; }
        public int AvailableTimberQuantity { get; set; }
        public int RequiredWorkSeconds { get; set; }
        public int TransferableTimberQuantity { get; set; }
        public int CabinStoredTimberQuantity { get; set; }
        public int CabinStorageCapacity { get; set; }
        public int RequiredRebuildPartQuantity { get; set; }
        public int AvailableRebuildPartQuantity { get; set; }
        public int TargetDroppedTimberQuantity { get; set; }
        public decimal RemainingInventoryCapacityUnits { get; set; }
        public string BuildingBlueprintStableId { get; set; } = string.Empty;
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
        public string SpatialEvidenceStateCode { get; set; }
            = SimulationWorldInteractionSpatialEvidenceCodes.RequiredMissing;
        public string[] SpatialEvidenceReferenceIds { get; set; }
            = Array.Empty<string>();
    }

    public sealed class SimulationNatureSurvivalStateSnapshot
    {
        public bool IsEnabled { get; set; }
        public string ProfileRevision { get; set; } = string.Empty;
        public string PlayerStableId { get; set; } = string.Empty;
        public string AreaSetStableId { get; set; } = string.Empty;
        public string H3StableId { get; set; } = string.Empty;
        public string CurrentH2StableId { get; set; } = string.Empty;
        public string CurrentH1StableId { get; set; } = string.Empty;
        public int CycleIndex { get; set; }
        public int ElapsedSecondsInCycle { get; set; }
        public string ClockPhaseCode { get; set; } = string.Empty;
        public bool ClockPaused { get; set; }
        public string PauseReasonCode { get; set; } = string.Empty;
        public bool HasAxe { get; set; }
        public int TimberQuantity { get; set; }
        public int StoredTimberQuantity { get; set; }
        public int NoiseEventCount { get; set; }
        public int RawThreatTier { get; set; }
        public int EffectiveThreatTier { get; set; }
        public int RebuildPartQuantity { get; set; }
        public int FieldSupplyPackQuantity { get; set; }
        public bool ExpeditionPrepared { get; set; }
        public string LastProtectedMaterialItemCode { get; set; } = string.Empty;
        public string LinkedCombatStableId { get; set; } = string.Empty;
        public string LastCombatResultCode { get; set; } = string.Empty;
        public bool Sleeping { get; set; }
        public string SelectedExpansionPlanCode { get; set; } = string.Empty;
        public bool Day2Ready { get; set; }
        public Simulation영역건물발전Snapshot? BuildingProgression { get; set; }
        public Simulation학습방문Snapshot? LearningVisit { get; set; }
        public bool PlayerInsideCabin { get; set; }
        public SimulationNatureResourceNodeSnapshot[] ResourceNodes { get; set; }
            = Array.Empty<SimulationNatureResourceNodeSnapshot>();
        public SimulationNatureDroppedTimberSnapshot[] DroppedTimber { get; set; }
            = Array.Empty<SimulationNatureDroppedTimberSnapshot>();
        public SimulationNatureCooperativeActorSnapshot[] CooperativeActors
            { get; set; } = Array.Empty<SimulationNatureCooperativeActorSnapshot>();
        public SimulationNatureActiveWorkSnapshot? ActiveWork { get; set; }
        public Simulation집중판정ChallengeSnapshot? ActiveFocusChallenge { get; set; }
        public Simulation집중판정ResultSnapshot? LastFocusResult { get; set; }
        public SimulationNatureCabinSnapshot Cabin { get; set; }
            = new SimulationNatureCabinSnapshot();
        public SimulationNatureEncounterSnapshot? Encounter { get; set; }
        public bool SimulationOnly { get; set; } = true;
        public bool IsOperationalState { get; set; }
    }

    public sealed class SimulationNatureResourceNodeSnapshot
    {
        public string ResourceNodeStableId { get; set; } = string.Empty;
        public string H2StableId { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public string StateCode { get; set; } = SimulationNatureSurvivalCodes.Standing;
        public int RegrowsAtCycleIndex { get; set; } = -1;
    }

    /// <summary>
    /// 벌목 결과가 월드에 남긴 결정적 통나무 묶음이다. Unity는 이 상태 사본을
    /// 표현할 뿐 생성 수량이나 획득 가능 여부를 계산하지 않는다.
    /// </summary>
    public sealed class SimulationNatureDroppedTimberSnapshot
    {
        public string DroppedTimberStableId { get; set; } = string.Empty;
        public string SourceResourceNodeStableId { get; set; } = string.Empty;
        public string H2StableId { get; set; } = string.Empty;
        public string H1StableId { get; set; } = string.Empty;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public int Quantity { get; set; }
        public string UnitCode { get; set; } = SimulationNatureSurvivalCodes.UnitEach;
        public string StateCode { get; set; }
            = SimulationNatureSurvivalCodes.DroppedTimberAvailable;
        public long CreatedWorldRevision { get; set; }
        public long CollectedWorldRevision { get; set; }
    }

    public sealed class SimulationNatureActiveWorkSnapshot
    {
        public string OriginCommandId { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string WorkKindCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public int RequiredWorkSeconds { get; set; }
        public int CompletedWorkSeconds { get; set; }
        public int ReservedTimberQuantity { get; set; }
        public int ReservedRebuildPartQuantity { get; set; }
    }

    public sealed class SimulationNatureCooperativeActorSnapshot
    {
        public string ActorStableId { get; set; } = string.Empty;
        public decimal InventoryCapacityUnits { get; set; }
        public bool HasAxe { get; set; }
        public int TimberQuantity { get; set; }
        public long RegisteredWorldRevision { get; set; }
    }

    public sealed class Simulation플레이어기회Snapshot
    {
        public string OpportunityStableId { get; set; } = string.Empty;
        public string PlayerActivityTrackCode { get; set; } = string.Empty;
        public string PlayerFlowCode { get; set; } = string.Empty;
        public string NextPlayerFlowCode { get; set; } = string.Empty;
        public string CycleHandoffCode { get; set; } = string.Empty;
        public string WorldInteractionId { get; set; } = string.Empty;
        public string WorldInteractionName { get; set; } = string.Empty;
        public string WorldInteractionDisplayName { get; set; } = string.Empty;
        public string ResponsibilityKindCode { get; set; } = string.Empty;
        public string PrimaryOutcomeCode { get; set; } = string.Empty;
        public string SingleResponsibilityAssessmentCode { get; set; } = string.Empty;
        public string ActionCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public bool Available { get; set; }
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
    }

    public sealed class Simulation영역수요Snapshot
    {
        public string AreaSetStableId { get; set; } = string.Empty;
        public string NeedCode { get; set; } = string.Empty;
        public string RequiredItemCode { get; set; } = string.Empty;
        public int RequiredQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public bool Satisfied { get; set; }
    }

    public sealed class SimulationNatureCabinSnapshot
    {
        public string CabinStableId { get; set; } = "facility:nature-cabin";
        public string H2StableId { get; set; } = SimulationNatureSurvivalCodes.HomeH2StableId;
        public string H1StableId { get; set; } = SimulationNatureSurvivalCodes.CabinSiteH1StableId;
        public string StateCode { get; set; } = SimulationNatureSurvivalCodes.Planned;
        public double LocalX { get; set; }
        public double LocalZ { get; set; }
        public double YawDegrees { get; set; }
        public int ReservedTimberQuantity { get; set; }
        public int CompletedWorkSeconds { get; set; }
        public int RequiredWorkSeconds { get; set; }
        public int StorageCapacity { get; set; }
        public bool RecoveryAvailable { get; set; }
        public bool DefenseAvailable { get; set; }
    }

    public sealed class SimulationNatureEncounterSnapshot
    {
        public string EncounterStableId { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string ThreatPresentationCode { get; set; }
            = SimulationNatureSurvivalCodes.SkeletonPlaceholderCode;
        public int TriggeredCycleIndex { get; set; }
        public string ResolutionCode { get; set; } = string.Empty;
        public bool CabinDefenseApplied { get; set; }
        public int RawThreatTier { get; set; }
        public int EffectiveThreatTier { get; set; }
        public int HostileCount { get; set; }
        public string LinkedCombatStableId { get; set; } = string.Empty;
    }
}
