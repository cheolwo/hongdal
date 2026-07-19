using FluentResults;
using Ssalddel.Contracts.Common.Payments;

namespace Ssalddel.Application.Shipper.Payment;

public sealed record 공통결제준비Command(
    int 결제대상유형,
    string 대상Id,
    int 결제제공자,
    int 금액,
    string? 주문명)
    : IRequest<Result<공통결제준비응답>>;
