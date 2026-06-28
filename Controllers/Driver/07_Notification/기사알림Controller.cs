using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly IDriverNotificationSettingsStore _notificationSettingsStore;
        private readonly ILogger<기사알림Controller> _logger;

        public 기사알림Controller(IDriverPushTokenStore pushTokenStore, IDriverNotificationSettingsStore notificationSettingsStore, ILogger<기사알림Controller> logger)
        {
            _pushTokenStore = pushTokenStore;
            _notificationSettingsStore = notificationSettingsStore;
            _logger = logger;
        }

        [HttpGet("push-token")]
        public async Task<IActionResult> 조회()
        {
            var driverId = 현재기사Id();
            var token = await _pushTokenStore.GetAsync(driverId);
            _logger.LogInformation("Action={Action} DriverId={DriverId} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}", "PushTokenViewed", driverId, "Success", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty, DateTime.UtcNow);
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
            _logger.LogInformation("Action={Action} DriverId={DriverId} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}", "PushTokenRegistered", driverId, "Success", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty, DateTime.UtcNow);
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
            _logger.LogInformation("Action={Action} DriverId={DriverId} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}", "PushTokenDeleted", driverId, "Success", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty, DateTime.UtcNow);
            return NoContent();
        }

        [HttpGet("settings")]
        public async Task<IActionResult> 설정조회()
        {
            var driverId = 현재기사Id();
            var settings = await _notificationSettingsStore.GetAsync(driverId);
            _logger.LogInformation("Action={Action} DriverId={DriverId} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}", "NotificationSettingsViewed", driverId, "Success", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty, DateTime.UtcNow);

            return Ok(new 기사알림설정응답
            {
                DriverId = driverId,
                배차추천알림사용 = settings.배차추천알림사용,
                운전중푸시만사용 = settings.운전중푸시만사용,
                소리사용 = settings.소리사용,
                진동사용 = settings.진동사용,
                야간알림제한 = settings.야간알림제한,
                정차후모아보기 = settings.정차후모아보기
            });
        }

        [HttpPut("settings")]
        public async Task<IActionResult> 설정수정([FromBody] 기사알림설정수정요청 request)
        {
            var driverId = 현재기사Id();
            if (request == null)
            {
                return BadRequest("request body is required");
            }

            var settings = new DriverNotificationSettings(
                request.배차추천알림사용,
                request.운전중푸시만사용,
                request.소리사용,
                request.진동사용,
                request.야간알림제한,
                request.정차후모아보기);

            await _notificationSettingsStore.SetAsync(driverId, settings);
            _logger.LogInformation("Action={Action} DriverId={DriverId} Result={Result} TraceId={TraceId} OccurredAt={OccurredAt}", "NotificationSettingsUpdated", driverId, "Success", System.Diagnostics.Activity.Current?.TraceId.ToString() ?? string.Empty, DateTime.UtcNow);

            return Ok(new 기사알림설정응답
            {
                DriverId = driverId,
                배차추천알림사용 = settings.배차추천알림사용,
                운전중푸시만사용 = settings.운전중푸시만사용,
                소리사용 = settings.소리사용,
                진동사용 = settings.진동사용,
                야간알림제한 = settings.야간알림제한,
                정차후모아보기 = settings.정차후모아보기
            });
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }

}
