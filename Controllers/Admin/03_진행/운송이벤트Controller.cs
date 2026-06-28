using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Hongdal.Application.Admin.Operating;
using Hongdal.Contracts.Admin.Progress;
using 홍달.도메인.운송;

namespace Hongdal.Controllers.Admin.Progress03
{
    [ApiController]
    [Route("api/v1/transport-events")]
    public class 운송이벤트Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 운송이벤트Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> 목록조회()
        {
            return Ok(await _sender.Send(new 운송이벤트목록조회Query()));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 단건조회(long id)
        {
            var item = await _sender.Send(new 운송이벤트단건조회Query(id));
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> 생성([FromBody] 운송이벤트요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _sender.Send(new 운송이벤트생성Command(
                request.의뢰Id,
                request.이벤트타입,
                request.이벤트시각 == default ? DateTime.UtcNow : request.이벤트시각,
                request.메타데이터));
            return CreatedAtAction(nameof(단건조회), new { id = entity.Id }, entity);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> 수정(long id, [FromBody] 운송이벤트요청 request)
        {
            if (request == null) return BadRequest();

            var entity = await _sender.Send(new 운송이벤트수정Command(
                id,
                request.의뢰Id,
                request.이벤트타입,
                request.이벤트시각 == default ? DateTime.UtcNow : request.이벤트시각,
                request.메타데이터));
            if (entity == null) return NotFound();

            return Ok(entity);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> 삭제(long id)
        {
            await _sender.Send(new 운송이벤트삭제Command(id));
            return NoContent();
        }
    }

}
