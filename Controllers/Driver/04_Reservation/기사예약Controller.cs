using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using FluentResults;
using Hongdal.Application.Driver.Reservation;
using Hongdal.Contracts.Driver.Reservation;
using 홍달.도메인.공통;

namespace Hongdal.Controllers.Driver.Reservation04
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/reservations")]
    public sealed class 기사예약Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 기사예약Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> 예약목록()
        {
            var driverId = 현재기사Id();
            var items = await _sender.Send(new 예약목록조회Query(driverId));

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> 예약([FromBody] 기사예약요청 request)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 예약생성Command(driverId, request.시작모드, request.시작시각, request.시작위치, request.복귀지));

            if (result.IsFailed)
            {
                return BadRequest(new { errors = result.Errors.Select(x => x.Message).ToArray() });
            }

            return CreatedAtAction(nameof(상세조회), new { id = result.Value.Id }, result.Value);
        }

        [HttpPost("{id:long}/cancel")]
        public async Task<IActionResult> 취소(long id)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 예약취소Command(driverId, id));

            return Ok(result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 상세조회(long id)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 예약상세조회Query(driverId, id));

            return Ok(result);
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
