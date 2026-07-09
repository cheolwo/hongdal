using Hongdal.Contracts.Shipper.Request;
using 홍달.도메인.공통;

namespace Hongdal.Application.Driver.Transport;

public interface I운송완료입금요청Service
{
    Task<운송완료입금요청결과> 준비Async(운송인수완료됨Event notification, CancellationToken cancellationToken = default);
}

public sealed partial class 운송완료입금요청Service : I운송완료입금요청Service
{
    private readonly HongdalContext _db;

    public 운송완료입금요청Service(HongdalContext db)
    {
        _db = db;
    }

    public async Task<운송완료입금요청결과> 준비Async(
        운송인수완료됨Event notification,
        CancellationToken cancellationToken = default)
    {
        var request = await _db.화주운송의뢰
            .FirstOrDefaultAsync(x => x.의뢰Id == notification.운송번호, cancellationToken);
        if (request is null)
        {
            return new 운송완료입금요청결과(false, "화주 운송 의뢰 없음");
        }

        request.배차상태 = 상태값.배차상태.인수완료;

        if (!운송완료입금요청정책.입금요청대상인가(request))
        {
            request.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return new 운송완료입금요청결과(false, "입금 요청 대상 아님", request.의뢰Id);
        }

        var amount = 운송완료입금요청정책.입금요청금액(request);
        if (amount <= 0)
        {
            request.정산상태 = 운임정산상태.미수발생.ToString();
            request.정산메모 = MergeMemo(request.정산메모, "운송 완료 후 입금 요청 금액을 산정하지 못했습니다.");
            request.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return new 운송완료입금요청결과(false, "입금 요청 금액 없음", request.의뢰Id);
        }

        var payment = await 결제대기건가져오거나생성Async(request, notification, amount, cancellationToken);
        request.결제상태 = 상태값.결제상태.결제대기;
        request.정산상태 = 운임정산상태.입금대기.ToString();
        request.결제수단 = 운송완료입금요청정책.토스가상계좌결제수단;
        request.정산메모 = MergeMemo(
            request.정산메모,
            $"운송 완료 후 토스페이먼츠 가상계좌 입금 요청 생성: 결제Id={payment.결제Id}, OrderId={payment.OrderId}");
        request.UpdatedAt = DateTime.UtcNow;

        var scheduledCount = await 입금요청알림예약Async(request, notification, payment, cancellationToken);

        await _db.SaveChangesAsync(cancellationToken);
        return new 운송완료입금요청결과(
            true,
            "입금 요청 준비 완료",
            request.의뢰Id,
            payment.결제Id,
            payment.OrderId,
            scheduledCount);
    }

    private static string MergeMemo(string existing, string memo)
        => string.IsNullOrWhiteSpace(existing) ? memo : $"{existing}\n{memo}";
}
