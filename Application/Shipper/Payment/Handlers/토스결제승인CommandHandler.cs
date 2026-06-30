using FluentResults;
using Hongdal.Contracts.Shipper.Request;
using Hongdal.Contracts.Shipper.Payment;
using 홍달.Services.External.Toss;

namespace Hongdal.Application.Shipper.Payment;

public sealed class 토스결제승인CommandHandler : IRequestHandler<토스결제승인Command, Result<토스결제승인응답>>
{
    private readonly HongdalContext _db;
    private readonly ITossPaymentsService _tossPaymentsService;

    public 토스결제승인CommandHandler(HongdalContext db, ITossPaymentsService tossPaymentsService)
    {
        _db = db;
        _tossPaymentsService = tossPaymentsService;
    }

    public async Task<Result<토스결제승인응답>> Handle(토스결제승인Command request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PaymentKey))
        {
            return Result.Fail<토스결제승인응답>("paymentKey is required");
        }

        if (string.IsNullOrWhiteSpace(request.OrderId))
        {
            return Result.Fail<토스결제승인응답>("orderId is required");
        }

        if (request.Amount <= 0)
        {
            return Result.Fail<토스결제승인응답>("amount must be greater than 0");
        }

        var payment = await _db.결제.FirstOrDefaultAsync(x => x.OrderId == request.OrderId, cancellationToken);
        if (payment is null)
        {
            return Result.Fail<토스결제승인응답>("결제 요청을 찾을 수 없습니다.");
        }

        if (payment.결제금액 != request.Amount)
        {
            return Result.Fail<토스결제승인응답>("결제 금액이 일치하지 않습니다.");
        }

        if (payment.결제상태 == 상태값.결제상태.결제완료)
        {
            return Result.Ok(new 토스결제승인응답
            {
                결제Id = payment.결제Id,
                의뢰Id = payment.의뢰Id,
                OrderId = payment.OrderId,
                PaymentKey = payment.PaymentKey ?? string.Empty,
                결제상태 = payment.결제상태,
                결제응답 = payment.Toss응답Json ?? string.Empty
            });
        }

        var confirmResult = await _tossPaymentsService.ConfirmAsync(new TossConfirmApiRequest(
            request.PaymentKey,
            request.OrderId,
            request.Amount));

        if (!confirmResult.IsSuccess)
        {
            return Result.Fail<토스결제승인응답>(confirmResult.ResponseJson);
        }

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        payment.PaymentKey = request.PaymentKey;
        payment.결제수단 = string.IsNullOrWhiteSpace(confirmResult.PaymentMethod) ? payment.결제수단 : confirmResult.PaymentMethod;
        payment.결제상태 = 상태값.결제상태.결제완료;
        payment.Toss응답Json = confirmResult.ResponseJson;
        payment.승인일시 = DateTime.UtcNow;

        var shipperRequest = await _db.화주운송의뢰.FirstOrDefaultAsync(x => x.의뢰Id == payment.의뢰Id, cancellationToken);
        if (shipperRequest == null)
        {
            return Result.Fail<토스결제승인응답>("의뢰를 찾을 수 없습니다.");
        }

        shipperRequest.결제상태 = 상태값.결제상태.결제완료;
        shipperRequest.정산상태 = 운임정산상태.결제완료.ToString();
        shipperRequest.UpdatedAt = DateTime.UtcNow;

        var settlementTime = TryParseSettlementTime(shipperRequest.정산시점);
        var createDispatchQueue = settlementTime == 정산시점.선결제;

        if (createDispatchQueue)
        {
            shipperRequest.배차상태 = 상태값.배차상태.매칭중;

            var existingQueue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == shipperRequest.의뢰Id, cancellationToken);
            if (existingQueue == null)
            {
                _db.배차대기.Add(new 홍달.도메인.배차.배차대기
                {
                    의뢰Id = shipperRequest.의뢰Id,
                    화주Id = shipperRequest.화주Id,
                    픽업_도로명주소 = shipperRequest.픽업_도로명주소,
                    픽업_상세주소 = shipperRequest.픽업_상세주소,
                    픽업_위도 = shipperRequest.픽업_위도,
                    픽업_경도 = shipperRequest.픽업_경도,
                    하차_도로명주소 = shipperRequest.하차_도로명주소,
                    하차_상세주소 = shipperRequest.하차_상세주소,
                    하차_위도 = shipperRequest.하차_위도,
                    하차_경도 = shipperRequest.하차_경도,
                    상태 = 상태값.배차대기상태.대기,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }
        else
        {
            shipperRequest.배차상태 = 상태값.배차상태.미시작;
        }

        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return Result.Ok(new 토스결제승인응답
        {
            결제Id = payment.결제Id,
            의뢰Id = payment.의뢰Id,
            OrderId = payment.OrderId,
            PaymentKey = payment.PaymentKey ?? string.Empty,
            결제상태 = payment.결제상태,
            결제응답 = confirmResult.ResponseJson
        });
    }

    private static 정산시점 TryParseSettlementTime(string? value)
    {
        return Enum.TryParse<정산시점>(value, ignoreCase: false, out var parsed)
            ? parsed
            : 정산시점.선결제;
    }
}
