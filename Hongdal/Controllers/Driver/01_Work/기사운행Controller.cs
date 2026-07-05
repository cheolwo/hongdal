using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using 홍달.도메인.공통;
using MediatR;
using FluentResults;
using Hongdal.Contracts.Driver.Work;
using Hongdal.Application.Driver.Work;

namespace Hongdal.Controllers.Driver.Work01
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/work")]
    public sealed class 기사운행Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 기사운행Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("status")]
        public async Task<IActionResult> 상태조회()
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 운행상태조회Query(driverId));
            return Ok(result);
        }

        [HttpGet("current")]
        public async Task<IActionResult> 현재근무조회()
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 현재근무조회Query(driverId));
            return Ok(result);
        }

        [HttpPost("start")]
        public async Task<IActionResult> 시작([FromBody] 기사운행시작요청 request)
        {
            var driverId = 현재기사Id();
            var result = await _sender.Send(new 운행시작Command(
                driverId,
                request.시작모드,
                request.시작시각,
                request.시작위치,
                request.복귀지,
                request.오늘의복귀지주소,
                request.오늘의복귀지위도,
                request.오늘의복귀지경도,
                request.기본복귀지사용,
                request.복귀지출처));

            if (result.IsFailed)
            {
                return BadRequest(new { errors = result.Errors.Select(x => x.Message).ToArray() });
            }

            return CreatedAtAction(nameof(상태조회), new { }, result.Value);
        }

        [HttpPost("stop")]
        public async Task<IActionResult> 종료()
        {
            var driverId = 현재기사Id();
            await _sender.Send(new 운행종료Command(driverId));

            return NoContent();
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
