using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E2,
        "Solo와 Hosted가 공유하는 전투 공간 파생 경계를 제공한다.",
        Boundary = "로컬 전투는 파생기가 없을 때 불변 World 문맥으로 실행하며 운영 공간을 변경하지 않는다.")]
    public interface ISimulationBattlefieldDerivationService
    {
        SimulationBattlefieldDerivationSnapshot Derive(
            string sessionStableId,
            string encounterStableId,
            string areaStableId,
            long capturedWorldRevision,
            bool natureEncounter);
    }
}
