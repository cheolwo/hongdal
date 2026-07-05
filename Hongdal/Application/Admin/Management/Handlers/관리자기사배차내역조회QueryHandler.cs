using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Management;

public sealed class 관리자기사배차내역조회QueryHandler : IRequestHandler<관리자기사배차내역조회Query, IReadOnlyList<기사배차내역응답>>
{
    private readonly HongdalContext _db;

    public 관리자기사배차내역조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<기사배차내역응답>> Handle(관리자기사배차내역조회Query request, CancellationToken cancellationToken)
    {
        var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == request.DriverId, cancellationToken);
        if (driver == null)
        {
            return [];
        }

        return await _db.기사배차
            .AsNoTracking()
            .Where(x => x.용달기사_id == driver.Id || x.기사Id == driver.Id)
            .OrderByDescending(x => x.배차일)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new 기사배차내역응답
            {
                Id = x.Id,
                배차명 = x.배차명,
                상태 = x.상태,
                배차일 = x.배차일,
                픽업지 = x.픽업지,
                배송지 = x.배송지,
                배차점수 = x.배차점수,
                실패사유 = x.실패사유,
                배차생성시각 = x.배차생성시각,
                배차완료시각 = x.배차완료시각
            })
            .ToListAsync(cancellationToken);
    }
}
