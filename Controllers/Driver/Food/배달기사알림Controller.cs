using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Data;
using 홍달.Services;

namespace Hongdal.Controllers.Driver.Food
{
    [ApiController]
    [Route("api/v1/drivers/{driverId}/push-tokens")]
    [Authorize(Roles = 역할명.기사)]
    public sealed class 배달기사알림Controller : ControllerBase
    {
        private readonly IDriverPushTokenStore _pushTokenStore;

        public 배달기사알림Controller(IDriverPushTokenStore pushTokenStore)
        {
            _pushTokenStore = pushTokenStore;
        }

        [HttpGet]
        public async Task<IActionResult> 조회(string driverId)
        {
            if (!현재기사확인(driverId)) return Forbid();

            var token = await _pushTokenStore.GetAsync(driverId);
            return Ok(new 배달기사푸시토큰응답
            {
                DriverId = driverId,
                HasToken = !string.IsNullOrWhiteSpace(token),
                PushToken = token ?? string.Empty
            });
        }

        [HttpPut]
        public async Task<IActionResult> 등록(string driverId, [FromBody] 배달기사푸시토큰등록요청 request)
        {
            if (!현재기사확인(driverId)) return Forbid();
            if (request == null) return BadRequest("request body is required");
            if (string.IsNullOrWhiteSpace(request.PushToken)) return BadRequest("pushToken is required");

            await _pushTokenStore.SetAsync(driverId, request.PushToken);
            return Ok(new 배달기사푸시토큰응답
            {
                DriverId = driverId,
                HasToken = true,
                PushToken = request.PushToken.Trim()
            });
        }

        [HttpDelete]
        public async Task<IActionResult> 삭제(string driverId)
        {
            if (!현재기사확인(driverId)) return Forbid();

            await _pushTokenStore.ClearAsync(driverId);
            return NoContent();
        }

        private bool 현재기사확인(string driverId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(currentUserId)
                   && string.Equals(currentUserId, driverId, StringComparison.Ordinal);
        }
    }

    public sealed class 배달기사푸시토큰등록요청
    {
        public string PushToken { get; set; } = string.Empty;
    }

    public sealed class 배달기사푸시토큰응답
    {
        public string DriverId { get; set; } = string.Empty;
        public bool HasToken { get; set; }
        public string PushToken { get; set; } = string.Empty;
    }
}
