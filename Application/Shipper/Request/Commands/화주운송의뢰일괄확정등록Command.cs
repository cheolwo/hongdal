using FluentResults;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed record 화주운송의뢰일괄확정등록Command(
    IReadOnlyList<화주운송의뢰일괄확정등록행> 행목록) : IRequest<Result<화주운송의뢰일괄등록결과응답>>;
