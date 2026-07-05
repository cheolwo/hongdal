using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Data;
using 홍달.Services;
using 홍달.도메인.공통;

namespace Hongdal.Controllers.Driver.Recommendation02
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/recommendations")]
    public sealed class 기사배차추천Controller : ControllerBase
    {
        private readonly I배차추천Service _dispatchRecommendationService;
        private readonly INationalDispatchRequestService _nationalDispatchRequestService;

        public 기사배차추천Controller(
            I배차추천Service dispatchRecommendationService,
            INationalDispatchRequestService nationalDispatchRequestService)
        {
            _dispatchRecommendationService = dispatchRecommendationService;
            _nationalDispatchRequestService = nationalDispatchRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> 조회()
        {
            var driverId = 현재기사Id();
            var items = await _dispatchRecommendationService.GetRecommendationsAsync(driverId);
            return Ok(items);
        }

        [HttpGet("idle")]
        public async Task<IActionResult> 비운행중조회()
        {
            var driverId = 현재기사Id();
            var items = await _dispatchRecommendationService.GetIdleRecommendationsAsync(driverId);
            return Ok(items);
        }

        [HttpGet("driving")]
        public async Task<IActionResult> 운행중조회()
        {
            var driverId = 현재기사Id();
            var items = await _dispatchRecommendationService.GetDrivingRecommendationsAsync(driverId);
            return Ok(items);
        }

        [HttpGet("search")]
        public async Task<IActionResult> 검색([FromQuery] decimal latitude, [FromQuery] decimal longitude, [FromQuery] decimal radiusKm)
        {
            var driverId = 현재기사Id();
            if (radiusKm <= 0)
            {
                return BadRequest("radiusKm must be greater than 0.");
            }

            var criteria = new 배차추천검색조건(latitude, longitude, radiusKm);
            var items = await _dispatchRecommendationService.GetRecommendationsAsync(driverId, criteria);
            return Ok(items);
        }

        [HttpGet("national")]
        public async Task<IActionResult> 전국콜조회()
        {
            var driverId = 현재기사Id();
            var items = await _nationalDispatchRequestService.GetNationwideRequestsAsync(driverId);
            return Ok(items);
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
