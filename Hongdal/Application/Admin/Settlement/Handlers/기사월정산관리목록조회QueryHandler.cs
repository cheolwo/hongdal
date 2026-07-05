using Hongdal.Contracts.Admin.Settlement;

namespace Hongdal.Application.Admin.Settlement;

public sealed class 기사월정산관리목록조회QueryHandler : IRequestHandler<기사월정산관리목록조회Query, IReadOnlyList<기사월정산관리응답>>
{
    private const decimal 월상한금액 = 5000m;
    private readonly HongdalContext _db;

    public 기사월정산관리목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<기사월정산관리응답>> Handle(기사월정산관리목록조회Query request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var targetYear = request.Year ?? now.Year;
        var targetMonth = request.Month ?? now.Month;

        var query = _db.기사월정산.AsNoTracking().Where(x => x.년도 == targetYear && x.월 == targetMonth);

        if (!string.IsNullOrWhiteSpace(request.DriverId))
        {
            var id = request.DriverId.Trim();
            query = query.Where(x => x.기사Id == id);
        }

        return await query
            .OrderBy(x => x.기사Id)
            .Select(x => new 기사월정산관리응답
            {
                기사Id = x.기사Id,
                년도 = x.년도,
                월 = x.월,
                배차건수 = x.배차건수,
                이용료 = x.이용료,
                월상한적용여부 = x.이용료 >= 월상한금액,
                결제완료 = x.결제완료,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
