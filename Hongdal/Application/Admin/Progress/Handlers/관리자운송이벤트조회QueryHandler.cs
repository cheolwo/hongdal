using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Progress;

public sealed class 관리자운송이벤트조회QueryHandler : IRequestHandler<관리자운송이벤트조회Query, IReadOnlyList<운송이벤트로그응답>>
{
    private readonly HongdalContext _db;

    public 관리자운송이벤트조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<운송이벤트로그응답>> Handle(관리자운송이벤트조회Query request, CancellationToken cancellationToken)
    {
        var query = _db.운송이벤트.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.RequestId))
        {
            var req = request.RequestId.Trim();
            query = query.Where(x => x.의뢰Id == req);
        }

        return await query
            .OrderByDescending(x => x.이벤트시각)
            .Take(200)
            .Select(x => new 운송이벤트로그응답
            {
                Id = x.Id,
                의뢰Id = x.의뢰Id,
                이벤트타입 = x.이벤트타입,
                이벤트시각 = x.이벤트시각,
                메타데이터 = x.메타데이터
            })
            .ToListAsync(cancellationToken);
    }
}
