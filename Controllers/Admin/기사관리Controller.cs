using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;

namespace Hongdal.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/drivers")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 기사관리Controller : ControllerBase
{
    private readonly HongdalContext _db;

    public 기사관리Controller(HongdalContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> 목록조회(
        [FromQuery] string? 운행상태,
        [FromQuery] string? 차량종류,
        [FromQuery] string? 활동지역검색어)
    {
        var query = _db.용달기사.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(운행상태))
        {
            var status = 운행상태.Trim();
            query = query.Where(x => x.운행상태 == status);
        }

        if (!string.IsNullOrWhiteSpace(차량종류))
        {
            var vehicle = 차량종류.Trim();
            query = query.Where(x => x.차량 == vehicle);
        }

        if (!string.IsNullOrWhiteSpace(활동지역검색어))
        {
            var keyword = 활동지역검색어.Trim();
            query = query.Where(x => x.주_활동지역.Contains(keyword));
        }

        var items = await query
            .OrderBy(x => x.기사명)
            .Select(x => new 기사목록응답
            {
                기사Id = x.기사Id,
                기사명 = x.기사명,
                연락처 = x.연락처,
                차량 = x.차량,
                주_활동지역 = x.주_활동지역,
                운행상태 = x.운행상태,
                최근위도 = _db.기사위치기록
                    .Where(l => l.기사Id == x.기사Id)
                    .OrderByDescending(l => l.기록시각)
                    .Select(l => (decimal?)l.위도)
                    .FirstOrDefault(),
                최근경도 = _db.기사위치기록
                    .Where(l => l.기사Id == x.기사Id)
                    .OrderByDescending(l => l.기록시각)
                    .Select(l => (decimal?)l.경도)
                    .FirstOrDefault(),
                최근위치기록시각 = _db.기사위치기록
                    .Where(l => l.기사Id == x.기사Id)
                    .OrderByDescending(l => l.기록시각)
                    .Select(l => (DateTime?)l.기록시각)
                    .FirstOrDefault(),
                배차건수 = _db.기사배차.Count(d => d.용달기사_id == x.Id || d.기사Id == x.Id)
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{driverId}")]
    public async Task<IActionResult> 단건조회(string driverId)
    {
        var item = await _db.용달기사
            .AsNoTracking()
            .Where(x => x.기사Id == driverId)
            .Select(x => new 기사상세응답
            {
                기사Id = x.기사Id,
                기사명 = x.기사명,
                연락처 = x.연락처,
                차량 = x.차량,
                주_활동지역 = x.주_활동지역,
                운행상태 = x.운행상태,
                메모 = x.메모,
                등록일 = x.등록일,
                최근위도 = _db.기사위치기록
                    .Where(l => l.기사Id == x.기사Id)
                    .OrderByDescending(l => l.기록시각)
                    .Select(l => (decimal?)l.위도)
                    .FirstOrDefault(),
                최근경도 = _db.기사위치기록
                    .Where(l => l.기사Id == x.기사Id)
                    .OrderByDescending(l => l.기록시각)
                    .Select(l => (decimal?)l.경도)
                    .FirstOrDefault(),
                최근위치기록시각 = _db.기사위치기록
                    .Where(l => l.기사Id == x.기사Id)
                    .OrderByDescending(l => l.기록시각)
                    .Select(l => (DateTime?)l.기록시각)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpGet("{driverId}/dispatches")]
    public async Task<IActionResult> 기사별배차내역(string driverId)
    {
        var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == driverId);
        if (driver == null)
        {
            return NotFound();
        }

        var items = await _db.기사배차
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
            .ToListAsync();

        return Ok(items);
    }
}

public sealed class 기사목록응답
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 주_활동지역 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public decimal? 최근위도 { get; set; }
    public decimal? 최근경도 { get; set; }
    public DateTime? 최근위치기록시각 { get; set; }
    public int 배차건수 { get; set; }
}

public sealed class 기사상세응답
{
    public string 기사Id { get; set; } = string.Empty;
    public string 기사명 { get; set; } = string.Empty;
    public string 연락처 { get; set; } = string.Empty;
    public string 차량 { get; set; } = string.Empty;
    public string 주_활동지역 { get; set; } = string.Empty;
    public string 운행상태 { get; set; } = string.Empty;
    public string 메모 { get; set; } = string.Empty;
    public DateTime? 등록일 { get; set; }
    public decimal? 최근위도 { get; set; }
    public decimal? 최근경도 { get; set; }
    public DateTime? 최근위치기록시각 { get; set; }
}

public sealed class 기사배차내역응답
{
    public long Id { get; set; }
    public string 배차명 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public DateTime? 배차일 { get; set; }
    public string 픽업지 { get; set; } = string.Empty;
    public string 배송지 { get; set; } = string.Empty;
    public decimal? 배차점수 { get; set; }
    public string 실패사유 { get; set; } = string.Empty;
    public DateTime? 배차생성시각 { get; set; }
    public DateTime? 배차완료시각 { get; set; }
}
