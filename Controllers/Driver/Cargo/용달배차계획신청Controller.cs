using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.공통;
using 홍달.도메인.기사;

namespace Hongdal.Controllers.Driver.Cargo;

[ApiController]
[Route("api/v1/drivers/dispatch-plans")]
[Authorize(Roles = 역할명.기사)]
public sealed class 용달배차계획신청Controller : ControllerBase
{
    private readonly HongdalContext _db;

    public 용달배차계획신청Controller(HongdalContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> 배차계획신청([FromBody] 배차계획신청요청 request)
    {
        if (request == null)
        {
            return BadRequest("request body is required");
        }

        if (string.IsNullOrWhiteSpace(request.출발지))
        {
            return BadRequest("출발지 is required");
        }

        if (string.IsNullOrWhiteSpace(request.복귀지))
        {
            return BadRequest("복귀지 is required");
        }

        var driverId = GetCurrentDriverId();
        if (string.IsNullOrWhiteSpace(driverId))
        {
            return Unauthorized();
        }

        var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == driverId);
        if (driver == null)
        {
            return NotFound(new { message = "용달기사 정보를 찾을 수 없습니다." });
        }

        var entity = new 배차계획신청
        {
            기사Id = driverId,
            출발지 = request.출발지.Trim(),
            복귀지 = request.복귀지.Trim(),
            희망복귀시각 = request.희망복귀시각,
            배차가능시각 = request.배차가능시각,
            상태 = string.IsNullOrWhiteSpace(request.상태) ? "신청" : request.상태.Trim(),
            메모 = request.메모?.Trim() ?? string.Empty,
            신청일시 = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.배차계획신청.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(내배차계획조회), new { }, Map(entity));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> 배차계획수정(long id, [FromBody] 배차계획수정요청 request)
    {
        if (request == null)
        {
            return BadRequest("request body is required");
        }

        if (string.IsNullOrWhiteSpace(request.출발지))
        {
            return BadRequest("출발지 is required");
        }

        if (string.IsNullOrWhiteSpace(request.복귀지))
        {
            return BadRequest("복귀지 is required");
        }

        var driverId = GetCurrentDriverId();
        if (string.IsNullOrWhiteSpace(driverId))
        {
            return Unauthorized();
        }

        var entity = await _db.배차계획신청.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        if (!string.Equals(entity.기사Id, driverId, StringComparison.Ordinal))
        {
            return Forbid();
        }

        if (string.Equals(entity.상태, "취소", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { message = "취소된 배차계획은 수정할 수 없습니다." });
        }

        entity.출발지 = request.출발지.Trim();
        entity.복귀지 = request.복귀지.Trim();
        entity.희망복귀시각 = request.희망복귀시각;
        entity.배차가능시각 = request.배차가능시각;
        entity.메모 = request.메모?.Trim() ?? string.Empty;
        entity.상태 = string.IsNullOrWhiteSpace(request.상태) ? entity.상태 : request.상태.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(Map(entity));
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> 배차계획취소(long id)
    {
        var driverId = GetCurrentDriverId();
        if (string.IsNullOrWhiteSpace(driverId))
        {
            return Unauthorized();
        }

        var entity = await _db.배차계획신청.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return NotFound();
        }

        if (!string.Equals(entity.기사Id, driverId, StringComparison.Ordinal))
        {
            return Forbid();
        }

        if (string.Equals(entity.상태, "취소", StringComparison.OrdinalIgnoreCase))
        {
            return Ok(Map(entity));
        }

        entity.상태 = "취소";
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(Map(entity));
    }

    [HttpGet("me")]
    public async Task<IActionResult> 내배차계획조회()
    {
        var driverId = GetCurrentDriverId();
        if (string.IsNullOrWhiteSpace(driverId))
        {
            return Unauthorized();
        }

        var entity = await _db.배차계획신청
            .AsNoTracking()
            .Where(x => x.기사Id == driverId)
            .OrderByDescending(x => x.신청일시)
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            return NotFound();
        }

        return Ok(Map(entity));
    }

    private string? GetCurrentDriverId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
    }

    private static 배차계획신청응답 Map(배차계획신청 entity)
    {
        return new 배차계획신청응답
        {
            Id = entity.Id,
            기사Id = entity.기사Id,
            출발지 = entity.출발지,
            복귀지 = entity.복귀지,
            희망복귀시각 = entity.희망복귀시각,
            배차가능시각 = entity.배차가능시각,
            상태 = entity.상태,
            메모 = entity.메모,
            신청일시 = entity.신청일시
        };
    }
}

public sealed class 배차계획신청요청
{
    public string 출발지 { get; set; } = string.Empty;
    public string 복귀지 { get; set; } = string.Empty;
    public DateTime? 희망복귀시각 { get; set; }
    public DateTime? 배차가능시각 { get; set; }
    public string? 상태 { get; set; }
    public string? 메모 { get; set; }
}

public sealed class 배차계획수정요청
{
    public string 출발지 { get; set; } = string.Empty;
    public string 복귀지 { get; set; } = string.Empty;
    public DateTime? 희망복귀시각 { get; set; }
    public DateTime? 배차가능시각 { get; set; }
    public string? 상태 { get; set; }
    public string? 메모 { get; set; }
}

public sealed class 배차계획신청응답
{
    public long Id { get; set; }
    public string 기사Id { get; set; } = string.Empty;
    public string 출발지 { get; set; } = string.Empty;
    public string 복귀지 { get; set; } = string.Empty;
    public DateTime? 희망복귀시각 { get; set; }
    public DateTime? 배차가능시각 { get; set; }
    public string 상태 { get; set; } = string.Empty;
    public string 메모 { get; set; } = string.Empty;
    public DateTime 신청일시 { get; set; }
}