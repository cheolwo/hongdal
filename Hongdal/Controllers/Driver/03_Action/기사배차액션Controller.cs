using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Controllers;
using Hongdal.Application.Driver.DispatchAction;
using Hongdal.Contracts.Driver.Action;
using 홍달.도메인.공통;
using Hongdal.ApiMetadata;

namespace Hongdal.Controllers.Driver.Action03
{
    [HongdalApiVersion(HongdalProductVersion.V1_0)]
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/dispatch-actions")]
    public sealed class 기사배차액션Controller : DriverControllerBase
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
        public async Task<IActionResult> 거절(string requestId, [FromBody] 기사배차거절요청? request = null)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 배차거절Command(driverId, requestId, request?.사유));
            return this.ToNoContentActionResult(result);
        }

        [HttpPost("{requestId}/cancel-acceptance")]
        public async Task<IActionResult> 수락취소(string requestId, [FromBody] 기사배차수락취소요청? request = null)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 배차수락취소Command(driverId, requestId, request?.사유));
            return this.ToNoContentActionResult(result);
        }

    }
}
