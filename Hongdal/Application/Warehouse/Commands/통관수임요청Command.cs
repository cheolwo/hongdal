using FluentResults;

namespace Hongdal.Application.Warehouse;

public sealed record 통관수임요청Command(
    string 관세사참여자Id,
    long 통관절차Id,
    string? 메모) : IRequest<Result<Unit>>;
