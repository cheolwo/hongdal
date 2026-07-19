using Ssalddel.Contracts.Driver.Settlement;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Application.Driver.Settlement;

public sealed class 기사정산현재월조회QueryHandler : IRequestHandler<기사정산현재월조회Query, 기사정산응답>
{
    private readonly SsalddelContext _db;
    private readonly 기사이용료정책Options _policy;

    public 기사정산현재월조회QueryHandler(SsalddelContext db, IOptions<기사이용료정책Options> policy)
    {
        _db = db;
        _policy = policy.Value;
    }

    public async Task<기사정산응답> Handle(기사정산현재월조회Query request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var settlement = await _db.기사월정산.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == request.기사Id && x.년도 == now.Year && x.월 == now.Month, cancellationToken);

        if (settlement == null)
        {
            settlement = new 기사월정산
            {
                기사Id = request.기사Id,
                년도 = now.Year,
                월 = now.Month,
                배차건수 = 0,
                이용료 = 0m,
                결제완료 = false,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        return 기사정산공통매퍼.To응답(settlement, MonthCap());
    }

    private decimal MonthCap()
    {
        return _policy.적용월상한이용료;
    }
}
