using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "구성 요소의 공통 Core·Application 또는 Adapter 실행 경계를 제공한다.",
        Boundary = "실행 경계는 실제 권위 위치와 E 단계 달성 증거를 분리한다.")]
    public interface I경영SimulationSessionStore
    {
        경영SimulationSessionAggregate CreateOrGet(경영SimulationSession생성Request request,
            SimulationRealityContextSnapshot? frozenRealityContext = null);
        경영SimulationSessionAggregate? Find(string sessionStableId);
        경영SimulationSessionAggregate Restore(경영SimulationSessionAggregate session);
        경영SimulationSessionAggregate ReplaceForCampaignRetry(
            경영SimulationSessionAggregate session, long expectedCurrentRevision);
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
        "결정성·Save/Replay 또는 회귀 검증 책임을 제공한다.",
        Boundary = "검증 코드 존재만으로 상위 E 증거를 승격하지 않는다.")]
    public interface ISimulationSessionSaveStore
    {
        SimulationSessionSavePackage SaveOrGet(SimulationSessionSavePackage package);
        SimulationSessionSavePackage? Find(string saveStableId);
    }

    public interface ISimulationBattleWorldReconciler
    {
        void EnsureWorldTickCanAdvance(string sessionStableId);
        void Reconcile(string sessionStableId, 경영SimulationSessionSnapshot world);
        SimulationBattleSaveRecordSnapshot[] Capture(string sessionStableId);
        void Restore(string sessionStableId,
            SimulationBattleSaveRecordSnapshot[] records);
    }

    public interface ISimulationBattleResourceLockReader
    {
        bool IsLocked(string sessionStableId, string resourceStableId);
    }
}
