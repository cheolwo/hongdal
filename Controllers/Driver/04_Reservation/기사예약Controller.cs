using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hongdal.Contracts.Driver.Reservation;
using 홍달.Data;
using 홍달.Services;
using 홍달.도메인.공통;
using 홍달.도메인.기사;

namespace Hongdal.Controllers.Driver.Reservation04
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/reservations")]
    public sealed class 기사예약Controller : ControllerBase
    {
        private readonly HongdalContext _db;
        private readonly I배차추천Service _dispatchRecommendationService;

        public 기사예약Controller(HongdalContext db, I배차추천Service dispatchRecommendationService)
        {
            _db = db;
            _dispatchRecommendationService = dispatchRecommendationService;
        }

        [HttpPost]
        public async Task<IActionResult> 예약([FromBody] 기사예약요청 request)
        {
            var driverId = 현재기사Id();
            if (request == null)
            {
                return BadRequest("request body is required");
            }

            if (string.IsNullOrWhiteSpace(request.시작모드))
            {
                return BadRequest("시작모드가 필요합니다. (startMode required)");
            }

            if (!request.시작시각.HasValue)
            {
                return BadRequest("예약 배차에는 시작시각이 필요합니다. (startedAt is required for reserve)");
            }

            if (request.시작시각.Value <= DateTime.UtcNow)
            {
                return BadRequest("예약 배차의 시작시각은 현재보다 미래여야 합니다. (startedAt must be in the future for reserve)");
            }

            var shift = new 기사근무
            {
                기사Id = driverId,
                시작모드 = request.시작모드,
                시작시각 = request.시작시각,
                시작위치 = request.시작위치 ?? string.Empty,
                복귀지 = request.복귀지,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.기사근무.Add(shift);
            await _db.SaveChangesAsync();

            await _dispatchRecommendationService.SendToDriverAsync(driverId);

            return CreatedAtAction(nameof(상세조회), new { id = shift.Id }, new 기사예약응답
            {
                Id = shift.Id,
                DriverId = shift.기사Id,
                StartMode = shift.시작모드,
                StartTime = shift.시작시각,
                StartLocation = shift.시작위치,
                ReturnDestination = shift.복귀지
            });
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 상세조회(long id)
        {
            var driverId = 현재기사Id();
            var shift = await _db.기사근무.FindAsync(id);
            if (shift == null || shift.기사Id != driverId)
            {
                return NotFound();
            }

            return Ok(new 기사예약응답
            {
                Id = shift.Id,
                DriverId = shift.기사Id,
                StartMode = shift.시작모드,
                StartTime = shift.시작시각,
                StartLocation = shift.시작위치,
                ReturnDestination = shift.복귀지
            });
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
