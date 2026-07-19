using FluentResults;

namespace Ssalddel.Application.Shipper.Payment;

public sealed record 토스결제준비Command(string 의뢰Id, int Amount) : IRequest<Result<Ssalddel.Contracts.Shipper.Payment.토스결제준비응답>>;
