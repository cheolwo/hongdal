using Ssalddel.Contracts.Shipper.Request;

namespace Ssalddel.Application.Shipper.Request;

public sealed class 공개화물요약조회QueryHandler : IRequestHandler<공개화물요약조회Query, IReadOnlyList<공개화물요약응답>>
{
    private readonly SsalddelContext _db;

    public 공개화물요약조회QueryHandler(SsalddelContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<공개화물요약응답>> Handle(공개화물요약조회Query request, CancellationToken cancellationToken)
    {
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 || request.PageSize > 200 ? 50 : request.PageSize;

        var items = await _db.화주운송의뢰
            .AsNoTracking()
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return items.Select(화주운송의뢰매퍼.To공개화물요약응답).ToList();
    }
}
