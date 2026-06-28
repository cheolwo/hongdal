using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Controllers.Admin.Progress03;

[ApiController]
[Route("api/v1/admin/transports")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 운송진행관리Controller : ControllerBase
{
    private readonly HongdalContext _db;

    public 운송진행관리Controller(HongdalContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> 운송목록조회([FromQuery] string? 상태)
    {
        var query = _db.배송_운송.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(상태))
        {
            var status = 상태.Trim();
            query = query.Where(x => x.상태 == status);
        }

        var items = await query
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new 운송진행응답
            {
                Id = x.Id,
                운송번호 = x.운송번호,
                상태 = x.상태,
                출발_픽업 = x.출발_픽업,
                도착 = x.도착,
                기사_운송자 = x.기사_운송자,
                출발지 = x.출발지,
                도착지 = x.도착지,
                운임 = x.운임,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("events")]
    public async Task<IActionResult> 운송이벤트조회([FromQuery] string? requestId)
    {
        var query = _db.운송이벤트.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(requestId))
        {
            var req = requestId.Trim();
            query = query.Where(x => x.의뢰Id == req);
        }

        var items = await query
            .OrderByDescending(x => x.이벤트시각)
            .Take(200)
            .Select(x => new 운송이벤트로그응답
            {
                Id = x.Id,
                의뢰Id = x.의뢰Id,
                이벤트타입 = x.이벤트타입,
                이벤트시각 = x.이벤트시각,
                메타데이터 = x.메타데이터
            })
            .ToListAsync();

        return Ok(items);
    }
}
