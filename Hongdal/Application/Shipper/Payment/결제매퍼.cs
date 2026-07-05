using Hongdal.Contracts.Shipper.Payment;
using 홍달.Services.Options;

namespace Hongdal.Application.Shipper.Payment;

internal static class 결제매퍼
{
    internal static 결제목록응답 To목록응답(홍달.도메인.결제.결제 entity)
    {
        return new 결제목록응답
        {
            결제Id = entity.결제Id,
            의뢰Id = entity.의뢰Id,
            화주Id = entity.화주Id,
            결제금액 = entity.결제금액,
            결제수단 = entity.결제수단,
            결제상태 = entity.결제상태,
            OrderId = entity.OrderId,
            PaymentKey = entity.PaymentKey,
            Toss응답Json = entity.Toss응답Json,
            생성일시Utc = entity.CreatedAt,
            승인일시Utc = entity.승인일시
        };
    }

    internal static 토스결제환경응답 To환경응답(TossPaymentsOptions options)
    {
        return new 토스결제환경응답
        {
            ClientKey = options.ClientKey,
            BaseUrl = options.BaseUrl,
            IsConfigured = !string.IsNullOrWhiteSpace(options.ClientKey) && !string.IsNullOrWhiteSpace(options.SecretKey)
        };
    }
}
