using Hongdal.Application.Shipper.Payment.Events;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Payment.Handlers;

public sealed class 용달운송의뢰결제승인완료EventHandler : INotificationHandler<결제승인완료Event>
{
    private readonly HongdalContext _db;
    private readonly ILogger<용달운송의뢰결제승인완료EventHandler> _logger;

    public 용달운송의뢰결제승인완료EventHandler(HongdalContext db, ILogger<용달운송의뢰결제승인완료EventHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(결제승인완료Event notification, CancellationToken cancellationToken)
    {
        if (notification.결제대상유형 != 홍달.도메인.결제.결제공통정의.결제대상유형.용달운송의뢰)
        {
            return;
        }

        var shipperRequest = await _db.화주운송의뢰.FirstOrDefaultAsync(x => x.의뢰Id == notification.대상Id, cancellationToken);
        if (shipperRequest is null)
        {
            _logger.LogWarning("결제승인완료 후처리 대상 의뢰를 찾지 못했습니다. 대상Id={대상Id}", notification.대상Id);
            return;
        }

        shipperRequest.결제상태 = 상태값.결제상태.결제완료;
        shipperRequest.정산상태 = 운임정산상태.결제완료.ToString();
        shipperRequest.UpdatedAt = DateTime.UtcNow;

        var createDispatchQueue = TryParseSettlementTime(shipperRequest.정산시점) == 정산시점.선결제;
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
                    배차업무유형 = 상태값.배차업무유형.용달운송,
                    원본의뢰유형 = "CargoTransport",
                    원본의뢰Id = shipperRequest.의뢰Id,
                    픽업_도로명주소 = shipperRequest.픽업_도로명주소,
                    픽업_상세주소 = shipperRequest.픽업_상세주소,
                    픽업_위도 = shipperRequest.픽업_위도,
                    픽업_경도 = shipperRequest.픽업_경도,
                    하차_도로명주소 = shipperRequest.하차_도로명주소,
                    하차_상세주소 = shipperRequest.하차_상세주소,
                    하차_위도 = shipperRequest.하차_위도,
                    하차_경도 = shipperRequest.하차_경도,
                    상태 = 상태값.배차대기상태.대기,
                    배차큐단계 = 상태값.배차큐단계.계획배차,
                    배차노출상태 = 상태값.배차노출상태.계획대기,
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

        _logger.LogInformation(
            "결제승인완료 후처리 완료(용달운송의뢰): 결제Id={결제Id}, 대상Id={대상Id}, 금액={금액}{통화}",
            notification.결제Id,
            notification.대상Id,
            notification.결제금액,
            notification.통화);
    }

    private static 정산시점 TryParseSettlementTime(string? value)
    {
        return Enum.TryParse<정산시점>(value, ignoreCase: false, out var parsed)
            ? parsed
            : 정산시점.선결제;
    }
}
