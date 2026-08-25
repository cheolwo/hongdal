using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    public enum SimulationAuthorityLocation
    {
        LocalProcess = 0,
        RemoteHost = 1,
        [Obsolete("ReviewFixture는 권위 위치가 아니라 실행 목적입니다. SimulationRuntimePurpose를 사용하세요.")]
        ReviewFixture = 2,
    }

    public enum SimulationRuntimePurpose
    {
        Playable = 0,
        ReviewFixture = 1,
    }

    public sealed class SimulationRuntimeDescriptor
    {
        private const int LegacyReviewFixtureAuthorityValue = 2;

        public SimulationAuthorityLocation AuthorityLocation { get; set; }
        public SimulationRuntimePurpose Purpose { get; set; } =
            SimulationRuntimePurpose.Playable;
        public string RuntimeStableId { get; set; } = string.Empty;
        public bool RequiresNetwork { get; set; }

        public bool IsPlayableAuthority =>
            Purpose == SimulationRuntimePurpose.Playable
            && (AuthorityLocation == SimulationAuthorityLocation.LocalProcess
                || AuthorityLocation == SimulationAuthorityLocation.RemoteHost);

        [Obsolete("Purpose를 사용하세요. 이 속성은 기존 소비자 호환용입니다.")]
        public bool IsReviewFixture
        {
            get => Purpose == SimulationRuntimePurpose.ReviewFixture
                || (int) AuthorityLocation == LegacyReviewFixtureAuthorityValue;
            set
            {
                if (value)
                    Purpose = SimulationRuntimePurpose.ReviewFixture;
                else if (Purpose == SimulationRuntimePurpose.ReviewFixture)
                    Purpose = SimulationRuntimePurpose.Playable;
            }
        }
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Simulation 실행 위치와 공통 기능 포트를 노출한다.",
        Boundary = "LocalProcess와 RemoteHost가 같은 Core 계약을 사용한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세션실행)]
    public interface ISimulationRuntime
    {
        SimulationRuntimeDescriptor Descriptor { get; }
        ISimulationSessionRuntime Sessions { get; }
        ISimulationNatureSurvivalRuntime Nature { get; }
        ISimulationSessionGameplayRuntime Gameplay { get; }
        ISimulationWorldInteractionRuntime WorldInteractions { get; }
    }

    /// <summary>
    /// 새 소비자가 필요한 기능만 선택할 수 있도록 좁은 Runtime 포트를 노출한다.
    /// 기존 ISimulationRuntime은 공개 호환 facade로 유지한다.
    /// </summary>
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "기능별 좁은 Simulation Runtime 포트를 조립한다.",
        Boundary = "기존 ISimulationRuntime facade 호환을 유지한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세션실행)]
    public interface ISimulationRuntimeModules : ISimulationRuntime
    {
        ISimulationTurnRuntime Turns { get; }
        ISimulationFarmChoiceRuntime FarmChoices { get; }
        ISimulationLogisticsRuntime Logistics { get; }
        ISimulationFarmWorldInteractionRuntime FarmWorldInteractions { get; }
        ISimulationNatureWorldInteractionRuntime NatureWorldInteractions { get; }
    }

    /// <summary>
    /// WI 계약을 같은 Session Aggregate에서 실행하는 공통 경계다.
    /// Local과 Remote는 전송 방식만 다르고 WI 규칙을 다시 구현하지 않는다.
    /// </summary>
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Farm WI Preview·Confirm을 공통 Runtime 경계로 노출한다.",
        Boundary = "Farm 규칙을 Adapter에서 다시 구현하지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-FARM-04", "WI-FARM-05", "WI-FARM-06" })]
    public interface ISimulationFarmWorldInteractionRuntime
    {
        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "Farm 건설 배치 후보를 권위 Core에서 검토한다.",
            Boundary = "배치 후보는 Confirm 전 상태를 변경하지 않는다.")]
        ValueTask<SimulationFarmConstructionPlacementPreviewSnapshot>
            PreviewFarmConstructionPlacementAsync(
            string sessionStableId,
            SimulationFarmConstructionPlacementPreviewRequest request,
            CancellationToken cancellationToken = default);
        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "Farm 건설 배치 제안을 권위 Core에 확정한다.",
            Boundary = "제안 식별자와 판본 검증 뒤에만 상태를 변경한다.")]
        ValueTask<경영SimulationSessionSnapshot> ConfirmFarmConstructionPlacementAsync(
            string sessionStableId,
            SimulationFarmConstructionPlacementConfirmRequest request,
            CancellationToken cancellationToken = default);

        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "Farm 작업 WI의 실행 가능성과 결과 후보를 검토한다.",
            Boundary = "공통 Farm 작업 Preview 포트다.",
            WorldInteractionIds = new[] { "WI-FARM-04", "WI-FARM-05", "WI-FARM-06" })]
        ValueTask<SimulationFarmWorkPreviewSnapshot> PreviewFarmWorkAsync(
            string sessionStableId,
            SimulationFarmWorkPreviewRequest request,
            CancellationToken cancellationToken = default);
        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "Farm 작업 WI를 권위 Session에 확정한다.",
            Boundary = "공통 Farm 작업 Confirm 포트다.",
            WorldInteractionIds = new[] { "WI-FARM-04", "WI-FARM-05", "WI-FARM-06" })]
        ValueTask<SimulationFarmSurvivalStateSnapshot> ConfirmFarmWorkAsync(
            string sessionStableId,
            SimulationFarmWorkConfirmRequest request,
            CancellationToken cancellationToken = default);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Nature WI Preview·Confirm을 공통 Runtime 경계로 노출한다.",
        Boundary = "Nature 규칙을 Adapter에서 다시 구현하지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { "WI-NATURE-01", "WI-NATURE-02", "WI-NATURE-03", "WI-NATURE-04" })]
    public interface ISimulationNatureWorldInteractionRuntime
    {
        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "자연권 위협 관찰 WI 후보를 검토한다.",
            Boundary = "관찰 Preview는 상태를 변경하지 않는다.",
            WorldInteractionIds = new[] { "WI-NATURE-01" })]
        ValueTask<SimulationNatureThreatObservationPreviewSnapshot>
            PreviewNatureThreatObservationAsync(
            string sessionStableId,
            SimulationNatureThreatObservationPreviewRequest request,
            CancellationToken cancellationToken = default);
        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "자연권 위협 관찰 WI를 확정한다.",
            Boundary = "권위 판본 검증 뒤 관찰 결과를 반영한다.",
            WorldInteractionIds = new[] { "WI-NATURE-01" })]
        ValueTask<경영SimulationSessionSnapshot> ConfirmNatureThreatObservationAsync(
            string sessionStableId,
            SimulationNatureThreatObservationConfirmRequest request,
            CancellationToken cancellationToken = default);
        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "자연권 긴급 후퇴 WI 후보를 검토한다.",
            Boundary = "후퇴 Preview는 상태를 변경하지 않는다.",
            WorldInteractionIds = new[] { "WI-NATURE-02" })]
        ValueTask<SimulationNatureEmergencyRetreatPreviewSnapshot>
            PreviewNatureEmergencyRetreatAsync(
            string sessionStableId,
            SimulationNatureEmergencyRetreatPreviewRequest request,
            CancellationToken cancellationToken = default);
        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "자연권 긴급 후퇴 WI를 확정한다.",
            Boundary = "권위 판본 검증 뒤 후퇴 결과를 반영한다.",
            WorldInteractionIds = new[] { "WI-NATURE-02" })]
        ValueTask<경영SimulationSessionSnapshot> ConfirmNatureEmergencyRetreatAsync(
            string sessionStableId,
            SimulationNatureEmergencyRetreatConfirmRequest request,
            CancellationToken cancellationToken = default);
        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "자연권 복원 WI 후보를 검토한다.",
            Boundary = "복원 Preview는 상태를 변경하지 않는다.",
            WorldInteractionIds = new[] { "WI-NATURE-03" })]
        ValueTask<SimulationNatureRestorationPreviewSnapshot>
            PreviewNatureRestorationAsync(
            string sessionStableId,
            SimulationNatureRestorationPreviewRequest request,
            CancellationToken cancellationToken = default);
        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "자연권 복원 WI를 확정한다.",
            Boundary = "권위 판본 검증 뒤 복원 결과를 반영한다.",
            WorldInteractionIds = new[] { "WI-NATURE-03" })]
        ValueTask<경영SimulationSessionSnapshot> ConfirmNatureRestorationAsync(
            string sessionStableId,
            SimulationNatureRestorationConfirmRequest request,
            CancellationToken cancellationToken = default);
        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "파티 회복 WI 후보를 검토한다.",
            Boundary = "회복 Preview는 상태를 변경하지 않는다.",
            WorldInteractionIds = new[] { "WI-NATURE-04" })]
        ValueTask<SimulationNaturePartyRecoveryPreviewSnapshot>
            PreviewNaturePartyRecoveryAsync(
            string sessionStableId,
            SimulationNaturePartyRecoveryPreviewRequest request,
            CancellationToken cancellationToken = default);
        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "파티 회복 WI를 확정한다.",
            Boundary = "권위 판본 검증 뒤 회복 결과를 반영한다.",
            WorldInteractionIds = new[] { "WI-NATURE-04" })]
        ValueTask<경영SimulationSessionSnapshot> ConfirmNaturePartyRecoveryAsync(
            string sessionStableId,
            SimulationNaturePartyRecoveryConfirmRequest request,
            CancellationToken cancellationToken = default);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Farm·Nature WI 실행 포트를 하나의 Runtime 표면으로 묶는다.",
        Boundary = "WI 소유권은 각 계약과 원장에 남긴다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행)]
    public interface ISimulationWorldInteractionRuntime :
        ISimulationFarmWorldInteractionRuntime,
        ISimulationNatureWorldInteractionRuntime
    {
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "턴 마감 Preview·Confirm 실행 경계를 제공한다.",
        Boundary = "WorldTick 변경은 권위 Session에서만 수행한다.")]
    public interface ISimulationTurnRuntime
    {
        ValueTask<SimulationTurnClosingContextSnapshot> GetTurnClosingContextAsync(
            string sessionStableId,
            CancellationToken cancellationToken = default);
        ValueTask<SimulationTurnClosingPreviewSnapshot> PreviewTurnClosingAsync(
            string sessionStableId,
            SimulationTurnClosingPreviewRequest request,
            CancellationToken cancellationToken = default);
        ValueTask<경영SimulationSessionSnapshot> ConfirmTurnClosingAsync(
            string sessionStableId,
            SimulationTurnClosingConfirmRequest request,
            CancellationToken cancellationToken = default);
        ValueTask<Simulation타로객체반응PreviewSnapshot>
            PreviewTarotObjectReactionAsync(
            string sessionStableId,
            Simulation타로객체반응PreviewRequest request,
            CancellationToken cancellationToken = default);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Farm 플레이어 선택 Preview·Confirm 실행 경계를 제공한다.",
        Boundary = "선택 결과를 Client에서 계산하지 않는다.")]
    public interface ISimulationFarmChoiceRuntime
    {
        ValueTask<SimulationFarmChoiceContextSnapshot> GetFarmChoiceContextAsync(
            string sessionStableId,
            CancellationToken cancellationToken = default);
        ValueTask<SimulationFarmChoicePreviewSnapshot> PreviewFarmChoiceAsync(
            string sessionStableId,
            SimulationFarmChoicePreviewRequest request,
            CancellationToken cancellationToken = default);
        ValueTask<경영SimulationSessionSnapshot> ConfirmFarmChoiceAsync(
            string sessionStableId,
            SimulationFarmChoiceConfirmRequest request,
            CancellationToken cancellationToken = default);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Simulation 물류 Preview·Confirm 실행 경계를 제공한다.",
        Boundary = "독립 영역 준비 전 연결 경로를 암묵적으로 열지 않는다.")]
    public interface ISimulationLogisticsRuntime
    {
        ValueTask<SimulationLogisticsMovementPreviewSnapshot>
            PreviewLogisticsMovementAsync(
            string sessionStableId,
            SimulationLogisticsMovementPreviewRequest request,
            CancellationToken cancellationToken = default);
        ValueTask<경영SimulationSessionSnapshot> ConfirmLogisticsMovementAsync(
            string sessionStableId,
            SimulationLogisticsMovementConfirmRequest request,
            CancellationToken cancellationToken = default);

        ValueTask<SimulationFreightDispatchPreviewSnapshot>
            PreviewFreightDispatchAsync(
            string sessionStableId,
            SimulationFreightDispatchPreviewRequest request,
            CancellationToken cancellationToken = default);
        ValueTask<경영SimulationSessionSnapshot> ConfirmFreightDispatchAsync(
            string sessionStableId,
            SimulationFreightDispatchConfirmRequest request,
            CancellationToken cancellationToken = default);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Session Gameplay의 턴·선택·물류 포트를 조립한다.",
        Boundary = "호환 facade이며 독립 상태를 소유하지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세션실행)]
    public interface ISimulationSessionGameplayRuntime :
        ISimulationTurnRuntime,
        ISimulationFarmChoiceRuntime,
        ISimulationLogisticsRuntime
    {
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Session 생성·조회·Tick·Save/Load 실행 경계를 제공한다.",
        Boundary = "같은 Session Aggregate를 Local과 Hosted에서 실행한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세션실행)]
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E3,
        "Session Save·Load·검증 회귀 경계를 노출한다.",
        Role = SsalddelEvidenceResponsibilityRole.Secondary,
        Boundary = "저장 포트 존재만으로 Save/Replay 증거를 승격하지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E3저장재생검증)]
    public interface ISimulationSessionRuntime
    {
        ValueTask<경영SimulationSessionSnapshot> CreateAsync(
            경영SimulationSession생성Request request,
            CancellationToken cancellationToken = default);

        ValueTask<경영SimulationSessionSnapshot> GetAsync(
            string sessionStableId,
            CancellationToken cancellationToken = default);

        ValueTask<경영SimulationSessionSnapshot> AdvanceWorldTickAsync(
            string sessionStableId,
            경영SimulationTick진행Request request,
            CancellationToken cancellationToken = default);

        ValueTask<SimulationSpatialCompositionStateSnapshot>
            GetSpatialCompositionAsync(string sessionStableId,
                string areaCode,
                CancellationToken cancellationToken = default);

        ValueTask<SimulationLocalSaveSlotResult> SaveSlotAsync(
            string sessionStableId,
            SimulationLocalSaveSlotRequest request,
            CancellationToken cancellationToken = default);

        ValueTask<SimulationLocalLoadSlotResult> LoadSlotAsync(
            string slotStableId,
            CancellationToken cancellationToken = default);

        ValueTask<SimulationLocalLoadSlotResult> VerifySlotAsync(
            string slotStableId,
            CancellationToken cancellationToken = default);
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "Nature 생존 실시간 명령과 WorldTick 합류 경계를 제공한다.",
        Boundary = "실시간 시계와 WorldTick을 분리한다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] {
            SimulationNatureSurvivalCodes.AcquireAxeWorldInteractionId,
            SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
            SimulationNatureSurvivalCodes.PlaceCabinBlueprintWorldInteractionId,
            SimulationNatureSurvivalCodes.BeginCabinBuildWorldInteractionId,
            SimulationNatureSurvivalCodes.EnterCabinWorldInteractionId,
            SimulationNatureSurvivalCodes.LeaveCabinWorldInteractionId,
            SimulationNatureSurvivalCodes.ResolveEncounterWorldInteractionId })]
    public interface ISimulationNatureSurvivalRuntime
    {
        ValueTask<SimulationNatureSurvivalStateSnapshot> GetAsync(
            string sessionStableId,
            CancellationToken cancellationToken = default);

        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "Nature 생존 WI 후보를 공통 Core에서 검토한다.",
            Boundary = "Preview는 권위 상태를 변경하지 않는다.",
            WorldInteractionIds = new[] {
                SimulationNatureSurvivalCodes.AcquireAxeWorldInteractionId,
                SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
                SimulationNatureSurvivalCodes.PlaceCabinBlueprintWorldInteractionId,
                SimulationNatureSurvivalCodes.BeginCabinBuildWorldInteractionId,
                SimulationNatureSurvivalCodes.EnterCabinWorldInteractionId,
                SimulationNatureSurvivalCodes.LeaveCabinWorldInteractionId,
                SimulationNatureSurvivalCodes.ResolveEncounterWorldInteractionId })]
        ValueTask<SimulationNatureSurvivalActionPreviewSnapshot> PreviewAsync(
            string sessionStableId,
            SimulationNatureSurvivalActionPreviewRequest request,
            CancellationToken cancellationToken = default);

        [SsalddelEvidenceResponsibility(
            SsalddelEvidenceStage.E2,
            "Nature 생존 WI를 공통 Core의 권위 Session에 확정한다.",
            Boundary = "Unity 표현이 아니라 Core가 권위 상태를 변경한다.",
            WorldInteractionIds = new[] {
                SimulationNatureSurvivalCodes.AcquireAxeWorldInteractionId,
                SimulationNatureSurvivalCodes.BeginHarvestWorldInteractionId,
                SimulationNatureSurvivalCodes.PlaceCabinBlueprintWorldInteractionId,
                SimulationNatureSurvivalCodes.BeginCabinBuildWorldInteractionId,
                SimulationNatureSurvivalCodes.EnterCabinWorldInteractionId,
                SimulationNatureSurvivalCodes.LeaveCabinWorldInteractionId,
                SimulationNatureSurvivalCodes.ResolveEncounterWorldInteractionId })]
        ValueTask<경영SimulationSessionSnapshot> ConfirmAsync(
            string sessionStableId,
            SimulationNatureSurvivalCommandRequest request,
            CancellationToken cancellationToken = default);

        ValueTask<경영SimulationSessionSnapshot> AdvanceRealtimeAsync(
            string sessionStableId,
            SimulationNatureSurvivalClockAdvanceRequest request,
            CancellationToken cancellationToken = default);
    }

    public sealed class SimulationLocalSaveSlotRequest
    {
        public string SlotStableId { get; set; } = string.Empty;
        public long ExpectedRevision { get; set; }
        public SimulationLhWorldStateSnapshot? LhWorldState { get; set; }
    }

    public sealed class SimulationLocalSaveSlotResult
    {
        public string SlotStableId { get; set; } = string.Empty;
        public string SaveStableId { get; set; } = string.Empty;
        public string ReplayHash { get; set; } = string.Empty;
        public int SavedWorldTick { get; set; }
        public long SavedWorldRevision { get; set; }
    }

    public sealed class SimulationLocalLoadSlotResult
    {
        public string SlotStableId { get; set; } = string.Empty;
        public bool RecoveredFromBackup { get; set; }
        public SimulationSessionRestoreResult Restore { get; set; }
            = new SimulationSessionRestoreResult();
    }

    public sealed class SimulationLocalSaveSlotPackage
    {
        public string SlotStableId { get; set; } = string.Empty;
        public bool RecoveredFromBackup { get; set; }
        public SimulationSessionSavePackage Package { get; set; }
            = new SimulationSessionSavePackage();
    }

    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E3,
        "Solo Save 슬롯의 저장·복원 경계를 제공한다.",
        Boundary = "저장 매체와 canonical Save/Replay 의미를 분리한다.")]
    public interface ISimulationLocalSaveSlotStore
    {
        void Write(string slotStableId, SimulationSessionSavePackage package);
        SimulationLocalSaveSlotPackage Read(string slotStableId);
    }
}
