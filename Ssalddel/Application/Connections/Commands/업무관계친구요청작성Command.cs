using FluentResults;
using MediatR;

namespace Ssalddel.Application.Connections.Commands;

public sealed record 업무관계친구요청작성Command(
    Guid 업무관계스냅샷Id,
    string 요청목적,
    string 요청메시지) : IRequest<Result<long>>;
