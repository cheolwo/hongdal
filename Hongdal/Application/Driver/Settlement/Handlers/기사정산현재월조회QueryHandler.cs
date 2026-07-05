using Hongdal.Contracts.Driver.Settlement;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;
using 홍달.Services.Settlement;

namespace Hongdal.Application.Driver.Settlement;

public sealed class 기사정산현재월조회QueryHandler : IRequestHandler<기사정산현재월조회Query, 기사정산응답>
{
    private readonly HongdalContext _db;
    private readonly I기사월정산Service _settlementService;
    private readonly 기사이용료정책Options _policy;

    public 기사정산현재월조회QueryHandler(HongdalContext db, I기사월정산Service settlementService, IOptions<기사이용료정책Options> policy)
    {
        _db = db;
        _settlementService = settlementService;
        _policy = policy.Value;
    }

    public async Task<기사정산응답> Handle(기사정산현재월조회Query request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var settlement = await _db.기사월정산.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == request.기사Id && x.년도 == now.Year && x.월 == now.Month, cancellationToken);

        if (settlement == null)
        {
            settlement = await _settlementService.배차확정반영Async(request.기사Id, now, cancellationToken);
        }

        return 기사정산공통매퍼.To응답(settlement, MonthCap());
    }

    private decimal MonthCap()
    {
        return _policy.무료배차 ? 0 : _policy.추가이용료;
    }
}
