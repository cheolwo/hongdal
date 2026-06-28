using FluentResults;
using Hongdal.Contracts.Shipper.Payment;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;
using 홍달.Services.External.Toss;

namespace Hongdal.Application.Shipper.Payment;

public sealed class 토스결제준비CommandHandler : IRequestHandler<토스결제준비Command, Result<토스결제준비응답>>
{
    private readonly HongdalContext _db;
    private readonly IOptions<TossPaymentsOptions> _options;

    public 토스결제준비CommandHandler(HongdalContext db, IOptions<TossPaymentsOptions> options)
    {
        _db = db;
        _options = options;
    }

    public async Task<Result<토스결제준비응답>> Handle(토스결제준비Command request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.의뢰Id))
        {
            return Result.Fail<토스결제준비응답>("의뢰Id is required");
        }

        if (request.Amount <= 0)
        {
            return Result.Fail<토스결제준비응답>("amount must be greater than 0");
        }

        var shipperRequest = await _db.화주운송의뢰.FirstOrDefaultAsync(x => x.의뢰Id == request.의뢰Id, cancellationToken);
        if (shipperRequest == null)
        {
            return Result.Fail<토스결제준비응답>("의뢰를 찾을 수 없습니다.");
        }

        if (!string.Equals(shipperRequest.배차상태, 상태값.배차상태.상차완료, StringComparison.Ordinal))
        {
            return Result.Fail<토스결제준비응답>("상차완료 이후에만 결제를 진행할 수 있습니다.");
        }

        if (shipperRequest.결제상태 == 상태값.결제상태.결제완료)
        {
            return Result.Fail<토스결제준비응답>("이미 결제완료된 의뢰입니다.");
        }

        var requestedAmount = request.Amount > 0 ? request.Amount : shipperRequest.결제예정금액 ?? 0;
        if (requestedAmount <= 0)
        {
            return Result.Fail<토스결제준비응답>("결제금액이 유효하지 않습니다.");
        }

        var existingPendingPayment = await _db.결제
            .Where(x => x.의뢰Id == shipperRequest.의뢰Id && x.결제상태 == 상태값.결제상태.결제대기 && x.결제금액 == request.Amount)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingPendingPayment != null)
        {
            return Result.Ok(new 토스결제준비응답
            {
                결제Id = existingPendingPayment.결제Id,
                의뢰Id = existingPendingPayment.의뢰Id,
                OrderId = existingPendingPayment.OrderId,
                Amount = existingPendingPayment.결제금액,
                ClientKey = _options.Value.ClientKey
            });
        }

        var orderId = 주문번호생성();
        var payment = new 홍달.도메인.결제.결제
        {
            결제Id = Guid.NewGuid().ToString("N"),
            의뢰Id = shipperRequest.의뢰Id,
            화주Id = shipperRequest.화주Id,
            결제금액 = requestedAmount,
            결제수단 = shipperRequest.결제수단,
            OrderId = orderId,
            결제상태 = 상태값.결제상태.결제대기,
            CreatedAt = DateTime.UtcNow
        };

        shipperRequest.결제상태 = 상태값.결제상태.결제대기;
        shipperRequest.UpdatedAt = DateTime.UtcNow;

        await _db.결제.AddAsync(payment, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(new 토스결제준비응답
        {
            결제Id = payment.결제Id,
            의뢰Id = payment.의뢰Id,
            OrderId = payment.OrderId,
            Amount = payment.결제금액,
            ClientKey = _options.Value.ClientKey
        });
    }

    private static string 주문번호생성()
    {
        return $"hongdal_{Guid.NewGuid():N}";
    }
}
