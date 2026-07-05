using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Hongdal.Application.Driver.Work;
using Hongdal.Contracts.Driver.Work;
using 홍달.도메인.공통;

namespace Hongdal.Controllers.Driver.Work01
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/shifts")]
    public sealed class 기사근무Controller : ControllerBase
    {
        private readonly ISender _sender;

        public 기사근무Controller(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet]
        public async Task<IActionResult> 목록조회()
        {
            var driverId = 현재기사Id();
            var items = await _sender.Send(new Hongdal.Application.Driver.Work.기사근무목록조회Query(driverId));

            return Ok(items);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 상세조회(long id)
        {
            var driverId = 현재기사Id();
            var shift = await _sender.Send(new Hongdal.Application.Driver.Work.기사근무상세조회Query(driverId, id));
            if (shift == null)
            {
                return NotFound();
            }

            return Ok(shift);
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
