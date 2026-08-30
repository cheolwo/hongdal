using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
    [SsalddelEvidenceResponsibility(
        SsalddelEvidenceStage.E2,
        "방문자 체류 조회·Preview·Confirm의 공통 실행 포트를 정의한다.",
        Boundary = "원장 실행 포트이며 실제 Session 귀속·저장 완료를 뜻하지 않는다.",
        SubmoduleKey = SsalddelEvidenceSubmoduleKeys.E2세계상호작용실행,
        WorldInteractionIds = new[] { Simulation공동체방문자체류Codes.WorldInteractionId })]
    public interface ISimulation방문자체류Runtime
    {
        ValueTask<Simulation공동체방문자체류LedgerSnapshot> GetVisitorsAsync(
            string ledgerStableId, CancellationToken cancellationToken = default);
        ValueTask<Simulation공동체방문자체류PreviewSnapshot> PreviewVisitorStayAsync(
            string ledgerStableId, Simulation공동체방문자체류PreviewRequest request,
            CancellationToken cancellationToken = default);
        ValueTask<Simulation공동체방문자체류ConfirmResult> ConfirmVisitorStayAsync(
            string ledgerStableId, Simulation공동체방문자체류ConfirmRequest request,
            CancellationToken cancellationToken = default);
    }
}
