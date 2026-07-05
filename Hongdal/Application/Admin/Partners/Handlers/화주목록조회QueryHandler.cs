using Microsoft.AspNetCore.Identity;
using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Application.Admin.Partners;

public sealed class 화주목록조회QueryHandler : IRequestHandler<화주목록조회Query, IReadOnlyList<화주관리응답>>
{
    private readonly HongdalContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public 화주목록조회QueryHandler(HongdalContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<화주관리응답>> Handle(화주목록조회Query request, CancellationToken cancellationToken)
    {
        var shippers = await _userManager.GetUsersInRoleAsync(역할명.화주);

        var shipperIds = shippers.Select(x => x.Id).ToArray();

        var requestStats = await _db.화주운송의뢰
            .AsNoTracking()
            .Where(x => shipperIds.Contains(x.화주Id))
            .GroupBy(x => x.화주Id)
            .Select(g => new
            {
                화주Id = g.Key,
                의뢰수 = g.Count(),
                최근의뢰일시 = g.Max(x => (DateTime?)x.CreatedAt)
            })
            .ToDictionaryAsync(x => x.화주Id, x => (x.의뢰수, x.최근의뢰일시), cancellationToken);

        return shippers
            .OrderBy(x => x.UserName)
            .Select(x =>
            {
                requestStats.TryGetValue(x.Id, out var stat);

                return new 화주관리응답
                {
                    화주Id = x.Id,
                    사용자명 = x.UserName ?? string.Empty,
                    이메일 = x.Email ?? string.Empty,
                    연락처 = x.PhoneNumber ?? string.Empty,
                    의뢰건수 = stat.의뢰수,
                    최근의뢰일시 = stat.최근의뢰일시,
                    거래상태 = stat.의뢰수 > 0 ? "거래중" : "신규"
                };
            })
            .ToList();
    }
}
