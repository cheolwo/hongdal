using FluentResults;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Shipper.Request;

public sealed record 화주운송의뢰일괄미리보기Command(
    IReadOnlyList<화주운송의뢰일괄등록행입력> 행목록,
    IReadOnlyList<string> 파싱오류목록) : IRequest<Result<화주운송의뢰일괄미리보기응답>>;
