using System.Linq;
using System.Threading.Tasks;
using Ssalddel.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ssalddel.Application.Admin.Operating;
using Ssalddel.Contracts.Admin.Progress;
using 살뜰.도메인.운송;
using Ssalddel.ApiMetadata;
using Microsoft.AspNetCore.Authorization;

namespace Ssalddel.Controllers.Admin.Progress03
{
    [SsalddelApiVersion(SsalddelProductVersion.V2_0)]
    [ApiController]
    [Route("api/v1/transport-events")]
    [Authorize(Policy = "서버관리자전용")]
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
            if (item == null) return this.ToNotFoundProblem("운송이벤트 정보를 찾을 수 없습니다.");
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> 생성([FromBody] 운송이벤트요청 request)
        {
            if (request == null) return this.ToProblemActionResult("request body is required");

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
            if (request == null) return this.ToProblemActionResult("request body is required");

            var entity = await _sender.Send(new 운송이벤트수정Command(
                id,
                request.의뢰Id,
                request.이벤트타입,
                request.이벤트시각 == default ? DateTime.UtcNow : request.이벤트시각,
                request.메타데이터));
            if (entity == null) return this.ToNotFoundProblem("운송이벤트 정보를 찾을 수 없습니다.");

            return Ok(entity);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> 삭제(long id)
        {
            var result = await _sender.Send(new 운송이벤트삭제Command(id));
            return this.ToNoContentActionResult(result);
        }
    }

}
