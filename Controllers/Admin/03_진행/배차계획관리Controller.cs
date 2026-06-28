using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Controllers.Admin.Progress03;

[ApiController]
[Route("api/v1/admin/dispatch-plans")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 배차계획관리Controller : ControllerBase
{
    private readonly HongdalContext _db;

    public 배차계획관리Controller(HongdalContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? 기사Id,
        [FromQuery] string? 상태,
        [FromQuery] DateTime? 신청일From,
        [FromQuery] DateTime? 신청일To)
    {
        var query = _db.배차계획신청
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(기사Id))
        {
            var driverId = 기사Id.Trim();
            query = query.Where(x => x.기사Id == driverId);
        }

        if (!string.IsNullOrWhiteSpace(상태))
        {
            var status = 상태.Trim();
            query = query.Where(x => x.상태 == status);
        }

        if (신청일From.HasValue)
        {
            var from = 신청일From.Value.Date;
            query = query.Where(x => x.신청일시 >= from);
        }

        if (신청일To.HasValue)
        {
            var toExclusive = 신청일To.Value.Date.AddDays(1);
            query = query.Where(x => x.신청일시 < toExclusive);
        }

        var items = await query
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
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> 단건조회(long id)
    {
        var item = await _db.배차계획신청
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new 배차계획관리상세응답
            {
                Id = x.Id,
                기사Id = x.기사Id,
                기사명 = _db.용달기사.Where(d => d.기사Id == x.기사Id).Select(d => d.기사명).FirstOrDefault() ?? string.Empty,
                연락처 = _db.용달기사.Where(d => d.기사Id == x.기사Id).Select(d => d.연락처).FirstOrDefault() ?? string.Empty,
                출발지 = x.출발지,
                복귀지 = x.복귀지,
                희망복귀시각 = x.희망복귀시각,
                배차가능시각 = x.배차가능시각,
                상태 = x.상태,
                메모 = x.메모,
                신청일시 = x.신청일시,
                최근수정시각 = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }
}
