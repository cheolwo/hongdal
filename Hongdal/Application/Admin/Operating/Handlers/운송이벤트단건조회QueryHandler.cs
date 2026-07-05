using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Operating;

public sealed class 운송이벤트단건조회QueryHandler : IRequestHandler<운송이벤트단건조회Query, 운송이벤트로그응답?>
{
    private readonly HongdalContext _db;

    public 운송이벤트단건조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<운송이벤트로그응답?> Handle(운송이벤트단건조회Query request, CancellationToken cancellationToken)
    {
        return await _db.운송이벤트
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new 운송이벤트로그응답
            {
                Id = x.Id,
                의뢰Id = x.의뢰Id,
                이벤트타입 = x.이벤트타입,
                이벤트시각 = x.이벤트시각,
                메타데이터 = x.메타데이터
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
