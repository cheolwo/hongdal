using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.도메인.공통;

namespace Hongdal.Controllers.Driver.Recommendation02
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/public-dispatches")]
    public sealed class 기사공개배차Controller : ControllerBase
    {
        private readonly 홍달.Services.Dispatch.Queue.I공개배차Service _publicDispatchService;

        public 기사공개배차Controller(홍달.Services.Dispatch.Queue.I공개배차Service publicDispatchService)
        {
            _publicDispatchService = publicDispatchService;
        }

        [HttpGet]
        public async Task<IActionResult> 조회()
        {
            var driverId = 현재기사Id();
            var items = await _publicDispatchService.GetPublicDispatchesAsync(driverId);
            return Ok(items);
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
