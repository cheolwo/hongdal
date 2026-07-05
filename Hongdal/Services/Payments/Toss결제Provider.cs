using 홍달.도메인.결제;
using 홍달.Services.External.Toss;

namespace 홍달.Services.Payments;

public sealed class Toss결제Provider : I결제Provider
{
    private readonly ITossPaymentsService _tossPaymentsService;

    public Toss결제Provider(ITossPaymentsService tossPaymentsService)
    {
        _tossPaymentsService = tossPaymentsService;
    }

    public int 제공자유형 => 결제공통정의.결제제공자.TossPayments;

    public async Task<결제승인결과> 결제승인Async(결제승인요청 request, CancellationToken cancellationToken = default)
    {
        var result = await _tossPaymentsService.ConfirmAsync(new TossConfirmApiRequest(
            request.PaymentKey,
            request.OrderId,
            request.Amount));

        return new 결제승인결과(
            result.IsSuccess,
            result.ResponseJson,
            result.PaymentMethod,
            request.PaymentKey);
    }
}
