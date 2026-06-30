using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Controllers;
using Hongdal.Application.Admin.Management;
using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Controllers.Admin.Master06
{
    [ApiController]
    [Route("api/v1/fare-configurations")]
    [Authorize]
    public class 운임구성Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 운임구성Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        [Authorize(Roles = 역할명.기사 + "," + 역할명.화주 + "," + 역할명.서버관리자)]
        public async Task<IActionResult> 목록조회()
        {
            return Ok(await _sender.Send(new 운임구성목록조회Query()));
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = 역할명.기사 + "," + 역할명.화주 + "," + 역할명.서버관리자)]
        public async Task<IActionResult> 단건조회(long id)
        {
            var item = await _sender.Send(new 운임구성단건조회Query(id));
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 생성([FromBody] 운임구성요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _sender.Send(new 운임구성생성Command(
                request.의뢰Id,
                request.기본운임,
                request.거리운임,
                request.할증,
                request.대기료,
                request.수작업비,
                request.최종운임));
            return CreatedAtAction(nameof(단건조회), new { id = entity.Id }, entity);
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 수정(long id, [FromBody] 운임구성요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _sender.Send(new 운임구성수정Command(
                id,
                request.의뢰Id,
                request.기본운임,
                request.거리운임,
                request.할증,
                request.대기료,
                request.수작업비,
                request.최종운임));
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 삭제(long id)
        {
            var result = await _sender.Send(new 운임구성삭제Command(id));
            return this.ToNoContentActionResult(result);
        }
    }

}
