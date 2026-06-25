using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/partners")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 업체화주관리Controller : ControllerBase
{
    private readonly HongdalContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public 업체화주관리Controller(HongdalContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("companies")]
    public async Task<IActionResult> 업체목록조회([FromQuery] string? 상태)
    {
        var query = _db.업체.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(상태))
        {
            var status = 상태.Trim();
            query = query.Where(x => x.상태 == status);
        }

        var items = await query
            .OrderBy(x => x.업체명)
            .Select(x => new 업체관리응답
            {
                Id = x.Id,
                업체명 = x.업체명,
                상태 = x.상태,
                대표연락처 = x.대표_연락처,
                담당자 = x.담당자,
                이메일 = x.이메일,
                주소 = x.주소,
                정산결제조건 = x.정산_결제_조건,
                등록일 = x.등록일
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("shippers")]
    public async Task<IActionResult> 화주목록조회()
    {
        var shippers = await _userManager.GetUsersInRoleAsync(역할명.화주);

        var shipperIds = shippers
            .Select(x => x.Id)
            .ToArray();

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
            .ToDictionaryAsync(x => x.화주Id, x => (x.의뢰수, x.최근의뢰일시));

        var items = shippers
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

        return Ok(items);
    }
}

public sealed class 업체관리응답
{
    public long Id { get; set; }
    public string 업체명 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 대표연락처 { get; set; } = string.Empty;
    public string 담당자 { get; set; } = string.Empty;
    public string 이메일 { get; set; } = string.Empty;
    public string 주소 { get; set; } = string.Empty;
    public string 정산결제조건 { get; set; } = string.Empty;
    public DateTime? 등록일 { get; set; }
}

public sealed class 화주관리응답
{
    public string 화주Id { get; set; } = string.Empty;
    public string 사용자명 { get; set; } = string.Empty;
    public string 이메일 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public int 의뢰건수 { get; set; }
    public DateTime? 최근의뢰일시 { get; set; }
    public string 거래상태 { get; set; } = string.Empty;
}
