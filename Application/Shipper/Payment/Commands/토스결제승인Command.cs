using FluentResults;

namespace Hongdal.Application.Shipper.Payment;

public sealed record 토스결제승인Command(string PaymentKey, string OrderId, int Amount) : IRequest<Result<Hongdal.Contracts.Shipper.Payment.토스결제승인응답>>;
