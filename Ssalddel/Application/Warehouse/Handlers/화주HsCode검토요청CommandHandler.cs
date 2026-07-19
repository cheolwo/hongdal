using FluentResults;
using MediatR;

namespace Ssalddel.Application.Warehouse;

public sealed class 화주HsCode검토요청CommandHandler : IRequestHandler<화주HsCode검토요청Command, Result<화주통관의뢰등록결과>>
{
    private readonly ISender _sender;

    public 화주HsCode검토요청CommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public Task<Result<화주통관의뢰등록결과>> Handle(화주HsCode검토요청Command request, CancellationToken cancellationToken)
    {
        return _sender.Send(
            new 화주통관의뢰등록Command(
                request.화주UserId,
                "HS_CODE_REVIEW",
                request.물류거래방향,
                request.대표상품명,
                request.주문참조번호,
                request.주문Id,
                null,
                null,
                request.대상관세사참여자Id,
                request.요청메모),
            cancellationToken);
    }
}
