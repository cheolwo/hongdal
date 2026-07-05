using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hongdal.Application.Admin.Management;
using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Controllers.Admin.Master06
{
    [ApiController]
    [Route("api/v1/admin/vehicle-recommendations")]
    [Authorize(Policy = "서버관리자전용")]
    public class 차량추천관리Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 차량추천관리Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("criteria")]
        public async Task<IActionResult> 추천기준목록조회()
        {
            return Ok(await _sender.Send(new 차량추천기준목록조회Query()));
        }

        [HttpPut("criteria/{vehicleCode}")]
        public async Task<IActionResult> 추천기준수정(string vehicleCode, [FromBody] 차량추천기준수정요청 request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            var item = await _sender.Send(new 차량추천기준수정Command(
                vehicleCode,
                request.권장최대CBM,
                request.추천우선순위,
                request.추천사용여부,
                request.운영권장중량Kg,
                request.팔레트적재개수));

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPost("simulate")]
        public async Task<IActionResult> 추천시뮬레이션([FromBody] 차량추천시뮬레이션요청 request)
        {
            if (request == null)
            {
                return BadRequest();
            }

            return Ok(await _sender.Send(new 차량추천시뮬레이션Query(request)));
        }
    }
}
