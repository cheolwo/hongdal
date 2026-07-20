using FluentResults;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Mart;
using 살뜰.Data;

namespace Ssalddel.Application.Mart;

public interface I마트주문요청조회UseCase
{
    Task<Result<마트주문요청응답>> 상세Async(
        Guid orderRequestId,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.SsalddelMart)]
[SsalddelUseCase(
    "내 마트 주문 요청 조회",
    Summary = "로그인 사용자가 직접 제출한 정확한 비구속 주문 요청 한 건만 조회합니다.")]
[SsalddelUseCaseActor(SsalddelActor.Orderer)]
public sealed class 마트주문요청조회UseCase(
    SsalddelContext db,
    ICurrentUserAccessor currentUserAccessor) : I마트주문요청조회UseCase
{
    public async Task<Result<마트주문요청응답>> 상세Async(
        Guid orderRequestId,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return 마트주문요청Results.Unauthorized<마트주문요청응답>();
        }

        if (orderRequestId == Guid.Empty)
        {
            return 마트주문요청Results.BadRequest<마트주문요청응답>("조회할 마트 주문 요청 ID를 확인해 주세요.");
        }

        var request = await db.마트주문요청
            .AsNoTracking()
            .SingleOrDefaultAsync(item =>
                item.Id == orderRequestId
                && item.요청자UserId == userId,
                cancellationToken);

        return request is null
            ? 마트주문요청Results.NotFound<마트주문요청응답>()
            : Result.Ok(마트주문요청Mapper.ToResponse(request));
    }
}
