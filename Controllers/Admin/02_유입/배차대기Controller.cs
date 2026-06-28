using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Application.Admin.Inbound;
using Hongdal.Contracts.Admin.Inbound;

namespace Hongdal.Controllers.Admin.Inflow02
{
    [ApiController]
    [Route("api/v1/dispatch/wait")]
    public class 배차대기Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 배차대기Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> 목록조회()
        {
            return Ok(await _sender.Send(new 배차대기목록조회Query()));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 단건조회(long id)
        {
            var item = await _sender.Send(new 배차대기단건조회Query(id));
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> 생성([FromBody] 배차대기요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _sender.Send(new 배차대기생성Command(
                request.의뢰Id,
                request.화주Id,
                request.픽업_도로명주소,
                request.픽업_상세주소,
                request.픽업_위도,
                request.픽업_경도,
                request.하차_도로명주소,
                request.하차_상세주소,
                request.하차_위도,
                request.하차_경도,
                request.상태));
            return CreatedAtAction(nameof(단건조회), new { id = entity.Id }, entity);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> 수정(long id, [FromBody] 배차대기요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _sender.Send(new 배차대기수정Command(
                id,
                request.의뢰Id,
                request.화주Id,
                request.픽업_도로명주소,
                request.픽업_상세주소,
                request.픽업_위도,
                request.픽업_경도,
                request.하차_도로명주소,
                request.하차_상세주소,
                request.하차_위도,
                request.하차_경도,
                request.상태));
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> 삭제(long id)
        {
            await _sender.Send(new 배차대기삭제Command(id));
            return NoContent();
        }
    }

}
