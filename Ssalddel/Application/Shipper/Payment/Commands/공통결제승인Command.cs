using FluentResults;
using Ssalddel.Contracts.Common.Payments;

namespace Ssalddel.Application.Shipper.Payment;

public sealed record 공통결제승인Command(
    int 결제제공자,
    string PaymentKey,
    string OrderId,
    int Amount)
    : IRequest<Result<공통결제승인응답>>;
