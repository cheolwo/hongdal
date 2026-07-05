using FluentResults;
using Hongdal.Contracts.Common.Payments;

namespace Hongdal.Application.Shipper.Payment;

public sealed class 공통결제승인CommandHandler : IRequestHandler<공통결제승인Command, Result<공통결제승인응답>>
{
    private readonly ISender _sender;

    public 공통결제승인CommandHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task<Result<공통결제승인응답>> Handle(공통결제승인Command request, CancellationToken cancellationToken)
    {
        if (request.결제제공자 != 계약결제제공자.TossPayments)
        {
            return Result.Fail<공통결제승인응답>("현재는 TossPayments 제공자만 지원합니다.");
        }

        var tossResult = await _sender.Send(new 토스결제승인Command(request.PaymentKey, request.OrderId, request.Amount), cancellationToken);
        if (tossResult.IsFailed)
        {
            return Result.Fail<공통결제승인응답>(tossResult.Errors.Select(x => x.Message));
        }

        return Result.Ok(new 공통결제승인응답
        {
            결제요청Id = tossResult.Value.결제Id,
            대상Id = tossResult.Value.의뢰Id,
            결제제공자 = 계약결제제공자.TossPayments,
            OrderId = tossResult.Value.OrderId,
            PaymentKey = tossResult.Value.PaymentKey,
            결제상태 = tossResult.Value.결제상태,
            결제응답 = tossResult.Value.결제응답
        });
    }
}
