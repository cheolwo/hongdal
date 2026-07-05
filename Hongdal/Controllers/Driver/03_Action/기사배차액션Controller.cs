using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Controllers;
using Hongdal.Application.Driver.DispatchAction;
using 홍달.도메인.공통;

namespace Hongdal.Controllers.Driver.Action03
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/dispatch-actions")]
    public sealed class 기사배차액션Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 기사배차액션Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost("{requestId}/accept")]
        public async Task<IActionResult> 수락(string requestId)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 배차수락Command(driverId, requestId));

            return this.ToActionResult(result);
        }

        [HttpPost("{requestId}/reject")]
        public async Task<IActionResult> 거절(string requestId)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 배차거절Command(driverId, requestId));
            return this.ToNoContentActionResult(result);
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
