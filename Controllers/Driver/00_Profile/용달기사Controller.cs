using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hongdal.Contracts.Driver.Profile;

namespace Hongdal.Controllers.Driver.Profile01;

[ApiController]
[Route("api/v1/drivers")]
[Authorize]
public sealed class 용달기사Controller : ControllerBase
{
    private readonly HongdalContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IDriverPushTokenStore _pushTokenStore;

    public 용달기사Controller(
        HongdalContext db,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IDriverPushTokenStore pushTokenStore)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _pushTokenStore = pushTokenStore;
    }

    [HttpPost("register")]
    public async Task<IActionResult> 용달기사등록([FromBody] 용달기사등록요청 request)
    {
        if (request == null)
        {
            return BadRequest("request body is required");
        }

        if (string.IsNullOrWhiteSpace(request.기사명))
        {
            return BadRequest("기사명 is required");
        }

        if (string.IsNullOrWhiteSpace(request.연락처))
        {
            return BadRequest("연락처 is required");
        }

        if (string.IsNullOrWhiteSpace(request.차량))
        {
            return BadRequest("차량 is required");
        }

        if (string.IsNullOrWhiteSpace(request.주_활동지역))
        {
            return BadRequest("주_활동지역 is required");
        }

        var driverId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(driverId))
        {
            return Unauthorized();
        }

        var existing = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == driverId);
        if (existing != null)
        {
            return Conflict(new { message = "이미 등록된 용달기사입니다." });
        }

        var user = await _userManager.FindByIdAsync(driverId);
        if (user == null)
        {
            return Unauthorized();
        }

        if (!await _roleManager.RoleExistsAsync(역할명.기사))
        {
            await _roleManager.CreateAsync(new IdentityRole(역할명.기사));
        }

        if (!await _userManager.IsInRoleAsync(user, 역할명.기사))
        {
            await _userManager.AddToRoleAsync(user, 역할명.기사);
        }

        var driver = new 용달기사
        {
            NotionPageId = Guid.NewGuid().ToString("N"),
            기사명 = request.기사명.Trim(),
            기사Id = driverId,
            상태 = string.IsNullOrWhiteSpace(request.상태) ? "활동중" : request.상태.Trim(),
            연락처 = request.연락처.Trim(),
            차량 = request.차량.Trim(),
            운행상태 = 상태값.기사운행상태.대기,
            주_활동지역 = request.주_활동지역.Trim(),
            메모 = request.메모?.Trim() ?? string.Empty,
            등록일 = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.용달기사.Add(driver);
        await _db.SaveChangesAsync();

        var vehicle = await _db.차량제원.AsNoTracking()
            .FirstOrDefaultAsync(x => x.차량코드 == driver.차량 || x.차량명 == driver.차량);

        var pushToken = await _pushTokenStore.GetAsync(driverId);

        return CreatedAtAction(nameof(내용달기사조회), new { }, new 용달기사등록응답
        {
            기사Id = driver.기사Id,
            기사명 = driver.기사명,
            연락처 = driver.연락처,
            차량 = driver.차량,
            차량코드 = vehicle?.차량코드,
            차량명 = vehicle?.차량명,
            주_활동지역 = driver.주_활동지역,
            상태 = driver.상태,
            운행상태 = driver.운행상태,
            등록일 = driver.등록일,
            메모 = driver.메모,
            푸시토큰등록됨 = !string.IsNullOrWhiteSpace(pushToken)
        });
    }

    [HttpGet("me")]
    public async Task<IActionResult> 내용달기사조회()
    {
        var driverId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(driverId))
        {
            return Unauthorized();
        }

        var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == driverId);
        if (driver == null)
        {
            return NotFound();
        }

        var vehicle = await _db.차량제원.AsNoTracking()
            .FirstOrDefaultAsync(x => x.차량코드 == driver.차량 || x.차량명 == driver.차량);
        var pushToken = await _pushTokenStore.GetAsync(driverId);

        return Ok(new 용달기사등록응답
        {
            기사Id = driver.기사Id,
            기사명 = driver.기사명,
            연락처 = driver.연락처,
            차량 = driver.차량,
            차량코드 = vehicle?.차량코드,
            차량명 = vehicle?.차량명,
            주_활동지역 = driver.주_활동지역,
            상태 = driver.상태,
            운행상태 = driver.운행상태,
            등록일 = driver.등록일,
            메모 = driver.메모,
            푸시토큰등록됨 = !string.IsNullOrWhiteSpace(pushToken)
        });
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? User.FindFirstValue("sub");
    }
}

