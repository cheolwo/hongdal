using 살뜰.도메인.결제;
using 살뜰.도메인.공통;

namespace Ssalddel.Application.Driver.Transport;

public sealed partial class 운송완료입금요청Service
{
    private async Task<살뜰.도메인.결제.결제> 결제대기건가져오거나생성Async(
        살뜰.도메인.화주.화주운송의뢰 request,
        운송입금요청Context context,
        int amount,
        CancellationToken cancellationToken)
    {
        var existing = await _db.결제
            .Where(x => x.대상Id == request.의뢰Id
                        && x.결제대상유형 == 결제공통정의.결제대상유형.용달운송의뢰
                        && x.결제제공자 == 결제공통정의.결제제공자.TossPayments
                        && x.결제수단 == 운송완료입금요청정책.토스가상계좌결제수단
                        && x.결제상태 == 상태값.결제상태.결제대기)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var paymentId = Guid.NewGuid().ToString("N");
        var orderId = 운송완료입금요청정책.주문번호생성(request.의뢰Id, context.운송Id);
        var payment = new 살뜰.도메인.결제.결제
        {
            결제Id = paymentId,
            의뢰Id = request.의뢰Id,
            화주Id = request.화주Id,
            결제대상유형 = 결제공통정의.결제대상유형.용달운송의뢰,
            대상Id = request.의뢰Id,
            PG사 = "TossPayments",
            결제제공자 = 결제공통정의.결제제공자.TossPayments,
            결제수단 = 운송완료입금요청정책.토스가상계좌결제수단,
            결제상태 = 상태값.결제상태.결제대기,
            공통결제상태 = 결제공통정의.결제상태.승인대기,
            결제금액 = amount,
            통화 = "KRW",
            OrderId = orderId,
            주문명 = $"살뜰 {context.정산메모} {request.의뢰Id}",
            원본응답Json = 운송완료입금요청정책.원본응답초안Json(request.의뢰Id, paymentId, orderId),
            CreatedAt = context.발생시각Utc
        };

        await _db.결제.AddAsync(payment, cancellationToken);
        return payment;
    }
}
