using FluentResults;

namespace Hongdal.Application.Shipper.Request;

public sealed record 화주운송의뢰현장지급처리Command(string RequestId, string? 현장지급메모) : IRequest<Result<Hongdal.Contracts.Shipper.Request.화주운송의뢰응답>>;
