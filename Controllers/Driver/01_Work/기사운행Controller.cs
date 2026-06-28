using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.공통;
using 홍달.Services;
using 홍달.Services.Storage.Local;
using 홍달.도메인.기사;
using Hongdal.Contracts.Driver.Work;

namespace Hongdal.Controllers.Driver.Work01
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/work")]
    public sealed class 기사운행Controller : ControllerBase
    {
        private readonly HongdalContext _db;
        private readonly I배차추천Service _dispatchRecommendationService;
        private readonly IDriverWorkQueueStore _driverWorkQueueStore;

        public 기사운행Controller(HongdalContext db, I배차추천Service dispatchRecommendationService, IDriverWorkQueueStore driverWorkQueueStore)
        {
            _db = db;
            _dispatchRecommendationService = dispatchRecommendationService;
            _driverWorkQueueStore = driverWorkQueueStore;
        }

        [HttpGet("status")]
        public async Task<IActionResult> 상태조회()
        {
            var driverId = 현재기사Id();
            var driver = await _db.용달기사.FindAsync(driverId);
            if (driver == null)
            {
                return NotFound("용달기사 정보를 찾을 수 없습니다.");
            }

            return Ok(new 기사운행상태응답
            {
                DriverId = driverId,
                Status = driver.운행상태,
                UpdatedAt = driver.UpdatedAt
            });
        }

        [HttpPost("start")]
        public async Task<IActionResult> 시작([FromBody] 기사운행시작요청 request)
        {
            var driverId = 현재기사Id();
            if (request == null)
            {
                return BadRequest("request body is required");
            }

            if (string.IsNullOrWhiteSpace(request.시작모드))
            {
                return BadRequest("시작모드가 필요합니다.");
            }

            if (string.IsNullOrWhiteSpace(request.시작위치))
            {
                return BadRequest("시작위치가 필요합니다.");
            }

            var driver = await _db.용달기사.FirstOrDefaultAsync(x => x.기사Id == driverId);
            if (driver == null)
            {
                return NotFound("용달기사 정보를 찾을 수 없습니다.");
            }

            var shift = new 기사근무
            {
                기사Id = driverId,
                시작모드 = request.시작모드,
                시작시각 = request.시작시각 ?? DateTime.UtcNow,
                시작위치 = request.시작위치,
                복귀지 = request.복귀지,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await using var tx = await _db.Database.BeginTransactionAsync();

            driver.운행상태 = 상태값.기사운행상태.운행중;
            driver.UpdatedAt = DateTime.UtcNow;

            _db.기사근무.Add(shift);
            await _db.SaveChangesAsync();
            await _driverWorkQueueStore.UpsertAsync(new DriverWorkQueueEntry(
                driverId,
                shift.Id,
                shift.CreatedAt,
                shift.시작모드,
                shift.시작위치,
                shift.복귀지));

            await tx.CommitAsync();

            await _dispatchRecommendationService.SendToDriverAsync(driverId);
            return CreatedAtAction(nameof(상태조회), new { }, new 기사운행시작응답
            {
                DriverId = driverId,
                Status = driver.운행상태,
                ShiftId = shift.Id,
                StartedAt = shift.시작시각
            });
        }

        [HttpPost("stop")]
        public async Task<IActionResult> 종료()
        {
            var driverId = 현재기사Id();
            var driver = await _db.용달기사.FirstOrDefaultAsync(x => x.기사Id == driverId);
            if (driver == null)
            {
                return NotFound("용달기사 정보를 찾을 수 없습니다.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            driver.운행상태 = 상태값.기사운행상태.대기;
            driver.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _driverWorkQueueStore.RemoveAsync(driverId);
            await tx.CommitAsync();

            return NoContent();
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
