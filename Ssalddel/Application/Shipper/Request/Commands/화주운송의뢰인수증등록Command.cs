using FluentResults;

namespace Ssalddel.Application.Shipper.Request;

public sealed record 화주운송의뢰인수증등록Command(string RequestId, string 인수증번호, string? 등록메모) : IRequest<Result<Ssalddel.Contracts.Shipper.Request.화주운송의뢰응답>>;
