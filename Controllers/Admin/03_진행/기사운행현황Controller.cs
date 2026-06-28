using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Application.Admin.Operating;
using Hongdal.Contracts.Admin.Progress;

namespace Hongdal.Controllers.Admin.Progress03
{
    [ApiController]
    [Route("api/v1/admin/drivers/operating")]
    [Authorize(Policy = "서버관리자전용")]
    public class 기사운행현황Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 기사운행현황Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> 현재운행기사조회([FromQuery] 현재운행기사조회요청 요청)
        {
            var 현재운행기사목록 = await _sender.Send(new 현재운행기사조회Query(요청 ?? new 현재운행기사조회요청()));

            return Ok(현재운행기사목록);
        }
    }

}
