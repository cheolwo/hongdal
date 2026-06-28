using Hongdal.Contracts.Driver.Settlement;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Application.Driver.Settlement;

public sealed class 기사정산목록조회QueryHandler : IRequestHandler<기사정산목록조회Query, IReadOnlyList<기사정산월요약응답>>
{
    private readonly HongdalContext _db;
    private readonly 기사이용료정책Options _policy;

    public 기사정산목록조회QueryHandler(HongdalContext db, IOptions<기사이용료정책Options> policy)
    {
        _db = db;
        _policy = policy.Value;
    }

    public async Task<IReadOnlyList<기사정산월요약응답>> Handle(기사정산목록조회Query request, CancellationToken cancellationToken)
    {
        var monthlyFeeCap = MonthCap();

        var items = await _db.기사월정산.AsNoTracking()
            .Where(x => x.기사Id == request.기사Id)
            .OrderByDescending(x => x.년도)
            .ThenByDescending(x => x.월)
            .ToListAsync(cancellationToken);

        return items.Select(x => 기사정산공통매퍼.To월요약응답(x, monthlyFeeCap)).ToList();
    }

    private decimal MonthCap()
    {
        return _policy.무료배차 ? 0 : _policy.추가이용료;
    }
}
