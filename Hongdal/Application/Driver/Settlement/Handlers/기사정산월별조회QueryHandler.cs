using Hongdal.Contracts.Driver.Settlement;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Application.Driver.Settlement;

public sealed class 기사정산월별조회QueryHandler : IRequestHandler<기사정산월별조회Query, 기사정산응답?>
{
    private readonly HongdalContext _db;
    private readonly 기사이용료정책Options _policy;

    public 기사정산월별조회QueryHandler(HongdalContext db, IOptions<기사이용료정책Options> policy)
    {
        _db = db;
        _policy = policy.Value;
    }

    public async Task<기사정산응답?> Handle(기사정산월별조회Query request, CancellationToken cancellationToken)
    {
        if (request.Year < 1 || request.Month < 1 || request.Month > 12)
        {
            return null;
        }

        var settlement = await _db.기사월정산.AsNoTracking()
            .FirstOrDefaultAsync(x => x.기사Id == request.기사Id && x.년도 == request.Year && x.월 == request.Month, cancellationToken);

        return settlement == null ? null : 기사정산공통매퍼.To응답(settlement, MonthCap());
    }

    private decimal MonthCap()
    {
        return _policy.무료배차 ? 0 : _policy.추가이용료;
    }
}
