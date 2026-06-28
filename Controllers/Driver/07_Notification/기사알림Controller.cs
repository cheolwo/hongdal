using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.Services;
using Hongdal.Contracts.Driver.Notification;

namespace Hongdal.Controllers.Driver.Notification07
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/notifications")]
    public sealed class 기사알림Controller : ControllerBase
    {
        private readonly IDriverPushTokenStore _pushTokenStore;

        public 기사알림Controller(IDriverPushTokenStore pushTokenStore)
        {
            _pushTokenStore = pushTokenStore;
        }

        [HttpGet("push-token")]
        public async Task<IActionResult> 조회()
        {
            var driverId = 현재기사Id();
            var token = await _pushTokenStore.GetAsync(driverId);
            return Ok(new 기사푸시토큰응답
            {
                DriverId = driverId,
                HasToken = !string.IsNullOrWhiteSpace(token),
                PushToken = token ?? string.Empty
            });
        }

        [HttpPut("push-token")]
        public async Task<IActionResult> 등록([FromBody] 기사푸시토큰등록요청 request)
        {
            var driverId = 현재기사Id();
            if (request == null)
            {
                return BadRequest("request body is required");
            }

            if (string.IsNullOrWhiteSpace(request.PushToken))
            {
                return BadRequest("pushToken is required");
            }

            await _pushTokenStore.SetAsync(driverId, request.PushToken.Trim());
            return Ok(new 기사푸시토큰응답
            {
                DriverId = driverId,
                HasToken = true,
                PushToken = request.PushToken.Trim()
            });
        }

        [HttpDelete("push-token")]
        public async Task<IActionResult> 삭제()
        {
            var driverId = 현재기사Id();
            await _pushTokenStore.ClearAsync(driverId);
            return NoContent();
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }

}
