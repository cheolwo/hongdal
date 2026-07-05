using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Application.Admin.Operating;

public sealed class 배차계획목록조회QueryHandler : IRequestHandler<배차계획목록조회Query, IReadOnlyList<배차계획관리목록응답>>
{
    private readonly HongdalContext _db;

    public 배차계획목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<배차계획관리목록응답>> Handle(배차계획목록조회Query request, CancellationToken cancellationToken)
    {
        var query = _db.배차계획신청.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.기사Id))
        {
            var driverId = request.기사Id.Trim();
            query = query.Where(x => x.기사Id == driverId);
        }

        if (!string.IsNullOrWhiteSpace(request.상태))
        {
            var status = request.상태.Trim();
            query = query.Where(x => x.상태 == status);
        }

        if (request.신청일From.HasValue)
        {
            var from = request.신청일From.Value.Date;
            query = query.Where(x => x.신청일시 >= from);
        }

        if (request.신청일To.HasValue)
        {
            var toExclusive = request.신청일To.Value.Date.AddDays(1);
            query = query.Where(x => x.신청일시 < toExclusive);
        }

        return await query
            .OrderByDescending(x => x.신청일시)
            .Select(x => new 배차계획관리목록응답
            {
                Id = x.Id,
                기사Id = x.기사Id,
                기사명 = _db.용달기사.Where(d => d.기사Id == x.기사Id).Select(d => d.기사명).FirstOrDefault() ?? string.Empty,
                출발지 = x.출발지,
                복귀지 = x.복귀지,
                희망복귀시각 = x.희망복귀시각,
                배차가능시각 = x.배차가능시각,
                상태 = x.상태,
                메모 = x.메모,
                신청일시 = x.신청일시,
                최근수정시각 = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
