using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hongdal.Application.Shipper.Payment;
using Hongdal.Contracts.Shipper.Payment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hongdal.Controllers.Shipper.Payment02
{
    [ApiController]
    [Route("api/v1/payments")]
    public class 화주결제Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 화주결제Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 결제목록조회(
            [FromQuery] string? 결제상태,
            [FromQuery] string? 의뢰Id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var items = await _sender.Send(new 결제목록조회Query(결제상태, 의뢰Id, page, pageSize));
            return Ok(items);
        }

        [HttpGet("toss/config")]
        [Authorize(Policy = "서버관리자전용")]
        public async Task<IActionResult> 토스결제환경조회()
        {
            var result = await _sender.Send(new 토스결제환경조회Query());
            return Ok(result);
        }

        [HttpPost("toss/prepare")]
        public async Task<IActionResult> 토스결제준비([FromBody] 토스결제준비요청 request)
        {
            var result = await _sender.Send(new 토스결제준비Command(request.의뢰Id, request.Amount));
            return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Errors.Select(x => x.Message));
        }

        [HttpPost("toss/confirm")]
        public async Task<IActionResult> 토스결제승인([FromBody] 토스결제승인요청 request)
        {
            var result = await _sender.Send(new 토스결제승인Command(request.PaymentKey, request.OrderId, request.Amount));
            return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Errors.Select(x => x.Message));
        }

        private IActionResult ToActionResult(IEnumerable<string> errors)
        {
            var messages = errors.ToArray();
            if (messages.Any(x => x.Contains("찾을 수 없습니다.", StringComparison.OrdinalIgnoreCase)))
            {
                return NotFound(new { errors = messages });
            }

            return BadRequest(new { errors = messages });
        }
    }
}
