using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Operating;

public sealed class 운송이벤트목록조회QueryHandler : IRequestHandler<운송이벤트목록조회Query, IReadOnlyList<운송이벤트로그응답>>
{
    private readonly HongdalContext _db;

    public 운송이벤트목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<운송이벤트로그응답>> Handle(운송이벤트목록조회Query request, CancellationToken cancellationToken)
    {
        return await _db.운송이벤트
            .AsNoTracking()
            .OrderBy(e => e.이벤트시각)
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
