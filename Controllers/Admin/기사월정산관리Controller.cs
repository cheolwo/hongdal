using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/driver-settlements")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 기사월정산관리Controller : ControllerBase
{
    private const decimal 월상한금액 = 5000m;
    private readonly HongdalContext _db;

    public 기사월정산관리Controller(HongdalContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> 목록조회([FromQuery] int? year, [FromQuery] int? month, [FromQuery] string? driverId)
    {
        var now = DateTime.UtcNow;
        var targetYear = year ?? now.Year;
        var targetMonth = month ?? now.Month;

        var query = _db.기사월정산
            .AsNoTracking()
            .Where(x => x.년도 == targetYear && x.월 == targetMonth);

        if (!string.IsNullOrWhiteSpace(driverId))
        {
            var id = driverId.Trim();
            query = query.Where(x => x.기사Id == id);
        }

        var items = await query
            .OrderBy(x => x.기사Id)
            .Select(x => new 기사월정산관리응답
            {
                기사Id = x.기사Id,
                년도 = x.년도,
                월 = x.월,
                배차건수 = x.배차건수,
                이용료 = x.이용료,
                월상한적용여부 = x.이용료 >= 월상한금액,
                결제완료 = x.결제완료,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return Ok(items);
    }
}

public sealed class 기사월정산관리응답
{
    public string 기사Id { get; set; } = string.Empty;
    public int 년도 { get; set; }
    public int 월 { get; set; }
    public int 배차건수 { get; set; }
    public decimal 이용료 { get; set; }
    public bool 월상한적용여부 { get; set; }
    public bool 결제완료 { get; set; }
    public DateTime UpdatedAt { get; set; }
}
