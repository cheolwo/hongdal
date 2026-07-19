using FluentResults;
using Ssalddel.Contracts.Shipper.Payment;

namespace Ssalddel.Application.Shipper.Payment;

public sealed record 페이크결제승인Command(
    string 의뢰Id,
    int Amount,
    string? 결제수단,
    string? 메모,
    string? IdempotencyKey)
    : IRequest<Result<페이크결제승인응답>>;
