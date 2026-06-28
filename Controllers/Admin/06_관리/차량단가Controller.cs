using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Hongdal.Application.Admin.Management;
using Hongdal.Contracts.Admin.Management;

namespace Hongdal.Controllers.Admin.Master06
{
    [ApiController]
    [Route("api/v1/vehicle-rates")]
    [Authorize]
    public class 차량단가Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 차량단가Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        [Authorize(Roles = 역할명.기사 + "," + 역할명.화주 + "," + 역할명.서버관리자)]
        public async Task<IActionResult> 목록조회()
        {
            return Ok(await _sender.Send(new 차량단가목록조회Query()));
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = 역할명.기사 + "," + 역할명.화주 + "," + 역할명.서버관리자)]
        public async Task<IActionResult> 단건조회(long id)
        {
            var item = await _sender.Send(new 차량단가단건조회Query(id));
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 생성([FromBody] 차량단가요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _sender.Send(new 차량단가생성Command(
                request.차량종류,
                request.기본운임,
                request.Km당단가,
                request.야간할증,
                request.우천할증,
                request.최소운임));
            return CreatedAtAction(nameof(단건조회), new { id = entity.Id }, entity);
        }

        [HttpPut("{id:long}")]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 수정(long id, [FromBody] 차량단가요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _sender.Send(new 차량단가수정Command(
                id,
                request.차량종류,
                request.기본운임,
                request.Km당단가,
                request.야간할증,
                request.우천할증,
                request.최소운임));
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpDelete("{id:long}")]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 삭제(long id)
        {
            await _sender.Send(new 차량단가삭제Command(id));
            return NoContent();
        }
    }

}
