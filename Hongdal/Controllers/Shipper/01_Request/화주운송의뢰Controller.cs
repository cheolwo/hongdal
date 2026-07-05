using System.Linq;
using System.Threading.Tasks;
using Hongdal.Application.Shipper.Request;
using Hongdal.Contracts.Shipper.Request;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Shipper.Request01
{
    [ApiController]
    [Route("api/v1/shipper/requests")]
    [Authorize]
    public class 화주운송의뢰Controller : ControllerBase
    {
        private readonly ISender _sender;
        private readonly I화주운송의뢰일괄등록파서Service _bulkParser;
        private readonly I차량추천Service _vehicleRecommendationService;

        public 화주운송의뢰Controller(ISender sender, I화주운송의뢰일괄등록파서Service bulkParser, I차량추천Service vehicleRecommendationService)
        {
            _sender = sender;
            _bulkParser = bulkParser;
            _vehicleRecommendationService = vehicleRecommendationService;
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

        [HttpPost("recommend-vehicle")]
        public async Task<ActionResult<차량추천응답>> 차량추천([FromBody] 차량추천요청 request, CancellationToken cancellationToken)
        {
            var result = await _vehicleRecommendationService.추천Async(request, cancellationToken);
            return Ok(result);
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
                req.정산조건,
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
            var result = await _sender.Send(new 의뢰수정Command(
                requestId,
                new 운송조건입력값(req.운송방식, req.차량종류, req.요금옵션?.서비스레벨),
                new 화물정보입력값(req.화물?.화물종류, req.화물?.설명, req.화물?.수량, req.화물?.중량Kg, req.화물?.부피Cbm, req.화물?.화물파손주의여부, req.화물?.온도조건),
                new 위치정보입력값(req.픽업?.주소?.도로명주소, req.픽업?.주소?.상세주소, req.픽업?.주소?.위도, req.픽업?.주소?.경도, req.픽업?.연락처?.이름, req.픽업?.연락처?.전화번호, req.픽업?.시간창?.시작일시, req.픽업?.시간창?.종료일시),
                new 위치정보입력값(req.하차?.주소?.도로명주소, req.하차?.주소?.상세주소, req.하차?.주소?.위도, req.하차?.주소?.경도, req.하차?.연락처?.이름, req.하차?.연락처?.전화번호, req.하차?.시간창?.시작일시, req.하차?.시간창?.종료일시),
                new 요청조건입력값(req.요금옵션?.요청사항),
                req.정산조건 is null ? null : new 정산조건입력값(req.결제수단, req.정산조건)));
            return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Errors.Select(x => x.Message));
        }

        [HttpDelete("{requestId}")]
        public async Task<IActionResult> 의뢰삭제(string requestId)
        {
            var result = await _sender.Send(new 의뢰삭제Command(requestId));
            return result.IsSuccess ? NoContent() : ToActionResult(result.Errors.Select(x => x.Message));
        }

        [HttpPost("bulk/preview")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> 일괄미리보기([FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            return await ProcessBulkPreviewAsync(file, cancellationToken);
        }

        [HttpPost("bulk/confirm")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> 일괄등록([FromForm] IFormFile file, CancellationToken cancellationToken)
        {
            var previewResult = await BuildBulkPreviewAsync(file, cancellationToken);
            if (previewResult.Result is not null)
            {
                return previewResult.Result;
            }

            var confirmRequest = new 화주운송의뢰일괄확정등록요청
            {
                행목록 = previewResult.Value!.행목록
                    .Where(x => x.유효함)
                    .Select(x => new 화주운송의뢰일괄확정등록행
                    {
                        행번호 = x.행번호,
                        등록여부 = x.등록대상여부,
                        최종선택차량종류 = x.최종선택차량종류,
                        원본행 = x.원본행
                    })
                    .ToArray()
            };

            var result = await _sender.Send(new 화주운송의뢰일괄확정등록Command(confirmRequest.행목록), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Errors.Select(x => x.Message));
        }

        [HttpPost("bulk/confirm-preview")]
        public async Task<IActionResult> 일괄미리보기확정등록([FromBody] 화주운송의뢰일괄확정등록요청 request, CancellationToken cancellationToken)
        {
            var result = await _sender.Send(new 화주운송의뢰일괄확정등록Command(request.행목록), cancellationToken);
            return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Errors.Select(x => x.Message));
        }

        [HttpPost("{requestId}/settlement/offline")]
        public async Task<IActionResult> 현장지급처리(string requestId, [FromBody] 화주운송의뢰현장지급처리요청 req)
        {
            var result = await _sender.Send(new 화주운송의뢰현장지급처리Command(requestId, req.현장지급메모));
            return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Errors.Select(x => x.Message));
        }

        [HttpPost("{requestId}/settlement/postpay/approve")]
        public async Task<IActionResult> 후불승인(string requestId, [FromBody] 화주운송의뢰후불승인요청 req)
        {
            var result = await _sender.Send(new 화주운송의뢰후불승인Command(requestId, req.승인메모));
            return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Errors.Select(x => x.Message));
        }

        [HttpPost("{requestId}/settlement/receipt")]
        public async Task<IActionResult> 인수증등록(string requestId, [FromBody] 화주운송의뢰인수증등록요청 req)
        {
            var result = await _sender.Send(new 화주운송의뢰인수증등록Command(requestId, req.인수증번호, req.등록메모));
            return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Errors.Select(x => x.Message));
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

        private async Task<IActionResult> ProcessBulkPreviewAsync(IFormFile? file, CancellationToken cancellationToken)
        {
            var previewResult = await BuildBulkPreviewAsync(file, cancellationToken);
            if (previewResult.Result is not null)
            {
                return previewResult.Result;
            }

            return Ok(previewResult.Value);
        }

        private async Task<(IActionResult? Result, 화주운송의뢰일괄미리보기응답? Value)> BuildBulkPreviewAsync(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null)
            {
                return (BadRequest(new { errors = new[] { "file is required" } }), null);
            }

            if (file.Length <= 0)
            {
                return (BadRequest(new { errors = new[] { "empty file is not allowed" } }), null);
            }

            await using var stream = file.OpenReadStream();
            var parsed = await _bulkParser.ParseAsync(stream, file.FileName, cancellationToken);
            if (parsed.행목록.Count == 0 && parsed.오류목록.Count > 0)
            {
                return (BadRequest(new { errors = parsed.오류목록 }), null);
            }

            var result = await _sender.Send(new 화주운송의뢰일괄미리보기Command(parsed.행목록, parsed.오류목록), cancellationToken);
            return result.IsSuccess
                ? (null, result.Value)
                : (ToActionResult(result.Errors.Select(x => x.Message)), null);
        }
    }
}
