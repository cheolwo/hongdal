using MediatR;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.Connections.Queries;

namespace Ssalddel.Application.Connections.Handlers;

public sealed class 내친구요청함조회QueryHandler : IRequestHandler<내친구요청함조회Query, IReadOnlyList<친구요청항목응답>>
{
    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public 내친구요청함조회QueryHandler(SsalddelContext db, ICurrentUserAccessor currentUserAccessor)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<IReadOnlyList<친구요청항목응답>> Handle(내친구요청함조회Query request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_currentUserAccessor.UserId))
        {
            return Array.Empty<친구요청항목응답>();
        }

        var page = Math.Max(1, request.페이지);
        var pageSize = Math.Clamp(request.페이지크기, 1, 200);

        return await _db.친구요청
            .AsNoTracking()
            .Where(x => x.요청자참여자Id == _currentUserAccessor.UserId)
            .OrderByDescending(x => x.요청일시)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new 친구요청항목응답
            {
                친구요청Id = x.Id,
                요청자참여자Id = x.요청자참여자Id,
                대상자참여자Id = x.대상자참여자Id,
                요청자역할 = x.요청자역할.ToString(),
                대상자역할 = x.대상자역할.ToString(),
                상태 = x.상태.ToString(),
                요청목적 = x.요청목적,
                요청메시지 = x.요청메시지,
                요청일시 = x.요청일시,
                응답일시 = x.응답일시,
                거절사유 = x.거절사유
            })
            .ToArrayAsync(cancellationToken);
    }
}
