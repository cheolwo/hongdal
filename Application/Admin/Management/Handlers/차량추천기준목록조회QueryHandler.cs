using Hongdal.Contracts.Admin.Management;
using Microsoft.EntityFrameworkCore;
using 홍달.도메인.차량;

namespace Hongdal.Application.Admin.Management;

public sealed class 차량추천기준목록조회QueryHandler : IRequestHandler<차량추천기준목록조회Query, IReadOnlyList<차량추천기준응답>>
{
    private readonly HongdalContext _db;

    public 차량추천기준목록조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<차량추천기준응답>> Handle(차량추천기준목록조회Query request, CancellationToken cancellationToken)
    {
        List<차량제원> items = await _db.차량제원
            .AsNoTracking()
            .OrderBy(x => x.추천우선순위)
            .ThenBy(x => x.차량명)
            .ToListAsync<차량제원>(cancellationToken);

        return items.Select(차량추천관리매퍼.To응답).ToArray();
    }
}
