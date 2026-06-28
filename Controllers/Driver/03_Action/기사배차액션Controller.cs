using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HongdalContext = 홍달.Data.HongdalContext;
using IDispatchAcceptanceLogStore = 홍달.Services.Dispatch.Recommendation.IDispatchAcceptanceLogStore;
using 홍달.Services.Storage.Local;
using 홍달.도메인.공통;
using 홍달.도메인.운송;

namespace Hongdal.Controllers.Driver.Action03
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/dispatch-actions")]
    public sealed class 기사배차액션Controller : ControllerBase
    {
        private readonly HongdalContext _db;
        private readonly IDispatchAcceptanceLogStore _acceptanceLogStore;
        private readonly ILogger<기사배차액션Controller> _logger;

        public 기사배차액션Controller(
            HongdalContext db,
            IDispatchAcceptanceLogStore acceptanceLogStore,
            ILogger<기사배차액션Controller> logger)
        {
            _db = db;
            _acceptanceLogStore = acceptanceLogStore;
            _logger = logger;
        }

        [HttpPost("{requestId}/accept")]
        public async Task<IActionResult> 수락(string requestId)
        {
            var driverId = 현재기사Id();
            var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == requestId);
            if (queue == null)
            {
                return NotFound(new { message = "배차대기 데이터를 찾을 수 없습니다." });
            }

            var request = await _db.화주운송의뢰.FirstOrDefaultAsync(x => x.의뢰Id == requestId);
            if (request == null)
            {
                return NotFound(new { message = "운송의뢰 데이터를 찾을 수 없습니다." });
            }

            queue.상태 = 상태값.배차대기상태.확정;
            request.배차상태 = 상태값.배차상태.매칭중;
            request.UpdatedAt = DateTime.UtcNow;
            queue.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _acceptanceLogStore.AppendAsync(new DispatchAcceptanceLogEntry(
                driverId,
                request.화주Id,
                requestId,
                DateTime.UtcNow,
                queue.상태,
                request.배차상태,
                request.결제상태));
            return Ok(new { message = "수락되었습니다.", requestId });
        }

        [HttpPost("{requestId}/reject")]
        public async Task<IActionResult> 거절(string requestId, [FromServices] IDriverRejectedRequestStore rejectedRequestStore)
        {
            var driverId = 현재기사Id();
            await rejectedRequestStore.RejectAsync(driverId, requestId);
            _logger.LogInformation("Driver {DriverId} rejected request {RequestId}", driverId, requestId);
            return NoContent();
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
