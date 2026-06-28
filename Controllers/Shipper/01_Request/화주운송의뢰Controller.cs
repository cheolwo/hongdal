using System.Linq;
using System.Threading.Tasks;
using Hongdal.Application.Shipper.Request;
using Hongdal.Contracts.Shipper.Request;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Shipper.Request01
{
    [ApiController]
    [Route("api/v1/shipper/requests")]
    [Authorize]
    public class 화주운송의뢰Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 화주운송의뢰Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> 의뢰목록조회(
            [FromQuery] string? shipperId,
            [FromQuery] string? status,
            [FromQuery] string? paymentStatus,
            [FromQuery] string? dispatchStatus,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var items = await _sender.Send(new 의뢰목록조회Query(shipperId, status, paymentStatus, dispatchStatus, page, pageSize));
            return Ok(items);
        }

        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<IActionResult> 공개화물요약조회(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var items = await _sender.Send(new 공개화물요약조회Query(page, pageSize));
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> 의뢰생성([FromBody] 화주운송의뢰생성요청 req)
        {
            var cargo = req.화물;
            var pickup = req.픽업;
            var dropoff = req.하차;
            var pricing = req.요금옵션;

            var result = await _sender.Send(new 의뢰생성Command(
                req.화주Id,
                req.운송방식,
                req.차량종류,
                req.결제수단,
                req.결제예정금액,
                cargo.화물종류,
                cargo.설명,
                cargo.수량,
                cargo.길이Mm,
                cargo.폭Mm,
                cargo.높이Mm,
                cargo.중량Kg,
                cargo.부피Cbm,
                cargo.팔레트개수,
                cargo.화물파손주의여부,
                cargo.온도조건,
                pickup?.주소.도로명주소 ?? string.Empty,
                pickup?.주소.상세주소,
                pickup?.주소.위도,
                pickup?.주소.경도,
                pickup?.연락처.이름 ?? string.Empty,
                pickup?.연락처.전화번호 ?? string.Empty,
                pickup?.시간창?.시작일시 ?? default,
                pickup?.시간창?.종료일시 ?? default,
                dropoff?.주소.도로명주소 ?? string.Empty,
                dropoff?.주소.상세주소,
                dropoff?.주소.위도,
                dropoff?.주소.경도,
                dropoff?.연락처.이름 ?? string.Empty,
                dropoff?.연락처.전화번호 ?? string.Empty,
                dropoff?.시간창?.시작일시,
                dropoff?.시간창?.종료일시,
                pricing?.서비스레벨,
                pricing?.요청사항,
                pricing?.대기료,
                pricing?.수작업비,
                pricing?.할증,
                req.클라이언트요청Id,
                req.결제상태));
            return result.IsSuccess
                ? CreatedAtAction(nameof(의뢰단건조회), new { requestId = result.Value.의뢰Id }, result.Value)
                : ToActionResult(result.Errors.Select(x => x.Message));
        }

        [HttpGet("{requestId}")]
        public async Task<IActionResult> 의뢰단건조회(string requestId)
        {
            var item = await _sender.Send(new 의뢰단건조회Query(requestId));
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPut("{requestId}")]
        public async Task<IActionResult> 의뢰수정(string requestId, [FromBody] 화주운송의뢰수정요청 req)
        {
            var cargo = req.화물;
            var pickup = req.픽업;
            var dropoff = req.하차;
            var pricing = req.요금옵션;

            var result = await _sender.Send(new 의뢰수정Command(
                requestId,
                req.운송방식,
                req.차량종류,
                req.결제수단,
                req.결제예정금액,
                cargo?.화물종류,
                cargo?.설명,
                cargo?.수량,
                cargo?.중량Kg,
                cargo?.부피Cbm,
                cargo?.화물파손주의여부,
                cargo?.온도조건,
                pickup?.주소?.도로명주소,
                pickup?.주소?.상세주소,
                pickup?.주소?.위도,
                pickup?.주소?.경도,
                pickup?.연락처?.이름,
                pickup?.연락처?.전화번호,
                pickup?.시간창?.시작일시,
                pickup?.시간창?.종료일시,
                dropoff?.주소?.도로명주소,
                dropoff?.주소?.상세주소,
                dropoff?.주소?.위도,
                dropoff?.주소?.경도,
                dropoff?.연락처?.이름,
                dropoff?.연락처?.전화번호,
                dropoff?.시간창?.시작일시,
                dropoff?.시간창?.종료일시,
                pricing?.서비스레벨,
                pricing?.요청사항,
                pricing?.대기료,
                pricing?.수작업비,
                pricing?.할증,
                req.결제상태,
                req.상태,
                req.배차상태));
            return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Errors.Select(x => x.Message));
        }

        [HttpDelete("{requestId}")]
        public async Task<IActionResult> 의뢰삭제(string requestId)
        {
            var result = await _sender.Send(new 의뢰삭제Command(requestId));
            return result.IsSuccess ? NoContent() : ToActionResult(result.Errors.Select(x => x.Message));
        }

        private IActionResult ToActionResult(IEnumerable<string> errors)
        {
            var messages = errors.ToArray();
            if (messages.Any(x => x.Contains("찾을 수 없습니다.", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = messages });
            }

            if (messages.Any(x => x.Contains("이미", StringComparison.OrdinalIgnoreCase)) ||
                messages.Any(x => x.Contains("동일한", StringComparison.OrdinalIgnoreCase)))
            {
                return Conflict(new { errors = messages });
            }

            return BadRequest(new { errors = messages });
        }
    }
}
