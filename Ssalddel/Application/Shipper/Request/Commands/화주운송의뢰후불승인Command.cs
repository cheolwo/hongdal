using FluentResults;

namespace Ssalddel.Application.Shipper.Request;

public sealed record 화주운송의뢰후불승인Command(string RequestId, string? 승인메모) : IRequest<Result<Ssalddel.Contracts.Shipper.Request.화주운송의뢰응답>>;
