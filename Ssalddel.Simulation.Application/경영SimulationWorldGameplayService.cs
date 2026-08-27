using System;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    [SsalddelCodeMetadata(
        SsalddelCodeFeatureKeys.SimulationSessionLifecycle,
        SsalddelCodeLayer.Application,
        "플레이어 심리·AreaSet 이동·호스팅·협동 건설의 세계 게임플레이를 조율한다.",
        StepKey = "application.world-gameplay",
        DependsOnStepKeys = new string[] { "api.world-gameplay" },
        ExecutionStage = SsalddelCodeExecutionStage.Confirm,
        Effects = SsalddelCodeEffect.StateMutation,
        ReadsFrom = SsalddelCodeDataScope.SimulationState,
        WritesTo = SsalddelCodeDataScope.SimulationState,
        FlowOrder = 30,
        Boundary = "운영 상태를 변경하지 않으며 서버 세션의 권한·개정·원장을 통해서만 세계 게임플레이 상태를 변경한다.")]
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public sealed class 경영SimulationWorldGameplayService
    {
        private readonly 경영SimulationSessionAccessor sessions;

        public 경영SimulationWorldGameplayService(
            경영SimulationSessionAccessor sessions)
        {
            this.sessions = sessions
                ?? throw new ArgumentNullException(nameof(sessions));
        }

        public SimulationNatureMindStateSnapshot GetNatureMindState(
            string sessionStableId)
            => sessions.Require(sessionStableId).GetNatureMindState();

        public SimulationTownNpcLifeStateSnapshot GetTownNpcLifeState(
            string sessionStableId)
            => sessions.Require(sessionStableId).GetTownNpcLifeState();

        public SimulationNatureFarmInterpretationSnapshot GetNatureFarmInterpretation(
            string sessionStableId, string playerStableId)
            => sessions.Require(sessionStableId)
                .GetNatureFarmInterpretation(playerStableId);

        public SimulationPlayerAreaAccessStateSnapshot GetPlayerAreaAccess(
            string sessionStableId, string playerStableId)
            => sessions.Require(sessionStableId).GetPlayerAreaAccess(playerStableId);

        public SimulationAreaTraversalPreviewSnapshot PreviewAreaTraversal(
            string sessionStableId, SimulationAreaTraversalPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewAreaTraversal(request);

        public 경영SimulationSessionSnapshot ConfirmAreaTraversal(
            string sessionStableId, SimulationAreaTraversalConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmAreaTraversal(request);

        public SimulationHostedWorldStateSnapshot GetHostedWorldState(
            string sessionStableId)
            => sessions.Require(sessionStableId).GetHostedWorldState();

        public SimulationHostedWorldPreviewSnapshot PreviewOpenHostedWorld(
            string sessionStableId, SimulationHostedWorldOpenPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewOpenHostedWorld(request);

        public 경영SimulationSessionSnapshot ConfirmOpenHostedWorld(
            string sessionStableId, SimulationHostedWorldOpenConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmOpenHostedWorld(request);

        public SimulationHostedWorldPreviewSnapshot PreviewJoinHostedWorld(
            string sessionStableId, SimulationHostedWorldJoinPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewJoinHostedWorld(request);

        public 경영SimulationSessionSnapshot ConfirmJoinHostedWorld(
            string sessionStableId, SimulationHostedWorldJoinConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmJoinHostedWorld(request);

        public SimulationHostedWorldPreviewSnapshot PreviewHostedGuestAction(
            string sessionStableId, SimulationHostedGuestActionPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewHostedGuestAction(request);

        public 경영SimulationSessionSnapshot ConfirmHostedGuestAction(
            string sessionStableId, SimulationHostedGuestActionConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmHostedGuestAction(request);

        public SimulationCoopConstructionStateSnapshot GetCoopConstructionState(
            string sessionStableId)
            => sessions.Require(sessionStableId).GetCoopConstructionState();

        public SimulationCoopConstructionPreviewSnapshot PreviewCoopContribution(
            string sessionStableId, SimulationCoopContributionPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewCoopContribution(request);

        public 경영SimulationSessionSnapshot ConfirmCoopContribution(
            string sessionStableId, SimulationCoopContributionConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmCoopContribution(request);

        public SimulationCoopConstructionPreviewSnapshot PreviewCoopDemolition(
            string sessionStableId, SimulationCoopProtectedActionPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewCoopDemolition(request);

        public 경영SimulationSessionSnapshot ConfirmCoopDemolition(
            string sessionStableId, SimulationCoopProtectedActionConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmCoopDemolition(request);

        public SimulationCoopConstructionPreviewSnapshot PreviewCoopRestore(
            string sessionStableId, SimulationCoopProtectedActionPreviewRequest request)
            => sessions.Require(sessionStableId).PreviewCoopRestore(request);

        public 경영SimulationSessionSnapshot ConfirmCoopRestore(
            string sessionStableId, SimulationCoopProtectedActionConfirmRequest request)
            => sessions.Require(sessionStableId).ConfirmCoopRestore(request);

        public SimulationGameplayObservabilitySnapshot GetGameplayObservability(
            string sessionStableId)
            => sessions.Require(sessionStableId).GetGameplayObservability();
    }
}
