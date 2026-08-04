using Ssalddel.Application.Shipper.Request;
using Ssalddel.Contracts.Admin.Transport;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Services.Community;

public interface I지도신청운송취소검토AdminWorkflow
{
    Task<IReadOnlyList<지도신청가원장Response>> 목록Async(
        CancellationToken cancellationToken = default);

    Task<지도신청가원장Response> 처리Async(
        string ledgerId,
        지도신청운송취소검토처리Request request,
        string actorUserId,
        CancellationToken cancellationToken = default);
}

public sealed class 지도신청운송취소검토AdminWorkflow(
    I지도신청가원장UseCase ledgerUseCase,
    I화주운송의뢰UseCase transportUseCase) : I지도신청운송취소검토AdminWorkflow
{
    public Task<IReadOnlyList<지도신청가원장Response>> 목록Async(
        CancellationToken cancellationToken = default)
        => ledgerUseCase.관리자운송취소검토목록Async(cancellationToken);

    public async Task<지도신청가원장Response> 처리Async(
        string ledgerId,
        지도신청운송취소검토처리Request request,
        string actorUserId,
        CancellationToken cancellationToken = default)
    {
        await ledgerUseCase.관리자운송취소검토확인Async(
            ledgerId,
            request,
            actorUserId,
            cancellationToken);

        if (request.승인)
        {
            var operational = await transportUseCase.관리자취소환불Async(
                request.확인운영원본Id,
                new 관리자운송의뢰취소환불요청
                {
                    확인의뢰Id = request.확인운영원본Id,
                    사유 = request.검토사유
                },
                cancellationToken);
            if (operational.IsFailed)
            {
                throw new InvalidOperationException(string.Join(" ", operational.Errors.Select(error => error.Message)));
            }
        }

        return await ledgerUseCase.관리자운송취소검토결과반영Async(
            ledgerId,
            request,
            actorUserId,
            cancellationToken);
    }
}
