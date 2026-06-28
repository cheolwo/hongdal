using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services.Dispatch.Request;
using 홍달.도메인.공통;
using Hongdal.Contracts.Driver.Recommendation;

namespace Hongdal.Controllers.Driver.Recommendation02
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/recommendations")]
    public sealed class 기사배차추천요약Controller : ControllerBase
    {
        private readonly I배차추천Service _dispatchRecommendationService;
        private readonly INationalDispatchRequestService _nationalDispatchRequestService;

        public 기사배차추천요약Controller(
            I배차추천Service dispatchRecommendationService,
            INationalDispatchRequestService nationalDispatchRequestService)
        {
            _dispatchRecommendationService = dispatchRecommendationService;
            _nationalDispatchRequestService = nationalDispatchRequestService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> 요약()
        {
            var driverId = 현재기사Id();
            var all = await _dispatchRecommendationService.GetRecommendationsAsync(driverId);
            var idle = await _dispatchRecommendationService.GetIdleRecommendationsAsync(driverId);
            var driving = await _dispatchRecommendationService.GetDrivingRecommendationsAsync(driverId);
            var national = await _nationalDispatchRequestService.GetNationwideRequestsAsync(driverId);

            return Ok(new 기사배차추천요약응답
            {
                전체추천수 = all.Count,
                적합추천수 = all.Count(x => x.차량적합여부),
                운행중추천수 = driving.Count,
                비운행중추천수 = idle.Count,
                전국콜수 = national.Count
            });
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
